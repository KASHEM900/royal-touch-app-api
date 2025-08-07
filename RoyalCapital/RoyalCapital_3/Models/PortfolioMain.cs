using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class PortfolioMain
    {
        public PortfolioMain(
            string SL, 
            string Instrument, 
            string Total, 
            string Free, 
            string Lock,
            string AvgPrice,
            string TotalCost,
            string MarketRate,
            string MarketValue,
            string GainLoss,
            string Percentage,
            string Margin
            )
        {
            this.SL = SL;
            this.Instrument = Instrument;
            this.Total = Total;
            this.Free = Free;
            this.Lock = Lock;
            this.AvgPrice = AvgPrice;
            this.TotalCost = TotalCost;
            this.MarketRate = MarketRate;
            this.MarketValue = MarketValue;
            this.GainLoss = GainLoss;
            this.Percentage = Percentage;
            this.Margin = Margin;
        }
        public string SL { get; set; }
        public string Instrument { get; set; }
        public string Total { get; set; }
        public string Free { get; set; }
        public string Lock { get; set; }
        public string AvgPrice { get; set; }
        public string TotalCost { get; set; }
        public string MarketRate { get; set; }
        public string MarketValue { get; set; }
        public string GainLoss { get; set; }
        public string Percentage { get; set; }
        public string Margin { get; set; }

    }
}
