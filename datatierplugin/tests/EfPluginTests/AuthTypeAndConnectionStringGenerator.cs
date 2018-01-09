using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Apprenda.API.Persistence;

namespace Apprenda.EFPluginTests
{
    internal class AuthTypeAndConnectionStringGenerator : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[]
            {
                DbAuthType.WindowsAuth,
                $@"Data Source={TestConstants.ServerName};Initial Catalog={TestConstants.DbName};Integrated Security=True;Application Name={TestConstants.AppAlias}-{TestConstants.VerAlias}"
            },
            new object[]
            {
                DbAuthType.Native,
                $@"Data Source={TestConstants.ServerName};Initial Catalog={TestConstants.DbName};Integrated Security=False;User ID={TestConstants.Username};Password={TestConstants.Password};Application Name={TestConstants.AppAlias}-{TestConstants.VerAlias}"
            }
        };

        public IEnumerator<object[]> GetEnumerator()
        {
            var knownAuthTypes = Enum.GetValues(typeof(DbAuthType)).OfType<DbAuthType>().ToList();
                if (!knownAuthTypes.All(t => t == DbAuthType.WindowsAuth || t == DbAuthType.Native))
            {
                throw new Exception($"DbAuthType has been extended to include new values not tested ({string.Join(",", knownAuthTypes)})");
            }
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
