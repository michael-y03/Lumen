using Microsoft.AspNetCore.Mvc;

namespace Lumen.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Lumen is running");
        }
    }
}