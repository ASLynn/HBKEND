using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class CorporatePositionModel
    {
        public int PositionId { get; set; }
        public int CorporateId { get; set; }
        public string Position { get; set; }

        public int Active { get; set; }
        public CorporatePositionModel()
        {

        }
        public CorporatePositionModel(Entity.CorporatePosition entityCorporate)
        {
            PositionId = entityCorporate.PositionId;
            CorporateId = entityCorporate.CorporateId;
            Position = entityCorporate.Position;
            Active = entityCorporate.Active;
        }
    }
}