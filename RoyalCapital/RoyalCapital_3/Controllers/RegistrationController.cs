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
    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public RegistrationController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpPost("")]
        public IActionResult Register([FromBody] Registration registration)
        {
            string rclConn = configuration.GetConnectionString("RCLWEB");

            // Checks if mobile no exists in T_USER_LOGIN table
            string chk_lgn_tbl_data = check_login_table(registration.MobileNo);
            if (chk_lgn_tbl_data == "true")
            {
                return NotFound(new
                {
                    message = "You are already registered"
                });
            }
            else if (chk_lgn_tbl_data == "false")
            {
                string chk_cdbl_data = check_CDBL(registration.MobileNo);
                if (chk_cdbl_data == "true")
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
                            string query = "INSERT INTO T_USER_LOGIN(mobile,password,lock,flag,ukey) VALUES('" + registration.MobileNo + "', ENCRYPTBYPASSPHRASE('royal','******'), 1, 0, '" + key + "')";

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
                            message = "A 6-digit OTP has been sent to your given/registered mobile number. Apply to get registered.",
                            key = key,
                            otp = snd_sms_data
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
                else if (chk_cdbl_data == "false")
                {
                    return NotFound(new
                    {
                        message = "Your mobile number doesn't match to any BO account. Try another one."
                    });
                }
                else
                {
                    return NotFound(new
                    {
                        message = chk_cdbl_data
                    });
                }
            }
            else
            {
                return NotFound(new
                {
                    message = chk_lgn_tbl_data
                });
            }
        }

        [HttpPost("set-pass")]
        public IActionResult SetPassword([FromBody] Password password)
        {
            string rclConn = configuration.GetConnectionString("RCLWEB");

            string chk_key_data = check_key_exists(password.Key);

            if (chk_key_data == "true")
            {
                try
                {
                    SqlConnection oConnection = new SqlConnection(rclConn);
                    oConnection.Open();
                    string query = "UPDATE T_USER_LOGIN SET password = ENCRYPTBYPASSPHRASE('royal','" + password.Pass + "'), ukey = '',lock=1,flag=0 WHERE ukey = '" + password.Key + "'";

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
                    message = "Registration successful"
                });
            }
            else
            {
                try
                {
                    SqlConnection oConnection = new SqlConnection(rclConn);
                    oConnection.Open();
                    string query = "DELETE FROM RCLWEB.[RCLWEB].[dbo].T_USER_LOGIN WHERE password = ''";

                    SqlCommand oCommand = new SqlCommand(query, oConnection);
                    oCommand.ExecuteNonQuery();
                    oConnection.Close();
                }
                catch (Exception ex)
                {
                    return NotFound(new
                    {
                        message = "Setting password failed! Please try again"
                    });
                }
                return NotFound(new
                {
                    message = "Setting password failed! Please try again"
                });
            }
        }

        public string check_login_table(string mobileNo)
        {
            string rclConn = configuration.GetConnectionString("RCLWEB");

            string val = "false";

            try
            {
                SqlConnection conn_CDBL = new SqlConnection(rclConn);
                string search_qry = "SELECT * FROM RCLWEB.[RCLWEB].[dbo].T_USER_LOGIN WHERE mobile='" + mobileNo.Trim() + "'";
                conn_CDBL.Open();
                SqlCommand comm_search = new SqlCommand(search_qry, conn_CDBL);
                DataTable DT_search = new DataTable();
                DT_search.Load(comm_search.ExecuteReader(CommandBehavior.CloseConnection));
                foreach (DataRow dr in DT_search.Rows)
                {
                    if (dr["mobile"].ToString() == mobileNo.Trim())
                    {
                        val = "true";
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return val;
        }

        public string check_CDBL(string mobileNo)
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

        public string send_SMS_automatically(string mobileNo)
        {
            Random ran_num = new Random();
            string verification_code = ran_num.Next(100000, 999999).ToString();
            string response = "failed";

            string url = "https://sms.royalcapitalbd.com/api/sendSMS?apikey=AMEYO02G7ZZX&mobile=" +
                mobileNo + "&text=" + "RCL Password Set Verification Code : " + verification_code + " \nFor query: 09606555333";

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
            catch(Exception ex)
            {
                response = "failed";
            }            
            return response;
        }
    }
}
