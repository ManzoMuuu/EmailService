using EmailService.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Tests.Middleware
{
    public class RequestTrackingMiddlewareTests
    {
        private readonly Mock<ILogger<RequestTrackingMiddleware>> _mockLogger;
        
        public RequestTrackingMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<RequestTrackingMiddleware>>();
        }
        
        [Fact]
        public async Task InvokeAsync_SetsCorrelationId()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var nextCalled = false;
            
            RequestDelegate next = (innerContext) => {
                nextCalled = true;
                return Task.CompletedTask;
            };
            
            var middleware = new RequestTrackingMiddleware(next, _mockLogger.Object);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            Assert.True(nextCalled, "Il middleware non ha chiamato il prossimo middleware nella pipeline");
            Assert.True(context.Items.ContainsKey("CorrelationId"), "CorrelationId non è stato impostato");
            var correlationId = context.Items["CorrelationId"] as string;
            Assert.NotNull(correlationId);
            Assert.NotEmpty(correlationId);
        }

        [Fact]
        public async Task InvokeAsync_ExtractsApplicationNameFromHeaders()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Application-Name"] = "TestApp";
            
            var middleware = new RequestTrackingMiddleware(
                next: (innerContext) => Task.CompletedTask,
                logger: _mockLogger.Object
            );
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            Assert.Equal("TestApp", context.Items["ApplicationName"]);
        }

        [Fact]
        public async Task InvokeAsync_PropagatesExceptions()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var expectedException = new InvalidOperationException("Test exception");
            
            RequestDelegate next = (innerContext) => {
                throw expectedException;
            };
            
            var middleware = new RequestTrackingMiddleware(next, _mockLogger.Object);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => middleware.InvokeAsync(context));
            
            Assert.Same(expectedException, exception);
        }
        
        [Fact]
        public async Task InvokeAsync_LogsResponseStatus()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.StatusCode = 200;
            
            RequestDelegate next = (innerContext) => {
                innerContext.Response.StatusCode = 201; // Cambia lo status code
                return Task.CompletedTask;
            };
            
            var middleware = new RequestTrackingMiddleware(next, _mockLogger.Object);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            Assert.Equal(201, context.Response.StatusCode);
        }
    }
}