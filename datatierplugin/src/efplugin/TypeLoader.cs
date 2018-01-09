using System;
using System.Linq;

namespace Apprenda.EFPlugin
{
    internal class TypeLoader : ITypeLoader
    {
        public Type Load(string className)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(className)).FirstOrDefault(type => type != null);
        }
    }
}
