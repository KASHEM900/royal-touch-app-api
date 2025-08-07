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
    [Route("api/[controller]")]
    [ApiController]
    public class BOAccountHolderPaymentController : ControllerBase
    {
        private readonly IConfiguration configuration; 
        private readonly string SqlConn;
        private readonly string storeID;
        private readonly string storePassword;
        readonly string totalAmount;
        string paybleAmount = "0";
        string amount = "0";
        string paid = "unpaid";

        public BOAccountHolderPaymentController(IConfiguration config)
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
                                int discountedAmount = reader["DiscountedAmount"] != DBNull.Value ? Convert.ToInt32(reader["DiscountedAmount"]): 0;

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
                string tran_date = form["tran_date"];
                string cus_name = form["cus_name"];

                if (string.IsNullOrEmpty(amount))
                {
                    amount = "0";
                }

                //HttpContext.Session.SetString("username", "");
                //HttpContext.Session.SetString("trackingno", tracking_no ?? "");


                using (SqlConnection connection = new SqlConnection(SqlConn))
                {
                    connection.Open();

                    if (status == "VALID")
                    {
                        using (SqlCommand cmdUpdate = new SqlCommand("UPDATE T_ONLINE_BO_REG SET IsAgree = 1 WHERE TRACKINGNO = @trackingNo", connection))
                        {
                            cmdUpdate.Parameters.AddWithValue("@trackingNo", tracking_no);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        using (SqlCommand cmdInsert = new SqlCommand(@"INSERT INTO T_ONLINE_PAYMENT_RECEIVER (tracking_no, payment_id, trxID, amount, create_date, status, source) VALUES (@tracking_no, @payment_id, @trxID, @amount, GETDATE(), @status, @source)", connection))
                        {
                            cmdInsert.Parameters.AddWithValue("@tracking_no", tracking_no);
                            cmdInsert.Parameters.AddWithValue("@payment_id", payment_id);
                            cmdInsert.Parameters.AddWithValue("@trxID", trxID);
                            cmdInsert.Parameters.AddWithValue("@amount", Convert.ToDecimal(amount));
                            cmdInsert.Parameters.AddWithValue("@status", "success");
                            cmdInsert.Parameters.AddWithValue("@source", card_type);
                            cmdInsert.ExecuteNonQuery();
                        }

                        connection.Close();

                        return Ok(new
                        {
                            success = true,
                            trackingNo = tracking_no,

                        });

                    }
                    else
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Payment Status Not Valid",
                            trackingNo = tracking_no

                        });
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

        //integration of SSL Commerz payment gateway

        [HttpPost("mycall")]
        public IActionResult mycall([FromQuery] string username, [FromQuery] string MOBILE, [FromQuery] string trackingNo)
        {
           //string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            NameValueCollection PostData = new NameValueCollection
            {
                { "total_amount", totalAmount.ToString()},
                { "currency", "BDT"},
                { "tran_id", GenerateUniqueId() },
                { "success_url", baseUrl + "Client/PaymentStatus" },
                { "fail_url", baseUrl + "Client/PaymentGatewayCallback" },
                { "cancel_url", baseUrl + "Client/PaymentGatewayCallback" },
                { "cus_name", username.ToString() },
                { "cus_email", "bo@royalcapitalbd.com" },
                { "cus_add1", "Address Line On" },
                { "cus_city", "Dhaka" },
                { "cus_postcode", "1219" },
                { "cus_country", "Bangladesh" },
                { "cus_phone",MOBILE.ToString() },
                { "shipping_method", "NO" },
                { "product_name", "ONLINE BO" },
                { "product_category", "Service" },
                { "product_profile", "general" },
                { "value_a",trackingNo.ToString() },
                { "value_b", "BO" },
                { "value_c", GetIp()+ ", Webpage" },
                { "value_d", MOBILE.ToString() }
            };

            var sslcz = new SSLCommerz(storeID, storePassword);
            var response = sslcz.InitiateTransaction(PostData); // postData is Dictionary<string, string>

            if (response.status == "SUCCESS")
            {
                return Ok(new
                {
                    status = "success",
                    gateway_url = response.GatewayPageURL,
                    logo = response.storeLogo
                });
            }
            else
            {
                return BadRequest(new
                {
                    status = "fail",
                    reason = response.status ?? "Transaction failed"
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


        [HttpPost("PaymentGatewayCallback")]
        public IActionResult PaymentGatewayCallback([FromForm] SSLCommerzValidatorResponse response)
        {
            if (!string.IsNullOrEmpty(response.status))
            {
                var status = response.status.ToUpper();

                if (status == "VALID")
                {
                    string qry = @"INSERT INTO [T_ONLINE_PAYMENT_RECEIVER] 
                            ([tracking_no], [payment_id], [trxID], [amount], [create_date], [status], [source])  
                            VALUES (@tracking_no, @payment_id, @trxID, @amount, @create_date, @status, @source)";

                    using (SqlConnection connection = new SqlConnection(SqlConn))
                    using (SqlCommand command = new SqlCommand(qry, connection))
                    {
                        command.Parameters.AddWithValue("@tracking_no", response.value_a ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@payment_id", response.bank_tran_id ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@trxID", response.tran_id ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@amount", response.store_amount ?? "0");
                        command.Parameters.AddWithValue("@create_date", response.tran_date ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@status", "success");
                        command.Parameters.AddWithValue("@source", "SSLCOMMERZ");

                        connection.Open();
                        command.ExecuteNonQuery();
                    }

                    return Ok(new
                    {
                        success = true,
                        message = "Payment Success",
                        tracking_no = response.value_a,
                        transaction_id = response.tran_id,
                        bank_transaction_id = response.bank_tran_id,
                        amount = response.store_amount,
                        date = response.tran_date
                    });
                }
                else if (status == "FAILED")
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Payment Failed",
                        reason = "SSLCommerz reported failure.",
                        tracking_no = response.value_a
                    });
                }
                else if (status == "CANCELLED")
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Payment Cancelled by user",
                        tracking_no = response.value_a
                    });
                }
            }

            return BadRequest(new
            {
                success = false,
                message = "Invalid payment status",
                status = response.status
            });
        }

        private static Dictionary<string, string> GetProperties(object obj)
        {
            var props = new Dictionary<string, string>();
            if (obj == null)
                return props;

            var type = obj.GetType();
            foreach (var prop in type.GetProperties())
            {
                var val = prop.GetValue(obj, new object[] { });

                if (val != null)
                {
                    props.Add(prop.Name, val.ToString());
                }
            }

            return props;
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


        [HttpPost("SendOTP")]
        public async Task<IActionResult> SendOTP(bool isInetrnationalNo, string OTP, string trackingNo)
        {
            OTP = string.Empty;
            try
            {
                string send_whatsUp_sms = "";
                string snd_sms_data = "";
                string mobile = "";
  
                SqlConnection oConnection = new SqlConnection(SqlConn);
                oConnection.Open();
                string query = "SELECT TOP 1 MOBILE FROM T_ONLINE_BO_REG WHERE [TRACKINGNO]='" + trackingNo.ToString() + "'";
                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr in oTable.Rows)
                {
                    mobile = dr["MOBILE"].ToString();
                }

                if (mobile != null)
                {
                    if (isInetrnationalNo)
                    {
                        send_whatsUp_sms = await send_WhatsUp_SMS_automatically(mobile);

                        if (send_whatsUp_sms != "failed")
                        {
                            OTP = send_whatsUp_sms.ToString();

                            return Ok(new
                            {
                                success = true,
                                trackingNo = trackingNo,
                                message = "OTP Succesfully sent your WhatsApp application",
                            });

                        }
                        else
                        {
                            return StatusCode(500,new
                            {
                                success = false,
                                trackingNo = trackingNo,
                                message = "OTP Sent Failed to your WhatsApp application!!! Plese Try Again Later",
                            }); 
                        }

                    }
                    else
                    {
                        snd_sms_data = send_SMS_automatically(mobile);

                        if (snd_sms_data != "failed")
                        {
                            OTP = snd_sms_data.ToString();
                            return Ok(new
                            {
                                success = true,
                                trackingNo = trackingNo,
                                message = "OTP Succesfully sent your WhatsApp application",
                            });
                        }

                        else
                        {
                            return StatusCode(500, new
                            {
                                success = false,
                                trackingNo = trackingNo,
                                message = "OTP Sent Failed to your WhatsApp application!!! Plese Try Again Later",
                            });
                        }
                    }


                }
                else
                {
                    return NotFound(new
                    {
                        success = false,
                        trackingNo = trackingNo,
                        message = "No Mobile Number Found",
                    });
                }


            }
            catch (Exception ex)
            {

                return StatusCode(500, new { error = "Server error", details = ex.Message });
            }
        }


        public string send_SMS_automatically(string mobileNo)
        {
            Random ran_num = new Random();
            string verification_code = ran_num.Next(100000, 999999).ToString();
            string response = "failed";

            string url = "https://sms.royalcapitalbd.com/api/sendSMS?apikey=AMEYO02G7ZZX&mobile=" +
                mobileNo + "&text=" + "Your Mobile Verification Code For Online BO Opening : " + verification_code + " \nFor query: 09606555333";

            try
            {
                using (var wb = new WebClient())
                {
                    response = wb.DownloadString(url);
                    if (response.Contains("SUCCESS"))
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

        public async Task<string> send_WhatsUp_SMS_automatically(string mobileNo)
        {
            Random ran_num = new Random();
            string verification_code = ran_num.Next(100000, 999999).ToString();
            string responseContent = "failed";

            string apiUrl = "https://backend.aisensy.com/campaign/t1/api/v2";

            // Define the request body
            var requestBody = new
            {
                apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjY0YmUyNjVkY2IwYTM0MGJkMzNiYTg1YyIsIm5hbWUiOiJSb3lhbCBDYXBpdGFsIEx0ZC4iLCJhcHBOYW1lIjoiQWlTZW5zeSIsImNsaWVudElkIjoiNjRiZTI2NWRjYjBhMzQwYmQzM2JhODU2IiwiYWN0aXZlUGxhbiI6IkJBU0lDX01PTlRITFkiLCJpYXQiOjE2OTAxODMyNjF9.zSIlck9dwspcMH2dEG1_tOAWNbXcVi24SC-GUU2u-Qc",
                campaignName = "bo_registration_authentication",
                destination = mobileNo,
                userName = "Royal Capital",
                templateParams = new string[] { verification_code },
                buttons = new[]
                {
            new
            {
                type = "button",
                sub_type = "url",
                index = "0",
                parameters = new[]
                {
                    new
                    {
                        type = "text",
                        text = verification_code
                    }
                }
            }
        }
            };
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            string jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Send the POST request with the JSON body, setting Content-Type via StringContent
                    var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        responseContent = await response.Content.ReadAsStringAsync();
                        responseContent = verification_code;

                        Console.WriteLine("Response: " + responseContent);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Request failed: " + ex.Message);
                }
            }

            return responseContent;
        }



        // End of SSL Commerz payment gateway
    }
}
