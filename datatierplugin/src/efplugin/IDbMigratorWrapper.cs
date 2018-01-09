using System.Collections.Generic;

namespace Apprenda.EFPlugin
{
    /// <summary>
    /// This is just a test-oriented wrapper for DbMigrator
    /// </summary>
    public interface IDbMigratorWrapper
    {
        void Update();
        void Update(string targetMigration);
        IEnumerable<string> GetLocalMigrations();
    }
}
