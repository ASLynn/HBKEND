using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class StateModel
    {
        public int StateId { get; set; }
        public int StateCode { get; set; }
        public string StateDesc { get; set; }
        public string StateDescMM { get; set; }
        public int StateStatus { get; set; }

        public int? StateNRCcode_EN { get; set; }
        public string StateNRCcode_MM { get; set; }
        
        public StateModel()
        {

        }
        public StateModel(Entity.State entityState)
        {
            StateId = entityState.StateId;
            StateCode = entityState.StateCode;
            StateDesc = entityState.StateDesc;
            StateDescMM = entityState.StateDescMM;
            StateStatus = entityState.StateStatus;
            StateNRCcode_EN = entityState.StateNRCcode_EN;
            StateNRCcode_MM = entityState.StateNRCcode_MM;
        }
    }
}