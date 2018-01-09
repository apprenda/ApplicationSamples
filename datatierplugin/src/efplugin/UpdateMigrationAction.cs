using System;
using System.Linq;
using Apprenda.API.Persistence;

namespace Apprenda.EFPlugin
{
    internal class UpdateMigrationAction : IMigrationAction
    {
        private readonly IMigrationFactory _migratorFactory;

        public UpdateMigrationAction(IMigrationFactory migratorFactory)
        {
            if (migratorFactory == null)
            {
                throw new ArgumentNullException(nameof(migratorFactory));
            }
            _migratorFactory = migratorFactory;
        }

        public void Migrate(PersistenceMigratorRequest request, PersistenceMigratorResult result)
        {
            var connString = request.GetConnectionString();
            var migrator = _migratorFactory.CreateMigrator(connString);
            var lastMigration = migrator.GetLocalMigrations().OrderByDescending(s => s).FirstOrDefault();

            if (null == lastMigration)
            {
                result.AddInfoMessage("EF Plugin: There were no model changes found so no migrations applied");

                if (request.PreviousVersion != null)
                {
                    //We have to bring the migration forward to the current version, or we won't know what to rollback to.
                    using (var ctx = _migratorFactory.CreateDbContext(connString))
                    {
                        _migratorFactory.CreateMigrationRepository(ctx)
                            .Copy(request.PreviousVersion.Alias, request.Version.Alias);
                        ctx.SaveChanges();
                        result.AddInfoMessage("EF Plugin: Added record to migration mapping history");
                    }
                }
            }
            else
            {
                migrator.Update();

                result.AddInfoMessage($"EF Plugin: Successfully executed EF migration up to '{lastMigration}'");

                using (var ctx = _migratorFactory.CreateDbContext(connString))
                {
                    _migratorFactory.CreateMigrationRepository(ctx)
                        .AddOrUpdate(request.Version.Alias, lastMigration);
                    ctx.SaveChanges();
                }

                result.AddInfoMessage("EF Plugin: Added record to migration mapping history");
            }
        }
    }
}
