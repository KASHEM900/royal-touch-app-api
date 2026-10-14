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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;


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


        // ACCOUNT HOLDER IMAGE UPLOAD STARTS HERE //

        [HttpPost("boAccountHolderIndvPhoto")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> BoAccountHolderIndvPhoto([FromForm] string trackingNo, [FromForm] IFormFile fIFile)
        {
            if (string.IsNullOrWhiteSpace(trackingNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tracking number is required."
                });
            }

            if (fIFile == null || fIFile.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please select an image."
                });
            }

            // Maximum original upload size: 10 MB
            if (fIFile.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Original image size must not exceed 10 MB."
                });
            }

            try
            {
                byte[] imageBytes;

                // Convert any supported image into JPEG under 300 KB.
                using (var inputStream = fIFile.OpenReadStream())
                {
                    imageBytes = ConvertToJpegUnder300KB(inputStream);
                }

                // Final size validation
                if (imageBytes == null || imageBytes.Length == 0 || imageBytes.Length >= 300 * 1024)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Unable to compress image below 300 KB."
                    });
                }

                string query = @" UPDATE T_ONLINE_BO_AccHolderImages SET fImage = @fIFile WHERE TRACKINGNO = @trackingNo";

                using (SqlConnection con = new SqlConnection(SqlConn))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@fIFile", SqlDbType.Image).Value = imageBytes;

                    cmd.Parameters.Add("@trackingNo", SqlDbType.VarChar, 50).Value = trackingNo;

                    await con.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "No account holder record found for this tracking number."
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Photo uploaded successfully.",
                    contentType = "image/jpeg",
                    sizeKB = Math.Round(imageBytes.Length / 1024.0, 2)
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or unsupported image file."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while uploading the photo."
                });
            }
        }

        [HttpPost("boAccountHolderIndvNIDFNT")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> boAccountHolderIndvNIDFNT([FromForm] string trackingNo, [FromForm] IFormFile fNFile)
        {
            if (string.IsNullOrWhiteSpace(trackingNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tracking number is required."
                });
            }

            if (fNFile == null || fNFile.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please select an image."
                });
            }

            // Maximum original upload size: 10 MB
            if (fNFile.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Original image size must not exceed 10 MB."
                });
            }

            try
            {
                byte[] imageBytes;

                // Convert any supported image into JPEG under 300 KB.
                using (var inputStream = fNFile.OpenReadStream())
                {
                    imageBytes = ConvertToJpegUnder300KB(inputStream);
                }

                // Final size validation
                if (imageBytes == null || imageBytes.Length == 0 || imageBytes.Length >= 300 * 1024)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Unable to compress image below 300 KB."
                    });
                }

                string query = @" UPDATE T_ONLINE_BO_AccHolderImages SET fNIDFont = @fNFile WHERE TRACKINGNO = @trackingNo";

                using (SqlConnection con = new SqlConnection(SqlConn))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@fNFile", SqlDbType.Image).Value = imageBytes;

                    cmd.Parameters.Add("@trackingNo", SqlDbType.VarChar, 50).Value = trackingNo;

                    await con.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "No account holder record found for this tracking number."
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "NID FRONT uploaded successfully.",
                    contentType = "image/jpeg",
                    sizeKB = Math.Round(imageBytes.Length / 1024.0, 2)
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or unsupported image file."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while uploading the photo."
                });
            }
        }

        [HttpPost("boAccountHolderIndvNIDBCK")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> boAccountHolderIndvNIDBCK([FromForm] string trackingNo, [FromForm] IFormFile fNBFile)
        {
            if (string.IsNullOrWhiteSpace(trackingNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tracking number is required."
                });
            }

            if (fNBFile == null || fNBFile.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please select an image."
                });
            }

            // Maximum original upload size: 10 MB
            if (fNBFile.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Original image size must not exceed 10 MB."
                });
            }

            try
            {
                byte[] imageBytes;

                // Convert any supported image into JPEG under 300 KB.
                using (var inputStream = fNBFile.OpenReadStream())
                {
                    imageBytes = ConvertToJpegUnder300KB(inputStream);
                }

                // Final size validation
                if (imageBytes == null || imageBytes.Length == 0 || imageBytes.Length >= 300 * 1024)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Unable to compress image below 300 KB."
                    });
                }

                string query = @" UPDATE T_ONLINE_BO_AccHolderImages SET fNIDBack = @fNBFile WHERE TRACKINGNO = @trackingNo";

                using (SqlConnection con = new SqlConnection(SqlConn))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@fNBFile", SqlDbType.Image).Value = imageBytes;

                    cmd.Parameters.Add("@trackingNo", SqlDbType.VarChar, 50).Value = trackingNo;

                    await con.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "No account holder record found for this tracking number."
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "NID BACK uploaded successfully.",
                    contentType = "image/jpeg",
                    sizeKB = Math.Round(imageBytes.Length / 1024.0, 2)
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or unsupported image file."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while uploading the photo."
                });
            }
        }

        [HttpPost("boAccountHolderIndvSign")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> boAccountHolderIndvSign([FromForm] string trackingNo, [FromForm] IFormFile fSFile)
        {
            if (string.IsNullOrWhiteSpace(trackingNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tracking number is required."
                });
            }

            if (fSFile == null || fSFile.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please select an image."
                });
            }

            // Maximum original upload size: 10 MB
            if (fSFile.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Original image size must not exceed 10 MB."
                });
            }

            try
            {
                byte[] imageBytes;

                // Convert any supported image into JPEG under 300 KB.
                using (var inputStream = fSFile.OpenReadStream())
                {
                    imageBytes = ConvertToJpegUnder300KB(inputStream);
                }

                // Final size validation
                if (imageBytes == null || imageBytes.Length == 0 || imageBytes.Length >= 300 * 1024)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Unable to compress image below 300 KB."
                    });
                }

                string query = @" UPDATE T_ONLINE_BO_AccHolderImages SET fSig = @fSFile, iIsComplete = 1 WHERE TRACKINGNO = @trackingNo";

                using (SqlConnection con = new SqlConnection(SqlConn))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@fSFile", SqlDbType.Image).Value = imageBytes;

                    cmd.Parameters.Add("@trackingNo", SqlDbType.VarChar, 50).Value = trackingNo;

                    await con.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "No account holder record found for this tracking number."
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Signature uploaded successfully.",

                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or unsupported image file."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while uploading the photo."
                });
            }
        }

        //ACCOUNT HOLDER IMAGE UPLOAD ENDS HERE //


        // JOINT ACCOUNT HOLDER IMAGE UPLOAD STARTS HERE //

        [HttpPost("boAccountHolderJointPhoto")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> boAccountHolderJointPhoto([FromForm] string trackingNo, [FromForm] IFormFile jIFile)
        {
            if (string.IsNullOrWhiteSpace(trackingNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tracking number is required."
                });
            }

            if (jIFile == null || jIFile.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please select an image."
                });
            }

            // Maximum original upload size: 10 MB
            if (jIFile.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Original image size must not exceed 10 MB."
                });
            }

            try
            {
                byte[] imageBytes;

                // Convert any supported image into JPEG under 300 KB.
                using (var inputStream = jIFile.OpenReadStream())
                {
                    imageBytes = ConvertToJpegUnder300KB(inputStream);
                }

                // Final size validation
                if (imageBytes == null || imageBytes.Length == 0 || imageBytes.Length >= 300 * 1024)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Unable to compress image below 300 KB."
                    });
                }

                string query = @" UPDATE T_ONLINE_BO_AccHolderImages SET jImage = @jIFile WHERE TRACKINGNO = @trackingNo";

                using (SqlConnection con = new SqlConnection(SqlConn))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@jIFile", SqlDbType.Image).Value = imageBytes;

                    cmd.Parameters.Add("@trackingNo", SqlDbType.VarChar, 50).Value = trackingNo;

                    await con.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "No account holder record found for this tracking number."
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Joint A/C Holder Photo uploaded successfully.",
                    contentType = "image/jpeg",
                    sizeKB = Math.Round(imageBytes.Length / 1024.0, 2)
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or unsupported image file."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while uploading the photo."
                });
            }
        }

        [HttpPost("boAccountHolderJointNIDFNT")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> boAccountHolderJointNIDFNT([FromForm] string trackingNo, [FromForm] IFormFile jNFile)
        {
            if (string.IsNullOrWhiteSpace(trackingNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tracking number is required."
                });
            }

            if (jNFile == null || jNFile.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please select an image."
                });
            }

            // Maximum original upload size: 10 MB
            if (jNFile.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Original image size must not exceed 10 MB."
                });
            }

            try
            {
                byte[] imageBytes;

                // Convert any supported image into JPEG under 300 KB.
                using (var inputStream = jNFile.OpenReadStream())
                {
                    imageBytes = ConvertToJpegUnder300KB(inputStream);
                }

                // Final size validation
                if (imageBytes == null || imageBytes.Length == 0 || imageBytes.Length >= 300 * 1024)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Unable to compress image below 300 KB."
                    });
                }

                string query = @"UPDATE T_ONLINE_BO_AccHolderImages SET jNIDFont = @jNFile WHERE TRACKINGNO = @trackingNo";

                using (SqlConnection con = new SqlConnection(SqlConn))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@jNFile", SqlDbType.Image).Value = imageBytes;

                    cmd.Parameters.Add("@trackingNo", SqlDbType.VarChar, 50).Value = trackingNo;

                    await con.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "No account holder record found for this tracking number."
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Joint A/C Holder NID FRONT uploaded successfully.",
                    contentType = "image/jpeg",
                    sizeKB = Math.Round(imageBytes.Length / 1024.0, 2)
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or unsupported image file."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while uploading the photo."
                });
            }
        }

        [HttpPost("boAccountHolderJointNIDBCK")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> boAccountHolderJointNIDBCK([FromForm] string trackingNo, [FromForm] IFormFile jNBFile)
        {
            if (string.IsNullOrWhiteSpace(trackingNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tracking number is required."
                });
            }

            if (jNBFile == null || jNBFile.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please select an image."
                });
            }

            // Maximum original upload size: 10 MB
            if (jNBFile.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Original image size must not exceed 10 MB."
                });
            }

            try
            {
                byte[] imageBytes;

                // Convert any supported image into JPEG under 300 KB.
                using (var inputStream = jNBFile.OpenReadStream())
                {
                    imageBytes = ConvertToJpegUnder300KB(inputStream);
                }

                // Final size validation
                if (imageBytes == null || imageBytes.Length == 0 || imageBytes.Length >= 300 * 1024)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Unable to compress image below 300 KB."
                    });
                }

                string query = @" UPDATE T_ONLINE_BO_AccHolderImages SET jNIDBack = @jNBFile WHERE TRACKINGNO = @trackingNo";

                using (SqlConnection con = new SqlConnection(SqlConn))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@jNBFile", SqlDbType.Image).Value = imageBytes;

                    cmd.Parameters.Add("@trackingNo", SqlDbType.VarChar, 50).Value = trackingNo;

                    await con.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "No account holder record found for this tracking number."
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Joint A/C Holder NID BACK uploaded successfully.",
                    contentType = "image/jpeg",
                    sizeKB = Math.Round(imageBytes.Length / 1024.0, 2)
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or unsupported image file."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while uploading the photo."
                });
            }
        }

        [HttpPost("boAccountHolderJointSign")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> boAccountHolderJointSign([FromForm] string trackingNo, [FromForm] IFormFile jSFile)
        {
            if (string.IsNullOrWhiteSpace(trackingNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tracking number is required."
                });
            }

            if (jSFile == null || jSFile.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please select an image."
                });
            }

            // Maximum original upload size: 10 MB
            if (jSFile.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Original image size must not exceed 10 MB."
                });
            }

            try
            {
                byte[] imageBytes;

                // Convert any supported image into JPEG under 300 KB.
                using (var inputStream = jSFile.OpenReadStream())
                {
                    imageBytes = ConvertToJpegUnder300KB(inputStream);
                }

                // Final size validation
                if (imageBytes == null || imageBytes.Length == 0 || imageBytes.Length >= 300 * 1024)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Unable to compress image below 300 KB."
                    });
                }

                string query = @" UPDATE T_ONLINE_BO_AccHolderImages SET jSig = @jSFile, iIsComplete = 1 WHERE TRACKINGNO = @trackingNo";

                using (SqlConnection con = new SqlConnection(SqlConn))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@jSFile", SqlDbType.Image).Value = imageBytes;

                    cmd.Parameters.Add("@trackingNo", SqlDbType.VarChar, 50).Value = trackingNo;

                    await con.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "No account holder record found for this tracking number."
                        });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Joint A/C Holder Signature uploaded successfully.",

                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or unsupported image file."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while uploading the photo."
                });
            }
        }

        //Joint Account Holder Image Upload Ends Here //

        private byte[] ConvertToJpegUnder300KB(Stream inputStream)
        {
            const int maxSize = 300 * 1024;

            using (System.Drawing.Image original = System.Drawing.Image.FromStream(inputStream))
            {
                int maxDimension = 1200;
                int newWidth;
                int newHeight;

                if (original.Width > original.Height)
                {
                    newWidth = maxDimension;
                    newHeight = (int)((double)original.Height / original.Width * maxDimension);
                }
                else
                {
                    newHeight = maxDimension;
                    newWidth = (int)((double)original.Width / original.Height * maxDimension);
                }

                using (Bitmap resized = new Bitmap(newWidth, newHeight))
                {
                    using (Graphics g = Graphics.FromImage(resized))
                    {
                        g.Clear(Color.White);

                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                        g.DrawImage(original, new Rectangle(0, 0, newWidth, newHeight));
                    }

                    ImageCodecInfo jpgEncoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);


                    for (long quality = 90; quality >= 20; quality -= 5)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (EncoderParameters encoderParameters = new EncoderParameters(1))
                            {
                                encoderParameters.Param[0] = new EncoderParameter( System.Drawing.Imaging.Encoder.Quality, quality );

                                resized.Save(ms, jpgEncoder, encoderParameters);
                            }

                            if (ms.Length <= maxSize)
                            {
                                return ms.ToArray();
                            }
                        }
                    }

                    // Final fallback
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (EncoderParameters encoderParameters = new EncoderParameters(1))
                        {
                            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 20L);

                            resized.Save(ms, jpgEncoder, encoderParameters);
                        }

                        return ms.ToArray();
                    }
                }
            }
        }
    }
}
