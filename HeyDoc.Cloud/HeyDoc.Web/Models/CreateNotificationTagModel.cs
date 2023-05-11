using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class CreateNotificationTagModel
    {
        [DisplayName("Tag Name")]
        public string TagName { get; set; }
    }
}