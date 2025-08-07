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
    public class AppliedIPOInformationController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public AppliedIPOInformationController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("")]
        public IActionResult GetAppliedIPOInfo(string aCode, string key)
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
                DT.Columns.Add("Apply Date");
                DT.Columns.Add("IPO Name");
                DT.Columns.Add("MR No");
                DT.Columns.Add("AC Type");
                DT.Columns.Add("RCV Type");
                DT.Columns.Add("Apply From");
                DT.Columns.Add("Applied Status");
                DT.Columns.Add("IPO Result");

                oConnection.Open();

                string qry = "SELECT ROW_NUMBER() OVER(ORDER BY a.ENTRY_DATE desc) AS SL,a.ENTRY_DATE AS 'Apply Date',b.IPO_NAME AS 'IPO Name',CONVERT(VARCHAR(50), MR_NO_M) AS 'MR No',a.ACC_TYPE AS 'AC Type', a.RCV_TYPE AS 'RCV Type',a.APPLY_FROM AS 'Apply From',(CASE WHEN a.CHK_APPROVED=0 THEN 'Rejected' WHEN a.CHK_APPROVED=1 THEN 'Applied' WHEN a.CHK_APPROVED=2 THEN 'Pending' END) AS 'Applied Status',a.IPO_RESULT AS 'IPO Result' FROM RCLWEB.[RCLWEB].[dbo].T_IPO_REC a,RCLWEB.[RCLWEB].[dbo].T_IPO_Details b WHERE a.IPO_SL=b.IPO_SL AND RCODE='" + aCode.ToString() + "'";
                qry += " UNION ";
                qry += "SELECT ROW_NUMBER() OVER(ORDER BY a.ENTRY_DATE desc) AS SL,a.ENTRY_DATE AS 'Apply Date',b.IPO_NAME AS 'IPO Name',MR_NO_M AS 'MR No',a.ACC_TYPE AS 'AC Type', a.RCV_TYPE AS 'RCV Type',a.APPLY_FROM AS 'Apply From',(CASE WHEN a.CHK_APPROVED=0 THEN 'Rejected' WHEN a.CHK_APPROVED=1 THEN 'Applied' WHEN a.CHK_APPROVED=2 THEN 'Pending' END) AS 'Applied Status',a.IPO_RESULT AS 'IPO Result' FROM RCLWEB.[RCLWEB].[dbo].T_IPO_REC_MASTER a,T_IPO_Details b WHERE a.IPO_SL=b.IPO_SL AND RCODE='" + aCode.ToString() + "'";
                qry += " order by a.ENTRY_DATE desc ";
                SqlCommand oCommand = new SqlCommand(qry, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;

                foreach (DataRow dr in oTable.Rows)
                { 
                    r = DT.NewRow();

                    r["SL"] = dr["SL"].ToString();
                    r["Apply Date"] = dr["Apply Date"].ToString();
                    r["IPO Name"] = dr["IPO Name"].ToString();
                    r["MR No"] = dr["MR No"].ToString();
                    r["AC Type"] = dr["AC Type"].ToString();
                    r["RCV Type"] = dr["RCV Type"].ToString();
                    r["Apply From"] = dr["Apply From"].ToString();
                    r["Applied Status"] = dr["Applied Status"].ToString();
                    r["IPO Result"] = dr["IPO Result"].ToString();

                    DT.Rows.Add(r);
                }


                List<AppliedIPOInformation> appliedIPOInformationList = new List<AppliedIPOInformation>();

                for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                {
                    appliedIPOInformationList.Add(new AppliedIPOInformation(
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[4]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[5]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[6]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[7]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[8])
                    ));
                }


                return Ok(appliedIPOInformationList);
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }

        }
    }
}
