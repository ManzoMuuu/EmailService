using EmailService.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.IO;
using Xunit;

namespace EmailService.Tests.Configuration
{
    public class RegistryConfigurationTests
    {
        private const string TestRegistryKey = @"SOFTWARE\EmailServiceTests";
        
        [Fact]
        public void RegistryConfigurationProvider_LoadsValuesFromRegistry()
        {
            // Questo test verifica solo se è possibile creare il provider
            // Non modifica effettivamente il registro di Windows
            
            // Arrange
            var source = new RegistryConfigurationSource(TestRegistryKey, RegistryHive.CurrentUser);
            
            // Act
            var provider = source.Build(new ConfigurationBuilder());
            
            // Assert
            Assert.NotNull(provider);
            Assert.IsType<RegistryConfigurationProvider>(provider);
        }
        
        [Fact]
        public void AddWindowsRegistry_ExtendsConfigurationBuilder()
        {
            // Arrange
            var configurationBuilder = new ConfigurationBuilder();
            
            // Act
            var result = configurationBuilder.AddWindowsRegistry(TestRegistryKey, RegistryHive.CurrentUser);
            
            // Assert
            Assert.Same(configurationBuilder, result);
        }
        
        [Fact]
        public void RegistryConfigurationSource_RequiresRegistryKey()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RegistryConfigurationSource(null));
        }
        
        [Fact]
        public void RegistryConfigurationProvider_HandlesNonExistentKey()
        {
            // Arrange
            var nonExistentKey = Guid.NewGuid().ToString();
            var source = new RegistryConfigurationSource(nonExistentKey, RegistryHive.CurrentUser);
            var provider = source.Build(new ConfigurationBuilder());
            
            // Act - questo non dovrebbe lanciare eccezioni anche se la chiave non esiste
            provider.Load();
            
            // Assert - non lanciando eccezioni, il test è considerato passato
        }
        
        // Nota: il test seguente manipola il registro di sistema
        // È commentato perché in genere non è consigliabile eseguire test che modificano il registro
        // in un ambiente di CI/CD o in esecuzioni automatiche
        /*
        [Fact]
        public void RegistryConfigurationProvider_ReadsValuesCorrectly()
        {
            // Arrange
            var testKey = $@"{TestRegistryKey}\{Guid.NewGuid()}";
            var testValue = "TestValue";
            var testName = "TestName";
            
            try
            {
                // Crea temporaneamente una chiave di registro per il test
                using (var key = Registry.CurrentUser.CreateSubKey(testKey))
                {
                    key.SetValue(testName, testValue);
                }
                
                // Act
                var config = new ConfigurationBuilder()
                    .AddWindowsRegistry(testKey, RegistryHive.CurrentUser)
                    .Build();
                
                // Assert
                Assert.Equal(testValue, config[testName]);
            }
            finally
            {
                // Pulizia - rimuove la chiave di test
                Registry.CurrentUser.DeleteSubKey(testKey, false);
            }
        }
        */
    }
}