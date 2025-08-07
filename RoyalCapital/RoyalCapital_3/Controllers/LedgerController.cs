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
    public class LedgerController : ControllerBase
    {
        double opbal1, bal;

        private readonly IConfiguration configuration;
        public LedgerController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("")]
        public IActionResult GetLedger(string aCode, string fromDate, string toDate, string key)
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
                #region Header data getting=================================================================================
                DataSet DS = new DataSet();
                DataTable DT = DS.Tables.Add();
                DT.Columns.Add("AccHolderName");
                DT.Columns.Add("FathersName");
                DT.Columns.Add("MothersName");
                DT.Columns.Add("AccNo");
                DT.Columns.Add("BoId");
                DT.Columns.Add("Address");

                oConnection.Open();

                string opbal = "SELECT acode,aname,boid, addr1,addr2,city,faname,moname FROM T_CLIENT WHERE acode='" + aCode + "'";
                SqlCommand oCommand = new SqlCommand(opbal, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;

                foreach (DataRow dr in oTable.Rows)
                {
                    r = DT.NewRow();

                    r["AccHolderName"] = dr["aname"].ToString();
                    r["FathersName"] = dr["faname"].ToString();
                    r["MothersName"] = dr["moname"].ToString();
                    r["AccNo"] = dr["acode"].ToString();
                    r["BoId"] = dr["boid"].ToString();
                    r["Address"] = dr["addr1"].ToString().Trim() + ", " + dr["addr2"].ToString().Trim() + ", " + dr["city"].ToString().Trim();
                    DT.Rows.Add(r);
                }
                #endregion


                #region Bal getter=================================================================================
                oConnection.Open();
                string opbal2 = "SELECT sum((case a.b_or_s  when '1' then -a.amount when '2' then a.amount when '3' then a.amount when '4' then -a.amount end) -a.commsn) as opbal  from t_sh a where a.acode ='" + aCode + "' AND a.tdate<'" + fDate + "'";
                SqlCommand oCommand2 = new SqlCommand(opbal2, oConnection);

                DataTable oTable2 = new DataTable();
                oTable2.Load(oCommand2.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr2 in oTable2.Rows)
                {
                    opbal1 = Convert.ToDouble(dr2["opbal"].ToString());
                }
                #endregion


                #region Main body data=================================================================================
                bal = opbal1;

                DataSet DS3 = new DataSet();
                DataTable DT3 = DS3.Tables.Add("ledger");
                DT3.Columns.Add("Date");
                DT3.Columns.Add("Type");
                DT3.Columns.Add("Instrument");
                DT3.Columns.Add("Quantity");
                DT3.Columns.Add("Rate");
                DT3.Columns.Add("Debit");
                DT3.Columns.Add("Credit");
                DT3.Columns.Add("commission");
                DT3.Columns.Add("Balance");

                oConnection.Open();
                string opbal3 = "SELECT a.vno as vno,a.tdate AS tdate,(case a.b_or_s when '1' then 'Buy' when '2' then 'Sale' when '3' then 'Receipt' when '4' then 'Payment' end) as  type,(case a.tran_type when 'T' then RTRIM(a.narr)+' '+rtrim(ISNULL(A.DOC_NO, '')) + (case when len(a.chqno) > 0 then ' [' +rtrim(a.chqno)+' ]' else '' end) when 'S' then b.firmsnm1 end) as narr,sum(a.quantity) as quantity,(CASE sum(a.quantity) WHEN 0 THEN 0 ELSE (SUM(a.amount)/sum(a.quantity)) END) as rate,sum(case a.b_or_s when '1' then a.amount when '2' then 0 when '3' then 0 when '4' then a.amount end) as debit, sum(case a.b_or_s when '1' then 0 when '2' then a.amount when '3' then a.amount when '4' then 0 end) as credit,sum(a.commsn) as commission, sum((case a.b_or_s  when '1' then -a.amount when '2' then a.amount when '3' then a.amount when '4' then -a.amount end) -a.commsn)  as balance,a.b_or_s,a.ttype FROM t_sh a, t_firms b WHERE a.firmscd=b.firmscd AND a.acode ='" + aCode + "' AND a.tdate BETWEEN '" + fDate + "' AND '" + tDate + "' group by a.acode, a.tdate, b.firmsnm1, a.b_or_s, a.narr, a.vno, a.tran_type, A.DOC_NO, a.ttype, a.chqno ORDER BY a.tdate,a.vno, b.firmsnm1,a.b_or_s";
                SqlCommand oCommand3 = new SqlCommand(opbal3, oConnection);

                DataTable oTable3 = new DataTable();
                oTable3.Load(oCommand3.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r3;

                foreach (DataRow dr3 in oTable3.Rows)
                {
                    r3 = DT3.NewRow();

                    //opbal1 = Convert.ToDouble(dr["opbal"].ToString());
                    //r["Voucher"] = dr["vno"].ToString();
                    r3["Date"] = string.Format("{0:dd-MMM-yyyy}", dr3["tdate"]);
                    r3["Type"] = dr3["type"].ToString();
                    r3["Instrument"] = dr3["narr"].ToString().Trim();
                    r3["Quantity"] = dr3["quantity"].ToString();
                    r3["Rate"] = string.Format("{0:#,#0.00}", dr3["rate"]);
                    r3["Debit"] = string.Format("{0:#,#0.00}", dr3["debit"]);
                    r3["Credit"] = string.Format("{0:#,#0.00}", dr3["credit"]);
                    r3["commission"] = string.Format("{0:#,#0.00}", dr3["commission"]);
                    bal = bal + Convert.ToDouble(dr3["balance"]);
                    r3["Balance"] = string.Format("{0:#,#0.00}", bal);

                    DT3.Rows.Add(r3);
                }

                List<LedgerMain> ledgerMainList = new List<LedgerMain>();

                for (int i = 0; i < DS3.Tables[0].Rows.Count; i++)
                {
                    ledgerMainList.Add(new LedgerMain(
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[4]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[5]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[6]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[7]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[8])
                    ));
                }
                #endregion

                Ledger ledger = new Ledger();

                ledger.AccHolderName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[0]).Trim();
                ledger.FathersName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[1]).Trim();
                ledger.MothersName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[2]).Trim();
                ledger.AccNo = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[3]).Trim();
                ledger.BoId = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[4]).Trim();
                ledger.Address = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[5]).Trim();

                ledger.LedgerMains = ledgerMainList;

                return Ok(ledger);
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
