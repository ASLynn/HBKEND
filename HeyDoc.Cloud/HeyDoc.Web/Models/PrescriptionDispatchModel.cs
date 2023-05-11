using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PrescriptionDispatchModel
    {
        public int DispatchId { get; set; }
        public PrescriptionStatus PrescriptionStatus { get; set; }
        public string DeliveryAddress { get; set; }
        public int? OutletId { get; set; }
        public int? OnsiteId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ApproveDate { get; set; }
        public DateTime? ReadyOrDeliveredDate { get; set; }

        public long PrescriptionId { get; set; }
        public DateTime? NextDispenseDate { get; set; }
        public DispatchStatus DispatchStatus { get; set; }

        public OnSiteDispenseModel OnSite { get; set; }
        public UserModel PharmacyOutlet { get; set; }
        public List<PrescriptionLogModel> PrescriptionLog { get; set; }
        public PackingType PackingType { get; set; }

        public PrescriptionDispatchModel()
        {

        }

        public PrescriptionDispatchModel(Entity.PrescriptionDispatch entityDispatch, bool withDispatchLog = false)
        {
            DispatchId = entityDispatch.DispatchId;
            PrescriptionStatus = entityDispatch.PrescriptionStatus;
            PackingType = entityDispatch.PackingType;

            if (PrescriptionStatus == PrescriptionStatus.Delivery)
            {
                DeliveryAddress = entityDispatch.DeliveryAddress;
            }

            if (PrescriptionStatus == PrescriptionStatus.OnSite)
            {
                OnsiteId = entityDispatch.OnSiteId.Value;
                OnSite = new OnSiteDispenseModel(entityDispatch.OnSiteDispens);
            }

            if (PrescriptionStatus == PrescriptionStatus.SelfCollection)
            {
                OutletId = entityDispatch.OutletId.Value;
                PharmacyOutlet = new UserModel(entityDispatch.OutletUser);
            }

            PrescriptionId = entityDispatch.PrescriptionId;
            CreatedDate = entityDispatch.CreatedDate;
            ApproveDate = entityDispatch.ApproveDate;
            ReadyOrDeliveredDate = entityDispatch.ReadyOrDeliveredDate;
            DispatchStatus = entityDispatch.DispatchStatus;
            NextDispenseDate = entityDispatch.NextDispenseDate;

            if (withDispatchLog)
            {
                List<PrescriptionLogModel> logModelList = new List<PrescriptionLogModel>();
                var log = entityDispatch.PrescriptionLogs.Where(e => !e.IsDelete).OrderByDescending(e => e.CreatedDate).ToList();
                foreach (var entityLog in log)
                {
                    PrescriptionLogModel logModel = new PrescriptionLogModel(entityLog);
                    logModelList.Add(logModel);
                }
                PrescriptionLog = logModelList;
            }
        }
    }
}