using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RoyalCapital_3.BL;
using RoyalCapital_3.Models;
using RoyalCapitalBE.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RoyalCapital_3.Controllers
{
    [Route("api/v1/bini")]
    [ApiController]
    public class BINIBOAcknowledgmentController : ControllerBase
    {
        private readonly IConfiguration configuration; 
        private readonly string SqlConn; readonly string totalAmount;
        private readonly IWebHostEnvironment _environment;
        public BINIBOAcknowledgmentController(IConfiguration config, IWebHostEnvironment environment)
        {
            configuration = config;
            SqlConn = config.GetConnectionString("RCLWEB");
            totalAmount = configuration["PaymentGateway:TotalAmount"];
            _environment = environment;
        }

        [HttpGet("bo-acknowledgment")]
        public IActionResult bOAcknowledgment([FromQuery] string clientCode, [FromQuery] string mobile)
        {
            try
            {
                if (!ValidateBiniApiKey())
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Unauthorized API Access."
                    });
                }


                if (string.IsNullOrWhiteSpace(clientCode) || string.IsNullOrWhiteSpace(mobile))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Client Code or Mobile Number not found."
                    });
                }

                using (SqlConnection con = new SqlConnection(SqlConn))
                {
                    string query = @"SELECT TOP 1 BOID AS BoId, BOTYPE AS BoType, RCODE AS ClientCode, FHNAME AS FirstHolder, fhShort AS ShortName, 
                                     SHNAME AS SecondHolder,
                                     SEX AS Gender, DOB AS DoB, FATHER AS FatherName, MOTHER AS MotherName, OCCUPATION AS Occupation, 
                                     RESIDENCY AS Residency, NATIONALITY AS Nationality, LTRIM(RTRIM(ISNULL(ADD1, '') + CASE WHEN ISNULL(ADD1, '') <> ''
                                     AND ISNULL(ADD2, '') <> '' THEN ' ' ELSE '' END + ISNULL(ADD2, '') + CASE WHEN (ISNULL(ADD1, '') <> ''
                                     OR ISNULL(ADD2, '') <> '') AND ISNULL(ADD3, '') <> '' THEN ' ' ELSE '' END + ISNULL(ADD3, '') )) AS Address,
                                     CITY AS City, STATE AS Division, COUNTRY AS Country, MOBILE AS Mobile, NID1 AS NID, passNum AS PassportNo, EMAIL AS Email, BNAME AS Bank,
                                     BBRANCH AS Branch, bAccNum AS AccountNo, ROUTING AS Routing, TIN AS TIN FROM T_CLIENT1
                                     WHERE MOBILE = @mobile AND RCODE = @clientCode";


                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@mobile", SqlDbType.VarChar).Value = mobile.Trim();
                        cmd.Parameters.Add("@clientCode", SqlDbType.VarChar).Value = clientCode.Trim();

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return NotFound(new
                                {
                                    success = false,
                                    message = "No Client Information Found!!!"
                                });
                            }

                            var accountInfo = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                accountInfo[reader.GetName(i)] =
                                    reader.IsDBNull(i) ? null : reader.GetValue(i);
                            }

                            // ==============================
                            // GENERATE PDF
                            // ==============================

                            QuestPDF.Settings.License = LicenseType.Community;

                            // Logo Path
                            string logoPath = Path.Combine(
                                _environment.ContentRootPath,
                                "Image",
                                "RoyalCapLogo.png"
                            );

                            byte[] logoBytes = System.IO.File.Exists(logoPath)
                                ? System.IO.File.ReadAllBytes(logoPath)
                                : null;

                            // Print Date
                            string printDate = DateTime.UtcNow
                                .AddHours(6)
                                .ToString("dd-MMMM-yyyy");

                            // Customer Information
                            string firstHolder = accountInfo["FirstHolder"]?.ToString() ?? "";
                            string boId = accountInfo["BoId"]?.ToString() ?? "";

                            string address = accountInfo["Address"]?.ToString() ?? "";
                            string division = accountInfo["Division"]?.ToString() ?? "";
                            string country = accountInfo["Country"]?.ToString() ?? "";

                            // Address and Division/Country Separate
                            string divisionCountry = string.Join(", ",
                                new[] { division, country }
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                            );

                            // ==============================
                            // PDF DOCUMENT
                            // ==============================

                            byte[] pdfBytes = Document.Create(container =>
                            {
                                // =====================================
                                // PAGE 1: BO SETUP ACKNOWLEDGEMENT
                                // =====================================

                                container.Page(page =>
                                {
                                    page.Size(PageSizes.A4);
                                    page.Margin(40);

                                    page.DefaultTextStyle(x => x.FontSize(11));

                                    // HEADER
                                    page.Header().Column(header =>
                                    {
                                        header.Item().Row(row =>
                                        {
                                            // Logo + Address + Hotline: Center aligned
                                            row.RelativeItem().AlignCenter().Column(col =>
                                            {
                                                if (logoBytes != null)
                                                {
                                                    col.Item()
                                                        .AlignCenter()
                                                        .Height(60)
                                                        .Image(logoBytes)
                                                        .FitArea();
                                                }

                                                col.Item()
                                                    .PaddingTop(1)
                                                    .AlignCenter()
                                                    .Text("DSE Annex Building, 9/E, Motijheel C/A, Dhaka")
                                                    .FontSize(13);

                                                col.Item()
                                                    .PaddingTop(2)
                                                    .AlignCenter()
                                                    .Text("Hotline: 16379, 09606016379")
                                                    .FontSize(13);
                                                    
                                            });

                                            // Print Date: Top Right
                                            row.ConstantItem(90)
                                                .AlignLeft()
                                                .AlignTop()
                                                .Text(printDate)
                                                .FontSize(8)
                                                .FontColor(Colors.Grey.Darken2);
                                        });

                                        // Horizontal Line
                                        header.Item()
                                            .PaddingTop(10)
                                            .LineHorizontal(1.5f)
                                            .LineColor(Colors.Blue.Darken2);
                                    });

                                    // PAGE 1 CONTENT
                                    page.Content()
                                        .PaddingTop(25)
                                        .Column(column =>
                                        {
                                            column.Spacing(12);

                                            // Customer Name
                                            column.Item()
                                                .Text(firstHolder)
                                                .Bold()
                                                .FontSize(12);

                                            // Customer Address
                                            column.Item()
                                                .Text(address)
                                                .FontSize(10);

                                            // Division and Country
                                            column.Item()
                                                .Text(divisionCountry)
                                                .FontSize(10);

                                            // Subject
                                            column.Item()
                                                .PaddingTop(15)
                                                .Text("Subject: BO Setup Acknowledgement")
                                                .Bold()
                                                .FontSize(12);

                                            column.Item()
                                                .PaddingTop(15)
                                                .Text("Dear Sir/Madam,");

                                            column.Item()
                                                .Text(
                                                    "We are pleased to inform you that your request for " +
                                                    "opening a new BO account has been setup."
                                                );

                                            // BO Account Number
                                            column.Item()
                                                .PaddingTop(8)
                                                .Text(text =>
                                                {
                                                    text.Span("Account Number : ");
                                                    text.Span(boId).Bold();
                                                });

                                            column.Item()
                                                .Text(
                                                    "Please verify BO Account Details specified in Annexure."
                                                );

                                            column.Item()
                                                .Text(
                                                    "All further communication in this connection should be " +
                                                    "addressed quoting Beneficiary Identification Number " +
                                                    "(BO ID) to us as stated above."
                                                );

                                            column.Item()
                                                .Text(
                                                    "Assuring you of our best services always."
                                                );

                                            column.Item()
                                                .Text("Thanking You, Yours faithfully");

                                            // Authorized Signatory
                                            column.Item()
                                                .PaddingTop(65)
                                                .Text("____________________________")
                                                .FontSize(10);

                                            column.Item()
                                                .Text("Authorised Signatory")
                                                .Bold()
                                                .FontSize(10);
                                        });

                                    // FOOTER
                                    page.Footer()
                                        .AlignCenter()
                                        .Text("Page 1 of 3")
                                        .FontSize(8)
                                        .FontColor(Colors.Grey.Darken1);
                                });


                                // =====================================
                                // PAGE 2: ANNEXURE
                                // =====================================

                                container.Page(page =>
                                {
                                    page.Size(PageSizes.A4);
                                    page.Margin(40);

                                    page.DefaultTextStyle(x => x.FontSize(10));

                                    // HEADER - SAME AS PAGE 1
                                    page.Header().Column(header =>
                                    {
                                        header.Item().Row(row =>
                                        {
                                            // Center Logo and Company Information
                                            row.RelativeItem().AlignCenter().Column(col =>
                                            {
                                                // Logo
                                                if (logoBytes != null)
                                                {
                                                    col.Item()
                                                        .AlignCenter()
                                                        .Height(60)
                                                        .Image(logoBytes)
                                                        .FitArea();
                                                }

                                                // Address
                                                col.Item()
                                                    .PaddingTop(1)
                                                    .AlignCenter()
                                                    .Text("DSE Annex Building, 9/E, Motijheel C/A, Dhaka")
                                                    .FontSize(13);

                                                // Hotline
                                                col.Item()
                                                    .PaddingTop(2)
                                                    .AlignCenter()
                                                    .Text("Hotline: 16379, 09606016379")
                                                    .FontSize(13);
                                                    
                                            });

                                            // Print Date - Right Corner
                                            row.ConstantItem(90)
                                                .AlignRight()
                                                .AlignTop()
                                                .Text(printDate)
                                                .FontSize(8)
                                                .FontColor(Colors.Grey.Darken2);
                                        });

                                        header.Item()
                                            .PaddingTop(10)
                                            .LineHorizontal(1.5f)
                                            .LineColor(Colors.Blue.Darken2);
                                    });


                                    // PAGE 2 CONTENT
                                    page.Content()
                                        .PaddingTop(20)
                                        .Column(column =>
                                        {
                                            column.Spacing(10);

                                            // Annexure Title
                                            column.Item()
                                                .AlignCenter()
                                                .Text("BO ACCOUNT DETAILS")
                                                .Bold()
                                                .FontSize(12);

                                            // Client Information Table
                                            column.Item()
                                                .PaddingTop(12)
                                                .Table(table =>
                                                {
                                                    table.ColumnsDefinition(columns =>
                                                    {
                                                        columns.RelativeColumn(3);
                                                        columns.RelativeColumn(5);
                                                    });

                                                    // Table Header
                                                    table.Header(header =>
                                                    {
                                                        header.Cell()
                                                            .Background(Colors.Blue.Darken2)
                                                            .Padding(8)
                                                            .Text("Particulars")
                                                            .Bold()
                                                            .FontColor(Colors.White);

                                                        header.Cell()
                                                            .Background(Colors.Blue.Darken2)
                                                            .Padding(8)
                                                            .Text("Details")
                                                            .Bold()
                                                            .FontColor(Colors.White);
                                                    });

                                                    // Database Information
                                                    foreach (var item in accountInfo)
                                                    {
                                                        string value = item.Value == null
                                                            ? ""
                                                            : item.Value is DateTime date
                                                                ? date.ToString("dd-MMMM-yyyy")
                                                                : item.Value.ToString();

                                                        string label = System.Text.RegularExpressions.Regex
                                                            .Replace(
                                                                item.Key,
                                                                "([a-z])([A-Z])",
                                                                "$1 $2"
                                                            );

                                                        table.Cell()
                                                            .BorderBottom(0.5f)
                                                            .BorderColor(Colors.Grey.Lighten2)
                                                            .Padding(7)
                                                            .Text(label)
                                                            .Bold();

                                                        table.Cell()
                                                            .BorderBottom(0.5f)
                                                            .BorderColor(Colors.Grey.Lighten2)
                                                            .Padding(7)
                                                            .Text(value);
                                                    }
                                                });
                                        });

                                    // FOOTER
                                    page.Footer()
                                    .AlignCenter()
                                    .Text(text =>
                                    {
                                        text.Span("Page ")
                                            .FontSize(8)
                                            .FontColor(Colors.Grey.Darken1);

                                        text.CurrentPageNumber()
                                            .FontSize(8)
                                            .FontColor(Colors.Grey.Darken1);

                                        text.Span(" of ")
                                            .FontSize(8)
                                            .FontColor(Colors.Grey.Darken1);

                                        text.TotalPages()
                                            .FontSize(8)
                                            .FontColor(Colors.Grey.Darken1);
                                    });
                                });

                            }).GeneratePdf();


                            // ==============================
                            // RETURN PDF FILE
                            // ==============================

                            return Ok(new
                            {
                                success = true,
                                message = "BO Acknowledgment generated successfully.",
                                accountInfo = accountInfo,
                                fileName = $"BO_Acknowledgment_{clientCode}.pdf",
                                contentType = "application/pdf",
                                pdfBase64 = Convert.ToBase64String(pdfBytes)
                            });

                            //return File(
                            //    pdfBytes,
                            //    "application/pdf",
                            //    $"BO_Acknowledgment_{clientCode}.pdf"
                            //);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "PDF Generation / Database Error.",
                    error = ex.Message
                });
            }
        }

        private string FormatLabel(string key)
        {
            return System.Text.RegularExpressions.Regex
                .Replace(key, "([a-z])([A-Z])", "$1 $2");
        }
        private bool ValidateBiniApiKey()
        {
            var apiKey = Request.Headers["X-API-Key"].ToString();

            var configuredKey = configuration["BiniApi:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(configuredKey))
            {
                return false;
            }

            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(apiKey),
                    System.Text.Encoding.UTF8.GetBytes(configuredKey)
                );
        }





    }
}
