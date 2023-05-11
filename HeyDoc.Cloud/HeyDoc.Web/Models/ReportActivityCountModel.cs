using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class ReportActivityCountModel
    {
        public string BranchName { get; set; }
        public int NewSignUpCount { get; set; }
        public int ActiveUsersCount { get; set; }
        public int ChatUseCount { get; set; }
    }
}