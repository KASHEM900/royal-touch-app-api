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
    public class BOOnlineBODetailModel
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


        public string BRANCHNAME { get; set; }
        public string BRANCHADDRESS { get; set; }


        public string BANKNAME { get; set; }

        public string BANKBRANCH { get; set; }

        public string BANKDISTRICT { get; set; }

        public string ROUTING { get; set; }

        //[StringLength(13, ErrorMessage = "Please enter a 13 digit Bank A/C Number.", MinimumLength = 13)]
        //[RegularExpression(@"^[0-9]+$", ErrorMessage = "Invalid Bank A/C Number.")]
        public string AC { get; set; }


        //// Authorize
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

        //// Nominee
        public string n1Title { get; set; }
        public string n1Title1 { get; set; }
        public string n1FirstName { get; set; }
        public string n1LastName { get; set; }
        public string n1RelWithACHolder { get; set; }
        public Nullable<decimal> n1Percentage { get; set; }
        public string n1IsResident { get; set; }

        // [DataType(DataType.Date)]
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

        ////images
        public byte[] fImage { get; set; }
        public byte[] fNIDFont { get; set; }
        public byte[] fNIDBack { get; set; }
        public byte[] fSig { get; set; }
        public byte[] jImage { get; set; }
        public byte[] jNIDFont { get; set; }
        public byte[] jNIDBack { get; set; }
        public byte[] jSig { get; set; }
        public byte[] aImage { get; set; }
        public byte[] aNIDFont { get; set; }
        public byte[] aNIDBack { get; set; }
        public byte[] aSig { get; set; }
        public byte[] n1Image { get; set; }
        public byte[] n1NIDFont { get; set; }
        public byte[] n1NIDBack { get; set; }
        public byte[] n1Sig { get; set; }
        public byte[] n2Image { get; set; }
        public byte[] n2NIDFont { get; set; }
        public byte[] n2NIDBack { get; set; }
        public byte[] n2Sig { get; set; }
        public byte[] g1Image { get; set; }
        public byte[] g1NIDFont { get; set; }
        public byte[] g1NIDBack { get; set; }
        public byte[] g1Sig { get; set; }
        public byte[] g2Image { get; set; }
        public byte[] g2NIDFont { get; set; }
        public byte[] g2NIDBack { get; set; }
        public byte[] g2Sig { get; set; }



        public string fullName { get; set; }
        public int DateFrom { get; set; }
        public int DateTo { get; set; }

        public int BOCHARGE { get; set; }
        public int CDBLCHARGE { get; set; }
        public int TOTALCHARGE { get; set; }


        public string MR_NO { get; set; }
        public string NAME { get; set; }
        public string DATE { get; set; }
        public string fis { get; set; }
        public string tamnt { get; set; }

        public string amount { get; set; }


        /// <summary>
        /// /////////  Payment End
        /// </summary>


        public string IsDirector { get; set; }
        public string DirectorShare { get; set; }

        public string bkash_result { get; set; }
        public string bkash_data { get; set; }

    }
}