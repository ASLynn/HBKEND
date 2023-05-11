using log4net;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Resources;

namespace HeyDoc.Web.Lib
{
    public class PhotoHelper
    {
        private static readonly ILog logger =
          LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<Entity.Photo> ReadFromStreamAndUpload(Entity.db_HeyDocEntities db, HttpContent content, string containerName)
        {
            // Read the form data and return an async task.
            var multipartResult = await content.ReadAsMultipartAsync();

            foreach (var data in multipartResult.Contents)
            {
                if (!string.IsNullOrEmpty(data.Headers.ContentDisposition.FileName))
                {
                    var readStream = await data.ReadAsStreamAsync();
                    Stream photoStream = readStream;

                    return UploadImageWithThumbnail(db, containerName, Guid.NewGuid().ToString(), photoStream);
                }
            }
            throw new WebApiException(
                   new WebApiError(WebApiErrorCode.InvalidArguments, Errors.ImageNotUploaded));
        }

        internal static Entity.Photo UploadImageWithThumbnail(Entity.db_HeyDocEntities db, string containerName, string filename, Stream photoStream)
        {
            Image originalImage = null;
            Image thumbnailImage = null;

            string originalBlobUrl = string.Empty;
            string thumbnailBlobUrl = string.Empty;

            string thumbnailName = filename + "_tb";

            try
            {
                originalImage = Image.FromStream(photoStream);
                originalBlobUrl = CloudBlob.UploadImage(containerName, filename, ImageFormat.Jpeg, originalImage);

                thumbnailImage = GetThumbnail(originalImage, 500);
                thumbnailBlobUrl = CloudBlob.UploadImage(containerName, thumbnailName, ImageFormat.Jpeg, thumbnailImage);

                var entityPhoto = db.Photos.Create();
                entityPhoto.ImageUrl = originalBlobUrl;
                entityPhoto.CreateDate = DateTime.UtcNow;

                db.Photos.Add(entityPhoto);
                db.SaveChanges();

                return entityPhoto;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                if (!string.IsNullOrEmpty(originalBlobUrl))
                {
                    CloudBlob.DeleteImage(originalBlobUrl);
                }
                if (!string.IsNullOrEmpty(thumbnailBlobUrl))
                {
                    CloudBlob.DeleteImage(thumbnailBlobUrl);
                }

                throw new WebApiException(new WebApiError(ex));
            }
            finally
            {
                if (originalImage != null)
                {
                    originalImage.Dispose();
                    originalImage = null;
                }
                if (thumbnailImage != null)
                {
                    thumbnailImage.Dispose();
                    thumbnailImage = null;
                }
            }
        }

        internal static BoolResult DeletePhoto(Entity.db_HeyDocEntities db, long photoId)
        {
            var entityPhoto = db.Photos.FirstOrDefault(e => e.PhotoId == photoId);
            if (entityPhoto != null)
            {
                return DeletePhoto(db, entityPhoto);
            }
            else
            {
                return new BoolResult(false);
            }
        }

        internal static BoolResult DeletePhoto(Entity.db_HeyDocEntities db, Entity.Photo entityPhoto)
        {
            DeleteEntityPhoto(db, entityPhoto);
            return new BoolResult(true);
        }

        private static Entity.Photo GetEntityPhoto(Entity.db_HeyDocEntities db, string photoId)
        {
            if (string.IsNullOrEmpty(photoId))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorPhotoIDNull));
            }
            long photoId_l = 0;
            if (!long.TryParse(photoId, out photoId_l))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Invalid photo ID : " + photoId));
            }

            var entityPhoto = db.Photos.FirstOrDefault(e => e.PhotoId == photoId_l);
            if (entityPhoto == null)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, "Photo not found"));
            }

            return entityPhoto;
        }

        private static BoolResult DeleteEntityPhoto(Entity.db_HeyDocEntities db, Entity.Photo entityPhoto)
        {
            CloudBlob.DeleteImage(entityPhoto.ImageUrl);
            CloudBlob.DeleteImage(GetThumbnailUrl(entityPhoto.ImageUrl));

            db.Photos.Remove(entityPhoto);
            db.SaveChanges();

            return new BoolResult(true);
        }

        public static string GetThumbnailUrl(string originalImageUrl)
        {
            if (string.IsNullOrEmpty(originalImageUrl))
            {
                return originalImageUrl;
            }
            try
            {
                string imageFileName = Path.GetFileName(originalImageUrl);
                string imageFileExtension = Path.GetExtension(imageFileName);
                string imageFileNameWithoutExtension = Path.GetFileNameWithoutExtension(imageFileName);

                string thumbnailFileNameWithExtension = imageFileNameWithoutExtension + "_tb" + imageFileExtension;

                return originalImageUrl.Replace(imageFileName, thumbnailFileNameWithExtension);
            }
            catch
            {
                return originalImageUrl;
            }
        }

        public static Image GetThumbnail(Image imgToResize, int minSize)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)minSize / (float)sourceWidth);
            nPercentH = ((float)minSize / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (Image)b;
        }
    }
}