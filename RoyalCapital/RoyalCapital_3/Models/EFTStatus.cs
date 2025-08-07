using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Models
{
    public class EFTStatus
    {
        public List<EFTSumittedRequisition> EFTSumittedRequisitions { get; set; }
        public List<EFTApprovedRequisition> EFTApprovedRequisitions { get; set; }
    }
}
