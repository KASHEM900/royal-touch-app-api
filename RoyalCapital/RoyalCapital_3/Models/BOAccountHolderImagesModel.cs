using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoyalCapital_3.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace RoyalCapital_3.Models
{
    public class BOAccountHolderIndvImagesModel
    {      
        public string TRACKINGNO { get; set; }
        public string fImage { get; set; }
        public string fNIDFont { get; set; }
        public string fNIDBack { get; set; }
        public string fSig { get; set; }

        public IFormFile fIFile { get; set; }
        public IFormFile fIFileDef { get; set; }

        public IFormFile fNFile { get; set; }
        public IFormFile fNFileDef { get; set; }

        public IFormFile fNBFile { get; set; }
        public IFormFile fNBFileDef { get; set; }

        public IFormFile fSFile { get; set; }
        public IFormFile fSFileDef { get; set; }


    }
    public class BOAccountHolderJointImagesModel
    {
        public string TRACKINGNO { get; set; }
        public string jImage { get; set; }
        public string jNIDFont { get; set; }
        public string jNIDBack { get; set; }
        public string jSig { get; set; }

        public IFormFile jIFile { get; set; }
        public IFormFile jIFileDef { get; set; }

        public IFormFile jNFile { get; set; }
        public IFormFile jNFileDef { get; set; }

        public IFormFile jNBFile { get; set; }
        public IFormFile jNBFileDef { get; set; }

        public IFormFile jSFile { get; set; }
        public IFormFile jSFileDef { get; set; }


    }
}