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
    public class BOAccountHolderNomineeModel
    {
        public string TRACKINGNO { get; set; }
        public string n1Title { get; set; }
        public string n1Title1 { get; set; }
        public string n1FirstName { get; set; }
        public string n1LastName { get; set; }
        public string n1RelWithACHolder { get; set; }
        public Nullable<decimal> n1Percentage { get; set; }
        public string n1IsResident { get; set; }
        public Nullable<System.DateTime> n1DoB { get; set; }
        public string n1NID { get; set; }
        public string n1Address { get; set; }
        public string n1City { get; set; }
        public string n1PostCode { get; set; }
        public string n1Division { get; set; }
        public string n1Country { get; set; }
        public string n1Mobile { get; set; }
        public string n1Tel { get; set; }
        public string n1Fax { get; set; }
        public string n1Email { get; set; }
        public string n2Title { get; set; }
        public string n2Title1 { get; set; }
        public string n2FirstName { get; set; }
        public string n2LastName { get; set; }
        public string n2RelWithACHolder { get; set; }
        public Nullable<decimal> n2Percentage { get; set; }
        public string n2IsResident { get; set; }
        public Nullable<System.DateTime> n2DoB { get; set; }
        public string n2NID { get; set; }
        public string n2Address { get; set; }
        public string n2City { get; set; }
        public string n2PostCode { get; set; }
        public string n2Division { get; set; }
        public string n2Country { get; set; }
        public string n2Mobile { get; set; }
        public string n2Tel { get; set; }
        public string n2Fax { get; set; }
        public string n2Email { get; set; }
        public string g1Title { get; set; }
        public string g1Title1 { get; set; }
        public string g1FirstName { get; set; }
        public string g1LastName { get; set; }
        public string g1RelWithNominee { get; set; }
        public Nullable<System.DateTime> g1DoBMinor { get; set; }
        public Nullable<System.DateTime> g1MaturityDoM { get; set; }
        public string g1IsResident { get; set; }
        public Nullable<System.DateTime> g1DoB { get; set; }
        public string g1NID { get; set; }
        public string g1Address { get; set; }
        public string g1City { get; set; }
        public string g1PostCode { get; set; }
        public string g1Division { get; set; }
        public string g1Country { get; set; }
        public string g1Mobile { get; set; }
        public string g1Tel { get; set; }
        public string g1Fax { get; set; }
        public string g1Email { get; set; }
        public string g2Title { get; set; }
        public string g2Title1 { get; set; }
        public string g2FirstName { get; set; }
        public string g2LastName { get; set; }
        public string g2RelWithNominee { get; set; }
        public Nullable<System.DateTime> g2DoBMinor { get; set; }
        public Nullable<System.DateTime> g2MaturityDoM { get; set; }
        public string g2IsResident { get; set; }
        public Nullable<System.DateTime> g2DoB { get; set; }
        public string g2NID { get; set; }
        public string g2Address { get; set; }
        public string g2City { get; set; }
        public string g2PostCode { get; set; }
        public string g2Division { get; set; }
        public string g2Country { get; set; }
        public string g2Mobile { get; set; }
        public string g2Tel { get; set; }
        public string g2Fax { get; set; }
        public string g2Email { get; set; }
    }
}