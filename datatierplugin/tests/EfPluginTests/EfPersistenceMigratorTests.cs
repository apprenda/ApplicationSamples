using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Apprenda.API.Persistence;
using Apprenda.EFPlugin;
using Moq;
using Xunit;

namespace Apprenda.EFPluginTests
{
    public class EfPersistenceMigratorTests
    {
        public class Constructor
        {
            [Theory]
            [MemberData(nameof(NullParameters))]
            public void ThrowsIfAnyParameterIsNull(IMigrationAction updateAction, IMigrationAction rollbackAction, string nullArgumentName)
            {
                var x = Assert.Throws<ArgumentNullException>(() => new EfPersistenceMigrator(updateAction, rollbackAction));

                Assert.Equal(nullArgumentName, x.ParamName);
            }

            public static IEnumerable<object[]> NullParameters()
            {
                var update = Mock.Of<IMigrationAction>();
                var rollback = Mock.Of<IMigrationAction>();

                yield return new object[] {null, rollback, "updateAction" };
                yield return new object[] {update, null, "rollbackAction" };
            }
        }

        public class Provision : ApiAction
        {
            protected override Mock<IMigrationAction> ExpectedAction => UpdateAction;
            protected override Mock<IMigrationAction> UnusedAction => RollbackAction;

            protected override PersistenceMigratorResult CallAction(PersistenceMigratorRequest request)
            {
                return GetInstance().Provision(request);
            }
        }

        public class Patch : ApiAction
        {
            protected override Mock<IMigrationAction> ExpectedAction => UpdateAction;
            protected override Mock<IMigrationAction> UnusedAction => RollbackAction;
            protected override PersistenceMigratorResult CallAction(PersistenceMigratorRequest request)
            {
                return GetInstance().Patch(request);
            }
        }

        public class Rollback : ApiAction
        {
            protected override Mock<IMigrationAction> ExpectedAction => RollbackAction;
            protected override Mock<IMigrationAction> UnusedAction => UpdateAction;

            protected override PersistenceMigratorResult CallAction(PersistenceMigratorRequest request)
            {
                return GetInstance().Rollback(request);
            }
        }

        public abstract class ApiAction
        {
            public readonly Mock<IMigrationAction> UpdateAction = new Mock<IMigrationAction>();
            public readonly Mock<IMigrationAction> RollbackAction = new Mock<IMigrationAction>();

            [Theory]
            [ClassData(typeof(RequestGenerator))]
            public void CallsAction(PersistenceMigratorRequest request)
            {
                var result = CallAction(request);

                Assert.Empty(result.ErrorMessages);
                //Total calls to update
                ExpectedAction.Verify(AnyCallToMigrateExpression, Times.Once);
                //Proper call to update
                ExpectedAction.Verify(m => m.Migrate(request, result), Times.Once);
                //No calls to Rollback
                UnusedAction.Verify(AnyCallToMigrateExpression, Times.Never);
            }

            [Fact]
            public void AddsErrorActionException()
            {
                var request = TestConstants.BuildRequest(DbAuthType.Native);
                var thrown = new Exception("message");
                ExpectedAction.Setup(AnyCallToMigrateExpression).Throws(thrown);

                var result = CallAction(request);

                Assert.StartsWith("EF Plugin: message", result.ErrorMessages.Single());
            }

            protected static Expression<Action<IMigrationAction>> AnyCallToMigrateExpression => m => m.Migrate(It.IsAny<PersistenceMigratorRequest>(), It.IsAny<PersistenceMigratorResult>());

            protected abstract PersistenceMigratorResult CallAction(PersistenceMigratorRequest request);
            protected abstract Mock<IMigrationAction> ExpectedAction { get; }
            protected abstract Mock<IMigrationAction> UnusedAction { get; }

            public EfPersistenceMigrator GetInstance()
            {
                return new EfPersistenceMigrator(UpdateAction.Object, RollbackAction.Object);
            }
        }

        public class RequestGenerator : IEnumerable<object[]>
        {
            private readonly List<object[]> _data = new List<object[]>
            {
                new object[] { TestConstants.BuildRequest(DbAuthType.WindowsAuth)},
                new object[] { TestConstants.BuildRequest(DbAuthType.Native)}
            };

            public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
