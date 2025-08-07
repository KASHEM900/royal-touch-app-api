using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class EFTApprovedRequisition
    {
        public EFTApprovedRequisition(
            string Date,
            string Code,
            string Amount,
            string Status
            )
        {
            this.Date = Date;
            this.Code = Code;
            this.Amount = Amount;
            this.Status = Status;
        }
        public string Date { get; set; }
        public string Code { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
    }
}
