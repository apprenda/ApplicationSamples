using System;
using System.Configuration;
using System.Data.Entity.Migrations;

namespace Apprenda.EFPlugin
{
    /// <summary>
    /// Loads EF Configuration from assemblies available to the CurrentDomain
    /// </summary>
    internal class EfConfigurationLoader
    {
        public const string ConfigClassSettingKey = @"Apprenda.EFPlugin.EFConfigurationClass";

        private readonly string _configClassName;
        private readonly ITypeLoader _typeLoader;

        public EfConfigurationLoader() :this(LoadSetting(), new TypeLoader())
        {
        }

        private static string LoadSetting()
        {
            var value = ConfigurationManager.AppSettings[ConfigClassSettingKey];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            throw new ConfigurationErrorsException($"AppSetting '{ConfigClassSettingKey}' must be configured with a fully qualified class name.");
        }

        /// <summary>
        /// For testing
        /// </summary>
        /// <param name="configClassName"></param>
        /// <param name="typeLoader"></param>
        public EfConfigurationLoader(string configClassName, ITypeLoader typeLoader)
        {
            if (string.IsNullOrWhiteSpace(configClassName))
            {
                throw new ArgumentNullException(nameof(configClassName));
            }
            if (typeLoader==null)
            {
                throw new ArgumentNullException(nameof(typeLoader));
            }
            _configClassName = configClassName;
            _typeLoader = typeLoader;
        }

        public DbMigrationsConfiguration InstantiateEfConfigurationClass()
        {
            Type configClassType;
            try
            {
                configClassType = _typeLoader.Load(_configClassName);
            }
            catch (Exception x)
            {
                throw new ConfigurationErrorsException($"Error loading type '{_configClassName}', configured in '{ConfigClassSettingKey}'", x);
            }
            if (configClassType == null)
            {
                throw new ConfigurationErrorsException($"Could not find type '{_configClassName}', configured in '{ConfigClassSettingKey}'");
            }

            if (!typeof(DbMigrationsConfiguration).IsAssignableFrom(configClassType))
            {
                throw new ConfigurationErrorsException($"Type '{configClassType.FullName}', configured in '{ConfigClassSettingKey}', does not extend DbMigrationsConfiguration");
            }

            object instance;
            try
            {
                instance = Activator.CreateInstance(configClassType);
            }
            catch (Exception x)
            {
                throw new ConfigurationErrorsException($"Could not instantiate type '{configClassType.FullName}', configured in '{ConfigClassSettingKey}'", x);
            }

            return instance as DbMigrationsConfiguration;
        }
    }
}
