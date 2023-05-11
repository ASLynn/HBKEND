using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class CategoryModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal CategoryPrice { get; set; }
        public decimal MidnightPrice { get; set; }
        public string ImageUrl { get; set; }
        public byte Sequence { get; set; }
        public bool IsFreeChat { get; set; }
        public bool IsQueueMedPublicUser { get; set; }
        public bool IsQueueMedCorporateUser { get; set; }
        public bool IsHiddenFromPublic { get; set; }

        public CategoryModel()
        {

        }

        public CategoryModel(Entity.Category entityCategory)
        {
            CategoryId = entityCategory.CategoryId;
            CategoryName = entityCategory.CategoryName;
            var malaysianTime = DateTime.UtcNow.AddHours(8);
            if (malaysianTime.Hour >= 0 && malaysianTime.Hour < 8)
            {
                CategoryPrice = entityCategory.MidNightPrice;
            }
            else
            {
                CategoryPrice = entityCategory.CategoryPrice;
            }
            MidnightPrice = entityCategory.MidNightPrice;
            ImageUrl = entityCategory.ImageUrl;
            Sequence = entityCategory.Sequence;
            IsFreeChat = entityCategory.IsFreeChat;
            IsQueueMedPublicUser = entityCategory.IsQueueMedPublicUser;
            IsQueueMedCorporateUser = entityCategory.IsQueueMedCorporateUser;
            IsHiddenFromPublic = entityCategory.IsHiddenFromPublic;
        }
    }
     
}