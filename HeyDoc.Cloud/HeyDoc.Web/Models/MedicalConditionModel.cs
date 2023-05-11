using HeyDoc.Web.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class MedicalConditionModel
    {
        public int ConditionId { get; set; }
        public string ConditionName { get; set; }

        public MedicalConditionModel(MedicalCondition entityMedicalCondition)
        {
            ConditionId = entityMedicalCondition.Id;
            ConditionName = entityMedicalCondition.Name;
        }
    }
}