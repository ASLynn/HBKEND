using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class BranchModel
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchAdress { get; set; }
        public string PhoneNumber { get; set; }
        public int? CorporateId { get; set; }
        public DateTime CreatedDate { get; set; }

        public BranchModel()
        {

        }

        public BranchModel(Entity.Branch entityBranch)
        {
            BranchId = entityBranch.BranchId;
            BranchName = entityBranch.BranchName;
            BranchAdress = entityBranch.BranchAddress;
            PhoneNumber = entityBranch.PhoneNumber;
            CorporateId = entityBranch.CorporateId;
            CreatedDate = entityBranch.CreatedDate;
        }
    }
}