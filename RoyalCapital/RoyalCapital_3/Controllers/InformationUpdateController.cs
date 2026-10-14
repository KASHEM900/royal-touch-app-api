using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RoyalCapital_3.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InformationUpdateController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly string SqlConn;
        private readonly IMemoryCache memoryCache;

        public InformationUpdateController(IConfiguration config, IMemoryCache memoryCache)
        {
            configuration = config;
            SqlConn = config.GetConnectionString("RCLWEB");
            this.memoryCache = memoryCache;
        }
     
        // COMMON OTP CACHE

        private void SaveOTP(string clientCode, string type, string otp)
        {
            string key = GetOTPKey(clientCode, type);
            memoryCache.Set(key,otp,TimeSpan.FromMinutes(5));
        }

        private bool VerifyOTP(string clientCode, string type, string otp)
        {
            string key = GetOTPKey(clientCode, type);

            if (!memoryCache.TryGetValue(key, out string savedOTP))
                return false;

            return savedOTP == otp;
        }

        private void RemoveOTP(string clientCode, string type)
        {
            memoryCache.Remove(GetOTPKey(clientCode, type));
        }

        private string GetOTPKey(string clientCode, string type)
        {
            return $"INFO_UPDATE_OTP_{type}_{clientCode}";
        }

        private string GetRequestKey(string clientCode, string type)
        {
            return $"INFO_UPDATE_REQUEST_{type}_{clientCode}";
        }

        // COMMON OTP GENERATE


        private string GenerateOTP()
        {
            Random random = new Random();

            return random.Next(100000, 999999).ToString();
        }

        // COMMON CLIENT CONTACT

        private bool GetClientContact(string clientCode,out string mobile, out string email)
        {
            mobile = "";
            email = "";

            string query = @"SELECT mobile, email FROM T_CLIENT1 WHERE RCODE = @ClientCode";

            using (SqlConnection connection = new SqlConnection(SqlConn))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = 800;
                    command.Parameters.AddWithValue("@ClientCode", clientCode ?? "");

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return false;

                        mobile = reader["mobile"] == DBNull.Value ? "" : reader["mobile"].ToString();

                        email = reader["email"] == DBNull.Value ? "" : reader["email"].ToString();

                        return true;
                    }
                }
            }
        }

        // COMMON SEND SMS

        private void SendSMS(string mobile, string message)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return;

            using (SqlConnection connection = new SqlConnection(SqlConn))
            {
                connection.Open();
                string query = @" EXEC sp_send_SMS_common @Mobile, @Message";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = 800;
                    command.Parameters.AddWithValue( "@Mobile", mobile.Trim());
                    command.Parameters.AddWithValue("@Message", message);
                    command.ExecuteNonQuery();
                }
            }
        }

        // COMMON SEND EMAIL

        private void SendEmail(string email, string displayName, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(email))
                return;

            using (MailMessage mail = new MailMessage())
            using (SmtpClient smtp = new SmtpClient("159.21.2.246"))
            {
                mail.From = new MailAddress("bo@royalcapitalbd.com", displayName);

                mail.To.Add(email);

                mail.Subject = subject;

                mail.Body = body;

                mail.IsBodyHtml = false;

                smtp.Port = 25;

                smtp.Credentials = new NetworkCredential("bo@royalcapitalbd.com", configuration["EmailSettings:Password"]);

                smtp.Send(mail);
            }
        }

        // COMMON OTP SEND

        private void SendOTPMessage(string mobile, string email, string otp, string updateType)
        {
            string smsContent = "You are requesting for change " + updateType + ". For that use RCL Verification Code : " + otp + " \nFor query: +8809606016379";

            string emailBody = "Dear Valued Client,\n\n" + "You are requesting for change " + updateType + ".\n\n"
                + "For that use RCL Verification Code : " + otp + "\n\n" + "For query: +8809606016379";

            if (!string.IsNullOrWhiteSpace(mobile))
            {
                SendSMS( mobile, smsContent);
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                SendEmail(email,"Royal Capital Online Information Update Notification","Royal Capital Online Information Update Notification",emailBody);
            }
        }

        // COMMON FILE VALIDATION

        private async Task<byte[]> GetFileBytes( IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            string extension =  Path.GetExtension(file.FileName) .ToLower();

            if (extension != ".jpg" && extension != ".jpeg")
            {
                throw new Exception( "Only JPG/JPEG file allowed!");
            }

            if (file.Length > 300 * 1024)
            {
                throw new Exception( "Image size must be within 300KB!");
            }

            using (MemoryStream stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                return stream.ToArray();
            }
        }

        // COMMON DATABASE ERROR

        private IActionResult DatabaseError(Exception ex)
        {
            return StatusCode(500,new {success = false, message = "Database Error... Failed to update information.",
                    error = ex.Message
                });
        }

        // ADDRESS

        [HttpPost("accountHolderAddressGetOTP")]
        public async Task<IActionResult> accountHolderAddressGetOTP([FromForm] AccountHolderAddressRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace( model.ClientCode))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Client Code is required."
                    });
                }

                if (model.Image == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Image is required."
                    });
                }

                if (HasPendingRequest("T_Client_Info_Update_Address", model.ClientCode,true))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message ="You have already one Address change request. For query: +8809606016379."
                    });
                }

                byte[] image = await GetFileBytes(model.Image);

                if (!GetClientContact( model.ClientCode, out string mobile, out string email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Client information not found."
                    });
                }

                string otp = GenerateOTP();

                var request = new AddressOTPRequest
                {
                    ClientCode = model.ClientCode,
                    Country = model.Country,
                    State = model.State,
                    City = model.City,
                    ZipCode = model.ZipCode,
                    Address = model.Address,
                    Image = image,
                    ImageFileName = model.Image.FileName
                };

                memoryCache.Set(GetRequestKey( model.ClientCode, "ADDRESS"), request, TimeSpan.FromMinutes(5));

                SaveOTP( model.ClientCode,"ADDRESS",otp);

                SendOTPMessage(mobile,email, otp, "Address");

                return Ok(new
                {
                    success = true,
                    message ="OTP sent successfully by SMS and Email."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }


        [HttpPost("accountHolderAddressVerifyOTP")]
        public IActionResult accountHolderAddressVerifyOTP( [FromBody] OTPVerifyModel model)
        {
            try
            {
                if (!VerifyOTP(model.ClientCode,"ADDRESS", model.OTP))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid or expired OTP."
                    });
                }

                string key = GetRequestKey( model.ClientCode, "ADDRESS");

                if (!memoryCache.TryGetValue(key,out AddressOTPRequest request))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Request expired. Please request OTP again."
                    });
                }

                string query = @"INSERT INTO T_Client_Info_Update_Address(ClientCode, Country, State, City, ZipCode,
                                 Address, Image, ImgPath) VALUES(@ClientCode, @Country, @State, @City,@ZipCode,@Address, @Image, @ImgPath)";

                string imgPath =  "/User/infoUpdateImages/Address/" + model.ClientCode;

                SqlParameter[] parameters =
                {
                    new SqlParameter("@ClientCode",request.ClientCode),

                    new SqlParameter("@Country", request.Country ?? ""),

                    new SqlParameter("@State", request.State ?? ""),

                    new SqlParameter("@City", request.City ?? ""),

                    new SqlParameter("@ZipCode", request.ZipCode ?? ""),

                    new SqlParameter("@Address", request.Address ?? ""),

                    new SqlParameter("@Image", SqlDbType.Image)
                    {
                        Value = (object)request.Image?? DBNull.Value
                    },

                    new SqlParameter("@ImgPath",imgPath)
                };

                if (!ExecuteInsert(query, parameters))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Failed to submit Address change request."
                    });
                }

                RemoveOTP( model.ClientCode, "ADDRESS");

                memoryCache.Remove(key);

                return Ok(new
                {
                    success = true,
                    message = "Address change request successfully submitted."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }

        // BANK

        [HttpPost("accountHolderBankGetOTP")]
        public async Task<IActionResult> accountHolderBankGetOTP( [FromForm] AccountHolderBankRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace( model.AccountNumber) || model.AccountNumber.Length < 13)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Bank Account Must be 13 Digit."
                    });
                }

                if (string.IsNullOrWhiteSpace( model.RoutingNumber))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Routing Number is required."
                    });
                }

                if (model.Image == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Image is required."
                    });
                }

                if (HasPendingRequest( "T_Client_Info_Update_Bank", model.ClientCode, true))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message ="You have already one Bank change request. For query: +8809606016379."
                    });
                }

                if (!BankBranchExists( model.BankName, model.District, model.BranchName, model.RoutingNumber))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid Bank, District, Branch or Routing Number."
                    });
                }

                byte[] image = await GetFileBytes(model.Image);

                if (!GetClientContact( model.ClientCode, out string mobile, out string email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Client information not found."
                    });
                }

                string otp = GenerateOTP();

                var request = new BankOTPRequest
                    {
                        ClientCode = model.ClientCode,

                        BankName = model.BankName,

                        District = model.District,

                        BranchName = model.BranchName,

                        AccountNumber = model.AccountNumber,

                        RoutingNumber = model.RoutingNumber,

                        Image = image
                    };

                memoryCache.Set(GetRequestKey( model.ClientCode, "BANK"), request,TimeSpan.FromMinutes(5));

                SaveOTP( model.ClientCode, "BANK", otp);

                SendOTPMessage(mobile, email, otp, "Bank Information");

                return Ok(new
                {
                    success = true,
                    message = "OTP sent successfully by SMS and Email."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }


        [HttpPost("accountHolderBankVerifyOTP")]
        public IActionResult accountHolderBankVerifyOTP([FromBody] OTPVerifyModel model)
        {
            try
            {
                if (!VerifyOTP( model.ClientCode,"BANK",model.OTP))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid or expired OTP."
                    });
                }

                string key = GetRequestKey( model.ClientCode, "BANK");

                if (!memoryCache.TryGetValue( key, out BankOTPRequest request))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Request expired. Please request OTP again."
                    });
                }

                string query = @"INSERT INTO T_Client_Info_Update_Bank ( ClientCode, BankName, District, BranchName,
                                AcNumber, RoutingNumber, Image, ImgPath) VALUES (@ClientCode, @BankName, @District,
                                @BranchName, @AcNumber, @RoutingNumber, @Image, @ImgPath)";

                string imgPath = "/User/infoUpdateImages/Bank/" + model.ClientCode;

                SqlParameter[] parameters =
                {
                    new SqlParameter("@ClientCode", request.ClientCode),

                    new SqlParameter("@BankName", request.BankName ?? ""),

                    new SqlParameter("@District", request.District ?? ""),

                    new SqlParameter("@BranchName", request.BranchName ?? ""),

                    new SqlParameter("@AcNumber", request.AccountNumber ?? ""),

                    new SqlParameter("@RoutingNumber", request.RoutingNumber ?? ""),

                    new SqlParameter("@Image",SqlDbType.Image)
                    {
                        Value =  (object)request.Image ?? DBNull.Value
                    },

                    new SqlParameter("@ImgPath", imgPath)
                };

                if (!ExecuteInsert( query, parameters))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to submit Bank change request."
                    });
                }

                RemoveOTP( model.ClientCode,"BANK");

                memoryCache.Remove(key);

                return Ok(new
                {
                    success = true,
                    message = "Bank change request successfully submitted."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }


        // EMAIL

        [HttpPost("accountHolderEmailGetOTP")]
        public IActionResult accountHolderEmailGetOTP([FromBody] AccountHolderEmailModel model)
        {
            try
            {
                if (model.NewEmail != model.ConfirmEmail)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "E-Mail Address And Confirm E-Mail Address Must be Same."
                    });
                }

                if (HasPendingRequest("T_Client_Info_Update_Email", model.ClientCode, false))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "You have already one Email change request. For query: +8809606016379."
                    });
                }

                if (!GetClientContact(model.ClientCode, out string mobile, out string email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Client information not found."
                    });
                }

                string otp = GenerateOTP();

                memoryCache.Set( GetRequestKey(model.ClientCode,"EMAIL"), model, TimeSpan.FromMinutes(5));

                SaveOTP(model.ClientCode, "EMAIL", otp);

                SendOTPMessage( mobile, email, otp, "Email");

                return Ok(new
                {
                    success = true,
                    message = "OTP sent successfully by SMS and Email."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }


        [HttpPost("accountHolderEmailVerifyOTP")]
        public IActionResult accountHolderEmailVerifyOTP( [FromBody] OTPVerifyModel model)
        {
            try
            {
                if (!VerifyOTP( model.ClientCode, "EMAIL", model.OTP))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid or expired OTP."
                    });
                }

                string key = GetRequestKey( model.ClientCode, "EMAIL");

                if (!memoryCache.TryGetValue(key, out AccountHolderEmailModel request))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Request expired. Please request OTP again."
                    });
                }

                string query = @"INSERT INTO T_Client_Info_Update_Email (ClientCode, OldEmail, NewEmail) VALUES (       
                               @ClientCode, @OldEmail, @NewEmail )";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@ClientCode", request.ClientCode),

                    new SqlParameter("@OldEmail", request.OldEmail ?? ""),

                    new SqlParameter("@NewEmail", request.NewEmail ?? "")
                };

                if (!ExecuteInsert(query, parameters))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to submit Email change request."
                    });
                }

                RemoveOTP(model.ClientCode, "EMAIL");

                memoryCache.Remove(key);

                return Ok(new
                {
                    success = true,
                    message = "Email change request successfully submitted."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }


        // E-TIN


        [HttpPost("accountHolderETINGetOTP")]
        public async Task<IActionResult> accountHolderETINGetOTP( [FromForm] AccountHolderETINRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace( model.NewTIN) || model.NewTIN.Length != 12)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "E-TIN Must be 12 Digit."
                    });
                }

                if (model.NewTIN !=model.ConfirmTIN)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "E-TIN And Confirm E-TIN Must be Same."
                    });
                }

                if (model.Image == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Image is required."
                    });
                }

                if (HasPendingRequest("T_Client_Info_Update_TIN", model.ClientCode, true))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "You have already one TIN change request. For query: +8809606016379."
                    });
                }

                byte[] image = await GetFileBytes(model.Image);

                if (!GetClientContact( model.ClientCode, out string mobile, out string email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Client information not found."
                    });
                }

                string otp = GenerateOTP();

                var request = new ETINOTPRequest
                {
                        ClientCode = model.ClientCode,
                        NewTIN = model.NewTIN,
                        ConfirmTIN = model.ConfirmTIN,
                        Image = image
                    };

                memoryCache.Set( GetRequestKey(model.ClientCode,"ETIN"), request, TimeSpan.FromMinutes(5));

                SaveOTP(model.ClientCode,"ETIN", otp);

                SendOTPMessage(mobile, email, otp, "E-TIN");

                return Ok(new
                {
                    success = true,
                    message = "OTP sent successfully by SMS and Email."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }


        [HttpPost("accountHolderETINVerifyOTP")]
        public IActionResult accountHolderETINVerifyOTP( [FromBody] OTPVerifyModel model)
        {
            try
            {
                if (!VerifyOTP( model.ClientCode, "ETIN", model.OTP))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid or expired OTP."
                    });
                }

                string key = GetRequestKey(model.ClientCode, "ETIN");

                if (!memoryCache.TryGetValue( key, out ETINOTPRequest request))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Request expired. Please request OTP again."
                    });
                }

                string query = @"INSERT INTO T_Client_Info_Update_TIN (ClientCode, TINNo, Image, ImgPath)
                    VALUES (@ClientCode, @TINNo, @Image, @ImgPath)";

                string imgPath = "/User/infoUpdateImages/TIN/" + model.ClientCode;

                SqlParameter[] parameters =
                {
                    new SqlParameter("@ClientCode", request.ClientCode),

                    new SqlParameter("@TINNo", request.NewTIN),

                    new SqlParameter("@Image", SqlDbType.Image)
                    {
                        Value = (object)request.Image ?? DBNull.Value
                    },

                    new SqlParameter("@ImgPath", imgPath)
                };

                if (!ExecuteInsert( query, parameters))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to submit E-TIN change request."
                    });
                }

                RemoveOTP( model.ClientCode, "ETIN");

                memoryCache.Remove(key);

                return Ok(new
                {
                    success = true,
                    message = "E-TIN change request successfully submitted."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }



        // MOBILE


        [HttpPost("accountHolderMobileGetOTP")]
        public IActionResult accountHolderMobileGetOTP( [FromBody] AccountHolderMobileModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace( model.NewMobile) || model.NewMobile.Length != 11)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mobile Number Must be 11 Digit."
                    });
                }

                if (model.NewMobile != model.ConfirmMobile)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mobile Number And Confirm Mobile Number Must be Same."
                    });
                }

                if (HasPendingRequest("T_Client_Info_Update_Mobile", model.ClientCode, false))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "You have already one Mobile change request. For query: +8809606016379."
                    });
                }

                if (CheckMobile(model.NewMobile))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "This mobile number is already used by another client."
                    });
                }

                if (!GetClientContact( model.ClientCode, out string mobile, out string email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Client information not found."
                    });
                }

                string otp = GenerateOTP();

                memoryCache.Set( GetRequestKey( model.ClientCode,"MOBILE"),model,TimeSpan.FromMinutes(5));

                SaveOTP( model.ClientCode, "MOBILE", otp);

                SendOTPMessage(mobile, email, otp, "Mobile Number");

                return Ok(new
                {
                    success = true,
                    message = "OTP sent successfully by SMS and Email."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }


        [HttpPost("accountHolderMobileVerifyOTP")]
        public IActionResult accountHolderMobileVerifyOTP( [FromBody] OTPVerifyModel model)
        {
            try
            {
                if (!VerifyOTP(model.ClientCode, "MOBILE", model.OTP))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid or expired OTP."
                    });
                }

                string key = GetRequestKey( model.ClientCode, "MOBILE");

                if (!memoryCache.TryGetValue( key, out AccountHolderMobileModel request))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Request expired. Please request OTP again."
                    });
                }

                GetClientContact( request.ClientCode, out string oldMobile, out string oldEmail);

                string query = @"INSERT INTO T_Client_Info_Update_Mobile (ClientCode, OldNumber, NewNumber)
                                 VALUES (@ClientCode, @OldNumber, @NewNumber)";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@ClientCode", request.ClientCode),

                    new SqlParameter("@OldNumber", oldMobile ?? ""),

                    new SqlParameter("@NewNumber", request.NewMobile ?? "")
                };

                if (!ExecuteInsert(query, parameters))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to submit Mobile change request."
                    });
                }

                RemoveOTP( model.ClientCode, "MOBILE");

                memoryCache.Remove(key);

                return Ok(new
                {
                    success = true,
                    message = "Mobile change request successfully submitted."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }


        // NOMINEE

        [HttpPost("accountHolderNomineeGetOTP")]
        public async Task<IActionResult> accountHolderNomineeGetOTP([FromForm] NomineeRequest model)
        {
            try
            {
                if (model.Nominees == null || model.Nominees.Count == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "At least one nominee is required."
                    });
                }

                int oldTotal = GetExistingNomineePercentage(model.ClientCode);

                int newTotal = 0;

                foreach (var nominee in model.Nominees)
                {
                    newTotal += nominee.Percentage;
                }

                if (oldTotal + newTotal > 100)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Total nominee percentage cannot exceed 100."
                    });
                }

                List<NomineeOTPItem> nomineeItems = new List<NomineeOTPItem>();

                foreach (var nominee in model.Nominees)
                {
                    byte[] image = await GetFileBytes(nominee.Image);

                    byte[] nidFront = await GetFileBytes(nominee.NidFront);

                    byte[] nidBack = await GetFileBytes(nominee.NidBack);

                    byte[] signature = await GetFileBytes(nominee.Signature);

                    nomineeItems.Add(
                        new NomineeOTPItem
                        {
                            Name = nominee.Name,
                            Mobile = nominee.Mobile,
                            Relationship =
                                nominee.Relationship,
                            Percentage =
                                nominee.Percentage,

                            Image = image,
                            NidFront = nidFront,
                            NidBack = nidBack,
                            Signature = signature
                        });
                }

                if (!GetClientContact(model.ClientCode, out string mobile, out string email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message ="Client information not found."
                    });
                }

                string otp = GenerateOTP();

                var request =new NomineeOTPRequest
                    {
                        ClientCode = model.ClientCode,

                        Nominees = nomineeItems
                    };

                memoryCache.Set(GetRequestKey(model.ClientCode, "NOMINEE"),request, TimeSpan.FromMinutes(5));

                SaveOTP(model.ClientCode, "NOMINEE", otp);

                SendOTPMessage(mobile, email, otp, "Nominee Information");

                return Ok(new
                {
                    success = true,
                    message = "OTP sent successfully by SMS and Email."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }


        [HttpPost("accountHolderNomineeVerifyOTP")]
        public IActionResult accountHolderNomineeVerifyOTP( [FromBody] OTPVerifyModel model)
        {
            try
            {
                if (!VerifyOTP (model.ClientCode, "NOMINEE", model.OTP))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid or expired OTP."
                    });
                }

                string key = GetRequestKey( model.ClientCode, "NOMINEE");

                if (!memoryCache.TryGetValue(key, out NomineeOTPRequest request))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Request expired. Please request OTP again."
                    });
                }

                using (SqlConnection connection = new SqlConnection(SqlConn))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (var nominee in request.Nominees)
                            {
                                string query = @" INSERT INTO T_Client_Info_Update_Nominee
                                    ( ClientCode, Name,Mobile, Relationship, Percentage, Image, NidFront, NidBack,
                                        Signature, EntryDate, EntryDateTime, IsApproveCallCenter ) VALUES
                                    ( @ClientCode, @Name, @Mobile, @Relationship,  @Percentage, @Image, @NidFront,
                                        @NidBack,  @Signature, @EntryDate, @EntryDateTime, 0 )";

                                using (SqlCommand command = new SqlCommand(query, connection,transaction))
                                {
                                    command.CommandTimeout = 800;

                                    command.Parameters.AddWithValue( "@ClientCode", request.ClientCode);

                                    command.Parameters.AddWithValue( "@Name", nominee.Name ?? "");

                                    command.Parameters.AddWithValue( "@Mobile", nominee.Mobile ?? "");

                                    command.Parameters.AddWithValue( "@Relationship",  nominee.Relationship ?? "");

                                    command.Parameters.AddWithValue( "@Percentage",  nominee.Percentage);

                                    command.Parameters.Add( "@Image", SqlDbType.Image).Value = (object)nominee.Image
                                        ?? DBNull.Value;

                                    command.Parameters.Add( "@NidFront", SqlDbType.Image).Value = (object)nominee.NidFront?? DBNull.Value;

                                    command.Parameters.Add("@NidBack", SqlDbType.Image).Value = (object)nominee.NidBack
                                        ?? DBNull.Value;

                                    command.Parameters.Add("@Signature", SqlDbType.Image).Value = (object)nominee.Signature ?? DBNull.Value;

                                    command.Parameters.AddWithValue("@EntryDate", DateTime.Now.Date);

                                    command.Parameters.AddWithValue("@EntryDateTime", DateTime.Now);

                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                RemoveOTP(model.ClientCode,"NOMINEE");

                memoryCache.Remove(key);

                return Ok(new
                {
                    success = true,
                    message = "Nominee change request successfully submitted."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }



        // AUTHORIZE

        [HttpPost("accountHolderAuthorizeGetOTP")]
        public async Task<IActionResult> accountHolderAuthorizeGetOTP([FromForm] AuthorizeRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.ClientCode))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Client Code is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.TRACKINGNO))
                {
                    model.TRACKINGNO =GetTrackingNo(model.ClientCode);
                }

                if (string.IsNullOrWhiteSpace( model.TRACKINGNO))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tracking number not found."
                    });
                }

                byte[] image = await GetFileBytes(model.aImage);

                byte[] nidFront = await GetFileBytes(model.aNIDFont);

                byte[] nidBack = await GetFileBytes(model.aNIDBack);

                byte[] signature = await GetFileBytes(model.aSig);

                if (!GetClientContact(model.ClientCode,  out string mobile, out string email))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Client information not found."
                    });
                }

                string otp = GenerateOTP();

                var request = new AuthorizeOTPRequest
                    {
                        ClientCode = model.ClientCode, TRACKINGNO = model.TRACKINGNO, aTitle = model.aTitle,

                        aFirstName = model.aFirstName, aLastName = model.aLastName, aOccupation = model.aOccupation,

                        aDoB = model.aDoB, aFName = model.aFName, aMName = model.aMName, aAddress = model.aAddress,

                        aCity = model.aCity, aPostCode =  model.aPostCode, aDivision = model.aDivision,

                        aCountry = model.aCountry, aMobile = model.aMobile, aTel = model.aTel,

                        aFax = model.aFax, aEmail = model.aEmail, aNID = model.aNID,

                        aImage = image, aNIDFont = nidFront, aNIDBack = nidBack, aSig = signature
                    };

                memoryCache.Set( GetRequestKey( model.ClientCode, "AUTHORIZE"), request, TimeSpan.FromMinutes(5));

                SaveOTP(model.ClientCode, "AUTHORIZE", otp);

                SendOTPMessage(mobile, email, otp, "Authorize Information");

                return Ok(new
                {
                    success = true,
                    message = "OTP sent successfully by SMS and Email."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }


        [HttpPost("accountHolderAuthorizeVerifyOTP")]
        public IActionResult accountHolderAuthorizeVerifyOTP([FromBody] OTPVerifyModel model)
        {
            try
            {
                if (!VerifyOTP( model.ClientCode,"AUTHORIZE", model.OTP))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid or expired OTP."
                    });
                }

                string key = GetRequestKey( model.ClientCode, "AUTHORIZE");

                if (!memoryCache.TryGetValue(key,out AuthorizeOTPRequest request))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =  "Request expired. Please request OTP again."
                    });
                }

                string query = @" UPDATE T_ONLINE_BO_AccHolderAuthorize SET aTitle = @aTitle, aFirstName = @aFirstName,
                        aLastName = @aLastName, aOccupation = @aOccupation, aDoB = @aDoB, aFName = @aFName,
                        aMName = @aMName, aAddress = @aAddress, aCity = @aCity, aPostCode = @aPostCode,
                        aDivision = @aDivision, aCountry = @aCountry, aMobile = @aMobile, aTel = @aTel, aFax = @aFax,
                        aEmail = @aEmail, aNID = @aNID, aImage = @aImage, aNIDFont = @aNIDFont, aNIDBack = @aNIDBack, aSig = @aSig, aIsComplete = 1 WHERE TRACKINGNO = @TRACKINGNO";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@TRACKINGNO", request.TRACKINGNO),

                    new SqlParameter("@aTitle", request.aTitle ?? ""),

                    new SqlParameter("@aFirstName", request.aFirstName ?? ""),

                    new SqlParameter("@aLastName", request.aLastName ?? ""),

                    new SqlParameter("@aOccupation", request.aOccupation ?? ""),

                    new SqlParameter("@aDoB", (object)request.aDoB ?? DBNull.Value),

                    new SqlParameter("@aFName", request.aFName ?? ""),

                    new SqlParameter("@aMName", request.aMName ?? ""),

                    new SqlParameter("@aAddress", request.aAddress ?? ""),

                    new SqlParameter("@aCity", request.aCity ?? ""),

                    new SqlParameter("@aPostCode", request.aPostCode ?? ""),

                    new SqlParameter("@aDivision", request.aDivision ?? ""),

                    new SqlParameter("@aCountry", request.aCountry ?? ""),

                    new SqlParameter("@aMobile", request.aMobile ?? ""),

                    new SqlParameter("@aTel", request.aTel ?? ""),

                    new SqlParameter("@aFax", request.aFax ?? ""),

                    new SqlParameter("@aEmail", request.aEmail ?? ""),

                    new SqlParameter("@aNID", request.aNID ?? ""),

                    new SqlParameter("@aImage", SqlDbType.Image)
                    {
                        Value = (object)request.aImage ?? DBNull.Value
                    },

                    new SqlParameter("@aNIDFont", SqlDbType.Image)
                    {
                        Value = (object)request.aNIDFont?? DBNull.Value
                    },

                    new SqlParameter("@aNIDBack", SqlDbType.Image)
                    {
                        Value = (object)request.aNIDBack ?? DBNull.Value
                    },

                    new SqlParameter("@aSig", SqlDbType.Image)
                    {
                        Value = (object)request.aSig ?? DBNull.Value
                    }
                };

                if (!ExecuteUpdate(query, parameters))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to update Authorize information."
                    });
                }

                RemoveOTP( model.ClientCode, "AUTHORIZE");

                memoryCache.Remove(key);

                return Ok(new
                {
                    success = true,
                    message = "Authorize information successfully updated."
                });
            }
            catch (Exception ex)
            {
                return DatabaseError(ex);
            }
        }

        // COMMON: PENDING REQUEST

        private bool HasPendingRequest( string tableName, string clientCode, bool checkImage)
        {
            string query;

            if (checkImage)
            {
                query = $@" SELECT COUNT(*) FROM {tableName} WHERE ClientCode = @ClientCode AND IsApproveCallCenter = 0
                    AND IsApproveCDBL = 0 AND IsDeleted = 0 AND Image IS NOT NULL";
            }
            else
            {
                query = $@" SELECT COUNT(*) FROM {tableName}  WHERE ClientCode = @ClientCode
                    AND IsApproveCallCenter = 0 AND IsApproveCDBL = 0 AND IsDeleted = 0";
            }

            using (SqlConnection connection = new SqlConnection(SqlConn))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = 800;

                    command.Parameters.AddWithValue("@ClientCode", clientCode);

                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        // COMMON INSERT
        private bool ExecuteInsert(string query, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(SqlConn))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = 800;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange( parameters);
                    }

                    return command.ExecuteNonQuery() == 1;
                }
            }
        }

        // COMMON UPDATE

        private bool ExecuteUpdate(string query, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(SqlConn))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = 800;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange( parameters);
                    }

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }


        // BANK VALIDATION

        private bool BankBranchExists( string bankName, string district, string branch, string routing)
        {
            string query = @" SELECT COUNT(*) FROM T_BANKINFO WHERE BANKNAME = @BankName AND 
                             DISTRICT = @District AND BRANCH = @Branch AND ROUTING = @Routing";

            using (SqlConnection connection = new SqlConnection(SqlConn))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand( query, connection))
                {
                    command.CommandTimeout = 800;

                    command.Parameters.AddWithValue("@BankName", bankName ?? "");

                    command.Parameters.AddWithValue("@District", district ?? "");

                    command.Parameters.AddWithValue("@Branch", branch ?? "");

                    command.Parameters.AddWithValue("@Routing", routing ?? "");

                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }


        // MOBILE VALIDATION
        private bool CheckMobile(string mobile)
        {
            string query = @" SELECT COUNT(*) FROM T_CLIENT1 WHERE mobile1 = @Mobile AND boStatus = 'Active'";

            using (SqlConnection connection = new SqlConnection(SqlConn))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand( query,connection))
                {
                    command.Parameters.AddWithValue( "@Mobile", mobile);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count >= 2;
                }
            }
        }


        // NOMINEE TOTAL
        private int GetExistingNomineePercentage( string clientCode)
        {
            string query = @"SELECT ISNULL(SUM(Percentage),0) FROM T_Client_Info_Update_Nominee 
                              WHERE ClientCode = @ClientCode";

            using (SqlConnection connection = new SqlConnection(SqlConn))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue( "@ClientCode", clientCode);

                    return Convert.ToInt32( command.ExecuteScalar());
                }
            }
        }


        // GET TRACKING NO
        private string GetTrackingNo(string clientCode)
        {
            string query = @"SELECT TRACKINGNO FROM T_ONLINE_BO_AccountHolder WHERE RCODE = @ClientCode";

            using (SqlConnection connection = new SqlConnection(SqlConn))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientCode", clientCode);

                    object result = command.ExecuteScalar();

                    return result == null || result == DBNull.Value ? "" : result.ToString();
                }
            }
        }

        [HttpPost("check")]
        public async Task<IActionResult> Check([FromBody] CheckRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ClientCode))
            {
                return BadRequest(new
                {
                    success = false,
                    exists = false,
                    canUpdate = false,
                    message = "Client Code is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Section))
            {
                return BadRequest(new
                {
                    success = false,
                    exists = false,
                    canUpdate = false,
                    message = "Section is required."
                });
            }

            var tables = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase)
            {
                { "Address", "T_Client_Info_Update_Address" },
                { "Bank", "T_Client_Info_Update_Bank" },
                { "Email", "T_Client_Info_Update_Email" },
                { "Mobile", "T_Client_Info_Update_Mobile" },
                { "Nominee", "T_Client_Info_Update_Nominee" },
                { "TIN", "T_Client_Info_Update_TIN" }
            };

            string section = request.Section.Trim();

            if (!tables.TryGetValue(section, out string? tableName))
            {
                return BadRequest(new
                {
                    success = false,
                    exists = false,
                    canUpdate = false,
                    message ="Invalid section. Allowed values: Address, Bank, Email, Mobile, Nominee, TIN."
                });
            }

            string clientCode = request.ClientCode.Trim();

            try
            {
                await using var connection = new SqlConnection(SqlConn);

                if (connection == null)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        exists = false,
                        canUpdate = false,
                        message = "Database connection is not configured."
                    });
                }

                string query = $@" SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.[{tableName}] WHERE ClientCode = @ClientCode) THEN 1 ELSE 0 END;";

                //await using var connection = new SqlConnection(connectionString);

                await using var command = new SqlCommand(query, connection);

                command.Parameters.Add("@ClientCode", SqlDbType.VarChar, 50).Value = clientCode;

                await connection.OpenAsync();

                object? result = await command.ExecuteScalarAsync();

                bool exists = result != null && result != DBNull.Value && Convert.ToInt32(result) == 1;

                return Ok(new
                {
                    success = true,
                    clientCode = clientCode,
                    section = section,
                    exists = exists,
                    canUpdate = !exists,
                    message = exists ? $"{section} update request already exists." : $"You can update {section} information."
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine( $"ClientInfoUpdateCheck error: {ex}");

                return StatusCode(500, new
                {
                    success = false,
                    exists = false,
                    canUpdate = false,
                    message = "Failed to check client update request."
                });
            }
        }
    }


    // ALREADY EXISTING CODE


 

    public class CheckRequest
    {
       public string ClientCode { get; set; } = string.Empty;
       public string Section { get; set; } = string.Empty;
    }



    // COMMON MODELS


    public class OTPVerifyModel
    {
        public string ClientCode { get; set; }
        public string OTP { get; set; }
    }



    // ADDRESS MODELS

    public class AccountHolderAddressRequest
    {
        public string ClientCode { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public IFormFile Image { get; set; }
    }

    public class AddressOTPRequest
    {
        public string ClientCode { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public byte[] Image { get; set; }
        public string ImageFileName { get; set; }
    }



    // BANK MODELS


    public class AccountHolderBankRequest
    {
        public string ClientCode { get; set; }
        public string BankName { get; set; }
        public string District { get; set; }
        public string BranchName { get; set; }
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public IFormFile Image { get; set; }
    }

    public class BankOTPRequest
    {
        public string ClientCode { get; set; }
        public string BankName { get; set; }
        public string District { get; set; }
        public string BranchName { get; set; }
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public byte[] Image { get; set; }
    }



    // E-TIN MODELS


    public class AccountHolderETINRequest
    {
        public string ClientCode { get; set; }
        public string NewTIN { get; set; }
        public string ConfirmTIN { get; set; }
        public IFormFile Image { get; set; }
    }

    public class ETINOTPRequest
    {
        public string ClientCode { get; set; }
        public string NewTIN { get; set; }
        public string ConfirmTIN { get; set; }
        public byte[] Image { get; set; }
    }



    // MOBILE MODEL


    public class AccountHolderMobileModel
    {
        public string ClientCode { get; set; }
        public string NewMobile { get; set; }
        public string ConfirmMobile { get; set; }
    }


    // EMAIL MODEL


    public class AccountHolderEmailModel
    {
        public string ClientCode { get; set; }

        public string OldEmail { get; set; }

        public string NewEmail { get; set; }

        public string ConfirmEmail { get; set; }
    }


    // NOMINEE MODELS


    public class NomineeRequest
    {
        public string ClientCode { get; set; }

        public List<NomineeItem> Nominees { get; set; }
    }

    public class NomineeItem
    {
        public string Name { get; set; }

        public string Mobile { get; set; }

        public string Relationship { get; set; }

        public int Percentage { get; set; }

        public IFormFile Image { get; set; }

        public IFormFile NidFront { get; set; }

        public IFormFile NidBack { get; set; }

        public IFormFile Signature { get; set; }
    }

    public class NomineeOTPRequest
    {
        public string ClientCode { get; set; }

        public List<NomineeOTPItem> Nominees { get; set; }
    }

    public class NomineeOTPItem
    {
        public string Name { get; set; }

        public string Mobile { get; set; }

        public string Relationship { get; set; }

        public int Percentage { get; set; }

        public byte[] Image { get; set; }

        public byte[] NidFront { get; set; }

        public byte[] NidBack { get; set; }

        public byte[] Signature { get; set; }
    }



    // AUTHORIZE MODELS


    public class AuthorizeRequest
    {
        public string ClientCode { get; set; }

        public string TRACKINGNO { get; set; }

        public string aTitle { get; set; }

        public string aFirstName { get; set; }

        public string aLastName { get; set; }

        public string aOccupation { get; set; }

        public DateTime? aDoB { get; set; }

        public string aFName { get; set; }

        public string aMName { get; set; }

        public string aAddress { get; set; }

        public string aCity { get; set; }

        public string aPostCode { get; set; }

        public string aDivision { get; set; }

        public string aCountry { get; set; }

        public string aMobile { get; set; }

        public string aTel { get; set; }

        public string aFax { get; set; }

        public string aEmail { get; set; }

        public string aNID { get; set; }

        public IFormFile aImage { get; set; }

        public IFormFile aNIDFont { get; set; }

        public IFormFile aNIDBack { get; set; }

        public IFormFile aSig { get; set; }
    }


    public class AuthorizeOTPRequest
    {
        public string ClientCode { get; set; }

        public string TRACKINGNO { get; set; }

        public string aTitle { get; set; }

        public string aFirstName { get; set; }

        public string aLastName { get; set; }

        public string aOccupation { get; set; }

        public DateTime? aDoB { get; set; }

        public string aFName { get; set; }

        public string aMName { get; set; }

        public string aAddress { get; set; }

        public string aCity { get; set; }

        public string aPostCode { get; set; }

        public string aDivision { get; set; }

        public string aCountry { get; set; }

        public string aMobile { get; set; }

        public string aTel { get; set; }

        public string aFax { get; set; }

        public string aEmail { get; set; }

        public string aNID { get; set; }

        public byte[] aImage { get; set; }

        public byte[] aNIDFont { get; set; }

        public byte[] aNIDBack { get; set; }

        public byte[] aSig { get; set; }
    }
}