using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PropertyCountsReportModel<T>
    {
        public string Name { get; set; }
        public List<T> Counts { get; set; }
    }
}