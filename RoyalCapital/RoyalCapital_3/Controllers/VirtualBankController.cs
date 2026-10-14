
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RoyalCapital_3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VirtualBankController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly string SqlConn;

        public VirtualBankController(IConfiguration config)
        {
            configuration = config;
            SqlConn = config.GetConnectionString("RCLWEB");
        }

        public class AccountRequest
        {
            public string ClientCode { get; set; } = string.Empty;
        }

        [HttpPost("account-details")]
        public async Task<IActionResult> GetAccountDetails([FromBody] AccountRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ClientCode))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ClientCode is required."
                });
            }

            string? connectionString =  configuration.GetConnectionString("RCLWEB");

            if (connectionString == null)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Database connection is not configured."
                });
            }

            const string query = @"SELECT 'Royal Capital Ltd' AS [AccountHolderName],'77061' + SUBSTRING(CAST(C.BOID AS VARCHAR(50)), 9, 8) 
                     AS [AccountNumber], N'STANDARD CHARTERED BANK' AS [BankName], N'CHITTAGONG(MAIN)' AS [Branch], N'215152143' AS [RoutingNo],
                     VB.Id AS [VirtualBankId], VB.IsActive AS [IsActive],
                     CASE
                         WHEN VB.Id IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS [IsConfigured]
                     FROM dbo.T_CLIENT1 C OUTER APPLY (SELECT TOP (1) B.Id, B.IsActive FROM dbo.T_Virtual_Bank B WHERE UPPER(LTRIM(RTRIM(B.BankName))) =
                          N'STANDARD CHARTERED BANK' ORDER BY B.Id) VB  WHERE C.RCODE = @ClientCode

                UNION ALL

                SELECT 'Royal Capital Ltd' AS [AccountHolderName], '776615900' + SUBSTRING(CAST(C.BOID AS VARCHAR(50)), 9, 8) AS [AccountNumber],
                    N'EASTERN BANK PLC' AS [BankName], N'AGRABAD' AS [Branch],N'095150136' AS [RoutingNo], VB.Id AS [VirtualBankId],
                    VB.IsActive AS [IsActive],
                    CASE
                        WHEN VB.Id IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS [IsConfigured] FROM dbo.T_CLIENT1 C OUTER APPLY ( SELECT TOP (1)
                        B.Id, B.IsActive FROM dbo.T_Virtual_Bank B WHERE UPPER(LTRIM(RTRIM(B.BankName))) = N'EASTERN BANK PLC' ORDER BY B.Id) VB
                WHERE C.RCODE = @ClientCode;";

            try
            {
                var banks = new List<object>();

                await using var connection = new SqlConnection(connectionString);

                await using var command = new SqlCommand(query, connection);

                command.Parameters.Add("@ClientCode", SqlDbType.VarChar, 50).Value = request.ClientCode.Trim();

                await connection.OpenAsync();

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    bool isConfigured = Convert.ToBoolean(reader["IsConfigured"]);

                    bool? isActive = reader["IsActive"] == DBNull.Value ? (bool?) null : Convert.ToBoolean(reader["IsActive"]);

                    string status = !isConfigured ? "NotConfigured" : isActive == true ? "Active" : "Inactive";

                    banks.Add(new
                    {
                        accountHolderName = reader["AccountHolderName"].ToString(),

                        accountNumber = reader["AccountNumber"].ToString(),

                        bankName = reader["BankName"].ToString(),

                        branch = reader["Branch"].ToString(),

                        routingNo = reader["RoutingNo"].ToString(),

                        virtualBankId = reader["VirtualBankId"] == DBNull.Value? (int?)null: Convert.ToInt32(reader["VirtualBankId"]),

                        isConfigured, isActive, status
                    });
                }

                if (banks.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ClientCode not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    clientCode = request.ClientCode.Trim(),
                    banks
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve virtual bank details."
                });
            }
        }
    }
}
