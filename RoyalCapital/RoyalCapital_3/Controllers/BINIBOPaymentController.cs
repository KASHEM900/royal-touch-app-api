using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoyalCapital_3.BL;
using RoyalCapital_3.Models;
using RoyalCapitalBE.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static RoyalCapital_3.SSLCommerz;
using static System.Net.Mime.MediaTypeNames;

namespace RoyalCapital_3.Controllers
{
    [Route("api/v1/bini")]
    [ApiController]
    public class BINIBOPaymentController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly string SqlConn;
        private readonly string storeID;
        private readonly string storePassword;
        readonly string totalAmount;
        string paybleAmount = "0";
        string amount = "0";
        string paid = "unpaid";

        public BINIBOPaymentController(IConfiguration config)
        {
            configuration = config;
            SqlConn = config.GetConnectionString("RCLWEB");
            storeID = configuration["PaymentGateway:StoreID"];
            storePassword = configuration["PaymentGateway:StorePassword"];
            totalAmount = configuration["PaymentGateway:TotalAmount"];
        }


        [HttpGet("boAccountHolderPayment")]
        public IActionResult BoAccountHolderPayment([FromQuery] string trackingNo)
        {
            if (string.IsNullOrEmpty(trackingNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No Tracking No Found. Account Holder Payment Information !!!"
                });
            }

            try
            {

                using (SqlConnection oConnection = new SqlConnection(SqlConn))
                {
                    oConnection.Open();

                    string query = @"SELECT a.tracking_no, a.status, ISNULL(b.DiscountedAmount, 0) AS DiscountedAmount FROM T_ONLINE_PAYMENT_RECEIVER a inner join 
                         T_ONLINE_BO_REG c on a.tracking_no = c.TRACKINGNO LEFT JOIN T_ONLINE_BO_PROMO b ON c.PromoCode = b.PromoCode WHERE a.tracking_no =@trackingNo";

                    using (SqlCommand command = new SqlCommand(query, oConnection))
                    {
                        command.Parameters.AddWithValue("@trackingNo", trackingNo);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var status = reader["status"]?.ToString();
                                int discountedAmount = reader["DiscountedAmount"] != DBNull.Value ? Convert.ToInt32(reader["DiscountedAmount"]) : 0;

                                if (status == "success")
                                {
                                    paid = "paid";
                                }
                                else
                                {
                                    paybleAmount = (Convert.ToInt32(totalAmount) - discountedAmount).ToString();
                                    amount = paybleAmount;
                                }

                                return Ok(new
                                {
                                    success = true,
                                    trackingNo = trackingNo,
                                    paid = paid,
                                    paybleAmount = paybleAmount,
                                    amount = amount
                                });
                            }
                            else
                            {
                                int discountedAmount = 0;
                                paybleAmount = (Convert.ToInt32(totalAmount) - discountedAmount).ToString();
                                amount = paybleAmount;

                                return Ok(new
                                {
                                    success = true,
                                    trackingNo = trackingNo,
                                    paid = paid,
                                    paybleAmount = paybleAmount,
                                    amount = amount
                                });
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database error. Account holder payment information not found!",
                    error = ex.Message
                });
            }
        }

        [HttpGet("BoPaymentStatus")]
        public IActionResult BoPaymentStatus([FromQuery] string trackingNo)
        {
            if (string.IsNullOrEmpty(trackingNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No Tracking No Found. Account Holder Payment Information !!!"
                });
            }

            try
            {

                using (SqlConnection oConnection = new SqlConnection(SqlConn))
                {
                    oConnection.Open();

                    string query = @"SELECT* FROM[T_ONLINE_PAYMENT_RECEIVER] WHERE[tracking_no] = @trackingNo";

                    using (SqlCommand command = new SqlCommand(query, oConnection))
                    {
                        command.Parameters.AddWithValue("@trackingNo", trackingNo);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var payment_id = reader["payment_id"].ToString();
                                var trxID = reader["trxID"].ToString();
                                var amount = reader["amount"].ToString();
                                var create_date = reader["create_date"].ToString();
                                var status = reader["status"].ToString();
                                var isApproved = reader["isApproved"].ToString();

                                return Ok(new
                                {
                                    success = true,
                                    payment_id = payment_id,
                                    trxID = trxID,
                                    create_date = create_date,
                                    status = status,
                                    isApproved = isApproved
                                });
                            }
                            else
                            {

                                return Ok(new
                                {
                                    success = false,
                                    message = "No Paymennt Found!!",
                                });
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database error. Payment not found!",
                    error = ex.Message
                });
            }
        }

        

        //integration of SSL Commerz payment gateway

        [HttpPost("bo-payment")]
        public IActionResult biniSSLPayemntCall([FromQuery] string mobile, [FromQuery] string trackingNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mobile) || string.IsNullOrWhiteSpace(trackingNo))
                {
                    return BadRequest(new
                    {
                        status = false,
                        message = "Mobile number or Tracking Number not found."
                    });
                }

                // Check whether payment already completed
                using (SqlConnection con = new SqlConnection(SqlConn))
                {
                    con.Open();

                    string checkQuery = @"SELECT TOP 1 status, trxID, amount FROM T_ONLINE_PAYMENT_RECEIVER  WHERE tracking_no = @trackingNo
                                          AND LOWER(LTRIM(RTRIM(status))) = 'success' ORDER BY create_date DESC";

                    using (SqlCommand cmd = new SqlCommand(checkQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@trackingNo", trackingNo);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new
                                {
                                    status = true,
                                    alreadyPaid = true,
                                    message = "Payment has already been completed for this Tracking Number.",
                                    trackingNo = trackingNo,
                                    transactionId = reader["trxID"]?.ToString(),
                                    amount = reader["amount"]?.ToString()
                                });
                            }
                        }
                    }
                }

                // Payment not completed - initiate new payment
                string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                NameValueCollection PostData = new NameValueCollection
                {
                    { "total_amount", totalAmount.ToString() },
                    { "currency", "BDT" },
                    { "tran_id", GenerateUniqueId() },

                    { "success_url", baseUrl.TrimEnd('/') + "/api/v1/bini/PaymentStatus" },
                    { "fail_url", baseUrl.TrimEnd('/') + "/api/v1/bini/PaymentGatewayCallback" },
                    { "cancel_url", baseUrl.TrimEnd('/') + "/api/v1/bini/PaymentGatewayCallback" },

                    { "cus_name", "BINI" },
                    { "cus_email", "bo@royalcapitalbd.com" },
                    { "cus_add1", "Address Line On" },
                    { "cus_city", "Dhaka" },
                    { "cus_postcode", "1219" },
                    { "cus_country", "Bangladesh" },
                    { "cus_phone", mobile },

                    { "shipping_method", "NO" },
                    { "product_name", "ONLINE BO" },
                    { "product_category", "Service" },
                    { "product_profile", "general" },

                    { "value_a", trackingNo },
                    { "value_b", "BO" },
                    { "value_c", GetIp() + ", Webpage" },
                    { "value_d", mobile }
                };

                var sslcz = new SSLCommerz(storeID, storePassword);
                var response = sslcz.InitiateTransaction(PostData);

                if (response.status == "SUCCESS")
                {
                    return Ok(new
                    {
                        status = true,
                        alreadyPaid = false,
                        message = "Payment session created successfully. Please use the payment URL to complete your BO registration payment.",
                        paymentUrl = response.GatewayPageURL
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        status = false,
                        message = "Payment session creation failed.",
                        reason = response.status ?? "Transaction failed"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while processing payment.",
                    error = ex.Message
                });
            }
        }
        private string GenerateUniqueId()
        {
            long i = 1;

            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= (b + 1);
            }

            return string.Format("{0:x}", i - DateTime.Now.Ticks).ToUpper();
        }


        

        [HttpPost("PaymentStatus")]
        public IActionResult PaymentStatus()
        {
            try
            {
                var form = Request.Form;

                string status = form["status"];
                string tracking_no = form["value_a"];
                string payment_id = form["bank_tran_id"];
                string amount = form["amount"];
                string trxID = form["tran_id"];
                string card_type = form["card_type"];

                if (status != "VALID")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Payment Status Not Valid",
                        trackingNo = tracking_no
                    });
                }

                if (string.IsNullOrWhiteSpace(tracking_no) || string.IsNullOrWhiteSpace(trxID) || string.IsNullOrWhiteSpace(amount))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid payment information."
                    });
                }

                if (!decimal.TryParse(amount, out decimal paymentAmount) ||
                    paymentAmount <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid payment amount."
                    });
                }

                using (SqlConnection connection = new SqlConnection(SqlConn))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Prevent duplicate payment insert
                            string checkQuery = @"SELECT COUNT(1) FROM T_ONLINE_PAYMENT_RECEIVER WHERE trxID = @trxID OR 
                                                (tracking_no = @trackingNo AND status = 'success')";

                            using (SqlCommand cmdCheck = new SqlCommand(checkQuery, connection, transaction))
                            {
                                cmdCheck.Parameters.AddWithValue("@trxID", trxID);
                                cmdCheck.Parameters.AddWithValue("@trackingNo", tracking_no);

                                int exists = Convert.ToInt32(cmdCheck.ExecuteScalar());

                                if (exists > 0)
                                {
                                    transaction.Rollback();

                                    return Ok(new
                                    {
                                        success = true,
                                        alreadyPaid = true,
                                        message = "Payment already recorded.",
                                        trackingNo = tracking_no
                                    });
                                }
                            }

                            // Update BO registration
                            using (SqlCommand cmdUpdate = new SqlCommand( @"UPDATE T_ONLINE_BO_REG SET IsAgree = 1 WHERE TRACKINGNO = @trackingNo",
                                connection, transaction))
                            {
                                cmdUpdate.Parameters.AddWithValue("@trackingNo", tracking_no);

                                int updated = cmdUpdate.ExecuteNonQuery();

                                if (updated == 0)
                                {
                                    transaction.Rollback();

                                    return NotFound(new
                                    {
                                        success = false,
                                        message = "BO Registration not found.",
                                        trackingNo = tracking_no
                                    });
                                }
                            }

                            // Insert successful payment
                            string insertQuery = @" INSERT INTO T_ONLINE_PAYMENT_RECEIVER (tracking_no, payment_id, trxID, amount, create_date, status, source) VALUES (@tracking_no, @payment_id, @trxID, @amount, GETDATE(), 'success', @source)";

                            using (SqlCommand cmdInsert = new SqlCommand(insertQuery, connection, transaction))
                            {
                                cmdInsert.Parameters.AddWithValue("@tracking_no", tracking_no);
                                cmdInsert.Parameters.AddWithValue("@payment_id", payment_id);
                                cmdInsert.Parameters.AddWithValue("@trxID", trxID);
                                cmdInsert.Parameters.AddWithValue("@amount", paymentAmount);
                                cmdInsert.Parameters.AddWithValue("@source", card_type);

                                cmdInsert.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            return Ok(new
                            {
                                success = true,
                                alreadyPaid = false,
                                message = "Payment Successfully Completed.",
                                trackingNo = tracking_no,
                                transactionId = trxID,
                                amount = paymentAmount
                            });
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Payment processing failed.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("PaymentGatewayCallback")]
        public IActionResult PaymentGatewayCallback([FromForm] SSLCommerzValidatorResponse response)
        {
            if (response == null || string.IsNullOrWhiteSpace(response.status))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid payment response."
                });
            }

            string status = response.status.ToUpper();

            if (status == "FAILED")
            {
                return Ok(new
                {
                    success = false,
                    message = "Payment Failed.",
                    trackingNo = response.value_a,
                    transactionId = response.tran_id
                });
            }

            if (status == "CANCELLED")
            {
                return Ok(new
                {
                    success = false,
                    message = "Payment Cancelled by User.",
                    trackingNo = response.value_a,
                    transactionId = response.tran_id
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "Invalid payment status.",
                status = status
            });
        }

        private string GetIp()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            return ip ?? "Unknown";
        }

        // End of SSL Commerz payment gateway


    }
}
