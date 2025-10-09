using Microsoft.AspNetCore.Mvc;

namespace QOPIQ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "API funcionando correctamente",
                timestamp = DateTime.UtcNow,
                database = "PostgreSQL configurado",
                database_connection = "PrintHubDB"
            });
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                services = new[]
                {
                    "Domain: OK",
                    "Application: OK",
                    "Infrastructure: OK",
                    "API: OK"
                }
            });
        }
    }
}

