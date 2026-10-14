using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace RoyalCapital_3.Data
{
    public class SP_CALL : ISP_CALL
    {
        private readonly ApplicationDbContext _db;

        private readonly SISRoyalDbContext _sisDb;
        private static string ConnectionString = "";
        private static string ConnectionString2 = "";
        public SP_CALL(ApplicationDbContext db, SISRoyalDbContext sisDb)
        {
            _db = db;
            _sisDb = sisDb;
            ConnectionString = db.Database.GetDbConnection().ConnectionString;
            ConnectionString2 = sisDb.Database.GetDbConnection().ConnectionString;
        }
        public void Dispose()
        {
            _db.Dispose();
        }

        public List<T> ListByRawQuery<T>(string sql)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return connection.Query<T>(sql).ToList();              
            }
        }

        public List<T> ListByRawQueryBySis<T>(string sql)
        {
            using (var connection = new SqlConnection(ConnectionString2))
            {
                return connection.Query<T>(sql).ToList();              
            }
        }
    }
}
