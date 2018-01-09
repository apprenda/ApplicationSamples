using System;
using Apprenda.API.Persistence;
using Apprenda.EFPlugin;
using Moq;
using Xunit;

namespace Apprenda.EFPluginTests
{
    public class UpdateMigrationActionTests
    {
        public class Constructor
        {
            [Fact]
            public void ThrowsIfArgumentIsNull()
            {
                var x = Assert.Throws<ArgumentNullException>(() => new UpdateMigrationAction(null));

                Assert.Equal("migratorFactory", x.ParamName);
            }
        }

        public class Migrate
        {

            public class MigrateCommon : MigrateBase
            {
                [Theory]
                [ClassData(typeof(AuthTypeAndConnectionStringGenerator))]
                public void RetrievesLastMigration(DbAuthType authType, string connString)
                {
                    TestConstants.ConfigureRequest(Request, authType);

                    var underTest = new UpdateMigrationAction(MigratorFactory.Object);
                    underTest.Migrate(Request, Result);

                    MigratorFactory.Verify(m => m.CreateMigrator(connString), Times.Once);
                    Migrator.Verify(m => m.GetLocalMigrations(), Times.Once);
                }
            }

            /// <summary>
            /// Cases of Migrate(...) where no lastMigration is found
            /// </summary>
            public class MigrateWithoutPrior : MigrateBase
            {
                [Theory]
                [ClassData(typeof(AuthTypeArgsGenerator))]
                public void AddsMessage(DbAuthType authType)
                {
                    TestConstants.ConfigureRequest(Request, authType);

                    var underTest = new UpdateMigrationAction(MigratorFactory.Object);
                    underTest.Migrate(Request, Result);

                    Assert.Contains("EF Plugin: There were no model changes found so no migrations applied",
                        Result.InfoMessages);
                }

                [Theory]
                [ClassData(typeof(AuthTypeAndConnectionStringGenerator))]
                public void MovesMigrationForward(DbAuthType authType, string connString)
                {
                    TestConstants.ConfigureRequest(Request, authType);
                    MigratorFactory.Setup(m => m.CreateMigrator(connString)).Returns(Migrator.Object);

                    var underTest = new UpdateMigrationAction(MigratorFactory.Object);

                    underTest.Migrate(Request, Result);

                    MigratorFactory.Verify(m => m.CreateDbContext(connString), Times.Once);
                    MigratorFactory.Verify(m => m.CreateMigrationRepository(Context.Object), Times.Once);
                    Repo.Verify(m => m.Copy(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
                    Repo.Verify(m => m.Copy(TestConstants.PreviousVersion, TestConstants.VerAlias), Times.Once);
                    Migrator.Verify(m => m.GetLocalMigrations(), Times.Once);
                }
            }

            /// <summary>
            /// Case of Migrate(...) where a prior migration is found
            /// </summary>
            public class MigrateWithPrior : MigrateBase
            {
                const string lastMigration = "201801051332494_RenamedFavoriteColor";

                private readonly string[] migrations =
                {
                    "201711131900045_InitialCreate",
                    "201711132011066_MaxLengthOnNames",
                    "201711132016393_ColumnFirstName",
                    "201711132146161_ComplexDataModel",
                    lastMigration, //Out-of-order on purpose!
                    "201712152105365_AddedDoB",
                    "201712152123053_AddedFavoriteColor"
                };
                public MigrateWithPrior()
                {
                    Migrator.Setup(m => m.GetLocalMigrations()).Returns(migrations);
                }

                [Theory]
                [ClassData(typeof(AuthTypeArgsGenerator))]
                public void CallsUpdate(DbAuthType authType)
                {
                    TestConstants.ConfigureRequest(Request, authType);

                    var underTest = new UpdateMigrationAction(MigratorFactory.Object);

                    underTest.Migrate(Request, Result);

                    Migrator.Verify(m => m.Update(), Times.Once);
                }

                [Theory]
                [ClassData(typeof(AuthTypeArgsGenerator))]
                public void AddsExecutionMessages(DbAuthType authType)
                {
                    TestConstants.ConfigureRequest(Request, authType);

                    var underTest = new UpdateMigrationAction(MigratorFactory.Object);

                    underTest.Migrate(Request, Result);

                    Assert.Contains($"EF Plugin: Successfully executed EF migration up to '{lastMigration}'", Result.InfoMessages);
                    Assert.Contains("EF Plugin: Added record to migration mapping history", Result.InfoMessages);
                }

                [Theory]
                [ClassData(typeof(AuthTypeAndConnectionStringGenerator))]
                public void EnsuresMigrationRecordExistsForNewVersion(DbAuthType authType, string connString)
                {
                    TestConstants.ConfigureRequest(Request, authType);

                    var underTest = new UpdateMigrationAction(MigratorFactory.Object);

                    underTest.Migrate(Request, Result);

                    MigratorFactory.Verify(m => m.CreateDbContext(connString), Times.Once);
                    Repo.Verify(m => m.AddOrUpdate(TestConstants.VerAlias, lastMigration), Times.Once);
                    Context.Verify(m => m.SaveChanges());
                }
            }

            public class MigrateBase
            {
                protected readonly Mock<IMigrationFactory> MigratorFactory = new Mock<IMigrationFactory>();
                protected readonly Mock<IDbMigratorWrapper> Migrator = new Mock<IDbMigratorWrapper>();
                protected readonly Mock<IMigrationVersionContext> Context = new Mock<IMigrationVersionContext>();
                protected readonly Mock<IMigrationRepository> Repo = new Mock<IMigrationRepository>();
                protected readonly PersistenceMigratorRequest Request = new PersistenceMigratorRequest();
                protected readonly PersistenceMigratorResult Result = new PersistenceMigratorResult();

                public MigrateBase()
                {
                    MigratorFactory.Setup(m => m.CreateDbContext(It.IsAny<string>())).Returns(Context.Object);
                    MigratorFactory.Setup(m => m.CreateMigrationRepository(Context.Object)).Returns(Repo.Object);
                    MigratorFactory.Setup(m => m.CreateMigrator(It.IsAny<string>())).Returns(Migrator.Object);
                }
            }
        }
    }
}
