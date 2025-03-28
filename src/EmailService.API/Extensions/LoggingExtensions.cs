using Microsoft.Extensions.Logging;
using System;

namespace EmailService.API.Extensions
{
    /// <summary>
    /// Estensioni per il logging specifiche per le operazioni sulle email
    /// </summary>
    public static class LoggingExtensions
    {
        private static readonly Action<ILogger, string, string, Exception?> _emailSent =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(1000, "EmailSent"),
                "Email inviata a {Recipient} con oggetto {Subject}");

        private static readonly Action<ILogger, string, string, Exception> _emailFailed =
            LoggerMessage.Define<string, string>(
                LogLevel.Error,
                new EventId(1001, "EmailFailed"),
                "Errore nell'invio dell'email a {Recipient} con oggetto {Subject}");

        private static readonly Action<ILogger, int, Exception?> _batchCompleted =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(1002, "BatchCompleted"),
                "Batch di {Count} email completato con successo");

        /// <summary>
        /// Registra l'invio di un'email
        /// </summary>
        public static void LogEmailSent(this ILogger logger, string recipient, string subject)
        {
            _emailSent(logger, recipient, subject, null);
        }

        /// <summary>
        /// Registra un errore nell'invio di un'email
        /// </summary>
        public static void LogEmailFailed(this ILogger logger, string recipient, string subject, Exception ex)
        {
            _emailFailed(logger, recipient, subject, ex);
        }

        /// <summary>
        /// Registra il completamento di un batch di email
        /// </summary>
        public static void LogBatchCompleted(this ILogger logger, int count)
        {
            _batchCompleted(logger, count, null);
        }
    }
}