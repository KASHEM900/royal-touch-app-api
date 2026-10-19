using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RoyalCapital_3.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using static RoyalCapital_3.Controllers.BORegistrationController;
using static System.Net.WebRequestMethods;

namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BINIBORegistrationController : Controller
    {
        private readonly IConfiguration _configuration;

        public BINIBORegistrationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost]
        public ActionResult BINIBORegister([FromBody] BORegistrationModel bomodel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string connectionString = _configuration.GetConnectionString("RCLWEB");

            try
            {
                // Validate promo code
                if (!string.IsNullOrEmpty(bomodel.PromoCode))
                {
                    using SqlConnection conn = new SqlConnection(connectionString);
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM T_ONLINE_BO_PROMO WHERE PromoCode=@promo AND GETDATE() BETWEEN Pstart AND Pend", conn);
                    cmd.Parameters.AddWithValue("@promo", bomodel.PromoCode);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        return Ok(new { message = "Promo code is not valid or expired!" });
                    }
                }

                // Check for existing mobile number
                int mobileCount;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT COUNT(mobile1) FROM T_Client1 WHERE mobile1=@mobile AND boStatus='Active'";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@mobile", bomodel.Mobile);
                    conn.Open();
                    mobileCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (mobileCount >= 2)
                {
                    return NotFound(new { message = "You already have two active BO accounts with this mobile number." });
                }

                // Generate TrackingNo and Password
                int maxSl;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT ISNULL(MAX(SL), 0) + 1 FROM T_ONLINE_BO_REG";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    maxSl = Convert.ToInt32(cmd.ExecuteScalar());
                }

                bomodel.TrackingNo = "BO" + maxSl;
                bomodel.Password = new Random().Next(100000, 999999).ToString("D6");

                //string boOTP = BoOTPSMS(bomodel.Mobile);

                var dbHandler = new OnlineBoRegDBHandle(_configuration);
                bool inserted = dbHandler.AddOnlineBoReg(bomodel);

                if (inserted)
                {
                    try
                    {
                        MailMessage mail = new MailMessage
                        {
                            From = new MailAddress("bo@royalcapitalbd.com", "Royal Capital Online BO Opening"),
                            Subject = "Royal Capital-Online BO Account Registration",


                            Body = $"Dear Concern!\n\n" +
                           $"Good day from Royal Capital Ltd.\n" +
                           $"Thanks for signing up with our online BO opening portal.\n\n" +
                           $"1. Access to our portal http://bo.royalcapitalbd.com/Home/Login\n" +
                           $"2. Put in your ID and Password in the “Sign in” area\n\n" +
                           $"User ID: {bomodel.TrackingNo}\n" +
                           $"Password: {bomodel.Password}\n\n" +
                           $"For more information, call at 16379 or 09606016379\n\n" +
                           $"Thanks for being with us.\n" +
                           $"Royal Capital Ltd."


                        };
                        mail.To.Add(bomodel.Email);

                        SmtpClient smtp = new SmtpClient("159.21.2.246")
                        {
                            Port = 25,
                            Credentials = new System.Net.NetworkCredential("bo@royalcapitalbd.com", "rclbd1234%"),
                            EnableSsl = false
                        };
                        smtp.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { message = "Email sending failed: " + ex.Message });
                    }

                    //return Ok(new { message = "Success! Please check your email for credentials." });

                    return Ok(new
                    {
                        message = "Success! Please check your email for credentials.",
                        success = true,
                        TrackingNo = bomodel.TrackingNo,
                        Password = bomodel.Password
                        //otp = boOTP
                    });
                }

                return StatusCode(500, new { message = "Registration Failed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Registration Failed: " + ex.Message });
            }
        }

 
    }
}

