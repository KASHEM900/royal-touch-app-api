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
    public class ReceiptPaymentController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public ReceiptPaymentController(IConfiguration config)
        {
            configuration = config;
        }

        public IActionResult GetRecPay(string aCode, string fromDate, string toDate, string key)
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


            DateTime fromDateD = DateTime.Parse(fromDate, null);
            DateTime toDateD = DateTime.Parse(toDate, null);

            string fDate = fromDateD.ToString("yyyy-MM-dd HH:mm:ss");
            string tDate = toDateD.ToString("yyyy-MM-dd HH:mm:ss");
            string conn = configuration.GetConnectionString("SISROYALU");
            SqlConnection oConnection = new SqlConnection(conn);

            try
            {
                DataSet DS = new DataSet();
                DataTable DT = DS.Tables.Add("recpay");
                DT.Columns.Add("Date");
                DT.Columns.Add("Voucher No");
                DT.Columns.Add("Deposit");
                DT.Columns.Add("Withdraw");


                oConnection.Open();
                string query = "SELECT vno AS 'Voucher No',tdate AS 'Date',(case b_or_s when 3 then amount end) AS Deposit, (case b_or_s when 4 then amount end) AS Withdraw FROM T_SH WHERE acode='" + aCode.Trim() + "' AND tran_type='T' AND tdate BETWEEN '" + fDate + "' AND '" + tDate + "' ORDER BY tdate ";

                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;
                foreach (DataRow dr in oTable.Rows)
                {
                    r = DT.NewRow();

                    r["Date"] = dr["Date"].ToString();
                    r["Voucher No"] = dr["Voucher No"].ToString();
                    r["Deposit"] = dr["Deposit"].ToString();
                    r["Withdraw"] = dr["Withdraw"].ToString();


                    DT.Rows.Add(r);
                }

                List<ReceiptPayment> receiptPaymentList = new List<ReceiptPayment>();

                for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                {
                    receiptPaymentList.Add(new ReceiptPayment(
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[3])
                    ));
                }

                return Ok(receiptPaymentList);
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
