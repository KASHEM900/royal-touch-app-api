using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using RoyalCapital_3.Models;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BOLoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BOLoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("")]
        public IActionResult BoCheckLoginCredentials([FromBody] BOLoginModel lbomodel)
        {
            if (lbomodel == null || string.IsNullOrEmpty(lbomodel.TrackingNo) || string.IsNullOrEmpty(lbomodel.Password))
            {
                return BadRequest(new { error = "TrackingNo and Password are required." });
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("RCLWEB");
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT TRACKINGNO, PASS, FIRSTNAME, LASTNAME, MOBILE, IsApp1, IsEkycValid FROM T_ONLINE_BO_REG WHERE TRACKINGNO = @tracking AND PASS = @pass";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@tracking", lbomodel.TrackingNo);
                    cmd.Parameters.AddWithValue("@pass", lbomodel.Password);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string isApproved = reader["IsApp1"]?.ToString();
                        if (isApproved == "1")
                        {
                            return NotFound(new { error = "Your Online BO Application is already approved." });
                        }

                        var trackingNo = reader["TRACKINGNO"].ToString();
                        var username = $"{reader["FIRSTNAME"]} {reader["LASTNAME"]}".Trim();
                        var mobile = reader["MOBILE"].ToString();
 
                        BOOtherDBHandler DB = new BOOtherDBHandler(_configuration);
                        SelectList BRANCHES = new SelectList(DB.branchInfo().ToList(), "BRANCHNAME", "BRANCHNAME");

                        BOOnlineBoDetailDBHandle BDB = new BOOnlineBoDetailDBHandle(_configuration);
                        var AccHolderSelect = BDB.AccHolderSelect(trackingNo).ToList();

                        BOOtherDBHandler DB1 = new BOOtherDBHandler(_configuration);
                        var Title = DB1.GetTitle().ToList();

                        return Ok(new
                        {
                            success = "Credential Matched",
                            trackingNo = trackingNo,
                            username = username,
                            mobile = mobile,
                            BRANCHES = BRANCHES,
                            AccHolderSelect = AccHolderSelect,
                            Title = Title
                        });
                    }
                    else
                    {
                        return NotFound(new { error = "Invalid Credentials" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Server error", details = ex.Message });
            }
        }


    }
}
