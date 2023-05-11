using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class GraphModel
    {
         public string Xvalue { get; set; }
         public string Yvalue { get; set; }
        public GraphModel()
        {

        }
        public GraphModel(string xValue, string yValue)
        {
            Xvalue = xValue;
            Yvalue = yValue;
        }
    }
}