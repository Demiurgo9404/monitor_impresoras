using System.Net;
using System.Text.Json;
using Serilog;

namespace MonitorImpresoras.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = Guid.NewGuid().ToString();
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Ha ocurrido un error inesperado";

            // Mapear excepciones conocidas a códigos HTTP específicos
            switch (exception)
            {
                case FluentValidation.ValidationException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Los datos de entrada no son válidos";
                    break;
                case KeyNotFoundException:
                case InvalidOperationException when exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase):
                    statusCode = HttpStatusCode.NotFound;
                    message = "El recurso solicitado no fue encontrado";
                    break;
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "No tienes permisos para acceder a este recurso";
                    break;
                case Microsoft.AspNetCore.Identity.IdentityErrorDescriber:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Error en la operación de identidad";
                    break;
            }

            // Loguear la excepción con detalles
            _logger.LogError(exception, "Error no controlado en {TraceId}: {Message}", traceId, exception.Message);

            // Crear respuesta JSON estándar
            var errorResponse = new
            {
                TraceId = traceId,
                StatusCode = (int)statusCode,
                Message = message,
                Details = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true
                    ? exception.Message
                    : null,
                Timestamp = DateTime.UtcNow
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
