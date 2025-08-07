using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RoyalCapital_3.BL;
using RoyalCapital_3.Models;
using RoyalCapitalBE.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoyalCapital_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BOAccountHolderImagesController : ControllerBase
    {
        private readonly IConfiguration configuration; 
        private readonly string SqlConn;
        public BOAccountHolderImagesController(IConfiguration config)
        {
            configuration = config;
            SqlConn = config.GetConnectionString("RCLWEB");
        }

        [HttpPost("boAccountHolderIndvImage")]
        [RequestSizeLimit(10_000_000)] 
        public async Task<IActionResult> boAccountHolderIndvImage([FromForm] string trackingNo,[FromForm] IFormFile fIFile,[FromForm] IFormFile fNFile,
            [FromForm] IFormFile fNBFile,[FromForm] IFormFile fSFile)
        {
            if (string.IsNullOrEmpty(trackingNo))
            {
                return BadRequest(new { success = false, message = "Tracking number is required." });
            }

            if (fIFile == null || fNFile == null || fNBFile == null || fSFile == null)
            {
                return BadRequest(new { success = false, message = "All four images are required." });
            }

            try
            {
                byte[] fIFileBytes = await ConvertToBytes(fIFile);
                byte[] fNFileBytes = await ConvertToBytes(fNFile);
                byte[] fNBFileBytes = await ConvertToBytes(fNBFile);
                byte[] fSFileBytes = await ConvertToBytes(fSFile);

                using (SqlConnection oConnection = new SqlConnection(SqlConn))
                {
                    await oConnection.OpenAsync();

                    string query = @"UPDATE T_ONLINE_BO_AccHolderImages SET fImage = @fIFile, fNIDFont = @fNFile, fNIDBack = @fNBFile, fSig = @fSFile, pDate = GETDATE(), iIsComplete = 1 WHERE TRACKINGNO = @trackingNo";

                    using (SqlCommand cmd = new SqlCommand(query, oConnection))
                    {
                        cmd.Parameters.AddWithValue("@fIFile", fIFileBytes);
                        cmd.Parameters.AddWithValue("@fNFile", fNFileBytes);
                        cmd.Parameters.AddWithValue("@fNBFile", fNBFileBytes);
                        cmd.Parameters.AddWithValue("@fSFile", fSFileBytes);
                        cmd.Parameters.AddWithValue("@trackingNo", trackingNo);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "Account Holder Images uploaded successfully." });
                        }
                        else
                        {
                            return BadRequest(new { success = false, message = "Update failed. Check tracking number." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error occurred.", error = ex.Message });
            }
        }

        [HttpPost("boAccountHolderJointImage")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> boAccountHolderJointImage([FromForm] string trackingNo, [FromForm] IFormFile jIFile, [FromForm] IFormFile jNFile,
            [FromForm] IFormFile jNBFile, [FromForm] IFormFile jSFile)
        {
            if (string.IsNullOrEmpty(trackingNo))
            {
                return BadRequest(new { success = false, message = "Tracking number is required." });
            }

            if (jIFile == null || jNFile == null || jNBFile == null || jSFile == null)
            {
                return BadRequest(new { success = false, message = "All four images are required." });
            }

            try
            {
                byte[] fIFileBytes = await ConvertToBytes(jIFile);
                byte[] fNFileBytes = await ConvertToBytes(jNFile);
                byte[] fNBFileBytes = await ConvertToBytes(jNBFile);
                byte[] fSFileBytes = await ConvertToBytes(jSFile);

                using (SqlConnection oConnection = new SqlConnection(SqlConn))
                {
                    await oConnection.OpenAsync();

                    string query = @"UPDATE T_ONLINE_BO_AccHolderImages SET jImage = @fIFile, jNIDFont = @fNFile, jNIDBack = @fNBFile, jSig = @fSFile, pDate = GETDATE(), iIsComplete = 1 WHERE TRACKINGNO = @trackingNo";

                    using (SqlCommand cmd = new SqlCommand(query, oConnection))
                    {
                        cmd.Parameters.AddWithValue("@fIFile", fIFileBytes);
                        cmd.Parameters.AddWithValue("@fNFile", fNFileBytes);
                        cmd.Parameters.AddWithValue("@fNBFile", fNBFileBytes);
                        cmd.Parameters.AddWithValue("@fSFile", fSFileBytes);
                        cmd.Parameters.AddWithValue("@trackingNo", trackingNo);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "Account Holder Images uploaded successfully." });
                        }
                        else
                        {
                            return BadRequest(new { success = false, message = "Update failed. Check tracking number." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error occurred.", error = ex.Message });
            }
        }
        private async Task<byte[]> ConvertToBytes(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
