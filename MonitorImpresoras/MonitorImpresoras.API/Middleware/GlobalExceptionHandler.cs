using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonitorImpresoras.API.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = GetErrorMessage(exception),
                Details = GetInnerExceptionDetails(exception),
                Timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private static string GetErrorMessage(Exception exception)
        {
            return exception switch
            {
                ArgumentException argEx => "Invalid argument: " + argEx.Message,
                UnauthorizedAccessException => "Access denied: " + exception.Message,
                KeyNotFoundException => "Resource not found: " + exception.Message,
                InvalidOperationException => "Invalid operation: " + exception.Message,
                _ => "An internal server error occurred"
            };
        }

        private static string GetInnerExceptionDetails(Exception exception)
        {
            if (exception.InnerException == null)
                return null;

            return exception.InnerException.Message;
        }
    }
}
