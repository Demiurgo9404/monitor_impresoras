using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BaseApiController : ControllerBase
    {
        protected string UserId => User?.FindFirst("uid")?.Value;
        protected string UserEmail => User?.FindFirst("email")?.Value;
        
        protected IActionResult HandleResult<T>(T result)
        {
            if (result == null) return NotFound();
            return Ok(result);
        }
        
        protected IActionResult HandlePagedResult<T>(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize)
        {
            if (items == null || !items.Any()) return NotFound();
            
            Response.Headers.Add("X-Pagination", 
                System.Text.Json.JsonSerializer.Serialize(new 
                { 
                    TotalItems = totalItems,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                }));
                
            return Ok(items);
        }
    }
}
