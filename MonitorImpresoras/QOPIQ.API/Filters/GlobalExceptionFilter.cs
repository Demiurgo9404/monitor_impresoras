using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace QOPIQ.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Error no manejado en la aplicación");

            var response = new
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Error interno del servidor",
                Detail = context.Exception.Message,
                Instance = context.HttpContext.Request.Path,
                Timestamp = DateTime.UtcNow
            };

            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }
}

