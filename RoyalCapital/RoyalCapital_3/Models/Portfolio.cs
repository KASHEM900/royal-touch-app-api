using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class Portfolio
    {
        public string AccHolderName { get; set; }
        public string FathersName { get; set; }
        public string MothersName { get; set; }
        public string AccNo { get; set; }
        public string BoId { get; set; }
        public string Address { get; set; }
        public List<PortfolioMain> PortfolioMains{ get; set; }
        public List<PortfolioPendingIPOApp> PortfolioPendingIPOApps{ get; set; }
        public string TotalBuyCost { get; set; }
        public string MarketValue { get; set; }
        public string MaturedBalance { get; set; }
        public string Equity { get; set; }
        public string SaleReceivable { get; set; }
        public string LedgerBalance { get; set; }
        public string RealisedGainLoss { get; set; }
        public string AccredCharges { get; set; }
        public string UnRealisedCapitalGainLoss { get; set; }
        public string ChargesAndFees { get; set; }
        public string TotalCapitalGainLoss { get; set; }
        public TotalAmount MarginableSubTotal { get; set; }
        public TotalAmount NonMarginableSubTotal { get; set; }
        public TotalAmount GrandSubTotal { get; set; }
    }
}
