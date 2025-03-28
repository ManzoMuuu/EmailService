using EmailService.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;

namespace EmailService.Tests
{
    /// <summary>
    /// Classe base per tutti i test con funzionalità e configurazioni comuni
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Crea una configurazione email predefinita per i test
        /// </summary>
        protected EmailConfig CreateTestEmailConfig()
        {
            return new EmailConfig
            {
                SmtpServer = "smtp.gmail.com",
                SmtpPort = 587,
                Username = "test@example.com",
                Password = "test-password",
                UseSsl = true,
                SenderName = "Test Sender",
                SenderEmail = "test@example.com"
            };
        }

        /// <summary>
        /// Crea un file temporaneo con il contenuto specificato per i test
        /// </summary>
        /// <param name="content">Contenuto del file</param>
        /// <param name="extension">Estensione del file (default: .txt)</param>
        /// <returns>Percorso completo del file creato</returns>
        protected string CreateTempFile(string content, string extension = ".txt")
        {
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            
            File.WriteAllText(filePath, content);
            
            return filePath;
        }

        /// <summary>
        /// Elimina un file dato il suo percorso, ignorando eventuali errori
        /// </summary>
        /// <param name="filePath">Percorso del file da eliminare</param>
        protected void DeleteFileIfExists(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Ignora eventuali errori durante la pulizia
            }
        }

        /// <summary>
        /// Crea un mock di ILogger generico
        /// </summary>
        /// <typeparam name="T">Tipo per cui creare il logger</typeparam>
        /// <returns>Mock di ILogger</returns>
        protected Mock<ILogger<T>> CreateMockLogger<T>()
        {
            return new Mock<ILogger<T>>();
        }
    }
}