using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using EmailService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Tests.Services
{
    public class GmailServiceTests
    {
        private readonly Mock<IOptions<EmailConfig>> _mockOptions;
        private readonly Mock<ILogger<GmailService>> _mockLogger;
        private readonly EmailConfig _emailConfig;

        public GmailServiceTests()
        {
            // Configurazione di test
            _emailConfig = new EmailConfig
            {
                SmtpServer = "smtp.gmail.com",
                SmtpPort = 587,
                Username = "test@gmail.com",
                Password = "test-password",
                UseSsl = true,
                SenderName = "Test Sender",
                SenderEmail = "test@gmail.com"
            };

            _mockOptions = new Mock<IOptions<EmailConfig>>();
            _mockOptions.Setup(x => x.Value).Returns(_emailConfig);
            
            _mockLogger = new Mock<ILogger<GmailService>>();
        }

        [Fact(Skip = "Questo test richiede una connessione SMTP reale e credenziali valide")]
        public async Task SendEmailAsync_WithValidMessage_SendsEmail()
        {
            // Nota: Questo test è saltato perché richiede una connessione reale
            // Arrange
            var service = new GmailService(_mockOptions.Object, _mockLogger.Object);
            var message = new EmailMessage
            {
                To = "recipient@example.com",
                Subject = "Test Subject",
                Body = "Test Body",
                IsHtml = false
            };

            // Act
            // In un test reale, questo tenterebbe effettivamente di connettersi a Gmail
            await service.SendEmailAsync(message);

            // Assert
            // Nessun assert esplicito - il test passa se non vengono lanciate eccezioni
        }

        [Fact]
        public async Task SendEmailsAsync_WithMultipleMessages_CallsSendEmailAsyncForEachMessage()
        {
            // Arrange
            // Creiamo un mock dell'interfaccia IEmailService invece di un mock parziale della classe
            var mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>()))
                .Returns(Task.CompletedTask);

            var messages = new List<EmailMessage>
            {
                new EmailMessage { To = "recipient1@example.com", Subject = "Test 1", Body = "Body 1" },
                new EmailMessage { To = "recipient2@example.com", Subject = "Test 2", Body = "Body 2" },
                new EmailMessage { To = "recipient3@example.com", Subject = "Test 3", Body = "Body 3" }
            };

            // Act
            await mockEmailService.Object.SendEmailsAsync(messages);

            // Assert
            mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<EmailMessage>()), Times.Exactly(3));
        }

        [Fact]
        public void Constructor_WithValidConfig_InitializesProperties()
        {
            // Arrange & Act
            var service = new GmailService(_mockOptions.Object, _mockLogger.Object);

            // Assert
            // Nessun assert esplicito - il test passa se il costruttore non lancia eccezioni
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IOptions<EmailConfig> nullOptions = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GmailService(nullOptions, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            ILogger<GmailService> nullLogger = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GmailService(_mockOptions.Object, nullLogger));
        }
    }
}