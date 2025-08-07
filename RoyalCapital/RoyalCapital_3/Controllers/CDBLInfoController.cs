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
    public class CDBLInfoController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public CDBLInfoController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("info")]
        public IActionResult GetCDBLInfo(string aCode, string key)
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
                DataTable DT = DS.Tables.Add("BOAck");
                DT.Columns.Add("BOID");
                DT.Columns.Add("ToC");
                DT.Columns.Add("CoA");
                DT.Columns.Add("fhName");
                DT.Columns.Add("jhName");
                DT.Columns.Add("thName");
                DT.Columns.Add("cp");
                DT.Columns.Add("fhSex");
                DT.Columns.Add("fhDOB");
                DT.Columns.Add("fhRegNo");
                DT.Columns.Add("fhFather");
                DT.Columns.Add("fhMother");
                DT.Columns.Add("fhOccupation");
                DT.Columns.Add("fhResidency");
                DT.Columns.Add("fhCountry");
                DT.Columns.Add("fhAddress");
                DT.Columns.Add("fhCity");
                DT.Columns.Add("fhDivision");
                DT.Columns.Add("fhPCode");
                DT.Columns.Add("fhMobile");
                DT.Columns.Add("fhFax");
                DT.Columns.Add("fhEmail");
                DT.Columns.Add("SCyCode");
                DT.Columns.Add("fhNationality");
                DT.Columns.Add("fhComRegDate");
                DT.Columns.Add("fhBank");
                DT.Columns.Add("fhBankBranch");
                DT.Columns.Add("fhBankAC");
                DT.Columns.Add("fhRouting");
                DT.Columns.Add("ElectDividend");
                DT.Columns.Add("PassportNo");
                DT.Columns.Add("PassportIssueDate");
                DT.Columns.Add("PassportExpiryDate");
                DT.Columns.Add("PassportIssuePlace");
                DT.Columns.Add("TaxExemption");
                DT.Columns.Add("TaxIdentficationNo");
                DT.Columns.Add("ExchangeName");
                DT.Columns.Add("TradingID");
                DT.Columns.Add("ACODE");
                DT.Columns.Add("oDate");

                oConnection.Open();

                SqlCommand oCommond = new SqlCommand("SELECT * FROM RCLWEB.[RCLWEB].[dbo].T_CLIENT1 WHERE RCODE=@acode", oConnection);
                oCommond.Parameters.Add("@acode", SqlDbType.Char).Value = aCode.ToString();

                DataTable oTable = new DataTable();
                oTable.Load(oCommond.ExecuteReader(CommandBehavior.CloseConnection));

                DataRow r;

                foreach (DataRow dr in oTable.Rows)
                {
                    r = DT.NewRow();

                    r["BOID"] = dr["boid"].ToString();
                    r["ToC"] = dr["botype"].ToString();
                    r["CoA"] = dr["bocat"].ToString();
                    r["fhName"] = dr["fhName"].ToString();
                    r["jhName"] = dr["shName"].ToString();
                    r["thName"] = dr["thName"].ToString();
                    r["cp"] = dr["cpName"].ToString();
                    r["fhSex"] = dr["sex"].ToString();
                    r["fhDOB"] = dr["dob"].ToString();
                    r["fhRegNo"] = dr["regNum"].ToString();
                    r["fhFather"] = dr["father"].ToString();
                    r["fhMother"] = dr["mother"].ToString();
                    r["fhOccupation"] = dr["occupation"].ToString();
                    r["fhResidency"] = dr["residency"].ToString();
                    r["fhCountry"] = dr["country"].ToString();
                    r["fhAddress"] = dr["add1"].ToString() + dr["add2"].ToString() + dr["add3"].ToString();
                    r["fhCity"] = dr["city"].ToString();
                    r["fhDivision"] = dr["city"].ToString();
                    r["fhPCode"] = dr["pCode"].ToString();
                    r["fhMobile"] = dr["mobile1"].ToString();
                    r["fhFax"] = dr["fax"].ToString();
                    r["fhEmail"] = dr["email"].ToString().Trim();
                    r["SCyCode"] = dr["scc"].ToString();
                    r["fhNationality"] = dr["nationality"].ToString();
                    r["fhComRegDate"] = dr["regNum"].ToString();
                    r["fhBank"] = dr["bName"].ToString();
                    r["fhBankBranch"] = dr["bBranch"].ToString();
                    r["fhBankAC"] = dr["bAccNum"].ToString();
                    r["fhRouting"] = dr["routing"].ToString();
                    r["ElectDividend"] = dr["eID"].ToString();
                    r["PassportNo"] = dr["passNum"].ToString();
                    r["PassportIssueDate"] = dr["passIDate"].ToString();
                    r["PassportExpiryDate"] = dr["passExDate"].ToString();
                    r["PassportIssuePlace"] = dr["passIPlace"].ToString();
                    r["TaxExemption"] = dr["taxStatus"].ToString();
                    r["TaxIdentficationNo"] = dr["tID"].ToString();
                    r["ExchangeName"] = "DSE and CSE";
                    r["TradingID"] = dr["RCODE"].ToString(); ;
                    r["ACODE"] = dr["RCODE"].ToString();
                    r["oDate"] = dr["setupDate"].ToString();

                    DT.Rows.Add(r);
                }

                //SqlDataAdapter DA = new SqlDataAdapter(oCommond);
                //DataSet DS = new DataSet();
                // DA.Fill(DS);

                //GridViewCDBLChargeReport.DataSource = DS.Tables[0];
                //GridViewCDBLChargeReport.DataBind();

                //List<string> cdblChargeReport = new List<string>();

                //for(int i=0; i<DS.Tables[0].Rows.Count; i++)
                //{
                //    cdblChargeReport.Add(DS.Tables[0].Rows[i][0]);
                //}


                CDBLInfo cdblInfo = new CDBLInfo();


                cdblInfo.BoId = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[0]);
                cdblInfo.BoType = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[1]);
                cdblInfo.BoCategory = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[2]);
                cdblInfo.FirstHolderName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[3]);
                cdblInfo.SecondHolderName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[4]);
                cdblInfo.ThirdHolderName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[5]);
                cdblInfo.ContactPerson = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[6]);
                cdblInfo.Sex = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[7]);
                cdblInfo.DOB = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[8]);
                cdblInfo.RegNo = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[9]);
                cdblInfo.FatherName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[10]);
                cdblInfo.MotherName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[11]);
                cdblInfo.Occupation = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[12]);
                cdblInfo.Residency = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[13]);
                cdblInfo.Country = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[14]);
                cdblInfo.Address = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[15]);
                cdblInfo.City = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[16]);
                cdblInfo.Division = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[17]);
                cdblInfo.PostalCode = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[18]);
                cdblInfo.Mobile = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[19]);
                cdblInfo.Fax = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[20]);
                cdblInfo.Email = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[21]);
                cdblInfo.StatementCycleCode = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[22]);
                cdblInfo.Nationality = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[23]);
                cdblInfo.ComRegDate = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[24]);
                cdblInfo.Bank = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[25]);
                cdblInfo.BankBranch = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[26]);
                cdblInfo.BankAc = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[27]);
                cdblInfo.Routing = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[28]);
                cdblInfo.ElectronicDivident = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[29]);
                cdblInfo.PassportNo = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[30]);
                cdblInfo.PassportIssueDate = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[31]);
                cdblInfo.PassportExpiryDate = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[32]);
                cdblInfo.PassportIssuePlace = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[33]);
                cdblInfo.TaxExemption = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[34]);
                cdblInfo.TaxIdentficationNo = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[35]);
                cdblInfo.ExchangeName = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[36]);
                cdblInfo.TradingID = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[37]);
                cdblInfo.ACode = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[38]);
                cdblInfo.ODate = Convert.ToString(DS.Tables[0].Rows[0].ItemArray[39]);

                Console.WriteLine(cdblInfo);


                return Ok(cdblInfo);

            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
