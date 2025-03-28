using EmailService.API.Controllers;
using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Tests.Controllers
{
    public class EmailControllerTests
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<EmailController>> _mockLogger;
        private readonly EmailController _controller;

        public EmailControllerTests()
        {
            // Setup delle dipendenze con mock
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<EmailController>>();
            
            // Creazione del controller con le dipendenze mock
            _controller = new EmailController(_mockEmailService.Object, _mockLogger.Object);
            
            // Setup di un HttpContext di base
            var httpContext = new DefaultHttpContext();
            httpContext.Items["ApplicationName"] = "TestApp";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task SendEmail_WithValidMessage_ReturnsOkResult()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body",
                IsHtml = false
            };

            _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SendEmail(message);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // Verifica che il metodo del servizio sia stato chiamato esattamente una volta
            _mockEmailService.Verify(x => x.SendEmailAsync(It.Is<EmailMessage>(m => 
                m.To == message.To &&
                m.Subject == message.Subject &&
                m.Body == message.Body)), Times.Once);
        }

        [Fact]
        public async Task SendEmail_WithInvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "invalid-email",  // Email non valida
                Subject = "Test Subject",
                Body = "Test Body"
            };

            // Act
            var result = await _controller.SendEmail(message);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            
            // Verifica che il servizio non sia stato chiamato
            _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<EmailMessage>()), Times.Never);
        }

        [Fact]
        public async Task SendEmail_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var message = new EmailMessage
            {
                To = "test@example.com",
                Subject = "Test Subject",
                Body = "Test Body"
            };

            _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>()))
                .ThrowsAsync(new System.Exception("Test exception"));

            // Act
            var result = await _controller.SendEmail(message);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task SendEmailBatch_WithValidMessages_ReturnsOkResult()
        {
            // Arrange
            var messages = new List<EmailMessage>
            {
                new EmailMessage { To = "test1@example.com", Subject = "Test 1", Body = "Body 1" },
                new EmailMessage { To = "test2@example.com", Subject = "Test 2", Body = "Body 2" }
            };

            _mockEmailService.Setup(x => x.SendEmailsAsync(It.IsAny<IEnumerable<EmailMessage>>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SendEmailBatch(messages);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            _mockEmailService.Verify(x => x.SendEmailsAsync(It.IsAny<IEnumerable<EmailMessage>>()), Times.Once);
        }

        [Fact]
        public async Task SendEmailBatch_WithEmptyList_ReturnsBadRequest()
        {
            // Arrange
            var messages = new List<EmailMessage>();

            // Act
            var result = await _controller.SendEmailBatch(messages);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            
            _mockEmailService.Verify(x => x.SendEmailsAsync(It.IsAny<IEnumerable<EmailMessage>>()), Times.Never);
        }

        [Fact]
        public async Task SendEmailBatch_WithInvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var messages = new List<EmailMessage>
            {
                new EmailMessage { To = "valid@example.com", Subject = "Test 1", Body = "Body 1" },
                new EmailMessage { To = "invalid-email", Subject = "Test 2", Body = "Body 2" }
            };

            // Act
            var result = await _controller.SendEmailBatch(messages);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            
            _mockEmailService.Verify(x => x.SendEmailsAsync(It.IsAny<IEnumerable<EmailMessage>>()), Times.Never);
        }
    }
}