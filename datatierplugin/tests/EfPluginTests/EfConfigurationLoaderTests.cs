using System;
using System.Configuration;
using System.Data.Entity.Migrations;
using Apprenda.EFPlugin;
using Moq;
using Xunit;

namespace Apprenda.EfPluginTests
{
    public class EfConfigurationLoaderTests
    {
        private const string ConfigClassSettingKey = @"Apprenda.EFPlugin.EFConfigurationClass";
        [Fact]
        public void ConfigClassSettingKeysMatch()
        {
            Assert.Equal(ConfigClassSettingKey, EfConfigurationLoader.ConfigClassSettingKey);
        }

        public class Constructor
        {
            [Fact]
            public void ThrowsIfNotConfiguredInFile()
            {
                var x = Assert.Throws<ConfigurationErrorsException>(() => new EfConfigurationLoader());

                Assert.Equal($"AppSetting '{ConfigClassSettingKey}' must be configured with a fully qualified class name.", x.Message);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("\n")]
            public void ThrowsIfClassNameEmpty(string className)
            {
                var x = Assert.Throws<ArgumentNullException>(() => new EfConfigurationLoader(className, Mock.Of<ITypeLoader>()));

                Assert.Equal("configClassName", x.ParamName);
            }

            [Fact]
            public void ThrowsIfTypeLoaderIsNull()
            {
                var x = Assert.Throws<ArgumentNullException>(() => new EfConfigurationLoader("blah", null));

                Assert.Equal("typeLoader", x.ParamName);
            }
        }

        public class InstantiateEfConfigurationClass
        {
            private const string ExceptionMessage = "message";
            private const string ClassName = "blah";
            [Fact]
            public void ThrowsIfLoaderThrows()
            {
                var x = new Exception(ExceptionMessage);
                var mockLoader = new Mock<ITypeLoader>();
                mockLoader.Setup(m => m.Load(ClassName)).Throws(x);

                var underTest = new EfConfigurationLoader(ClassName, mockLoader.Object);
                var cx = Assert.Throws<ConfigurationErrorsException>(() => underTest.InstantiateEfConfigurationClass());

                Assert.Equal($"Error loading type '{ClassName}', configured in '{ConfigClassSettingKey}'", cx.Message);
                Assert.Equal(x, cx.InnerException);
            }

            [Fact]
            public void ThrowsIfLoaderReturnsNull()
            {
                var underTest = new EfConfigurationLoader(ClassName, Mock.Of<ITypeLoader>());
                var cx = Assert.Throws<ConfigurationErrorsException>(() => underTest.InstantiateEfConfigurationClass());

                Assert.Equal($"Could not find type '{ClassName}', configured in '{ConfigClassSettingKey}'", cx.Message);
            }

            [Fact]
            public void ThrowsIfTypeIsNotAssignableToDbMigrationsConfiguration()
            {
                var mockLoader = new Mock<ITypeLoader>();
                var type = typeof(NotAssignableToDbMigrationsConfiguration);

                Assert.False(typeof(DbMigrationsConfiguration).IsAssignableFrom(type));

                mockLoader.Setup(m => m.Load(ClassName)).Returns(type);
                var underTest = new EfConfigurationLoader(ClassName, mockLoader.Object);
                var cx = Assert.Throws<ConfigurationErrorsException>(() => underTest.InstantiateEfConfigurationClass());

                Assert.Equal($"Type '{type.FullName}', configured in '{ConfigClassSettingKey}', does not extend DbMigrationsConfiguration", cx.Message);
            }

            [Fact]
            public void ThrowsIfClassCannotBeInstantiated()
            {
                var mockLoader = new Mock<ITypeLoader>();
                var type = typeof(CannotInstantiate);

                Assert.True(typeof(DbMigrationsConfiguration).IsAssignableFrom(type));

                mockLoader.Setup(m => m.Load(ClassName)).Returns(type);
                var underTest = new EfConfigurationLoader(ClassName, mockLoader.Object);
                var cx = Assert.Throws<ConfigurationErrorsException>(() => underTest.InstantiateEfConfigurationClass());

                Assert.Equal($"Could not instantiate type '{type.FullName}', configured in '{ConfigClassSettingKey}'", cx.Message);
                Assert.NotNull(cx.InnerException);
                Assert.Equal("Exception has been thrown by the target of an invocation.", cx.InnerException.Message);
                Assert.NotNull(cx.InnerException.InnerException);
                Assert.Equal(ExceptionMessage, cx.InnerException.InnerException.Message);
            }

            [Fact]
            public void HappyPath()
            {
                var mockLoader = new Mock<ITypeLoader>();
                var type = typeof(CanInstantiate);

                Assert.True(typeof(DbMigrationsConfiguration).IsAssignableFrom(type));

                mockLoader.Setup(m => m.Load(ClassName)).Returns(type);
                var underTest = new EfConfigurationLoader(ClassName, mockLoader.Object);
                var instance = underTest.InstantiateEfConfigurationClass();

                Assert.IsAssignableFrom<CanInstantiate>(instance);
            }

            public class NotAssignableToDbMigrationsConfiguration
            {
                
            }

            public class CannotInstantiate : DbMigrationsConfiguration
            {
                public CannotInstantiate()
                {
                    throw new Exception(ExceptionMessage);
                }
            }

            public class CanInstantiate : DbMigrationsConfiguration
            {
            }
        }
    }
}
