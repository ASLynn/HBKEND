using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class IcdEntryModel
    {
        public string IcdEntityId { get; set; }
        public string IcdCode { get; set; }
        public string IcdCodeDescription { get; set; }
        public string ReleaseId { get; set; }
        public string LinearizationName { get; set; }

    }
}