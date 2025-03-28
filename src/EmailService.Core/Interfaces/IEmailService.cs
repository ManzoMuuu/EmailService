using EmailService.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmailService.Core.Interfaces
{
    /// <summary>
    /// Interfaccia che definisce le operazioni di base per un servizio di invio email
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Invia una singola email in modo asincrono
        /// </summary>
        /// <param name="message">Il messaggio email da inviare</param>
        /// <returns>Un task che rappresenta l'operazione asincrona</returns>
        Task SendEmailAsync(EmailMessage message);

        /// <summary>
        /// Invia multiple email in modo asincrono
        /// </summary>
        /// <param name="messages">La collezione di messaggi email da inviare</param>
        /// <returns>Un task che rappresenta l'operazione asincrona</returns>
        Task SendEmailsAsync(IEnumerable<EmailMessage> messages);
    }
}