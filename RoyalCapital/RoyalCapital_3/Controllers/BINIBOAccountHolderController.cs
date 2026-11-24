
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using RoyalCapital_3.Models;
using System;
using System.Collections.Generic;
using System.Data;
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
    public class BINIBOAccountHolderController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string SqlConn;

        public BINIBOAccountHolderController(IConfiguration configuration)
        {
            _configuration = configuration;
            SqlConn = _configuration.GetConnectionString("RCLWEB");
        }

        [Authorize]
        [HttpPost("bo-accountholderbranch")]
        public IActionResult boAccountHolderBranchTypeAC([FromBody] BOAccountHolderBranchTypeAC boModel)
        {
            string tokenTrackingNo = User.FindFirst("trackingNo")?.Value;

            if (string.IsNullOrEmpty(tokenTrackingNo))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid or expired token."
                });
            }

            if (boModel.TRACKINGNO != tokenTrackingNo)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "You are not authorized to update this account."
                });
            }

            using (SqlConnection connection = new SqlConnection(SqlConn))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Update T_ONLINE_BO_AccountHolder
                        string queryAccountHolder = @"UPDATE dbo.T_ONLINE_BO_AccountHolder SET [DesireBranch] = @DesireBranch, [ToC] = @ToC
                                                   WHERE [TRACKINGNO] = @TRACKINGNO";

                        using (SqlCommand command = new SqlCommand(queryAccountHolder, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@TRACKINGNO", boModel.TRACKINGNO);

                            command.Parameters.AddWithValue("@DesireBranch", boModel.DesireBranch);

                            command.Parameters.AddWithValue("@ToC", boModel.ToC);

                            command.ExecuteNonQuery();
                        }

                        // 2. Update T_ONLINE_BO_REG
                        string queryReg = @"UPDATE dbo.T_ONLINE_BO_REG SET [RefarralCode] = @RefarralCode WHERE [TRACKINGNO] = @TRACKINGNO";

                        using (SqlCommand command = new SqlCommand(queryReg, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@TRACKINGNO", boModel.TRACKINGNO);

                            command.Parameters.AddWithValue("@RefarralCode", string.IsNullOrWhiteSpace(boModel.RefarralCode)
                                    ? (object)DBNull.Value : boModel.RefarralCode);

                            command.ExecuteNonQuery();
                        }
                        // 3. Get FIRSTNAME and LASTNAME
                        string nameQuery = @"SELECT [FIRSTNAME], [LASTNAME] FROM dbo.T_ONLINE_BO_REG WHERE [TRACKINGNO] = @TRACKINGNO";

                        string firstName = "";
                        string lastName = "";
                        string trackingNo = "";

                        using (SqlCommand command = new SqlCommand(
                            nameQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@TRACKINGNO", boModel.TRACKINGNO);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    firstName = reader["FIRSTNAME"]?.ToString() ?? "";
                                    lastName = reader["LASTNAME"]?.ToString() ?? "";
                                }
                            }
                        }

                        transaction.Commit();

                        return Ok(new
                        {
                            success = true,
                            message = "Account Holder Branch Information Inserted !!",
                            trackingNo = boModel.TRACKINGNO
                        });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        return StatusCode(500, new
                        {
                            message = ex.Message,
                            success = false
                        });
                    }
                }
            }
        }

        [Authorize]
        [HttpPost("bo-accountholder")]
        public IActionResult boAccountHolder([FromBody] BOAccountHolderModel abomodel)
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

                string tokenTrackingNo = User.FindFirst("trackingNo")?.Value;

                if (string.IsNullOrEmpty(tokenTrackingNo))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid or expired token."
                    });
                }

                if (abomodel.TRACKINGNO != tokenTrackingNo)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "You are not authorized to update this account."
                    });
                }

                if (!string.IsNullOrEmpty(abomodel.TRACKINGNO))
                {
                    BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(_configuration);
                    bool isSuccess = DB.AddAccHolder(abomodel);

                    if (isSuccess)
                    {
                        //var bankinfo = new List<BOBankInfoModel>();

                        //SqlConnection oConnection = new SqlConnection(SqlConn);
                        //oConnection.Open();

                        //string query = "SELECT [BANKNAME],[BANKBRANCH],[DISTRICT],[ROUTING],[AC] FROM T_ONLINE_BO_AccHolderBANK WHERE [TRACKINGNO]=@boId";

                        //SqlCommand oCommand = new SqlCommand(query, oConnection);
                        //oCommand.Parameters.AddWithValue("@boId", abomodel.TRACKINGNO);

                        //DataTable oTable = new DataTable();
                        //oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                        //BOOnlineBoDetailDBHandle bank = new BOOnlineBoDetailDBHandle(_configuration);

                        //var banks = (bank.binibankNameInfo().ToList());

                        return Ok(new
                        {
                            success = true,
                            message = "Account Holder Information Successfully Updated.",
                            TrackingNo = abomodel.TRACKINGNO,
                            //banks = banks

                        });


                    }
                    else
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Account Holder Information Insertion Failed!!!"
                        });
                    }

                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "NO Tracking NO Found, Account Holder Information Insertion Failed!!!"
                    });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database Error... Account Holder Information Insertion Failed!!!",
                    error = ex.Message
                });
            }
        }


        [Authorize]
        [HttpPost("bo-jointaccountholder")]
        public IActionResult boAccountJointHolder([FromBody] BoSecondAccHolderModel abomodel)
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

                string tokenTrackingNo = User.FindFirst("trackingNo")?.Value;

                if (string.IsNullOrEmpty(tokenTrackingNo))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid or expired token."
                    });
                }

                if (abomodel.TRACKINGNO != tokenTrackingNo)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "You are not authorized to update this account."
                    });
                }

                if (!string.IsNullOrEmpty(abomodel.TRACKINGNO))
                {
                    BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(_configuration);
                    bool isSuccess = DB.AddSecondAccHolder(abomodel);

                    if (isSuccess)
                    {
                        //var bankinfo = new List<BOBankInfoModel>();

                        //SqlConnection oConnection = new SqlConnection(SqlConn);
                        //oConnection.Open();

                        //string query = "SELECT [BANKNAME],[BANKBRANCH],[DISTRICT],[ROUTING],[AC] FROM T_ONLINE_BO_AccHolderBANK WHERE [TRACKINGNO]=@boId";

                        //SqlCommand oCommand = new SqlCommand(query, oConnection);
                        //oCommand.Parameters.AddWithValue("@boId", abomodel.TRACKINGNO);

                        //DataTable oTable = new DataTable();
                        //oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                        //BOOnlineBoDetailDBHandle bank = new BOOnlineBoDetailDBHandle(_configuration);

                        //var banks = (bank.binibankNameInfo().ToList());

                        return Ok(new
                        {
                            success = true,
                            message = "Joint Account Holder Information Successfully Updated.",
                            TrackingNo = abomodel.TRACKINGNO,
                            //banks = banks

                        });
                    }
                    else
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Joint Account Holder Information Insertion Failed!!!"
                        });
                    }

                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "NO Tracking NO Found, Joint Account Holder Information Insertion Failed!!!"
                    });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database Error... Joint Account Holder Information Insertion Failed!!!",
                    error = ex.Message
                });
            }
        }


        [Authorize]
        [HttpPost("bo-accountholderaddress")]
        public IActionResult boAccountHolderAddress([FromBody] BoSecondAccHolderAddressModel adsbomodel)
        {
            try
            {
                string tokenTrackingNo = User.FindFirst("trackingNo")?.Value;

                if (string.IsNullOrEmpty(tokenTrackingNo))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid or expired token."
                    });
                }

                if (adsbomodel.TRACKINGNO != tokenTrackingNo)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "You are not authorized to update this account."
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No Data FOund.",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                if (!string.IsNullOrEmpty(adsbomodel.TRACKINGNO))
                {
                    BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(_configuration);
                    bool isSuccess = DB.AddAccHolderAddress(adsbomodel);

                    if (isSuccess)
                    {
                        var bankinfo = new List<BOBankInfoModel>();

                        SqlConnection oConnection = new SqlConnection(SqlConn);
                        oConnection.Open();

                        string query = "SELECT [BANKNAME],[BANKBRANCH],[DISTRICT],[ROUTING],[AC] FROM T_ONLINE_BO_AccHolderBANK WHERE [TRACKINGNO]=@boId";

                        SqlCommand oCommand = new SqlCommand(query, oConnection);
                        oCommand.Parameters.AddWithValue("@boId", adsbomodel.TRACKINGNO);

                        DataTable oTable = new DataTable();
                        oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                        BOOnlineBoDetailDBHandle bank = new BOOnlineBoDetailDBHandle(_configuration);

                        var banks = (bank.binibankNameInfo().ToList());

                        return Ok(new
                        {
                            success = true,
                            message = "Account Holder Information Successfully Updated.",
                            TrackingNo = adsbomodel.TRACKINGNO,
                            banks = banks

                        });


                    }
                    else
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Account Holder Information Insertion Failed!!!"
                        });
                    }

                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "NO Tracking NO Found, Account Holder Information Insertion Failed!!!"
                    });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database Error... Account Holder Information Insertion Failed!!!",
                    error = ex.Message
                });
            }
        }


        [Authorize]
        [HttpPost("bo-accountolderbank")]
        public IActionResult boAccountHolderBank([FromBody] BOAccountHolderBankModel bbomodel)
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

                if (!string.IsNullOrEmpty(bbomodel.TrackingNo))
                {
                    BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(_configuration);
                    bool isSuccess = DB.AddBankDetail(bbomodel);

                    if (isSuccess)
                    {
                        return Ok(new
                        {
                            TrackingNo = bbomodel.TrackingNo,
                            success = true,
                            message = "Account Holder Bank Information Successfully Updated.",
                        });
                    }
                    else
                    {
                        return StatusCode(500, new
                        {
                            success = false,
                            message = "Account Holder Information Insertion Failed!!!"
                        });
                    }

                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "NO Tracking NO Found, Account Holder Information Insertion Failed!!!"
                    });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database Error... Account Holder Information Insertion Failed!!!",
                    error = ex.Message
                });
            }
        }

    }
}


