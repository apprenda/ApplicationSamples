using System;

namespace Apprenda.EFPlugin
{
    /// <summary>
    /// Helper class to load the guest app's EF Configuration
    /// </summary>
    public interface ITypeLoader
    {
        Type Load(string className);
    }
}
