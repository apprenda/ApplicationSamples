using System.Collections.Generic;
using System.Data.Entity.Migrations;

namespace Apprenda.EFPlugin
{
    /// <inheritdoc/>
    internal class DbMigratorWrapper : IDbMigratorWrapper
    {
        private readonly DbMigrator _migrator;

        public DbMigratorWrapper(DbMigrationsConfiguration configuration)
        {
            _migrator = new DbMigrator(configuration);
        }

        public void Update()
        {
            _migrator.Update();
        }

        public void Update(string targetMigration)
        {
            _migrator.Update(targetMigration);
        }

        public IEnumerable<string> GetLocalMigrations()
        {
            return _migrator.GetLocalMigrations();
        }
    }
}
