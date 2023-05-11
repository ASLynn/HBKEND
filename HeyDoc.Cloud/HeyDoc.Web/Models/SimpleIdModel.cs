using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class SimpleIdModel
    {
        public int Result { get; set; }

        public SimpleIdModel()
        {

        }

        public SimpleIdModel(int result)
        {
            Result = result;
        }
    }
}