using EmailService.API.Extensions;
using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API.Controllers
{
    /// <summary>
    /// Controller per la gestione delle operazioni di invio email
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailController> _logger;

        /// <summary>
        /// Costruttore che inizializza il controller con le dipendenze necessarie
        /// </summary>
        /// <param name="emailService">Servizio per l'invio di email</param>
        /// <param name="logger">Logger per registrare le operazioni</param>
        public EmailController(IEmailService emailService, ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Invia una singola email
        /// </summary>
        /// <param name="message">Modello contenente i dati dell'email da inviare</param>
        /// <returns>Risultato dell'operazione di invio</returns>
        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendEmail([FromBody] EmailMessage message)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Tentativo di invio email con dati non validi: {ValidationErrors}", 
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
                        
                return BadRequest(ApiResponse<object>.ErrorResponse("Dati non validi", ModelState));
            }

            try
            {
                // Controlla se è stato passato un nome applicazione nell'header
                var appName = HttpContext.Items["ApplicationName"]?.ToString() ?? "internal-app";
                _logger.LogInformation("Richiesta di invio email ricevuta da {AppName} per {Recipient}", appName, message.To);
                
                // Esegue il controllo di validità dell'email
                if (string.IsNullOrEmpty(message.To) || !IsValidEmail(message.To))
                {
                    _logger.LogWarning("Tentativo di invio a indirizzo email non valido: {Recipient}", message.To);
                    return BadRequest(ApiResponse<object>.ErrorResponse("L'indirizzo email del destinatario non è valido"));
                }
                
                // Effettua l'invio dell'email
                await _emailService.SendEmailAsync(message);
                
                // Registra il successo dell'operazione
                _logger.LogEmailSent(message.To, message.Subject);
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { Recipient = message.To }, 
                    "Email inviata con successo"
                ));
            }
            catch (Exception ex)
            {
                // Registra l'errore con tutti i dettagli disponibili
                _logger.LogEmailFailed(message.To, message.Subject, ex);
                
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Si è verificato un errore durante l'invio dell'email", 
                    new { ErrorMessage = ex.Message }
                ));
            }
        }

        /// <summary>
        /// Invia un batch di email a diversi destinatari
        /// </summary>
        /// <param name="messages">Lista di email da inviare</param>
        /// <returns>Risultato dell'operazione di invio batch</returns>
        [HttpPost("send-batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendEmailBatch([FromBody] List<EmailMessage> messages)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Dati non validi", ModelState));
            }

            if (messages == null || !messages.Any())
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("La lista delle email è vuota"));
            }

            try
            {
                var count = messages.Count;
                _logger.LogInformation("Richiesta di invio batch di {Count} email ricevuta", count);
                
                // Validazione preliminare di tutti gli indirizzi email
                var invalidEmails = messages
                    .Where(m => string.IsNullOrEmpty(m.To) || !IsValidEmail(m.To))
                    .Select(m => m.To)
                    .ToList();
                
                if (invalidEmails.Any())
                {
                    _logger.LogWarning("Batch contiene {Count} indirizzi email non validi", invalidEmails.Count);
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Alcuni indirizzi email non sono validi",
                        new { InvalidEmails = invalidEmails }
                    ));
                }
                
                // Invio del batch di email
                await _emailService.SendEmailsAsync(messages);
                
                // Registra il successo dell'operazione
                _logger.LogBatchCompleted(count);
                
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { Count = count, Recipients = messages.Select(m => m.To).ToList() },
                    $"{count} email inviate con successo"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'invio batch di {Count} email", messages.Count);
                
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Si è verificato un errore durante l'invio delle email", 
                    new { ErrorMessage = ex.Message }
                ));
            }
        }

        /// <summary>
        /// Verifica la validità di un indirizzo email tramite espressione regolare
        /// </summary>
        /// <param name="email">Indirizzo email da verificare</param>
        /// <returns>True se l'indirizzo è valido, False altrimenti</returns>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}