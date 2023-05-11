using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class VirtualPointModel
    {
        public int VirtualPointId { get; set; }
        public decimal VPBal { get; set; }
        public string DorP { get; set; }

        public int UserId { get; set; }
        public VirtualPointModel()
        {

        }

        public VirtualPointModel(Entity.VirtualPoint entityVirtualPoint)
        {
            VirtualPointId = entityVirtualPoint.VirtualPointId;
            VPBal = entityVirtualPoint.VPBal;
            DorP = entityVirtualPoint.DorP;
            UserId = entityVirtualPoint.UserId;

        }
    }
    public class VirtualPointDetailsModel
    {
        public int VirtualPointDetailsId { get; set; }
        public int VirtualPointId { get; set; }
        public string VPDesc { get; set; }

        public string VPType { get; set; }

        public DateTime VPDT { get; set; }

        public decimal VPBal { get; set; }
        public VirtualPointDetailsModel()
        {

        }

        public VirtualPointDetailsModel(Entity.VirtualPointDetails entityVirtualPointDetails)
        {
            VirtualPointDetailsId = entityVirtualPointDetails.VirtualPointDetailsId;
            VirtualPointId = entityVirtualPointDetails.VirtualPointId;
            VPDesc = entityVirtualPointDetails.VPDesc;
            VPType = entityVirtualPointDetails.VPType;
            VPDT = entityVirtualPointDetails.VPDT;
            VPBal = entityVirtualPointDetails.VPBal;

        }
    }
}