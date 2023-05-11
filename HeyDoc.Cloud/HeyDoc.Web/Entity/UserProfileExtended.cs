using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Entity
{
    public partial class UserProfile
    {
        // Note: If multi-role support is ever to be implemented, references to UserProfile.webpages_Roles have to be checked
        // in addition to this Role property as some places check against webpages_Roles.FirstOrDefault instead of the whole collection
        [Obsolete("Use Roles instead to support multiple user roles")]
        public RoleType Role
        {
            get
            {
                RoleType role = RoleType.User;
                if (this.webpages_Roles.Count > 0)
                {
                    Enum.TryParse(this.webpages_Roles.FirstOrDefault().RoleName, out role);
                }
                return role;
            }
        }       
        public List<RoleType> Roles
        {
            get
            {
                if (this.webpages_Roles.Count > 0)
                {
                    var roles = this.webpages_Roles.Select(r => (RoleType)Enum.Parse(typeof(RoleType), r.RoleName));
                    return roles.ToList();
                }
                else
                {
                    return new List<RoleType> { RoleType.User };
                }
            }
        }
    }
}