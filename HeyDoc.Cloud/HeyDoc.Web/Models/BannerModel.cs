using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class BannerModel
    {
        public int BannerId { get; set; }
        public string ImageUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsHidden { get; set; }
        public int Sequence { get; set; }
        public string LinkUrl { get; set; }

        public BannerModel()
        {

        }

        public BannerModel(Entity.Banner entityBanner)
        {
            BannerId = entityBanner.BannerId;
            ImageUrl = entityBanner.ImageUrl;
            Width = entityBanner.Width;
            Height = entityBanner.Height;
            IsHidden = entityBanner.IsHidden;
            Sequence = entityBanner.Sequence;
            LinkUrl = entityBanner.LinkUrl;
        }

    }
}