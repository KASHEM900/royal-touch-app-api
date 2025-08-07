using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class Confirmation
    {
        public string LedgerBalBeforeTrading { get; set; }
        public string Receipt { get; set; }
        public string Payment { get; set; }
        public string NetAmtOfTrading { get; set; }
        public string ColsingBalOfDay { get; set; }
        public List<ConfirmationMain> ConfirmationMains { get; set; }
    }
}
