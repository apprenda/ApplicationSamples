using System;
using Apprenda.API.Persistence;

namespace Apprenda.EFPlugin
{
    internal class RollbackMigrationAction : IMigrationAction
    {
        private readonly IMigrationFactory _migratorFactory;

        public RollbackMigrationAction(IMigrationFactory migratorFactory)
        {
            if (migratorFactory == null)
            {
                throw new ArgumentNullException(nameof(migratorFactory));
            }
            _migratorFactory = migratorFactory;
        }

        public void Migrate(PersistenceMigratorRequest request, PersistenceMigratorResult result)
        {
            string targetMigration = null;

            string connString=null;

            if (request.PreviousVersion != null)
            {
                result.AddInfoMessage("EF Plugin: Executing rollback");
                connString = request.GetConnectionString();
                using (var ctx = _migratorFactory.CreateDbContext(connString))
                {
                    var repo = _migratorFactory.CreateMigrationRepository(ctx);
                    targetMigration = repo.GetTargetMigration(request.PreviousVersion.Alias);

                    if (repo.EnsureDoesNotExist(request.Version.Alias))
                    {
                        ctx.SaveChanges();
                        result.AddInfoMessage($"EF Plugin: removed '{targetMigration}' from migration mapping history");
                    }
                }
            }

            if (null == targetMigration)
            {
                result.AddInfoMessage("EF Plugin: There were no previous model migrations so nothing needed to rollback.");
            }
            else
            {
                var migrator = _migratorFactory.CreateMigrator(connString);
                migrator.Update(targetMigration);

                result.AddInfoMessage($"EF Plugin: Rolled back schema to migration target '{targetMigration}'");
            }
        }
    }
}
