using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class BranchService
    {
        public static BranchModel CreateBranch(Entity.db_HeyDocEntities db, BranchModel model)
        {
            var entityBranch = db.Branchs.Create();
            entityBranch.BranchName = model.BranchName;
            entityBranch.BranchAddress = model.BranchAdress;
            entityBranch.PhoneNumber = model.PhoneNumber;
            entityBranch.CreatedDate = DateTime.UtcNow;
            entityBranch.IsDelete = false;
            entityBranch.CorporateId = model.CorporateId;

            db.Branchs.Add(entityBranch);

            db.SaveChanges();
            return new BranchModel(entityBranch);
        }

        public static BranchModel UpdateBranch(Entity.db_HeyDocEntities db, BranchModel model)
        {
            var entityBranch = db.Branchs.FirstOrDefault(e => !e.IsDelete && e.BranchId == model.BranchId);
            if (entityBranch == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateBranchNotFound));
            }
            entityBranch.BranchName = model.BranchName;
            entityBranch.BranchAddress = model.BranchAdress;
            entityBranch.PhoneNumber = model.PhoneNumber;
            //entityBranch.CorporateId = model.CorporateId;

            db.SaveChanges();
            return new BranchModel(entityBranch);
        }

        public static BranchModel GetBranchById(int branchId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityBranch = db.Branchs.FirstOrDefault(e => !e.IsDelete && e.BranchId == branchId);
                if (entityBranch == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateBranchNotFound));
                }
                return new BranchModel(entityBranch);
            }
        }

        private static List<BranchModel> GetBranchList(Entity.db_HeyDocEntities db, int corporateId, int skip, int take)
        {
            if (take > 2000)
            {
                take = 2000;
            }
            List<BranchModel> modelList = new List<BranchModel>();
            var entityBranchList = db.Branchs.Where(e => !e.IsDelete && e.CorporateId == corporateId).OrderBy(e => e.BranchName).Skip(skip);
            if (take != -1)
            {
                entityBranchList = entityBranchList.Take(take);
            }
            foreach (var branch in entityBranchList)
            {
                modelList.Add(new BranchModel(branch));
            }

            return modelList;
        }

        public static List<BranchModel> GetBranchListMobile(int corporateId, int skip, int take)
        {
            List<BranchModel> modelList;
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityCorporate = db.Corporates.FirstOrDefault(e => !e.IsDelete && e.CorporateId == corporateId);
                if (entityCorporate == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateNotFound));
                }
                modelList = GetBranchList(db, corporateId, skip, take);
            }
            return modelList;
        }

        public static List<BranchModel> GetAllBranches(Entity.db_HeyDocEntities db)
        {
            List<BranchModel> modelList = new List<BranchModel>();

            var entityBranchList = db.Branchs.Where(e => !e.IsDelete).OrderBy(e => e.BranchName);
            foreach (var branch in entityBranchList)
            {
                modelList.Add(new BranchModel(branch));
            }

            return modelList;
        }

    }
}