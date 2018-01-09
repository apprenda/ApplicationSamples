using System.Data.SqlClient;
using Apprenda.API.Persistence;

namespace Apprenda.EFPlugin
{
    internal static class PersistenceMigratorRequestExtensions
    {
        public static string GetConnectionString(this PersistenceMigratorRequest request)
        {
            var connInfo = request.ConnectionInformation;
            var builder = new SqlConnectionStringBuilder
            {
                ApplicationName = $"{request.Application.Alias}-{request.Version.Alias}",
                IntegratedSecurity = connInfo.AuthenticationType == DbAuthType.WindowsAuth,
                InitialCatalog = connInfo.DatabaseName,
                DataSource = connInfo.Server
            };

            if (!builder.IntegratedSecurity)
            {
                builder.UserID = connInfo.Username;
                builder.Password = connInfo.Password;
            }
            return builder.ConnectionString;
        }
    }
}
