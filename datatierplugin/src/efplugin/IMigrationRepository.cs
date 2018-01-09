namespace Apprenda.EFPlugin
{
    public interface IMigrationRepository
    {
        string GetTargetMigration(string versionAlias);
        void AddOrUpdate(string versionAlias, string lastMigration);
        void Copy(string sourceVersionAlias, string targetVersionAlias);
        bool EnsureDoesNotExist(string versionAlias);
    }
}
