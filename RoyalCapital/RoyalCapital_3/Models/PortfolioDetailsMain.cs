using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class PortfolioDetailsMain
    {
        public PortfolioDetailsMain(
            string SL,
            string Instrument,
            string BoughtQty,
            string BoughtCost,
            string SoldQty,
            string SoldCost,
            string RealisedGainLoss,
            string BalanceQty,
            string BalanceRate,
            string BalanceAmt,
            string MarketRate,
            string MarketAmt,
            string UnrealisedGain
            )
        {
            this.SL = SL;
            this.Instrument = Instrument;
            this.BoughtQty = BoughtQty;
            this.BoughtCost = BoughtCost;
            this.SoldQty = SoldQty;
            this.SoldCost = SoldCost;
            this.RealisedGainLoss = RealisedGainLoss;
            this.BalanceQty = BalanceQty;
            this.BalanceRate = BalanceRate;
            this.BalanceAmt = BalanceAmt;
            this.MarketRate = MarketRate;
            this.MarketAmt = MarketAmt;
            this.UnrealisedGain = UnrealisedGain;
        }
        public string SL { get; set; }
        public string Instrument { get; set; }
        public string BoughtQty { get; set; }
        public string BoughtCost { get; set; }
        public string SoldQty { get; set; }
        public string SoldCost { get; set; }
        public string RealisedGainLoss { get; set; }
        public string BalanceQty { get; set; }
        public string BalanceRate { get; set; }
        public string BalanceAmt { get; set; }
        public string MarketRate { get; set; }
        public string MarketAmt { get; set; }
        public string UnrealisedGain { get; set; }
    }
}
