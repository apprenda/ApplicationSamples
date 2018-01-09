using System;
using Apprenda.API.Persistence;
using System.Data.Entity.Migrations;

namespace Apprenda.EFPlugin
{
    /// <summary>
    /// This is the Apprenda platform custom persistence API implementation for generically handling EF migrations
    /// Provision & Patch are backed equally by Update(), but also stores migration targets used by Rollback()
    /// Rollback calls Updated(target) by looking up the EF migration target index by platform version stored by Provision & Patch
    /// </summary>
    public class EfPersistenceMigrator : PersistenceMigratorBase
    {
        private readonly IMigrationAction _updateAction;
        private readonly IMigrationAction _rollbackAction;

        public EfPersistenceMigrator() : this(new EfConfigurationLoader().InstantiateEfConfigurationClass())
        {
        }

        protected EfPersistenceMigrator(DbMigrationsConfiguration configuration) : this(new MigrationFactory(configuration))
        {
        }

        protected EfPersistenceMigrator(IMigrationFactory migratorFactory) : this(new UpdateMigrationAction(migratorFactory), new RollbackMigrationAction(migratorFactory))
        {
        }

        /// <summary>
        /// For testing
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="contextFactory"></param>
        /// <param name="updateAction"></param>
        /// <param name="rollbackAction"></param>
        internal EfPersistenceMigrator(IMigrationAction updateAction, IMigrationAction rollbackAction)
        {
            if (updateAction == null)
            {
                throw new ArgumentNullException(nameof(updateAction));
            }
            if (rollbackAction == null)
            {
                throw new ArgumentNullException(nameof(rollbackAction));
            }
            _updateAction = updateAction;
            _rollbackAction = rollbackAction;
        }

        public override PersistenceMigratorResult Provision(PersistenceMigratorRequest request)
        {
            return Migrate(request, _updateAction);
        }

        public override PersistenceMigratorResult Patch(PersistenceMigratorRequest request)
        {
            return Migrate(request, _updateAction);
        }

        public override PersistenceMigratorResult Rollback(PersistenceMigratorRequest request)
        {
            return Migrate(request, _rollbackAction);
        }

        private PersistenceMigratorResult Migrate(PersistenceMigratorRequest request, IMigrationAction action)
        {
            var result = new PersistenceMigratorResult();

            try
            {
                action.Migrate(request, result);
            }
            catch (Exception e)
            {
#if DEBUG
                result.AddErrorMessage($"EF Plugin: {e.Message}\n{e.StackTrace}");
#else
                result.AddErrorMessage($"EF Plugin: {e.Message}");
#endif 
            }

            return result;
        }
    }
}
