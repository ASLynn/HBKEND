using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace HeyDoc.Web.Helpers
{
    public static class HtmlHelpers
    {
        public static IHtmlString Image(this HtmlHelper helper, string src)
        {
            TagBuilder tb = new TagBuilder("img");
            if (string.IsNullOrEmpty(src))
            {
                tb.Attributes.Add("src", VirtualPathUtility.ToAbsolute("/Images/Widget/placeholder.png"));
            }
            else
            {
                tb.Attributes.Add("src", src);
            }

            //tb.AddCssClass("img-thumbnail");

            return new MvcHtmlString(tb.ToString(TagRenderMode.SelfClosing));
        }

        public class MenuLink
        {
            public string LinkText { get; set; }
            public string ActionName { get; set; }
            public string ControllerName { get; set; }
            public object Parements { get; set; }
            public MenuLink() { }
            public MenuLink(string linkText, string actionName, string controllerName)
            {
                LinkText = linkText;
                ActionName = actionName;
                ControllerName = controllerName;
            }
        }

        public static MvcHtmlString MenuLinks(this HtmlHelper htmlHelper, List<MenuLink> Links)
        {
            var tagOl = new TagBuilder("ol");
            tagOl.AddCssClass("breadcrumb pull-left");

            string currentAction = htmlHelper.ViewContext.RouteData.GetRequiredString("action");
            string currentController = htmlHelper.ViewContext.RouteData.GetRequiredString("controller");

            foreach (var Link in Links)
            {
                var tagLi = new TagBuilder("li");
                if (Link.ActionName.Equals(currentAction, StringComparison.InvariantCultureIgnoreCase) && Link.ControllerName.Equals(currentController, StringComparison.InvariantCultureIgnoreCase))
                {
                    tagLi.AddCssClass("active");
                    tagLi.InnerHtml = Link.LinkText;

                }
                else
                {
                    tagLi.InnerHtml = htmlHelper.ActionLink(Link.LinkText, Link.ActionName, Link.ControllerName, Link.Parements, null).ToString();
                }
                tagOl.InnerHtml += tagLi.ToString();
            }
            return MvcHtmlString.Create(tagOl.ToString());
        }


        #region Main Menu
        public class MainMenuLink : MenuLink
        {
            public string icon { get; set; }
            public MainMenuLink() { }
            public MainMenuLink(string linkText, string actionName, string controllerName, string iconStr = null)
                : base(linkText, actionName, controllerName)
            {
                icon = iconStr;
            }
        }
        public static MvcHtmlString MainMenu(this HtmlHelper htmlHelper, RoleType userRole = RoleType.Admin)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var tagUl = new TagBuilder("ul");
                tagUl.AddCssClass("nav main-menu");
                string currentAction = htmlHelper.ViewContext.RouteData.GetRequiredString("action");
                string currentController = htmlHelper.ViewContext.RouteData.GetRequiredString("controller");

                List<MainMenuLink> Links = new List<MainMenuLink>();
                if (userRole == RoleType.SuperAdmin)
                {
                    Links.Add(new MainMenuLink("Banner", "Index", "Banner", "fa-bookmark"));
                    Links.Add(new MainMenuLink("Categories", "Index", "Category", "fa-tags"));
                    Links.Add(new MainMenuLink("Cash out Requests", "Index", "Payment", "fa-money"));
                    Links.Add(new MainMenuLink("Chat Room", "Index", "Chat", "fa-comments"));
                    Links.Add(new MainMenuLink("Corporate Branches", "Index", "Corporate", "fa-building-o"));
                    Links.Add(new MainMenuLink("Corporate User", "Index", "CorporateUser", "fa-suitcase"));
                    Links.Add(new MainMenuLink("Corporate User Bulk", "corporatebulkinsert", "CorporateUser", "fa-suitcase"));
                    Links.Add(new MainMenuLink("Corporate Prescription Request", "RequestList", "PrescriptionRequest", "fa-folder"));
                    Links.Add(new MainMenuLink("Dashboard", "Index", "Home", "fa-dashboard"));
                    Links.Add(new MainMenuLink("Doctors", "Index", "Doctor", "fa-user-md"));
                    Links.Add(new MainMenuLink("Doctors Duty", "Index", "DoctorDuty", "fa-user-md"));
                    Links.Add(new MainMenuLink("EPS Pharmacy Outlets", "Index", "PharmacyOutlets", "fa-plus-square"));
                    Links.Add(new MainMenuLink("EPS Audit Logs", "List", "AuditLog", "fa-tasks"));
                    Links.Add(new MainMenuLink("Facilities", "Index", "Facilities", "fa-plus-circle"));
                    Links.Add(new MainMenuLink("Groups", "Index", "Group", "fa-users"));                 
                    Links.Add(new MainMenuLink("Medication", "Index", "Medication", "fa-medkit"));
                    Links.Add(new MainMenuLink("Management Accounts", "Index", "ManagementAccounts", "fa-user"));
                    Links.Add(new MainMenuLink("On Site Events", "Index", "OnSiteEvent", "fa-medkit"));
                    Links.Add(new MainMenuLink("Prescriptions", "AllPrescriptions", "Prescription", "fa-file-text"));
                    Links.Add(new MainMenuLink("Promo Codes", "Index", "PromoCode", "fa-gift"));   
                    Links.Add(new MainMenuLink("Third Party Administrators", "Index", "ThirdPartyAdministrator", "fa-building-o"));
                    Links.Add(new MainMenuLink("Topup Logs", "Index", "PatientTransaction", "fa-file-text"));
                    Links.Add(new MainMenuLink("Transactions", "Index", "Transaction", "fa-credit-card"));
                    Links.Add(new MainMenuLink("Users", "Index", "User", "fa-wheelchair"));
                    Links.Add(new MainMenuLink("Vaccine Approval", "VaccineApproval", "Vaccine", "fa-hospital-o"));
                }  
                if (userRole == RoleType.Admin)
                {
                    Links.Add(new MainMenuLink("Banner", "Index", "Banner", "fa-bookmark"));
                    Links.Add(new MainMenuLink("Categories", "Index", "Category", "fa-tags"));
                    Links.Add(new MainMenuLink("Cash out Requests", "Index", "Payment", "fa-money"));
                    Links.Add(new MainMenuLink("Chat Room", "Index", "Chat", "fa-comments"));
                    Links.Add(new MainMenuLink("Corporate Branches", "Index", "Corporate", "fa-building-o"));                 
                    Links.Add(new MainMenuLink("Dashboard", "Index", "Home", "fa-dashboard"));
                    Links.Add(new MainMenuLink("Doctors", "Index", "Doctor", "fa-user-md"));
                    Links.Add(new MainMenuLink("Doctors Duty", "Index", "DoctorDuty", "fa-user-md"));
                    Links.Add(new MainMenuLink("EPS Pharmacy Outlets", "Index", "PharmacyOutlets", "fa-plus-square"));                   
                    Links.Add(new MainMenuLink("Facilities", "Index", "Facilities", "fa-plus-circle"));
                    Links.Add(new MainMenuLink("Groups", "Index", "Group", "fa-users"));                  
                    Links.Add(new MainMenuLink("Medication", "Index", "Medication", "fa-medkit"));
                    Links.Add(new MainMenuLink("Management Accounts", "Index", "ManagementAccounts", "fa-user"));
                    Links.Add(new MainMenuLink("On Site Events", "Index", "OnSiteEvent", "fa-medkit"));                  
                    Links.Add(new MainMenuLink("Promo Codes", "Index", "PromoCode", "fa-gift"));
                    Links.Add(new MainMenuLink("Third Party Administrators", "Index", "ThirdPartyAdministrator", "fa-building-o"));                  
                    Links.Add(new MainMenuLink("Users", "Index", "User", "fa-wheelchair"));
                    Links.Add(new MainMenuLink("Vaccine Approval", "VaccineApproval", "Vaccine", "fa-hospital-o"));                   
                }
                if (userRole == RoleType.Doctor)
                {
                    Links.Add(new MainMenuLink("Chat Room", "Index", "Chat", "fa-comments"));
                    Links.Add(new MainMenuLink("Transactions", "Index", "Transaction", "fa-credit-card"));
                }



                foreach (var Link in Links)
                {
                    var tagLi = new TagBuilder("li");
                    if (Link.ControllerName.Equals(currentController, StringComparison.InvariantCultureIgnoreCase)
                        && Link.ActionName.Equals(currentAction, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tagLi.AddCssClass("active");
                    }
                    var tagA = new TagBuilder("a");
                    tagA.Attributes["href"] = "/" + Link.ControllerName + "/" + Link.ActionName;
                    tagA.InnerHtml = "<i class='fa " + Link.icon + "'></i><span class='hidden-xs'>" + Link.LinkText + "</span>";
                    tagLi.InnerHtml += tagA.ToString();
                    //tagLi.InnerHtml = htmlHelper.ActionLink("<i class='fa " + Link.icon + "'></i><span class='hidden-xs'>" + Link.LinkText + "</span>", Link.ActionName, Link.ControllerName, Link.Parements, null).ToString();

                    tagUl.InnerHtml += tagLi.ToString();
                }
                return MvcHtmlString.Create(tagUl.ToString());
            }
        }
        #endregion Main Menu
    }
}