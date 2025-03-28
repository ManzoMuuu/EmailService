using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EmailService.Infrastructure.Services
{
    /// <summary>
    /// Implementazione del servizio per la creazione di allegati email
    /// </summary>
    public class EmailAttachmentService : IEmailAttachmentService
    {
        private readonly ILogger<EmailAttachmentService> _logger;

        /// <summary>
        /// Costruttore che inizializza il servizio con il logger
        /// </summary>
        /// <param name="logger">Logger per registrare le operazioni e gli errori</param>
        public EmailAttachmentService(ILogger<EmailAttachmentService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Crea un allegato email da un file fisico
        /// </summary>
        /// <param name="filePath">Percorso completo del file</param>
        /// <returns>Un oggetto EmailAttachment pronto per essere allegato</returns>
        public async Task<EmailAttachment> CreateFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("File non trovato: {filePath}", filePath);
                throw new FileNotFoundException("Il file specificato non esiste", filePath);
            }

            try
            {
                _logger.LogDebug("Creazione allegato dal file: {filePath}", filePath);
                
                // Legge il file in memoria
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                
                // Ottiene il nome del file e il tipo MIME
                string fileName = Path.GetFileName(filePath);
                string contentType = MimeTypes.GetMimeType(fileName);
                
                _logger.LogInformation("Allegato creato da file: {fileName}, tipo: {contentType}, dimensione: {size} bytes", 
                    fileName, contentType, fileBytes.Length);
                
                return new EmailAttachment
                {
                    FileName = fileName,
                    Content = fileBytes,
                    ContentType = contentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'allegato dal file: {filePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Crea un allegato email da uno stream di dati
        /// </summary>
        /// <param name="stream">Stream contenente i dati dell'allegato</param>
        /// <param name="fileName">Nome del file da assegnare all'allegato</param>
        /// <param name="contentType">Tipo MIME del contenuto (opzionale)</param>
        /// <returns>Un oggetto EmailAttachment pronto per essere allegato</returns>
        public async Task<EmailAttachment> CreateFromStreamAsync(Stream stream, string fileName, string? contentType = null)
        {
            try
            {
                _logger.LogDebug("Creazione allegato da stream per il file: {fileName}", fileName);
                
                // Legge lo stream in un array di byte
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] content = memoryStream.ToArray();
                
                // Determina il tipo MIME se non specificato
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = MimeTypes.GetMimeType(fileName);
                }
                
                _logger.LogInformation("Allegato creato da stream: {fileName}, tipo: {contentType}, dimensione: {size} bytes", 
                    fileName, contentType, content.Length);
                
                return new EmailAttachment
                {
                    FileName = fileName,
                    Content = content,
                    ContentType = contentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'allegato da stream per il file: {fileName}", fileName);
                throw;
            }
        }

        /// <summary>
        /// Crea un allegato email da un array di byte
        /// </summary>
        /// <param name="content">Array di byte contenente i dati dell'allegato</param>
        /// <param name="fileName">Nome del file da assegnare all'allegato</param>
        /// <param name="contentType">Tipo MIME del contenuto (opzionale)</param>
        /// <returns>Un oggetto EmailAttachment pronto per essere allegato</returns>
        public EmailAttachment CreateFromBytes(byte[] content, string fileName, string? contentType = null)
        {
            _logger.LogDebug("Creazione allegato da byte array per il file: {fileName}", fileName);
            
            // Determina il tipo MIME se non specificato
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = MimeTypes.GetMimeType(fileName);
            }
            
            _logger.LogInformation("Allegato creato da byte array: {fileName}, tipo: {contentType}, dimensione: {size} bytes", 
                fileName, contentType, content.Length);
            
            return new EmailAttachment
            {
                FileName = fileName,
                Content = content,
                ContentType = contentType
            };
        }
    }
}