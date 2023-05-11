using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class RemarkModel
    {
        public long GoalId { get; set; }
        public long RemarkId { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public UserModel User { get; set; }
        public bool IsDeleted { get; set; }
        public void Validate()
        {
            if ( GoalId == 0)
            {
                throw new WebApiException(
                           new WebApiError(WebApiErrorCode.InvalidArguments, "GoalId Cannot be Zero"));
            }
            if (string.IsNullOrEmpty(Remarks))
            {
                throw new WebApiException(
                                        new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorRemarksNull));
            }
            if (!string.IsNullOrEmpty(Remarks) && Remarks.Length > 2000)
            {
                throw new WebApiException(
                                        new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorRemarksNull));
            }
        }
        public RemarkModel()
        {

        }
        public RemarkModel(Entity.Remark entityRemark)
        {
            GoalId = entityRemark.GoalId;
            RemarkId = entityRemark.RemarkId;
            Remarks = entityRemark.Remarks;
            CreatedDate = entityRemark.CreatedDate;
            if (entityRemark.UserProfile != null)
            {
                User = new UserModel(entityRemark.UserProfile);
            }
        }
    }
}