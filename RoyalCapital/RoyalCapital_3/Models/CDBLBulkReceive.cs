using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class GetCDBLBulkReceive
    {
        public GetCDBLBulkReceive(
            string SL,
            string RCode,
            string Name
        )
        {
            this.SL = SL;
            this.RCode = RCode;
            this.Name= Name;
        }
        public string SL { get; set; }
        public string RCode { get; set; }
        public string Name { get; set; }
    }

    public class UpdateCDBLBulkReceive
    {
        public string mr_no { get; set; }
    }
    public class CDBLBulkReceiveList
    {
        public List<CDBLBulkReceive> CDBLBulkReceive { get; set; }
    }

    public class CDBLBulkReceive
    {
        public string RCode { get; set; }
        public string Name { get; set; }
        public string Year { get; set; }
        public string TotalAmount { get; set; }
        public string StartYear{ get; set; }
        public string EndYear{ get; set; }
    }
}
