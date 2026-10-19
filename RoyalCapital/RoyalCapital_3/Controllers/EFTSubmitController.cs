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
    public class EFTSubmitController : ControllerBase
    {
        string ledger;
        string matured;

        private readonly IConfiguration configuration;
        public EFTSubmitController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("")]
        public IActionResult GetEFT(string aCode, string key)
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
                oConnection.Open();
                string query = "SELECT matbalP,ldgbal FROM T_Tkbal WHERE acode='" + aCode.Trim() + "'";
                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));
                int has_value = 0;

                foreach (DataRow dr in oTable.Rows)
                {

                    ledger = dr["ldgbal"].ToString();
                    matured = dr["matbalP"].ToString();

                    has_value++;
                }
                if (has_value == 0)
                {
                    ledger = "0";
                    matured = "0";
                }

                return Ok(new
                {
                    ledger = ledger,
                    matured = matured
                });
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }

        [HttpGet("unprocessed")]
        public IActionResult GetUnprocessedEFT(string aCode, string key)
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
                DataTable DT = DS.Tables.Add("sms");
                DT.Columns.Add("Date");
                DT.Columns.Add("Number");
                DT.Columns.Add("Code");
                DT.Columns.Add("Amount");
                DT.Columns.Add("Status");
                DT.Columns.Add("Registered");
                DT.Columns.Add("Deleted");
                DT.Columns.Add("I/F");
                SqlConnection oConnection = new SqlConnection(rclConn);
                oConnection.Open();
                string query = "SELECT * FROM RCLWEB.[RCLWEB].[dbo].T_SMS_TRANSECTION WHERE RCODE='" + aCode.Trim() + "' AND flag1=1 AND flag2=0 AND clr=0 AND status='NO'";
                var oCommond = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();

                oTable.Load(oCommond.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;
                foreach (DataRow dr in oTable.Rows)
                {
                    r = DT.NewRow();

                    r["Date"] = string.Format("{0:dd-MMM-yyyy}", dr["dat"]);
                    r["Number"] = dr["SMS_Number"].ToString();
                    r["Code"] = dr["RCODE"].ToString();
                    r["Amount"] = dr["amount"].ToString();

                    if ((dr["status"].ToString().ToUpper() == "NO") && ((Convert.ToInt16(dr["flag1"].ToString()) == 1) || (Convert.ToInt16(dr["flag2"].ToString()) == 0) || (Convert.ToInt16(dr["clr"].ToString()) == 0)))
                    {
                        r["Status"] = "Processing...";
                    }
                    if ((dr["status"].ToString().ToUpper() == "NO") && ((Convert.ToInt16(dr["flag1"]) == 0) || (Convert.ToInt16(dr["flag2"].ToString()) == 1) || (Convert.ToInt16(dr["clr"].ToString()) == 1)))
                    {
                        r["Status"] = "Deny...";
                    }
                    if ((dr["status"].ToString().ToUpper() == "YES"))
                    {
                        r["Status"] = "Received...";
                    }
                    if (Convert.ToInt16(dr["flag1"].ToString()) == 1)
                    {
                        r["Registered"] = "Yes";
                    }
                    if (Convert.ToInt16(dr["flag1"]) == 0)
                    {
                        r["Registered"] = "No";
                    }
                    if (Convert.ToInt16(dr["flag1"]) == 11)
                    {
                        r["Registered"] = "From Web";
                    }
                    if (Convert.ToInt16(dr["flag2"]) == 1)
                    {
                        r["Deleted"] = "Yes";
                    }
                    if (Convert.ToInt16(dr["flag2"]) == 0)
                    {
                        r["Deleted"] = "No";
                    }
                    if (Convert.ToInt16(dr["clr"]) == 1)
                    {
                        r["I/F"] = "Yes";
                    }
                    if (Convert.ToInt16(dr["clr"]) == 0)
                    {
                        r["I/F"] = "No";
                    }
                    DT.Rows.Add(r);
                }

                List<EFTSumittedRequisition> eFTSumittedRequisitionList = new List<EFTSumittedRequisition>();

                for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                {
                    eFTSumittedRequisitionList.Add(new EFTSumittedRequisition(
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[4]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[5]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[6])
                    ));
                }
                return Ok(eFTSumittedRequisitionList);
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }

            
        }

        [HttpPost("")]
        public IActionResult PostEFT([FromBody] EFTPost eFTPost, string key)
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

            string transactionStatus;
            string aCode = eFTPost.ACode;
            string amount = eFTPost.Amount;


            SqlConnection oConnection5 = new SqlConnection(sisConn);

            try
            {
                oConnection5.Open();
                string query5 = "SELECT matbalP,ldgbal FROM T_Tkbal WHERE acode='" + aCode.Trim() + "'";
                SqlCommand oCommand5 = new SqlCommand(query5, oConnection5);

                DataTable oTable5 = new DataTable();
                oTable5.Load(oCommand5.ExecuteReader(CommandBehavior.CloseConnection));
                int has_value = 0;

                foreach (DataRow dr in oTable5.Rows)
                {

                    ledger = dr["ldgbal"].ToString();
                    matured = dr["matbalP"].ToString();

                    has_value++;
                }
                if (has_value == 0)
                {
                    ledger = "0";
                    matured = "0";
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }


            if (eft_posting_check(aCode, ledger, matured, amount) == 0)
            {

                SqlConnection oConnection = new SqlConnection(rclConn);

                try
                {
                    oConnection.Open();

                    string query = "INSERT INTO T_SMS_TRANSECTION(SMS_Number,RCODE,amount,dat,flag1,flag2,status,clr) VALUES('MOBILE_APP','" + aCode.Trim() + "','" + amount.Trim() + "','" + string.Format("{0:MM/dd/yyyy}", DateTime.Now) + ' ' + string.Format("{0:hh:mm:ss tt}", DateTime.Now) + "','1','0','NO','0')";
                    SqlCommand oCommand = new SqlCommand(query, oConnection);

                    oCommand.ExecuteNonQuery();

                    oConnection.Close();

                    transactionStatus = "Transaction Submitted Successfully!";

                    return Ok(transactionStatus);

                }
                catch (Exception ex)
                {
                    return NotFound(ex.ToString());
                }
            }
            else if(eft_posting_check(aCode, ledger, matured, amount) == 5)
            {
                return Ok(new { 
                    message = "Insufficient Amount for EFT Requisition."
                });
            }
            else if (eft_posting_check(aCode, ledger, matured, amount) == 2)
            {
                return Ok(new
                {
                    message = "You already have a pending EFT application to process."
                });
            }
            else
            {
                return Ok(new
                {
                    message = "Insufficient Amount for EFT Requisition."
                });
            }

        }


        public int eft_posting_check(string aCode, string ledger, string matured, string formAmount)
        {

            int count;
            double amount;
            string errorMessage;
            string sisConn = configuration.GetConnectionString("SISROYALU");
            string rclConn = configuration.GetConnectionString("RCLWEB");

            amount = Convert.ToDouble(matured) - Convert.ToDouble(formAmount);
            if (Convert.ToDouble(formAmount) == 0)
            {
                errorMessage = "Oops...!!! Amount is too low";
                return 5;
            }
            if ((amount > 0))
            {
                DataSet DS = new DataSet();
                DataTable DT = DS.Tables.Add("sms");
                DT.Columns.Add("Date");
                DT.Columns.Add("Number");
                DT.Columns.Add("Code");
                DT.Columns.Add("Amount");
                DT.Columns.Add("Status");
                DT.Columns.Add("Registered");
                DT.Columns.Add("Deleted");
                DT.Columns.Add("I/F");
                SqlConnection oConnection = new SqlConnection(rclConn);
                oConnection.Open();
                string query = "SELECT * FROM RCLWEB.[RCLWEB].[dbo].T_SMS_TRANSECTION WHERE RCODE='" + aCode.Trim() + "' AND flag1=1 AND flag2=0 AND clr=0 AND status='NO'";
                var oCommond = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();

                oTable.Load(oCommond.ExecuteReader(CommandBehavior.CloseConnection));
                count = oTable.Rows.Count;
                if (count > 0)
                {
                    DataRow r;
                    foreach (DataRow dr in oTable.Rows)
                    {
                        r = DT.NewRow();

                        r["Date"] = string.Format("{0:dd-MMM-yyyy}", dr["dat"]);
                        r["Number"] = dr["SMS_Number"].ToString();
                        r["Code"] = dr["RCODE"].ToString();
                        r["Amount"] = dr["amount"].ToString();

                        if ((dr["status"].ToString().ToUpper() == "NO") && ((Convert.ToInt16(dr["flag1"].ToString()) == 1) || (Convert.ToInt16(dr["flag2"].ToString()) == 0) || (Convert.ToInt16(dr["clr"].ToString()) == 0)))
                        {
                            r["Status"] = "Processing...";
                        }
                        if ((dr["status"].ToString().ToUpper() == "NO") && ((Convert.ToInt16(dr["flag1"]) == 0) || (Convert.ToInt16(dr["flag2"].ToString()) == 1) || (Convert.ToInt16(dr["clr"].ToString()) == 1)))
                        {
                            r["Status"] = "Deny...";
                        }
                        if ((dr["status"].ToString().ToUpper() == "YES"))
                        {
                            r["Status"] = "Received...";
                        }
                        if (Convert.ToInt16(dr["flag1"].ToString()) == 1)
                        {
                            r["Registered"] = "Yes";
                        }
                        if (Convert.ToInt16(dr["flag1"]) == 0)
                        {
                            r["Registered"] = "No";
                        }
                        if (Convert.ToInt16(dr["flag1"]) == 11)
                        {
                            r["Registered"] = "From Web";
                        }
                        if (Convert.ToInt16(dr["flag2"]) == 1)
                        {
                            r["Deleted"] = "Yes";
                        }
                        if (Convert.ToInt16(dr["flag2"]) == 0)
                        {
                            r["Deleted"] = "No";
                        }
                        if (Convert.ToInt16(dr["clr"]) == 1)
                        {
                            r["I/F"] = "Yes";
                        }
                        if (Convert.ToInt16(dr["clr"]) == 0)
                        {
                            r["I/F"] = "No";
                        }
                        DT.Rows.Add(r);
                    }

                    List<EFTSumittedRequisition> eFTSumittedRequisitionList = new List<EFTSumittedRequisition>();

                    for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                    {
                        eFTSumittedRequisitionList.Add(new EFTSumittedRequisition(
                        Convert.ToString(DS.Tables[0].Rows[i].ItemArray[0]),
                        Convert.ToString(DS.Tables[0].Rows[i].ItemArray[1]),
                        Convert.ToString(DS.Tables[0].Rows[i].ItemArray[2]),
                        Convert.ToString(DS.Tables[0].Rows[i].ItemArray[3]),
                        Convert.ToString(DS.Tables[0].Rows[i].ItemArray[4]),
                        Convert.ToString(DS.Tables[0].Rows[i].ItemArray[5]),
                        Convert.ToString(DS.Tables[0].Rows[i].ItemArray[6])
                        ));
                    }

                    return 2;
                    //GridView2.DataSource = DS.Tables[0];
                    //GridView2.DataBind();
                    //lblError.Text = "You have " + count + " unprocessed EFT application(s). For any query contact with our helpline: 09606555333";
                }
            }
            else
            {
                errorMessage = "You do not have sufficient balance to submit an EFT requisition";
                count = -1;
            }
            return count;
        }
    }
}
