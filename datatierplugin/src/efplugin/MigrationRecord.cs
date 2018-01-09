using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apprenda.EFPlugin
{
    [Table("__ApprendaMigrationMapping")]
    public class MigrationRecord
    {
        [Key]
        public string VersionAlias { get; set; }
        /// <summary>
        /// Last EF Migration applied for this version
        /// </summary>
        public string TargetMigration { get; set; }
    }
}