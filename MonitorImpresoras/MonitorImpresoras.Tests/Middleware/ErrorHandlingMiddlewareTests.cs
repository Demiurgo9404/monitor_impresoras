using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MonitorImpresoras.API.Middleware;
using System.Text.Json;

namespace MonitorImpresoras.Tests.Middleware
{
    public class ErrorHandlingMiddlewareTests
    {
        [Fact]
        public async Task Invoke_ShouldReturnInternalServerError_WhenUnhandledExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var middleware = new ErrorHandlingMiddleware(
                (innerHttpContext) => throw new Exception("Test exception"),
                loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be(500);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            errorResponse.GetProperty("statusCode").GetInt32().Should().Be(500);
            errorResponse.GetProperty("message").GetString().Should().Be("Ha ocurrido un error inesperado");
            errorResponse.GetProperty("traceId").GetString().Should().NotBeNullOrEmpty();

            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task Invoke_ShouldReturnBadRequest_WhenValidationExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var middleware = new ErrorHandlingMiddleware(
                (innerHttpContext) => throw new FluentValidation.ValidationException("Validation failed"),
                loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be(400);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            errorResponse.GetProperty("statusCode").GetInt32().Should().Be(400);
            errorResponse.GetProperty("message").GetString().Should().Be("Los datos de entrada no son válidos");
        }

        [Fact]
        public async Task Invoke_ShouldReturnNotFound_WhenKeyNotFoundExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var middleware = new ErrorHandlingMiddleware(
                (innerHttpContext) => throw new KeyNotFoundException("Entity not found"),
                loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be(404);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            errorResponse.GetProperty("statusCode").GetInt32().Should().Be(404);
            errorResponse.GetProperty("message").GetString().Should().Be("El recurso solicitado no fue encontrado");
        }

        [Fact]
        public async Task Invoke_ShouldReturnUnauthorized_WhenUnauthorizedAccessExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var middleware = new ErrorHandlingMiddleware(
                (innerHttpContext) => throw new UnauthorizedAccessException("Access denied"),
                loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be(401);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            errorResponse.GetProperty("statusCode").GetInt32().Should().Be(401);
            errorResponse.GetProperty("message").GetString().Should().Be("No tienes permisos para acceder a este recurso");
        }

        [Fact]
        public async Task Invoke_ShouldIncludeDetails_WhenInDevelopmentEnvironment()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var middleware = new ErrorHandlingMiddleware(
                (innerHttpContext) => throw new Exception("Development exception"),
                loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Mock environment to be Development
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.IsDevelopment()).Returns(true);
            context.RequestServices = new ServiceCollection()
                .AddSingleton(envMock.Object)
                .BuildServiceProvider();

            // Act
            await middleware.Invoke(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            errorResponse.GetProperty("details").GetString().Should().Be("Development exception");
        }

        [Fact]
        public async Task Invoke_ShouldNotIncludeDetails_WhenNotInDevelopmentEnvironment()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var middleware = new ErrorHandlingMiddleware(
                (innerHttpContext) => throw new Exception("Production exception"),
                loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Mock environment to be Production
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.IsDevelopment()).Returns(false);
            context.RequestServices = new ServiceCollection()
                .AddSingleton(envMock.Object)
                .BuildServiceProvider();

            // Act
            await middleware.Invoke(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
            errorResponse.GetProperty("details").ValueKind.Should().Be(JsonValueKind.Null);
        }

        [Fact]
        public async Task Invoke_ShouldContinuePipeline_WhenNoExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
            var middleware = new ErrorHandlingMiddleware(
                (innerHttpContext) =>
                {
                    innerHttpContext.Response.StatusCode = 200;
                    return Task.CompletedTask;
                },
                loggerMock.Object);

            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be(200);
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()),
                Times.Never);
        }
    }
}
