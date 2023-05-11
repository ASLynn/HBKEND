using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class DutyService
    {
        public static DutyModel UploadFile(HttpContent content)
        {
            // Check if the request contains multipart/form-data.
            if (!content.IsMimeMultipartContent())
            {
                throw new WebApiException(
                       new WebApiError(WebApiErrorCode.UnsupportedMediaType, Errors.ErrorNotMultipart));
            }

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                string containerName = Guid.NewGuid().ToString();

                var entityFile = DutyFileHelper.ReadFromStreamAndUpload(db, content, containerName).Result;

                return new DutyModel(entityFile);
            }
        }

        public static DutyModel DownloadFile(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false, RoleType.Doctor);
                var entityFile = db.DutyRosters.FirstOrDefault();

                if (entityFile == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Errors.ErrorFileNotFound));
                }

                return new DutyModel(entityFile);
            }
        }
    }
}