using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class GroupModel
    {
        public string GroupName { get; set; }
        public long GroupId { get; set; }
        public int CategoryId { get; set; }
        public decimal PlatformCut { get; set; }
        public CategoryModel Category { get; set; }
        public PhotoModel Photo { get; set; }

        public GroupModel()
        {

        }

        public GroupModel(Entity.Group entityGroup, bool isdetailed = false, DoctorGroupUserTypeCategories userTypeCategory = DoctorGroupUserTypeCategories.Doc2Us)
        {
            GroupId = entityGroup.GroupId;
            switch (userTypeCategory)
            {
                case DoctorGroupUserTypeCategories.UserType1:
                case DoctorGroupUserTypeCategories.UserType2:
                    GroupName = entityGroup.GenericGroupName;
                    if (entityGroup.TpPhoto != null)
                    {
                        Photo = new PhotoModel(entityGroup.TpPhoto);
                    }
                    break;
                case DoctorGroupUserTypeCategories.Doc2Us:
                default:
                    GroupName = entityGroup.GroupName;
                    if (entityGroup.Photo != null)
                    {
                        Photo = new PhotoModel(entityGroup.Photo);
                    }
                    break;
            }
            CategoryId = entityGroup.CategoryId;
            if (isdetailed)
            {
                PlatformCut = entityGroup.PlatformCut;
                Category = new CategoryModel(entityGroup.Category);
            }
        }
    }

    public class GroupEditModel
    {
        public string GroupName { get; set; }
        public string GenericGroupName { get; set; }
        public long GroupId { get; set; }
        public int CategoryId { get; set; }
        public decimal PlatformCut { get; set; }
        public CategoryModel Category { get; set; }
        public PhotoModel Photo { get; set; }
        public PhotoModel TpPhoto { get; set; }

        public GroupEditModel()
        {
        }

        public GroupEditModel(Entity.Group entityGroup)
        {
            GroupId = entityGroup.GroupId;
            GroupName = entityGroup.GroupName;
            GenericGroupName = entityGroup.GenericGroupName;
            CategoryId = entityGroup.CategoryId;
            PlatformCut = entityGroup.PlatformCut;
            Category = new CategoryModel(entityGroup.Category);
            if (entityGroup.Photo != null)
            {
                Photo = new PhotoModel(entityGroup.Photo);
            }
            if (entityGroup.TpPhoto != null)
            {
                TpPhoto = new PhotoModel(entityGroup.TpPhoto);
            }
        }
    }
}