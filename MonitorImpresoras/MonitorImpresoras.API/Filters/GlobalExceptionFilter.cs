using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MonitorImpresoras.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Error no manejado en la aplicación");

            var response = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "Ha ocurrido un error inesperado",
                Error = _env.IsDevelopment() ? context.Exception.Message : "Error interno del servidor",
                StackTrace = _env.IsDevelopment() ? context.Exception.StackTrace : null
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
