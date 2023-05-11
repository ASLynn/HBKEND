using HeyDoc.Web.Entity;
using HeyDoc.Web.Helpers;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using HeyDoc.Web.Lib;
using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.InkML;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;

namespace HeyDoc.Web.Controllers.Api
{
    public class AccountController : ApiController
    {
        //[HttpPost]
        //public UserModel Login(EmailLoginModel model)
        //{
        //    return WebApiWrapper.Call<UserModel>(e => AccountService.Login(model));
        //}

        [HttpPost]
        public Object GetToken(string phone)
        {
            string pw = "asdfJKL123$%^";
            string secretkey = "asdf_JKL_123_$%^";
            var issuer = "http://mysite.com";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey));
            //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("asdf_JKL_123_$%^"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var permClaims = new List<Claim>();
            permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            permClaims.Add(new Claim("phone", phone));
            permClaims.Add(new Claim("valid", pw));

            var token = new JwtSecurityToken(issuer,
                issuer,
                permClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);
            var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
            return new { data = jwt_token };
        }

        [HttpPost]
        public UserModel LoginPWA(String accessToken, int deviceType)
        {
            return WebApiWrapper.Call<UserModel>(e => AccountService.LoginAPIPWA(accessToken, deviceType));
        }

        [HttpPost]
        public UserModel LoginV2(EmailLoginModel model)
        {
            return WebApiWrapper.Call<UserModel>(e => AccountService.LoginAPIV2(model));
        }
        [HttpPost]
        public bool VerifyPhoneWithOTP(UserExtraModel model)
        {
            return WebApiWrapper.Call<bool>(e => AccountService.VerifyPhoneWithOTP(model));
        }
        [HttpPost]
        public UserModel LoginOAuth(OAuthLoginModel model)
        {
            return WebApiWrapper.Call<UserModel>(e => AccountService.LoginOAuth(model));
        }

        [HttpPost]
        public BoolResult Logout([ValueProvider(new [] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<BoolResult>(e => AccountService.Logout(accessToken));
        }

        //[HttpPost]
        //public UserModel Register(RegisterModel model)
        //{
        //    return WebApiWrapper.Call<UserModel>(e => AccountService.RegisterEmail(model));
        //}
      
        [HttpGet]
        public CorporateSignUpOptions GetCorporateSignUpOptions(Guid? referralCode = null)
        {
            return WebApiWrapper.Call(e => AccountService.GetCorporateSignUpOptions(referralCode));
        }
        [HttpGet]
        public List<RelationshipModel> GetRelationship()
        {
            return WebApiWrapper.Call(e => RelationshipService.GetRelationshipList());
        }
        [HttpGet]
        public PublicReferralSignUpResources GetReferralSignUpResources(Guid referralCode)
        {
            return WebApiWrapper.Call(e => AccountService.GetPublicReferralSignUpResources(referralCode));
        }

        [HttpPost]
        public PhotoModel Photo([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<PhotoModel>(e => AccountService.UploadPhoto(accessToken, Request.Content));
        }

        [HttpPut]
        public UserModel Update([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, UserModel model)
        {
            return WebApiWrapper.Call<UserModel>(e => AccountService.UpdateProfile(accessToken, model));
        }

        [HttpGet]
        public UserModel View([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int? userId = null)
        {
            UserModel um = new UserModel();
            um = AccountService.ViewProfile(accessToken, userId);
            return WebApiWrapper.Call<UserModel>(e => um);
        }

        [HttpGet]
        public BoolResult Resend(string email)
        {
            return WebApiWrapper.Call<BoolResult>(e => AccountService.ResendVerificationEmail(email));
        }

        [HttpGet]
        public BoolResult ForgotPassword(string email)
        {
            return WebApiWrapper.Call<BoolResult>(e => AccountService.ForgotPasswordBoth(email));
        }
        [HttpGet]
        public BoolResult ForgotPasswordWithPhone(string phone)
        {
            return WebApiWrapper.Call<BoolResult>(e => AccountService.ForgotPasswordBoth(phone));
        }
        [HttpPut]
        public BoolResult Set([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, bool isOnline)
        {
            return WebApiWrapper.Call<BoolResult>(e => AccountService.SetOnlineStatus(accessToken, isOnline));
        }

        [HttpDelete]
        public BoolResult Deactivate([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<BoolResult>(e => AccountService.DeactivateAcc(accessToken));
        }

        [HttpPost]
        public BoolResult ChangePassword([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, ChangePasswordModel model)
        {
            return WebApiWrapper.Call<BoolResult>(e => AccountService.ChangePassword(accessToken, model));
        }

        //Will Remove create doctor
        //[HttpPost]
        //public UserModel Create(RegisterModel model)
        //{
        //    return WebApiWrapper.Call<UserModel>(e => AccountService.CreateUser(model, RoleType.Doctor, false));
        //}
        [HttpPost]
        public UserModel UserFromWallet(RegisterModel model)
        {
            return WebApiWrapper.Call<UserModel>(e => AccountService.UserUidPhNumFromWallet(model));
        }
        [HttpPost]
        public UserModel RegisterV2(RegisterModel model)
        {
            return WebApiWrapper.Call<UserModel>(e => AccountService.RegisterEmailAPIV2(model));
        }
        [HttpPost]
        public UserModel RegisterPWA(string merchantUid, string PhoneNum)
        {
            return WebApiWrapper.Call<UserModel>(e => AccountService.RegisterPWAAPI(merchantUid, PhoneNum));
        }
        [HttpPost]
        public UserModel CreateDoctorV2(RegisterModel model)
        {
            var request = HttpContext.Current.Request;
            DoctorController dc = new DoctorController();
           
            return WebApiWrapper.Call<UserModel>(e => dc.CreateDoctorAPI(model,request));
        }      
        [HttpGet]
        public SimpleIdModel GetUserIdByMedicalId(string MedicalId, [ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken)
        {
            return WebApiWrapper.Call<SimpleIdModel>(e => AccountService.GetUserIdByMedicalId(MedicalId, accessToken));
        }
        [HttpGet]
        public List<ScanUserModel> GetScannedList([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int skip=0, int take=15)
        {
            return WebApiWrapper.Call<List<ScanUserModel>>(e => AccountService.GetScannedList(accessToken,skip,take));
        }
        
        [HttpGet]
        public OAuthUserStatusModel GetOAuthUserStatus(string oAuthId, OAuthType provider)
        {
            return WebApiWrapper.Call(_ => AccountService.GetOAuthUserStatus(oAuthId, provider));
        }

        public DoctorDashboardModel GetDoctorDashboard([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null)
        {
            return WebApiWrapper.Call(e => AccountService.GetDoctorDashboard(accessToken, fromDate, toDate));
        }

        [HttpPost]
        public string ResendOTP(string email)
        {
            return WebApiWrapper.Call<string>(e => AccountService.ResendOTP(email));
        }
        [HttpPost]
        public Boolean VerifyPhoneWithOTPResend(string email,string otpcode)
        {
            return WebApiWrapper.Call<Boolean>(e => AccountService.Otpverify(email,otpcode));
        }
        [HttpPost]
        public int Otpverify(OTPverifyModel model)
        {
            return WebApiWrapper.Call<int>(e => AccountService.Otpverify(model));
        }
        [HttpPost]
        public int Otpresend(OTPverifyModel model)
        {
            return WebApiWrapper.Call<int>(e => AccountService.ResendOTP(model));
        }        
    }
}
