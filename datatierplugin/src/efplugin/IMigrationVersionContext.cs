using System;
using System.Data.Entity;
using Apprenda.EFPlugin;

namespace Apprenda.EFPlugin
{
    public interface IMigrationVersionContext : IDisposable
    {
        DbSet<MigrationRecord> Migrations { get; }
        int SaveChanges();
    }
}
