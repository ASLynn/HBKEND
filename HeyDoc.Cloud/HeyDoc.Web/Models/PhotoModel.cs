using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PhotoModel
    {
        public long PhotoId { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }

        public HttpPostedFileBase PhotoUrl { get; set; }

        public PhotoModel()
        {

        }

        public PhotoModel(string imageUrl, string thumbnailUrl)
        {
            ImageUrl = imageUrl;
            ThumbnailUrl = thumbnailUrl;
        }

        public PhotoModel(Entity.Photo entityPhoto, string thumbnailUrl = null)
        {
            if (entityPhoto != null)
            {
                PhotoId = entityPhoto.PhotoId;
                ImageUrl = entityPhoto.ImageUrl;
                if (string.IsNullOrEmpty(thumbnailUrl))
                {
                    ThumbnailUrl = PhotoHelper.GetThumbnailUrl(entityPhoto.ImageUrl);
                }
                else
                {
                    ThumbnailUrl = thumbnailUrl;
                }
            }
            else
            {
                ThumbnailUrl = null;
            }
        }
    }
}