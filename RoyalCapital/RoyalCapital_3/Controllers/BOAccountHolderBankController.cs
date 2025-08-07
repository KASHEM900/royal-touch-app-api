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
    public class BOAccountHolderBankController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public BOAccountHolderBankController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpPost("boAccountHolderBank")]
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
                    BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(configuration);
                    bool isSuccess = DB.AddBankDetail(bbomodel);

                    if (isSuccess)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Account Holder Bank Information Successfully Updated.",
                            //accHolders = accHolders,
                            //titles = titles
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

        [HttpPost("GetBankNameList")]
        public IActionResult GetBankNameList()
        {
            BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(configuration);
            var banklist = DB.bankNameInfo().ToList();

            return Ok(new
            {
                success = true,
                banklist = banklist,
            });
        }

        [HttpPost("GetBankDistrictList")]
        public IActionResult GetBankDistrictList([FromQuery] string bankName)
        {
            BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(configuration);
            var bankDistrictList = DB.bankDistrictInfo(bankName).ToList();
            return Ok(new
            {
                success = true,
                bankDistrictList = bankDistrictList
            });
        }

        [HttpPost("GetBankBranchList")]
        public IActionResult GetBankBranchList([FromQuery] string bankName, [FromQuery] string districtName)
        {
            BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(configuration);
            var bankBranchList = DB.bankbranchInfo(bankName, districtName).ToList();
            return Ok(new
            {
                success = true,
                bankBranchList = bankBranchList
            });
        }

        [HttpPost("GetBankRoutingList")]
        public IActionResult GetBankRoutingList([FromQuery] string bankName, [FromQuery] string districtName, [FromQuery] string bankBranch)
        {
            BOOnlineBoDetailDBHandle DB = new BOOnlineBoDetailDBHandle(configuration);
            var bankRoutingList = DB.bankroutingInfo(bankName, districtName, bankBranch).ToList();
            return Ok(new
            {
                success = true,
                bankRoutingList = bankRoutingList
            });
        }

    }
}
