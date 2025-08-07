using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using RoyalCapital_3.Models;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace RoyalCapital_3.Models
{
    public class BOAccountHolderModel
    {
        public bool val { get; set; }
        //public decimal TRACKINGNO { get; set; }

        public string TRACKINGNO { get; set; }
        public string RCODE { get; set; }

        public string BOID { get; set; }

        public string ToC { get; set; }

        public string fFirstName { get; set; }

        public string fLastName { get; set; }

        public string fOccupation { get; set; }

        public string fDoB { get; set; }

        public string fTitle { get; set; }
        public string fTitle1 { get; set; }

        public string fFName { get; set; }

        public string fMName { get; set; }

        public string fAddress { get; set; }

        public string fCity { get; set; }

        public string fPostCode { get; set; }

        public string fDivision { get; set; }

        public string fCountry { get; set; }

        public string fMobile { get; set; }
        public string fTel { get; set; }
        public string fFax { get; set; }

        public string fEmail { get; set; }

        public string fNationality { get; set; }

        public string fNID { get; set; }

        public string fTIN { get; set; }


        public string fSex { get; set; }


        public string fResidency { get; set; }


        public string jTitle { get; set; }
        public string jTitle1 { get; set; }


        public string jFastName { get; set; }
        public string jLastName { get; set; }

        public string jOccupation { get; set; }

        public Nullable<System.DateTime> jDoB { get; set; }

        public string jFName { get; set; }

        public string jMName { get; set; }

        public string jAddress { get; set; }

        public string jCity { get; set; }

        public string jPostCode { get; set; }

        public string jDivision { get; set; }

        public string jCountry { get; set; }

        public string jMobile { get; set; }
        public string jTel { get; set; }
        public string jFax { get; set; }
        public string jEmail { get; set; }

        public string jNID { get; set; }

        public string RefarralCode { get; set; }


        public string DesireBranch { get; set; }
        public string DesireBranch1 { get; set; }


        public string IsDirector { get; set; }
        public string DirectorShare { get; set; }


    }
}