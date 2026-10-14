using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace RoyalCapital_3.Models
{

    public class BOOtherDBHandler
    {
        private readonly string SqlConn;

        public BOOtherDBHandler(IConfiguration config)
        {
            SqlConn = config.GetConnectionString("RCLWEB");
        }

        public IEnumerable<BOBranchInfo> branchInfo()
        {
            var branch = new List<BOBranchInfo>();
            using (SqlConnection oConnection = new SqlConnection(SqlConn))
            {
                oConnection.Open();
                string query = "SELECT BRANCHNAME,[ADDRESS] FROM T_BRANCHES WHERE BRANCHNAME IS NOT NULL AND LTRIM(RTRIM(BRANCHNAME)) <> '' AND BRANCHNAME NOT IN ('Dhaka Ext special','CTG Ext special','WEB','SMS') ORDER BY BRANCHNAME ASC";
                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr in oTable.Rows)
                {
                    branch.Add(new BOBranchInfo
                    {
                        BRANCHNAME = dr["BRANCHNAME"].ToString(),
                        BRANCHADDRESS = dr["ADDRESS"].ToString()
                    });
                }
            }
            return branch;
        }

        public IEnumerable<BOBankInfoModel> bankDistrictInfo(string bankname)
        {
            var bank = new List<BOBankInfoModel>();
            using (SqlConnection oConnection = new SqlConnection(SqlConn))
            {
                oConnection.Open();
                string query = "SELECT DISTINCT [DISTRICT] FROM T_BANKINFO WHERE BANKNAME=@bankname";
                SqlCommand oCommand = new SqlCommand(query, oConnection);
                oCommand.Parameters.AddWithValue("@bankname", bankname.Trim());

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr in oTable.Rows)
                {
                    bank.Add(new BOBankInfoModel { BANKDISTRICT = dr["DISTRICT"].ToString() });
                }
            }
            return bank;
        }

        public IEnumerable<BOBankInfoModel> bankbranchInfo(string bankName, string districtName)
        {
            var bank = new List<BOBankInfoModel>();
            using (SqlConnection oConnection = new SqlConnection(SqlConn))
            {
                oConnection.Open();
                string query = "SELECT [BRANCH] FROM T_BANKINFO WHERE BANKNAME=@bankName AND DISTRICT=@districtName";
                SqlCommand oCommand = new SqlCommand(query, oConnection);
                oCommand.Parameters.AddWithValue("@bankName", bankName.Trim());
                oCommand.Parameters.AddWithValue("@districtName", districtName.Trim());

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr in oTable.Rows)
                {
                    bank.Add(new BOBankInfoModel { BANKBRANCH = dr["BRANCH"].ToString() });
                }
            }
            return bank;
        }

        public IEnumerable<BOBankInfoModel> bankroutingInfo(string bankName, string districtName, string bankBranch)
        {
            var bank = new List<BOBankInfoModel>();
            using (SqlConnection oConnection = new SqlConnection(SqlConn))
            {
                oConnection.Open();
                string query = "SELECT [ROUTING] FROM T_BANKINFO WHERE BANKNAME=@bankName AND DISTRICT=@districtName AND BRANCH=@bankBranch";
                SqlCommand oCommand = new SqlCommand(query, oConnection);
                oCommand.Parameters.AddWithValue("@bankName", bankName.Trim());
                oCommand.Parameters.AddWithValue("@districtName", districtName.Trim());
                oCommand.Parameters.AddWithValue("@bankBranch", bankBranch.Trim());

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr in oTable.Rows)
                {
                    bank.Add(new BOBankInfoModel { BANKROUTING = dr["ROUTING"].ToString() });
                }
            }
            return bank;
        }

        public IEnumerable<BOBankInfoModel> defaultVAlue()
        {
            var bank = new List<BOBankInfoModel>();
            using (SqlConnection oConnection = new SqlConnection(SqlConn))
            {
                oConnection.Open();
                string query = "SELECT TOP 1 [ROUTING],[BRANCH],[DISTRICT] FROM T_BANKINFO";
                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr in oTable.Rows)
                {
                    bank.Add(new BOBankInfoModel
                    {
                        BANKDISTRICT = dr["DISTRICT"].ToString(),
                        BANKBRANCH = dr["BRANCH"].ToString(),
                        BANKROUTING = dr["ROUTING"].ToString()
                    });
                }
            }
            return bank;
        }

        public IEnumerable<BOBankInfoModel> otherBankInfo()
        {
            var bank = new List<BOBankInfoModel>();
            using (SqlConnection oConnection = new SqlConnection(SqlConn))
            {
                oConnection.Open();
                string query = "SELECT [ROUTING],[BRANCH],[DISTRICT] FROM T_BANKINFO";
                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                foreach (DataRow dr in oTable.Rows)
                {
                    bank.Add(new BOBankInfoModel
                    {
                        BANKDISTRICT = dr["DISTRICT"].ToString(),
                        BANKBRANCH = dr["BRANCH"].ToString(),
                        BANKROUTING = dr["ROUTING"].ToString()
                    });
                }
            }
            return bank;
        }

        public byte[] ConvertImageToByteArray(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            using (var ms = new MemoryStream())
            {
                imageFile.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public IEnumerable<BOBankInfoModel> GetTitle()
        {
            return new List<BOBankInfoModel>
                {
                    new BOBankInfoModel { TitleValue = "Mr.", TitleText = "Mr." },
                    new BOBankInfoModel { TitleValue = "Ms.", TitleText = "Ms." },
                    new BOBankInfoModel { TitleValue = "Mrs.", TitleText = "Mrs." },
                    new BOBankInfoModel { TitleValue = "Dr.", TitleText = "Dr." }
                };
        }
    }



    public enum Gender
    {
        Male,
        Female
    }

    public enum Residency
    {
        Resident,
        NonResident
    }
    public enum Title
    {
        Mr,
        Mrs,
        Ms,
        Dr
    }


}