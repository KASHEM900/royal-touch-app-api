using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class PortfolioDetails
    {
        public string AccHolderName { get; set; }
        public string FathersName { get; set; }
        public string MothersName { get; set; }
        public string AccNo { get; set; }
        public string BoId { get; set; }
        public string Address { get; set; }
        public string JointHolder { get; set; }


        public string OpeningShareBal { get; set; }
        public string LedgerBal { get; set; }
        public string PortfolioValueMarketPrice { get; set; }
        public string PortfolioValueCostPric { get; set; }
        public string Deposit { get; set; }
        public string WithdrawnAmount { get; set; }
        public string Charges { get; set; }
        public string NetDeposit { get; set; }
        public string RealisedCapitalGainLoss { get; set; }


        public List<PortfolioDetailsMain> portfolioDetailsMains { get; set; }
        public List<Shares> portfolioDetailsIPO { get; set; }
        public List<Shares> portfolioDetailsBonus { get; set; }
        public List<Shares> portfolioDetailsRight { get; set; }

    }
}
