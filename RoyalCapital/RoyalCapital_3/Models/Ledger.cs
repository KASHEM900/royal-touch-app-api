using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class Ledger
    {
        public string AccHolderName { get; set; }
        public string FathersName { get; set; }
        public string MothersName { get; set; }
        public string AccNo { get; set; }
        public string BoId { get; set; }
        public string Address { get; set; }
        public List<LedgerMain> LedgerMains { get; set; }

    }
}
