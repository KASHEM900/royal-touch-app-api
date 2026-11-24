using Microsoft.AspNetCore.Authorization;
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
    [Route("api/v1/bini")]
    [ApiController]
    public class BINIBOPreviewController : ControllerBase
    {
        private readonly IConfiguration configuration; 
        private readonly string SqlConn; readonly string totalAmount;
        public BINIBOPreviewController(IConfiguration config)
        {
            configuration = config;
            SqlConn = config.GetConnectionString("RCLWEB");
            totalAmount = configuration["PaymentGateway:TotalAmount"];
        }

        [Authorize]
        [HttpGet("bo-preview")]
        public IActionResult boPreviewAccountHolder([FromQuery] string trackingNo, string mobile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No Data FOund.",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                if (!string.IsNullOrEmpty(trackingNo) && !string.IsNullOrEmpty(mobile))
                {
                    BOOnlineBODetailModel bo = new BOOnlineBODetailModel();
                    bo.TRACKINGNO = trackingNo;

                    BOBankInfoModel bo1 = new BOBankInfoModel();
                    bo1.TrackingNo = trackingNo;


                    BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(configuration);

                    var AccInfo = DB.BINIAccHolderSelect(bo.TRACKINGNO.ToString()).ToList().FirstOrDefault(); 

                    var AccBank = DB.BINIAccBankInfo(bo1.TrackingNo.ToString()).ToList().FirstOrDefault();

                    var AccBranchInfo = DB.BINIAccBranchInfo(bo1.TrackingNo.ToString()).ToList().FirstOrDefault();

                    //var AccAddessInfo = DB.AccAddressInfo(bo1.TrackingNo.ToString()).ToList().FirstOrDefault();

                    //var AccImages = GetImages(trackingNo).ToList().FirstOrDefault();

                    string paymentStatus = GetPaymentStatus(trackingNo);
                    string clientInfo = GetClientInfo(trackingNo);

                    string Mobile = mobile?.Trim();

                    if (!string.Equals(AccInfo.fMobile?.Trim(), Mobile, StringComparison.Ordinal))
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Account Holder Information Not Found!! Please Give Correct Mobile and Tracking No."
                        });
                    }

                    return Ok(new
                    {

                        TrackingNo = trackingNo,
                        ClientCODE = AccInfo.RCODE,
                        amount = totalAmount,
                        boPayment = paymentStatus,
                        accoundHolderAcBranchInfo = AccBranchInfo,
                        accoundHolderInfo = AccInfo,
                        accountHolderBankInfo = AccBank,



                    });

                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tracking Number or Mobile Number not found. Please provide valid information."
                    });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database Error... Account Holder Information Not Found!!!",
                    error = ex.Message
                });
            }
        }
        private List<BOOnlineBODetailModel> GetImages(string trackingNo)
        {
            List<BOOnlineBODetailModel> images = new List<BOOnlineBODetailModel>();

            string query = @"SELECT fImage, fNIDFont, fNIDBack, fSig, jImage, jNIDFont, jNIDBack, jSig FROM T_ONLINE_BO_AccHolderImages
                              WHERE TRACKINGNO = @trackingNo";

            using (SqlConnection con = new SqlConnection(SqlConn))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@trackingNo", SqlDbType.VarChar, 50).Value =
                    trackingNo;

                con.Open();

                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        images.Add(new BOOnlineBODetailModel
                        {
                            fImage = sdr["fImage"] != DBNull.Value ? (byte[])sdr["fImage"] : null,

                            fNIDFont = sdr["fNIDFont"] != DBNull.Value ? (byte[])sdr["fNIDFont"] : null,

                            fNIDBack = sdr["fNIDBack"] != DBNull.Value ? (byte[])sdr["fNIDBack"] : null,

                            fSig = sdr["fSig"] != DBNull.Value ? (byte[])sdr["fSig"] : null,

                            jImage = sdr["jImage"] != DBNull.Value ? (byte[])sdr["jImage"] : null,

                            jNIDFont = sdr["jNIDFont"] != DBNull.Value ? (byte[])sdr["jNIDFont"] : null,

                            jNIDBack = sdr["jNIDBack"] != DBNull.Value ? (byte[])sdr["jNIDBack"] : null,

                            jSig = sdr["jSig"] != DBNull.Value ? (byte[])sdr["jSig"] : null
                        });
                    }
                }
            }

            return images;
        }

        private string GetPaymentStatus(string trackingNo)
        {
            string status = "unpaid";

            using (SqlConnection con = new SqlConnection(SqlConn))
            {
                string query = @"SELECT TOP 1 status FROM [T_ONLINE_PAYMENT_RECEIVER] WHERE tracking_no = @trackingNo  AND status = 'success' ORDER BY create_date DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@trackingNo", trackingNo);

                    con.Open();

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        status = "paid";
                    }
                }
            }

            return status;
        }

        private string GetClientInfo(string trackingNo)
        {
            string status = "unpaid";

            using (SqlConnection con = new SqlConnection(SqlConn))
            {
                string query = @"SELECT TOP 1 status FROM [T_ONLINE_PAYMENT_RECEIVER] WHERE tracking_no = @trackingNo  AND status = 'success' ORDER BY create_date DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@trackingNo", trackingNo);

                    con.Open();

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        status = "paid";
                    }
                }
            }

            return status;
        }

        
    }
}
