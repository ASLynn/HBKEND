using HeyDoc.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class BannerService
    {
        public static List<BannerModel> GetList(int take = 10, int skip = 0)
        {
            if (take > 20)
            {
                take = 20;
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityBannerList = db.Banners.Where(e => !e.IsHidden).OrderBy(e => e.Sequence).Skip(skip).Take(take);

                List<BannerModel> result = new List<BannerModel>();
                foreach (var entityBanner in entityBannerList)
                {
                    result.Add(new BannerModel(entityBanner));
                }

                return result;
            }
        }
    }
}