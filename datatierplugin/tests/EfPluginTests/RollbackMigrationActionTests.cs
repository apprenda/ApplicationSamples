using System;
using System.Collections.Generic;
using System.Linq;
using Apprenda.API.Persistence;
using Apprenda.EFPlugin;
using Moq;
using Xunit;

namespace Apprenda.EFPluginTests
{
    public class RollbackMigrationActionTests
    {
        public class Constructor
        {
            [Fact]
            public void ThrowsIfArgumentIsNull()
            {
                var x = Assert.Throws<ArgumentNullException>(() => new RollbackMigrationAction(null));

                Assert.Equal("migratorFactory", x.ParamName);
            }
        }

        public class Migrate
        {
            private readonly Mock<IMigrationFactory> _migratorFactory = new Mock<IMigrationFactory>();
            private readonly Mock<IDbMigratorWrapper> _migrator = new Mock<IDbMigratorWrapper>();
            private readonly Mock<IMigrationVersionContext> _context = new Mock<IMigrationVersionContext>();
            private readonly Mock<IMigrationRepository> _repo = new Mock<IMigrationRepository>();
            private readonly PersistenceMigratorRequest _request = new PersistenceMigratorRequest();
            private readonly PersistenceMigratorResult _result = new PersistenceMigratorResult();

            public Migrate()
            {
                _migratorFactory.Setup(m => m.CreateMigrator(It.IsAny<string>())).Returns(_migrator.Object);
                _migratorFactory.Setup(m => m.CreateDbContext(It.IsAny<string>())).Returns(_context.Object);
                _migratorFactory.Setup(m => m.CreateMigrationRepository(It.IsAny<IMigrationVersionContext>())).Returns(_repo.Object);
            }

            [Fact]
            public void DoesNotCreateContextIfNoPreviousVersion()
            {
                var underTest = new RollbackMigrationAction(_migratorFactory.Object);

                underTest.Migrate(_request, _result);

                _migratorFactory.Verify(m => m.CreateDbContext(It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public void AddMessageIfNoPreviousVersion()
            {
                var underTest = new RollbackMigrationAction(_migratorFactory.Object);

                underTest.Migrate(_request, _result);

                Assert.Equal("EF Plugin: There were no previous model migrations so nothing needed to rollback.", _result.InfoMessages.Single());
            }

            [Fact]
            public void DoesNotCallMigratorIfNoPreviousVersion()
            {
                var underTest = new RollbackMigrationAction(_migratorFactory.Object);

                underTest.Migrate(_request, _result);

                _migratorFactory.Verify(m => m.CreateMigrator(It.IsAny<string>()), Times.Never);
                _migrator.Verify(m => m.Update(It.IsAny<string>()), Times.Never);
            }

            [Theory]
            [ClassData(typeof(AuthTypeArgsGenerator))]
            public void AddsExecutingMessage(DbAuthType authType)
            {
                var underTest = new RollbackMigrationAction(_migratorFactory.Object);

                TestConstants.ConfigureRequest(_request, authType);

                underTest.Migrate(_request, _result);

                Assert.Contains("EF Plugin: Executing rollback", _result.InfoMessages);
            }

            [Theory]
            [ClassData(typeof(AuthTypeAndConnectionStringGenerator))]
            public void CreatesContextBeforeRepo(DbAuthType authType, string connString)
            {
                var underTest = new RollbackMigrationAction(_migratorFactory.Object);

                TestConstants.ConfigureRequest(_request, authType);

                var contextCreated = false;
                var contextCreatedBeforeRepo = false;

                _migratorFactory.Setup(m => m.CreateDbContext(It.IsAny<string>()))
                    .Callback(() => contextCreated = true);

                _migratorFactory.Setup(m => m.CreateMigrationRepository(It.IsAny<IMigrationVersionContext>()))
                    .Callback(() => contextCreatedBeforeRepo = contextCreated)
                    .Returns(_repo.Object);

                underTest.Migrate(_request, _result);

                _migratorFactory.Verify(m => m.CreateDbContext(connString), Times.Once);
                Assert.True(contextCreatedBeforeRepo);
            }

            [Theory]
            [ClassData(typeof(AuthTypeArgsGenerator))]
            public void DisposesContext(DbAuthType authType)
            {
                var underTest = new RollbackMigrationAction(_migratorFactory.Object);

                TestConstants.ConfigureRequest(_request, authType);

                underTest.Migrate(_request, _result);

                _context.Verify(m => m.Dispose(), Times.Once);
            }

            [Theory]
            [ClassData(typeof(AuthTypeArgsGenerator))]
            public void RetrievesPreviousVersion(DbAuthType authType)
            {
                var underTest = new RollbackMigrationAction(_migratorFactory.Object);

                TestConstants.ConfigureRequest(_request, authType);

                underTest.Migrate(_request, _result);

                _repo.Verify(m => m.GetTargetMigration(TestConstants.PreviousVersion), Times.Once);
            }

            [Theory]
            [ClassData(typeof(AuthTypeArgsGenerator))]
            public void EnsuresCurrentVersionDoesNotExist(DbAuthType authType)
            {
                var underTest = new RollbackMigrationAction(_migratorFactory.Object);

                TestConstants.ConfigureRequest(_request, authType);

                underTest.Migrate(_request, _result);

                _repo.Verify(m => m.EnsureDoesNotExist(TestConstants.VerAlias), Times.Once);
            }

            [Theory]
            [MemberData(nameof(SavesChangesAndAddsMessageArgs))]
            public void SavesChangesAndAddsMessage(DbAuthType authType, bool existed, int existedCnt)
            {
                var underTest = new RollbackMigrationAction(_migratorFactory.Object);

                TestConstants.ConfigureRequest(_request, authType);

                const string targetMigration = "target";
                _repo.Setup(m => m.GetTargetMigration(TestConstants.PreviousVersion)).Returns(targetMigration);

                _repo.Setup(m => m.EnsureDoesNotExist(TestConstants.VerAlias)).Returns(existed);

                underTest.Migrate(_request, _result);

                _context.Verify(m => m.SaveChanges(), Times.Exactly(existedCnt));
                Assert.Equal(existed, _result.InfoMessages.Contains($"EF Plugin: removed '{targetMigration}' from migration mapping history"));
            }
            public static IEnumerable<object[]> SavesChangesAndAddsMessageArgs()
            {
                foreach (var t in Enum.GetValues(typeof(DbAuthType)).OfType<DbAuthType>())
                {
                    yield return new object[] { t, false, 0 };
                    yield return new object[] { t, true, 1 };
                }
            }

            [Theory]
            [ClassData(typeof(AuthTypeAndConnectionStringGenerator))]
            public void CreatesAndCallsMigrator(DbAuthType authType, string connString)
            {
                var underTest = new RollbackMigrationAction(_migratorFactory.Object);

                TestConstants.ConfigureRequest(_request, authType);

                const string targetMigration = "target";
                _repo.Setup(m => m.GetTargetMigration(TestConstants.PreviousVersion)).Returns(targetMigration);

                underTest.Migrate(_request, _result);

                _migratorFactory.Verify(m => m.CreateMigrator(connString), Times.Once);
                _migrator.Verify(m => m.Update(targetMigration), Times.Once);
            }

            [Theory]
            [ClassData(typeof(AuthTypeArgsGenerator))]
            public void AddsMessageMigrationComplete(DbAuthType authType)
            {
                var underTest = new RollbackMigrationAction(_migratorFactory.Object);

                TestConstants.ConfigureRequest(_request, authType);

                const string targetMigration = "target";
                _repo.Setup(m => m.GetTargetMigration(TestConstants.PreviousVersion)).Returns(targetMigration);

                underTest.Migrate(_request, _result);

                Assert.Contains($"EF Plugin: Rolled back schema to migration target '{targetMigration}'", _result.InfoMessages);
            }
        }
    }
}
