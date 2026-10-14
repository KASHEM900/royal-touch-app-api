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
    public class BOAccountHolderController : ControllerBase
    {
        private readonly IConfiguration configuration; 
        private readonly string SqlConn;
        public BOAccountHolderController(IConfiguration config)
        {
            configuration = config;
            SqlConn = config.GetConnectionString("RCLWEB");
        }


        [HttpPost("boAccountHolder")]
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

                if (!string.IsNullOrEmpty(abomodel.TRACKINGNO))
                {
                    BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(configuration);
                    bool isSuccess = DB.AddAccHolder(abomodel);

                    if (isSuccess)
                    {                  
                        var bankinfo = new List<BOBankInfoModel>();

                        SqlConnection oConnection = new SqlConnection(SqlConn);
                        oConnection.Open();

                        string query = "SELECT [BANKNAME],[BANKBRANCH],[DISTRICT],[ROUTING],[AC] FROM T_ONLINE_BO_AccHolderBANK WHERE [TRACKINGNO]=@boId";

                        SqlCommand oCommand = new SqlCommand(query, oConnection); 
                        oCommand.Parameters.AddWithValue("@boId", abomodel.TRACKINGNO);

                        DataTable oTable = new DataTable();
                        oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                        BOOnlineBoDetailDBHandle bank = new BOOnlineBoDetailDBHandle(configuration);

                        var banks = (bank.bankNameInfo().ToList());

                        return Ok(new
                        {
                            success = true,
                            message = "Account Holder Information Successfully Updated.",
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
                        message =  "NO Tracking NO Found, Account Holder Information Insertion Failed!!!"
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

       
        [HttpPost("boAccountJointHolder")]
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

                if (!string.IsNullOrEmpty(abomodel.TRACKINGNO))
                {
                    BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(configuration);
                    bool isSuccess = DB.AddSecondAccHolder(abomodel);

                    if (isSuccess)
                    {
                        var bankinfo = new List<BOBankInfoModel>();

                        SqlConnection oConnection = new SqlConnection(SqlConn);
                        oConnection.Open();

                        string query = "SELECT [BANKNAME],[BANKBRANCH],[DISTRICT],[ROUTING],[AC] FROM T_ONLINE_BO_AccHolderBANK WHERE [TRACKINGNO]=@boId";

                        SqlCommand oCommand = new SqlCommand(query, oConnection);
                        oCommand.Parameters.AddWithValue("@boId", abomodel.TRACKINGNO);

                        DataTable oTable = new DataTable();
                        oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                        BOOnlineBoDetailDBHandle bank = new BOOnlineBoDetailDBHandle(configuration);

                        var banks = (bank.bankNameInfo().ToList());

                        return Ok(new
                        {
                            success = true,
                            message = "Account Holder Information Successfully Updated.",
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

        [HttpPost("boAccountHolderAddress")]
        public IActionResult boAccountHolderAddress([FromBody] BoSecondAccHolderAddressModel adsbomodel)
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

                if (!string.IsNullOrEmpty(adsbomodel.TRACKINGNO))
                {
                    BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(configuration);
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

                        BOOnlineBoDetailDBHandle bank = new BOOnlineBoDetailDBHandle(configuration);

                        var banks = (bank.bankNameInfo().ToList());

                        return Ok(new
                        {
                            success = true,
                            message = "Account Holder Information Successfully Updated.",
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

        [HttpPost("boAccountHolderBranchTypeAC")]
        public IActionResult boAccountHolderBranchTypeAC([FromBody] BOAccountHolderBranchTypeAC boModel)
        {
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
                                    ? (object)DBNull.Value: boModel.RefarralCode);

                            command.ExecuteNonQuery();
                        }
                        // 3. Get FIRSTNAME and LASTNAME
                        string nameQuery = @"SELECT [FIRSTNAME], [LASTNAME] FROM dbo.T_ONLINE_BO_REG
                        WHERE [TRACKINGNO] = @TRACKINGNO";

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
                            message = "Account Holder Branch Information Updated !!",
                            firstName = firstName, 
                            lastName = lastName,
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

        public class BOAccountHolderBranchTypeAC
        {
            public string TRACKINGNO { get; set; }
            public string ToC { get; set; }
            public string DesireBranch { get; set; }
            public string RefarralCode { get; set; }
        }

    }
}
