using EmailService.Core.Models;
using EmailService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Tests.Services
{
    public class EmailAttachmentServiceTests
    {
        private readonly Mock<ILogger<EmailAttachmentService>> _mockLogger;
        private readonly EmailAttachmentService _service;

        public EmailAttachmentServiceTests()
        {
            _mockLogger = new Mock<ILogger<EmailAttachmentService>>();
            _service = new EmailAttachmentService(_mockLogger.Object);
        }

        [Fact]
        public void CreateFromBytes_WithValidInput_ReturnsEmailAttachment()
        {
            // Arrange
            var fileName = "test.txt";
            var content = Encoding.UTF8.GetBytes("This is a test");
            var contentType = "text/plain";

            // Act
            var result = _service.CreateFromBytes(content, fileName, contentType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fileName, result.FileName);
            Assert.Equal(contentType, result.ContentType);
            Assert.Equal(content, result.Content);
        }

        [Fact]
        public void CreateFromBytes_WithoutContentType_UsesDefaultMimeType()
        {
            // Arrange
            var fileName = "test.txt";
            var content = Encoding.UTF8.GetBytes("This is a test");

            // Act
            var result = _service.CreateFromBytes(content, fileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fileName, result.FileName);
            Assert.Equal("text/plain", result.ContentType); // MimeKit dovrebbe rilevare il tipo in base all'estensione
            Assert.Equal(content, result.Content);
        }

        [Fact]
        public async Task CreateFromStreamAsync_WithValidInput_ReturnsEmailAttachment()
        {
            // Arrange
            var fileName = "test.txt";
            var content = Encoding.UTF8.GetBytes("This is a test from stream");
            var contentType = "text/plain";
            
            using var stream = new MemoryStream(content);

            // Act
            var result = await _service.CreateFromStreamAsync(stream, fileName, contentType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fileName, result.FileName);
            Assert.Equal(contentType, result.ContentType);
            Assert.Equal(content.Length, result.Content.Length);
            
            // Confronto byte per byte
            for (int i = 0; i < content.Length; i++)
            {
                Assert.Equal(content[i], result.Content[i]);
            }
        }

        [Fact]
        public async Task CreateFromFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                _service.CreateFromFileAsync(nonExistentFilePath));
        }

        [Fact]
        public async Task CreateFromFileAsync_WithValidFile_ReturnsEmailAttachment()
        {
            // Arrange
            var fileName = "testfile.txt";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            var content = "This is a test file content";
            
            try
            {
                // Crea un file temporaneo per il test
                await File.WriteAllTextAsync(filePath, content);
                
                // Act
                var result = await _service.CreateFromFileAsync(filePath);
                
                // Assert
                Assert.NotNull(result);
                Assert.Equal(fileName, result.FileName);
                Assert.Equal("text/plain", result.ContentType);
                
                // Verifica il contenuto
                var resultContent = Encoding.UTF8.GetString(result.Content);
                Assert.Equal(content, resultContent);
            }
            finally
            {
                // Pulizia: elimina il file temporaneo
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}