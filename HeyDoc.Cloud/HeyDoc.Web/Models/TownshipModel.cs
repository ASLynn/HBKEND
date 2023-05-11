using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class TownshipModel
    {
        public int TownshipId { get; set; }
        public int TownshipCode { get; set; }
        public int StateId { get; set; }
        public string TownshipDesc { get; set; }
        public string TownshipDescMM { get; set; }
        public int TownshipStatus { get; set; }

        public string TownshipNRCabb_EN { get; set; }

        public string TownshipNRCabb_MM { get; set; }

        public TownshipModel()
        { }

        public TownshipModel (Entity.Township entityTownship)
        {
            TownshipId = entityTownship.TownshipId;
            TownshipCode = entityTownship.TownshipCode;
            StateId = entityTownship.StateId;
            TownshipDesc = entityTownship.TownshipDesc;
            TownshipDescMM = entityTownship.TownshipDescMM;
            TownshipStatus = entityTownship.TownshipStatus;
            TownshipNRCabb_EN = entityTownship.TownshipNRCabb_EN;
            TownshipNRCabb_MM = entityTownship.TownshipNRCabb_MM;
        }








    }
}