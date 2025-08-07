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
    public class ConfirmationController : ControllerBase
    {

        private readonly IConfiguration configuration;
        public ConfirmationController(IConfiguration config)
        {
            configuration = config;
        }

        double NAOT = 0;
        string LedgerBalBeforeTrading;
        string Receipt;
        string Payment;
        string NetAmtOfTrading;
        string ColsingBalOfDay;
    
        public IActionResult GetConfirmation(string aCode, string date, string key)
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

            string conn = configuration.GetConnectionString("SISROYALU");

            DateTime dateT = DateTime.Parse(date, null);

            string fDate = dateT.ToString("yyyy-MM-dd HH:mm:ss");

            SqlConnection oConnection = new SqlConnection(conn);

            try
            {
                #region Header data getting=================================================================================
                DataSet DS = new DataSet();
                DataTable DT = DS.Tables.Add("confirmation");
                DT.Columns.Add("Exch");
                DT.Columns.Add("CODE");
                DT.Columns.Add("Instrument");
                DT.Columns.Add("Buy Qty");
                DT.Columns.Add("Buy Rate");
                DT.Columns.Add("Buy Amt");
                DT.Columns.Add("Sale Qty");
                DT.Columns.Add("Sale Rate");
                DT.Columns.Add("Sale Amt");
                DT.Columns.Add("Bal Qty");
                DT.Columns.Add("Com (B+S)");
                DT.Columns.Add("Balance");
                DT.Columns.Add("NAOT");

                string query = "SELECT acode,cocd,firmsnm1,SUM((case when b_or_s ='1' then quantity else 0.00 end)) AS buyQnty,  SUM((case when b_or_s='1' then amount else 0.00 end)) AS Bamnt,SUM((case when b_or_s ='2' then quantity else 0.00 end)) AS saleQnty, SUM((case when b_or_s='2' then amount else 0.00 end)) AS Samnt,(SUM((case when b_or_s ='1' then quantity else 0.00 end))-SUM((case when b_or_s ='2' then quantity else 0.00 end))) AS 'Bal Qnty', SUM(commsn) AS commsn,((SUM((case when b_or_s='2' then amount else 0.00 end)) - SUM((case when b_or_s='1' then amount else 0.00 end)))-SUM(commsn)) AS balance FROM T_Sh WHERE acode='" + aCode.Trim() + "' AND tdate = '" + fDate + "' GROUP BY acode,firmsnm1,cocd";


                oConnection.Open();
                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;

                foreach (DataRow dr in oTable.Rows)
                {
                    r = DT.NewRow();

                    if (dr["cocd"].ToString() == "01")
                    {
                        r["Exch"] = "DSE";
                    }
                    if (dr["cocd"].ToString() == "02")
                    {
                        r["Exch"] = "CSE";
                    }
                    r["CODE"] = dr["acode"].ToString();
                    r["Instrument"] = dr["firmsnm1"].ToString();

                    if (Convert.ToDouble(dr["buyQnty"].ToString()) != 0.00)
                    {
                        r["Buy Qty"] = string.Format("{0:#,0}", dr["buyQnty"]);
                        r["Buy Amt"] = string.Format("{0:#,0.00}", dr["Bamnt"]);
                        r["Buy Rate"] = string.Format("{0:#,0.00}", (Convert.ToDouble(r["Buy Amt"].ToString()) / Convert.ToDouble(r["Buy Qty"].ToString()))); //string.Format("{0:#,0.00}", dr["Brate"]);

                    }
                    if (Convert.ToDouble(dr["buyQnty"].ToString()) == 0.00)
                    {
                        r["Buy Qty"] = "0.00";
                        r["Buy Amt"] = "0.00";
                        r["Buy Rate"] = "0.00";
                    }

                    if (Convert.ToDouble(dr["saleQnty"].ToString()) != 0.00)
                    {
                        r["Sale Qty"] = string.Format("{0:#,0}", dr["saleQnty"]);
                        r["Sale Amt"] = string.Format("{0:#,0.00}", dr["Samnt"]);
                        r["Sale Rate"] = string.Format("{0:#,0.00}", (Convert.ToDouble(r["Sale Amt"].ToString()) / Convert.ToDouble(r["Sale Qty"].ToString()))); //string.Format("{0:#,0.00}", dr["Brate"]);

                    }
                    if (Convert.ToDouble(dr["saleQnty"].ToString()) == 0.00)
                    {
                        r["Sale Qty"] = "0.00";
                        r["Sale Amt"] = "0.00";
                        r["Sale Rate"] = "0.00";
                    }

                    r["Bal Qty"] = string.Format("{0:#,0}", dr["Bal Qnty"]);
                    r["Com (B+S)"] = string.Format("{0:#,0.00}", dr["commsn"]);
                    r["Balance"] = string.Format("{0:#,0.00}", dr["balance"]);
                    NAOT += Convert.ToDouble(r["Balance"].ToString());

                    DT.Rows.Add(r);
                }

                r = DT.NewRow();
                r["NAOT"] = NAOT.ToString();
                DT.Rows.Add(r);

                List<ConfirmationMain> confirmationMainList = new List<ConfirmationMain>();

                for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                {
                    confirmationMainList.Add(new ConfirmationMain(
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[4]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[5]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[6]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[7]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[8]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[9]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[10]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[11]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[12])
                    ));
                }
                #endregion


                #region Other Data=================================================================================
                oConnection.Open();

                SqlCommand oCommand2 = new SqlCommand("SP_CLIENT_BALANCE_SINGLE", oConnection);
                oCommand2.CommandType = CommandType.StoredProcedure;
                oCommand2.Parameters.Add("@TDATE", SqlDbType.DateTime).Value = fDate.Trim();
                oCommand2.Parameters.Add("@COCD", SqlDbType.NVarChar).Value = "00";
                oCommand2.Parameters.Add("@ACODE", SqlDbType.NVarChar).Value = aCode.Trim();

                DataTable oTable2 = new DataTable();
                oTable2.Load(oCommand2.ExecuteReader(CommandBehavior.CloseConnection));


                foreach (DataRow dr in oTable2.Rows)
                {
                    LedgerBalBeforeTrading = dr["prebal"].ToString();
                    Receipt = dr["recamt"].ToString();
                    Payment = dr["payamt"].ToString();
                    NetAmtOfTrading = NAOT.ToString();
                    ColsingBalOfDay = dr["todbal"].ToString();
                }
                #endregion


                Confirmation confirmation = new Confirmation();
                confirmation.ConfirmationMains = confirmationMainList;
                confirmation.LedgerBalBeforeTrading = LedgerBalBeforeTrading;
                confirmation.Receipt = Receipt;
                confirmation.Payment = Payment;
                confirmation.NetAmtOfTrading = NetAmtOfTrading;
                confirmation.ColsingBalOfDay = ColsingBalOfDay;

                return Ok(confirmation);
            }
            catch (Exception ex )
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
