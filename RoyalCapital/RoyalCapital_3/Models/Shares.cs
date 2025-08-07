using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class Shares
    {
        public Shares(
            string SL,
            string Instrument,
            string Quantit,
            string Amount,
            string Date
            )
        {
            this.SL = SL;
            this.Instrument = Instrument;
            this.Quantit = Quantit;
            this.Amount = Amount;
            this.Date = Date;
        }
        public string SL { get; set; }
        public string Instrument { get; set; }
        public string Quantit { get; set; }
        public string Amount { get; set; }
        public string Date { get; set; }

    }
}
