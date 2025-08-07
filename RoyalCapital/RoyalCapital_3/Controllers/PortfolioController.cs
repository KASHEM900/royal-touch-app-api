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
using RoyalCapital_3.BL;


namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        string realiseGainLoss;
        string accruedCharge;
        string matured;
        string ledger;
        string saleRec;
        string totalBuyCost;
        string marketVal;
        string equity;
        string unrealise;
        string totalCapital;

        private readonly IConfiguration configuration;
        public PortfolioController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("")]
        public IActionResult GetPortfolio(string aCode, string key)
        {
            string decryptedData;
            double mtc = 0;
            double mmv = 0;
            double mug = 0;
            double nmtc = 0;
            double nmmv = 0;
            double nmug = 0;
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

            SqlConnection oConnection = new SqlConnection(conn);

            try
            {
                #region PORTFOLIO Initial=================================================================================

                DataSet DS = new DataSet();
                DataTable DT = DS.Tables.Add();
                DT.Columns.Add("AccHolderName");
                DT.Columns.Add("FathersName");
                DT.Columns.Add("MothersName");
                DT.Columns.Add("AccNo");
                DT.Columns.Add("BoId");
                DT.Columns.Add("Address");

                oConnection.Open();

                SqlCommand oCommond = new SqlCommand("SELECT acode,aname,boid,addr1,addr2,city,faname,moname FROM T_CLIENT WHERE acode=@acode", oConnection);
                oCommond.Parameters.Add("@acode", SqlDbType.Char).Value = aCode.ToString();

                DataTable oTable = new DataTable();
                oTable.Load(oCommond.ExecuteReader(CommandBehavior.CloseConnection));

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

                # region PORTFOLIO MAIN=================================================================================

                oConnection = new SqlConnection(conn);

                DataSet DS2 = new DataSet();
                DataTable DT2 = DS2.Tables.Add();
                DT2.Columns.Add("SL#");
                DT2.Columns.Add("Instrument");
                DT2.Columns.Add("Total");
                DT2.Columns.Add("Free");
                DT2.Columns.Add("Lock");
                DT2.Columns.Add("Avg Price");
                DT2.Columns.Add("Total Cost");
                DT2.Columns.Add("Market Rate");
                DT2.Columns.Add("Market Value");
                DT2.Columns.Add("Gain/Loss");
                DT2.Columns.Add("Percentage");
                DT2.Columns.Add("Margin");

                oConnection.Open();

                SqlCommand oCommond2 = new SqlCommand("inv_pfs_td", oConnection);
                oCommond2.CommandType = CommandType.StoredProcedure;

                oCommond2.Parameters.Add("@acode", SqlDbType.Char).Value = aCode.ToString();

                DataTable oTable2 = new DataTable();
                oTable2.Load(oCommond2.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r2;

                int count = 0;

                foreach (
                    DataRow dr in oTable2.Rows)
                {
                    if (Convert.ToDouble(dr["grp"].ToString()) == 0)
                    {
                        count++;
                        r2 = DT2.NewRow();
                        r2["SL#"] = count.ToString();
                        r2["Instrument"] = dr["firmsnm1"].ToString();
                        r2["Total"] = string.Format("{0:#,#}", dr["quantity"]);
                        r2["Free"] = string.Format("{0:#,#}", dr["slbqty"]);
                        r2["Lock"] = string.Format("{0:#,#}", dr["Pldqty"]);
                        r2["Avg Price"] = string.Format("{0:#,#0.00}", dr["rate"]);
                        r2["Total Cost"] = string.Format("{0:#,#0.00}", dr["amount"]);
                        r2["Market Rate"] = string.Format("{0:#,#0.00}", dr["mktrt"]);
                        r2["Market Value"] = string.Format("{0:#,#0.00}", dr["mktamt"]);
                        r2["Gain/Loss"] = string.Format("{0:#,#0.00}", (Convert.ToDouble(dr["mktamt"]) - Convert.ToDouble(dr["amount"])));
                        r2["Percentage"] = string.Format("{0:#,#0.00}", (((Convert.ToDouble(dr["mktamt"]) / Convert.ToDouble(dr["amount"])) * 100)-100));
                        r2["Margin"] = dr["mrgsec"].ToString();
                        if (Convert.ToBoolean(dr["mrgsec"]))
                        {
                            mtc += Convert.ToDouble(dr["amount"]);
                            mmv += Convert.ToDouble(dr["mktamt"]);
                            mug += Convert.ToDouble(dr["mktamt"]) - Convert.ToDouble(dr["amount"]);
                        }
                        else
                        {
                            nmtc += Convert.ToDouble(dr["amount"]);
                            nmmv += Convert.ToDouble(dr["mktamt"]);
                            nmug += Convert.ToDouble(dr["mktamt"]) - Convert.ToDouble(dr["amount"]);
                        }

                        DT2.Rows.Add(r2);
                    }
                }

                List<PortfolioMain> portfolioMainList = new List<PortfolioMain>();

                for (int i = 0; i < DS2.Tables[0].Rows.Count; i++)
                {
                    portfolioMainList.Add(new PortfolioMain(
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[4]),
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[5]),
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[6]),
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[7]),
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[8]),
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[9]),
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[10]),
                    Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[11])
                    ));
                }

                #endregion

                #region PORTFOLIO PENDING IPO APP=================================================================================

                oConnection = new SqlConnection(conn);
                int flag = 0;
                int findFlag = 0;

                DataSet DS3 = new DataSet();
                DataTable DT3 = DS3.Tables.Add();
                DT3.Columns.Add("SL#");
                DT3.Columns.Add("IPO NAME");
                DT3.Columns.Add("IPO AMOUNT");
                DT3.Columns.Add("Applied Date");

                oConnection.Open();

                SqlCommand oCommond3 = new SqlCommand("SELECT IPO_NAME,IPO_AMOUNT,CONVERT(VARCHAR(10),ENTRY_DATE,101) 'Applied Date' FROM [RCLWEB].RCLWEB.dbo.T_IPO_REC WHERE RCODE='" + aCode.ToString() + "' AND IPO_RESULT='' UNION ALL SELECT IPO_NAME,IPO_AMOUNT,CONVERT(VARCHAR(10),ENTRY_DATE,101) 'Applied Date' FROM [RCLWEB].RCLWEB.dbo.T_IPO_REC_MASTER WHERE RCODE='" + aCode.ToString() + "' AND IPO_RESULT=''", oConnection);

                DataTable oTable3 = new DataTable();
                oTable3.Load(oCommond3.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r3;

                foreach (DataRow dr in oTable3.Rows)
                {
                    findFlag = 1;
                    flag++;
                    r3 = DT3.NewRow();

                    r3["SL#"] = flag.ToString();
                    r3["IPO NAME"] = dr["IPO_NAME"].ToString();
                    r3["IPO AMOUNT"] = string.Format("{0:#,#0.00}", dr["IPO_AMOUNT"]);
                    r3["Applied Date"] = string.Format("{0:dd-MMM-yyyy}", dr["Applied Date"]);

                    DT3.Rows.Add(r3);

                }
                if (findFlag == 0)
                {
                    r3 = DT3.NewRow();

                    r3["SL#"] = "0";
                    r3["IPO NAME"] = "";
                    r3["IPO AMOUNT"] = string.Format("{0:#,#0.00}", 0);
                    r3["Applied Date"] = "";

                    DT3.Rows.Add(r3);
                }
                findFlag = 0;

                List<PortfolioPendingIPOApp> portfolioPendingIPOAppList = new List<PortfolioPendingIPOApp>();

                for (int i = 0; i < DS3.Tables[0].Rows.Count; i++)
                {
                    portfolioPendingIPOAppList.Add(new PortfolioPendingIPOApp(
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[3])
                    ));
                }

                #endregion

                #region Realise gain/loss=================================================================================
                oConnection = new SqlConnection(conn);
                oConnection.Open();
                string query = "SELECT rgl FROM T_RGL WHERE acode='" + aCode.ToString() + "'";
                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable4 = new DataTable();
                oTable4.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr4 in oTable4.Rows)
                {
                    realiseGainLoss = string.Format("{0:#,#0.00}", dr4["rgl"]);
                }
                #endregion

                #region Accrued Charge =================================================================================
                oConnection = new SqlConnection(conn);
                oConnection.Open();
                string queryAccuredCharge = "SELECT intamt FROM T_intsum WHERE acode='" + aCode.ToString() + "'";
                SqlCommand oCommand5 = new SqlCommand(queryAccuredCharge, oConnection);

                DataTable oTable5 = new DataTable();
                oTable5.Load(oCommand5.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr5 in oTable5.Rows)
                {
                    accruedCharge = string.Format("{0:#,#0.00}", String.IsNullOrEmpty(dr5["intamt"].ToString()) ? "0" : dr5["intamt"]);
                }
                #endregion

                #region Matured and Ledger =================================================================================
                oConnection = new SqlConnection(conn);
                oConnection.Open();
                string queryMaturedAndLedger = "SELECT matbalP,ldgbal FROM T_Tkbal WHERE acode='" + aCode.ToString() + "'";
                SqlCommand oCommand6 = new SqlCommand(queryMaturedAndLedger, oConnection);

                DataTable oTable6 = new DataTable();
                oTable6.Load(oCommand6.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr6 in oTable6.Rows)
                {
                    matured = string.Format("{0:#,#0.00}", dr6["matbalP"]);
                    ledger = string.Format("{0:#,#0.00}", dr6["ldgbal"]);
                }
                #endregion

                #region Sale Rec =================================================================================
                oConnection = new SqlConnection(conn);
                oConnection.Open();
                string querySaleRec = "SELECT BAL FROM T_SRTD WHERE ACODE='" + aCode.ToString() + "'";
                SqlCommand oCommand7 = new SqlCommand(querySaleRec, oConnection);

                DataTable oTable7 = new DataTable();
                oTable7.Load(oCommand7.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr7 in oTable7.Rows)
                {
                    saleRec = string.Format("{0:#,#0.00}", dr7["BAL"]);

                    if (Convert.ToDouble(saleRec) != 0.00)
                    {
                        ledger = string.Format("{0:#,#0.00}", (Convert.ToDouble(ledger)));
                    }
                    else
                    {
                        ledger = string.Format("{0:#,#0.00}", (Convert.ToDouble(ledger) + Convert.ToDouble(dr7["BAL"].ToString())));
                    }
                }
                #endregion

                #region Market Value =================================================================================
                oConnection = new SqlConnection(conn);
                oConnection.Open();
                string queryMarketValue = "SELECT SUM(CONVERT(decimal(20,2),totcost)) AS 'Total Cost',SUM(CONVERT(decimal(20,2),(a.totqty*c.iclose))) AS 'Market Value',SUM(CONVERT(decimal(20,2),((a.totqty*c.iclose)-(a.totcost)))) AS 'Gain/Loss' FROM T_PF a,T_idxshrp c WHERE a.acode='" + aCode.ToString() + "' AND a.firmscd=c.firmscd AND c.tdate=(SELECT MAX(tdate) FROM T_idxshrp where firmscd= c.firmscd)";
                SqlCommand oCommand8 = new SqlCommand(queryMarketValue, oConnection);

                DataTable oTable8 = new DataTable();
                oTable8.Load(oCommand8.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr8 in oTable8.Rows)
                {
                    totalBuyCost = string.Format("{0:#,#0.00}", dr8["Total Cost"]);
                    marketVal = string.Format("{0:#,#0.00}", dr8["Market Value"]);
                    equity = string.Format("{0:#,#0.00}", (Convert.ToDouble(dr8["Market Value"].ToString()) + Convert.ToDouble(ledger) - Convert.ToDouble(accruedCharge)));
                    unrealise = string.Format("{0:#,#0.00}", dr8["Gain/Loss"]);
                    totalCapital = string.Format("{0:#,#0.00}", (Convert.ToDouble(unrealise) + Convert.ToDouble(realiseGainLoss)));
                }
                #endregion

                #region Total values =================================================================================
                TotalAmount mTA = new TotalAmount();
                mTA.SubTotalCost = mtc;
                mTA.SubMarketValue = mmv;
                mTA.SubRealisedGainLoss = mug;

                TotalAmount nMTA = new TotalAmount();
                nMTA.SubTotalCost = nmtc;
                nMTA.SubMarketValue = nmmv;
                nMTA.SubRealisedGainLoss = nmug;

                TotalAmount gTA = new TotalAmount();
                gTA.SubTotalCost = mtc + nmtc;
                gTA.SubMarketValue = mmv + nmmv;
                gTA.SubRealisedGainLoss = mug + nmug;
                #endregion

                Portfolio portfolio = new Portfolio();

                portfolio.AccHolderName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[0]).Trim();
                portfolio.FathersName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[1]).Trim();
                portfolio.MothersName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[2]).Trim();
                portfolio.AccNo = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[3]).Trim();
                portfolio.BoId = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[4]).Trim();
                portfolio.Address = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[5]).Trim();

                portfolio.PortfolioMains = portfolioMainList;

                portfolio.PortfolioPendingIPOApps = portfolioPendingIPOAppList;

                portfolio.RealisedGainLoss = realiseGainLoss;
                portfolio.AccredCharges = accruedCharge;
                portfolio.MaturedBalance = matured;
                portfolio.LedgerBalance = ledger;
                portfolio.SaleReceivable = saleRec;
                portfolio.TotalBuyCost = totalBuyCost;
                portfolio.MarketValue = marketVal;
                portfolio.Equity = equity;
                portfolio.UnRealisedCapitalGainLoss = unrealise;
                portfolio.TotalCapitalGainLoss = totalCapital;
                portfolio.ChargesAndFees = "0";
                portfolio.MarginableSubTotal = mTA;
                portfolio.NonMarginableSubTotal = nMTA;
                portfolio.GrandSubTotal = gTA;

                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
