using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RoyalCapital_3.Models
{
    public class BOBankInfoModel
    {
        public string TrackingNo { get; set; }
        public string BANKNAME { get; set; }
        public string BANKDISTRICT { get; set; }
        public string BANKBRANCH { get; set; }
        public string BANKROUTING { get; set; }
        public string BANKAC { get; set; }

        public string BANKNAME1 { get; set; }
        public string BANKDISTRICT1 { get; set; }
        public string BANKBRANCH1 { get; set; }
        public string BANKROUTING1 { get; set; }
        public string BANKAC1 { get; set; }

        public string TitleText { get; set; }
        public string TitleValue { get; set; }
        public bool val { get; set; }
        //public HttpPostedFileBase File { set; get; }
        //public HttpPostedFileBase FileDef { set; get; }

        public IFormFile File { get; set; }
        public IFormFile FileDef { get; set; }
    }

}
