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
    public class CDBLBulkReceiveController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public CDBLBulkReceiveController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("")]
        public IActionResult GetTransactionStatus(string mobileNo, string key)
        {
            string decryptedData;
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
                DataSet DS = new DataSet();
                DataTable DT = DS.Tables.Add("RCODE");
                DT.Columns.Add("SL");
                DT.Columns.Add("RCode");
                DT.Columns.Add("Name");

                SqlConnection conn_ipo = new SqlConnection(rclConn);
                conn_ipo.Open();

                string all_code_qry = "SELECT RCODE, fhname from RCLWEB.[RCLWEB].[dbo].T_CLIENT1 WHERE mobile='" + mobileNo.ToString() + "' AND boStatus='Active' AND RCODE NOT IN (SELECT RCODE FROM T_CDBL_CHARGE WHERE [YEAR]=DATEPART(YEAR,GETDATE())+1)";

                SqlCommand oCommand = new SqlCommand(all_code_qry, conn_ipo);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;

                int counter = 0;
                foreach (DataRow dr in oTable.Rows)
                {
                    r = DT.NewRow();

                    r["SL"] = ++counter;
                    r["RCode"] = dr["RCODE"].ToString();
                    r["Name"] = dr["fhName"].ToString();

                    DT.Rows.Add(r);
                }

                List<GetCDBLBulkReceive> getCDBLBulkReceiveList = new List<GetCDBLBulkReceive>();

                for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                {
                    getCDBLBulkReceiveList.Add(new GetCDBLBulkReceive(
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[2])
                    ));
                }

                return Ok(getCDBLBulkReceiveList);

            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }

        [HttpPost("")]
        public IActionResult SetTransactionStatus([FromBody] CDBLBulkReceiveList cDBLBulkReceiveList, string key)
        {
            string decryptedData;
            int unit_price = 450;

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
                SqlConnection conn_ipo = new SqlConnection(rclConn);
                conn_ipo.Open();

                string mr_no = "P" + Guid.NewGuid().ToString("N").Substring(0, 16);

                string pop = "";

                foreach (CDBLBulkReceive cDBLBulkReceive in cDBLBulkReceiveList.CDBLBulkReceive)
                {
                    string qry = "INSERT INTO [T_CDBL_CHARGE_ONLINE] ([MR_NO] ,[RCODE] ,[YEAR] ,[FISCAL] ,[AMOUNT] ,[BRANCHCODE] ,[DATE] ,[NOTE] ,[fis] ,[tamnt] ,[NAME] , [transaction_status]) VALUES (@MR_NO, @RCODE, @year, @fiscal, @amount, @branch_code, GETDATE(), '', @fis, @tamnt, @name, @tran_status)";
                    SqlCommand oCommand = new SqlCommand(qry, conn_ipo);
                    oCommand.Parameters.AddWithValue("@MR_NO", mr_no);
                    oCommand.Parameters.AddWithValue("@RCODE", cDBLBulkReceive.RCode);
                    oCommand.Parameters.AddWithValue("@year", cDBLBulkReceive.Year);
                    oCommand.Parameters.AddWithValue("@fiscal", "July " + cDBLBulkReceive.StartYear + " - June " + cDBLBulkReceive.EndYear);
                    oCommand.Parameters.AddWithValue("@amount", unit_price);
                    oCommand.Parameters.AddWithValue("@branch_code", 1);
                    oCommand.Parameters.AddWithValue("@fis", "July " + cDBLBulkReceive.StartYear + " to June " + cDBLBulkReceive.EndYear);
                    oCommand.Parameters.AddWithValue("@tamnt", cDBLBulkReceive.TotalAmount);
                    oCommand.Parameters.AddWithValue("@name", cDBLBulkReceive.Name);
                    oCommand.Parameters.AddWithValue("@tran_status", "Fail");

                    oCommand.ExecuteNonQuery();
                }

                return Ok(new
                {
                    mr_no = mr_no,
                });
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }

        [HttpPut("")]
        public IActionResult UpdateTransactionStatus([FromBody] UpdateCDBLBulkReceive updateCDBLBulkReceive, string rCode, string key)
        {
            string decryptedData;
            string transaction_id = "";
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
                SqlConnection conn_ipo = new SqlConnection(rclConn);
                conn_ipo.Open();

                string bank_tran_id = "SELECT bank_tran_id from RCLWEB.[RCLWEB].[dbo].T_SSL_IPN WHERE value_a = '" + rCode.ToString() + "' AND tran_date = (SELECT MAX(CONVERT(DATE, tran_date, 120)) FROM T_SSL_IPN WHERE value_a = '" + rCode.ToString() + "')";
                SqlCommand oCommand2 = new SqlCommand(bank_tran_id, conn_ipo); 
                DataTable oTable2 = new DataTable();
                oTable2.Load(oCommand2.ExecuteReader());
                foreach (DataRow dr in oTable2.Rows)
                {
                    transaction_id = dr["bank_tran_id"].ToString();
                }

                string qry;
                if (transaction_id != "")
                {
                    qry = "UPDATE T_CDBL_CHARGE_ONLINE SET transaction_status='Success', transaction_id='" + transaction_id + "'   WHERE MR_NO='" + updateCDBLBulkReceive.mr_no + "'";
                }
                else
                {
                    qry = "UPDATE T_CDBL_CHARGE_ONLINE SET transaction_status='Success', transaction_id='" + "NULL" + "'   WHERE MR_NO='" + updateCDBLBulkReceive.mr_no + "'";
                }
                SqlCommand oCommand = new SqlCommand(qry, conn_ipo);
                
                oCommand.ExecuteNonQuery();

                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
