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
    public class NotificationController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public NotificationController(IConfiguration config)
        {
            configuration = config;
        }

        [HttpGet("")]
        public IActionResult push()
        {
            string rclConn = configuration.GetConnectionString("RCLWEB");

            SqlConnection oConnection = new SqlConnection(rclConn);
            try
            {
                oConnection.Open();
                string query = "SELECT * FROM RCLWEB.[RCLWEB].[dbo].T_PUSHNOTIFICATION ORDER BY Id DESC";

                SqlCommand cmd = new SqlCommand(query, oConnection);


                DataTable oTable = new DataTable();
                oTable.Load(cmd.ExecuteReader(CommandBehavior.CloseConnection));

                

                List< pushnotification> listpn=new List<pushnotification>();

                foreach (DataRow dr in oTable.Rows)
                {
                    pushnotification pn = new pushnotification();

                    pn.Title = dr["Title"].ToString();
                    pn.Body = dr["Body"].ToString();
                    pn.Url = dr["Url"].ToString();

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
