using HeyDoc.Web.Helpers;
using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Security;
using System.Data.Entity;
using HeyDoc.Web.Resources;
using RestSharp;
using WebMatrix.WebData;
using Newtonsoft.Json;
using System.Net;
using HeyDoc.Web.Entity;
using Microsoft.Azure;
using Newtonsoft.Json.Linq;
using System.Data.Entity.Validation;
using static HeyDoc.Web.Helpers.SMSHelper;
using System.Web.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Antlr.Runtime;

namespace HeyDoc.Web.Services
{
    public class AccountService
    {
        public const string DateTimeISO8601 = "yyyy-MM-ddTHH:mm:ssZ";

        private static readonly log4net.ILog log =
           log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //public static UserModel RegisterEmail(RegisterModel model)
        //{
        //    ActivityAuditHelper.AddRequestDataToLog();
        //    string username;      
        //    try
        //    {
        //        model.Validate();
        //        username = model.Email.Trim();

        //        //if (!EmailHelper.IsValidEmail(username))
        //        //{
        //        //    throw new WebApiException(
        //        //        new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorEmailInvalidFormat));
        //        //}

        //        if (username.Length >= 450)
        //        {
        //            throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorEmailTooLong));
        //        }
        //    }
        //    catch (WebApiException ex)
        //    {
        //        ActivityAuditHelper.LogEvent($"Registration attempted with invalid data (Error: {ex.ErrorDetails.ErrorMessage})", ActivityAuditEvent.RegisterFail);
        //        throw;
        //    }

        //    using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())// has to separate to multiple DB connections due to WebSecurity will close DB connection
        //    {
        //        var entityUser = GetEntityTargetUserByUsername(db, username, true);
        //        if (entityUser != null)
        //        {
        //            ActivityAuditHelper.LogEvent($"Registration attempted for existing username ({username})", ActivityAuditEvent.RegisterFail);
        //            throw new WebApiException(
        //                new WebApiError(WebApiErrorCode.InvalidArguments, ErrorCodeToString(MembershipCreateStatus.DuplicateUserName)));
        //        }
        //        if (model.ReferralCode.HasValue && !db.UserReferralCodes.Any(e => e.Id == model.ReferralCode.Value))
        //        {
        //            ActivityAuditHelper.LogEvent($"Registration attempted with invalid referral code ({model.ReferralCode.Value})", ActivityAuditEvent.RegisterFail);
        //            throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorReferralCodeInvalid));
        //        }

        //        var optionalFields = SignUpOptionalFields.None;
        //        if (model.ReferralCode.HasValue)
        //        {
        //            optionalFields = db.UserReferralCodes.FirstOrDefault(e => e.Id == model.ReferralCode.Value)?.SignUpOptionalFields ?? SignUpOptionalFields.None;
        //        }
        //        if (optionalFields.HasFlag(SignUpOptionalFields.Gender) && (!model.Gender.HasValue || !Enum.IsDefined(typeof(Gender), model.Gender)))
        //        {
        //            model.Gender = null;
        //        }

        //        if (model.CorporateId.HasValue && model.CorporateId != 0)
        //        {
        //            try
        //            {
        //                model.CorporateValidate(optionalFields);

        //                var entityCorporates = db.Corporates.FirstOrDefault(e => e.CorporateId == model.CorporateId && !e.IsDelete);
        //                if (entityCorporates == null)
        //                {
        //                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorCorporateNotFound));
        //                }

        //                var entityBranch = db.Branchs.FirstOrDefault(e => e.BranchId == model.BranchId && !e.IsDelete);
        //                if (entityBranch == null)
        //                {
        //                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorCorporateBranchNotFound));
        //                }

        //                if ((model.IsDependent.HasValue && model.IsDependent.Value) || model.CorporateUserType == CorporateUserType.EmployeeDependants || model.CorporateUserType == CorporateUserType.EmployeeChild)
        //                {
        //                    if (string.IsNullOrWhiteSpace(model.EmployeeDependantName))
        //                    {
        //                        throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDependantNameNull));
        //                    }
        //                    if (string.IsNullOrWhiteSpace(model.EmployeeDependantIC))
        //                    {
        //                        throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDependantICNull));
        //                    }
        //                }
        //            }
        //            catch (WebApiException ex)
        //            {
        //                ActivityAuditHelper.LogEvent($"Registration attempted with invalid corporate data (Error: {ex.ErrorDetails.ErrorMessage})", ActivityAuditEvent.RegisterFail);
        //                throw;
        //            }
        //        }
        //        else
        //        {
        //            if (model.Birthday.HasValue && DateTime.UtcNow.AddYears(-18) < model.Birthday.Value)
        //            {
        //                ActivityAuditHelper.LogEvent("Registration attempted with age below restriction", ActivityAuditEvent.RegisterFail);
        //                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorAgeRestriction));
        //            }
        //        }
        //    }

        //    try
        //    {
        //        var result = CreateUser(model, RoleType.User, true);

        //        ActivityAuditHelper.LogEvent("Successful registration", ActivityAuditEvent.RegisterSuccess);
        //        return result;
        //    }
        //    catch (MembershipCreateUserException ex)
        //    {
        //        ActivityAuditHelper.LogEvent($"Registration failed to create user (Error: {ex.StatusCode})", ActivityAuditEvent.RegisterFail);
        //        throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, ErrorCodeToString(ex.StatusCode)));
        //    }
        //    catch (WebApiException ex)
        //    {
        //        ActivityAuditHelper.LogEvent($"Registration failed to create user (Error: {ex.ErrorDetails.ErrorMessage})", ActivityAuditEvent.RegisterFail);
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        ActivityAuditHelper.LogEvent($"Registration failed to create user (Error: {ex.Message})", ActivityAuditEvent.RegisterFail);
        //        log.Error(ex);
        //        throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Errors.GenericError));
        //    }
        //}

        internal static Boolean Otpverify(string emailorphone,string otpcode)
        {
            bool message = true;
            string phoneno = emailorphone;
            if (emailorphone.Contains('@'))
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == emailorphone && e.IsDelete == false);
                    phoneno = entityUser.PhoneNumber;
                }
            }

            if (phoneno == null)
            {               
                message = false;
                return message;
            }
            else
            {
                Entity.UserProfile userProfile;
                Entity.UserExtra userExtra;
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    userProfile = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == phoneno && !e.IsDelete);
                    userExtra = db.UserExtras.FirstOrDefault(e => e.UserId == userProfile.UserId && e.OTP == otpcode);
                    if (userExtra != null)
                    {
                        userExtra.OTPVerified = 1;
                        userExtra.Status = 0;
                        db.SaveChanges();
                        message = true;
                        return message;
                    }
                    else
                    {
                        message = false;
                        return message;
                    }

                }
            }
        }

        internal static int Otpverify(OTPverifyModel model)
        {
            int message = 0;
            //var companyList = new List<SelectListItem> { new SelectListItem() };
            //using (Entity.db_HeyDocEntities dB = new Entity.db_HeyDocEntities())
            //{
            //    var companies = dB.CompanyWhiteLabels
            //                     .OrderBy(y => y.CompanyId)
            //                     .Select(y => new SelectListItem() { Text = y.CompanyName, Value = y.CompanyId.ToString() })
            //                     .ToList();

            //    companyList.AddRange(companies);
            //    ViewBag.companies = companies; // Supply a selection list without the all option for statistics export UI
            //}
            var request = System.Web.HttpContext.Current.Request;
            if (model.PhoneNumber == null)
            {
                message = 4; //"Please enter register phone number and OTP Code";
                return message;
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
                    //string url = Request.Url.AbsoluteUri;

                    if (userExtra != null)
                    {                        
                        DateTime ExpireDt = userExtra.OTPCreateDT;
                        ExpireDt = ExpireDt.AddMinutes(10);
                        DateTime DtNow = DateTime.UtcNow;
                        if (DtNow < ExpireDt)
                        {
                            userExtra.OTPVerified = 1;
                            userExtra.Status = 0;
                            db.SaveChanges();
                            message = 1; //Successful.
                            return message;
                        }
                        else
                        {
                            message = 2; //OTP expired.
                            return message;
                        }                        
                    }
                    else
                    {
                        message = 3; //Request OTP again.
                        return message;
                    }

                }
            }
        }

        internal static string ResendOTP(string emailorphone)
        {
            string phoneno = emailorphone;
            if (emailorphone.Contains('@'))
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == phoneno && e.IsDelete == false);
                    phoneno = entityUser.PhoneNumber;
                }
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var userProfile = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == phoneno && !e.IsDelete);
                if (userProfile != null)
                {
                    var userExtra = db.UserExtras.FirstOrDefault(e => e.UserId == userProfile.UserId && e.OTPVerified == 0);
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
                        sMSReqModel.to = phoneno;
                        sMSReqModel.from = "HOPE";
                        smsResponseModel = SMSSend(sMSReqModel);
                        db.SaveChanges();


                        return "OTP resend successful";
                    }
                    else
                    {
                        return "OTP resend failed.Wrong phone number.Please call 09970928688 for technical assist";
                    }
                }
                else
                {
                    return "Unable to send OTP!. this phone number is not registered. Please call 09970928688 for technical assist";
                }
            }
        }

        internal static int ResendOTP(OTPverifyModel model)
        {
            int message = 0;
            var request = System.Web.HttpContext.Current.Request;
            if (model.PhoneNumber == null)
            {
                message = 4;
                return message; // "Please enter register phone number to receive OTP";                
            }
            else
            {
                Entity.UserProfile userProfile;
                Entity.UserExtra userExtra;
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    userProfile = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == model.PhoneNumber && !e.IsDelete);
                    if (userProfile != null)
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
                            message = 1;
                            return message; //"OTP resend successful. Please click here to ";
                        }
                        else
                        {
                            message = 2;
                            return message; //"OTP resend failed. Wrong phone number. Please call 09970928688 for technical assist";
                        }
                    }
                    else
                    {
                        message = 3;
                        return message; //"Unable to send OTP!. this phone number is not registered. Please call 09970928688 for technical assist";
                    }
                }
            }
        }

        internal static BoolResult ForgotPasswordBoth(string emailorphone)
        {
            if (emailorphone.Contains('@'))
            {
                return ForgotPassword(emailorphone);
            }
            else
            {
                return ForgotPasswordWithPhone(emailorphone);
            }
        }
        public static UserModel UserUidPhNumFromWallet(RegisterModel model)
        {
            var result = new UserModel();
            return result;
        }
        public static UserModel RegisterPWAAPI (string merchantUid, string PhoneNum)
        {
            var result = new UserModel();
            return result;
        }
        public static UserModel RegisterEmailAPIV2(RegisterModel model)
        {           
            ActivityAuditHelper.AddRequestDataToLog();
            string username;
            string phone;
            
            try
            {
                if (model.createUserisAdmin == false)
                {
                    model.Validate();
                }                
                username = model.Email.Trim();
                phone = model.PhoneNumber.Trim();

                if(username.Length == 0)
                {
                    username = phone + "." + DateTime.UtcNow.Ticks + "@hope.com.mm";
                    model.Email = username;
                }
                //if (!EmailHelper.IsValidEmail(username))
                //{
                //    throw new WebApiException(
                //        new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorEmailInvalidFormat));
                //}
            }
            catch (WebApiException ex)
            {
                ActivityAuditHelper.LogEvent($"Registration attempted with invalid data (Error: {ex.ErrorDetails.ErrorMessage})", ActivityAuditEvent.RegisterFail);
                throw;
            }

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())// has to separate to multiple DB connections due to WebSecurity will close DB connection
            {
                var entityUser = GetEntityTargetUserByPhoneNumber(db, phone, true,model);
                var entityUserEmail = GetEntityTargetUserByUsername(db, username, true,model);
                if (entityUser != null || entityUserEmail != null)                {
                    
                    ActivityAuditHelper.LogEvent($"Registration attempted for existing username ({username})", ActivityAuditEvent.RegisterFail);
                    throw new WebApiException(
                        new WebApiError(WebApiErrorCode.InvalidArguments, ErrorCodeToString(MembershipCreateStatus.DuplicateUserName)));
                }
                if (model.ReferralCode.HasValue && !db.UserReferralCodes.Any(e => e.Id == model.ReferralCode.Value))
                {
                    ActivityAuditHelper.LogEvent($"Registration attempted with invalid referral code ({model.ReferralCode.Value})", ActivityAuditEvent.RegisterFail);
                    throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorReferralCodeInvalid));
                }

                //var optionalFields = SignUpOptionalFields.None;
                //if (model.ReferralCode.HasValue)
                //{
                //    optionalFields = db.UserReferralCodes.FirstOrDefault(e => e.Id == model.ReferralCode.Value)?.SignUpOptionalFields ?? SignUpOptionalFields.None;
                //}
                //if (optionalFields.HasFlag(SignUpOptionalFields.Gender) && (!model.Gender.HasValue || !Enum.IsDefined(typeof(Gender), model.Gender)))
                //{
                //    model.Gender = null;
                //}

                if (model.CorporateId.HasValue && model.CorporateId != 0)
                {
                    try
                    {
                        //model.CorporateValidate(optionalFields);

                        var entityCorporates = db.Corporates.FirstOrDefault(e => e.CorporateId == model.CorporateId && !e.IsDelete && e.SecretKey == model.CorporateSecret);
                        if (entityCorporates == null)
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorCorporateNotFound));
                        }
                        else
                        {
                            int maxCoUser = Convert.ToInt32(entityCorporates.MaxSecretKey);
                            var CoUser = db.UserCorperates.Count(e => e.CorperateId == model.CorporateId);
                            if (CoUser >= maxCoUser)
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorCorporateNotFound));
                            }
                        }
                        var entityBranch = db.Branchs.FirstOrDefault(e => e.BranchId == model.BranchId && !e.IsDelete);
                        if (entityBranch == null)
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorCorporateBranchNotFound));
                        }

                        if ((model.IsDependent.HasValue && model.IsDependent.Value) || model.CorporateUserType == CorporateUserType.EmployeeDependants || model.CorporateUserType == CorporateUserType.EmployeeChild)
                        {
                            if (string.IsNullOrWhiteSpace(model.EmployeeDependantName))
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDependantNameNull));
                            }
                            if (string.IsNullOrWhiteSpace(model.EmployeeDependantIC))
                            {
                                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorDependantICNull));
                            }
                        }                      
                    }
                    catch (WebApiException ex)
                    {
                        ActivityAuditHelper.LogEvent($"Registration attempted with invalid corporate data (Error: {ex.ErrorDetails.ErrorMessage})", ActivityAuditEvent.RegisterFail);
                        throw;
                    }
                }
                else
                {
                    if (model.Birthday.HasValue && DateTime.UtcNow.AddYears(-18) < model.Birthday.Value)
                    {
                        ActivityAuditHelper.LogEvent("Registration attempted with age below restriction", ActivityAuditEvent.RegisterFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorAgeRestriction));
                    }
                }
            }

            try
            {
                var result = CreateUser(model, RoleType.User, false);
                ActivityAuditHelper.LogEvent("Successful registration", ActivityAuditEvent.RegisterSuccess);
                return result;
            }
            catch (MembershipCreateUserException ex)
            {
                ActivityAuditHelper.LogEvent($"Registration failed to create user (Error: {ex.StatusCode})", ActivityAuditEvent.RegisterFail);
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, ErrorCodeToString(ex.StatusCode)));
            }
            catch (WebApiException ex)
            {
                ActivityAuditHelper.LogEvent($"Registration failed to create user (Error: {ex.ErrorDetails.ErrorMessage})", ActivityAuditEvent.RegisterFail);
                throw;
            }
            catch (Exception ex)
            {
                ActivityAuditHelper.LogEvent($"Registration failed to create user (Error: {ex.Message})", ActivityAuditEvent.RegisterFail);
                log.Error(ex);
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Errors.GenericError));
            }
        }
        public static UserModel Login(EmailLoginModel model)
        {
            ActivityAuditHelper.AddRequestDataToLog();

            string username = model.Email.Trim();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityDevice = DeviceService.GetEntityDevice(db, model.Device, AccessTokenType.Doc2Us);
                ActivityAuditHelper.AddDeviceDataToLog(entityDevice);

                var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == username && e.IsDelete == false);

                if (entityUser != null)
                {
                    ActivityAuditHelper.AddUserDataToLog(entityUser);
                }

                const int maxPasswordAttempts = 5;
                const int passwordAttemptsWindow = 5;
                if (WebSecurity.IsAccountLockedOut(username, maxPasswordAttempts, passwordAttemptsWindow * 60))
                {
                    ActivityAuditHelper.LogEvent("Login attempted during password retry lock out", ActivityAuditEvent.LoginFail);
                    throw new WebApiException(
                             new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Account.AccountLockedTooManyAttempts, passwordAttemptsWindow, maxPasswordAttempts, ConstantHelper.Doc2UsEmailContact)));
                }

                if (entityUser != null && WebSecurity.Login(entityUser.UserName, model.Password))
                {
                    if (entityUser.IsBan)
                    {
                        ActivityAuditHelper.LogEvent("Login attempted by banned user", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                                 new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorBanned));
                    }

                    if (entityUser.CorporateId.HasValue && entityUser.Corporate.IsBan)
                    {
                        ActivityAuditHelper.LogEvent($"Login attempted by user under banned corporate (corporate ID: { entityUser.CorporateId.Value })", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorCorporateBanned));
                    }

                    // only allow logins from doctor or patient accounts
                    var allowedRoles = new HashSet<RoleType>(ConstantHelper.DoctorRoles, ConstantHelper.DoctorRoles.Comparer);
                    allowedRoles.Add(RoleType.User);
                    if (!allowedRoles.Any(r => entityUser.Roles.Contains(r)))
                    {
                        ActivityAuditHelper.LogEvent($"Login attempted by user with wrong role ({string.Join(";", entityUser.Roles)})", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, "Only HOPE-registered doctor and patient accounts can be used to login. Please try using another account."));
                    }
                    if (entityUser.Role == RoleType.Doctor && !entityUser.Doctor.IsVerified)
                    {
                        ActivityAuditHelper.LogEvent("Login attempted by unverified doctor", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                                 new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorHCPNotVerified));
                    }

                    entityUser.IsOnline = true;
                    entityUser.LastActivityDate = DateTime.UtcNow;
                    db.SaveChanges();

                    UserModel user = new UserModel(entityUser);
                    user.AccessToken = GetAccessToken(entityUser, entityDevice);
                    if (entityUser.Doctor != null)
                    {
                        user.GroupId = entityUser.Doctor.GroupId;
                    }
                    LogOnlineStatusUpdate(db, true, entityDevice, OnlineStatusChangeSource.Login);
                    db.SaveChanges();

                    // Update access token for log
                    ActivityAuditHelper.AddDeviceDataToLog(entityDevice);

                    ActivityAuditHelper.LogEvent("Successful login", ActivityAuditEvent.LoginSuccess);
                    return user;
                }
                else
                {
                    if (entityUser == null)
                    {
                        ActivityAuditHelper.LogEvent($"Login attempted for unregistered email ({username})", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                            new WebApiError(WebApiErrorCode.NotFound, Account.ErrorEmailNotRegistered));
                    }
                    else if (entityUser.webpages_Membership == null)
                    {
                        ActivityAuditHelper.LogEvent($"Login with password attempted for third party associated account", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                            new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorAccountAssociatedWithThirdParty));
                    }
                    else if (!entityUser.webpages_Membership.IsConfirmed.HasValue || !entityUser.webpages_Membership.IsConfirmed.Value)
                    {
                        ActivityAuditHelper.LogEvent($"Login attempted for unverified email", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                               new WebApiError(WebApiErrorCode.UnverifiedEmail, Account.ErrorEmailNotVerified));
                    }
                    else
                    {
                        ActivityAuditHelper.LogEvent("Login attempted with wrong password", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                            new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorInvalidPassword));
                    }
                }
            }
        }

        public static UserModel LoginAPIPWA(string accessToken, int deviceType)
        {
            //ActivityAuditHelper.AddRequestDataToLog();

            //string username = Email.Trim();
            //string phone = Email.Trim();
            int cid = 1;
            if (deviceType == 7)
            {
                cid = 5;
            }

            var token = accessToken;
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            
            var phone = jwtSecurityToken.Claims.First(claim => claim.Type == "phone").Value;
            var Password = jwtSecurityToken.Claims.First(claim => claim.Type == "valid").Value;

            string username = phone.Trim();

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                //var entityDevice = DeviceService.GetEntityDevice(db, model.Device, AccessTokenType.Doc2Us);
                //ActivityAuditHelper.AddDeviceDataToLog(entityDevice);

                var entityUser = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == phone && e.IsDelete == false && e.CompanyId == cid);
                if (username.IndexOf('@') > 0)
                {
                    //Login using email
                    entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == username && e.IsDelete == false && e.CompanyId == cid);
                }

                //int isVerify = 0;

                //if (entityUser != null)
                //{
                //    Email = entityUser.UserName;
                //    username = entityUser.UserName;
                //    var otpVerify = db.UserExtras.FirstOrDefault(e => e.UserId == entityUser.UserId);
                //    isVerify = otpVerify.OTPVerified;
                //    ActivityAuditHelper.AddUserDataToLog(entityUser);
                //}

                const int maxPasswordAttempts = 30;
                const int passwordAttemptsWindow = 30;
                if (WebSecurity.IsAccountLockedOut(username, maxPasswordAttempts, passwordAttemptsWindow * 60))
                {
                    ActivityAuditHelper.LogEvent("Login attempted during password retry lock out", ActivityAuditEvent.LoginFail);
                    throw new WebApiException(
                             new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Account.AccountLockedTooManyAttempts, passwordAttemptsWindow, maxPasswordAttempts, ConstantHelper.Doc2UsEmailContact)));
                }

                if (entityUser != null && WebSecurity.Login(entityUser.UserName, Password) && deviceType == 7)
                {
                    if (entityUser.IsBan)
                    {
                        ActivityAuditHelper.LogEvent("Login attempted by banned user", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                                 new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorBanned));
                    }

                    if (entityUser.CorporateId.HasValue && entityUser.Corporate.IsBan)
                    {
                        ActivityAuditHelper.LogEvent($"Login attempted by user under banned corporate (corporate ID: {entityUser.CorporateId.Value})", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorCorporateBanned));
                    }

                    // only allow logins from doctor or patient accounts
                    var allowedRoles = new HashSet<RoleType>(ConstantHelper.DoctorRoles, ConstantHelper.DoctorRoles.Comparer);
                    allowedRoles.Add(RoleType.User);
                    if (!allowedRoles.Any(r => entityUser.Roles.Contains(r)))
                    {
                        ActivityAuditHelper.LogEvent($"Login attempted by user with wrong role ({string.Join(";", entityUser.Roles)})", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, "Only HOPE-registered doctor and patient accounts can be used to login. Please try using another account."));
                    }
                    if (entityUser.Role == RoleType.Doctor && !entityUser.Doctor.IsVerified)
                    {
                        ActivityAuditHelper.LogEvent("Login attempted by unverified doctor", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                                 new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorHCPNotVerified));
                    }

                    entityUser.IsOnline = true;
                    entityUser.LastActivityDate = DateTime.UtcNow;
                    db.SaveChanges();

                    UserModel user = new UserModel(entityUser);
                    user.AccessToken = accessToken;
                    if (entityUser.Doctor != null)
                    {
                        user.GroupId = entityUser.Doctor.GroupId;
                    }
                    //LogOnlineStatusUpdate(db, true, entityDevice, OnlineStatusChangeSource.Login);
                    //Login from Other Applications Start
                    //if (model.CompanyId == 0)
                    //{
                    //    //var profileCompanyId = db.UserProfiles.FirstOrDefault(e => e.UserId == entityUser.UserId).CompanyId;
                    //    //model.LoginFromOtherApp = profileCompanyId ?? 1;
                    //    model.CompanyId = 1;
                    //}
                    Entity.CompanyWhiteLabelCurrentLogin loginApp = db.CompanyWhiteLabelCurrentLogins.FirstOrDefault(e => e.UserId == entityUser.UserId);
                    if (loginApp == null)
                    {

                        Entity.CompanyWhiteLabelCurrentLogin newLogin = new Entity.CompanyWhiteLabelCurrentLogin();
                        //Create new record
                        newLogin.UserId = entityUser.UserId;
                        newLogin.CurrentLoginCompanyId = cid;
                        db.CompanyWhiteLabelCurrentLogins.Add(newLogin);
                    }
                    else
                    {

                        //Update Existing record
                        loginApp.CurrentLoginCompanyId = cid;
                    }
                    //Login from Other Applications End
                    db.SaveChanges();

                    // Update access token for log
                    //ActivityAuditHelper.AddDeviceDataToLog(entityDevice);

                    ActivityAuditHelper.LogEvent("Successful login", ActivityAuditEvent.LoginSuccess);
                    return user;
                }
                else
                {
                    if (entityUser == null)
                    {
                        ActivityAuditHelper.LogEvent($"Login attempted for unregistered email ({username})", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                            new WebApiError(WebApiErrorCode.NotFound, Account.ErrorEmailNotRegistered));
                    }
                    else if (entityUser.webpages_Membership == null)
                    {
                        ActivityAuditHelper.LogEvent($"Login with password attempted for third party associated account", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                            new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorAccountAssociatedWithThirdParty));
                    }
                    //else if (!entityUser.webpages_Membership.IsConfirmed.HasValue || !entityUser.webpages_Membership.IsConfirmed.Value || isVerify == 0)
                    //{
                    //    ActivityAuditHelper.LogEvent($"Login attempted for unverified phone or email", ActivityAuditEvent.LoginFail);
                    //    throw new WebApiException(
                    //           new WebApiError(WebApiErrorCode.UnverifiedEmail, Account.ErrorPhoneNotVerified));
                    //}
                    else
                    {
                        ActivityAuditHelper.LogEvent("Login attempted with wrong password", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                            new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorInvalidPassword));
                    }
                }
            }
        }
        public static UserModel LoginAPIV2(EmailLoginModel model)
        {            
            ActivityAuditHelper.AddRequestDataToLog();

            string username = model.Email.Trim();
            string phone = model.Email.Trim();
            int cid = 1;
            if(model.CompanyId > 1)
            {
                cid = model.CompanyId;
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityDevice = DeviceService.GetEntityDevice(db, model.Device, AccessTokenType.Doc2Us);
                ActivityAuditHelper.AddDeviceDataToLog(entityDevice);
           
                var entityUser = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == phone && e.IsDelete == false && e.CompanyId == cid);
                if (username.IndexOf('@') > 0)
                {   
                    //Login using email
                    entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == username && e.IsDelete == false && e.CompanyId == cid);
                }

                int isVerify = 0;
            
                if (entityUser != null)
                {
                    model.Email = entityUser.UserName;
                    username = entityUser.UserName;
                    var otpVerify = db.UserExtras.FirstOrDefault(e => e.UserId == entityUser.UserId);
                    isVerify = otpVerify.OTPVerified;
                    ActivityAuditHelper.AddUserDataToLog(entityUser);
                }

                const int maxPasswordAttempts = 30;
                const int passwordAttemptsWindow = 30;
                if (WebSecurity.IsAccountLockedOut(username, maxPasswordAttempts, passwordAttemptsWindow * 60))
                {
                    ActivityAuditHelper.LogEvent("Login attempted during password retry lock out", ActivityAuditEvent.LoginFail);
                    throw new WebApiException(
                             new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Account.AccountLockedTooManyAttempts, passwordAttemptsWindow, maxPasswordAttempts, ConstantHelper.Doc2UsEmailContact)));
                }

                if (entityUser != null && WebSecurity.Login(entityUser.UserName, model.Password) && isVerify == 1)
                {
                    if (entityUser.IsBan)
                    {
                        ActivityAuditHelper.LogEvent("Login attempted by banned user", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                                 new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorBanned));
                    }

                    if (entityUser.CorporateId.HasValue && entityUser.Corporate.IsBan)
                    {
                        ActivityAuditHelper.LogEvent($"Login attempted by user under banned corporate (corporate ID: { entityUser.CorporateId.Value })", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorCorporateBanned));
                    }

                    // only allow logins from doctor or patient accounts
                    var allowedRoles = new HashSet<RoleType>(ConstantHelper.DoctorRoles, ConstantHelper.DoctorRoles.Comparer);
                    allowedRoles.Add(RoleType.User);
                    if (!allowedRoles.Any(r => entityUser.Roles.Contains(r)))
                    {
                        ActivityAuditHelper.LogEvent($"Login attempted by user with wrong role ({string.Join(";", entityUser.Roles)})", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, "Only HOPE-registered doctor and patient accounts can be used to login. Please try using another account."));
                    }
                    if (entityUser.Role == RoleType.Doctor && !entityUser.Doctor.IsVerified)
                    {
                        ActivityAuditHelper.LogEvent("Login attempted by unverified doctor", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                                 new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorHCPNotVerified));
                    }

                    entityUser.IsOnline = true;
                    entityUser.LastActivityDate = DateTime.UtcNow;
                    db.SaveChanges();

                    UserModel user = new UserModel(entityUser);
                    user.AccessToken = GetAccessToken(entityUser, entityDevice);
                    if (entityUser.Doctor != null)
                    {
                        user.GroupId = entityUser.Doctor.GroupId;
                    }
                    LogOnlineStatusUpdate(db, true, entityDevice, OnlineStatusChangeSource.Login);
                    //Login from Other Applications Start
                    if (model.CompanyId == 0)
                    {
                        //var profileCompanyId = db.UserProfiles.FirstOrDefault(e => e.UserId == entityUser.UserId).CompanyId;
                        //model.LoginFromOtherApp = profileCompanyId ?? 1;
                        model.CompanyId = 1;
                    }
                    Entity.CompanyWhiteLabelCurrentLogin loginApp = db.CompanyWhiteLabelCurrentLogins.FirstOrDefault(e => e.UserId == entityUser.UserId);
                    if (loginApp == null)
                    {
                       
                        Entity.CompanyWhiteLabelCurrentLogin newLogin = new Entity.CompanyWhiteLabelCurrentLogin();
                        //Create new record
                        newLogin.UserId = entityUser.UserId;
                        newLogin.CurrentLoginCompanyId = model.CompanyId;
                        db.CompanyWhiteLabelCurrentLogins.Add(newLogin);
                    }
                    else
                    {
                        
                        //Update Existing record
                        loginApp.CurrentLoginCompanyId = model.CompanyId;
                    }                 
                    //Login from Other Applications End
                    db.SaveChanges();

                    // Update access token for log
                    ActivityAuditHelper.AddDeviceDataToLog(entityDevice);

                    ActivityAuditHelper.LogEvent("Successful login", ActivityAuditEvent.LoginSuccess);
                    return user;
                }
                else
                {
                    if (entityUser == null)
                    {
                        ActivityAuditHelper.LogEvent($"Login attempted for unregistered email ({username})", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                            new WebApiError(WebApiErrorCode.NotFound, Account.ErrorEmailNotRegistered));
                    }
                    else if (entityUser.webpages_Membership == null)
                    {
                        ActivityAuditHelper.LogEvent($"Login with password attempted for third party associated account", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                            new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorAccountAssociatedWithThirdParty));
                    }
                    else if (!entityUser.webpages_Membership.IsConfirmed.HasValue || !entityUser.webpages_Membership.IsConfirmed.Value || isVerify == 0)
                    {
                        ActivityAuditHelper.LogEvent($"Login attempted for unverified phone or email", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                               new WebApiError(WebApiErrorCode.UnverifiedEmail, Account.ErrorPhoneNotVerified));
                    }
                    else
                    {
                        ActivityAuditHelper.LogEvent("Login attempted with wrong password", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(
                            new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorInvalidPassword));
                    }
                }
            }
        }
        public static BoolResult Logout(string accessToken)
        {
            ActivityAuditHelper.AddRequestDataToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityDevice = DeviceService.GetEntityDevice(db, accessToken, false);
                ActivityAuditHelper.AddDeviceDataToLog(entityDevice);
                ActivityAuditHelper.AddUserDataToLog(entityDevice.UserProfile);
                if (db.Devices.Count(e => e.UserId == entityDevice.UserId) <= 1)
                {
                    entityDevice.UserProfile.IsOnline = false;
                }
                entityDevice.UserProfile.IsOnline = false;
                LogOnlineStatusUpdate(db, false, entityDevice, OnlineStatusChangeSource.Logout);
                entityDevice.UserId = null;
                entityDevice.AccessToken = null;
                entityDevice.RegistrationId = null;
                entityDevice.AwsIosProdEndpointArn = null;
                entityDevice.AwsIosDevEndpointArn = null;

                db.SaveChanges();

                BoolResult result = new BoolResult(true);

                ActivityAuditHelper.LogEvent("User logged out", ActivityAuditEvent.Logout);
                return result;
            }
        }

        public static BoolResult DeactivateAcc(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityDevice = DeviceService.GetEntityDevice(db, accessToken, false);
                var entityUser = GetEntityUserByAccessToken(db, accessToken, false);

                entityDevice.UserId = null;
                entityDevice.AccessToken = null;

                entityUser.UserName = "[Deleted]" + entityUser.UserName + entityUser.UserId.ToString();
                entityUser.IsDelete = true;

                var entityChatRoomList = entityUser.Patient.ChatRooms;

                foreach (var entityChatRoom in entityChatRoomList)
                {
                    entityChatRoom.IsDelete = true;
                    db.SaveChanges();
                }

                BoolResult result = new BoolResult(true);

                return result;
            }
        }

        public static PhotoModel UploadPhoto(string accessToken, HttpContent content)
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
                Entity.UserProfile entityUser = GetEntityUserByAccessToken(db, accessToken, false);
                long oldPhotoId = 0;
                if (entityUser.PhotoId.HasValue)
                {
                    oldPhotoId = entityUser.PhotoId.Value;
                }

                string containerName = "u" + entityUser.UserId.ToString("D5");
                var entityPhoto = PhotoHelper.ReadFromStreamAndUpload(db, content, containerName).Result;
                entityUser.PhotoId = entityPhoto.PhotoId;

                entityUser.LastUpdateDate = DateTime.Now;

                db.SaveChanges();

                if (oldPhotoId > 0)
                {
                    PhotoHelper.DeletePhoto(db, oldPhotoId);
                }

                return new PhotoModel(entityPhoto);
            }
        }

        public static UserModel UpdateProfile(string accessToken, UserModel model)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                var entityUser = GetEntityUserByAccessToken(db, accessToken, false);
                return _UpdateProfilePartial(model, db, now, entityUser, true);
            }
        }

        public static UserModel UpdateProfileById(string userId, UserModel model)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                var entityUser = GetEntityUserByUserId(db, userId, false, true);
                return _UpdateProfilePartial(model, db, now, entityUser);
            }
        }

        public static UserModel UpdateProfileWithCorporate(db_HeyDocEntities db, UserModel model)
        {
            DateTime now = DateTime.UtcNow;
            var entityUser = GetEntityUserByUserId(db, model.UserId.ToString(), false, true);
            return _UpdateProfilePartial(model, db, now, entityUser, false, true);
        }

        private static UserModel _UpdateProfilePartial(UserModel model, Entity.db_HeyDocEntities db, DateTime now, Entity.UserProfile entityUser, bool updateActivityDate = false, bool updateCorporate = false)
        {
            entityUser.Birthday = model.Birthday ?? entityUser.Birthday;
            entityUser.Gender = model.Gender ?? entityUser.Gender;

            var userCorporate = entityUser.UserCorperates.FirstOrDefault();
            if (updateCorporate)
            {
                if (model.IsCorporate)
                {
                    entityUser.CorporateId = model.CorporateId;
                    if (userCorporate == null)
                    {
                        entityUser.UserCorperates.Add(new UserCorperate()
                        {
                            UserId = entityUser.UserId,
                            PurposeId = 3, // PurposeId 3 is Others
                            CorporateUserType = model.CorporateUserType,
                            BranchId = model.BranchId,
                            CorperateId = model.CorporateId,
                            PositionId = Convert.ToInt32(model.PositionId)
                        });                     
                    }
                    else
                    {
                        userCorporate.CorporateUserType = model.CorporateUserType;
                        userCorporate.CorperateId = model.CorporateId;
                        userCorporate.BranchId = model.BranchId;
                        userCorporate.PositionId = Convert.ToInt32(model.PositionId);
                    }
                    entityUser.PositionId = Convert.ToInt32(model.PositionId);
                }
                else
                {
                    entityUser.CorporateId = null;
                    entityUser.PositionId = null;
                    if (userCorporate != null)
                    {
                        db.UserCorperates.Remove(userCorporate);
                    }
                }
                db.SaveChanges();
                userCorporate = entityUser.UserCorperates.FirstOrDefault();
            }
            
            if (userCorporate != null)
            {
                if (!string.IsNullOrWhiteSpace(model.FullName))
                {
                    entityUser.FullName = model.FullName;
                    entityUser.Nickname = model.FullName;
                }

                if (model.BranchId != 0)
                {
                    var branch = db.Branchs.FirstOrDefault(e => !e.IsDelete && e.BranchId == model.BranchId);
                    if (branch == null)
                    {
                        throw new WebApiException(
                           new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateBranchNotFound));
                    }
                    userCorporate.BranchId = model.BranchId;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.Nickname))
                {
                    entityUser.Nickname = model.Nickname;
                }
                if (!string.IsNullOrWhiteSpace(model.FullName))
                {
                    entityUser.FullName = model.FullName;
                }
                entityUser.CountryId = model.Country?.CountryId;
            }

            entityUser.LastUpdateDate = now;
            if (updateActivityDate)
            {
                entityUser.LastActivityDate = now;
            }

            if (!string.IsNullOrEmpty(model.Address))
            {
                entityUser.Address = model.Address;
            }
            if (!string.IsNullOrEmpty(model.IC))
            {
                model.IC = model.IC.Replace("-", "");
                entityUser.IC = model.IC;
            }
            //if (!string.IsNullOrEmpty(model.PhoneNumber))
            //{
            //    entityUser.PhoneNumber = model.PhoneNumber;
            //}
            if (!string.IsNullOrEmpty(model.UserName))
            {
                entityUser.UserName = model.UserName;
            }
            entityUser.StateId = model.StateId;
            entityUser.TownshipId = model.TownshipId;
            entityUser.BloodType = model.BloodType;
            if (model.PatientExtra != null)
            {
                Entity.PatientExtra existingPE = db.PatientExtras.FirstOrDefault(e => e.UserId == model.UserId);
                if(existingPE != null)
                {
                    existingPE.RelationshipId = model.PatientExtra.RelationshipId;
                    existingPE.EmergencyPerson = model.PatientExtra.EmergencyPerson;
                    existingPE.EmergencyContact = model.PatientExtra.EmergencyContact;
                    existingPE.EmergencyAddress = model.PatientExtra.EmergencyAddress;
                }
                else
                {
                    Entity.PatientExtra pe = new PatientExtra();
                    pe.RelationshipId = model.PatientExtra.RelationshipId;
                    pe.EmergencyPerson = model.PatientExtra.EmergencyPerson;
                    pe.EmergencyContact = model.PatientExtra.EmergencyContact;
                    pe.EmergencyAddress = model.PatientExtra.EmergencyAddress;

                    entityUser.Patient.PatientExtras.Add(pe);
                }
            }
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                throw e;
            }
             
            return new UserModel(entityUser);
        }

        public static UserModel ViewProfile(string accessToken, int? userId = null)
        {
            ActivityAuditHelper.AddRequestParamsToLog();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                var entityUser = GetEntityUserByAccessToken(db, accessToken, false);

                if (userId.HasValue)
                {
                    entityUser = db.UserProfiles.FirstOrDefault(e => e.UserId == userId.Value);
                }

                return new UserModel(entityUser);
            }
        }

        public static BoolResult ChangePassword(string accessToken, ChangePasswordModel model)
        {
            model.Validate();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = GetEntityUserByAccessToken(db, accessToken, false);
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                try
                {
                    if (WebSecurity.ChangePassword(entityUser.UserName, model.OldPassword, model.NewPassword))
                    {
                        AccountService.NotifyPasswordChanged(entityUser.UserName);
                        return new BoolResult(true);
                    }
                    else
                    {
                        throw new WebApiException(
                            new WebApiError(WebApiErrorCode.NotFound, Account.ErrorOldOrNewPasswordInvalid));
                    }
                }
                catch (Exception)
                {
                    throw new WebApiException(
                        new WebApiError(WebApiErrorCode.NotFound, Account.ErrorOldOrNewPasswordInvalid));
                }
            }
        }

        public static UserModel CreateUser(RegisterModel model, RoleType role, bool confirmationRequired, HttpPostedFileBase signatureFile = null, SourceType sourceType = SourceType.Apps)
        {
            try
            {
                string username = model.Email.Trim();
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityUser = AccountService.GetEntityTargetUserByUsername(db, username, true,model);
                    if (entityUser != null)
                    {
                        return null;
                    }
                }
                int? countryId = null;
                if (model.CountryId > 0)
                {
                    countryId = model.CountryId;
                }
                if (!string.IsNullOrEmpty(model.IC))
                {
                    model.IC = model.IC.Replace("-", "");
                }
                if (model.StateId == 0) { model.StateId = 1; }
                if (model.TownshipId == 0) { model.TownshipId = 1; }
                model.CompanyId = model.CompanyId ?? 1;
                
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            string confirmationToken = WebSecurity.CreateUserAndAccount(username, model.Password,
                                    new
                                    {
                                        IsBan = false,
                                        Nickname = model.Nickname,
                                        FullName = model.FullName ?? model.Nickname,
                                        Gender = model.Gender,
                                        CountryId = countryId,
                                        IsOnline = false,
                                        Language = model.Language,
                                        Title = model.Title,
                                        IC = model.IC,
                                        Address = model.Address ?? "",
                                        PhoneNumber = model.PhoneNumber,
                                        StateId = model.StateId,
                                        TownshipId = model.TownshipId,
                                        BloodType = model.BloodType,
                                        CompanyId = model.CompanyId,
                                    }, confirmationRequired);

                        }
                        catch (DbEntityValidationException e)
                        {
                            ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(model.Email);
                            ((SimpleMembershipProvider)Membership.Provider).DeleteUser(model.Email, true);
                            throw;
                        }
                        var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == username && e.IsDelete == false);
                        AddUserToRole(db, entityUser, role);

                        DateTime now = DateTime.UtcNow;
                        if (role == RoleType.User)
                        {
                            entityUser.Birthday = model.Birthday;
                            entityUser.PhoneNumber = model.PhoneNumber;
                            entityUser.Address = model.Address;
                            entityUser.IC = model.IC;
                            entityUser.CreatedSource = sourceType;
                            entityUser.ReferrerId = model.ReferralCode;
                            var entityPatient = db.Patients.Create();
                            entityPatient.UserId = entityUser.UserId;
                            db.Patients.Add(entityPatient);

                            PatientExtra patientExtra = new PatientExtra();
                            patientExtra.UserId = entityUser.UserId;
                            patientExtra.EmergencyPerson = "";
                            patientExtra.EmergencyContact = "";
                            patientExtra.EmergencyAddress = "";
                            patientExtra.RelationshipId = 1;
                           

                            db.PatientExtras.Add(patientExtra);

                            UserExtra userExtra = new UserExtra();
                            Random r = new Random();
                            int rInt = r.Next(100000, 999999);
                            userExtra.OTP = rInt.ToString();
                            userExtra.UserId = entityUser.UserId;
                            userExtra.OTPCreateDT = DateTime.UtcNow;
                            userExtra.OTPVerified = 0;
                            userExtra.Status = 1;

                            SMSResModel smsResponseModel = new SMSResModel();
                            if (model.createUserisAdmin)
                            {
                                userExtra.OTPVerified = 1;
                                userExtra.Status = 0;

                            }
                            else
                            {
                                SMSReqModel sMSReqModel = new SMSReqModel();
                                sMSReqModel.text = ""+ rInt + " HOPE registration OTP: Please click below link for verification https://app.hope.com.mm/account/otpverify/";
                                sMSReqModel.to = model.PhoneNumber;
                                sMSReqModel.from = "HOPE";
                                smsResponseModel = SMSSend(sMSReqModel);
                            }
                            if (entityUser.FullName.ToLower().Trim().Contains("test"))
                            {
                                userExtra.OTP = "000000";
                            }
                            db.UserExtras.Add(userExtra);


                            try
                            {
                                if (smsResponseModel.status == 200 || model.createUserisAdmin)
                                {
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(model.Email);
                                    ((SimpleMembershipProvider)Membership.Provider).DeleteUser(model.Email, true);
                                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPhoneInvalid));
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                throw;
                            }
                            var nickName = entityUser.FullName;
                            //if (string.IsNullOrEmpty(entityUser.Nickname))
                            //{
                            //    var name = entityUser.UserName.Split('@');
                            //    if (name != null && name.Length > 1)
                            //    {
                            //        nickName = name[0];
                            //    }
                            //}
                            //if (confirmationRequired)
                            //{
                            //    SendVerificationEmail(entityUser.UserName, confirmationToken, nickName);
                            //}
                        }
                        else if (ConstantHelper.DoctorRoles.Contains(role))
                        {
                            entityUser.IC = model.IC;
                            entityUser.Birthday = model.Birthday;
                            var entityDoctor = db.Doctors.Create();
                            entityDoctor.UserId = entityUser.UserId;
                            entityDoctor.Practicing = model.Practicing;
                            entityDoctor.MedicalSch = model.MedicalSch;
                            entityDoctor.AboutMe = model.AboutMe;
                            entityDoctor.CategoryId = model.CategoryId;
                            entityDoctor.GroupId = model.GroupId;
                            entityDoctor.Specialty = model.Specialty;
                            entityDoctor.Qualifications = model.Qualification;
                            entityDoctor.HospitalName = model.HospitalName;
                            entityDoctor.RegisterNumber = model.RegisterNumber;
                            entityDoctor.IsPartner = model.IsPartner;
                            entityUser.PhoneNumber = model.PhoneNumber;
                            entityDoctor.ShowInApp = model.ShowInApp;
                            entityDoctor.CanApproveEPS = model.CanApproveEPS;
                            entityDoctor.IsChatBotEnabled = model.IsChatBotEnabled;
                            entityDoctor.VideoChatUrl = string.IsNullOrWhiteSpace(model.VideoChatURL) ? null : model.VideoChatURL;
                            entityDoctor.IsDigitalSignatureEnabled = true;
                            var entityDoctorStat = db.DoctorStats.Create();
                            entityDoctorStat.UserId = entityUser.UserId;

                            db.Doctors.Add(entityDoctor);
                            db.DoctorStats.Add(entityDoctorStat);

                            UserExtra userExtra = new UserExtra();
                            Random r = new Random();
                            int rInt = r.Next(100000, 999999);
                            userExtra.OTP = rInt.ToString();
                            userExtra.UserId = entityUser.UserId;
                            userExtra.OTPCreateDT = DateTime.UtcNow;
                            userExtra.OTPVerified = 0;
                            userExtra.Status = 1;

                            SMSResModel smsResponseModel = new SMSResModel();
                            if (model.createUserisAdmin)
                            {
                                userExtra.OTPVerified = 1;
                                userExtra.Status = 0;

                            }
                            else
                            {
                                SMSReqModel sMSReqModel = new SMSReqModel();
                                sMSReqModel.text = ""+ rInt +" HOPE registration OTP:Please click below link for verification https://app.hope.com.mm/account/otpverify/";
                                sMSReqModel.to = model.PhoneNumber;
                                sMSReqModel.from = "HOPE";
                                smsResponseModel = SMSSend(sMSReqModel);
                            }
                            if (entityUser.FullName.ToLower().Trim().Contains("test"))
                            {
                                userExtra.OTP = "000000";
                            }
                            db.UserExtras.Add(userExtra);
                            try
                            {
                                if (smsResponseModel.status == 200 || model.createUserisAdmin)
                                {
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(model.Email);
                                    ((SimpleMembershipProvider)Membership.Provider).DeleteUser(model.Email, true);
                                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorPhoneInvalid));
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(model.Email);
                                ((SimpleMembershipProvider)Membership.Provider).DeleteUser(model.Email, true);
                                throw;
                            }
                            if (signatureFile != null)
                            {
                                CreateOrUpdateSignature(db, entityDoctor, signatureFile.InputStream, signatureFile.FileName);
                            }
                        }

                        if (model.CorporateId.HasValue && model.CorporateId != 0)
                        {
                            var corporateExist = db.Corporates.Any(e => !e.IsDelete && e.CorporateId == model.CorporateId.Value);
                            if (!corporateExist)
                            {
                                ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(model.Email);
                                ((SimpleMembershipProvider)Membership.Provider).DeleteUser(model.Email, true);
                                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateNotFound));
                            }

                            var branchExist = db.Branchs.Any(e => !e.IsDelete && e.BranchId == model.BranchId.Value);
                            if (!branchExist)
                            {
                                ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(model.Email);
                                ((SimpleMembershipProvider)Membership.Provider).DeleteUser(model.Email, true);
                                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorCorporateBranchNotFound));
                            }

                            if (model.Birthday.HasValue)
                            {
                                entityUser.Birthday = model.Birthday;
                            }
                            else if (GetDateOfBirthFromIC(model.IC, out var dateOfBirth))
                            {
                                entityUser.Birthday = dateOfBirth;
                            }

                            entityUser.CorporateId = model.CorporateId;
                            entityUser.IsDependent = model.IsDependent;
                            entityUser.PositionId = Convert.ToInt32(model.CorporatePositionId);
                            var entityCorperateUser = db.UserCorperates.Create();

                            entityCorperateUser.UserId = entityUser.UserId;
                            entityCorperateUser.CorperateId = model.CorporateId.Value;
                            entityCorperateUser.BranchId = model.BranchId.Value;
                            entityCorperateUser.PositionId = Convert.ToInt32(model.CorporatePositionId);
                            // PurposeId 3 is "Others"
                            entityCorperateUser.PurposeId = string.IsNullOrEmpty(model.SignUpPurpose) ? 3 : Convert.ToInt32(model.SignUpPurpose);

                            if (model.CorporateUserType == CorporateUserType.Unknown)
                            {
                                model.CorporateUserType = model.IsDependent.HasValue && model.IsDependent.Value ? CorporateUserType.EmployeeDependants : CorporateUserType.Employee;
                            }

                            if (model.CorporateUserType == CorporateUserType.EmployeeDependants || model.CorporateUserType == CorporateUserType.EmployeeChild)
                            {
                                entityCorperateUser.EmployeeDependant = model.EmployeeDependantName;
                                entityCorperateUser.EmployeeDependantIC = model.EmployeeDependantIC;
                            }

                            entityCorperateUser.CorporateUserType = model.CorporateUserType;
                            db.UserCorperates.Add(entityCorperateUser);
                        }

                        if (role == RoleType.Pharmacy)
                        {
                            entityUser.PrescriptionSourceId = model.PrescriptionSourceId;
                        }
                        else if (role == RoleType.Doctor)
                        {
                            entityUser.PrescriptionSourceId = ConstantHelper.DoctorPrescriptionSourceId;
                        }

                        //Generate Medical Id
                        entityUser.MedicalId = GenerateMedicalId(entityUser.UserId);
                        if (string.IsNullOrEmpty(entityUser.MedicalId))
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Forms.ErrorDescNull));
                        }

                        db.SaveChanges();
                        transaction.Commit();                       
                        UserModel result = new UserModel(entityUser);
                        return result;

                    }
                }
            }
            catch (MembershipCreateUserException mcuex)
            {
                ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(model.Email);
                ((SimpleMembershipProvider)Membership.Provider).DeleteUser(model.Email, true);

                string error = ErrorCodeToString(mcuex.StatusCode);
                log.Error(error);
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.InvalidArguments, error));
            }            
            catch (Exception ex)
            {
                ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(model.Email);
                ((SimpleMembershipProvider)Membership.Provider).DeleteUser(model.Email, true);

                if (ex is WebApiException)
                {
                    throw;
                }

                log.Error(ex);
                throw new WebApiException(
                   new WebApiError(WebApiErrorCode.UnknownError));
            }
        }

        public static CorporateSignUpOptions GetCorporateSignUpOptions(Guid? referralCode)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var corporates = CorporateService.GetCorporateListMobile(0, -1).Select(e => new SignUpOption { Value = e.CorporateId.ToString(), DisplayName = e.BranchName }).ToList();
                var signUpPurposes = db.SignUpPurposes.Select(e => new SignUpOption { Value = e.Id.ToString(), DisplayName = e.Purpose }).ToList();
                var signUpOptions = new CorporateSignUpOptions { SignUpPurpose = signUpPurposes, Corporates = corporates };
                if (referralCode.HasValue)
                {
                    var entityReferral = db.UserReferralCodes.FirstOrDefault(e => e.Id == referralCode);
                    var defaultCorporateId = entityReferral?.DefaultSignUpCorporateId;
                    signUpOptions.DefaultCorporateId = defaultCorporateId;
                    signUpOptions.ReferralName = entityReferral?.ReferrerName;
                    signUpOptions.LogoUrl = entityReferral?.LogoUrl;
                    signUpOptions.LogoDisplayName = entityReferral?.LogoDisplayName;
                    signUpOptions.ReferralDisclaimer = entityReferral?.ReferralDisclaimerText;
                    if (defaultCorporateId.HasValue && !corporates.Any(o => o.Value.Equals(defaultCorporateId.Value.ToString())))
                    {
                        signUpOptions.Corporates.Add(new SignUpOption { Value = defaultCorporateId.ToString(), DisplayName = CorporateService.GetCorporateById(defaultCorporateId.Value).BranchName });
                    }
                }
                return signUpOptions;
            }
        }

        public static PublicReferralSignUpResources GetPublicReferralSignUpResources(Guid referralCode)
        {
            using (var db = new db_HeyDocEntities())
            {
                var entityReferral = db.UserReferralCodes.FirstOrDefault(e => e.Id == referralCode);
                if (entityReferral == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound));
                }

                return new PublicReferralSignUpResources(entityReferral);
            }
        }

        public static void CreateOrUpdateSignature(Entity.db_HeyDocEntities db, Entity.Doctor entityDoctor, Stream file, string oriFileName)
        {
            string oldUrl = entityDoctor.SignatureUrl;
            var ext = Path.GetExtension(oriFileName);
            if (string.IsNullOrEmpty(ext))
            {
                ext = ".png";
            }
            string containerName = "u" + entityDoctor.UserId.ToString("D5");
            string path = "signature/" + Guid.NewGuid().ToString() + ext;
            var originalBlobUrl = CloudBlob.UploadFile(containerName, path, file);
            entityDoctor.SignatureUrl = originalBlobUrl;
            db.SaveChanges();
            if (!string.IsNullOrEmpty(oldUrl))
            {
                CloudBlob.DeleteImage(oldUrl);
            }
        }

        public static BoolResult SetOnlineStatus(string accessToken, bool isOnline)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);
                var device = DeviceService.GetEntityDevice(db, accessToken, false);

                entityUser.IsOnline = isOnline;
                LogOnlineStatusUpdate(db, isOnline, device, OnlineStatusChangeSource.SetOnlineStatus);

                db.SaveChanges();

                return new BoolResult(true);
            }
        }
        
        public static bool VerifyPhoneWithOTP(UserExtraModel model)
        {
            
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityTargetUserByUserId(db, model.UserId, false);
                var recordToVerify = db.UserExtras.FirstOrDefault(e => e.UserId == entityUser.UserId && e.Status == 1);
                if (recordToVerify == null)
                {
                    return false;
                }
                else
                {
                    var dtNow = DateTime.UtcNow;
                    var dtValidFrom = recordToVerify.OTPCreateDT;
                    var dtValidTo = dtValidFrom.AddDays(1);
                    
                    if (model.OTP == recordToVerify.OTP && dtNow > dtValidFrom && dtNow < dtValidTo)
                    {
                        recordToVerify.OTPVerified = 1;
                        recordToVerify.Status = 0;
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        public static void LogOnlineStatusUpdate(Entity.db_HeyDocEntities db, bool isOnline, Entity.Device device, OnlineStatusChangeSource source)
        {
            if (!device.UserId.HasValue)
            {
                log.Warn($"Online status change from invalid device (ID: {device.DeviceId})");
                return;
            }
            // Skip logging for non-doctors as online status for non-doctors isn't really used for anything
            if (!db.Doctors.Any(e => e.UserId == device.UserId.Value))
            {
                return;
            }
            var onlineStatusLog = new Entity.OnlineStatusLog()
            {
                DoctorId = device.UserId.Value,
                LogDate = DateTime.UtcNow,
                OnlineStatus = isOnline,
                SourceDeviceType = (byte)device.DeviceType,
                SourceApi = (byte)source
            };
            db.OnlineStatusLogs.Add(onlineStatusLog);
        }

        public static BoolResult ResendVerificationEmail(string email)
        {
            ActivityAuditHelper.AddRequestDataToLog();
            email = email.Trim();
            if (string.IsNullOrEmpty(email))
            {
                ActivityAuditHelper.LogEvent("Resend verification attempted for empty email", ActivityAuditEvent.ResendVerificationEmailFail);
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorEmailNull));
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == email && e.IsDelete == false);
                if (entityUser == null || entityUser.webpages_Membership == null)
                {
                    ActivityAuditHelper.LogEvent($"Resend verification attempted for unregistered email ({email})", ActivityAuditEvent.ResendVerificationEmailFail);
                    throw new WebApiException(
                        new WebApiError(WebApiErrorCode.NotFound, Account.ErrorEmailNotRegistered));
                }
                ActivityAuditHelper.AddUserDataToLog(entityUser);
                if (entityUser.webpages_Membership.IsConfirmed.HasValue && entityUser.webpages_Membership.IsConfirmed.Value)
                {
                    ActivityAuditHelper.LogEvent($"Resend verification attempted for already verified email ({email})", ActivityAuditEvent.ResendVerificationEmailFail);
                    throw new WebApiException(
                            new WebApiError(WebApiErrorCode.InvalidAction, Account.ErrorEmailAlreadyVerified));
                }
                var nickName = entityUser.FullName;
                //if (string.IsNullOrEmpty(entityUser.Nickname))
                //{
                //    var name = email.Split('@');
                //    if (name != null && name.Length > 1)
                //    {

                //        nickName = name[0];
                //    }
                //}

                ActivityAuditHelper.LogEvent("Successful resend verification email", ActivityAuditEvent.ResendVerificationEmailSuccess);
                return SendVerificationEmail(email, entityUser.webpages_Membership.ConfirmationToken, nickName);
            }
        }
        public static BoolResult ForgotPassword(string email)
        {
            ActivityAuditHelper.AddRequestDataToLog();
            email = email.Trim();
            if (string.IsNullOrEmpty(email))
            {
                ActivityAuditHelper.LogEvent("Password reset attempted for empty email", ActivityAuditEvent.PasswordResetFail);
                throw new WebApiException(
                    new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorEmailNull));
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == email && e.IsDelete == false);
                if (entityUser == null || entityUser.webpages_Membership == null)
                {
                    ActivityAuditHelper.LogEvent("Password reset attempted for unresgistered email", ActivityAuditEvent.PasswordResetFail);
                    throw new WebApiException(
                        new WebApiError(WebApiErrorCode.NotFound, Account.ErrorEmailNotRegistered));
                }
                ActivityAuditHelper.AddUserDataToLog(entityUser);
                if (entityUser.webpages_Membership.IsConfirmed.HasValue && !entityUser.webpages_Membership.IsConfirmed.Value)
                {
                    ActivityAuditHelper.LogEvent("Password reset attempted for uncofirmed email", ActivityAuditEvent.PasswordResetFail);
                    throw new WebApiException(
                        new WebApiError(WebApiErrorCode.UnverifiedEmail, Account.ErrorEmailNotVerified));
                }

                string token = WebSecurity.GeneratePasswordResetToken(email);
                if (SendForgotPasswordEmail(email, token))
                {
                    ActivityAuditHelper.LogEvent("Password reset email sent successfully", ActivityAuditEvent.PasswordResetSuccess);
                }
                else
                {
                    ActivityAuditHelper.LogEvent("Password reset email failed to sent", ActivityAuditEvent.PasswordResetFail);
                }
                return new BoolResult(true);
            }
        }

        public static BoolResult ForgotPasswordWithPhone(string phone)
        {
           
                ActivityAuditHelper.AddRequestDataToLog();
                phone = phone.Trim();
                if (string.IsNullOrEmpty(phone))
                {
                    ActivityAuditHelper.LogEvent("Password reset attempted for empty email", ActivityAuditEvent.PasswordResetFail);
                    throw new WebApiException(
                        new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorEmailNull));
                }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == phone && e.IsDelete == false);
                if (entityUser == null || entityUser.webpages_Membership == null)
                {
                    ActivityAuditHelper.LogEvent("Password reset attempted for unresgistered email", ActivityAuditEvent.PasswordResetFail);
                    throw new WebApiException(
                        new WebApiError(WebApiErrorCode.NotFound, Account.ErrorEmailNotRegistered));
                }
                ActivityAuditHelper.AddUserDataToLog(entityUser);
                if (entityUser.webpages_Membership.IsConfirmed.HasValue && !entityUser.webpages_Membership.IsConfirmed.Value)
                {
                    ActivityAuditHelper.LogEvent("Password reset attempted for uncofirmed email", ActivityAuditEvent.PasswordResetFail);
                    throw new WebApiException(
                        new WebApiError(WebApiErrorCode.UnverifiedEmail, Account.ErrorEmailNotVerified));
                }
                string email = entityUser.UserName;
                string token = WebSecurity.GeneratePasswordResetToken(email);
                if (SendForgotPasswordPhone(phone, token))
                {
                    ActivityAuditHelper.LogEvent("Password reset email sent successfully", ActivityAuditEvent.PasswordResetSuccess);
                }
                else
                {
                    ActivityAuditHelper.LogEvent("Password reset email failed to sent", ActivityAuditEvent.PasswordResetFail);
                }
                return new BoolResult(true);
            }
            
        }
        public static BoolResult SendPatientSupportEmail(string email, DateTime notRespondedTime, bool IsPremium, string doctorInCharge)
        {
            try
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {
                    var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == email);
                    if (entityUser == null || entityUser.webpages_Membership == null)
                    {
                        return new BoolResult(false);
                    }
                    if (entityUser.webpages_Membership.IsConfirmed.HasValue && !entityUser.webpages_Membership.IsConfirmed.Value)
                    {
                        return new BoolResult(false);

                    }
                    int userId = entityUser.UserId;
                    var name = entityUser.FullName;
                    string subject = "HOPE: Patient Support";
                    string content = EmailHelper.ConvertEmailHtmlToString(@"Emails\PatientSupportEmail.html");
                    content = string.Format(content, email, userId, name, IsPremium, notRespondedTime.AddHours(8), doctorInCharge);

                    // TODO M UNBLANK: Support email as recepient
                    EmailHelper.SendViaSendGrid(new List<string>() { "" }, subject, content, string.Empty);
                    return new BoolResult(true);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new BoolResult(false);
            }
        }
        public static BoolResult SendPatientSupportFreeChatEmail(Entity.UserProfile entityPatient, Entity.UserProfile entityDoctor, string entityChatList)
        {
            try
            {
                using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
                {

                    string subject = "HOPE: Patient Support";
                    string content = EmailHelper.ConvertEmailHtmlToString(@"Emails\PatientSupportFreeChatEmail.html");

                    content = string.Format(content, entityPatient.UserName, entityPatient.FullName, entityDoctor.UserName, entityDoctor.FullName, entityChatList);


                    // TODO M UNBLANK: Support email as recepient
                    EmailHelper.SendViaSendGrid(new List<string>() { "" }, subject, content, string.Empty);
                    return new BoolResult(true);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new BoolResult(false);
            }
        }
        public static string GenerateMedicalId(int userId)
        {
            // raw data that will be hash to Medical Id
            StringBuilder rawData = new StringBuilder();
            rawData.Append(userId.ToString());
            rawData.Append("-");
            rawData.Append(DateTime.UtcNow.ToString());
            // convert raw data to bytes
            return HashBySHA512(rawData.ToString());
        }
        public static SimpleIdModel GetUserIdByMedicalId(string MedicalId, string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                var entityPharmacist = GetEntityUserByAccessToken(db, accessToken, false, RoleType.Doctor);
                if (!entityPharmacist.Doctor.IsPartner)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Errors.UnauthorizedNotPartner));
                }
                var entityUser = db.UserProfiles.FirstOrDefault(x => x.MedicalId == MedicalId && !x.IsDelete);
                if (entityUser == null)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Account.ErrorUserNotFound));
                }
                if (entityUser.IsBan)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.UserIsBanned, Account.ErrorBanned));
                }
                var entityScan = db.ScanDetails.FirstOrDefault(x => x.MedicalId == MedicalId && x.UserId == entityPharmacist.UserId);
                if (entityScan != null)
                {
                    entityScan.ScannedTime = DateTime.UtcNow;
                }
                else
                {
                    entityScan = db.ScanDetails.Create();
                    entityScan.UserId = entityPharmacist.UserId;
                    entityScan.MedicalId = MedicalId;
                    entityScan.ScannedTime = DateTime.UtcNow;
                    db.ScanDetails.Add(entityScan);
                }
                db.SaveChanges();
                return new SimpleIdModel(entityUser.UserId);
            }
        }
        public static List<ScanUserModel> GetScannedList(string accessToken, int skip, int take)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                DateTime now = DateTime.UtcNow;
                var entityPharmacist = GetEntityUserByAccessToken(db, accessToken, false, RoleType.Doctor);
                if (!entityPharmacist.Doctor.IsPartner)
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Errors.UnauthorizedNotPartner));
                }
                var modelList = new List<ScanUserModel>();
                var entityScannedList = db.ScanDetails.Where(x => x.UserId == entityPharmacist.UserId);
                entityScannedList = entityScannedList.OrderByDescending(x => x.ScannedTime).Skip(skip).Take(take);
                foreach (var entityScan in entityScannedList)
                {
                    var entityUser = db.UserProfiles.FirstOrDefault(x => x.MedicalId == entityScan.MedicalId && !x.IsDelete && !x.IsBan);
                    if (entityUser != null)
                    {
                        var model = new ScanUserModel(entityUser);
                        model.ScannedTime = entityScan.ScannedTime;
                        modelList.Add(model);
                    }

                }

                return modelList;
            }
        }

        public static UserModel LoginOAuth(OAuthLoginModel model)
        {
            ActivityAuditHelper.AddRequestDataToLog();
            try
            {
                model.Validate();
            }
            catch (WebApiException ex)
            {
                ActivityAuditHelper.LogEvent($"Registration/Login failed (Error: {ex.ErrorDetails.ErrorMessage})", ActivityAuditEvent.IntegratedRegisterLoginFail);
                throw;
            }
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    DateTime now = DateTime.UtcNow;
                    Entity.UserProfile entityUser = null;
                    OAuthData oauthData;
                    try
                    {
                        oauthData = GetDataFromOAuthProvider(model.OAuthType, model.OAuthId, model.OAuthToken);
                    }
                    catch (WebApiException ex)
                    {
                        ActivityAuditHelper.LogEvent($"Registration/Login failed (Error: {ex.ErrorDetails.ErrorMessage})", ActivityAuditEvent.IntegratedRegisterLoginFail);
                        throw;
                    }

                    entityUser = db.UserProfiles.FirstOrDefault(x => x.UserName == oauthData.Email && x.IsDelete == false);

                    if (entityUser.IsBan)
                    {
                        ActivityAuditHelper.LogEvent("Login attempted by banned user", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorBanned));
                    }
                    else if (entityUser != null && entityUser.CorporateId.HasValue && entityUser.Corporate.IsBan)
                    {
                        ActivityAuditHelper.LogEvent($"OAuth login attempted by user under banned corporate (corporate ID: { entityUser.CorporateId.Value })", ActivityAuditEvent.LoginFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorCorporateBanned));
                    }

                    if (entityUser == null)
                    {
                        entityUser = db.UserProfiles.Create();
                        entityUser.CreateDate = now;
                        entityUser.LastUpdateDate = now;
                        entityUser.UserName = oauthData.Email;
                        entityUser.FullName = oauthData.Name;
                        entityUser.Nickname = oauthData.Name;
                        AddUserToRole(db, entityUser, RoleType.User);
                        db.UserProfiles.Add(entityUser);
                    }
                    else
                    {
                        entityUser.UserName = oauthData.Email;
                        entityUser.FullName = oauthData.Name;
                        entityUser.Nickname = oauthData.Name;
                    }
                    var entityPhoto = entityUser.Photo;
                    if (entityPhoto == null)
                    {
                        entityPhoto = db.Photos.Create();
                        entityPhoto.ImageUrl = oauthData.ImageUrl;
                        entityPhoto.CreateDate = now;

                        db.Photos.Add(entityPhoto);
                        entityUser.PhotoId = entityPhoto.PhotoId;
                        db.SaveChanges();
                    }
                    else
                    {
                        entityUser.Photo.ImageUrl = oauthData.ImageUrl;
                    }
                    if (string.IsNullOrEmpty(entityUser.MedicalId))
                    {
                        entityUser.MedicalId = GenerateMedicalId(entityUser.UserId);
                    }
                    db.SaveChanges();
                    if (entityUser.Patient == null)
                    {
                        var entityPatient = db.Patients.Create();
                        entityPatient.UserId = entityUser.UserId;
                        db.Patients.Add(entityPatient);
                        db.SaveChanges();
                    }
                    ActivityAuditHelper.AddUserDataToLog(entityUser);

                    var entityDevice = DeviceService.GetEntityDevice(db, model.Device, AccessTokenType.Doc2Us);

                    db.SaveChanges();

                    // if user never login before, create user profile and oauth data
                    var entityOAuth = GetEntityOAuth(db, model.OAuthType, model.OAuthId, true);
                    if (entityOAuth == null)
                    {
                        entityOAuth = db.webpages_OAuthMembership.Create();
                        entityOAuth.Provider = model.OAuthType.ToString().ToLower();
                        entityOAuth.ProviderUserId = model.OAuthId;
                        entityOAuth.UserId = entityUser.UserId;
                        db.webpages_OAuthMembership.Add(entityOAuth);
                    }
                    entityUser = entityOAuth.UserProfile;

                    string accessToken = GetAccessToken(entityUser, entityDevice);
                    db.SaveChanges();
                    ActivityAuditHelper.AddDeviceDataToLog(entityDevice);

                    var result = new UserModel(entityUser);
                    result.Photo = new PhotoModel(entityPhoto);
                    result.AccessToken = accessToken;
                    transaction.Commit();

                    ActivityAuditHelper.LogEvent("Successful registration/login", ActivityAuditEvent.IntegratedRegisterLoginSuccess);
                    return result;
                }
            }
        }

        internal static Entity.webpages_OAuthMembership GetEntityOAuth(Entity.db_HeyDocEntities db, OAuthType provider, string providerUserId, bool nullable)
        {
            string providerStr = provider.ToString().ToLower();
            var entityOAuth = db.webpages_OAuthMembership.FirstOrDefault(e => e.Provider == providerStr && e.ProviderUserId == providerUserId);

            if (entityOAuth == null && !nullable)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.UserNotRegistered));
            }

            return entityOAuth;
        }

        private static OAuthData GetDataFromOAuthProvider(OAuthType oauthType, string oauthId, string oauthToken)
        {
            try
            {
                OAuthData oauthData = new OAuthData();

                switch (oauthType)
                {
                    case OAuthType.Facebook:
                        var fbClient = new Facebook.FacebookClient(oauthToken);
                        var parameters = new Dictionary<string, object>();
                        parameters["fields"] = "id,name,email";
                        dynamic fbUserData = fbClient.Get("me", parameters);
                        string fbId = fbUserData.id;
                        if (string.IsNullOrEmpty(fbId))
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.UnknownError, Account.ErrorFacebookLoginFailed));
                        }
                        if (fbId != oauthId)
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorFacebookLoginFailed));
                        }
                        string email = fbUserData.email;
                        if (!string.IsNullOrEmpty(email))
                        {
                            oauthData.Email = email;
                        }
                        else
                        {
                            oauthData.Email = oauthId + "@facebook.com";
                        }
                        string name = fbUserData.name;

                        if (!string.IsNullOrEmpty(name))
                        {
                            oauthData.Name = name;
                        }
                        oauthData.ImageUrl = string.Format("https://graph.facebook.com/{0}/picture/", fbId);
                        break;
                }

                return oauthData;
            }
            catch (WebApiException ex)
            {
                log.Error(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw new WebApiException(new WebApiError(WebApiErrorCode.UnknownError, Errors.GenericError));
            }
        }

        public static OAuthUserStatusModel GetOAuthUserStatus(string oAuthId, OAuthType provider)
        {
            using (var db = new Entity.db_HeyDocEntities())
            {
                var entityOAuth = GetEntityOAuth(db, provider, oAuthId, true);
                if (entityOAuth == null)
                {
                    return new OAuthUserStatusModel
                    {
                        IsExisting = false,
                        IsDeleted = false,
                        IsBanned = false
                    };
                }
                else
                {
                    var entityUser = entityOAuth.UserProfile;
                    return new OAuthUserStatusModel
                    {
                        IsExisting = true,
                        IsDeleted = entityUser.IsDelete,
                        IsBanned = entityUser.IsBan
                    };
                }
            }
        }

        public static bool GetDateOfBirthFromIC(string ic, out DateTime? dateOfBirth)
        {
            dateOfBirth = null;
            if (ic != null && ic.Length >= 12)
            {
                var charCount = Regex.Matches(ic, @"[a-zA-Z]").Count;
                if (charCount == 0)
                {
                    ic = ic.Replace("-", "");
                    string dateNumber = ic.Remove(6, ic.Length - 6).ToString();
                    string yearNumber = ic.Remove(1, ic.Length - 1).ToString();
                    if (yearNumber == "0" || yearNumber == "1" || yearNumber == "2" || yearNumber == "3" || yearNumber == "4")
                    {
                        dateNumber = "20" + dateNumber;
                    }
                    else
                    {
                        dateNumber = "19" + dateNumber;
                    }

                    bool validDate = DateTime.TryParseExact(dateNumber, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirthTemp);
                    if (validDate)
                    {
                        dateOfBirth = dateOfBirthTemp;
                    }
                    return validDate;
                }
            }

            return false;
        }

        public static void NotifyPasswordChanged(string email)
        {
            string subject = "HOPE: Password Changed";
            string content = EmailHelper.ConvertEmailHtmlToString(@"Emails\PasswordChanged.html");
            content = string.Format(content, ConstantHelper.Doc2UsEmailContact);

            EmailHelper.SendViaSendGrid(new List<string>() { email }, subject, content, string.Empty);
        }

        public static DoctorDashboardModel GetDoctorDashboard(string accessToken, DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null)
        {
            using (var db = new db_HeyDocEntities())
            {
                var entityUser = GetEntityUserByAccessToken(db, accessToken, false);
                var doctorRoles = ConstantHelper.DoctorRoles;
                if (!doctorRoles.Any(r => entityUser.Roles.Contains(r)))
                {
                    throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized));
                }

                var fromDateTime = (fromDate ?? DateTimeOffset.MinValue).UtcDateTime;
                var toDateTime = (toDate ?? DateTimeOffset.MaxValue).UtcDateTime;

                var query =
                    from doctor in db.Doctors
                    join userProfile in db.UserProfiles on doctor.UserId equals userProfile.UserId
                    join chatData in (
                        // average chat response time in seconds, number of chat requests, number of accepted chat requests
                        from res1 in db.ChatResponses
                        from res2 in db.ChatResponses
                            .Where(r => r.CreatedDate > res1.CreatedDate && r.ChatRoomId == res1.ChatRoomId)
                            .OrderBy(r => r.CreatedDate)
                            .Take(1)
                        where res1.RequestStatus == RequestStatus.Requested && res1.CreatedDate >= fromDateTime && res1.CreatedDate < toDateTime
                        select new
                        {
                            res1,
                            res2
                        } into temp
                        group temp by temp.res1.ChatRoom.DoctorId into tempResult
                        select new
                        {
                            UserId = tempResult.Key,
                            AverageChatResponseTime = tempResult.Average(e => DbFunctions.DiffSeconds(e.res1.CreatedDate, e.res2.CreatedDate)),
                            ChatRequests = tempResult.Count(),
                            AcceptChatRequests = tempResult.Count(e => e.res2.RequestStatus == RequestStatus.Accepted)
                        }
                    ) on doctor.UserId equals chatData.UserId into chatDataJoin
                    from chatDataSelect in chatDataJoin.DefaultIfEmpty()
                    join sessionDataFromChat in (
                        // number of patients seen
                        from chatroom in db.ChatRooms
                        where (
                            db.Chats.Any(c => c.ChatRoomId == chatroom.ChatRoomId && c.CreateDate >= fromDateTime && c.CreateDate < toDateTime) ||
                            db.ExternalVideoChatLogs.Any(log => log.DoctorId == chatroom.DoctorId && log.PatientId == chatroom.PatientId && log.CallDate >= fromDateTime && log.CallDate < toDateTime)
                        )
                        group chatroom by chatroom.DoctorId into tempResult
                        select new
                        {
                            UserId = tempResult.Key,
                            PatientsSeen = tempResult.Count()
                        }
                    ) on doctor.UserId equals sessionDataFromChat.UserId into sessionDataJoin
                    from sessionDataSelect in sessionDataJoin.DefaultIfEmpty()
                    join sessionDataFromEps in (
                        // number of patients seen (EPS)
                        from prescription in db.Prescriptions
                        where prescription.CreateDate >= fromDateTime && prescription.CreateDate < toDateTime && prescription.PrescribedBy.HasValue
                        group prescription.PatientId by prescription.DoctorId into tempResult
                        select new
                        {
                            UserId = tempResult.Key,
                            PatientsSeen = tempResult.Distinct().Count()
                        }
                    ) on doctor.UserId equals sessionDataFromEps.UserId into sessionEpsDataJoin
                    from sessionEpsDataSelect in sessionEpsDataJoin.DefaultIfEmpty()
                    join prescriptionData in (
                        // number of prescriptions (including EPS)
                        from prescription in db.Prescriptions
                        where prescription.CreateDate >= fromDateTime && prescription.CreateDate < toDateTime
                        group prescription by prescription.DoctorId into tempResult
                        select new
                        {
                            UserId = tempResult.Key,
                            NumberOfPrescriptions = tempResult.Count()
                        }
                    ) on doctor.UserId equals prescriptionData.UserId into prescriptionDataJoin
                    from prescriptionDataSelect in prescriptionDataJoin.DefaultIfEmpty()
                    join epsData in (
                        // average EPS response time
                        from prescription in db.Prescriptions
                        where prescription.CreateDate >= fromDateTime && prescription.CreateDate < toDateTime && prescription.PrescribedBy.HasValue && prescription.ApprovedDate.HasValue
                        group prescription by prescription.DoctorId into tempResult
                        select new
                        {
                            UserId = tempResult.Key,
                            AverageEpsResponseTime = tempResult.Average(r => DbFunctions.DiffSeconds(r.AssignedDate ?? r.CreateDate, r.ApprovedDate))
                        }
                    ) on doctor.UserId equals epsData.UserId into epsDataJoin
                    from epsDataSelect in epsDataJoin.DefaultIfEmpty()
                    join chatSessionData in (
                        // average chat session duration
                        from res1 in db.ChatResponses
                        where res1.RequestStatus == RequestStatus.Accepted && res1.CreatedDate >= fromDateTime && res1.CreatedDate < toDateTime
                        from res2 in db.ChatResponses
                            .Where(r => r.CreatedDate > res1.CreatedDate && r.ChatRoomId == res1.ChatRoomId && r.RequestStatus == RequestStatus.Completed)
                            .OrderBy(r => r.CreatedDate)
                            .Take(1)
                        select new
                        {
                            res1,
                            res2
                        } into temp
                        group temp by temp.res1.ChatRoom.DoctorId into tempResult
                        select new
                        {
                            UserId = tempResult.Key,
                            AverageChatSessionTime = tempResult.Average(r => DbFunctions.DiffSeconds(r.res1.CreatedDate, r.res2.CreatedDate))
                        }
                    ) on doctor.UserId equals chatSessionData.UserId into chatSessionDataJoin
                    from chatSessionDataSelect in chatSessionDataJoin.DefaultIfEmpty()
                    join logData in (
                        // total online time
                        from log1 in db.OnlineStatusLogs
                        from log2 in db.OnlineStatusLogs
                            .Where(r => r.LogDate > log1.LogDate && r.DoctorId == log1.DoctorId)
                            .OrderBy(r => r.LogDate)
                            .Take(1)
                        where log1.OnlineStatus == true && log1.LogDate >= fromDateTime && log1.LogDate < toDateTime
                        select new
                        {
                            log1,
                            log2
                        } into temp
                        group temp by temp.log1.DoctorId into tempResult
                        select new
                        {
                            UserId = tempResult.Key,
                            OnlineTime = tempResult.Sum(r => DbFunctions.DiffSeconds(r.log1.LogDate, r.log2.LogDate) ?? 0)
                        }
                    ) on doctor.UserId equals logData.UserId into logDataJoin
                    from logDataSelect in logDataJoin.DefaultIfEmpty()
                    join ratingData in (
                        // average rating, total number of ratings
                        from review in db.DoctorUserReviews
                        where review.CreateDate >= fromDateTime && review.CreateDate < toDateTime
                        group review by review.DoctorId into tempResult
                        select new
                        {
                            UserId = tempResult.Key,
                            AverageRating = tempResult.Average(r => r.Rating),
                            NumberOfRatings = tempResult.Count()
                        }
                    ) on doctor.UserId equals ratingData.UserId into ratingDataJoin
                    from ratingDataSelect in ratingDataJoin.DefaultIfEmpty()
                    where doctor.UserId == entityUser.UserId
                    orderby doctor.UserProfile.FullName
                    select new DoctorDashboardModel
                    {
                        ChatAverageResponseTimeInSeconds = chatDataSelect == null ? 0 : (chatDataSelect.AverageChatResponseTime ?? 0),
                        RequestTotalCount = chatDataSelect == null ? 0 : chatDataSelect.ChatRequests,
                        RequestAcceptedCount = chatDataSelect == null ? 0 : chatDataSelect.AcceptChatRequests,
                        PatientCount = (sessionDataSelect == null ? 0 : sessionDataSelect.PatientsSeen) + (sessionEpsDataSelect == null ? 0 : sessionEpsDataSelect.PatientsSeen),
                        PrescriptionCreatedCount = prescriptionDataSelect == null ? 0 : prescriptionDataSelect.NumberOfPrescriptions,
                        EpsAverageResponseTimeInSeconds = epsDataSelect == null ? 0 : (epsDataSelect.AverageEpsResponseTime ?? 0),
                        ChatSessionAverageDurationInSeconds = chatSessionDataSelect == null ? 0 : (chatSessionDataSelect.AverageChatSessionTime ?? 0),
                        TotalOnlineTimeInSeconds = logDataSelect == null ? 0 : logDataSelect.OnlineTime,
                        AverageRating = ratingDataSelect == null ? 0 : ratingDataSelect.AverageRating,
                        TotalRatingCount = ratingDataSelect == null ? 0 : ratingDataSelect.NumberOfRatings
                    };

                var model = query.FirstOrDefault();
                model.LatestRatingPreviews = db.DoctorUserReviews
                    .Where(r => r.DoctorId == entityUser.UserId && r.CreateDate >= fromDateTime && r.CreateDate < toDateTime)
                    .OrderByDescending(r => r.CreateDate)
                    .Take(5)
                    .ToList()
                    .Select(r => new ReviewModel(r))
                    .ToList();


                model.PointEarned = (from prequest in db.PaymentRequests
                                    join chatroom in db.ChatRooms on prequest.ChatRoomId equals chatroom.ChatRoomId
                                    where chatroom.DoctorId == entityUser.UserId && prequest.PaymentStatus == PaymentStatus.Paid && prequest.CreateDate >= fromDateTime && prequest.CreateDate <= toDateTime
                                    select (Decimal?) prequest.Amount).Sum() ?? 0;
                return model;
            }
        }

        public static void InvalidateAccessTokens(db_HeyDocEntities db, ICollection<Device> entityDevice)
        {
            foreach (var device in entityDevice)
            {
                device.AccessToken = null;
                device.UserId = null;
            }
            db.SaveChanges();
        }

        #region internal Methods
        internal static Entity.UserProfile GetEntityUserByAccessToken(Entity.db_HeyDocEntities db, string accessToken, bool nullable = false, RoleType? isInRole = null)
        {
            var oldUseDatabaseNullSemantics = db.Configuration.UseDatabaseNullSemantics;
            try
            {
                // Prevent generation of extra null checks in the query which serve the purpose of allowing null accessToken
                // to return database rows where AccessToken is null since NULL = NULL in SQL is NULL, but in this case accessToken
                // can be assumed to be non-null and the extra null checks in the query cause a performance issue as it may cause
                // the database to not use the index for the AccessToken column
                db.Configuration.UseDatabaseNullSemantics = true;
                 var entityDevice = db.Devices
                    .Include(e => e.UserProfile)
                    .Include(e => e.UserProfile.webpages_Roles)
                    .Include(e => e.UserProfile.Doctor)
                    .FirstOrDefault(e => e.AccessToken == accessToken && e.UserId != null);

                ActivityAuditHelper.AddRequestDataToLog();
                if (entityDevice == null && !nullable)
                {
                    ActivityAuditHelper.LogEvent("Invalid access token used", ActivityAuditEvent.ApiCallFail);
                    throw new WebApiException(new WebApiError(WebApiErrorCode.  NotFound, Account.ErrorSessionExpired));
                }
                Entity.UserProfile entityUser = null;

                if (entityDevice != null)
                {
                    ActivityAuditHelper.AddDeviceDataToLog(entityDevice);
                    ActivityAuditHelper.AddUserDataToLog(entityDevice.UserProfile);

                    var now = DateTime.UtcNow;

                    DateTime tokenExpiryTime;
                    string tokenExpiredMessage;
                    tokenExpiryTime = now.AddDays(-60);
                    tokenExpiredMessage = Account.ErrorSessionExpired;
                    if (entityDevice.LastActivityDate < tokenExpiryTime)
                    {
                        entityDevice.UserId = null;
                        entityDevice.AccessToken = null;
                        db.SaveChanges();
                        ActivityAuditHelper.LogEvent("Expired access token used", ActivityAuditEvent.ApiCallFail);
                        if (nullable)
                        {
                            return null;
                        }
                        else
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.AccessTokenNotFound, tokenExpiredMessage));
                        }
                    }

                    entityUser = entityDevice.UserProfile;
                    if (entityUser.IsBan)
                    {
                        ActivityAuditHelper.LogEvent("Banned user called API", ActivityAuditEvent.ApiCallFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.UserIsBanned, Account.ErrorBanned));
                    }
                    if (entityUser.IsDelete)
                    {
                        ActivityAuditHelper.LogEvent("Deleted user called API", ActivityAuditEvent.ApiCallFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.UserIsBanned, Account.ErrorUserDeleted));
                    }

                    var entityCorporate = entityUser.Corporate;
                    if (entityCorporate != null && entityCorporate.IsBan)
                    {
                        InvalidateAccessTokens(db, entityUser.Devices);
                        ActivityAuditHelper.LogEvent("User from banned corporate called API", ActivityAuditEvent.ApiCallFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.UserIsBanned, Account.ErrorBanned));
                    }

                    entityDevice.LastActivityDate = now;
                    entityUser.LastActivityDate = now;
                    db.SaveChanges();

                    if (isInRole.HasValue && entityUser.Role != isInRole)
                    {
                        ActivityAuditHelper.LogEvent($"Unauthorised user called API (expected role: {isInRole})", ActivityAuditEvent.ApiCallFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, string.Format(Errors.UnauthorizedRole, ConstantHelper.Doc2UsEmailContact)));
                    }
                    if (entityUser.Role == RoleType.Doctor && !entityUser.Doctor.IsVerified)
                    {
                        ActivityAuditHelper.LogEvent("Unverified doctor called API", ActivityAuditEvent.ApiCallFail);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.Unauthorized, Account.ErrorHCPNotVerified));
                    }
                    ActivityAuditHelper.LogEvent("Successful API call", ActivityAuditEvent.ApiCallSuccess);
                }
                else
                {
                    ActivityAuditHelper.LogEvent("Successful anonymous API call", ActivityAuditEvent.ApiCallSuccess);
                }
                return entityUser;
            }
            finally
            {
                db.Configuration.UseDatabaseNullSemantics = oldUseDatabaseNullSemantics;
            }
        }

        internal static Entity.UserProfile GetEntityTargetUserByUsername(Entity.db_HeyDocEntities db, string username, bool nullable, RegisterModel model)
        {
            model.CompanyId = model.CompanyId ?? 1;
            var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserName == username && !e.IsDelete && e.CompanyId == model.CompanyId);

            if (entityUser == null && !nullable)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Account.ErrorEmailNotRegistered));
            }

            return entityUser;
        }
        internal static Entity.UserProfile GetEntityTargetUserByPhoneNumber(Entity.db_HeyDocEntities db, string phone, bool nullable, RegisterModel model)
        {
            var entityUser = db.UserProfiles.FirstOrDefault(e => e.PhoneNumber == phone && !e.IsDelete && e.CompanyId == model.CompanyId);

            if (entityUser == null && !nullable)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Account.ErrorEmailNotRegistered));
            }

            return entityUser;
        }
        internal static Entity.UserProfile GetEntityUserByUserId(Entity.db_HeyDocEntities db, string userId, bool nullable = false, bool banable = false)
        {
            int id = int.Parse(userId);
            var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserId == id);

            if (entityUser == null && !nullable)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Account.ErrorUserNotFound));
            }

            if (entityUser != null)
            {
                DateTime now = DateTime.UtcNow;
                //entityUser.LastActivityDate = now;
                if (!banable)
                {
                    if (entityUser.Corporate != null && entityUser.Corporate.IsBan)
                    {
                        InvalidateAccessTokens(db, entityUser.Devices);
                        throw new WebApiException(new WebApiError(WebApiErrorCode.UserIsBanned, Account.ErrorBanned));
                    }

                    if (entityUser.IsBan)
                    {
                        throw new WebApiException(new WebApiError(WebApiErrorCode.UserIsBanned, Account.ErrorBanned));
                    }
                }
            }

            return entityUser;
        }

        internal static Entity.UserProfile GetEntityTargetUserByUserId(Entity.db_HeyDocEntities db, int targetUserId, bool nullable)
        {
            var entityUser = db.UserProfiles.FirstOrDefault(e => e.UserId == targetUserId && !e.IsDelete);

            if (entityUser == null && !nullable)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.NotFound, Account.ErrorUserNotFound));
            }
            return entityUser;
        }
        #endregion

        #region Private Methods
        private static BoolResult SendVerificationEmail(string email, string confirmationToken, string nickName = "")
        {
            try
            {
                string verifyLink = ConstantHelper.ServerUrl + @"/Account/Confirmation/" + confirmationToken;
                string subject = "HOPE: Verify your email address";
                string content = EmailHelper.ConvertEmailHtmlToString(@"Emails\VerificationEmail.html");
                content = string.Format(content, verifyLink, email, nickName);

                EmailHelper.SendViaSendGrid(new List<string>() { email }, subject, content, string.Empty);
                return new BoolResult(true);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new BoolResult(false);
            }
        }

        private static bool SendForgotPasswordEmail(string email, string resetToken)
        {
            try
            {
                //string content = EmailHelper.ConvertEmailHtmlToString(@"Emails\ResetEmail.html");
                string content = "";
                string resetLink = ConstantHelper.ServerUrl + @"/Account/Reset/" + resetToken;

              //  if (System.Web.HttpContext.Current.Request.Url.ToString().Contains("localhost"))
              //  {
                 //   content = EmailHelper.ConvertEmailHtmlToString(@"Emails\ResetEmail.html");
              //  }

              //  if (System.Web.HttpContext.Current.Request.Url.ToString().Contains("hopestaging"))
              //  {
                    content = EmailHelper.ConvertEmailHtmlToString(@"Emails\ResetEmail.html");
              //  }

                content = string.Format(content, resetLink);
#if DEBUG
                string subject = "HOPE Staging: Reset Password";
                var bccList = CloudConfigurationManager.GetSetting("Doc2UsDevEmails").Split(',').ToList();
#else
                string subject = "Doc2Us: Reset Password";
                // TODO M UNBLANK: Admin email as recepient
                var bccList = new List<string>
                {
                    "",
                };
#endif

                EmailHelper.SendViaSendGrid(new List<string>() { email }, subject, content, string.Empty, bccEmails: bccList, allowDoc2Us: true);
                //EmailHelper.SendViaSmtp(new List<string>() { email }, subject, content, "", bccList);
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }
        private static bool SendForgotPasswordPhone(string phone, string resetToken)
        {
            try
            {
                string resetLink = ConstantHelper.ServerUrl + @"/Account/Reset/" + resetToken;
                string content = "Password reset process for your HOPE Account, click the link below within 24 hours: "+ resetLink;

                SMSHelper.SMSReqModel sMSReqModel = new SMSHelper.SMSReqModel();
                sMSReqModel.text = content;
                sMSReqModel.to = phone;
                sMSReqModel.from = "HOPE";
                var smsResponseModel = SMSHelper.SMSSend(sMSReqModel);
                try
                {
                    if (smsResponseModel.status == 200)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (DbEntityValidationException e)
                {
                    throw;
                }                
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }
        /**
         * <summary>Create or retrieve an access token for a user on a device. Make sure to call SaveChanges on the database context after this function to persist the potentially new token</summary>
         * <param name="forceRegenerate">If true, will always regenerate the accessToken even if there is an existing one for the requested user and device</param>
         */
        private static string GetAccessToken(Entity.UserProfile entityUser, Entity.Device entityDevice, bool forceRegenerate = false)
        {
            // logout previous user
            if (forceRegenerate || (entityDevice.UserId.HasValue && entityDevice.UserId.Value != entityUser.UserId))
            {
                entityDevice.UserId = null;
                entityDevice.AccessToken = null;
            }
            if (entityDevice.UserId == null)
            {
                entityDevice.UserId = entityUser.UserId;
                entityDevice.AccessToken = GenerateAccessToken(entityUser.UserId, entityDevice.DeviceType, entityDevice.DeviceId, entityDevice.TokenType);
            }
            return entityDevice.AccessToken;
        }

        private static string GenerateAccessToken(int userId, DeviceType deviceType, string deviceId, AccessTokenType tokenType)
        {
            // raw data that will be hash to access token
            var rawData = $"{userId}-{deviceType}-{deviceId}-{tokenType}-{DateTime.UtcNow}";
            // convert raw data to bytes
            return HashBySHA512(rawData);
        }

        //public static string GetToken()
        //{
        //    string secretkey = "asdf_JKL_123_$%^";
        //    var issuer = "http://mysite.com";
        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey));
        //    //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("asdf_JKL_123_$%^"));
        //    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //    var permClaims = new List<Claim>();
        //    permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        //    permClaims.Add(new Claim("valid", "1"));
        //    permClaims.Add(new Claim("phone", "09964677559"));

        //    var token = new JwtSecurityToken(issuer,
        //        issuer,
        //        permClaims,
        //        expires: DateTime.Now.AddDays(1),
        //        signingCredentials: credentials);
        //    string jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
        //    //return new { data = jwt_token };
        //    return jwt_token;
        //}

        private static string HashBySHA512(string input)
        {
            SHA512 shaM = new SHA512Managed();
            byte[] data = Encoding.ASCII.GetBytes(input);
            byte[] result = shaM.ComputeHash(data);
            return Convert.ToBase64String(result).Replace("=", "").Replace('+', '-').Replace('/', '_');
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Phone Number or Email already exists. Please enter a different phone/email.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "Phone Number or Email already exists. Please enter a different phone/email.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The email provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        public static bool AddUserToRole(Entity.db_HeyDocEntities db, Entity.UserProfile entityUser, RoleType role)
        {
            string roleStr = role.ToString();
            var entityRole = db.webpages_Roles.FirstOrDefault(e => e.RoleName == roleStr);
            if (entityRole != null && !entityUser.webpages_Roles.Any(e => e.RoleId == entityRole.RoleId))
            {
                entityUser.webpages_Roles.Add(entityRole);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
