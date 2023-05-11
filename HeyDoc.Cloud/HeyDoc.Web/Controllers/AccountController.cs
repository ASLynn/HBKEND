using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using HeyDoc.Web.Filters;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.Helpers;
using static HeyDoc.Web.Helpers.SMSHelper;
namespace HeyDoc.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        //RedirectToDefault
        public RedirectToRouteResult RedirectToDefault(Entity.UserProfile entityUser)
        {
            switch (entityUser.Role)
            {
                case RoleType.Doctor:
                    //Krish : Quick fix. Later add a flag to category table and integrate with admin portal-- 23/11/2017
                    //if (entityUser.Doctor.Category.CategoryName == "Family Doctors" || entityUser.Doctor.Category.CategoryName == "Specialists" || entityUser.Doctor.Category.CategoryName == "Others")
                    //{
                    //    return RedirectToAction("Home", "DutyDoctor");
                    //}
                    return RedirectToAction("Index", "Chat");
                case RoleType.Admin: return RedirectToAction("Index", "Home");
                case RoleType.SuperAdmin: return RedirectToAction("Index", "Home");
                case RoleType.PharmacyManagement: return RedirectToAction("Index", "PharmacyOutlets");
                case RoleType.Pharmacy: return RedirectToAction("Index", "Home");
                default:
                    return null;
            }
        }
        //Redirect to PWA For True Money
        public RedirectResult RedirectToPWATM(string PhoneNum)
        {
            return Redirect("https://www.hope.com.mm");
        }
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (User.Identity.IsAuthenticated)
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == User.Identity.Name);
                    var redirect = RedirectToDefault(entityUser);
                    if (redirect != null) 
                    { 
                        return redirect;
                    }
                }
            }

            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            
            if (ModelState.IsValid)
            {
                bool hasError = false;
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == model.UserName && !e.IsDelete);
                    if (entityUser == null)
                    {
                        entityUser = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == model.UserName && !e.IsDelete);
                    }
                    if (entityUser == null)
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                        hasError = true;
                        return View(model);
                    }
                    
                    if (entityUser.IsBan)
                    {
                        ModelState.AddModelError("", "You are banned from using this.");
                        hasError = true;
                    }

                    if (entityUser.Role == RoleType.Doctor && !entityUser.Doctor.IsVerified)
                    {
                        ModelState.AddModelError("", "You are not verified yet.");
                        hasError = true;
                    }

                    if (entityUser.Role == RoleType.User)
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                        hasError = true;
                        return View(model);
                    }


                    if (!hasError)
                    {
                        try
                        {
                            var isSuccess = WebSecurity.Login(entityUser.UserName, model.Password, persistCookie: model.RememberMe);
                            if (isSuccess)
                            {
                                if (returnUrl != null)
                                {
                                    return RedirectToLocal(returnUrl);
                                }
                                else 
                                {
                                    var redirect = RedirectToDefault(entityUser);
                                    if (redirect != null)
                                    {
                                        return redirect;
                                    }
                                }
                                return RedirectToAction("Index", "Home");
                            }
                            ModelState.AddModelError("", "The user name or password provided is incorrect.");
                        }
                        catch
                        {
                            ModelState.AddModelError("", "The user name or password provided is incorrect.");
                        }
                    }

                }
            }
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Login", "Account");
        }


        public ActionResult Setting()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Setting(LocalPasswordModel model)
        {
            // ChangePassword will throw an exception rather than return false in certain failure scenarios.
            bool changePasswordSucceeded;
            (var pass, var errorMessage) = PasswordModel.CheckPassword(model.NewPassword);
            if (!pass)
            {
                ModelState.AddModelError("NewPassword", errorMessage);
            }
            if (!ModelState.IsValid)
            {
                changePasswordSucceeded = false;
                return View(model);
            }
            
            try
            {
                changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                if (!changePasswordSucceeded)
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    return View(model);
                }
                return RedirectToAction("Index", "User");
            }
            catch (Exception)
            {
                changePasswordSucceeded = false;
                ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                return View(model);
            }
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            (var pass, var errorMessage) = PasswordModel.CheckPassword(model.NewPassword);
            if (!pass)
            {
                ModelState.AddModelError("NewPassword", errorMessage);
            }
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        [AllowAnonymous]
        public ActionResult Confirmation(string id)
        {
            if (WebSecurity.ConfirmAccount(id))
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityUser = db.UserProfiles.FirstOrDefault(e => e.webpages_Membership.ConfirmationToken == id);

                    var entityInvitation = db.Invitations.FirstOrDefault(e => e.InvitedUserId == entityUser.UserId);
                    if (entityInvitation != null)
                    {
                        entityInvitation.IsRegistered = true;
                        entityInvitation.UserProfile1.Patient.InvitedCount++;
                        db.SaveChanges();
                    }
                }
                ViewBag.msg = "Email address confirmed!";
            }
            else
            {
                ViewBag.msg = "Email address confirmation failed!";
            }
            return View("ConfirmationDeepLink");
        }

        //[AllowAnonymous]
        //public ActionResult ConfirmationSuccess()
        //{
        //    return View();
        //}

        //[AllowAnonymous]
        //public ActionResult ConfirmationFailure()
        //{
        //    return View();
        //}

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Reset(string id)
        {
            ResetPasswordModel model = new ResetPasswordModel();
            model.ResetToken = id;

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Reset(ResetPasswordModel model)
        {
            (var pass, var errorMessage) = PasswordModel.CheckPassword(model.NewPassword);
            if (!pass)
            {
                ModelState.AddModelError("NewPassword", errorMessage);
            }
            if (ModelState.IsValid)
            {
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "New password is mismatch with confirm password.");
                }
                else
                {
                    var userId = WebSecurity.GetUserIdFromPasswordResetToken(model.ResetToken);
                    if (WebSecurity.ResetPassword(model.ResetToken, model.NewPassword))
                    {
                        using (var db = new Entity.db_HeyDocEntities())
                        {
                            var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserId == userId);
                            if (entityUser != null)
                            {
                                //AccountService.NotifyPasswordChanged(entityUser.UserName);
                            }
                        }
                        return RedirectToAction("ChangePasswordSuccess");
                    }
                    else
                    {
                        ModelState.AddModelError("", "The link has expired. Please request for a new link by clicking \"Forget Password\" on the HOPE app.");
                    }
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ChangePasswordSuccess()
        {
            // begin::Check request URL
            string current_url = Request.Url.ToString();
            string url_text = "";
            if (current_url.Contains("localhost"))
            {
                url_text = "localhost";
            }
            else if (current_url.Contains("web.hope.com.mm"))
            {
                url_text = "web.hope.com.mm";
            }
            else
            {
                url_text = "hopestaging.azurewebsites.net";
            }

            // end::Check request URL
            ViewBag.url = url_text;
            return View();
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public JsonResult View(int userId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserId == userId);
                return Json(new UserModel(entityUser), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Otpverify(OTPverifyModel model)
        {
            var companyList = new List<SelectListItem> { new SelectListItem() };
            using (Entity.db_HeyDocEntities dB = new Entity.db_HeyDocEntities())
            {
                var companies = dB.CompanyWhiteLabels
                                 .OrderBy(y => y.CompanyId)
                                 .Select(y => new SelectListItem() { Text = y.CompanyName, Value = y.CompanyId.ToString() })
                                 .ToList();

                companyList.AddRange(companies);
                ViewBag.companies = companies; // Supply a selection list without the all option for statistics export UI
            }
            var request = System.Web.HttpContext.Current.Request;
            if (model.PhoneNumber == null)
            {
             
                ViewBag.message = "Please enter register phone number and OTP Code";
                return View(model);
            }
            else
            {
                Entity.UserProfile userProfile;
                Entity.UserExtra userExtra;
                int cid = model.CompanyId;
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    userProfile = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == model.PhoneNumber && !e.IsDelete && e.CompanyId == cid);
                    userExtra = db.UserExtras.FirstOrDefault(e => e.UserId == userProfile.UserId && e.OTP == model.OTPCode);
                    string url = Request.Url.AbsoluteUri;

                    if (url.Contains("web.hope.com.mm") || url.Contains("telemed"))
                    {
                        ViewBag.CompanyId = "https://web.hope.com.mm/login/" + userProfile.CompanyId + "/" ?? "https://web.hope.com.mm/login/1/";
                    }
                    else if (url.Contains("localhost"))
                    {
                        ViewBag.CompanyId = "http://localhost:4200/login/" + userProfile.CompanyId + "/" ?? "http://localhost:4200/login/1/";
                    }
                    else
                    {
                        ViewBag.CompanyId = "https://hopewebstaging.azurewebsites.net/login/" + userProfile.CompanyId + "/" ?? "https://hopewebstaging.azurewebsites.net/login/1/";
                    }
                   
                    if (userExtra != null)
                    {
                        userExtra.OTPVerified = 1;
                        userExtra.Status = 0;
                        db.SaveChanges();
                        ViewBag.message = "OTP verification successful. Please click here to ";
                        return View(model);
                    }
                    else
                    {
                        ViewBag.message = "OTP verification failed. Wrong phone number or OTP code. Please try again or call 09456880345 for assist";
                        return View();
                    }

                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Otpresend(OTPverifyModel model)
        {
            var request = System.Web.HttpContext.Current.Request;
            if (model.PhoneNumber == null)
            {
                ViewBag.message = "Please enter register phone number to receive OTP";
                return View(model);
            }
            else
            {
                Entity.UserProfile userProfile;
                Entity.UserExtra userExtra;
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    userProfile = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == model.PhoneNumber && !e.IsDelete);
                    if(userProfile != null)
                    {
                        userExtra = db.UserExtras.FirstOrDefault(e => e.UserId == userProfile.UserId && e.OTPVerified == 0);
                        if (userExtra != null)
                        {


                            Random r = new Random();
                            int rInt = r.Next(100000, 999999);
                            userExtra.OTP = rInt.ToString();                          
                            userExtra.OTPCreateDT = DateTime.UtcNow;
                            userExtra.OTPVerified = 0;
                            userExtra.Status = 1;

                            SMSResModel smsResponseModel = new SMSResModel();

                            SMSReqModel sMSReqModel = new SMSReqModel();
                            sMSReqModel.text = "" + rInt + " HOPE OTP Code: Reset Please click below link for verification https://app.hope.com.mm/account/otpverify/";
                            sMSReqModel.to = model.PhoneNumber;
                            sMSReqModel.from = "HOPE";
                            smsResponseModel = SMSSend(sMSReqModel);
                            db.SaveChanges();

                            ViewBag.message = "OTP resend successful. Please click here to ";
                            return View(model);
                        }
                        else
                        {
                            ViewBag.message = "OTP resend failed. Wrong phone number. Please call 09970928688 for technical assist";
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.message = "Unable to send OTP!. this phone number is not registered. Please call 09970928688 for technical assist";
                        return View();
                    }

                }
            }
        }
    }
}
