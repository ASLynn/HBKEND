using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class BoolResult
    {
        public bool Result { get; set; }

        public BoolResult()
        {

        }

        public BoolResult(bool result)
        {
            Result = result;
        }
    }
}