using Apprenda.API.Persistence;

namespace Apprenda.EFPlugin
{
    public interface IMigrationAction
    {
        void Migrate(PersistenceMigratorRequest request, PersistenceMigratorResult result);
    }
}
