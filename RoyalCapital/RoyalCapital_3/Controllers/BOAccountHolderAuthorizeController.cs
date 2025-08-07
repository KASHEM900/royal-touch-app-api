using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RoyalCapital_3.BL;
using RoyalCapital_3.Models;
using RoyalCapitalBE.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BOAccountHolderAuthorizeController : ControllerBase
    {
        private readonly IConfiguration configuration; 
        private readonly string SqlConn;
        public BOAccountHolderAuthorizeController(IConfiguration config)
        {
            configuration = config;
            SqlConn = config.GetConnectionString("RCLWEB");
        }


        [HttpPost("boAccountHolderAuthorizeInfo")]
        public IActionResult boAccountHolderAuthorizeInfo([FromBody] BOAccountHolderAuthorizeModel azbomodel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(configuration);
                    
                    if (DB.AddAuthorize(azbomodel))
                    {
                        return Ok(new
                        {
                            success = "Authorize Info Successfully Inserted",
                        });
                    }
                    else
                    {
                        return NotFound(new { error = "Failed to insert authorize info." });
                    }

                }
                else
                {
                   return BadRequest(new { error = "Invalid model state." });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database Error... Failed to insert authorize info !!!",
                    error = ex.Message
                });
            }
        }

        
    }
}
