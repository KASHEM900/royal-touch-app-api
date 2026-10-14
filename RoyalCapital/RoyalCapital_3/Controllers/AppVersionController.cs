
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace RoyalCapital_3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppVersionController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AppVersionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetAppVersion")]
        public IActionResult GetAppVersion()
        {
            var androidVersion = _configuration["androidAppVersion"];

            var iosVersion = _configuration["iosAppVersion"];

            var androidUrl =_configuration["androidAppUrl"];

            var iosUrl = _configuration["iosAppUrl"];

            var updateMessage = _configuration["appUpdateMessage"];

            if (string.IsNullOrWhiteSpace(androidVersion) || string.IsNullOrWhiteSpace(iosVersion) || string.IsNullOrWhiteSpace(androidUrl) ||
                string.IsNullOrWhiteSpace(iosUrl))
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "App update configuration is missing."
                    });
            }

            return Ok(new
            {
                androidAppVersion = androidVersion,
                iosAppVersion = iosVersion,
                androidAppUrl = androidUrl,
                iosAppUrl = iosUrl,
                appUpdateMessage = string.IsNullOrWhiteSpace(updateMessage) ? "A new version of Royal Touch is available." : updateMessage
            });
        }
    }
}
