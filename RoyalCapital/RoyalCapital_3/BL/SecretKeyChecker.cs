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

using System.Text;

using RoyalCapital_3.BL;

namespace RoyalCapital_3.BL
{
    public static class SecretKeyChecker
    {
       public static bool SectetKeyChecker(string rclConn, string mobileNo, string pass)
        {
            string password = "";
            SqlConnection oConnection = new SqlConnection(rclConn);

            try
            {
                oConnection.Open();
                string query = "SELECT CONVERT(varchar(max), DECRYPTBYPASSPHRASE('royal',password)) password FROM RCLWEB.[RCLWEB].[dbo].T_USER_LOGIN WHERE mobile='" + mobileNo.Trim() + "'";
                SqlCommand oCommand = new SqlCommand(query, oConnection);

                DataTable oTable = new DataTable();
                oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

                if (oTable.Rows.Count == 0)
                {
                    return false;
                }

                foreach (DataRow dr in oTable.Rows)
                {
                    password = dr["password"].ToString();
                }

                if (password != pass)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
