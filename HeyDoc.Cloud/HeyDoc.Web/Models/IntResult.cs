using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class IntResult
    {
        public int Result { get; set; }
        public IntResult()
        {

        }

        public IntResult(int result)
        {
            Result = result;
        }
    }
}