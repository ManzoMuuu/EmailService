using EmailService.Core.Models;
using System;
using Xunit;

namespace EmailService.Tests.Models
{
    public class ApiResponseTests
    {
        [Fact]
        public void SuccessResponse_CreatesResponseWithCorrectProperties()
        {
            // Arrange
            var data = "Test Data";
            var message = "Success Message";

            // Act
            var response = ApiResponse<string>.SuccessResponse(data, message);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(message, response.Message);
            Assert.Equal(data, response.Data);
            Assert.Null(response.Errors);
        }

        [Fact]
        public void SuccessResponse_WithDefaultMessage_SetsDefaultMessage()
        {
            // Arrange
            var data = 42;

            // Act
            var response = ApiResponse<int>.SuccessResponse(data);

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Operation completed successfully", response.Message);
            Assert.Equal(data, response.Data);
        }

        [Fact]
        public void ErrorResponse_CreatesResponseWithCorrectProperties()
        {
            // Arrange
            var message = "Error Message";
            var errors = new { Field = "Username", Error = "Required" };

            // Act
            var response = ApiResponse<object>.ErrorResponse(message, errors);

            // Assert
            Assert.False(response.Success);
            Assert.Equal(message, response.Message);
            Assert.Null(response.Data);
            Assert.Equal(errors, response.Errors);
        }

        [Fact]
        public void ErrorResponse_WithNullErrors_SetsNullErrorsProperty()
        {
            // Arrange
            var message = "Error without details";

            // Act
            var response = ApiResponse<string>.ErrorResponse(message);

            // Assert
            Assert.False(response.Success);
            Assert.Equal(message, response.Message);
            Assert.Null(response.Data);
            Assert.Null(response.Errors);
        }
    }
}