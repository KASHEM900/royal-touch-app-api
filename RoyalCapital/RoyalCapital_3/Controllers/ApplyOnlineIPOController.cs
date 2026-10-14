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

using System.Net;


namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplyOnlineIPOController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public string iPOCName;
        public string iPOName;
        public string lotSize;
        public string price;
        public string startDate;
        public string endDate;
        public string tPrice;

        public double mr_no_m, row_id, ipo_sl, ipo_amount, ipo_code, s_charge;
        public string mr_no, acc_type, mobile, ipo_name, ipo_amount_words, rcv_type, query;
        public ApplyOnlineIPOController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("get-ipo-names")]
        public IActionResult GetIPOName(string aCode, string key)
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

            SqlConnection oConnection = new SqlConnection(rclConn);

            try
            {
                DataSet DS = new DataSet();
                DataTable DT = DS.Tables.Add();
                DT.Columns.Add("SL");
                DT.Columns.Add("IPO Name");

                oConnection.Open();
                string query = "SELECT ROW_NUMBER() OVER(ORDER BY T_IPO_Details.IPO_SL) AS SL, IPO_NAME AS 'IPO NAME' FROM RCLWEB.[RCLWEB].[dbo].T_IPO_Details WHERE ON_OFF='True'";
                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;

                foreach (DataRow dr in oTable.Rows)
                {
                    r = DT.NewRow();

                    r["SL"] = dr["SL"].ToString();
                    r["IPO Name"] = dr["IPO Name"].ToString();

                    DT.Rows.Add(r);
                }

                List<IPOInitialInfo> iPOInitialInfoList = new List<IPOInitialInfo>();

                for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                {
                    iPOInitialInfoList.Add(new IPOInitialInfo(
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[1])
                    ));
                }

                return Ok(iPOInitialInfoList);
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }

        [HttpGet("get-ipo-details-info")]
        public IActionResult GetIPODetailsInfo(string iPOName, string key)
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

            SqlConnection oConnection = new SqlConnection(rclConn);

            try
            {
                DataSet DS = new DataSet();
                DataTable DT = DS.Tables.Add();
                DT.Columns.Add("IPO_CNAME");
                DT.Columns.Add("IPO_NAME");
                DT.Columns.Add("LOT_SIZE");
                DT.Columns.Add("PRICE");
                DT.Columns.Add("START_DATE");
                DT.Columns.Add("END_DATE");
                DT.Columns.Add("T_PRICE");

                oConnection.Open();
                string query = "SELECT IPO_CNAME,IPO_NAME,LOT_SIZE,PRICE,START_DATE,END_DATE,T_PRICE FROM RCLWEB.[RCLWEB].[dbo].T_IPO_Details WHERE ON_OFF='True' AND IPO_NAME = '" + iPOName.ToString() + "'";
                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;

                foreach (DataRow dr in oTable.Rows)
                {
                    r = DT.NewRow();

                    r["IPO_CNAME"] = dr["IPO_CNAME"].ToString();
                    r["IPO_NAME"] = dr["IPO_NAME"].ToString();
                    r["LOT_SIZE"] = dr["LOT_SIZE"].ToString();
                    r["PRICE"] = dr["PRICE"].ToString();
                    r["START_DATE"] = string.Format("{0:dd-MMM-yyyy}", dr["START_DATE"]);
                    r["END_DATE"] = string.Format("{0:dd-MMM-yyyy}", dr["END_DATE"]);
                    r["T_PRICE"] = dr["T_PRICE"].ToString();

                    DT.Rows.Add(r);
                }

                List<IPODetailsInfo> iPODetailsInfoList = new List<IPODetailsInfo>();

                for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                {
                    iPODetailsInfoList.Add(new IPODetailsInfo(
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[4]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[5]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[6])
                    ));
                }

                return Ok(iPODetailsInfoList);
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }

        [HttpPost("")]
        public IActionResult PostIPO([FromBody] IPOPost iPOPost, string aCode, string mobileNo, string key)
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

            SqlConnection oConnection = new SqlConnection(rclConn);
            try
            {
                //// IPO eligibility checking
                string qry_ipo_eligibility = "SELECT START_DATE, END_DATE+1 AS END_DATE  FROM RCLWEB.[RCLWEB].[dbo].T_IPO_Details WHERE IPO_NAME='" + iPOPost.IPOName + "'  ";
                oConnection.Open();
                SqlCommand eligibility_command = new SqlCommand(qry_ipo_eligibility, oConnection);
                DataTable eligibility_table = new DataTable();
                eligibility_table.Load(eligibility_command.ExecuteReader(CommandBehavior.CloseConnection));
                foreach (DataRow dr in eligibility_table.Rows)
                {
                    if (Convert.ToDateTime(dr["END_DATE"].ToString()) < DateTime.Now)
                    {
                        return Ok(new
                        {
                            message = "IPO Subscription date is over"
                        });
                    }
                    if (Convert.ToDateTime(dr["START_DATE"].ToString()) > DateTime.Now)
                    {
                        return Ok(new
                        {
                            message = "IPO Subscription is not started yet"
                        });
                    }
                }
                //// end of eligibility checking

                //// Checking master-child relationship
                string qry_master_check = "SELECT RCODE FROM RCLWEB.[RCLWEB].[dbo].T_IPO_MASTER_CODE WHERE RCODE='" + aCode.ToString() + "'";
                oConnection.Open();
                SqlCommand master_check_command = new SqlCommand(qry_master_check, oConnection);
                DataTable master_check_table = new DataTable();
                master_check_table.Load(master_check_command.ExecuteReader(CommandBehavior.CloseConnection));
                foreach (DataRow dr in master_check_table.Rows)
                {
                    return Ok(new
                    {
                        message = "Please contact with your nearest branch"
                    });
                }
                //// end of master-child relationship checking


                oConnection.Open();
                string tmp_RCODE = "";
                //// checking previous Entry
                string qry_double_check = "SELECT RCODE FROM RCLWEB.[RCLWEB].[dbo].T_IPO_REC WHERE RCODE='" + aCode.ToString() + "' AND IPO_NAME='" + iPOPost.IPOName + "'";
                SqlCommand oCommand = new SqlCommand(qry_double_check, oConnection);
                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));
                foreach (DataRow dr in oTable.Rows)
                {
                    tmp_RCODE = dr["RCODE"].ToString();
                }
                if (tmp_RCODE != "")
                {
                    return Ok(new
                    {
                        message = "Your code: '" + tmp_RCODE + "' is already Applied<br> You can not apply twice for '" + iPOPost.IPOName + "'"
                    });
                }
                else
                {
                    oConnection.Open();
                    acc_type = "General";
                    /// get affected code list
                    string queryAff = "SELECT RCODE FROM RCLWEB.[RCLWEB].[dbo].T_affected WHERE RCODE='" + aCode.ToString() + "'";
                    SqlCommand oCommandAff = new SqlCommand(queryAff, oConnection);
                    DataTable oTableAff = new DataTable();
                    oTableAff.Load(oCommandAff.ExecuteReader());

                    foreach (DataRow drAff in oTableAff.Rows)
                    {
                        acc_type = "Affected";
                    }
                    oConnection.Close();
                    ////// get IPO details
                    oConnection.Open();
                    query = "SELECT IPO_SL,IPO_NAME,LOT_SIZE,PRICE,T_PRICE,S_CHARGE,T_PRICE_WORD FROM RCLWEB.[RCLWEB].[dbo].T_IPO_Details WHERE IPO_NAME='" + iPOPost.IPOName + "' AND ON_OFF='1'";
                    oCommand = new SqlCommand(query, oConnection);
                    oTable = new DataTable();
                    oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                    foreach (DataRow dr in oTable.Rows)
                    {
                        ipo_code = Convert.ToDouble(dr["IPO_SL"].ToString());
                        ipo_name = dr["IPO_NAME"].ToString();
                        ipo_amount = Convert.ToDouble(dr["T_PRICE"].ToString());
                        ipo_amount_words = dr["T_PRICE_WORD"].ToString();
                        s_charge = Convert.ToDouble(dr["S_CHARGE"].ToString());
                    }

                    /// get maximum serial number
                    oConnection.Open();
                    query = "SELECT MAX(ROW_ID) AS ROW_ID FROM RCLWEB.[RCLWEB].[dbo].T_IPO_REC";
                    oCommand = new SqlCommand(query, oConnection);

                    oTable = new DataTable();
                    oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                    foreach (DataRow dr in oTable.Rows)
                    {
                        if (DBNull.Value.Equals(dr["ROW_ID"]))
                        {
                            row_id = 1;
                        }
                        else
                        {
                            row_id = (Convert.ToDouble(dr["ROW_ID"].ToString()) + 1);
                        }
                    }

                    if (check_EI(aCode.ToString(), iPOPost.IPOName))
                    {
                        int branch_code = get_branch_code(aCode.ToString()); //////get branch code according to RCODE
                        /// insert into ipo table
                        oConnection.Open();
                        string get_max_transection_id = "INSERT INTO T_IPO_TRANSECTION_GENERATOR (note,date_time) VALUES('Not Used', getdate()); SELECT SCOPE_IDENTITY() AS max_id";
                        SqlCommand comm_max = new SqlCommand(get_max_transection_id, oConnection);
                        DataTable dt_max = new DataTable();
                        dt_max.Load(comm_max.ExecuteReader());
                        mr_no_m = 0;
                        foreach (DataRow dr in dt_max.Rows)
                        {
                            mr_no_m = Convert.ToDouble(dr["max_id"].ToString());
                        }

                        rcv_type = "Ledger";

                        mr_no = "18" + Convert.ToString(row_id) + Convert.ToString(ipo_code) + DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString();
                        string queryInsert = "INSERT INTO RCLWEB.[RCLWEB].[dbo].T_IPO_REC(ROW_ID,MR_NO_M,MR_NO,RCODE,ACC_TYPE,MOBILE,IPO_SL,IPO_CODE,IPO_NAME,RCV_TYPE,APPLY_FROM,BKASH_REF_NO,BKASH_FLAG,BANK_NAME,CHK_NO,CHK_APPROVED,CHK_REJ_DATE,IPO_AMOUNT,IPO_AMOUNT_WORDS,ENTRY_DATE,BRANCH_CODE,PC_NAME,IP_ADDRESS,USER_NAME,IPO_RESULT,EFT, FLAG1, REFUND_CODE, FLAG3, BANK_BRANCH_NAME, DEPOSIT_DATE, sendLedger, SCHARGE)VALUES(" + row_id + ","
                        + mr_no_m + ",'"
                        + mr_no + "','"
                        + aCode.ToString() + "','"
                        + acc_type + "','"
                        + mobileNo.ToString() + "',"
                        + ipo_code + ",'"
                        + ipo_code.ToString() + "','"
                        + iPOPost.IPOName + "','"
                        + rcv_type + "','WEB','','','','',2,'','" + ipo_amount + "','" + ipo_amount_words + "', GETDATE() ," + branch_code + ",'','','','','0', '','','', '', '', 1, " + s_charge + ")";

                        SqlCommand oCommand1 = new SqlCommand(queryInsert, oConnection);
                        if (Convert.ToDouble(iPOPost.LedgerBalance.ToString()) >= ipo_amount)
                        {
                            oCommand1.ExecuteNonQuery();
                            string get_max_transection_id1 = "UPDATE T_IPO_TRANSECTION_GENERATOR SET note=CONVERT(VARCHAR,getDate(),109) WHERE tr_no='" + mr_no_m + "'";
                            SqlCommand comm_max1 = new SqlCommand(get_max_transection_id1, oConnection);
                            comm_max1.ExecuteNonQuery();

                            //////////////////////////////////////////////////////   Send SMS   ////////////////
                            string sms_content = "IPO Application: " + iPOPost.IPOName + " is received against your code: " + aCode + " AND MR No: " + mr_no_m + ".For Query: 09606016379";
                            string response = send_SMS_automatically(mobileNo.ToString(), sms_content);
                            //////////////////////////////////  End Send SMS //////////////////////////////////
                            
                            oConnection.Close();
                            return Ok(new
                            {
                                message = "Congratulation...!<br>Your IPO application for '" + iPOPost.IPOName + "' is Applied"
                            });
                        }
                        else
                        {
                            return Ok(new
                            {
                                message = "You don't have sufficient Ledger Balance to Apply this IPO '" + iPOPost.IPOName + "'"
                            });
                        }
                    }
                    else
                    {
                        return Ok(new
                        {
                            message = "You are not an Eligible for this IPO Application"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }

        private bool check_EI(string RCODE, string IPO_NAME)
        {
            string sisConn = configuration.GetConnectionString("SISROYALU");
            string rclConn = configuration.GetConnectionString("RCLWEB");
            SqlConnection oConnection = new SqlConnection(rclConn);
            try
            {
                oConnection.Open();
                string query = "SELECT [RCODE] FROM RCLWEB.[RCLWEB].[dbo].[T_IPO_EI_CLIENT] WHERE RCODE='" + RCODE + "' AND [IPONAME]='" + IPO_NAME + "'";
                SqlCommand oCommand = new SqlCommand(query, oConnection);
                DataTable oTable = new DataTable();
                oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr in oTable.Rows)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        private int get_branch_code(string rcode)
        {
            int branch_code = 0;
            int n;
            bool isNumeric = int.TryParse(rcode, out n);

            if (isNumeric)
            {
                double rcode_numeric = Convert.ToDouble(rcode);
                if ((rcode_numeric > 0 && rcode_numeric <= 9999) || (rcode_numeric >= 20000 && rcode_numeric < 30000) || (rcode_numeric >= 90000 && rcode_numeric <= 150000))
                {
                    branch_code = 1;
                }
                if (rcode_numeric >= 10000 && rcode_numeric <= 19999)
                {
                    branch_code = 3;
                }
                if (rcode_numeric >= 30000 && rcode_numeric <= 39999)
                {
                    branch_code = 4;
                }
                if (rcode_numeric >= 40000 && rcode_numeric <= 45100)
                {
                    branch_code = 10;
                }
                if (rcode_numeric >= 45101 && rcode_numeric <= 49999)
                {
                    branch_code = 11;
                }
                if (rcode_numeric >= 50000 && rcode_numeric <= 59999)
                {
                    branch_code = 9;
                }
                if (rcode_numeric >= 60000 && rcode_numeric <= 64999)
                {
                    branch_code = 8;
                }
                if (rcode_numeric >= 65000 && rcode_numeric <= 69999)
                {
                    branch_code = 7;
                }
                if (rcode_numeric >= 70000 && rcode_numeric <= 74999)
                {
                    branch_code = 6;
                }
                if (rcode_numeric >= 75000 && rcode_numeric <= 79999)
                {
                    branch_code = 12;
                }
                if (rcode_numeric >= 80000 && rcode_numeric <= 85000)
                {
                    branch_code = 13;
                }
                if (rcode_numeric == 200001)
                {
                    branch_code = 10;
                }
                if (rcode_numeric == 200002)
                {
                    branch_code = 11;
                }
                if (rcode_numeric == 200003)
                {
                    branch_code = 9;
                }
                if (rcode_numeric == 200004)
                {
                    branch_code = 8;
                }
                if (rcode_numeric == 200005)
                {
                    branch_code = 7;
                }
                if (rcode_numeric == 200006)
                {
                    branch_code = 6;
                }
                if (rcode_numeric == 200007)
                {
                    branch_code = 12;
                }
                if (rcode_numeric == 200008)
                {
                    branch_code = 13;
                }
                if (rcode_numeric == 200009)
                {
                    branch_code = 5;
                }
            }
            else
            {
                string branch_prefix = rcode.ToString().Substring(0, 2);
                switch (branch_prefix.ToUpper())
                {
                    case "AG":
                        branch_code = 1;
                        break;
                    case "EB":
                        branch_code = 3;
                        break;
                    case "MJ":
                        branch_code = 4;
                        break;
                    case "DX":
                        branch_code = 4;
                        break;
                    case "KL":
                        branch_code = 10;
                        break;
                    case "JR":
                        branch_code = 11;
                        break;
                    case "FE":
                        branch_code = 9;
                        break;
                    case "NG":
                        branch_code = 8;
                        break;
                    case "FM":
                        branch_code = 7;
                        break;
                    case "MP":
                        branch_code = 6;
                        break;
                    case "BG":
                        branch_code = 12;
                        break;
                    case "RB":
                        branch_code = 13;
                        break;
                    case "DN":
                        branch_code = 5;
                        break;
                    case "BP":
                        branch_code = 15;
                        break;
                    case "KT":
                        branch_code = 20;
                        break;
                    case "NK":
                        branch_code = 21;
                        break;
                    default:
                        branch_code = 1;
                        break;
                }
            }
            return branch_code;
        }

        private string send_SMS_automatically(string mobileNo, string message)
        {
            Random ran_num = new Random();
            string verification_code = ran_num.Next(100000, 999999).ToString();
            string response = "failed";

            string url = "https://sms.royalcapitalbd.com/api/sendSMS?apikey=AMEYO02G7ZZX&mobile=" +
                mobileNo + "&text=" + message;

            try
            {
                using (var wb = new WebClient())
                {
                    response = wb.DownloadString(url);
                    if (response == "\"Seccess\"")
                    {
                        response = "success";
                    }
                }
            }
            catch (Exception ex)
            {
                response = "failed";
            }
            return response;
        }
    }
}
