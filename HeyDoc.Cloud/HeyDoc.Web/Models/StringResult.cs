using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class StringResult
    {
        public string Result { get; set; }

        public StringResult()
        {

        }

        public StringResult(string result)
        {
            Result = result;
        }
    }
}