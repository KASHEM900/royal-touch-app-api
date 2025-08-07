using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapitalBE.Models
{
    public class IPODetailsInfo
    {
        public IPODetailsInfo(
            string iPOCName,
            string iPOName,
            string lotSize,
            string price,
            string startDate,
            string endDate,
            string tPrice
            )
        {
            this.IPOCName = iPOCName;
            this.IPOName = iPOName;
            this.LotSize = lotSize;
            this.Price = price;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.TPrice = tPrice;
        }
        public string IPOCName { get; set; }
        public string IPOName { get; set; }
        public string LotSize { get; set; }
        public string Price { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string TPrice { get; set; }
    }
}