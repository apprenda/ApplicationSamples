using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;

namespace Apprenda.EFPlugin
{
    internal class MigrationFactory : IMigrationFactory
    {
        private readonly DbMigrationsConfiguration _configuration;
        public MigrationFactory(DbMigrationsConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            _configuration = configuration;
        }

        public IDbMigratorWrapper CreateMigrator(string connString)
        {
            _configuration.TargetDatabase = new DbConnectionInfo(connString, "System.Data.SQLClient");
            return new DbMigratorWrapper(_configuration);
        }

        public IMigrationVersionContext CreateDbContext(string connString)
        {
            return new MigrationVersionContext(connString);
        }

        public IMigrationRepository CreateMigrationRepository(IMigrationVersionContext ctx)
        {
            return new MigrationRepository(ctx);
        }
    }
}
