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
    public class UserViewAllCodesController : ControllerBase
    {
        private readonly IConfiguration configuration;
        
        public UserViewAllCodesController(IConfiguration config)
        {
            configuration = config;
        }
        public IActionResult GetUserViewAllCodes(string mobileNo, string key)
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
                return NotFound(new { error = "invalid user"});
            }

            SqlConnection oConnection = new SqlConnection(sisConn);

            try
            {
                DataSet DS = new DataSet();
                DataTable DT = DS.Tables.Add();
                DT.Columns.Add("SL");
                DT.Columns.Add("RCODE");
                DT.Columns.Add("NAME");
                DT.Columns.Add("Ledger Bal");
                DT.Columns.Add("Mature Bal");
                oConnection.Open();
                string opbal = "SELECT ROW_NUMBER() OVER(ORDER BY a.RCODE) AS sl,  a.RCODE, a.fhName,b.matbalP AS matbalP,b.ldgbal AS ldgbal FROM [RCLWEB].[RCLWEB].[dbo].[T_CLIENT1] a,[SISROYALU].[dbo].[T_Tkbal] b WHERE a.mobile='" + mobileNo.Trim() + "' AND a.boStatus='Active' AND a.RCODE=b.acode UNION ALL SELECT ROW_NUMBER() OVER(ORDER BY a.RCODE) AS sl,  a.RCODE, a.fhName,0 AS matbalP,0 AS ldgbal FROM RCLWEB.[RCLWEB].[dbo].[T_CLIENT1] a WHERE a.boStatus='Active' AND a.mobile='" + mobileNo.Trim() + "' AND a.RCODE NOT IN (SELECT a.RCODE FROM RCLWEB.[RCLWEB].[dbo].[T_CLIENT1] a,[SISROYALU].[dbo].[T_Tkbal] b WHERE a.boStatus='Active' AND a.mobile='" + mobileNo.Trim() + "' AND a.RCODE=b.acode)";
                SqlCommand oCommand = new SqlCommand(opbal, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;

                foreach (DataRow dr in oTable.Rows)
                {
                    r = DT.NewRow();

                    r["SL"] = dr["sl"].ToString();
                    r["RCODE"] = dr["RCODE"].ToString();
                    r["NAME"] = dr["fhName"].ToString();
                    r["Ledger Bal"] = dr["ldgbal"].ToString();
                    r["Mature Bal"] = dr["matbalP"].ToString();

                    DT.Rows.Add(r);
                }

                List<UserViewAllCodes> userViewAllCodesList = new List<UserViewAllCodes>();

                for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                {
                    userViewAllCodesList.Add(new UserViewAllCodes(
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[0]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[1]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[2]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[3]),
                    Convert.ToString(DS.Tables[0].Rows[i].ItemArray[4])
                    ));
                }

                return Ok(userViewAllCodesList);

            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
