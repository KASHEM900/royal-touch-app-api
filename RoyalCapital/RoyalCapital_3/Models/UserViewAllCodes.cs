using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class UserViewAllCodes
    {
        public UserViewAllCodes(
        string SL,
        string RCode,
        string Name,
        string LedgerBal,
        string MatureBal
        )
        {
            this.SL = SL;
            this.RCode = RCode;
            this.Name = Name;
            this.LedgerBal = LedgerBal;
            this.MatureBal = MatureBal;
        }
        public string SL { get; set; }
        public string RCode { get; set; }
        public string Name { get; set; }
        public string LedgerBal { get; set; }
        public string MatureBal { get; set; }
        
    }
}
