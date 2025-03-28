namespace EmailService.Core.Models
{
    /// <summary>
    /// Configurazioni per la connessione al server SMTP e l'invio di email
    /// </summary>
    public class EmailConfig
    {
        /// <summary>
        /// Indirizzo del server SMTP (es. "smtp.gmail.com")
        /// </summary>
        public string SmtpServer { get; set; } = string.Empty;
        
        /// <summary>
        /// Porta del server SMTP (es. 587 per TLS, 465 per SSL)
        /// </summary>
        public int SmtpPort { get; set; }
        
        /// <summary>
        /// Nome utente per l'autenticazione SMTP (tipicamente l'indirizzo email)
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Password per l'autenticazione SMTP (o App Password per Gmail con 2FA)
        /// </summary>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se utilizzare SSL per la connessione
        /// </summary>
        public bool UseSsl { get; set; } = true;
        
        /// <summary>
        /// Nome del mittente che apparirà nell'email
        /// </summary>
        public string SenderName { get; set; } = string.Empty;
        
        /// <summary>
        /// Indirizzo email del mittente
        /// </summary>
        public string SenderEmail { get; set; } = string.Empty;
    }
}