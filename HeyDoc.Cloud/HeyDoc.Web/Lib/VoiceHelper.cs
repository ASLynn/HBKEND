using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using log4net;
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

namespace HeyDoc.Web.Lib
{
    public class VoiceHelper
    {
        private static readonly ILog logger =
          LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<Entity.Voice> ReadFromStreamAndUpload(Entity.db_HeyDocEntities db, HttpContent content, string containerName)
        {
            // Read the form data and return an async task.
            var multipartResult = await content.ReadAsMultipartAsync();

            foreach (var data in multipartResult.Contents)
            {
                string filename = Guid.NewGuid().ToString();
                string fileWithExt = data.Headers.ContentDisposition.FileName.Replace("\"", "");
                string extension = Path.GetExtension(fileWithExt);

                string filenameWithExtension = filename + extension;

                if (!string.IsNullOrEmpty(filename))
                {
                    var readStream = await data.ReadAsStreamAsync();
                    Stream fileStream = readStream;

                    return UploadFile(db, containerName, filenameWithExtension, fileStream);
                }
            }
            throw new WebApiException(
                   new WebApiError(WebApiErrorCode.InvalidArguments, Errors.ErrorNoFileUploaded));
        }

        internal static Entity.Voice UploadFile(Entity.db_HeyDocEntities db, string containerName, string filenameWithExtension, Stream fileStream)
        {
            string originalBlobUrl = string.Empty;

            try
            {
                originalBlobUrl = CloudBlob.UploadFile(containerName, filenameWithExtension, fileStream);

                var entityFile = db.Voices.Create();
                entityFile.VoiceUrl = originalBlobUrl;
                entityFile.CreateDate = DateTime.UtcNow;

                db.Voices.Add(entityFile);
                db.SaveChanges();

                return entityFile;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                if (!string.IsNullOrEmpty(originalBlobUrl))
                {
                    CloudBlob.DeleteImage(originalBlobUrl);
                }

                throw new WebApiException(new WebApiError(ex));
            }
            finally
            {

            }
        }
    }
}