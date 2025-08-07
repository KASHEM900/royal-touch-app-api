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
using RoyalCapital_3.BL;

namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrentLedgerBalanceController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public CurrentLedgerBalanceController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("")]
        public IActionResult GetLedgerBalance(string aCode, string key)
        {
            string decryptedData;
            string ledgerBalance = "";
            string secretKey = configuration["secret"];
            try
            {
                decryptedData = TripleDESImp.Decrypt(key.Trim(), true, secretKey);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = "invalid user" });
            }
            string[] subs = decryptedData.Split('+');

            string sisConn = configuration.GetConnectionString("SISROYALU");
            string rclConn = configuration.GetConnectionString("RCLWEB");

            if (!SecretKeyChecker.SectetKeyChecker(rclConn, subs[0], subs[1]))
            {
                return NotFound(new { error = "invalid user" });
            }

            try
            {
                SqlConnection sis_connection = new SqlConnection(sisConn);
                string qry_ldg = "SELECT ldgbal FROM T_Tkbal WHERE acode='" + aCode.ToString() + "'";
                sis_connection.Open();
                SqlCommand comm_ldg = new SqlCommand(qry_ldg, sis_connection);
                DataTable tbl_ldg = new DataTable();
                tbl_ldg.Load(comm_ldg.ExecuteReader(CommandBehavior.CloseConnection));
                foreach (DataRow dr in tbl_ldg.Rows)
                {
                    ledgerBalance = dr["ldgbal"].ToString();
                }
                double ldg_bal = (ledgerBalance.ToString() == "") ? 0 : Convert.ToDouble(ledgerBalance.ToString());
                return Ok(new
                {
                    ledger_balance = ldg_bal,
                });
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
