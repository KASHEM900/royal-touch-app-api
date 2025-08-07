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
    public class PortfolioDetailsController : ControllerBase
    {
        int flag = 0;
        int findFlag = 0;
        double totalBCost = 0;
        double totalSCost = 0;
        double totalRG = 0;
        double totalBCost1 = 0;
        double totalSCost1 = 0;
        double totalRG1 = 0;
        double TotalBA = 0;
        double TotalBA1 = 0;
        double TotalMA = 0;
        double TotalMA1 = 0;
        double TotalUR = 0;
        double TotalUR1 = 0;

        double openingShareBal;
        double ledgerBal;
        double portfolioValueMarketPrice;
        double portfolioValueCostPric;
        double deposit;
        double withdrawnAmount;
        double charges;
        double netDeposit;
        double realisedCapitalGainLoss;

        string sopeningShareBal;
        string sledgerBal;
        string sportfolioValueMarketPrice;
        string slblPortfolioValueCostPric;
        string slblDeposit;
        string slblWithdrawnAmount;
        string slblCharges;
        string slblNetDeposit;
        string slblRealisedCapitalGainLoss;

        private readonly IConfiguration configuration;
        public PortfolioDetailsController(IConfiguration config)
        {
            configuration = config;
        }
        public IActionResult GetPortfolioDetails(string aCode, string fromDate, string toDate, string key)
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


            SqlConnection oConnection = new SqlConnection(sisConn);

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
                DT.Columns.Add("JointHolder");
                oConnection.Open();
                string opbal = "SELECT acode,aname,boid,addr1,addr2,city,faname,moname,aatype,jname1 FROM T_CLIENT WHERE acode='" + aCode.Trim() + "'";
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
                    if (dr["aatype"].ToString().Trim() == "INDIVIDUAL")
                        r["JointHolder"] = "";
                    else if (dr["aatype"].ToString().Trim() != "INDIVIDUAL")
                        r["JointHolder"] = dr["jname1"].ToString();
                }

                #endregion


                #region data inserting part=================================================================================
                oConnection = new SqlConnection(rclConn);

                oConnection.Open();
                string opbal2 = "SELECT DISTINCT firmscd FROM SISROYALU.dbo.T_SH ab WHERE ab.acode='" + aCode.Trim() + "' AND ab.tdate<= '" + tDate + "' AND ab.tran_type='S' GROUP BY ab.firmscd";
                SqlCommand oCommand2 = new SqlCommand(opbal2, oConnection);

                DataTable oTable2 = new DataTable();
                oTable2.Load(oCommand2.ExecuteReader());

                foreach (DataRow dr in oTable2.Rows)
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO OpeningShare EXEC [SISROYALU].[dbo].[sp_cos_ss_dt] '" + aCode + "','" + dr["firmscd"].ToString() + "' ,'" + fDate + "','" + tDate + "'", oConnection);
                    cmd.ExecuteNonQuery();
                }
                oConnection.Close();
                #endregion


                #region main table part=================================================================================
                oConnection = new SqlConnection(rclConn);

                DataSet DS3 = new DataSet();
                DataTable DT3 = DS3.Tables.Add();
                DT3.Columns.Add("SL#");
                DT3.Columns.Add("Instrument");
                DT3.Columns.Add("Bought Qty.");
                DT3.Columns.Add("Bought Cost");
                DT3.Columns.Add("Sold Qnt.");
                DT3.Columns.Add("Sold Cost");
                DT3.Columns.Add("Realised Gain/Loss");
                DT3.Columns.Add("Balance Qty.");
                DT3.Columns.Add("Balance Rate");
                DT3.Columns.Add("Balance Amnt");
                DT3.Columns.Add("Market Rate");
                DT3.Columns.Add("Market Amnt");
                DT3.Columns.Add("Unrealised Gain");

                oConnection.Open();

                SqlCommand oCommond3 = new SqlCommand("EXEC [dbo].[uSP_PL] '" + aCode.Trim() + "','" + fDate.Trim() + "','" + tDate + "'", oConnection);

                DataTable oTable3 = new DataTable();
                oTable3.Load(oCommond3.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r3;

                foreach (DataRow dr in oTable3.Rows)
                {
                    if (Convert.ToDouble(dr["buyQnty"].ToString()) != 0 && (dr["saleQnt"].ToString() != null || dr["saleQnt"].ToString() != ""))
                    {
                        flag++;
                        r3 = DT3.NewRow();

                        r3["SL#"] = flag.ToString();
                        r3["Instrument"] = dr["firmsnm1"].ToString();
                        r3["Bought Qty."] = string.Format("{0:#,#0}", dr["buyQnty"]).ToString();
                        r3["Bought Cost"] = string.Format("{0:#,#0.00}", dr["buyAmount"]).ToString();

                        if (dr["buyAmount"].ToString() == "") totalBCost1 = 0;
                        else totalBCost1 = Convert.ToDouble(dr["buyAmount"].ToString());
                        totalBCost += totalBCost1;


                        r3["Sold Qnt."] = string.Format("{0:#,#0}", dr["saleQnt"]).ToString();
                        r3["Sold Cost"] = string.Format("{0:#,#0.00}", dr["saleAmnt"]);

                        if (dr["saleAmnt"].ToString() == "") totalSCost1 = 0;
                        else totalSCost1 = Convert.ToDouble(dr["saleAmnt"].ToString());
                        totalSCost += totalSCost1;

                        r3["Realised Gain/Loss"] = string.Format("{0:#,#0.00}", dr["RG"]);

                        if (dr["RG"].ToString() == "") totalRG1 = 0;
                        else totalRG1 = Convert.ToDouble(dr["RG"].ToString());
                        totalRG += totalRG1;

                        r3["Balance Qty."] = string.Format("{0:#,#0.00}", dr["BQ"]);
                        r3["Balance Rate"] = string.Format("{0:#,#0.00}", dr["BR"]);
                        r3["Balance Amnt"] = string.Format("{0:#,#0.00}", dr["BA"]);

                        if (dr["BA"].ToString() == "") TotalBA1 = 0;
                        else TotalBA1 = Convert.ToDouble(dr["BA"].ToString());
                        TotalBA += TotalBA1;

                        r3["Market Rate"] = string.Format("{0:#,#0.00}", dr["TMR"]);
                        r3["Market Amnt"] = string.Format("{0:#,#0.00}", dr["TMA"]);

                        if (dr["TMA"].ToString() == "") TotalMA1 = 0;
                        else TotalMA1 = Convert.ToDouble(dr["TMA"].ToString());
                        TotalMA += TotalMA1;

                        r3["Unrealised Gain"] = string.Format("{0:#,#0.00}", dr["TUG"]);


                        if (dr["TUG"].ToString() == "") TotalUR1 = 0;
                        else TotalUR1 = Convert.ToDouble(dr["TUG"].ToString());
                        TotalUR += TotalUR1;

                        DT3.Rows.Add(r3);
                    }
                }

                r = DT3.NewRow();

                r["SL#"] = "";
                r["Instrument"] = "";
                r["Bought Qty."] = "";
                r["Bought Cost"] = string.Format("{0:#,#0.00}", totalBCost);
                r["Sold Qnt."] = "";
                r["Sold Cost"] = string.Format("{0:#,#0.00}", totalSCost);
                r["Realised Gain/Loss"] = string.Format("{0:#,#0.00}", totalRG);
                r["Balance Qty."] = "";
                r["Balance Rate"] = "";
                r["Balance Amnt"] = string.Format("{0:#,#0.00}", TotalBA);
                r["Market Rate"] = "";
                r["Market Amnt"] = string.Format("{0:#,#0.00}", TotalMA);
                r["Unrealised Gain"] = string.Format("{0:#,#0.00}", TotalUR);

                portfolioValueMarketPrice = TotalMA;
                portfolioValueCostPric = TotalBA;

                DT3.Rows.Add(r);

                List<PortfolioDetailsMain> portfolioDetailsMainList = new List<PortfolioDetailsMain>();

                for (int i = 0; i < DS3.Tables[0].Rows.Count; i++)
                {
                    portfolioDetailsMainList.Add(new PortfolioDetailsMain(
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[4]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[5]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[6]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[7]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[8]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[9]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[10]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[11]),
                    Convert.ToString(DS3.Tables[0].Rows[i].ItemArray[12])
                    ));
                }
                #endregion


                #region IPO Share Data=================================================================================
                oConnection = new SqlConnection(sisConn);

                DataSet DS4= new DataSet();
                DataTable DT4 = DS4.Tables.Add();
                DT4.Columns.Add("SL#");
                DT4.Columns.Add("Instrument");
                DT4.Columns.Add("Quantit");
                DT4.Columns.Add("Amount");
                DT4.Columns.Add("Date");

                oConnection.Open();

                SqlCommand oCommond4 = new SqlCommand("SELECT b.firmsnm1,ISNULL(a.quantity,0) quantity,ISNULL(a.amount,0) amount, CONVERT(VARCHAR,a.dtofent,105) dat FROM T_RR a,T_firms b WHERE a.firmscd=b.firmscd AND acode='" + aCode.Trim() + "' AND cert_no='IPO' AND a.dtofent BETWEEN '" + fDate + "' AND '" + tDate + "'", oConnection);

                DataTable oTable4 = new DataTable();
                oTable4.Load(oCommond4.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r4;

                foreach (DataRow dr in oTable4.Rows)
                {
                    findFlag = 1;
                    flag++;
                    r4 = DT4.NewRow();

                    r4["SL#"] = flag.ToString();
                    r4["Instrument"] = dr["firmsnm1"].ToString();
                    r4["Quantit"] = string.Format("{0:#,#0}", dr["quantity"]).ToString();
                    r4["Amount"] = string.Format("{0:#,#0.00}", dr["amount"]).ToString();
                    r4["Date"] = dr["dat"].ToString();

                    DT4.Rows.Add(r4);

                }
                if (findFlag == 0)
                {
                    r4 = DT4.NewRow();

                    r4["SL#"] = "0";
                    r4["Instrument"] = "";
                    r4["Quantit"] = string.Format("{0:#,#0}", 0).ToString();
                    r4["Amount"] = string.Format("{0:#,#0.00}", 0).ToString();
                    r4["Date"] = "";

                    DT4.Rows.Add(r4);
                }
                findFlag = 0;

                List<Shares> sharesIPO = new List<Shares>();

                for (int i = 0; i < DS4.Tables[0].Rows.Count; i++)
                {
                    sharesIPO.Add(new Shares(
                    Convert.ToString(DS4.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS4.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS4.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS4.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS4.Tables[0].Rows[i].ItemArray[4])
                    ));
                }

                #endregion


                #region Bonus Share Data=================================================================================
                oConnection = new SqlConnection(sisConn);

                DataSet DS5 = new DataSet();
                DataTable DT5 = DS5.Tables.Add();
                DT5.Columns.Add("SL#");
                DT5.Columns.Add("Instrument");
                DT5.Columns.Add("Quantit");
                DT5.Columns.Add("Amount");
                DT5.Columns.Add("Date");

                oConnection.Open();

                SqlCommand oCommond5 = new SqlCommand("SELECT b.firmsnm1,ISNULL(a.quantity,0) quantity,ISNULL(a.amount,0) amount, CONVERT(VARCHAR,a.dtofent,105) dat FROM T_RR a,T_firms b WHERE a.firmscd=b.firmscd AND acode='" + aCode.Trim() + "' AND cert_no='BONUS' AND a.dtofent BETWEEN '" + fDate + "' AND '" + tDate + "'", oConnection);

                DataTable oTable5 = new DataTable();
                oTable5.Load(oCommond5.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r5;

                foreach (DataRow dr in oTable5.Rows)
                {
                    findFlag = 1;
                    flag++;
                    r5 = DT5.NewRow();

                    r5["SL#"] = flag.ToString();
                    r5["Instrument"] = dr["firmsnm1"].ToString();
                    r5["Quantit"] = string.Format("{0:#,#0}", dr["quantity"]).ToString();
                    r5["Amount"] = string.Format("{0:#,#0.00}", dr["amount"]).ToString();
                    r5["Date"] = dr["dat"].ToString();

                    DT5.Rows.Add(r5);

                }
                if (findFlag == 0)
                {
                    r5 = DT5.NewRow();

                    r5["SL#"] = "0";
                    r5["Instrument"] = "";
                    r5["Quantit"] = string.Format("{0:#,#0}", 0).ToString();
                    r5["Amount"] = string.Format("{0:#,#0.00}", 0).ToString();
                    r5["Date"] = "";

                    DT5.Rows.Add(r5);
                }
                findFlag = 0;

                List<Shares> sharesBonus = new List<Shares>();

                for (int i = 0; i < DS5.Tables[0].Rows.Count; i++)
                {
                    sharesBonus.Add(new Shares(
                    Convert.ToString(DS5.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS5.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS5.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS5.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS5.Tables[0].Rows[i].ItemArray[4])
                    ));
                }

                #endregion

                #region Right Share Data=================================================================================
                oConnection = new SqlConnection(sisConn);

                DataSet DS6 = new DataSet();
                DataTable DT6 = DS6.Tables.Add();
                DT6.Columns.Add("SL#");
                DT6.Columns.Add("Instrument");
                DT6.Columns.Add("Quantit");
                DT6.Columns.Add("Amount");
                DT6.Columns.Add("Date");

                oConnection.Open();

                SqlCommand oCommond6 = new SqlCommand("SELECT b.firmsnm1,ISNULL(a.quantity,0) quantity,ISNULL(a.amount,0) amount, CONVERT(VARCHAR,a.dtofent,105) dat FROM T_RR a,T_firms b WHERE a.firmscd=b.firmscd AND acode='" + aCode.Trim() + "' AND cert_no='RIGHT' AND a.dtofent BETWEEN '" + fDate + "' AND '" + tDate + "'", oConnection);

                DataTable oTable6 = new DataTable();
                oTable6.Load(oCommond6.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r6;

                foreach (DataRow dr in oTable6.Rows)
                {
                    findFlag = 1;
                    flag++;
                    r6 = DT6.NewRow();

                    r6["SL#"] = flag.ToString();
                    r6["Instrument"] = dr["firmsnm1"].ToString();
                    r6["Quantit"] = string.Format("{0:#,#0}", dr["quantity"]).ToString();
                    r6["Amount"] = string.Format("{0:#,#0.00}", dr["amount"]).ToString();
                    r6["Date"] = dr["dat"].ToString();

                    DT6.Rows.Add(r6);

                }
                if (findFlag == 0)
                {
                    r6 = DT6.NewRow();

                    r6["SL#"] = "0";
                    r6["Instrument"] = "";
                    r6["Quantit"] = string.Format("{0:#,#0}", 0).ToString();
                    r6["Amount"] = string.Format("{0:#,#0.00}", 0).ToString();
                    r6["Date"] = "";

                    DT6.Rows.Add(r6);
                }
                findFlag = 0;

                List<Shares> sharesRight = new List<Shares>();

                for (int i = 0; i < DS6.Tables[0].Rows.Count; i++)
                {
                    sharesRight.Add(new Shares(
                    Convert.ToString(DS6.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS6.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS6.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS6.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS6.Tables[0].Rows[i].ItemArray[4])
                    ));
                }

                #endregion


                #region Last table Data=================================================================================
                oConnection = new SqlConnection(rclConn);

                oConnection.Open();

                SqlCommand oCommond7 = new SqlCommand("EXEC [dbo].[uSP_PL_CAL] '" + aCode.Trim() + "','" + fDate + "','" + tDate + "'", oConnection);

                DataTable oTable7 = new DataTable();
                oTable7.Load(oCommond7.ExecuteReader(CommandBehavior.CloseConnection));


                foreach (DataRow dr in oTable7.Rows)
                {
                    sledgerBal = string.Format("{0:#,#0.00}", dr["Ledger"]).ToString();  ////  Closing Equity 
                    sportfolioValueMarketPrice = string.Format("{0:#,#0.00}", portfolioValueMarketPrice).ToString();
                    slblPortfolioValueCostPric = string.Format("{0:#,#0.00}", (portfolioValueCostPric));
                    slblDeposit = string.Format("{0:#,#0.00}", dr["deposit"]).ToString();
                    slblWithdrawnAmount = string.Format("{0:#,#0.00}", dr["withdraw"]);
                    slblCharges = string.Format("{0:#,#0.00}", dr["charge"]);
                    slblNetDeposit = string.Format("{0:#,#0.00}", dr["NetDeposit"]);
                    slblRealisedCapitalGainLoss = string.Format("{0:#,#0.00}", totalRG);
                }

                #endregion

                #region Last table Data=================================================================================
                oConnection = new SqlConnection(rclConn);

                oConnection.Open();

                SqlCommand oCommond8 = new SqlCommand("EXEC SISROYALU.[dbo].[SP_INV_TCOS] '" + fDate + "','" + aCode.Trim() + "'", oConnection);

                DataTable oTable8 = new DataTable();
                oTable8.Load(oCommond8.ExecuteReader(CommandBehavior.CloseConnection));


                foreach (DataRow dr in oTable8.Rows)
                {
                    if (Convert.ToDouble(dr["rate"].ToString()) != 0)
                    {

                        openingShareBal += (Convert.ToDouble(dr["amount"].ToString()));
                    }
                    //lblOpeningBal.Text = string.Format("{0:#,#0.00}", Convert.ToDouble(dr["OpeningCost"].ToString()));


                }
                sopeningShareBal = string.Format("{0:#,#0.00}", openingShareBal);

                #endregion


                PortfolioDetails portfolioDetails = new PortfolioDetails();
                portfolioDetails.AccHolderName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[0]).Trim();
                portfolioDetails.FathersName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[1]).Trim();
                portfolioDetails.MothersName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[2]).Trim();
                portfolioDetails.AccNo = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[3]).Trim();
                portfolioDetails.BoId = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[4]).Trim();
                portfolioDetails.Address = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[5]).Trim();
                portfolioDetails.JointHolder = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[6]).Trim();

                portfolioDetails.portfolioDetailsMains = portfolioDetailsMainList;

                portfolioDetails.portfolioDetailsIPO = sharesIPO;

                portfolioDetails.portfolioDetailsBonus = sharesBonus;

                portfolioDetails.portfolioDetailsRight = sharesRight;

                portfolioDetails.OpeningShareBal = sopeningShareBal;
                portfolioDetails.LedgerBal = sledgerBal;
                portfolioDetails.PortfolioValueMarketPrice = sportfolioValueMarketPrice;
                portfolioDetails.PortfolioValueCostPric = slblPortfolioValueCostPric;
                portfolioDetails.Deposit = slblDeposit;
                portfolioDetails.WithdrawnAmount = slblWithdrawnAmount;
                portfolioDetails.Charges = slblCharges;
                portfolioDetails.NetDeposit = slblNetDeposit;
                portfolioDetails.RealisedCapitalGainLoss = slblRealisedCapitalGainLoss;


                return Ok(portfolioDetails);

            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
