using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapitalBE.Models
{
    public class IPOInitialInfo
    {
        public IPOInitialInfo(
            string sl,
            string iPOName
            )
        {
            this.SL = sl;
            this.IPOName = iPOName;
        }
        public string SL { get; set; }
        public string IPOName { get; set; }
    }
}