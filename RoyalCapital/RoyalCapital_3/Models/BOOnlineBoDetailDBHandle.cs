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
using Microsoft.AspNetCore.Http;

namespace RoyalCapital_3.Models
{
    public class BOOnlineBoDetailDBHandle
    {
        private readonly string SqlConn;

        public BOOnlineBoDetailDBHandle(IConfiguration config)
        {
            SqlConn = config.GetConnectionString("RCLWEB");
        }

        public IEnumerable<BOOnlineBODetailModel> AccHolderSelect(string id)
        {
            var acc = new List<BOOnlineBODetailModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();

            string query = "SELECT [dbo].[T_ONLINE_BO_AccountHolder].[TRACKINGNO],[RCODE],[ToC],[fFirstName],[fLastName],[fOccupation],[fDoB],[fTitle],[fFName],[fMName],[fAddress],[fCity],[fPostCode],[fDivision],[fCountry],[fMobile],[fTel],[fFax],[fEmail],[fNationality],[fNID],[fTIN],[fSex],[fResidency],[jTitle],[jFirstName],[jLastName],[jOccupation],[jDoB],[jFName],[jMName],[jAddress],[jCity],[jPostCode],[jDivision],[jCountry],[jMobile],[jTel],[jFax],[jEmail],[jNID],[DesireBranch],[IsDirector],[DirectorShare],[T_ONLINE_BO_REG].[pDate],[fIsComplete],[T_ONLINE_BO_REG].[RefarralCode],[fSpouseName] FROM [dbo].[T_ONLINE_BO_AccountHolder],[dbo].[T_ONLINE_BO_REG] WHERE [dbo].[T_ONLINE_BO_AccountHolder].TRACKINGNO=[dbo].[T_ONLINE_BO_REG].TRACKINGNO AND [dbo].[T_ONLINE_BO_AccountHolder].TRACKINGNO=@id";
            SqlCommand oCommand = new SqlCommand(query, oConnection);
            oCommand.Parameters.AddWithValue("@id", id);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {

                acc.Add(new BOOnlineBODetailModel
                { RCODE = dr["RCODE"].ToString(), ToC = dr["ToC"].ToString(), fFirstName = dr["fFirstName"].ToString(), 
                    fLastName = dr["fLastName"].ToString(), fOccupation = dr["fOccupation"].ToString(),
                    RefarralCode = dr["RefarralCode"].ToString(),fSpouseName = dr["fSpouseName"].ToString(),
                    fDoB = (dr["fDoB"] == DBNull.Value ? Convert.ToDateTime("1/1/1900").ToString("dd/MM/yyyy") : Convert.ToDateTime(dr["fDoB"]).ToString("dd/MM/yyyy")), 
                    fTitle1 = dr["fTitle"].ToString(), fFName = dr["fFName"].ToString(), fMName = dr["fMName"].ToString(), fAddress = dr["fAddress"].ToString(), fCity = dr["fCity"].ToString(), fPostCode = dr["fPostCode"].ToString(), fDivision = dr["fDivision"].ToString(), fCountry = dr["fCountry"].ToString(), fMobile = dr["fMobile"].ToString(), fTel = dr["fTel"].ToString(), fFax = dr["fFax"].ToString(), fEmail = dr["fEmail"].ToString(), fNationality = dr["fNationality"].ToString(), fNID = dr["fNID"].ToString(), fTIN = dr["fTIN"].ToString(), fSex = dr["fSex"].ToString(), fResidency = dr["fResidency"].ToString(), jTitle1 = dr["jTitle"].ToString(), jFastName = dr["jFirstName"].ToString(), jLastName = dr["jLastName"].ToString(), jOccupation = dr["jOccupation"].ToString(), jDoB = (dr["jDoB"] == DBNull.Value ? Convert.ToDateTime("1/1/1900").ToString("dd/MM/yyyy") : Convert.ToDateTime(dr["fDoB"]).ToString("dd/MM/yyyy")), jFName = dr["jFName"].ToString(), jMName = dr["jMName"].ToString(), jAddress = dr["jAddress"].ToString(), jCity = dr["jCity"].ToString(), jPostCode = dr["jPostCode"].ToString(), jDivision = dr["jDivision"].ToString(), jCountry = dr["jCountry"].ToString(), jMobile = dr["jMobile"].ToString(), jTel = dr["jTel"].ToString(), jFax = dr["jFax"].ToString(), jEmail = dr["jEmail"].ToString(), jNID = dr["jNID"].ToString(), DesireBranch = dr["DesireBranch"].ToString(), IsDirector = dr["IsDirector"].ToString(), DirectorShare = dr["DirectorShare"].ToString() });
            }
            return acc.ToList();
        }


        public IEnumerable<BOOnlineBODetailModel> AccBankInfo(string id)
        {
            var acc = new List<BOOnlineBODetailModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();


            string query = "SELECT [TRACKINGNO],[BANKNAME],[BANKBRANCH],[DISTRICT],[ROUTING],[AC],[pDate],[bIsComplete] FROM [dbo].[T_ONLINE_BO_AccHolderBANK] WHERE TRACKINGNO=@id";

            SqlCommand oCommand = new SqlCommand(query, oConnection);
            oCommand.Parameters.AddWithValue("@id",id);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {

                acc.Add(new BOOnlineBODetailModel { BANKNAME = dr["BANKNAME"].ToString(), BANKBRANCH = dr["BANKBRANCH"].ToString(), BANKDISTRICT = dr["DISTRICT"].ToString(), ROUTING = dr["ROUTING"].ToString(), AC = dr["AC"].ToString() });
            }
            return acc.ToList();
        }

        public IEnumerable<BOAccountHolderBranchTypeAC> AccBranchInfo(string id)
        {
            var acc = new List<BOAccountHolderBranchTypeAC>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();


            string query = @"SELECT A.[DesireBranch], A.[ToC], R.[RefarralCode] FROM dbo.T_ONLINE_BO_AccountHolder A INNER JOIN dbo.T_ONLINE_BO_REG R
                    ON A.[TRACKINGNO] = R.[TRACKINGNO] WHERE A.[TRACKINGNO] = @id";

            SqlCommand oCommand = new SqlCommand(query, oConnection);
            oCommand.Parameters.AddWithValue("@id", id);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {

                acc.Add(new BOAccountHolderBranchTypeAC { DesireBranch = dr["DesireBranch"].ToString(), ToC = dr["ToC"].ToString(), 
                    RefarralCode = dr["RefarralCode"].ToString() });
            }
            return acc.ToList();
        }

        public IEnumerable<BoSecondAccHolderAddressModel> AccAddressInfo(string id)
        {
            var acc = new List<BoSecondAccHolderAddressModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();


            string query = @"SELECT [fAddress],[fPostCode],[fCity],[fDivision],[fCountry] FROM dbo.T_ONLINE_BO_AccountHolder WHERE [TRACKINGNO] = @id";

            SqlCommand oCommand = new SqlCommand(query, oConnection);
            oCommand.Parameters.AddWithValue("@id", id);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {

                acc.Add(new BoSecondAccHolderAddressModel
                {
                    fAddress = dr["fAddress"].ToString(),
                    fPostCode = dr["fPostCode"].ToString(),
                    fCity = dr["fCity"].ToString(),
                    fDivision = dr["fDivision"].ToString(),
                    fCountry = dr["fCountry"].ToString()
                });
            }
            return acc.ToList();
        }

        public IEnumerable<BOOnlineBODetailModel> AccAuInfo(string id)
        {
            var acc = new List<BOOnlineBODetailModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();

            string query = "SELECT [TRACKINGNO],[aTitle],[aFirstName],[aLastName],[aOccupation],[aDoB],[aFName],[aMName],[aAddress],[aCity],[aPostCode],[aDivision],[aCountry],[aMobile],[aTel],[aFax],[aEmail],[aNID],[pDate],[aIsComplete] FROM [dbo].[T_ONLINE_BO_AccHolderAuthorize] WHERE TRACKINGNO=@id";

            SqlCommand oCommand = new SqlCommand(query, oConnection);
            oCommand.Parameters.AddWithValue("@id", id);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {

                acc.Add(new BOOnlineBODetailModel { aTitle1 = dr["aTitle"].ToString(), aFirstName = dr["aFirstName"].ToString(), aLastName = dr["aLastName"].ToString(), aOccupation = dr["aOccupation"].ToString(), aDoB = (dr["aDoB"] == DBNull.Value ? Convert.ToDateTime("1/1/1900") : Convert.ToDateTime(dr["aDoB"])), aFName = dr["aFName"].ToString(), aMName = dr["aMName"].ToString(), aAddress = dr["aAddress"].ToString(), aCity = dr["aCity"].ToString(), aPostCode = dr["aPostCode"].ToString(), aDivision = dr["aDivision"].ToString(), aCountry = dr["aCountry"].ToString(), aMobile = dr["aMobile"].ToString(), aTel = dr["aTel"].ToString(), aFax = dr["aFax"].ToString(), aEmail = dr["aEmail"].ToString(), aNID = dr["aNID"].ToString() });
            }
            return acc.ToList();
        }


        public IEnumerable<BOOnlineBODetailModel> AccNoInfo(string id)
        {
            var acc = new List<BOOnlineBODetailModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();

            string query = "SELECT [n1Title],[n1FirstName],[n1LastName],[n1RelWithACHolder],ISNULL([n1Percentage],0) n1Percentage,[n1IsResident],ISNULL([n1DoB],'1/1/1900') n1DoB,[n1NID],[n1Address],[n1City],[n1PostCode],[n1Division],[n1Country],[n1Mobile],[n1Tel],[n1Fax],[n1Email],[n2Title],[n2FirstName],[n2LastName],[n2RelWithACHolder],ISNULL([n2Percentage],0) n2Percentage,[n2IsResident],ISNULL([n2DoB],'1/1/1900') n2DoB,[n2NID],[n2Address],[n2City],[n2PostCode],[n2Division],[n2Country],[n2Mobile],[n2Tel],[n2Fax],[n2Email],[g1Title],[g1FirstName],[g1LastName],[g1RelWithNominee],ISNULL([g1DoBMinor],'1/1/1900') g1DoBMinor,ISNULL([g1MaturityDoM],'1/1/1900') g1MaturityDoM,[g1IsResident],ISNULL([g1DoB],'1/1/1900') g1DoB,[g1NID],[g1Address],[g1City],[g1PostCode],[g1Division],[g1Country],[g1Mobile],[g1Tel],[g1Fax],[g1Email],[g2Title],[g2FirstName],[g2LastName],[g2RelWithNominee],ISNULL([g2DoBMinor],'1/1/1900') g2DoBMinor,ISNULL([g2MaturityDoM],'1/1/1900') g2MaturityDoM,[g2IsResident],ISNULL([g2DoB],'1/1/1900') g2DoB,[g2NID],[g2Address],[g2City],[g2PostCode],[g2Division],[g2Country],[g2Mobile],[g2Tel],[g2Fax],[g2Email] FROM [dbo].[T_ONLINE_BO_AccHolderNominee] WHERE TRACKINGNO=@id";
            SqlCommand oCommand = new SqlCommand(query, oConnection);
            oCommand.Parameters.AddWithValue("@id", id);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {
                //    
                try
                {
                    acc.Add(new BOOnlineBODetailModel
                    {
                        n1Title1 = dr["n1Title"].ToString(),
                        n1FirstName = dr["n1FirstName"].ToString(),
                        n1LastName = dr["n1LastName"].ToString(),
                        n1RelWithACHolder = dr["n1RelWithACHolder"].ToString(),
                        n1Percentage = Convert.ToDecimal(dr["n1Percentage"].ToString()),
                        n1IsResident = dr["n1IsResident"].ToString(),
                        n1DoB = (Convert.ToDateTime(dr["n1DoB"].ToString())),
                        n1NID = dr["n1NID"].ToString(),
                        n1Address = dr["n1Address"].ToString(),
                        n1City = dr["n1City"].ToString(),
                        n1PostCode = dr["n1PostCode"].ToString(),
                        n1Division = dr["n1Division"].ToString(),
                        n1Country = dr["n1Country"].ToString(),
                        n1Mobile = dr["n1Mobile"].ToString(),
                        n1Tel = dr["n1Tel"].ToString(),
                        n1Fax = dr["n1Fax"].ToString(),
                        n1Email = dr["n1Email"].ToString(),
                        n2Title1 = dr["n2Title"].ToString(),
                        n2FirstName = dr["n2FirstName"].ToString(),
                        n2LastName = dr["n2LastName"].ToString(),
                        n2RelWithACHolder = dr["n2RelWithACHolder"].ToString(),
                        n2Percentage = Convert.ToDecimal(dr["n2Percentage"].ToString()),
                        n2IsResident = dr["n2IsResident"].ToString(),
                        n2DoB = (Convert.ToDateTime(dr["n2DoB"].ToString())),
                        n2NID = dr["n2NID"].ToString(),
                        n2Address = dr["n2Address"].ToString(),
                        n2City = dr["n2City"].ToString(),
                        n2PostCode = dr["n2PostCode"].ToString(),
                        n2Division = dr["n2Division"].ToString(),
                        n2Country = dr["n2Country"].ToString(),
                        n2Mobile = dr["n2Mobile"].ToString(),
                        n2Tel = dr["n2Tel"].ToString(),
                        n2Fax = dr["n2Fax"].ToString(),
                        n2Email = dr["n2Email"].ToString(),
                        g1Title1 = dr["g1Title"].ToString(),
                        g1FirstName = dr["g1FirstName"].ToString(),
                        g1LastName = dr["g1LastName"].ToString(),
                        g1RelWithNominee = dr["g1RelWithNominee"].ToString(),
                        g1DoBMinor = (Convert.ToDateTime(dr["g1DoBMinor"].ToString())),
                        g1MaturityDoM = (Convert.ToDateTime(dr["g1MaturityDoM"].ToString())),
                        g1IsResident = dr["g1IsResident"].ToString(),
                        g1DoB = (Convert.ToDateTime(dr["g1DoB"].ToString())),
                        g1NID = dr["g1NID"].ToString(),
                        g1Address = dr["g1Address"].ToString(),
                        g1City = dr["g1City"].ToString(),
                        g1PostCode = dr["g1PostCode"].ToString(),
                        g1Division = dr["g1Division"].ToString(),
                        g1Country = dr["g1Country"].ToString(),
                        g1Mobile = dr["g1Mobile"].ToString(),
                        g1Tel = dr["g1Tel"].ToString(),
                        g1Fax = dr["g1Fax"].ToString(),
                        g1Email = dr["g1Email"].ToString(),
                        g2Title1 = dr["g2Title"].ToString(),
                        g2FirstName = dr["g2FirstName"].ToString(),
                        g2LastName = dr["g2LastName"].ToString(),
                        g2RelWithNominee = dr["g2RelWithNominee"].ToString(),
                        g2DoBMinor = (Convert.ToDateTime(dr["g2DoBMinor"].ToString())),
                        g2MaturityDoM = (Convert.ToDateTime(dr["g2MaturityDoM"].ToString())),
                        g2DoB = (Convert.ToDateTime(dr["g2DoB"].ToString())),
                        g2IsResident = dr["g2IsResident"].ToString(),
                        g2NID = dr["g2NID"].ToString(),
                        g2Address = dr["g2Address"].ToString(),
                        g2City = dr["g2City"].ToString(),
                        g2PostCode = dr["g2PostCode"].ToString(),
                        g2Division = dr["g2Division"].ToString(),
                        g2Country = dr["g2Country"].ToString(),
                        g2Mobile = dr["g2Mobile"].ToString(),
                        g2Tel = dr["g2Tel"].ToString(),
                        g2Fax = dr["g2Fax"].ToString(),
                        g2Email = dr["g2Email"].ToString()
                    });
                }
                catch (Exception ex)
                {
                    /////
                }
            }
            return acc.ToList();
        }

        public IEnumerable<BOOnlineBODetailModel> AccDocInfo(string trackingno)
        {
            var acc = new List<BOOnlineBODetailModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();
            // change to  Convert.ToInt32(HttpContext.Session.GetString("trackingno")) 
            string query = "SELECT [TRACKINGNO],[fImage],[fNIDFont],[fNIDBack],[fSig],[jImage],[jNIDFont],[jNIDBack],[jSig],[aImage],[aNIDFont],[aNIDBack],[aSig],[n1Image],[n1NIDFont],[n1NIDBack],[n1Sig],[n2Image],[n2NIDFont],[n2NIDBack],[n2Sig],[g1Image],[g1NIDFont],[g1NIDBack],[g1Sig],[g2Image],[g2NIDFont],[g2NIDBack],[g2Sig],[pDate],[iIsComplete] FROM [dbo].[T_ONLINE_BO_AccHolderImages] WHERE TRACKINGNO=" + Convert.ToInt32(trackingno) + "";
            SqlCommand oCommand = new SqlCommand(query, oConnection);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {

                acc.Add(new BOOnlineBODetailModel { n1Title = dr["n1Title"].ToString(), n1FirstName = dr["n1FirstName"].ToString(), n1LastName = dr["n1LastName"].ToString(), n1RelWithACHolder = dr["n1RelWithACHolder"].ToString(), n1Percentage = Convert.ToDecimal(dr["n1Percentage"].ToString()) });
            }
            return acc.ToList();
        }


        public IEnumerable<BOBankInfoModel> bankNameInfo()
        {
            var bank = new List<BOBankInfoModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();


            string query = "SELECT DISTINCT [BANKNAME] FROM T_BANKINFO";
            SqlCommand oCommand = new SqlCommand(query, oConnection);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {
                bank.Add(new BOBankInfoModel { BANKNAME = dr["BANKNAME"].ToString() });
            }
            return bank.ToList();
        }

        public IEnumerable<BOBranchInfo> branchInfo()
        {
            var branch = new List<BOBranchInfo>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();

            string query = "SELECT BRANCHNAME,[ADDRESS] FROM T_BRANCHES WHERE BRANCHNAME NOT IN ('Dhaka Ext special','CTG Ext special','WEB','SMS')";
            SqlCommand oCommand = new SqlCommand(query, oConnection);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {
                branch.Add(new BOBranchInfo { BRANCHNAME = dr["BRANCHNAME"].ToString(), BRANCHADDRESS = dr["ADDRESS"].ToString() });
            }
            return branch.ToList();
        }

        public IEnumerable<BOBankInfoModel> bankDistrictInfo(string bankname)
        {
            var bank = new List<BOBankInfoModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();

            string query = "SELECT DISTINCT [DISTRICT] FROM T_BANKINFO WHERE BANKNAME='" + bankname.Trim() + "'";
            SqlCommand oCommand = new SqlCommand(query, oConnection);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {
                bank.Add(new BOBankInfoModel { BANKDISTRICT = dr["DISTRICT"].ToString() });
            }
            return bank.ToList();
        }

        public IEnumerable<BOBankInfoModel> bankbranchInfo(string bankName, string districtName)
        {
            var bank = new List<BOBankInfoModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();


            string query = "SELECT [BRANCH] FROM T_BANKINFO WHERE BANKNAME='" + bankName.Trim() + "' AND DISTRICT='" + districtName.Trim() + "'";
            SqlCommand oCommand = new SqlCommand(query, oConnection);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {
                //bank.Add(new BankInfo { BANKNAME = dr["BANKNAME"].ToString(), BANKBRANCH = dr["BRANCH"].ToString(), BANKDISTRICT = dr["DISTRICT"].ToString(), BANKROUTING = dr["ROUTING"].ToString() });
                bank.Add(new BOBankInfoModel { BANKBRANCH = dr["BRANCH"].ToString() });
            }
            return bank.ToList();
        }

        public IEnumerable<BOBankInfoModel> bankroutingInfo(string bankName, string districtName, string bankBranch)
        {
            var bank = new List<BOBankInfoModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();

            string query = "SELECT [ROUTING] FROM T_BANKINFO WHERE BANKNAME='" + bankName.Trim() + "' AND DISTRICT='" + districtName.Trim() + "' AND BRANCH='" + bankBranch.Trim() + "'";
            SqlCommand oCommand = new SqlCommand(query, oConnection);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {
                bank.Add(new BOBankInfoModel { BANKROUTING = dr["ROUTING"].ToString() });
            }
            return bank.ToList();
        }


        public IEnumerable<BOBankInfoModel> defaultVAlue()
        {
            var bank = new List<BOBankInfoModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();

            string query = "SELECT TOP 1 [ROUTING],[BRANCH],[DISTRICT],[BANKNAME] FROM T_BANKINFO";
            SqlCommand oCommand = new SqlCommand(query, oConnection);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {
                bank.Add(new BOBankInfoModel { BANKNAME = dr["BANKNAME"].ToString(), BANKDISTRICT = dr["DISTRICT"].ToString(), BANKBRANCH = dr["BRANCH"].ToString(), BANKROUTING = dr["ROUTING"].ToString() });
            }
            return bank.ToList();
        }

        public IEnumerable<BOBankInfoModel> otherBankInfo()
        {
            var bank = new List<BOBankInfoModel>();

            SqlConnection oConnection = new SqlConnection(SqlConn);
            oConnection.Open();

            string query = "SELECT [ROUTING],[BRANCH],[DISTRICT] FROM T_BANKINFO";
            SqlCommand oCommand = new SqlCommand(query, oConnection);

            DataTable oTable = new DataTable();
            oTable.Load(oCommand.ExecuteReader(CommandBehavior.CloseConnection));

            foreach (DataRow dr in oTable.Rows)
            {
                bank.Add(new BOBankInfoModel { BANKDISTRICT = dr["DISTRICT"].ToString(), BANKBRANCH = dr["BRANCH"].ToString(), BANKROUTING = dr["ROUTING"].ToString() });
            }
            return bank.ToList();
        }

        public byte[] ConvertImageToByteArray(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            using (var ms = new MemoryStream())
            {
                imageFile.CopyTo(ms);
                return ms.ToArray();
            }
        }


        public IEnumerable<BOBankInfoModel> GetTitle()
        {
            var branch = new List<BOBankInfoModel>();

            branch.Add(new BOBankInfoModel { TitleValue = "Mr.", TitleText = "Mr." });
            branch.Add(new BOBankInfoModel { TitleValue = "Ms.", TitleText = "Ms." });
            branch.Add(new BOBankInfoModel { TitleValue = "Mrs.", TitleText = "Mrs." });
            branch.Add(new BOBankInfoModel { TitleValue = "Dr.", TitleText = "Dr." });

            return branch.ToList();
        }

        // **************** ADD NEW Online BO Detail *********************
        public bool AddOnlineBoDetail(BOOnlineBODetailModel bomodel)
        {
            //SqlConnection oConnection = new SqlConnection(SqlConn);

            //string query = "INSERT INTO T_ONLINE_BO_DETAIL(FirstName,MidName,LastName,d_flag) VALUES('" + bomodel.FirstName + "','" + bomodel.MidName + "','" + bomodel.LastName + "',"+bomodel.d_flag+")";

            //SqlCommand oCommand = new SqlCommand(query,oConnection);

            //oConnection.Open();
            //int i = oCommand.ExecuteNonQuery();
            //oConnection.Close();

            //if (i >= 1)
            //    return true;
            //else
                return false;
        }

        //     public bool AddAccHolder(BOAccountHolderModel bomodel)
        //     {
        //         int i = 0;
        //         SqlConnection oConnection = new SqlConnection(SqlConn);

        //         string query = "UPDATE T_ONLINE_BO_AccountHolder SET [ToC]=@ToC,[fFirstName]=@fFirstName,[fLastName]=@fLastName,[fOccupation]=@fOccupation,[fDoB]=@fDoB,[fTitle]=@fTitle,[fFName]=@fFName,[fMName]=@fMName,[fAddress]=@fAddress,[fCity]=@fCity,[fPostCode]=@fPostCode,[fDivision]=@fDivision,[fCountry]=@fCountry,[fMobile]=@fMobile,[fTel]=@fTel,[fFax]=@fFax,[fEmail]=@fEmail,[fNationality]=@fNationality,[fNID]=@fNID,[fTIN]=@fTIN,[fSex]=@fSex,[fResidency]=@fResidency,[jTitle]=@jTitle,[jFirstName]=@jFirstName,[jLastName]=@jLastName,[jOccupation]=@jOccupation,[jDoB]=@jDoB,[jFName]=@jFName,[jMName]=@jMName,[jAddress]=@jAddress,[jCity]=@jCity,[jPostCode]=@jPostCode,[jDivision]=@jDivision,[jCountry]=@jCountry,[jMobile]=@jMobile,[jTel]=@jTel,[jFax]=@jFax,[jEmail]=@jEmail,[jNID]=@jNID,[DesireBranch]=@DesireBranch,IsDirector=@IsDirector,[DirectorShare]=@DirectorShare,fIsComplete=@fIsComplete,[fSpouseName]=@fSpouseName WHERE [TRACKINGNO]=@id";
        //         SqlCommand oCommand = new SqlCommand(query, oConnection);
        //         oCommand.Parameters.AddWithValue("@id", bomodel.TRACKINGNO.ToString());


        //         try
        //         {

        //             //if (bomodel.TRACKINGNO == null) bomodel.TRACKINGNO = 0;
        //             if (bomodel.ToC == null) bomodel.ToC = "";
        //             if (bomodel.fFirstName == null) bomodel.fFirstName = "";
        //             if (bomodel.fLastName == null) bomodel.fLastName = "";
        //             if (bomodel.fOccupation == null) bomodel.fOccupation = "";
        //             if (bomodel.fTitle1 == null) bomodel.fTitle1 = "";
        //             //if (bomodel.n1DoB == null) bomodel.n1DoB = "";
        //             if (bomodel.fFName == null) bomodel.fFName = "";
        //             if (bomodel.fMName == null) bomodel.fMName = "";
        //             if (bomodel.fAddress == null) bomodel.fAddress = "";
        //             if (bomodel.fCity == null) bomodel.fCity = "";
        //             if (bomodel.fPostCode == null) bomodel.fPostCode = "";
        //             if (bomodel.fDivision == null) bomodel.fDivision = "";
        //             if (bomodel.fCountry == null) bomodel.fCountry = "";
        //             if (bomodel.fMobile == null) bomodel.fMobile = "";
        //             if (bomodel.fTel == null) bomodel.fTel = "";
        //             if (bomodel.fFax == null) bomodel.fFax = "";
        //             if (bomodel.fEmail == null) bomodel.fEmail = "";
        //             if (bomodel.fNationality == null) bomodel.fNationality = "";
        //             if (bomodel.fNID == null) bomodel.fNID = "";
        //             if (bomodel.fTIN == null) bomodel.fTIN = "";
        //             if (bomodel.fSex == null) bomodel.fSex = "";
        //             if (bomodel.fResidency == null) bomodel.fResidency = "";
        //             if (bomodel.jFastName == null) bomodel.jFastName = "";
        //             if (bomodel.jLastName == null) bomodel.jLastName = "";
        //             if (bomodel.jOccupation == null) bomodel.jOccupation = "";
        //             if (bomodel.jTitle1 == null) bomodel.jTitle1 = "";
        //             //if (bomodel.n1DoB == null) bomodel.n1DoB = "";
        //             if (bomodel.jFName == null) bomodel.jFName = "";
        //             if (bomodel.jMName == null) bomodel.jMName = "";
        //             if (bomodel.jAddress == null) bomodel.jAddress = "";
        //             if (bomodel.jCity == null) bomodel.jCity = "";
        //             if (bomodel.jPostCode == null) bomodel.jPostCode = "";
        //             if (bomodel.jDivision == null) bomodel.jDivision = "";
        //             if (bomodel.jCountry == null) bomodel.jCountry = "";
        //             if (bomodel.jMobile == null) bomodel.jMobile = "";
        //             if (bomodel.jTel == null) bomodel.jTel = "";
        //             if (bomodel.jFax == null) bomodel.jFax = "";
        //             if (bomodel.jEmail == null) bomodel.jEmail = "";
        //             if (bomodel.jNID == null) bomodel.jNID = "";
        //             if (bomodel.IsDirector == null) bomodel.IsDirector = "";
        //             if (bomodel.DirectorShare == null) bomodel.DirectorShare = "";
        //             if (bomodel.fSpouseName == null) bomodel.fSpouseName = "";


        //             oCommand.Parameters.AddWithValue("@ToC", bomodel.ToC);
        //             oCommand.Parameters.AddWithValue("@fFirstName", bomodel.fFirstName);
        //             oCommand.Parameters.AddWithValue("@fLastName", bomodel.fLastName);
        //             oCommand.Parameters.AddWithValue("@fOccupation", bomodel.fOccupation);
        //             if (bomodel.fDoB == null) bomodel.fDoB = Convert.ToDateTime("1/1/1900").ToString();
        //             if (bomodel.DesireBranch == null) bomodel.DesireBranch = "";
        //             oCommand.Parameters.AddWithValue("@fDoB", DateTime.ParseExact(bomodel.fDoB, "dd/MM/yyyy", null) );
        //             oCommand.Parameters.AddWithValue("@fTitle", bomodel.fTitle1);
        //             oCommand.Parameters.AddWithValue("@fFName", bomodel.fFName);
        //             oCommand.Parameters.AddWithValue("@fMName", bomodel.fMName);
        //             oCommand.Parameters.AddWithValue("@fAddress", bomodel.fAddress);
        //             oCommand.Parameters.AddWithValue("@fCity", bomodel.fCity);
        //             oCommand.Parameters.AddWithValue("@fPostCode", bomodel.fPostCode);
        //             oCommand.Parameters.AddWithValue("@fDivision", bomodel.fDivision);
        //             oCommand.Parameters.AddWithValue("@fCountry", bomodel.fCountry);
        //             oCommand.Parameters.AddWithValue("@fMobile", bomodel.fMobile);
        //             oCommand.Parameters.AddWithValue("@fTel", bomodel.fTel);
        //             oCommand.Parameters.AddWithValue("@fFax", bomodel.fFax);
        //             oCommand.Parameters.AddWithValue("@fEmail", bomodel.fEmail);
        //             oCommand.Parameters.AddWithValue("@fNationality", bomodel.fNationality);
        //             oCommand.Parameters.AddWithValue("@fNID", bomodel.fNID);
        //             oCommand.Parameters.AddWithValue("@fTIN", bomodel.fTIN);
        //             oCommand.Parameters.AddWithValue("@fSex", bomodel.fSex);
        //             oCommand.Parameters.AddWithValue("@fResidency", bomodel.fResidency);
        //             oCommand.Parameters.AddWithValue("@jTitle", bomodel.jTitle1);
        //             oCommand.Parameters.AddWithValue("@jFirstName", bomodel.jFastName);
        //             oCommand.Parameters.AddWithValue("@jLastName", bomodel.jLastName);
        //             oCommand.Parameters.AddWithValue("@jOccupation", bomodel.jOccupation);
        //             if (bomodel.jDoB == null) bomodel.jDoB = Convert.ToDateTime("1/1/1900");
        //             oCommand.Parameters.AddWithValue("@jDoB", bomodel.jDoB);
        //             oCommand.Parameters.AddWithValue("@jFName", bomodel.jFName);
        //             oCommand.Parameters.AddWithValue("@jMName", bomodel.jMName);
        //             oCommand.Parameters.AddWithValue("@jAddress", bomodel.jAddress);
        //             oCommand.Parameters.AddWithValue("@jCity", bomodel.jCity);
        //             oCommand.Parameters.AddWithValue("@jPostCode", bomodel.jPostCode);
        //             oCommand.Parameters.AddWithValue("@jDivision", bomodel.jDivision);
        //             oCommand.Parameters.AddWithValue("@jCountry", bomodel.jCountry);
        //             oCommand.Parameters.AddWithValue("@jMobile", bomodel.jMobile);
        //             oCommand.Parameters.AddWithValue("@jTel", bomodel.jTel);
        //             oCommand.Parameters.AddWithValue("@jFax", bomodel.jFax);
        //             oCommand.Parameters.AddWithValue("@jEmail", bomodel.jEmail);
        //             oCommand.Parameters.AddWithValue("@jNID", bomodel.jNID);
        //             oCommand.Parameters.AddWithValue("@DesireBranch", bomodel.DesireBranch);
        //             oCommand.Parameters.AddWithValue("@IsDirector", bomodel.IsDirector);
        //             oCommand.Parameters.AddWithValue("@DirectorShare", bomodel.DirectorShare);
        //             oCommand.Parameters.AddWithValue("@fIsComplete", 1);
        //             oCommand.Parameters.AddWithValue("@fSpouseName", bomodel.fSpouseName);

        //             oConnection.Open();
        //             i = oCommand.ExecuteNonQuery();
        //             oConnection.Close();

        //}
        //         catch (Exception ex)
        //         {
        //             return false;
        //         }
        //         if (i >= 1)
        //         {
        //	try
        //	{
        //		if (bomodel.RefarralCode == null || bomodel.RefarralCode == "" || bomodel.RefarralCode == "0")
        //		{
        //			bomodel.RefarralCode = "0";
        //		}

        //		SqlConnection oConnection2 = new SqlConnection(SqlConn);

        //		string query2 = "UPDATE T_ONLINE_BO_REG SET RefarralCode='" + bomodel.RefarralCode + "' Where TRACKINGNO='" + bomodel.TRACKINGNO + "'";

        //		SqlCommand oCommand2 = new SqlCommand(query2, oConnection2);
        //		oConnection2.Open();

        //		oCommand2.ExecuteNonQuery();

        //		oConnection2.Close();

        //	}
        //	catch (Exception ex)
        //	{
        //		return false;
        //	}

        //	return true;
        //}

        //         else
        //             return false;

        //     }

        public bool AddAccHolder(BOAccountHolderModel bomodel)
        {
            int i = 0;
            SqlConnection oConnection = new SqlConnection(SqlConn);

            string query = "UPDATE T_ONLINE_BO_AccountHolder SET [fFirstName]=@fFirstName, [fLastName]=@fLastName, [fDoB]=@fDoB, [fSex]=@fSex, [fFName]=@fFName,[fMName]=@fMName,[fSpouseName]=@fSpouseName,[fOccupation]=@fOccupation,[fNID]=@fNID WHERE [TRACKINGNO]=@id";
            SqlCommand oCommand = new SqlCommand(query, oConnection);
            oCommand.Parameters.AddWithValue("@id", bomodel.TRACKINGNO.ToString());


            try
            {

       
                if (bomodel.fFirstName == null) bomodel.fFirstName = "";
                if (bomodel.fLastName == null) bomodel.fLastName = "";
                if (bomodel.fDoB == null) bomodel.fDoB = "";
                if (bomodel.fOccupation == null) bomodel.fOccupation = "";
                if (bomodel.fFName == null) bomodel.fFName = "";
                if (bomodel.fMName == null) bomodel.fMName = "";
                if (bomodel.fNID == null) bomodel.fNID = "";
                if (bomodel.fSex == null) bomodel.fSex = "";
                if (bomodel.fSpouseName == null) bomodel.fSpouseName = "";


                oCommand.Parameters.AddWithValue("@fFirstName", bomodel.fFirstName);
                oCommand.Parameters.AddWithValue("@fLastName", bomodel.fLastName);
                oCommand.Parameters.AddWithValue("@fOccupation", bomodel.fOccupation);
                if (bomodel.fDoB == null) bomodel.fDoB = Convert.ToDateTime("1/1/1900").ToString();
                if (bomodel.DesireBranch == null) bomodel.DesireBranch = "";
                oCommand.Parameters.AddWithValue("@fDoB", DateTime.ParseExact(bomodel.fDoB, "dd/MM/yyyy", null));
                oCommand.Parameters.AddWithValue("@fFName", bomodel.fFName);
                oCommand.Parameters.AddWithValue("@fMName", bomodel.fMName);
                oCommand.Parameters.AddWithValue("@fNID", bomodel.fNID);
                oCommand.Parameters.AddWithValue("@fSex", bomodel.fSex);               
                oCommand.Parameters.AddWithValue("@fSpouseName", bomodel.fSpouseName);

                oConnection.Open();
                i = oCommand.ExecuteNonQuery();
                oConnection.Close();

            }
            catch (Exception ex)
            {
                return false;
            }
            if (i >= 1)
            {         
                return true;
            }

            else
                return false;

        }

        public bool AddSecondAccHolder(BoSecondAccHolderModel bomodel)
        {
            int i = 0;
            SqlConnection oConnection = new SqlConnection(SqlConn);

            string query = "UPDATE T_ONLINE_BO_AccountHolder SET [jFirstName]=@jFirstName,[jLastName]=@jLastName,[jDoB]=@jDoB,[jNID]=@jNID,[jSex]=@jSex  WHERE [TRACKINGNO]=@id";
            SqlCommand oCommand = new SqlCommand(query, oConnection);
            oCommand.Parameters.AddWithValue("@id", bomodel.TRACKINGNO.ToString());


            try
            {    
                if (bomodel.jFastName == null) bomodel.jFastName = "";
                if (bomodel.jLastName == null) bomodel.jLastName = "";             
                if (bomodel.jNID == null) bomodel.jNID = "";
                if (bomodel.jSex == null) bomodel.jSex = "";

                oCommand.Parameters.AddWithValue("@jFirstName", bomodel.jFastName);
                oCommand.Parameters.AddWithValue("@jLastName", bomodel.jLastName);
                if (bomodel.jDoB == null) bomodel.jDoB = Convert.ToDateTime("1/1/1900").ToString();
                oCommand.Parameters.AddWithValue("@jDoB", DateTime.ParseExact(bomodel.jDoB, "dd/MM/yyyy", null));
                oCommand.Parameters.AddWithValue("@jNID", bomodel.jNID);
                oCommand.Parameters.AddWithValue("@jSex", bomodel.jSex);

                oConnection.Open();
                i = oCommand.ExecuteNonQuery();
                oConnection.Close();

            }
            catch (Exception ex)
            {
                return false;
            }
            if (i >= 1)
            {
                

                return true;
            }

            else
                return false;

        }

        public bool AddAccHolderAddress(BoSecondAccHolderAddressModel bomodel)
        {
            int i = 0;
            SqlConnection oConnection = new SqlConnection(SqlConn);

            string query = "UPDATE T_ONLINE_BO_AccountHolder SET [fAddress]=@fAddress,[fPostCode]=@fPostCode,[fCity]=@fCity,[fDivision]=@fDivision,[fCountry]=@fCountry WHERE [TRACKINGNO]=@id";
            SqlCommand oCommand = new SqlCommand(query, oConnection);
            oCommand.Parameters.AddWithValue("@id", bomodel.TRACKINGNO.ToString());


            try
            {
                if (bomodel.fAddress == null) bomodel.fAddress = "";
                if (bomodel.fPostCode == null) bomodel.fPostCode = "";
                if (bomodel.fCity == null) bomodel.fCity = "";
                if (bomodel.fDivision == null) bomodel.fDivision = "";
                if (bomodel.fCountry == null) bomodel.fCountry = "";

                oCommand.Parameters.AddWithValue("@fAddress", bomodel.fAddress);
                oCommand.Parameters.AddWithValue("@fPostCode", bomodel.fPostCode);
                oCommand.Parameters.AddWithValue("@fCity", bomodel.fCity);
                oCommand.Parameters.AddWithValue("@fDivision", bomodel.fDivision);
                oCommand.Parameters.AddWithValue("@fCountry", bomodel.fCountry);

                oConnection.Open();
                i = oCommand.ExecuteNonQuery();
                oConnection.Close();

            }
            catch (Exception ex)
            {
                return false;
            }
            if (i >= 1)
            {


                return true;
            }

            else
                return false;

        }
        public bool AddBankDetail(BOAccountHolderBankModel bomodel)
        {
            int i = 0;
            SqlConnection oConnection = new SqlConnection(SqlConn);

            string query = "UPDATE T_ONLINE_BO_AccHolderBANK SET [BANKNAME]=@BANKNAME,[BANKBRANCH]=@BANKBRANCH,[DISTRICT]=@DISTRICT,[ROUTING]=@ROUTING,[AC]=@AC WHERE TRACKINGNO=@TRACKINGNO";

            SqlCommand oCommand = new SqlCommand(query, oConnection);

            try
            {
                //oCommand.Parameters.AddWithValue("@TRACKINGNO", 1);
                //oCommand.Parameters.AddWithValue("@BANKNAME", bomodel.BANKNAME);
                //oCommand.Parameters.AddWithValue("@BANKBRANCH", bomodel.BANKBRANCH);
                //oCommand.Parameters.AddWithValue("@DISTRICT", bomodel.BANKDISTRICT);
                //oCommand.Parameters.AddWithValue("@ROUTING", bomodel.BANKROUTING);
                //oCommand.Parameters.AddWithValue("@AC", bomodel.BANKAC);

                oCommand.Parameters.AddWithValue("@TRACKINGNO", bomodel.TrackingNo.ToString());
                oCommand.Parameters.AddWithValue("@BANKNAME", bomodel.BANKNAME);
                oCommand.Parameters.AddWithValue("@BANKBRANCH", bomodel.BANKBRANCH);
                oCommand.Parameters.AddWithValue("@DISTRICT", bomodel.BANKDISTRICT);
                oCommand.Parameters.AddWithValue("@ROUTING", bomodel.BANKROUTING);
                oCommand.Parameters.AddWithValue("@AC", bomodel.BANKAC);


                oConnection.Open();
                i = oCommand.ExecuteNonQuery();
                oConnection.Close();

            }
            catch (Exception ex)
            {
                return false;
            }
            if (i >= 1)
                return true;
            else
                return false;

        }
        public bool AddAuthorize(BOAccountHolderAuthorizeModel bomodel)
        {
            int i = 0;
            SqlConnection oConnection = new SqlConnection(SqlConn);

            string query = "UPDATE T_ONLINE_BO_AccHolderAuthorize SET [aTitle]=@aTitle,[aFirstName]=@aFirstName,[aLastName]=@aLastName,[aOccupation]=@aOccupation,[aDoB]=@aDoB,[aFName]=@aFName,[aMName]=@aMName,[aAddress]=@aAddress,[aCity]=@aCity,[aPostCode]=@aPostCode,[aDivision]=@aDivision,[aCountry]=@aCountry,[aMobile]=@aMobile,[aTel]=@aTel,[aFax]=@aFax,[aEmail]=@aEmail,[aNID]=@aNID,[aIsComplete]=@aIsComplete WHERE TRACKINGNO=@TRACKINGNO";

            SqlCommand oCommand = new SqlCommand(query, oConnection);

            try
            {

                //if (bomodel.TRACKINGNO == null) bomodel.TRACKINGNO = 0;
                if (bomodel.aTitle1 == null) bomodel.aTitle1 = "";
                if (bomodel.aFirstName == null) bomodel.aFirstName = "";
                if (bomodel.aLastName == null) bomodel.aLastName = "";
                if (bomodel.aOccupation == null) bomodel.aOccupation = "";
                if (bomodel.aFName == null) bomodel.aFName = "";
                if (bomodel.aMName == null) bomodel.aMName = "";
                if (bomodel.aAddress == null) bomodel.aAddress = "";
                if (bomodel.aCity == null) bomodel.aCity = "";
                if (bomodel.aPostCode == null) bomodel.aPostCode = "";
                if (bomodel.aDivision == null) bomodel.aDivision = "";
                if (bomodel.aCountry == null) bomodel.aCountry = "";
                if (bomodel.aMobile == null) bomodel.aMobile = "";
                if (bomodel.aTel == null) bomodel.aTel = "";
                if (bomodel.aFax == null) bomodel.aFax = "";
                if (bomodel.aEmail == null) bomodel.aEmail = "";
                if (bomodel.aNID == null) bomodel.aNID = "";


                oCommand.Parameters.AddWithValue("@TRACKINGNO", bomodel.TRACKINGNO.ToString());
                oCommand.Parameters.AddWithValue("@aTitle", bomodel.aTitle1);
                oCommand.Parameters.AddWithValue("@aFirstName", bomodel.aFirstName);
                oCommand.Parameters.AddWithValue("@aLastName", bomodel.aLastName);
                oCommand.Parameters.AddWithValue("@aOccupation", bomodel.aOccupation);

                if (bomodel.aDoB == null) bomodel.aDoB = Convert.ToDateTime("1/1/1900");
                oCommand.Parameters.AddWithValue("@aDoB", Convert.ToDateTime(bomodel.aDoB));
                oCommand.Parameters.AddWithValue("@aFName", bomodel.aFName);
                oCommand.Parameters.AddWithValue("@aMName", bomodel.aMName);
                oCommand.Parameters.AddWithValue("@aAddress", bomodel.aAddress);
                oCommand.Parameters.AddWithValue("@aCity", bomodel.aCity);
                oCommand.Parameters.AddWithValue("@aPostCode", bomodel.aPostCode);
                oCommand.Parameters.AddWithValue("@aDivision", bomodel.aDivision);
                oCommand.Parameters.AddWithValue("@aCountry", bomodel.aCountry);
                oCommand.Parameters.AddWithValue("@aMobile", bomodel.aMobile);
                oCommand.Parameters.AddWithValue("@aTel", bomodel.aTel);
                oCommand.Parameters.AddWithValue("@aFax", bomodel.aFax);
                oCommand.Parameters.AddWithValue("@aEmail", bomodel.aEmail);
                oCommand.Parameters.AddWithValue("@aNID", bomodel.aNID);
                oCommand.Parameters.AddWithValue("@aIsComplete", 1);




                oConnection.Open();
                i = oCommand.ExecuteNonQuery();
                oConnection.Close();

            }
            catch (Exception ex)
            {
                return false;
            }
            if (i >= 1)
                return true;
            else
                return false;

        }
        public bool AddNominee(BOAccountHolderNomineeModel bomodel)
        {
            int i = 0;
            SqlConnection oConnection = new SqlConnection(SqlConn);

            string query = "UPDATE T_ONLINE_BO_AccHolderNominee SET [n1Title]=@n1Title,[n1FirstName]=@n1FirstName,[n1LastName]=@n1LastName,[n1RelWithACHolder]=@n1RelWithACHolder,[n1Percentage]=@n1Percentage,[n1IsResident]=@n1IsResident,[n1DoB]=@n1DoB,[n1NID]=@n1NID,[n1Address]=@n1Address,[n1City]=@n1City,[n1PostCode]=@n1PostCode,[n1Division]=@n1Division,[n1Country]=@n1Country,[n1Mobile]=@n1Mobile,[n1Tel]=@n1Tel,[n1Fax]=@n1Fax,[n1Email]=@n1Email,[n2Title]=@n2Title,[n2FirstName]=@n2FirstName,[n2LastName]=@n2LastName,[n2RelWithACHolder]=@n2RelWithACHolder,[n2Percentage]=@n2Percentage,[n2IsResident]=@n2IsResident,[n2DoB]=@n2DoB,[n2NID]=@n2NID,[n2Address]=@n2Address,[n2City]=@n2City,[n2PostCode]=@n2PostCode,[n2Division]=@n2Division,[n2Country]=@n2Country,[n2Mobile]=@n2Mobile,[n2Tel]=@n2Tel,[n2Fax]=@n2Fax,[n2Email]=@n2Email,[g1Title]=@g1Title,[g1FirstName]=@g1FirstName,[g1LastName]=@g1LastName,[g1RelWithNominee]=@g1RelWithNominee,[g1DoBMinor]=@g1DoBMinor,[g1MaturityDoM]=@g1MaturityDoM,[g1IsResident]=@g1IsResident,[g1DoB]=@g1DoB,[g1NID]=@g1NID,[g1Address]=@g1Address,[g1City]=@g1City,[g1PostCode]=@g1PostCode,[g1Division]=@g1Division,[g1Country]=@g1Country,[g1Mobile]=@g1Mobile,[g1Tel]=@g1Tel,[g1Fax]=@g1Fax,[g1Email]=@g1Email,[g2Title]=@g2Title,[g2FirstName]=@g2FirstName,[g2LastName]=@g2LastName,[g2RelWithNominee]=@g2RelWithNominee,[g2DoBMinor]=@g2DoBMinor,[g2MaturityDoM]=@g2MaturityDoM,[g2IsResident]=@g2IsResident,[g2DoB]=@g2DoB,[g2NID]=@g2NID,[g2Address]=@g2Address,[g2City]=@g2City,[g2PostCode]=@g2PostCode,[g2Division]=@g2Division,[g2Country]=@g2Country,[g2Mobile]=@g2Mobile,[g2Tel]=@g2Tel,[g2Fax]=@g2Fax,[g2Email]=@g2Email,[nIsComplete]=@nIsComplete WHERE TRACKINGNO=@TRACKINGNO";

            SqlCommand oCommand = new SqlCommand(query, oConnection);



            try
            {
                if (bomodel.n1Title1 == null) bomodel.n1Title1 = "";
                if (bomodel.n1FirstName == null) bomodel.n1FirstName = "";
                if (bomodel.n1LastName == null) bomodel.n1LastName = "";
                if (bomodel.n1RelWithACHolder == null) bomodel.n1RelWithACHolder = "";
                if (bomodel.n1Percentage == null) bomodel.n1Percentage = 0;
                if (bomodel.n1IsResident == null) bomodel.n1IsResident = "";
                //if (bomodel.n1DoB == null) bomodel.n1DoB = "";
                if (bomodel.n1NID == null) bomodel.n1NID = "";
                if (bomodel.n1Address == null) bomodel.n1Address = "";
                if (bomodel.n1City == null) bomodel.n1City = "";
                if (bomodel.n1PostCode == null) bomodel.n1PostCode = "";
                if (bomodel.n1Division == null) bomodel.n1Division = "";
                if (bomodel.n1Country == null) bomodel.n1Country = "";
                if (bomodel.n1Mobile == null) bomodel.n1Mobile = "";
                if (bomodel.n1Tel == null) bomodel.n1Tel = "";
                if (bomodel.n1Fax == null) bomodel.n1Fax = "";
                if (bomodel.n1Email == null) bomodel.n1Email = "";
                if (bomodel.n2Title1 == null) bomodel.n2Title1 = "";
                if (bomodel.n2FirstName == null) bomodel.n2FirstName = "";
                if (bomodel.n2LastName == null) bomodel.n2LastName = "";
                if (bomodel.n2RelWithACHolder == null) bomodel.n2RelWithACHolder = "";
                if (bomodel.n2Percentage == null) bomodel.n2Percentage = 0;
                if (bomodel.n2IsResident == null) bomodel.n2IsResident = "";
                //if (bomodel.n2DoB == null) bomodel.n2DoB = "";
                if (bomodel.n2NID == null) bomodel.n2NID = "";
                if (bomodel.n2Address == null) bomodel.n2Address = "";
                if (bomodel.n2City == null) bomodel.n2City = "";
                if (bomodel.n2PostCode == null) bomodel.n2PostCode = "";
                if (bomodel.n2Division == null) bomodel.n2Division = "";
                if (bomodel.n2Country == null) bomodel.n2Country = "";
                if (bomodel.n2Mobile == null) bomodel.n2Mobile = "";
                if (bomodel.n2Tel == null) bomodel.n2Tel = "";
                if (bomodel.n2Fax == null) bomodel.n2Fax = "";
                if (bomodel.n2Email == null) bomodel.n2Email = "";
                if (bomodel.g1Title1 == null) bomodel.g1Title1 = "";
                if (bomodel.g1FirstName == null) bomodel.g1FirstName = "";
                if (bomodel.g1LastName == null) bomodel.g1LastName = "";
                if (bomodel.g1RelWithNominee == null) bomodel.g1RelWithNominee = "";
                //if (bomodel.g1DoBMinor == null) bomodel.g1DoBMinor = "";
                //if (bomodel.g1MaturityDoM == null) bomodel.g1MaturityDoM = "";
                if (bomodel.g1IsResident == null) bomodel.g1IsResident = "";
                //if (bomodel.g1DoB == null) bomodel.g1DoB = "";
                if (bomodel.g1NID == null) bomodel.g1NID = "";
                if (bomodel.g1Address == null) bomodel.g1Address = "";
                if (bomodel.g1City == null) bomodel.g1City = "";
                if (bomodel.g1PostCode == null) bomodel.g1PostCode = "";
                if (bomodel.g1Division == null) bomodel.g1Division = "";
                if (bomodel.g1Country == null) bomodel.g1Country = "";
                if (bomodel.g1Mobile == null) bomodel.g1Mobile = "";
                if (bomodel.g1Tel == null) bomodel.g1Tel = "";
                if (bomodel.g1Fax == null) bomodel.g1Fax = "";
                if (bomodel.g1Email == null) bomodel.g1Email = "";
                if (bomodel.g2Title1 == null) bomodel.g2Title1 = "";
                if (bomodel.g2FirstName == null) bomodel.g2FirstName = "";
                if (bomodel.g2LastName == null) bomodel.g2LastName = "";
                if (bomodel.g2RelWithNominee == null) bomodel.g2RelWithNominee = "";
                //if (bomodel.g2DoBMinor == null) bomodel.g2DoBMinor = "";
                //if (bomodel.g2MaturityDoM == null) bomodel.g2MaturityDoM = "";
                if (bomodel.g2IsResident == null) bomodel.g2IsResident = "";
                //if (bomodel.g2DoB == null) bomodel.g2DoB = "";
                if (bomodel.g2NID == null) bomodel.g2NID = "";
                if (bomodel.g2Address == null) bomodel.g2Address = "";
                if (bomodel.g2City == null) bomodel.g2City = "";
                if (bomodel.g2PostCode == null) bomodel.g2PostCode = "";
                if (bomodel.g2Division == null) bomodel.g2Division = "";
                if (bomodel.g2Country == null) bomodel.g2Country = "";
                if (bomodel.g2Mobile == null) bomodel.g2Mobile = "";
                if (bomodel.g2Tel == null) bomodel.g2Tel = "";
                if (bomodel.g2Fax == null) bomodel.g2Fax = "";
                if (bomodel.g2Email == null) bomodel.g2Email = "";




                //oCommand.Parameters.AddWithValue("@TRACKINGNO", Convert.ToInt32(HttpContext.Current.Session["trackingno"].ToString()));
                oCommand.Parameters.AddWithValue("@TRACKINGNO", bomodel.TRACKINGNO.ToString());
                oCommand.Parameters.AddWithValue("@n1Title", bomodel.n1Title1);
                oCommand.Parameters.AddWithValue("@n1FirstName", bomodel.n1FirstName);
                oCommand.Parameters.AddWithValue("@n1LastName", bomodel.n1LastName);
                oCommand.Parameters.AddWithValue("@n1RelWithACHolder", bomodel.n1RelWithACHolder);
                oCommand.Parameters.AddWithValue("@n1Percentage", bomodel.n1Percentage);
                oCommand.Parameters.AddWithValue("@n1IsResident", bomodel.n1IsResident);
                if (bomodel.n1DoB == null) bomodel.n1DoB = Convert.ToDateTime("1/1/1900");
                oCommand.Parameters.AddWithValue("@n1DoB", Convert.ToDateTime(bomodel.n1DoB));
                oCommand.Parameters.AddWithValue("@n1NID", bomodel.n1NID);
                oCommand.Parameters.AddWithValue("@n1Address", bomodel.n1Address);
                oCommand.Parameters.AddWithValue("@n1City", bomodel.n1City);
                oCommand.Parameters.AddWithValue("@n1PostCode", bomodel.n1PostCode);
                oCommand.Parameters.AddWithValue("@n1Division", bomodel.n1Division);
                oCommand.Parameters.AddWithValue("@n1Country", bomodel.n1Country);
                oCommand.Parameters.AddWithValue("@n1Mobile", bomodel.n1Mobile);
                oCommand.Parameters.AddWithValue("@n1Tel", bomodel.n1Tel);
                oCommand.Parameters.AddWithValue("@n1Fax", bomodel.n1Fax);
                oCommand.Parameters.AddWithValue("@n1Email", bomodel.n1Email);
                oCommand.Parameters.AddWithValue("@n2Title", bomodel.n2Title1);
                oCommand.Parameters.AddWithValue("@n2FirstName", bomodel.n2FirstName);
                oCommand.Parameters.AddWithValue("@n2LastName", bomodel.n2LastName);
                oCommand.Parameters.AddWithValue("@n2RelWithACHolder", bomodel.n2RelWithACHolder);
                oCommand.Parameters.AddWithValue("@n2Percentage", bomodel.n2Percentage);
                oCommand.Parameters.AddWithValue("@n2IsResident", bomodel.n2IsResident);
                if (bomodel.n2DoB == null) bomodel.n2DoB = Convert.ToDateTime("1/1/1900");
                oCommand.Parameters.AddWithValue("@n2DoB", bomodel.n2DoB);
                oCommand.Parameters.AddWithValue("@n2NID", bomodel.n2NID);
                oCommand.Parameters.AddWithValue("@n2Address", bomodel.n1Address);
                oCommand.Parameters.AddWithValue("@n2City", bomodel.n2City);
                oCommand.Parameters.AddWithValue("@n2PostCode", bomodel.n2PostCode);
                oCommand.Parameters.AddWithValue("@n2Division", bomodel.n2Division);
                oCommand.Parameters.AddWithValue("@n2Country", bomodel.n2Country);
                oCommand.Parameters.AddWithValue("@n2Mobile", bomodel.n2Mobile);
                oCommand.Parameters.AddWithValue("@n2Tel", bomodel.n2Tel);
                oCommand.Parameters.AddWithValue("@n2Fax", bomodel.n2Fax);
                oCommand.Parameters.AddWithValue("@n2Email", bomodel.n2Email);
                oCommand.Parameters.AddWithValue("@g1Title", bomodel.g1Title1);
                oCommand.Parameters.AddWithValue("@g1FirstName", bomodel.g1FirstName);
                oCommand.Parameters.AddWithValue("@g1LastName", bomodel.g1LastName);
                oCommand.Parameters.AddWithValue("@g1RelWithNominee", bomodel.g1RelWithNominee);
                if (bomodel.g1DoBMinor == null) bomodel.g1DoBMinor = Convert.ToDateTime("1/1/1900");
                oCommand.Parameters.AddWithValue("@g1DoBMinor", bomodel.g1DoBMinor);
                if (bomodel.g1MaturityDoM == null) bomodel.g1MaturityDoM = Convert.ToDateTime("1/1/1900");
                oCommand.Parameters.AddWithValue("@g1MaturityDoM", bomodel.g1MaturityDoM);
                oCommand.Parameters.AddWithValue("@g1IsResident", bomodel.g1IsResident);
                if (bomodel.g1DoB == null) bomodel.g1DoB = Convert.ToDateTime("1/1/1901");
                oCommand.Parameters.AddWithValue("@g1DoB", bomodel.g1DoB);
                oCommand.Parameters.AddWithValue("@g1NID", bomodel.g1NID);
                oCommand.Parameters.AddWithValue("@g1Address", bomodel.g1Address);
                oCommand.Parameters.AddWithValue("@g1City", bomodel.g1City);
                oCommand.Parameters.AddWithValue("@g1PostCode", bomodel.g1PostCode);
                oCommand.Parameters.AddWithValue("@g1Division", bomodel.g1Division);
                oCommand.Parameters.AddWithValue("@g1Country", bomodel.g1Country);
                oCommand.Parameters.AddWithValue("@g1Mobile", bomodel.g1Mobile);
                oCommand.Parameters.AddWithValue("@g1Tel", bomodel.g1Tel);
                oCommand.Parameters.AddWithValue("@g1Fax", bomodel.g1Fax);
                oCommand.Parameters.AddWithValue("@g1Email", bomodel.g1Email);
                oCommand.Parameters.AddWithValue("@g2Title", bomodel.g2Title1);
                oCommand.Parameters.AddWithValue("@g2FirstName", bomodel.g2FirstName);
                oCommand.Parameters.AddWithValue("@g2LastName", bomodel.g2LastName);
                oCommand.Parameters.AddWithValue("@g2RelWithNominee", bomodel.g2RelWithNominee);
                if (bomodel.g2DoBMinor == null) bomodel.g2DoBMinor = Convert.ToDateTime("1/1/1900");
                oCommand.Parameters.AddWithValue("@g2DoBMinor", bomodel.g2DoBMinor);
                if (bomodel.g2MaturityDoM == null) bomodel.g2MaturityDoM = Convert.ToDateTime("1/1/1900");
                oCommand.Parameters.AddWithValue("@g2MaturityDoM", bomodel.g2MaturityDoM);
                oCommand.Parameters.AddWithValue("@g2IsResident", bomodel.g2IsResident);
                if (bomodel.g2DoB == null) bomodel.g2DoB = Convert.ToDateTime("1/1/1900");
                oCommand.Parameters.AddWithValue("@g2DoB", bomodel.g2DoB);
                oCommand.Parameters.AddWithValue("@g2NID", bomodel.g2NID);
                oCommand.Parameters.AddWithValue("@g2Address", bomodel.g2Address);
                oCommand.Parameters.AddWithValue("@g2City", bomodel.g2City);
                oCommand.Parameters.AddWithValue("@g2PostCode", bomodel.g2PostCode);
                oCommand.Parameters.AddWithValue("@g2Division", bomodel.g2Division);
                oCommand.Parameters.AddWithValue("@g2Country", bomodel.g2Country);
                oCommand.Parameters.AddWithValue("@g2Mobile", bomodel.g2Mobile);
                oCommand.Parameters.AddWithValue("@g2Tel", bomodel.g2Tel);
                oCommand.Parameters.AddWithValue("@g2Fax", bomodel.g2Fax);
                oCommand.Parameters.AddWithValue("@g2Email", bomodel.g2Email);

                //oCommand.Parameters.AddWithValue("@pDate", DateTime.Now.ToString());
                oCommand.Parameters.AddWithValue("@nIsComplete", 1);




                oConnection.Open();
                i = oCommand.ExecuteNonQuery();
                oConnection.Close();

            }
            catch (Exception ex)
            {
                return false;
            }
            if (i >= 1)
                return true;
            else
                return false;

        }

    }
}