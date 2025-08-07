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
    public class BOAccountHolderAuthorizeModel
    {
        public string TRACKINGNO { get; set; }
        public string aTitle { get; set; }
        public string aTitle1 { get; set; }
        public string aFirstName { get; set; }
        public string aLastName { get; set; }
        public string aOccupation { get; set; }
        public Nullable<System.DateTime> aDoB { get; set; }
        public string aFName { get; set; }
        public string aMName { get; set; }
        public string aAddress { get; set; }
        public string aCity { get; set; }
        public string aPostCode { get; set; }
        public string aDivision { get; set; }
        public string aCountry { get; set; }
        public string aMobile { get; set; }
        public string aTel { get; set; }
        public string aFax { get; set; }
        public string aEmail { get; set; }
        public string aNID { get; set; }

        

    }
}