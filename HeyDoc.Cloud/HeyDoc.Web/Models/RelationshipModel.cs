using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class RelationshipModel

    {
        public int RelationshipId { get; set; }
        public string RelationshipDesc { get; set; }
        public int Status { get; set; }
      

        public RelationshipModel()
        {

        }

        public RelationshipModel(Entity.Relationship entityRelationship)
        {
            RelationshipId = entityRelationship.RelationshipId;
            RelationshipDesc = entityRelationship.RelationshipDesc;
            Status = entityRelationship.Status;

        }




    }

   
}