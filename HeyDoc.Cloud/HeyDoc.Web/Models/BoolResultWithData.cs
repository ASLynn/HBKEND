using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class BoolResultWithData<T>: BoolResult
    {
        public T Data { get; set; }

        public BoolResultWithData() { }

        public BoolResultWithData(bool result, T data): base(result)
        {
            Data = data;
        }
    }
}