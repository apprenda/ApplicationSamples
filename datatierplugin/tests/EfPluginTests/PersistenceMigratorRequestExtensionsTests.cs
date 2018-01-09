using System.Data.SqlClient;
using Apprenda.API.Persistence;
using Apprenda.EFPlugin;
using Xunit;

namespace Apprenda.EFPluginTests
{
    public class PersistenceMigratorRequestExtensionsTests
    {
        private const string AppAlias = "app";
        private const string VerAlias = "vN";
        private const string DataSource = "server";
        private const string InitialCatalog = "dbName";
        private const string Password = "pwd";
        private const string UserId = "user";

        [Theory]
        [ClassData(typeof(AuthTypeArgsGenerator))]
        public void SetsApplicationName(DbAuthType authType)
        {
            var request = BuildRequest(authType);
            var connString = request.GetConnectionString();

            Assert.Equal($"{AppAlias}-{VerAlias}", new SqlConnectionStringBuilder(connString).ApplicationName);
        }

        [Theory]
        [InlineData(DbAuthType.Native, false, UserId, Password)]
        [InlineData(DbAuthType.WindowsAuth, true, "", "")]
        public void SetsSecurityFields(DbAuthType authType, bool integratedSecurity, string userId, string password)
        {
            var request = BuildRequest(authType);
            var connString = request.GetConnectionString();

            var b = new SqlConnectionStringBuilder(connString);
            Assert.Equal(integratedSecurity, b.IntegratedSecurity);
            Assert.Equal(userId, b.UserID);
            Assert.Equal(password, b.Password);
        }

        [Theory]
        [ClassData(typeof(AuthTypeArgsGenerator))]
        public void SetsInitialCatalog(DbAuthType authType)
        {
            var request = BuildRequest(authType);
            var connString = request.GetConnectionString();

            Assert.Equal(InitialCatalog, new SqlConnectionStringBuilder(connString).InitialCatalog);
        }

        [Theory]
        [ClassData(typeof(AuthTypeArgsGenerator))]
        public void SetsDataSource(DbAuthType authType)
        {
            var request = BuildRequest(authType);
            var connString = request.GetConnectionString();

            Assert.Equal(DataSource, new SqlConnectionStringBuilder(connString).DataSource);
        }

        private PersistenceMigratorRequest BuildRequest(DbAuthType authType)
        {
            return new PersistenceMigratorRequest
            {
                Application = new NameAlias {Alias = AppAlias},
                Version = new VersionInfo {Alias = VerAlias},
                ConnectionInformation = new ConnectionInformation { AuthenticationType = authType, Server= DataSource, DatabaseName = InitialCatalog, Password = Password, Username = UserId, Type=DbType.SqlServer}
            };
        }
    }
}
