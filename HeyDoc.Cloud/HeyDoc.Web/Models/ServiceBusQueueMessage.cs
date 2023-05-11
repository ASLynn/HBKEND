using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class ServiceBusQueueMessage<T>
    {
        public long SequenceNumber { get; set; }
        public T Message { get; set; }
    }
}