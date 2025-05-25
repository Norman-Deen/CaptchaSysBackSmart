using Microsoft.AspNetCore.Mvc;

namespace CaptchaApi.Controllers
{
    [ApiController]
    [Route("api/ping")]
    public class PingController : ControllerBase
    {
        // Lightweight endpoint used to keep the Render app alive
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok("Pong - App is alive");
        }
    }
}
