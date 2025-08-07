using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapitalBE.Models
{
    public class CDBLChargeReport
    {
        public CDBLChargeReport(string year, string duration, string amount, string date, string note)
        {
            this.Year = year;
            this.Duration = duration;
            this.Amount = amount;
            this.Date = date;
            this.Note = note;
        }
        public string Year { get; set; }
        public string Duration { get; set; }
        public string Amount { get; set; }
        public string Date { get; set; }
        public string Note { get; set; }
    }
}
