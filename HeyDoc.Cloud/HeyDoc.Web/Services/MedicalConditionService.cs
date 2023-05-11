using HeyDoc.Web.Entity;
using HeyDoc.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class MedicalConditionService
    {
        public static List<MedicalConditionModel> GetList(int skip = 0, int take = -1)
        {
            using (var db = new db_HeyDocEntities())
            {
                var entityMedicalConditions = db.MedicalConditions.OrderBy(e => e.Name).Skip(skip);

                if (take > -1)
                {
                    entityMedicalConditions = entityMedicalConditions.Take(take);
                }

                var res = entityMedicalConditions.ToList().Select(e => new MedicalConditionModel(e)).ToList();
                return res;
            }
        }
    }
}