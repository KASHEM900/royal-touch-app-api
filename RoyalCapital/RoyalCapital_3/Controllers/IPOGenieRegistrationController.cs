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
using RoyalCapital_3.BL;
namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IPOGenieRegistrationController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public IPOGenieRegistrationController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("check-status")]
        public IActionResult CheckStatus(string aCode, string key)
        {
            string decryptedData;
            int masterFlag = 0;

            string secretKey = configuration["secret"];
            try
            {
                decryptedData = TripleDESImp.Decrypt(key.Trim(), true, secretKey);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = "invalid user" });
            }
            string[] subs = decryptedData.Split('+');



            string sisConn = configuration.GetConnectionString("SISROYALU");
            string rclConn = configuration.GetConnectionString("RCLWEB");

            if (!SecretKeyChecker.SectetKeyChecker(rclConn, subs[0], subs[1]))
            {
                return NotFound(new { error = "invalid user" });
            }

            SqlConnection oConnection = new SqlConnection(rclConn);

            try
            {
                oConnection.Open();

                string querySrcMaster = "SELECT RCODE FROM RCLWEB.[RCLWEB].[dbo].[T_IPO_REPEAT_CODE] WHERE RCODE='" + aCode.ToString() + "'";
                SqlCommand oCommand = new SqlCommand(querySrcMaster, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader());

                foreach (DataRow dr in oTable.Rows)
                {
                    if (!DBNull.Value.Equals(dr["RCODE"].ToString()))
                    {
                        masterFlag = 1;
                    }

                }
                oConnection.Close();
                if (masterFlag == 0)
                {
                    return Ok(new
                    {
                        active_status = false,
                    });
                }
                else
                {
                    return Ok(new
                    {
                        active_status = true,
                    });
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }


        [HttpPost("")]
        public IActionResult ApplyIPOGenie(string aCode, string mobileNo, string key)
        {
            string decryptedData;
            int masterFlag = 0;

            string secretKey = configuration["secret"];
            try
            {
                decryptedData = TripleDESImp.Decrypt(key.Trim(), true, secretKey);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = "invalid user" });
            }
            string[] subs = decryptedData.Split('+');



            string sisConn = configuration.GetConnectionString("SISROYALU");
            string rclConn = configuration.GetConnectionString("RCLWEB");

            if (!SecretKeyChecker.SectetKeyChecker(rclConn, subs[0], subs[1]))
            {
                return NotFound(new { error = "invalid user" });
            }

            SqlConnection oConnection = new SqlConnection(rclConn);

            try
            {
                ////generating tracking number
                string get_max_transection_id = "INSERT INTO T_IPO_TRANSECTION_GENERATOR_R (note,date_time) VALUES('unused', getdate()); SELECT SCOPE_IDENTITY() AS max_id";
                //SqlConnection oConnection = new SqlConnection(sql_connection.connRCLWEB);
                oConnection.Open();
                SqlCommand comm_max = new SqlCommand(get_max_transection_id, oConnection);
                DataTable dt_max = new DataTable();
                dt_max.Load(comm_max.ExecuteReader());
                string tracking_no = "0";
                foreach (DataRow dr1 in dt_max.Rows)
                {
                    tracking_no = dr1["max_id"].ToString();
                }

                string querySrcMaster = "SELECT MCODE FROM RCLWEB.[RCLWEB].[dbo].[T_IPO_MASTER_CODE] WHERE RCODE='" + aCode.ToString() + "'";
                SqlCommand oCommand = new SqlCommand(querySrcMaster, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader());

                foreach (DataRow dr in oTable.Rows)
                {
                    if (!DBNull.Value.Equals(dr["MCODE"]))
                    {
                        SqlCommand oCommand1 = new SqlCommand("INSERT INTO [T_IPO_REPEAT_CODE]([TrackingNO],[MCODE],[RCODE],[edat],[isActive],[mobile],[BRANCHCODE]) VALUES('" + tracking_no + "','" + dr["MCODE"].ToString() + "','" + aCode.ToString() + "',GETDATE(),1,'" + mobileNo.ToString() + "','WEB')", oConnection);
                        oCommand1.ExecuteNonQuery();

                        masterFlag = 1;
                    }
                }
                if (masterFlag == 0)
                {
                    SqlCommand oCommand2 = new SqlCommand("INSERT INTO T_IPO_REPEAT_CODE([TrackingNO],[MCODE],[RCODE],[edat],[isActive],[mobile],[BRANCHCODE]) VALUES('" + tracking_no + "','0','" + aCode.ToString() + "',GETDATE(),1,'" + mobileNo.ToString() + "','WEB')", oConnection);
                    oCommand2.ExecuteNonQuery();
                    masterFlag = 0;
                }

                ////updating transaction number
                get_max_transection_id = "UPDATE T_IPO_TRANSECTION_GENERATOR_R SET note=CONVERT(VARCHAR,getDate(),109) WHERE tr_no='" + tracking_no + "'";
                comm_max = new SqlCommand(get_max_transection_id, oConnection);
                comm_max.ExecuteNonQuery();
                //end of updating transaction number		

                oConnection.Close();

                return Ok(new
                {
                    message = "Congratulations, IPO Genie is Successfully Activated.",
                });
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
