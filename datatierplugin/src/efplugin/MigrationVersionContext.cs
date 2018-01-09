using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Apprenda.EFPlugin
{
    /// <summary>
    /// Context to contain the plug-in's tracking table for Migrations
    /// Having this context also means that V1 of the application will have a record in EF's migration tracking table, "__MigrationHistory", for the plugin itself:
    /// Something like: "201801081539519_InitialCreate", "Apprenda.EFPlugin.MigrationVersionContext"
    /// </summary>
    internal class MigrationVersionContext : DbContext, IMigrationVersionContext
    {
        public MigrationVersionContext(string connectionString)
            : base(connectionString)
        {
        }

        public DbSet<MigrationRecord> Migrations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}