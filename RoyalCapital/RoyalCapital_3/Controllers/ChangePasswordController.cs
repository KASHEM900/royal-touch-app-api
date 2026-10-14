using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
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
    public class ChangePasswordController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public ChangePasswordController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpPost("")]
        public IActionResult ChangePassword([FromBody] Registration registration)
        {
            string rclConn = configuration.GetConnectionString("RCLWEB");

            string snd_sms_data = send_SMS_automatically(registration.MobileNo);

            if (snd_sms_data != "failed")
            {
                Random ran_num = new Random();
                string key = ran_num.Next(100000000, 999999999).ToString();

                try
                {
                    SqlConnection oConnection = new SqlConnection(rclConn);
                    oConnection.Open();
                    string query = "UPDATE T_USER_LOGIN SET ukey = '" + key + "',lock=1,flag=0 WHERE mobile = '" + registration.MobileNo + "'";

                    SqlCommand oCommand = new SqlCommand(query, oConnection);
                    oCommand.ExecuteNonQuery();
                    oConnection.Close();
                }
                catch (Exception ex)
                {
                    return NotFound(new
                    {
                        message = ex.ToString()
                    });
                }

                return Ok(new
                {
                    otp = snd_sms_data,
                    key = key
                });
            }
            else
            {
                return NotFound(new
                {
                    message = "OPT sending failed. Please try again later"
                });
            }
        }


        [HttpPost("change-current-pass")]
        public IActionResult ResetPassword([FromBody] ResetPassword resetPassword)
        {
            string rclConn = configuration.GetConnectionString("RCLWEB");

            string chk_key_data = check_key_exists(resetPassword.ukey);

            if (chk_key_data == "true")
            {
                try
                {
                    SqlConnection oConnection = new SqlConnection(rclConn);
                    oConnection.Open();
                    string query = "UPDATE T_USER_LOGIN SET password = ENCRYPTBYPASSPHRASE('royal','" + resetPassword.Pass + "'), ukey = '',lock=1,flag=0 WHERE ukey = '" + resetPassword.ukey + "'";

                    SqlCommand oCommand = new SqlCommand(query, oConnection);
                    oCommand.ExecuteNonQuery();
                    oConnection.Close();
                }
                catch (Exception ex)
                {
                    return NotFound(new
                    {
                        message = ex.ToString()
                    });
                }

                return Ok(new
                {
                    message = "Password Updated"
                });
            }
            else
            {
                try
                {
                    SqlConnection oConnection = new SqlConnection(rclConn);
                    oConnection.Open();
                    string query = "UPDATE T_USER_LOGIN SET ukey = '' WHERE mobile = '" + resetPassword.MobileNo + "'";

                    SqlCommand oCommand = new SqlCommand(query, oConnection);
                    oCommand.ExecuteNonQuery();
                    oConnection.Close();
                }
                catch (Exception ex)
                {
                    return NotFound(new
                    {
                        message = ex.ToString()
                    });
                }

                return NotFound(new
                {
                    message = "Password Update failed. Try again"
                });
            }
        }

        private string send_SMS_automatically(string mobileNo)
        {
            Random ran_num = new Random();
            string verification_code = ran_num.Next(100000, 999999).ToString();
            string response = "failed";

            string url = "https://sms.royalcapitalbd.com/api/sendSMS?apikey=AMEYO02G7ZZX&mobile=" +
                mobileNo + "&text=" + "RCL Change Password Verification Code : " + verification_code + " \nFor query: 09606555333";

            try
            {
                using (var wb = new WebClient())
                {
                    response = wb.DownloadString(url);
                    if (response == "\"Seccess\"")
                    {
                        response = verification_code;
                    }
                }
            }
            catch (Exception ex)
            {
                response = "failed";
            }
            return response;
        }

        private string check_key_exists(string ukey)
        {
            string rclConn = configuration.GetConnectionString("RCLWEB");
            string status = "false";

            try
            {
                SqlConnection ipo_connection = new SqlConnection(rclConn);
                int count = 0;
                string num_test_qry = "SELECT * FROM RCLWEB.[RCLWEB].[dbo].T_USER_LOGIN WHERE ukey='" + ukey + "' ";
                ipo_connection.Open();
                SqlCommand ipo_command = new SqlCommand(num_test_qry, ipo_connection);
                DataTable ipo_table = new DataTable();
                ipo_table.Load(ipo_command.ExecuteReader(CommandBehavior.CloseConnection));
                foreach (DataRow mobile_num in ipo_table.Rows)
                {
                    count++;
                }
                if (count > 0)
                {
                    status = "true";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return status;
        }
    }
}
