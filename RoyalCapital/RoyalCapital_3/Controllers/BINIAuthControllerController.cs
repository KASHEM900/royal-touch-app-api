
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using RoyalCapital_3.Models;
using System;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static RoyalCapital_3.Controllers.BORegistrationController;

namespace RoyalCapital_3.Controllers
{
    [Route("api/v1/bini")]
    [ApiController]
    public class BINIAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BINIAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("bo-registration")]
        public IActionResult BINIBORegister([FromBody] BiniBORegistrationModel bomodel)
        {
            if (bomodel == null || !ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid registration information.",
                    errors = ModelState
                });

            string connectionString = _configuration.GetConnectionString("RCLWEB");

            try
            {
                // 1. Validate Promo Code
                if (!string.IsNullOrWhiteSpace(bomodel.PromoCode))
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = @"SELECT COUNT(*) FROM T_ONLINE_BO_PROMO WHERE PromoCode = @promo AND GETDATE() BETWEEN Pstart AND Pend";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@promo", bomodel.PromoCode);

                            conn.Open();

                            int promoCount = Convert.ToInt32( cmd.ExecuteScalar());

                            if (promoCount == 0)
                            {
                                return BadRequest(new
                                {
                                    success = false,
                                    message = "Promo code is not valid or expired!"
                                });
                            }
                        }
                    }
                }

                // 2. Check Existing Active BO Accounts
                int mobileCount;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT COUNT(*) FROM T_Client1 WHERE mobile1 = @mobile AND boStatus = 'Active'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@mobile", bomodel.Mobile);

                        conn.Open();

                        mobileCount = Convert.ToInt32( cmd.ExecuteScalar());
                    }
                }

                if (mobileCount >= 2)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "You already have two active BO accounts with this mobile number."
                    });
                }

                // 3. Generate Tracking Number
                int maxSl;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT ISNULL(MAX(SL), 0) + 1  FROM T_ONLINE_BO_REG";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        maxSl = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }

                bomodel.TrackingNo = "BO" + maxSl;

                // 4. Generate Secure 6 Digit Password
                bomodel.Password = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                // 5. Generate Password Hash
                var passwordHasher = new PasswordHasher<string>();

                bomodel.PasswordHash = passwordHasher.HashPassword( bomodel.TrackingNo, bomodel.Password);

                // 6. Save Registration
                // AddOnlineBoReg must save BOTH:
                // PASS = bomodel.Password
                // PasswordHash = bomodel.PasswordHash

                var dbHandler = new OnlineBoRegDBHandle(_configuration);

                bool inserted = dbHandler.AddOnlineBoRegBINI(bomodel);

                if (!inserted)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Registration Failed."
                    });
                }

                // 7. Send Email
                try
                {
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress( _configuration["Smtp:Username"], "Royal Capital Online BO Opening");

                        mail.To.Add(bomodel.Email);

                        mail.Subject = "Royal Capital-Online BO Account Registration";

                        mail.Body =
                            "Dear Concern!\n\n" +
                            "Good day from Royal Capital Ltd.\n" +
                            "Thanks for signing up with our online BO opening portal.\n\n" +
                            "1. Access to our portal:\n" +
                            "http://bo.royalcapitalbd.com/Home/Login\n\n" +
                            "2. Put in your ID and Password in the Sign in area.\n\n" +
                            "User ID: " + bomodel.TrackingNo + "\n" +
                            "Password: " + bomodel.Password + "\n\n" +
                            "For more information, call at 16379 or 09606016379.\n\n" +
                            "Thanks for being with us.\n" +
                            "Royal Capital Ltd.";

                        using (SmtpClient smtp = new SmtpClient(_configuration["Smtp:Host"]))
                        {
                            smtp.Port = Convert.ToInt32(_configuration["Smtp:Port"]);

                            smtp.Credentials = new NetworkCredential( _configuration["Smtp:Username"], _configuration["Smtp:Password"]);

                            smtp.EnableSsl = false;

                            smtp.Send(mail);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Registration is already saved.
                    // Log email failure internally.

                    return StatusCode(500, new
                    {
                        success = false,
                        registered = true,
                        message = "Registration successful, but email sending failed. Please contact support."
                    });
                }

                // 8. Registration Success
                return Ok(new
                {
                    success = true,
                    message = "Success! Please check your email for credentials.",
                    TrackingNo = bomodel.TrackingNo,
                    Password = bomodel.Password

                    // Password email-e pathano hocche.
                    // API response-e password return korchi na.
                });
            }
            catch (Exception)
            {
                // Log exception internally.

                return StatusCode(500, new
                {
                    success = false,
                    message = "Registration Failed."
                });
            }
        }

        [HttpPost("bo-login")]
        public IActionResult BoCheckLoginCredentials([FromBody] BOLoginModel lbomodel)
        {
            if (lbomodel == null || string.IsNullOrWhiteSpace(lbomodel.TrackingNo) || string.IsNullOrWhiteSpace(lbomodel.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "TrackingNo and Password are required."
                });
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("RCLWEB");



                string trackingNo;
                string username;
                string mobile;
                string passwordHash;
                string isApproved;

                // 1. Get Login Information
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
  
                    string query = @"SELECT TRACKINGNO, PasswordHash, FIRSTNAME, LASTNAME, MOBILE, IsApp1, IsEkycValid FROM T_ONLINE_BO_REG  WHERE TRACKINGNO = @tracking";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tracking", lbomodel.TrackingNo);

                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return Unauthorized(new
                                {
                                    success = false,
                                    message = "Invalid Credentials."
                                });
                            }

                            trackingNo = reader["TRACKINGNO"].ToString();

                            passwordHash = reader["PasswordHash"] == DBNull.Value ? null : reader["PasswordHash"].ToString();

                            username = (reader["FIRSTNAME"]?.ToString() + " " + reader["LASTNAME"]?.ToString()).Trim();

                            mobile = reader["MOBILE"].ToString();

                            isApproved = reader["IsApp1"]?.ToString();
                        }
                    }
                }

                // 2. Verify Password Hash
                if (string.IsNullOrWhiteSpace(passwordHash))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid Credentials."
                    });
                }

                var passwordHasher = new PasswordHasher<string>();

                var passwordResult = passwordHasher.VerifyHashedPassword( trackingNo, passwordHash, lbomodel.Password
                    );

                if (passwordResult == PasswordVerificationResult.Failed)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid Credentials."
                    });
                }

                // 3. Check RCODE
                // RCODE NULL or empty = Continue Login
                // RCODE has value = Login Not Allowed

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT TOP 1 RCODE FROM T_ONLINE_BO_AccountHolder WHERE TRACKINGNO = @tracking";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tracking", trackingNo);

                        conn.Open();

                        object rcodeResult = cmd.ExecuteScalar();

                        string rcode = rcodeResult == null || rcodeResult == DBNull.Value ? null : rcodeResult.ToString().Trim();

                        if (!string.IsNullOrWhiteSpace(rcode))
                        {
                            return Conflict(new
                            {
                                success = false,
                                message = "Your BO application has already been approved. You can no longer log in."
                            });
                        }
                    }
                }

                // 4. Existing IsApp1 Check
                if (isApproved == "1")
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Your BO application has already been approved. You can no longer log in."
                    });
                }

                // 5. Load Existing Dropdown Data

                //BOOtherDBHandler DB = new BOOtherDBHandler(_configuration);

                //SelectList BRANCHES = new SelectList( DB.branchInfo().ToList(), "BRANCHNAME", "BRANCHNAME"
                //);

                //BOOnlineBoDetailDBHandle BDB = new BOOnlineBoDetailDBHandle(_configuration);

                //var AccHolderSelect = BDB.AccHolderSelect(trackingNo).ToList();

               // BOOtherDBHandler DB1 = new BOOtherDBHandler(_configuration);

                //var Title = DB1.GetTitle().ToList();

                // 6. Generate JWT Bearer Token

                string token = GenerateJwtToken(trackingNo, username);

                // 7. Successful Login Response

                return Ok(new
                {
                    success = true,
                    message = "Login successful.",

                    trackingNo = trackingNo,
                    username = username,
                    mobile = mobile,

                    tokenType = "Bearer",
                    accessToken = token,
                    expiresIn = 1800,

                    //BRANCHES = BRANCHES,
                    //AccHolderSelect = AccHolderSelect,
                    //Title = Title
                });
            }
            catch (Exception ex)
            {
                // Log exception internally in production.

                return StatusCode(500, new
                {
                    success = false,
                    message = "Server error."
                });
            }
        }


        // JWT Token Generate Method

        private string GenerateJwtToken(string trackingNo, string username)
        {
            var key = new SymmetricSecurityKey( Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]) );

            var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                //new Claim(ClaimTypes.NameIdentifier,trackingNo),

                new Claim("trackingNo", trackingNo),

                //new Claim(ClaimTypes.Name,username )
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}


