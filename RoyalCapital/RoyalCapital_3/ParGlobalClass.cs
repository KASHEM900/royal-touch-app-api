
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoyalCapital_3
{
    public class ParGlobalClass
    {
        public static string get_client_code_from_tracking(string tracking_no, string SqlConn)
        {
            using (SqlConnection oConnection = new SqlConnection(SqlConn))
            {
                string query = @"SELECT TOP 1 RCODE FROM T_ONLINE_BO_AccountHolder WHERE TRACKINGNO = @tracking";

                using (SqlCommand oCommand = new SqlCommand(query, oConnection))
                {
                    oCommand.Parameters.Add("@tracking", SqlDbType.VarChar).Value = tracking_no;

                    oConnection.Open();

                    object result = oCommand.ExecuteScalar();

                    return result == null || result == DBNull.Value? null : result.ToString();
                }
            }
        }

        public static void LastUpdateDate(string trackingno, string SqlConn)
        {
            using (SqlConnection oConnection = new SqlConnection(SqlConn))
            {
                string query = @"UPDATE T_ONLINE_BO_REG SET LastUpdate = GETDATE() WHERE TRACKINGNO = @tracking";

                using (SqlCommand oCommand = new SqlCommand(query, oConnection))
                {
                    oCommand.Parameters.Add("@tracking", SqlDbType.VarChar).Value = trackingno;

                    oConnection.Open();
                    oCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
