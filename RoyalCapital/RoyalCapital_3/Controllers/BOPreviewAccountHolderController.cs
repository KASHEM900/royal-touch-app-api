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
    public class BOPreviewAccountHolderController : ControllerBase
    {
        private readonly IConfiguration configuration; 
        private readonly string SqlConn; readonly string totalAmount;
        public BOPreviewAccountHolderController(IConfiguration config)
        {
            configuration = config;
            SqlConn = config.GetConnectionString("RCLWEB");
            totalAmount = configuration["PaymentGateway:TotalAmount"];
        }


        [HttpGet("boPreviewAccountHolder")]
        public IActionResult boPreviewAccountHolder([FromQuery] string trackingNo)
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

                if (!string.IsNullOrEmpty(trackingNo))
                {
                    BOOnlineBODetailModel bo = new BOOnlineBODetailModel();
                    bo.TRACKINGNO = trackingNo;

                    BOBankInfoModel bo1 = new BOBankInfoModel();
                    bo1.TrackingNo = trackingNo;


                    BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(configuration);

                    var AccInfo = DB.AccHolderSelect(bo.TRACKINGNO.ToString()).ToList().FirstOrDefault(); 

                    var AccBank = DB.AccBankInfo(bo1.TrackingNo.ToString()).ToList().FirstOrDefault();

                    var AccBranchInfo = DB.AccBranchInfo(bo1.TrackingNo.ToString()).ToList().FirstOrDefault();

                    var AccAddessInfo = DB.AccAddressInfo(bo1.TrackingNo.ToString()).ToList().FirstOrDefault();

                    //var AccAu = DB.AccAuInfo(bo.TRACKINGNO.ToString()).ToList();

                    //var AccNo = DB.AccNoInfo(bo.TRACKINGNO.ToString()).ToList();

                    var AccImages = GetImages(trackingNo).ToList().FirstOrDefault();

                    string paymentStatus = GetPaymentStatus(trackingNo);


                    return Ok(new
                    {
                        success = true,
                        accoundHolderInfo = AccInfo,
                        secondAccoundHolderInfo = AccInfo,
                        accountHolderBankInfo = AccBank,
                        accoundHolderAcBranchInfo = AccBranchInfo,
                        accoundHolderAddressInfo = AccAddessInfo,
                        //AccAuzs = AccAu,
                        //AccNoms = AccNo,
                        accountsImages = AccImages,
                        trackingNo = trackingNo,
                        accountType = AccInfo?.ToC,
                        mobile = AccInfo?.fMobile,
                        amount =  totalAmount,
                        boPayment = paymentStatus
                    });

                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =  "NO Tracking NO Found, Account Holder Information Insertion Failed!!!"
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

        //private List<BOOnlineBODetailModel> GetImages(string trackingNo)
        //{
        //    string query = "SELECT * FROM T_ONLINE_BO_AccHolderImages WHERE TRACKINGNO='" + trackingNo.ToString() + "'";
        //    List<BOOnlineBODetailModel> images = new List<BOOnlineBODetailModel>();
        //    using (SqlConnection con = new SqlConnection(SqlConn))
        //    {
        //        using (SqlCommand cmd = new SqlCommand(query))
        //        {
        //            cmd.CommandType = CommandType.Text;
        //            cmd.Connection = con;
        //            con.Open();
        //            using (SqlDataReader sdr = cmd.ExecuteReader())
        //            {
        //                while (sdr.Read())
        //                {
        //                    if (!Convert.IsDBNull(sdr["def"]))
        //                    {
        //                        images.Add(new BOOnlineBODetailModel
        //                        {
        //                            fImage = (sdr["fImage"] != DBNull.Value) ? (byte[])sdr["fImage"] : (byte[])sdr["def"],
        //                            fNIDFont = (sdr["fNIDFont"] != DBNull.Value) ? (byte[])sdr["fNIDFont"] : (byte[])sdr["def"],
        //                            fNIDBack = (sdr["fNIDBack"] != DBNull.Value) ? (byte[])sdr["fNIDBack"] : (byte[])sdr["def"],
        //                            fSig = (sdr["fSig"] != DBNull.Value) ? (byte[])sdr["fSig"] : (byte[])sdr["def"],

        //                            jImage = (sdr["jImage"] != DBNull.Value) ? (byte[])sdr["jImage"] : (byte[])sdr["def"],
        //                            jNIDFont = (sdr["jNIDFont"] != DBNull.Value) ? (byte[])sdr["jNIDFont"] : (byte[])sdr["def"],
        //                            jNIDBack = (sdr["jNIDBack"] != DBNull.Value) ? (byte[])sdr["jNIDBack"] : (byte[])sdr["def"],
        //                            jSig = (sdr["jSig"] != DBNull.Value) ? (byte[])sdr["jSig"] : (byte[])sdr["def"],

        //                            //aImage = (sdr["aImage"] != DBNull.Value) ? (byte[])sdr["aImage"] : (byte[])sdr["def"],
        //                            //aNIDFont = (sdr["aNIDFont"] != DBNull.Value) ? (byte[])sdr["aNIDFont"] : (byte[])sdr["def"],
        //                            //aNIDBack = (sdr["aNIDBack"] != DBNull.Value) ? (byte[])sdr["aNIDBack"] : (byte[])sdr["def"],
        //                            //aSig = (sdr["aSig"] != DBNull.Value) ? (byte[])sdr["aSig"] : (byte[])sdr["def"],

        //                            //n1Image = (sdr["n1Image"] != DBNull.Value) ? (byte[])sdr["n1Image"] : (byte[])sdr["def"],
        //                            //n1NIDFont = (sdr["n1NIDFont"] != DBNull.Value) ? (byte[])sdr["n1NIDFont"] : (byte[])sdr["def"],
        //                            //n1NIDBack = (sdr["n1NIDBack"] != DBNull.Value) ? (byte[])sdr["n1NIDBack"] : (byte[])sdr["def"],
        //                            //n1Sig = (sdr["n1Sig"] != DBNull.Value) ? (byte[])sdr["n1Sig"] : (byte[])sdr["def"],


        //                            //n2Image = (sdr["n2Image"] != DBNull.Value) ? (byte[])sdr["n2Image"] : (byte[])sdr["def"],
        //                            //n2NIDFont = (sdr["n2NIDFont"] != DBNull.Value) ? (byte[])sdr["n2NIDFont"] : (byte[])sdr["def"],
        //                            //n2NIDBack = (sdr["n2NIDBack"] != DBNull.Value) ? (byte[])sdr["n2NIDBack"] : (byte[])sdr["def"],
        //                            //n2Sig = (sdr["n2Sig"] != DBNull.Value) ? (byte[])sdr["n2Sig"] : (byte[])sdr["def"],


        //                            //g1Image = (sdr["g1Image"] != DBNull.Value) ? (byte[])sdr["g1Image"] : (byte[])sdr["def"],
        //                            //g1NIDFont = (sdr["g1NIDFont"] != DBNull.Value) ? (byte[])sdr["g1NIDFont"] : (byte[])sdr["def"],
        //                            //g1NIDBack = (sdr["g1NIDBack"] != DBNull.Value) ? (byte[])sdr["g1NIDBack"] : (byte[])sdr["def"],
        //                            //g1Sig = (sdr["g1Sig"] != DBNull.Value) ? (byte[])sdr["g1Sig"] : (byte[])sdr["def"],

        //                            //g2Image = (sdr["g2Image"] != DBNull.Value) ? (byte[])sdr["g2Image"] : (byte[])sdr["def"],
        //                            //g2NIDFont = (sdr["g2NIDFont"] != DBNull.Value) ? (byte[])sdr["g2NIDFont"] : (byte[])sdr["def"],
        //                            //g2NIDBack = (sdr["g2NIDBack"] != DBNull.Value) ? (byte[])sdr["g2NIDBack"] : (byte[])sdr["def"],
        //                            //g2Sig = (sdr["g2Sig"] != DBNull.Value) ? (byte[])sdr["g2Sig"] : (byte[])sdr["def"]
        //                        });

        //                    }
        //                }
        //            }
        //            con.Close();
        //        }

        //        return images;
        //    }
        //}
    }
}
