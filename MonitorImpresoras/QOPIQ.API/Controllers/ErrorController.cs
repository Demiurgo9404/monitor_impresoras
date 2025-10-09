using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace QOPIQ.API.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult HandleError()
        {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionHandlerFeature?.Error;
            
            // Registrar el error en el sistema de logs
            // _logger.LogError(exception, "Error no manejado");
            
            var response = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "Ha ocurrido un error en el servidor. Por favor, intente nuevamente más tarde.",
                Detail = exception?.Message,
                StackTrace = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() 
                    ? exception?.ToString() 
                    : null
            };
            
            return StatusCode(response.StatusCode, response);
        }
        
        [Route("/error-development")]
        public IActionResult HandleErrorDevelopment(
            [FromServices] IWebHostEnvironment webHostEnvironment)
        {
            if (!webHostEnvironment.IsDevelopment())
            {
                return NotFound();
            }

            var exceptionHandlerFeature =
                HttpContext.Features.Get<IExceptionHandlerFeature>()!;

            return Problem(
                detail: exceptionHandlerFeature.Error.StackTrace,
                title: exceptionHandlerFeature.Error.Message);
        }
    }
}

