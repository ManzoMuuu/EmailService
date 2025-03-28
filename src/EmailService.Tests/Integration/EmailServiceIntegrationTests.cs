using EmailService.API.Controllers;
using EmailService.Core.Models;
using EmailService.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Tests.Integration
{
    /// <summary>
    /// Test di integrazione che verificano l'interazione tra il controller e il servizio.
    /// Questi test non usano componenti reali esterni come il server SMTP.
    /// </summary>
    public class EmailServiceIntegrationTests
    {
        private readonly EmailController _controller;
        private readonly GmailService _gmailService;
        private readonly Mock<IOptions<EmailConfig>> _mockOptions;
        private readonly Mock<ILogger<GmailService>> _mockServiceLogger;
        private readonly Mock<ILogger<EmailController>> _mockControllerLogger;

        public EmailServiceIntegrationTests()
        {
            // Configurazione di test
            var emailConfig = new EmailConfig
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
            _mockOptions.Setup(x => x.Value).Returns(emailConfig);
            
            _mockServiceLogger = new Mock<ILogger<GmailService>>();
            _mockControllerLogger = new Mock<ILogger<EmailController>>();
            
            // Questi sono test di integrazione parziali dove testiamo
            // l'interazione tra componenti reali, ma interrompiamo la catena
            // prima di arrivare a componenti esterni (SMTP)
            _gmailService = new Mock<GmailService>(_mockOptions.Object, _mockServiceLogger.Object) 
            {
                CallBase = true // Usa l'implementazione reale dove possibile
            }.Object;
            
            _controller = new EmailController(_gmailService, _mockControllerLogger.Object);
            
            // Setup di un contesto HTTP minimo
            var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            httpContext.Items["ApplicationName"] = "TestApp";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact(Skip = "Questo test richiede una connessione SMTP reale")]
        public async Task SendEmail_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "test@example.com",
                Subject = "Integration Test",
                Body = "This is an integration test.",
                IsHtml = false
            };

            // Act
            var result = await _controller.SendEmail(message);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
        
        [Fact]
        public async Task SendEmailBatch_EmptyList_ReturnsBadRequest()
        {
            // Arrange - una lista vuota di messaggi
            var messages = new System.Collections.Generic.List<EmailMessage>();

            // Act
            var result = await _controller.SendEmailBatch(messages);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}