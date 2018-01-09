namespace Apprenda.EFPlugin
{
    public interface IMigrationFactory
    {
        IDbMigratorWrapper CreateMigrator(string connString);
        IMigrationVersionContext CreateDbContext(string connString);
        IMigrationRepository CreateMigrationRepository(IMigrationVersionContext ctx);
    }
}
