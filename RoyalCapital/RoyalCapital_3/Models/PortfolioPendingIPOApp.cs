using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class PortfolioPendingIPOApp
    {
        public PortfolioPendingIPOApp(
            string SL,
            string IPOName,
            string IPOAmount,
            string AppliedDate
            )
        {
            this.SL = SL;
            this.IPOName = IPOName;
            this.IPOAmount = IPOAmount;
            this.AppliedDate = AppliedDate;
        }
        public string SL { get; set; }
        public string IPOName { get; set; }
        public string IPOAmount { get; set; }
        public string AppliedDate { get; set; }
    }
}
