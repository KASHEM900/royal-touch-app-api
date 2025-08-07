using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class LedgerMain
    {
        public LedgerMain(
            string Date,
            string Type,
            string Instrument,
            string Quantity,
            string Rate,
            string Debit,
            string Credit,
            string Commission,
            string Balance
            )
        {
            this.Date = Date;
            this.Type = Type;
            this.Instrument = Instrument;
            this.Quantity = Quantity;
            this.Rate = Rate;
            this.Debit = Debit;
            this.Credit = Credit;
            this.Commission = Commission;
            this.Balance = Balance;
        }
        public string Date { get; set; }
        public string Type { get; set; }
        public string Instrument { get; set; }
        public string Quantity { get; set; }
        public string Rate { get; set; }
        public string Debit { get; set; }
        public string Credit { get; set; }
        public string Commission { get; set; }
        public string Balance { get; set; }
    }
}
