using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmailService.Infrastructure.Services
{
    /// <summary>
    /// Implementazione del servizio email che utilizza Gmail tramite protocollo SMTP
    /// </summary>
    public class GmailService : IEmailService
    {
        private readonly EmailConfig _emailConfig;
        private readonly ILogger<GmailService> _logger;

        /// <summary>
        /// Costruttore che inizializza il servizio con la configurazione e il logger
        /// </summary>
        /// <param name="emailConfig">Configurazione per la connessione al server SMTP</param>
        /// <param name="logger">Logger per registrare le operazioni e gli errori</param>
        public GmailService(IOptions<EmailConfig> emailConfig, ILogger<GmailService> logger)
        {
            _emailConfig = emailConfig.Value;
            _logger = logger;
        }

        /// <summary>
        /// Invia una singola email in modo asincrono
        /// </summary>
        /// <param name="message">Il messaggio email da inviare</param>
        public async Task SendEmailAsync(EmailMessage message)
        {
            _logger.LogInformation("Preparazione dell'email per {recipient} con oggetto: {subject}", 
                message.To, message.Subject);
            
            var mimeMessage = CreateMimeMessage(message);
            await SendAsync(mimeMessage);
            
            _logger.LogInformation("Email inviata con successo a {recipient}", message.To);
        }

        /// <summary>
        /// Invia multiple email in modo asincrono
        /// </summary>
        /// <param name="messages">La collezione di messaggi email da inviare</param>
        public async Task SendEmailsAsync(IEnumerable<EmailMessage> messages)
        {
            _logger.LogInformation("Iniziando l'invio di un batch di email");
            
            int count = 0;
            foreach (var message in messages)
            {
                await SendEmailAsync(message);
                count++;
            }
            
            _logger.LogInformation("Batch di email completato: {count} email inviate", count);
        }

        /// <summary>
        /// Crea un oggetto MimeMessage da un EmailMessage
        /// </summary>
        /// <param name="message">Il messaggio email da convertire</param>
        /// <returns>Un oggetto MimeMessage pronto per l'invio</returns>
        private MimeMessage CreateMimeMessage(EmailMessage message)
        {
            var mimeMessage = new MimeMessage();
            
            // Imposta mittente
            mimeMessage.From.Add(new MailboxAddress(_emailConfig.SenderName, _emailConfig.SenderEmail));
            
            // Imposta destinatario
            mimeMessage.To.Add(MailboxAddress.Parse(message.To));
            
            // Imposta oggetto
            mimeMessage.Subject = message.Subject;

            // Costruisce il corpo dell'email
            var bodyBuilder = new BodyBuilder();
            
            if (message.IsHtml)
            {
                bodyBuilder.HtmlBody = message.Body;
            }
            else
            {
                bodyBuilder.TextBody = message.Body;
            }

            // Aggiungi allegati se presenti
            if (message.Attachments != null && message.Attachments.Count > 0)
            {
                _logger.LogInformation("Aggiungendo {count} allegati all'email", message.Attachments.Count);
                
                foreach (var attachment in message.Attachments)
                {
                    bodyBuilder.Attachments.Add(attachment.FileName, 
                        attachment.Content, 
                        ContentType.Parse(attachment.ContentType));
                }
            }

            mimeMessage.Body = bodyBuilder.ToMessageBody();
            
            return mimeMessage;
        }

        /// <summary>
        /// Invia un MimeMessage tramite SMTP
        /// </summary>
        /// <param name="mimeMessage">Il messaggio formato MimeKit da inviare</param>
        private async Task SendAsync(MimeMessage mimeMessage)
        {
            using var client = new SmtpClient();
            
            try
            {
                _logger.LogDebug("Connessione al server SMTP {server}:{port}", 
                    _emailConfig.SmtpServer, _emailConfig.SmtpPort);
                
                // Connessione al server SMTP
                await client.ConnectAsync(_emailConfig.SmtpServer, 
                    _emailConfig.SmtpPort, 
                    _emailConfig.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                
                // Autenticazione
                _logger.LogDebug("Autenticazione con username: {username}", _emailConfig.Username);
                await client.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
                
                // Invio del messaggio
                _logger.LogDebug("Invio del messaggio in corso...");
                await client.SendAsync(mimeMessage);
                
                _logger.LogInformation("Email inviata con successo a {recipient}", string.Join(", ", mimeMessage.To));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'invio dell'email a {recipient}", string.Join(", ", mimeMessage.To));
                throw; // Rilancia l'eccezione per gestirla a livello superiore
            }
            finally
            {
                // Disconnessione (anche in caso di errore)
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                    _logger.LogDebug("Disconnessione dal server SMTP completata");
                }
            }
        }
    }
}