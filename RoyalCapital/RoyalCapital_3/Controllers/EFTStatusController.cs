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
    public class EFTStatusController : ControllerBase
    {

        private readonly IConfiguration configuration;
        public EFTStatusController(IConfiguration config)
        {
            configuration = config;
        }

        public IActionResult GetEFTStatus(string aCode, string key)
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


            SqlConnection oConnection = new SqlConnection(sisConn);

            try
            {
                #region Requisition Approved part=================================================================================

                DataSet DS = new DataSet();
                DataTable DT = DS.Tables.Add("EFT");
                DT.Columns.Add("Date");
                DT.Columns.Add("Code");
                DT.Columns.Add("Amount");
                DT.Columns.Add("Status");

                oConnection.Open();
                string query = "SELECT tdate AS Date,acode AS Code,amount AS Amount,posted,approved,rejected FROM T_EFT WHERE acode='" + aCode.Trim() + "' ORDER BY tdate desc";

                SqlCommand oCommand = new SqlCommand(query, oConnection);
                oCommand.CommandTimeout = 0;
                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;

                foreach (DataRow dr in oTable.Rows)
                {
                    r = DT.NewRow();

                    r["Date"] = string.Format("{0:dd-MMM-yyyy}", dr["Date"]);
                    r["Code"] = dr["Code"].ToString();
                    r["Amount"] = string.Format("{0:#,#.00}", dr["Amount"]);

                    if ((dr["posted"].ToString() == "True") && (dr["approved"].ToString() == "True") && (dr["rejected"].ToString() == "False"))
                    {
                        r["Status"] = "Approved";
                    }

                    else if ((dr["posted"].ToString() == "True") && (dr["approved"].ToString() == "False") && (dr["rejected"].ToString() == "False"))
                    {
                        r["Status"] = "Posted";
                    }

                    else if ((dr["posted"].ToString() == "True") && (dr["approved"].ToString() == "False") && (dr["rejected"].ToString() == "True"))
                    {
                        r["Status"] = "Rejected";
                    }

                    else
                    {
                        r["Status"] = "Contact With Our Help Line";
                    }

                    DT.Rows.Add(r);
                }

                List<EFTApprovedRequisition> eFTApprovedRequisitionList = new List<EFTApprovedRequisition>();

                for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                {
                    eFTApprovedRequisitionList.Add(new EFTApprovedRequisition(
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[3])
                    ));
                }
                #endregion


                #region Requisition Submited part=================================================================================
                List<EFTSumittedRequisition> eFTSumittedRequisitionList = new List<EFTSumittedRequisition>();
                try
                {
                    DataSet DS2 = new DataSet();
                    DataTable DT2 = DS2.Tables.Add("sms");
                    DT2.Columns.Add("Date");
                    DT2.Columns.Add("Code");
                    DT2.Columns.Add("Amount");
                    DT2.Columns.Add("Status");
                    DT2.Columns.Add("Registered");
                    DT2.Columns.Add("Deleted");
                    DT2.Columns.Add("I/F");

                    var oConnection2 = new SqlConnection(rclConn);

                    oConnection2.Open();
                    string query2 = "SELECT TOP 10 * FROM (SELECT RCODE,amount,dat,flag1,flag2,CONVERT(VARCHAR(3),status) [status],clr FROM RCLWEB.[RCLWEB].[dbo].T_SMS_TRANSECTION WHERE RCODE='" + aCode.Trim() + "' UNION ALL SELECT acode RCODE,amount AS amount, tdate AS dat,1 flag1,rejected flag2,(CASE approved WHEN 1 THEN 'Yes' ELSE 'No' END) [status],0 clr FROM RCLWEB.SISROYALU.dbo.T_EFT WHERE acode='" + aCode.Trim() + "') a ORDER BY a.dat desc";

                    var oCommond2 = new SqlCommand(query2, oConnection2);
                    oCommond2.CommandTimeout = 0;

                    DataTable oTable2 = new DataTable();
                    oTable2.Load(oCommond2.ExecuteReader(CommandBehavior.CloseConnection));

                    DataRow r2;
                    foreach (DataRow dr in oTable2.Rows)
                    {
                        r2 = DT2.NewRow();

                        r2["Date"] = string.Format("{0:dd-MMM-yyyy}", dr["dat"]);
                        r2["Code"] = dr["RCODE"].ToString();
                        r2["Amount"] = string.Format("{0:#,#0.00}", dr["amount"]);

                        if ((dr["status"].ToString() == "NO") && (((dr["flag1"].ToString()) == "1") || ((dr["flag2"].ToString()) == "0") || ((dr["clr"].ToString()) == "0"))) //Convert.ToInt16
                        {
                            r2["Status"] = "Processing...";
                        }
                        if ((dr["status"].ToString() == "NO") && (((dr["flag1"].ToString()) == "0") || ((dr["flag2"].ToString()) == "0") || ((dr["clr"].ToString()) == "1")))
                        {
                            r2["Status"] = "Deny...";
                        }
                        if ((dr["status"].ToString() == "YES"))
                        {
                            r2["Status"] = "Received...";
                        }
                        ////////////////////////////////
                        if (Convert.ToInt16(dr["flag1"].ToString()) == 1)
                        {
                            r2["Registered"] = "Yes";
                        }
                        if (Convert.ToInt16(dr["flag1"]) == 0)
                        {
                            r2["Registered"] = "No";
                        }
                        if (Convert.ToInt16(dr["flag1"]) == 11)
                        {
                            r2["Registered"] = "From Web";
                        }
                        /////////////////////////
                        if (Convert.ToInt16(dr["flag2"]) == 1)
                        {
                            r2["Deleted"] = "Yes";
                        }
                        if (Convert.ToInt16(dr["flag2"]) == 0)
                        {
                            r2["Deleted"] = "No";
                        }

                        ///////////////////////////////////
                        if (Convert.ToInt16(dr["clr"]) == 1)
                        {
                            r2["I/F"] = "Yes";
                        }
                        if (Convert.ToInt16(dr["clr"]) == 0)
                        {
                            r2["I/F"] = "No";
                        }

                        DT2.Rows.Add(r2);

                    }

                    for (int i = 0; i < DS2.Tables[0].Rows.Count; i++)
                    {
                        eFTSumittedRequisitionList.Add(new EFTSumittedRequisition(
                        Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[0]),
                        Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[1]),
                        Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[2]),
                        Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[3]),
                        Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[4]),
                        Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[5]),
                        Convert.ToString(DS2.Tables[0].Rows[i].ItemArray[6])
                        ));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                #endregion


                EFTStatus eFTStatus = new EFTStatus();

                eFTStatus.EFTApprovedRequisitions = eFTApprovedRequisitionList;
                eFTStatus.EFTSumittedRequisitions = eFTSumittedRequisitionList;

                return Ok(eFTStatus);

            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
