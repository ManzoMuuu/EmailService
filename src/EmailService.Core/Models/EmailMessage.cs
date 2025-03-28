using System;
using System.Collections.Generic;

namespace EmailService.Core.Models
{
    /// <summary>
    /// Rappresenta un messaggio email completo da inviare
    /// </summary>
    public class EmailMessage
    {
        /// <summary>
        /// Indirizzo email del destinatario
        /// </summary>
        public string To { get; set; } = string.Empty;
        
        /// <summary>
        /// Oggetto dell'email
        /// </summary>
        public string Subject { get; set; } = string.Empty;
        
        /// <summary>
        /// Corpo del messaggio email
        /// </summary>
        public string Body { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se il corpo dell'email è in formato HTML
        /// </summary>
        public bool IsHtml { get; set; } = true;
        
        /// <summary>
        /// Lista degli allegati dell'email (opzionale)
        /// </summary>
        public List<EmailAttachment>? Attachments { get; set; }
    }

    /// <summary>
    /// Rappresenta un allegato di un'email
    /// </summary>
    public class EmailAttachment
    {
        /// <summary>
        /// Nome del file allegato
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Contenuto binario del file
        /// </summary>
        public byte[] Content { get; set; } = Array.Empty<byte>();
        
        /// <summary>
        /// Tipo MIME del contenuto (es. "application/pdf", "image/jpeg")
        /// </summary>
        public string ContentType { get; set; } = "application/octet-stream";
    }
}