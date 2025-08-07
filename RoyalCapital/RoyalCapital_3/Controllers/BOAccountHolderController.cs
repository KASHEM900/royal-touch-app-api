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

using System.Text;

using RoyalCapital_3.BL;

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

        
    }
}
