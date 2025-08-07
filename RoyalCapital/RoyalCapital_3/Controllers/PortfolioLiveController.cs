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
    public class PortfolioLiveController : ControllerBase
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
        public PortfolioLiveController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("")]
        public IActionResult GetPortfolio(string aCode, string key)
        {
            string decryptedData;
            double mktValLive = 0.00;
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
            SqlConnection oConnectionLive = new SqlConnection(rclConn);

            try
            {
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
                oConnectionLive.Open();

                SqlCommand oCommond2 = new SqlCommand("inv_pfs_td", oConnection);
                oCommond2.CommandType = CommandType.StoredProcedure;

                oCommond2.Parameters.Add("@acode", SqlDbType.Char).Value = aCode.ToString();

                DataTable oTable2 = new DataTable();
                oTable2.Load(oCommond2.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r2;

                int count = 0;

                foreach (DataRow dr in oTable2.Rows)
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


                        string queryLive = "SELECT MKISTAT_INSTRUMENT_CODE,MKISTAT_PUB_LAST_TRADED_PRICE FROM RCLWEB.[RCLWEB].[dbo].T_DSE_MKISTAT WHERE MKISTAT_INSTRUMENT_CODE='" + dr["firmsnm1"].ToString().Trim() + "'";

                        DataTable oTableLive = new DataTable();
                        SqlCommand oCommondLive = new SqlCommand(queryLive, oConnectionLive);
                        oTableLive.Load(oCommondLive.ExecuteReader());

                        foreach (DataRow drLive in oTableLive.Rows)
                        {
                            r2["Market Rate"] = string.Format("{0:#,#.00}", drLive["MKISTAT_PUB_LAST_TRADED_PRICE"]);
                            r2["Market Value"] = string.Format("{0:#,#.00}", Convert.ToDouble(dr["quantity"].ToString()) * Convert.ToDouble(drLive["MKISTAT_PUB_LAST_TRADED_PRICE"].ToString()));
                            r2["Gain/Loss"] = string.Format("{0:#,#.00}", (Convert.ToDouble(r2["Market Value"]) - Convert.ToDouble(dr["amount"])));

                            mktValLive += Convert.ToDouble(r2["Market Value"]);
                        }

                        r2["Percentage"] = string.Format("{0:#,#0.00}", (((Convert.ToDouble(dr["mktamt"]) / Convert.ToDouble(dr["amount"])) * 100) - 100));

                        r2["Margin"] = dr["mrgsec"].ToString();

                        if (Convert.ToBoolean(dr["mrgsec"]))
                        {
                            mtc += Convert.ToDouble(dr["amount"]);
                            mmv += Convert.ToDouble(r2["Market Value"]);
                            mug += Convert.ToDouble(r2["Gain/Loss"]);
                        }
                        else
                        {
                            nmtc += Convert.ToDouble(dr["amount"]);
                            nmmv += Convert.ToDouble(r2["Market Value"]);
                            nmug += Convert.ToDouble(r2["Gain/Loss"]);
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
                string queryMarketValue = "SELECT SUM(CONVERT(decimal(20,2),totcost)) AS 'Total Cost',SUM(CONVERT(decimal(20,2),(a.totqty*c.iclose))) AS 'Market Value',SUM(CONVERT(decimal(20,2),((a.totqty*c.iclose)))) AS 'Gain/Loss' FROM T_PF a,T_idxshrp c WHERE a.acode='" + aCode.ToString() + "' AND a.firmscd=c.firmscd AND c.tdate=(SELECT MAX(tdate) FROM T_idxshrp)";
                SqlCommand oCommand8 = new SqlCommand(queryMarketValue, oConnection);

                DataTable oTable8 = new DataTable();
                oTable8.Load(oCommand8.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr8 in oTable8.Rows)
                {
                    totalBuyCost = dr8["Total Cost"].ToString();
                    marketVal = mktValLive.ToString();
                    equity = (Convert.ToDouble(marketVal) + Convert.ToDouble(ledger) - Convert.ToDouble(accruedCharge)).ToString();
                    unrealise = (mktValLive - Convert.ToDouble(dr8["Total Cost"].ToString())).ToString();
                    totalCapital = (Convert.ToDouble(unrealise) + Convert.ToDouble(realiseGainLoss)).ToString();
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


                PortfolioLive portfolioLive = new PortfolioLive();

                portfolioLive.PortfolioMains = portfolioMainList;

                portfolioLive.RealisedGainLoss = realiseGainLoss;
                portfolioLive.AccredCharges = accruedCharge;
                portfolioLive.MaturedBalance = matured;
                portfolioLive.LedgerBalance = ledger;
                portfolioLive.SaleReceivable = saleRec;
                portfolioLive.TotalBuyCost = totalBuyCost;
                portfolioLive.MarketValue = marketVal;
                portfolioLive.Equity = equity;
                portfolioLive.UnRealisedCapitalGainLoss = unrealise;
                portfolioLive.TotalCapitalGainLoss = totalCapital;
                portfolioLive.ChargesAndFees = "0";
                portfolioLive.MarginableSubTotal = mTA;
                portfolioLive.NonMarginableSubTotal = nMTA;
                portfolioLive.GrandSubTotal = gTA;


                return Ok(portfolioLive);
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
