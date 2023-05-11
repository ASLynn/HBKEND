using HeyDoc.Web.Resources;
using HeyDoc.Web.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Nickname { get; set; }
        public Nullable<HeyDoc.Web.Gender> Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public Nullable<long> PhotoId { get; set; }
        public Nullable<int> CountryId { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public bool IsDelete { get; set; }
        public bool IsBan { get; set; }
        public bool IsOnline { get; set; }
        public string Language { get; set; }
        public string AccessToken { get; set; }

        // This model is used for API responses, so all consumers have to be changed before it can be considered for removal
        [Obsolete("Use Roles instead to support multiple user roles")]
        public RoleType Role { get; set; }
        public List<RoleType> Roles { get; set; }
        public PhotoModel Photo { get; set; }
        public CountryModel Country { get; set; }
        public string MedicalId { get; set; }
        public bool IsPartner { get; set; }

        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string IC { get; set; }

        public bool IsCorporate { get; set; }
        public int CorporateId { get; set; }
        public string PositionId { get; set; }     
        public string PositionName { get; set; }
        public string CorporateName { get; set; }
        public string BranchAdress { get; set; }
        public string BranchName { get; set; }
        public string CorporateTPAName { get; set; }

        //public string BranchAddress { get; set; }
        // 2020-06-11 Possibly make IsDependent nullable/remove it in future as registration and db has moved to using only CorporateUserType
        public bool IsDependent { get; set; }
        public string EmployeeDependantName { get; set; }
        public string EmployeeDependantIC { get; set; }
        public int BranchId { get; set; }
        public bool IsVerified { get; set; }
        public CorporateUserType CorporateUserType { get; set; }
        public string ReferrerName { get; set; }
        public long? GroupId { get; set; } // manually included in AccountService/Login
        public PrescriptionSourceModel PrescriptionSource { get; set; }
        public string CreatedSourceName { get; internal set; }

        public int? StateId { get; set; }
        public int? TownshipId { get; set; }
        public string BloodType { get; set; }
        public StateModel State { get; set; }
        public TownshipModel Township { get; set; }
        public string ErrorMessage { get; set; }
        public PatientExtraModel PatientExtra { get; set; }
        public string cloneOrWhite { get; set; }
        public UserModel()
        {

        }

        public UserModel(Entity.UserProfile entityUser, bool isNotDoctor = false)
        {
            CompanyId = entityUser.CompanyId ?? 1;
            UserId = entityUser.UserId;
            UserName = entityUser.UserName;
            // 2020-12-23 Fallback to FullName is mainly to fix mobile showing "null" in some cases like patient names in doctor chatrooms
            // Can potentially remove it in future as a fix has also been applied on mobile side
            Nickname = string.IsNullOrEmpty(entityUser.Nickname) ? entityUser.FullName : entityUser.Nickname;
            Gender = entityUser.Gender;
            Birthday = entityUser.Birthday;
            if(entityUser.CompanyId != null)
            {
                cloneOrWhite = entityUser.CompanyWhiteLabel.CompanyType;
            }
            if (entityUser.ReferrerId != null)
            {
                ReferrerName = entityUser.UserReferralCode.ReferrerName;
            }
            if (entityUser.Photo != null)
            {
                Photo = new PhotoModel(entityUser.Photo);
            }
            if (entityUser.Country != null)
            {
                Country = new CountryModel(entityUser.Country);
            }
            if (entityUser.StateId != null)
            {
                StateId = entityUser.StateId;
                State = new StateModel(entityUser.State);
            }
            if (entityUser.Township != null)
            {
                TownshipId = entityUser.TownshipId;
                Township = new TownshipModel(entityUser.Township);
            }
            if (entityUser.Patient != null)
            {
                if(entityUser.Patient.PatientExtras != null)
                {
                    foreach (var item in entityUser.Patient.PatientExtras)
                    {
                        PatientExtra = new PatientExtraModel(item);
                    }
                }
               
            }
            Language = entityUser.Language;
            if (entityUser.LastActivityDate.HasValue)
            {
                LastActivityDate = entityUser.LastActivityDate.Value;
            }
            LastUpdateDate = entityUser.LastUpdateDate;
            IsBan = entityUser.IsBan;
            IsDelete = entityUser.IsDelete;
            IsOnline = entityUser.IsOnline;
            Role = entityUser.Role;
            Roles = entityUser.Roles;
            MedicalId = entityUser.MedicalId;
            Address = entityUser.Address;
            IC = entityUser.IC;
            PhoneNumber = entityUser.PhoneNumber;
            IsPartner = false;
            FullName = string.IsNullOrEmpty(entityUser.FullName) ? entityUser.Nickname : entityUser.FullName;
            BloodType = entityUser.BloodType;
            IsVerified = true;
            if (entityUser.webpages_Membership != null && entityUser.webpages_Membership.IsConfirmed.HasValue)
            {
                IsVerified = entityUser.webpages_Membership.IsConfirmed.Value;
            }

            if (!string.IsNullOrEmpty(entityUser.Title))
            {

                FullName = string.Format("{0} {1}", entityUser.Title, entityUser.FullName);
            }
            Title = entityUser.Title;
            if (!isNotDoctor && entityUser.Doctor != null)
            {
                IsPartner = entityUser.Doctor.IsPartner;
            }
            CreateDate = entityUser.CreateDate;

            IsCorporate = false;
            if (entityUser.CorporateId.HasValue)
            {
                IsCorporate = true;
                var userCorporate = entityUser.UserCorperates.FirstOrDefault();

                CorporateId = entityUser.CorporateId.Value;
                if(entityUser.IsDependent != null)
                {
                    IsDependent = entityUser.IsDependent ?? userCorporate.CorporateUserType == CorporateUserType.EmployeeDependants || userCorporate.CorporateUserType == CorporateUserType.EmployeeChild;
                }
                CorporateName = entityUser.Corporate.BranchName;
                if(userCorporate.PositionId != null)
                {
                    PositionId = userCorporate.PositionId.ToString();
                    PositionName = entityUser.Corporate.CorporatePositions.FirstOrDefault().Position.ToString();
                }
               
                BranchId = userCorporate.BranchId;
                BranchAdress = userCorporate.Branch.BranchAddress;
                BranchName = userCorporate.Branch.BranchName;
                CorporateTPAName = userCorporate.Corporate.ThirdPartyAdministrator?.Name;

                if (userCorporate.CorporateUserType == CorporateUserType.EmployeeDependants || userCorporate.CorporateUserType == CorporateUserType.EmployeeChild)
                {
                    EmployeeDependantName = userCorporate.EmployeeDependant ?? "";
                    EmployeeDependantIC = userCorporate.EmployeeDependantIC ?? "";
                    CorporateUserType = userCorporate.CorporateUserType;
                }
                else
                {
                    CorporateUserType = userCorporate.CorporateUserType;
                }
            }

            if (entityUser.PrescriptionSourceId.HasValue)
            {
                PrescriptionSource = new PrescriptionSourceModel(entityUser.PrescriptionSource);
            }
        }
    }

    public class UserSimpleModel
    {
        public int UserId { get; set; }
        public string Nickname { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }

        // This model is used for API responses, so all consumers have to be changed before it can be considered for removal
        [Obsolete("Use Roles instead to support multiple user roles")]
        public RoleType Role { get; set; }
        public List<RoleType> Roles { get; set; }
        public PhotoModel Photo { get; set; }
        public PrescriptionSourceModel PrescriptionSource { get; set; }
        public UserSimpleModel() { }
        public UserSimpleModel(Entity.UserProfile entityUser)
        {
            UserId = entityUser.UserId;
            Nickname = entityUser.Nickname;
            FullName = string.IsNullOrEmpty(entityUser.FullName) ? entityUser.Nickname : entityUser.FullName;
            Photo = new PhotoModel(entityUser.Photo);
            Title = entityUser.Title;
            Role = entityUser.Role;
            Roles = entityUser.Roles;
            if (entityUser.PrescriptionSourceId.HasValue)
            {
                PrescriptionSource = new PrescriptionSourceModel(entityUser.PrescriptionSource);
            }
        }

        public UserSimpleModel(Entity.Doctor entityHCP)
        {
            UserId = entityHCP.UserId;
            FullName = entityHCP.UserProfile.FullName;
        }
    }
    public class ChangePasswordModel
    {
        public string OldPassword
        {
            get { return _OldPasswordModel.Password; }
            set { _OldPasswordModel.Password = value; }
        }
        public string NewPassword
        {
            get { return _NewPasswordModel.Password; }
            set { _NewPasswordModel.Password = value; }
        }
        private PasswordModel _OldPasswordModel { get; set; } = new PasswordModel("");
        private PasswordModel _NewPasswordModel { get; set; } = new PasswordModel("");

        public void Validate()
        {
            _NewPasswordModel.Validate();
        }
    }

    public class PharmacyOutletModel
    {
        public int UserId { get; set; }
        public string OutletName { get; set; }
        public string OutletAddress { get; set; }
        public string PhoneNumber { get; set; }

        public PharmacyOutletModel()
        {

        }

        public PharmacyOutletModel(Entity.UserProfile entityOutlet)
        {
            UserId = entityOutlet.UserId;
            OutletName = entityOutlet.FullName;
            OutletAddress = entityOutlet.Address;
            PhoneNumber = entityOutlet.PhoneNumber;
        }
    }
}
