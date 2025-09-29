using System.Net;
using System.Text.Json;
using Serilog;
using MonitorImpresoras.Domain.Constants;

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
            var errorCode = ErrorCodes.UnexpectedError;
            var message = ErrorMessages.Messages[errorCode];

            // Mapear excepciones específicas a códigos de error y códigos HTTP
            switch (exception)
            {
                case FluentValidation.ValidationException validationEx:
                    statusCode = HttpStatusCode.BadRequest;
                    errorCode = ErrorCodes.ValidationFailed;
                    message = "Los datos de entrada no son válidos";
                    break;

                case KeyNotFoundException:
                case InvalidOperationException when exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase):
                    statusCode = HttpStatusCode.NotFound;
                    errorCode = ErrorCodes.PrinterNotFound;
                    message = "El recurso solicitado no fue encontrado";
                    break;

                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    errorCode = ErrorCodes.AuthenticationFailed;
                    message = "No tienes permisos para acceder a este recurso";
                    break;

                case Microsoft.AspNetCore.Identity.IdentityErrorDescriber:
                    statusCode = HttpStatusCode.BadRequest;
                    errorCode = ErrorCodes.ValidationFailed;
                    message = "Error en la operación de identidad";
                    break;

                case Microsoft.EntityFrameworkCore.DbUpdateException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorCode = ErrorCodes.DatabaseConstraintViolation;
                    message = "Error de integridad en la base de datos";
                    break;

                case Microsoft.Data.SqlClient.SqlException:
                    statusCode = HttpStatusCode.InternalServerError;
                    errorCode = ErrorCodes.DatabaseConnectionError;
                    message = "Error de conexión con la base de datos";
                    break;

                case System.Security.Authentication.AuthenticationException:
                    statusCode = HttpStatusCode.Unauthorized;
                    errorCode = ErrorCodes.InvalidCredentials;
                    message = "Credenciales inválidas";
                    break;

                case System.Security.SecurityException:
                    statusCode = HttpStatusCode.Forbidden;
                    errorCode = ErrorCodes.InsufficientPermissions;
                    message = "Permisos insuficientes para esta operación";
                    break;

                case ArgumentException argEx when argEx.Message.Contains("IP"):
                    statusCode = HttpStatusCode.BadRequest;
                    errorCode = ErrorCodes.InvalidInputFormat;
                    message = "Formato de dirección IP inválido";
                    break;

                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorCode = ErrorCodes.InvalidInputFormat;
                    message = "Datos de entrada inválidos";
                    break;

                case TimeoutException:
                    statusCode = HttpStatusCode.RequestTimeout;
                    errorCode = ErrorCodes.ServiceUnavailable;
                    message = "La operación tardó demasiado tiempo";
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    errorCode = ErrorCodes.UnexpectedError;
                    message = "Ha ocurrido un error inesperado";
                    break;
            }

            // Loguear la excepción con detalles completos
            _logger.LogError(exception,
                "Error no controlado [{TraceId}]: {ErrorCode} - {Message} | Status: {StatusCode} | Path: {Path} | User: {UserId}",
                traceId, errorCode, message, (int)statusCode, context.Request.Path, GetUserId(context));

            // Crear respuesta JSON estandarizada
            var errorResponse = new
            {
                ErrorCode = errorCode,
                Message = message,
                Details = GetEnvironmentSpecificDetails(context, exception),
                TraceId = traceId,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                Method = context.Request.Method
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }

        private string GetUserId(HttpContext context)
        {
            return context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        }

        private object? GetEnvironmentSpecificDetails(HttpContext context, Exception exception)
        {
            var environment = context.RequestServices.GetService<IWebHostEnvironment>();
            if (environment?.IsDevelopment() == true)
            {
                return new
                {
                    ExceptionType = exception.GetType().Name,
                    ExceptionMessage = exception.Message,
                    StackTrace = exception.StackTrace
                };
            }

            return null;
        }
    }
}
