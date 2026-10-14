using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RoyalCapital_3.BL;
using RoyalCapital_3.Data;
using RoyalCapital_3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketStatController : ControllerBase
    {
        private readonly ISP_CALL _sp_Call;
        private readonly IConfiguration _configuration;
        public MarketStatController(ISP_CALL sp_Call,IConfiguration configuration)
        {
            _sp_Call = sp_Call;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("getMarketNews")]
        public ActionResult<List<MarketNews>> GetMarketNews(string key)
        {
            string decryptedData;
            string secretKey = _configuration["secret"];
            try
            {
                decryptedData = TripleDESImp.Decrypt(key.Trim(), true, secretKey);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = "invalid user" });
            }
            string[] subs = decryptedData.Split('+');

            string sisConn = _configuration.GetConnectionString("SISROYALU");
            string rclConn = _configuration.GetConnectionString("RCLWEB");

            if (!SecretKeyChecker.SectetKeyChecker(rclConn, subs[0], subs[1]))
            {
                return NotFound(new { error = "invalid user" });
            }

            try
            {
                string query = @"SELECT TOP (10) [SL],[MAN_DATE_TIME],[MAN_PREFIX],[MAN_ANNOUNCE]
                            FROM [RCLWEB].[dbo].[T_DSE_MDS_MAN] WHERE MAN_PREFIX NOT IN('EXCH','REGL') ORDER BY MAN_DATE_TIME DESC";

                var list = _sp_Call.ListByRawQuery<MarketNews>(query);
                return list;
            }

            catch
            {
                return NotFound(new { error = "something went wrong!!!!" });
            }

        }

        [HttpGet]
        [Route("getMarketNewsByCode")]
        public ActionResult<List<MarketNews>> MarketNewsByCode(string acode, string key)
        {
            string decryptedData;
            string secretKey = _configuration["secret"];
            try
            {
                decryptedData = TripleDESImp.Decrypt(key.Trim(), true, secretKey);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = "invalid user" });
            }
            string[] subs = decryptedData.Split('+');

            string sisConn = _configuration.GetConnectionString("SISROYALU");
            string rclConn = _configuration.GetConnectionString("RCLWEB");

            if (!SecretKeyChecker.SectetKeyChecker(rclConn, subs[0], subs[1]))
            {
                return NotFound(new { error = "invalid user" });
            }

            try
            {
                string query = @"SELECT TOP (10) [SL],[MAN_DATE_TIME],[MAN_PREFIX],[MAN_ANNOUNCE]
                        FROM [RCLWEB].[dbo].[T_DSE_MDS_MAN] a, SISROYALU.dbo.T_PF b, SISROYALU.dbo.T_firms c
                        WHERE b.firmscd=c.firmscd AND a.MAN_PREFIX=c.firmsnm1 AND b.acode='" + acode + "' AND MAN_PREFIX NOT IN('EXCH','REGL') ORDER BY MAN_DATE_TIME DESC";

                var list = _sp_Call.ListByRawQuery<MarketNews>(query);
                return list;
            }

            catch
            {
                return NotFound(new { error = "something went wrong!!!!" });
            }

        }

        [HttpGet]
        [Route("getMarketVolume")]
        public ActionResult<MarketVolume> GetMarketVolume(string key)
        {
            string decryptedData;
            string secretKey = _configuration["secret"];
            try
            {
                decryptedData = TripleDESImp.Decrypt(key.Trim(), true, secretKey);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = "invalid user" });
            }
            string[] subs = decryptedData.Split('+');

            string sisConn = _configuration.GetConnectionString("SISROYALU");
            string rclConn = _configuration.GetConnectionString("RCLWEB");

            if (!SecretKeyChecker.SectetKeyChecker(rclConn, subs[0], subs[1]))
            {
                return NotFound(new { error = "invalid user" });
            }

            try
            {
                string query = @"SELECT TOP 1 [TRD_SNO],[TRD_TOTAL_TRADES],[TRD_TOTAL_VOLUME],[TRD_TOTAL_VALUE]
                                    ,[TRD_LM_DATE_TIME]FROM [RCLWEB].[dbo].[T_DSE_TRD] ORDER BY TRD_LM_DATE_TIME DESC";

                var list = _sp_Call.ListByRawQuery<MarketVolume>(query).FirstOrDefault();
                return list;
            }

            catch
            {
                return NotFound(new { error = "something went wrong!!!!" });
            }

        }

        [HttpGet]
        [Route("getMarketIndex")]
        public ActionResult<List<MarketIndex>> MarketIndex(string key)
        {
            string decryptedData;
            string secretKey = _configuration["secret"];
            try
            {
                decryptedData = TripleDESImp.Decrypt(key.Trim(), true, secretKey);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = "invalid user" });
            }
            string[] subs = decryptedData.Split('+');

            string sisConn = _configuration.GetConnectionString("SISROYALU");
            string rclConn = _configuration.GetConnectionString("RCLWEB");

            if (!SecretKeyChecker.SectetKeyChecker(rclConn, subs[0], subs[1]))
            {
                return NotFound(new { error = "invalid user" });
            }

            try
            {
                string query = @"SELECT TOP 3 [IDX_INDEX_ID],[IDX_DATE_TIME],[IDX_CAPITAL_VALUE],[IDX_DEVIATION],[lDX_PERCENTAGE_DEVIATION] 
                    FROM [RCLWEB].[dbo].[T_DSE_MDS_IDX] a ORDER BY IDX_DATE_TIME DESC,[IDX_CAPITAL_VALUE] DESC,[IDX_INDEX_ID]";

                var list = _sp_Call.ListByRawQuery<MarketIndex>(query);
                return list;
            }

            catch
            {
                return NotFound(new { error = "something went wrong!!!!" });
            }

        }

        [HttpGet]
        [Route("getItemPrice")]
        public ActionResult<List<ItemPrice>> ItemPrices(string key)
        {
            string decryptedData;
            string secretKey = _configuration["secret"];
            try
            {
                decryptedData = TripleDESImp.Decrypt(key.Trim(), true, secretKey);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = "invalid user" });
            }
            string[] subs = decryptedData.Split('+');

            string sisConn = _configuration.GetConnectionString("SISROYALU");
            string rclConn = _configuration.GetConnectionString("RCLWEB");

            if (!SecretKeyChecker.SectetKeyChecker(rclConn, subs[0], subs[1]))
            {
                return NotFound(new { error = "invalid user" });
            }

            try
            {
                string query = @"SELECT TOP (5) [MKISTAT_INSTRUMENT_CODE]
             ,iif(cast([MKISTAT_CLOSE_PRICE] as numeric(20,2))=0,[MKISTAT_PUB_LAST_TRADED_PRICE],[MKISTAT_CLOSE_PRICE]) Last_Trade_Price,[MKISTAT_CLOSE_PRICE]
             ,[MKISTAT_YDAY_CLOSE_PRICE]
	         , CONVERT(numeric(20,2),(((cast([MKISTAT_PUB_LAST_TRADED_PRICE] as numeric(20,2))- cast([MKISTAT_YDAY_CLOSE_PRICE] as numeric(20,2)))/cast([MKISTAT_YDAY_CLOSE_PRICE] as numeric(20,2)))*100)) as Deviation
            FROM [RCLWEB].[dbo].[T_DSE_MKISTAT] WHERE (cast([MKISTAT_YDAY_CLOSE_PRICE] as numeric(20,2)))>0 ORDER BY Deviation DESC";

                var list = _sp_Call.ListByRawQuery<ItemPrice>(query);
                return list;
            }

            catch
            {
                return NotFound(new { error = "something went wrong!!!!" });
            }

        }

        [HttpGet]
        [Route("getLoseItemPrice")]
        public ActionResult<List<ItemPrice>> LoseItemPrices(string key)
        {
            string decryptedData;
            string secretKey = _configuration["secret"];
            try
            {
                decryptedData = TripleDESImp.Decrypt(key.Trim(), true, secretKey);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = "invalid user" });
            }
            string[] subs = decryptedData.Split('+');

            string sisConn = _configuration.GetConnectionString("SISROYALU");
            string rclConn = _configuration.GetConnectionString("RCLWEB");

            if (!SecretKeyChecker.SectetKeyChecker(rclConn, subs[0], subs[1]))
            {
                return NotFound(new { error = "invalid user" });
            }

            try
            {
                string query = @"SELECT TOP (5) [MKISTAT_INSTRUMENT_CODE]
             ,iif(cast([MKISTAT_CLOSE_PRICE] as numeric(20,2))=0,[MKISTAT_PUB_LAST_TRADED_PRICE],[MKISTAT_CLOSE_PRICE]) Last_Trade_Price,[MKISTAT_CLOSE_PRICE]
             ,[MKISTAT_YDAY_CLOSE_PRICE]
	         , CONVERT(numeric(20,2),(((cast([MKISTAT_PUB_LAST_TRADED_PRICE] as numeric(20,2))- cast([MKISTAT_YDAY_CLOSE_PRICE] as numeric(20,2)))/cast([MKISTAT_YDAY_CLOSE_PRICE] as numeric(20,2)))*100)) as Deviation
            FROM [RCLWEB].[dbo].[T_DSE_MKISTAT] WHERE (cast([MKISTAT_YDAY_CLOSE_PRICE] as numeric(20,2))) > 0 AND (cast([MKISTAT_PUB_LAST_TRADED_PRICE] as numeric(20,2)))!=0 ORDER BY Deviation ASC";

                var list = _sp_Call.ListByRawQuery<ItemPrice>(query);
                return list;
            }

            catch
            {
                return NotFound(new { error = "something went wrong!!!!" });
            }

        }

        [HttpGet]
        [Route("getItemPriceByCode")]
        public ActionResult<List<ItemPrice>> ItemPricesByCode(string acode, string key)
        {
            string decryptedData;
            string secretKey = _configuration["secret"];
            try
            {
                decryptedData = TripleDESImp.Decrypt(key.Trim(), true, secretKey);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = "invalid user" });
            }
            string[] subs = decryptedData.Split('+');

            string sisConn = _configuration.GetConnectionString("SISROYALU");
            string rclConn = _configuration.GetConnectionString("RCLWEB");

            if (!SecretKeyChecker.SectetKeyChecker(rclConn, subs[0], subs[1]))
            {
                return NotFound(new { error = "invalid user" });
            }

            try
            {
                string query = @"SELECT TOP (10) [MKISTAT_INSTRUMENT_CODE]
      ,iif(cast([MKISTAT_CLOSE_PRICE] as numeric(20,2))=0,[MKISTAT_PUB_LAST_TRADED_PRICE],[MKISTAT_CLOSE_PRICE]) Last_Trade_Price,[MKISTAT_CLOSE_PRICE],[MKISTAT_YDAY_CLOSE_PRICE]
	  , CONVERT(numeric(20,2),(((cast([MKISTAT_PUB_LAST_TRADED_PRICE] as numeric(20,2))- cast([MKISTAT_YDAY_CLOSE_PRICE] as numeric(20,2)))/cast([MKISTAT_YDAY_CLOSE_PRICE] as numeric(20,2)))*100)) as dev
                FROM [RCLWEB].[dbo].[T_DSE_MKISTAT] a, SISROYALU.dbo.T_PF b, SISROYALU.dbo.T_firms c 
                WHERE b.firmscd=c.firmscd AND a.[MKISTAT_INSTRUMENT_CODE]=c.firmsnm1 AND b.acode='" + acode + "' ORDER BY dev desc";

                var list = _sp_Call.ListByRawQuery<ItemPrice>(query);
                return list;
            }

            catch
            {
                return NotFound(new { error = "something went wrong!!!!" });
            }

        }
    }
}
