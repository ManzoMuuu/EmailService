using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace EmailService.Infrastructure.Configuration
{
    /// <summary>
    /// Estensioni per configurare la lettura dal registro di Windows
    /// </summary>
    public static class RegistryConfigurationExtensions
    {
        /// <summary>
        /// Aggiunge il registro di Windows come sorgente di configurazione
        /// </summary>
        /// <param name="builder">Il builder di configurazione</param>
        /// <param name="registryKey">Percorso della chiave di registro</param>
        /// <param name="registryHive">Hive del registro (default: LocalMachine)</param>
        /// <param name="registryView">Vista del registro (default: Default)</param>
        /// <returns>Il builder di configurazione per concatenazione</returns>
        public static IConfigurationBuilder AddWindowsRegistry(
            this IConfigurationBuilder builder,
            string registryKey,
            RegistryHive registryHive = RegistryHive.LocalMachine,
            RegistryView registryView = RegistryView.Default)
        {
            return builder.Add(new RegistryConfigurationSource(registryKey, registryHive, registryView));
        }
    }
}