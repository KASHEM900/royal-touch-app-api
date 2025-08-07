using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapitalBE.Models
{
    public class AppliedIPOInformation
    {
        public AppliedIPOInformation(
            string sl, 
            string applyDate, 
            string iPOName,
            string mRNo, 
            string aCType,
            string rCVType,
            string applyFrom,
            string appliedStatus,
            string iPOResult
            )
        {
            this.SL = sl;
            this.ApplyDate = applyDate;
            this.IPOName = iPOName;
            this.MRNo = mRNo;
            this.ACType = aCType;
            this.RCVType = rCVType;
            this.ApplyFrom = applyFrom;
            this.AppliedStatus = appliedStatus;
            this.IPOResult = iPOResult;
        }
        public string SL { get; set; }
        public string ApplyDate { get; set; }
        public string IPOName { get; set; }
        public string MRNo { get; set; }
        public string ACType { get; set; }
        public string RCVType { get; set; }
        public string ApplyFrom { get; set; }
        public string AppliedStatus { get; set; }
        public string IPOResult { get; set; }
    }
}