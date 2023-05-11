using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PromoCodeModel
    {
        public long PromoCodeId { get; set; }
        public string PromoCode { get; set; }
        public string Description { get; set; }
        public decimal Discount { get; set; }
        public string PartnerName { get; set; }
        public PromoDiscountType DiscountType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UserUsageLimit { get; set; }
        public int? MaxUserLimit { get; set; }
        public PromoStatus PromoStatus { get; set; }

        public decimal? TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? AmountToBePaid { get; set; }

        public DoctorModel Doctor { get; set; }
        public CategoryModel Category { get; set; }

        public int DoctorId { get; set; }
        public int CategoryId { get; set; }

        public PromoCodeModel()
        {

        }

        public PromoCodeModel(Entity.PromoCode entityPromoCode, decimal? totalAmount = null, decimal? discountAmount = null, decimal? amountToBePaid = null, bool isDetailed=false)
        {
            PromoCodeId = entityPromoCode.PromoCodeId;
            PromoCode = entityPromoCode.PromoCode1;
            Description = entityPromoCode.Description;
            Discount = entityPromoCode.Discount;
            PartnerName = entityPromoCode.PartnerName;
            DiscountType = entityPromoCode.DiscountType;
            StartDate = entityPromoCode.StartDate;
            EndDate = entityPromoCode.EndDate;
            UserUsageLimit = entityPromoCode.UserUsageLimit;
            MaxUserLimit = entityPromoCode.MaxUserLimit;
            PromoStatus = entityPromoCode.PromoStatus;

            TotalAmount = totalAmount;
            DiscountAmount = discountAmount;
            AmountToBePaid = amountToBePaid;
            DoctorId = entityPromoCode.DoctorId.HasValue ? entityPromoCode.DoctorId.Value : 0;
            CategoryId = entityPromoCode.CategoryId.HasValue ? entityPromoCode.CategoryId.Value : 0;
            if (isDetailed)
            {
                if (entityPromoCode.Doctor != null)
                {
                    Doctor = new DoctorModel(entityPromoCode.Doctor);
                }
                if (entityPromoCode.Category != null)
                {
                    Category = new CategoryModel(entityPromoCode.Category);
                }
            }
        }

        public void Validate(bool isBulkGenerate,int codeCount)
        {
            if (!isBulkGenerate && string.IsNullOrEmpty(PromoCode))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Promo.ErrorPromoNull));
            }
            if (isBulkGenerate && codeCount <= 0)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Promo.ErrorCountNegative));
            }
            if (Discount <= 0)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Promo.ErrorDiscountNegative));
            }
            if (string.IsNullOrEmpty(PartnerName))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Partner name should be valid"));
            }
            if (string.IsNullOrEmpty(Description))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDescNull));
            }
        }
    }
}