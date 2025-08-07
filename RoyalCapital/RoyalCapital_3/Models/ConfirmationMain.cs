using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class ConfirmationMain
    {
        public ConfirmationMain(
            string Exch,
            string CODE,
            string Instrument,
            string BuyQty,
            string BuyRate,
            string BuyAmt,
            string SaleQty,
            string SaleRate,
            string SaleAmt,
            string BalQty,
            string ComBS,
            string Balance,
            string NAOT
            )
        {
            this.Exch = Exch;
            this.CODE = CODE;
            this.Instrument = Instrument;
            this.BuyQty = BuyQty;
            this.BuyRate = BuyRate;
            this.BuyAmt = BuyAmt;
            this.SaleQty = SaleQty;
            this.SaleRate = SaleRate;
            this.SaleAmt = SaleAmt;
            this.BalQty = BalQty;
            this.ComBS = ComBS;
            this.Balance = Balance;
            this.NAOT = NAOT;
        }
        public string Exch { get; set; }
        public string CODE { get; set; }
        public string Instrument { get; set; }
        public string BuyQty { get; set; }
        public string BuyRate { get; set; }
        public string BuyAmt { get; set; }
        public string SaleQty { get; set; }
        public string SaleRate { get; set; }
        public string SaleAmt { get; set; }
        public string BalQty { get; set; }
        public string ComBS { get; set; }
        public string Balance { get; set; }
        public string NAOT { get; set; }
    }
}
