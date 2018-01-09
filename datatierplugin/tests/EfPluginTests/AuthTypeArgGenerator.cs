using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Apprenda.API.Persistence;

namespace Apprenda.EFPluginTests
{
    class AuthTypeArgsGenerator : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = Enum.GetValues(typeof(DbAuthType)).OfType<DbAuthType>().Select(t => new object[] {t}).ToList();

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
