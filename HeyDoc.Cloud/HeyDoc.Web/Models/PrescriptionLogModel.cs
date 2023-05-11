using System;

namespace HeyDoc.Web.Models
{
    public class PrescriptionLogModel
    {
        public long LogId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDelete { get; set; }
        public string LogText { get; set; }
        public string LogUrl { get; set; }
        public PnActionType LogType { get; set; }

        public PrescriptionLogModel()
        {

        }

        public PrescriptionLogModel(Entity.PrescriptionLog entityLog)
        {
            LogId = entityLog.LogId;
            CreatedDate = entityLog.CreatedDate;
            IsDelete = entityLog.IsDelete;
            LogText = entityLog.LogText;
            LogUrl = entityLog.LogUrl;
            LogType = entityLog.LogType;
        }
    }
}