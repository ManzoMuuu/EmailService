// src/EmailService.Core/Interfaces/IEmailAttachmentService.cs
using EmailService.Core.Models;
using System.IO;
using System.Threading.Tasks;

namespace EmailService.Core.Interfaces
{
    /// <summary>
    /// Interfaccia che definisce le operazioni per creare allegati email da diverse fonti
    /// </summary>
    public interface IEmailAttachmentService
    {
        /// <summary>
        /// Crea un allegato email da un file fisico
        /// </summary>
        /// <param name="filePath">Percorso completo del file</param>
        /// <returns>Un oggetto EmailAttachment pronto per essere allegato</returns>
        Task<EmailAttachment> CreateFromFileAsync(string filePath);

        /// <summary>
        /// Crea un allegato email da uno stream di dati
        /// </summary>
        /// <param name="stream">Stream contenente i dati dell'allegato</param>
        /// <param name="fileName">Nome del file da assegnare all'allegato</param>
        /// <param name="contentType">Tipo MIME del contenuto (opzionale)</param>
        /// <returns>Un oggetto EmailAttachment pronto per essere allegato</returns>
        Task<EmailAttachment> CreateFromStreamAsync(Stream stream, string fileName, string? contentType = null);

        /// <summary>
        /// Crea un allegato email da un array di byte
        /// </summary>
        /// <param name="content">Array di byte contenente i dati dell'allegato</param>
        /// <param name="fileName">Nome del file da assegnare all'allegato</param>
        /// <param name="contentType">Tipo MIME del contenuto (opzionale)</param>
        /// <returns>Un oggetto EmailAttachment pronto per essere allegato</returns>
        EmailAttachment CreateFromBytes(byte[] content, string fileName, string? contentType = null);
    }
}