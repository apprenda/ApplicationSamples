using Apprenda.API.Persistence;

namespace Apprenda.EFPluginTests
{
    public class TestConstants
    {
        public const string PreviousVersion = "v0";
        public const string AppAlias = "app";
        public const string VerAlias = "v1";
        public const string ServerName = "server";
        public const string DbName = "db";
        public const string Password = "password";
        public const string Username = "user";

        public static void ConfigureRequest(PersistenceMigratorRequest request, DbAuthType authType)
        {
            request.Application = new NameAlias { Alias = AppAlias };
            request.Version = new VersionInfo { Alias = VerAlias };
            request.ConnectionInformation = new ConnectionInformation
            {
                AuthenticationType = authType,
                DatabaseName = DbName,
                Server = ServerName,
                Username = Username,
                Password = Password,
            };
            request.PreviousVersion = new VersionInfo { Alias = PreviousVersion };
        }

        public static PersistenceMigratorRequest BuildRequest(DbAuthType authType)
        {
            var request = new PersistenceMigratorRequest();
            ConfigureRequest(request, authType);
            return request;
        }
    }
}
