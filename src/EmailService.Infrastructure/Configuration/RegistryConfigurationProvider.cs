using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace EmailService.Infrastructure.Configuration
{
    /// <summary>
    /// Provider di configurazione che legge i valori dal registro di Windows
    /// </summary>
    public class RegistryConfigurationProvider : ConfigurationProvider
    {
        private readonly string _registryKey;
        private readonly RegistryHive _registryHive;
        private readonly RegistryView _registryView;

        /// <summary>
        /// Inizializza una nuova istanza del provider di configurazione del registro
        /// </summary>
        /// <param name="source">La sorgente di configurazione del registro</param>
        public RegistryConfigurationProvider(RegistryConfigurationSource source)
        {
            _registryKey = source.RegistryKey ?? throw new ArgumentNullException(nameof(source.RegistryKey));
            _registryHive = source.RegistryHive;
            _registryView = source.RegistryView;
        }

        /// <summary>
        /// Carica i valori di configurazione dal registro di Windows
        /// </summary>
        public override void Load()
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(_registryHive, _registryView);
                using var key = baseKey.OpenSubKey(_registryKey);

                if (key == null)
                {
                    // Il registro specificato non esiste
                    return;
                }

                foreach (var valueName in key.GetValueNames())
                {
                    if (string.IsNullOrEmpty(valueName))
                    {
                        continue;
                    }

                    var value = key.GetValue(valueName);
                    if (value != null)
                    {
                        // Converte il valore del registro in stringa e lo aggiunge al dizionario
                        data[valueName] = value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                // In caso di errore, registriamo l'eccezione ma non la propaghiamo
                // per non interrompere il caricamento delle altre configurazioni
                Console.Error.WriteLine($"Errore nel caricamento della configurazione dal registro: {ex.Message}");
            }

            Data = data;
        }
    }
}