using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class EFTSumittedRequisition
    {
        public EFTSumittedRequisition(
            string Date,
            string Code,
            string Amount,
            string Status,
            string Registered,
            string Deleted,
            string IF
            )
        {
            this.Date = Date;
            this.Code = Code;
            this.Amount = Amount;
            this.Status = Status;
            this.Registered = Registered;
            this.Deleted = Deleted;
            this.IF = IF;
        }
        public string Date { get; set; }
        public string Code { get; set; }
        public string Amount { get; set; }
        public string Status { get; set; }
        public string Registered { get; set; }
        public string Deleted { get; set; }
        public string IF { get; set; }
    }
}
