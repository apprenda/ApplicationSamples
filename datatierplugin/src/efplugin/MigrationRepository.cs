using System;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Apprenda.EFPlugin
{
    internal class MigrationRepository : IMigrationRepository
    {
        private readonly IMigrationVersionContext _ctx;
        public MigrationRepository(IMigrationVersionContext ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }
            _ctx = ctx;
        }

        /// <summary>
        /// Retrieves the correct row for a particular version
        /// </summary>
        /// <param name="versionAlias"></param>
        /// <returns></returns>
        public string GetTargetMigration(string versionAlias)
        {
            return Get(versionAlias)?.TargetMigration;
        }

        /// <summary>
        /// Upserts the row for versionAlias
        /// </summary>
        /// <param name="versionAlias"></param>
        /// <param name="lastMigration"></param>
        public void AddOrUpdate(string versionAlias, string lastMigration)
        {
            _ctx.Migrations.AddOrUpdate(new MigrationRecord
            {
                VersionAlias = versionAlias,
                TargetMigration = lastMigration
            });
        }

        /// <summary>
        /// Copies one migration target to a new version
        /// </summary>
        /// <param name="sourceVersionAlias">versionAlias for existing prior version</param>
        /// <param name="targetVersionAlias">versionAlias for new version, overwrites existing records</param>
        public void Copy(string sourceVersionAlias, string targetVersionAlias)
        {
            var priorTarget = Get(sourceVersionAlias).TargetMigration;
            AddOrUpdate(targetVersionAlias, priorTarget);
        }

        /// <summary>
        /// Removes migration record for a version, if it exists
        /// </summary>
        /// <param name="versionAlias"></param>
        /// <returns>true if action was taken to remove a row</returns>
        public bool EnsureDoesNotExist(string versionAlias)
        {
            var record = Get(versionAlias);

            if (null == record)
            {
                return false;
            }
            _ctx.Migrations.Remove(record);
            return true;
        }
        
        /// <summary>
        /// Retrieves the only record for versionAlias allowed or none
        /// </summary>
        /// <param name="versionAlias"></param>
        /// <returns></returns>
        private MigrationRecord Get(string versionAlias)
        {
            return _ctx.Migrations.SingleOrDefault(r => r.VersionAlias.Equals(versionAlias));
        }
    }
}
