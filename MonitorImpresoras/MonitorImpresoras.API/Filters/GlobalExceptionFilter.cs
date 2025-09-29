using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace MonitorImpresoras.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Error no manejado en la aplicación");

            var response = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "Ha ocurrido un error inesperado",
                Error = context.Environment.IsDevelopment() ? context.Exception.Message : "Error interno del servidor",
                StackTrace = context.Environment.IsDevelopment() ? context.Exception.StackTrace : null
            };

            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }

    // Extensión para facilitar el registro del filtro
    public static class GlobalExceptionFilterExtensions
    {
        public static IServiceCollection AddGlobalExceptionFilter(this IServiceCollection services)
        {
            services.AddScoped<GlobalExceptionFilter>();
            return services;
        }
    }
}
