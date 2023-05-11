using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class GenericDataModel<T>
    {
        public List<T> Data { get; set; }
        public int Total { get; set; }

        public GenericDataModel()
        { 
        }
    }
     
}