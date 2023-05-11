using HeyDoc.Web.Entity;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Services
{
    public class RelationshipService
    {

        public static List<RelationshipModel> GetRelationshipList()
        {
            List<RelationshipModel> relationshipList = new List<RelationshipModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tmpRes = from e in db.Relationships
                             orderby e.RelationshipDesc
                             select e;

                foreach (var a in tmpRes)
                {

                    relationshipList.Add(new RelationshipModel(a));
                }
            }
            return relationshipList;
        }
        public static List<SelectListItem> GetRelationshipAll()
        {
            List<SelectListItem> RelationshipsList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                RelationshipsList = db.Relationships.Where (a=>a.Status == 1).OrderBy(e => e.RelationshipDesc).Select(e => new SelectListItem()
                              {
                                  Text = e.RelationshipDesc,
                                  Value = e.RelationshipId.ToString()
                              }).ToList();

            }
            return RelationshipsList;

        }
        public static List<SelectListItem> GetRelationshipsById(int RelationshipId)
        {
            List<SelectListItem> RelationshipsList = new List<SelectListItem>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                RelationshipsList = db.Relationships.Where(a => (a.Status == 1)&&(a.RelationshipId == RelationshipId)).OrderBy(e => e.RelationshipDesc).Select(e => new SelectListItem()
                {
                    Text = e.RelationshipDesc,
                    Value = e.RelationshipId.ToString()
                }).ToList();

            }
            return RelationshipsList;

        }
    }


}