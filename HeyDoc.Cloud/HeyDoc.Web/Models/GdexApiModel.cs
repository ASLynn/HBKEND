using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class GdexApiModel<T>
    {
        public string s { get; set; }
        public T r { get; set; }
        public string e { get; set; }
    }

    public class GdexShipmentStatusDetailModel
    {
        public string consignmentNote { get; set; }
        public string latestConsignmentNoteStatus { get; set; }
        public GdexShipmentStatus latestEnumStatus { get; set; }
        public GdexShipmentStatusDetailStatusModel[] cnDetailStatusList { get; set; }
    }

    public class GdexShipmentStatusDetailStatusModel
    {
        public GdexShipmentStatus enumStatus { get; set; }
        public string consignmentNoteStatus { get; set; }
        public DateTime dateScan { get; set; }
        public string statusDetail { get; set; }
        public string location { get; set; }
    }

    public class GdexGetConsignmentImageErrorModel
    {
        public string consignmentNo { get; set; }
        public string errorMessage { get; set; }
    }
}