using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models.RQModel
{
    public class GenericRQModel<T>
    {
        public T Param { get; set; }
    }
}