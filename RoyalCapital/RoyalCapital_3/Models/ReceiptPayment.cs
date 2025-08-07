using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class ReceiptPayment
    {
        public ReceiptPayment(
            string Date,
            string VoucherNo,
            string Deposit,
            string Withdraw
            )
        {
            this.Date = Date;
            this.VoucherNo = VoucherNo;
            this.Deposit = Deposit;
            this.Withdraw = Withdraw;
        }
        public string Date { get; set; }
        public string VoucherNo { get; set; }
        public string Deposit { get; set; }
        public string Withdraw { get; set; }
    }
}
