using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace EmailService.Infrastructure.Configuration
{
    /// <summary>
    /// Sorgente di configurazione per il registro di Windows
    /// </summary>
    public class RegistryConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Percorso della chiave di registro
        /// </summary>
        public string RegistryKey { get; set; }

        /// <summary>
        /// Hive del registro (LocalMachine, CurrentUser, ecc.)
        /// </summary>
        public RegistryHive RegistryHive { get; set; }

        /// <summary>
        /// Vista del registro (Default, Registry32, Registry64)
        /// </summary>
        public RegistryView RegistryView { get; set; }

        /// <summary>
        /// Costruttore che inizializza una nuova istanza della sorgente di configurazione
        /// </summary>
        /// <param name="registryKey">Percorso della chiave di registro</param>
        /// <param name="registryHive">Hive del registro (default: LocalMachine)</param>
        /// <param name="registryView">Vista del registro (default: Default)</param>
        public RegistryConfigurationSource(
            string registryKey,
            RegistryHive registryHive = RegistryHive.LocalMachine,
            RegistryView registryView = RegistryView.Default)
        {
            RegistryKey = registryKey;
            RegistryHive = registryHive;
            RegistryView = registryView;
        }

        /// <summary>
        /// Crea il provider di configurazione
        /// </summary>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new RegistryConfigurationProvider(this);
        }
    }
}