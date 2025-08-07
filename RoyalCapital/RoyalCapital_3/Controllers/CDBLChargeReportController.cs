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
using RoyalCapital_3.BL;

namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CDBLChargeReportController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public CDBLChargeReportController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("code")]
        public IActionResult GetCDBLChargeReport(string aCode, string key)
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

            string conn = configuration.GetConnectionString("RCLWEB");

            SqlConnection oConnection = new SqlConnection(conn);

            try
            {

                DataSet DS = new DataSet();
                DataTable DT = DS.Tables.Add();
                DT.Columns.Add("Year");
                DT.Columns.Add("Duration");
                DT.Columns.Add("Amount");
                DT.Columns.Add("Date");
                DT.Columns.Add("Note");

                oConnection.Open();

                SqlCommand oCommond = new SqlCommand("select RCODE,[YEAR],FISCAL,AMOUNT,(convert(varchar(10),[DATE],101)) dat,NOTE from RCLWEB.[RCLWEB].[dbo].T_CDBL_CHARGE WHERE rcode=@acode ORDER BY [YEAR] DESC", oConnection);
                oCommond.Parameters.Add("@acode", SqlDbType.Char).Value = aCode.ToString();

                DataTable oTable = new DataTable();
                oTable.Load(oCommond.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;

                foreach (DataRow dr in oTable.Rows)
                {
                    r = DT.NewRow();

                    r["Year"] = dr["YEAR"].ToString();
                    r["Duration"] = dr["FISCAL"].ToString();
                    r["Amount"] = string.Format("{0:#,#}", dr["AMOUNT"]).ToString();
                    r["Date"] = dr["dat"].ToString();
                    r["Note"] = dr["NOTE"].ToString();

                    DT.Rows.Add(r);
                }

                List<CDBLChargeReport> cdblChargeReportList = new List<CDBLChargeReport>();

                for(int i=0; i< DS.Tables[0].Rows.Count; i++)
                {
                    cdblChargeReportList.Add(new CDBLChargeReport(
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[4])
                    ));
                }

                return Ok(cdblChargeReportList);

            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
 