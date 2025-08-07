using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using RoyalCapitalBE.Models;
using RoyalCapital_3.Models;
using RoyalCapital_3.BL;

namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class verController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public verController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            string rclConn = configuration.GetConnectionString("RCLWEB");

            SqlConnection oConnection = new SqlConnection(rclConn);
            try
            {
                oConnection.Open();
                string query = "SELECT AppVersion FROM RCLWEB.[RCLWEB].[dbo].T_ROYALTOUCHAPPVERSION";

                SqlCommand cmd = new SqlCommand(query, oConnection);


                DataTable oTable = new DataTable();
                oTable.Load(cmd.ExecuteReader(CommandBehavior.CloseConnection));

                List<AppVer> listpn = new List<AppVer>();

                foreach (DataRow dr in oTable.Rows)
                {
                    AppVer pn = new AppVer();

                    pn.ver = dr["AppVersion"].ToString();

                    listpn.Add(pn);
                }

                return Ok(listpn);
            }
            catch (Exception ex)
            {
                return NotFound(ex.ToString());
            }
        }
    }
}
