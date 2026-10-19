using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public ForgotPasswordController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpPost("")]
        public IActionResult ForgotPassword([FromBody] Registration registration)
        {
            string rclConn = configuration.GetConnectionString("RCLWEB");

            string chk_mbl_exists_data = check_mobile_exists(registration.MobileNo);

            if (chk_mbl_exists_data == "false")
            {
                return NotFound(new
                {
                    message = "You are not Registered Yet. Please Register Your Account First"
                });
            }
            else
            {
                string snd_sms_data = send_SMS_automatically(registration.MobileNo);

                if (snd_sms_data != "failed")
                {
                    Random ran_num = new Random();
                    string key = ran_num.Next(100000000, 999999999).ToString();

                    try
                    {
                        SqlConnection oConnection = new SqlConnection(rclConn);
                        oConnection.Open();
                        string query = "UPDATE T_USER_LOGIN SET ukey = '" + key + "' WHERE mobile = '" + registration.MobileNo + "'";

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
        }

        [HttpPost("reset-pass")]
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
                    string query = "UPDATE T_USER_LOGIN SET password = ENCRYPTBYPASSPHRASE('royal','" + resetPassword.Pass + "'), ukey = '' WHERE ukey = '" + resetPassword.ukey + "'";

                    SqlCommand oCommand = new SqlCommand(query, oConnection);
                    oCommand.ExecuteNonQuery();
                    oConnection.Close();
                }
                catch (Exception ex)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = ex.ToString()
                    });
                }

                return Ok(new
                {
                    success = true,
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
                        success = false,
                        message = ex.ToString()
                    });
                }

                return NotFound(new
                {
                    success = false,
                    message = "Password Update failed. Try again"
                });
            }
        }

        public string check_mobile_exists(string mobileNo)
        {
            string rclConn = configuration.GetConnectionString("RCLWEB");
            string status = "false";

            try
            {
                SqlConnection ipo_connection = new SqlConnection(rclConn);
                string mobile = mobileNo;
                int count = 0;
                string num_test_qry = "SELECT * FROM RCLWEB.[RCLWEB].[dbo].T_CLIENT1 WHERE mobile='" + mobile + "' ";
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

        public string send_SMS_automatically(string mobileNo)
        {
            Random ran_num = new Random();
            string verification_code = ran_num.Next(100000, 999999).ToString();
            string response = "failed";

            string url = "https://sms.royalcapitalbd.com/api/sendSMS?apikey=AMEYO02G7ZZX&mobile=" +
                mobileNo + "&text=" + "RCL Forgot Password Set Verification Code : " + verification_code + " \nFor query: 09606555333";

            try
            {
                using (var wb = new WebClient())
                {

                    response = wb.DownloadString(url);
                    JToken token = JToken.Parse(response);

                    if (token.Type == JTokenType.String)
                    {
                        token = JToken.Parse(token.Value<string>());
                    }

                    if (token.Type == JTokenType.Object && (int?)token["api_response_code"] == 200 && (string)token["api_response_message"] == "SUCCESS")
                    {
                        response = verification_code;
                    }

                    //response = wb.DownloadString(url);
                    //if (response == "\"Seccess\"")
                    //{
                    //    response = verification_code;
                    //}
                }
            }
            catch (Exception ex)
            {
                response = "failed";
            }
            return response;
        }

        public string check_key_exists(string ukey)
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
