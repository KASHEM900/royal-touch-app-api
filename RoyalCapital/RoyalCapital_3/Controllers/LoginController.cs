using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using RoyalCapitalBE.Models;
using RoyalCapital_3.Models;
using System.Globalization;

using System.Text;

using RoyalCapital_3.BL;

namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public LoginController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpPost("")]
        public IActionResult CheckLoginCredentials([FromBody] Login login)
        {
            //string sisConn = configuration.GetConnectionString("SISROYALU");
            string rclConn = configuration.GetConnectionString("RCLWEB");
            string key = configuration["secret"];
            string apiVersion = configuration["APIVersion"];
            string password = "";
            string lock_data = "0";
            string flag_data = "0";

            SqlConnection oConnection = new SqlConnection(rclConn);

            try
            {
                oConnection.Open();
                string query = "SELECT CONVERT(varchar(max), DECRYPTBYPASSPHRASE('royal',password)) password, lock, flag FROM RCLWEB.[RCLWEB].[dbo].T_USER_LOGIN WHERE mobile='" + login.MobileNo.Trim() + "'";
                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                if (oTable.Rows.Count == 0)
                {
                    return NotFound(new { error = "Invalid Credentials" });
                }

                foreach (DataRow dr in oTable.Rows)
                {
                    password = dr["password"].ToString();
                    lock_data = dr["lock"].ToString();
                    flag_data = dr["flag"].ToString();
                }

                if (password != login.Password.Trim())
                {
                    return NotFound(new { error = "Invalid Credentials" });
                }
                else if (password == login.Password.Trim())
                {
                    if (lock_data == "1" && flag_data == "0")
                    {
                        string secretKey = TripleDESImp.Encrypt(login.MobileNo.Trim() + "+" + password, true, key);
                        return Ok(new { success = "Credential Matched", key = secretKey });
                    }
                    else
                    {
                        return NotFound(new { error = "Your account is locked, contact the branch" });
                    }
                }

                return NotFound(new { error = "Invalid Credentials" });
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }


        }
    }
}
