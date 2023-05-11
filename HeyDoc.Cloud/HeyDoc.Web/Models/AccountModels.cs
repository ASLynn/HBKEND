using HeyDoc.Web.Helpers;
using HeyDoc.Web.Resources;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HeyDoc.Web.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
    public class ImportedCorporateUser
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
    public class RegisterModel
    {
        public IEnumerable<ImportedCorporateUser> List { get; set; }
        public DeviceModel Device { get; set; }

        
        [Display(Name = "User name")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password
        {
            get { return _PasswordModel.Password; }
            set { _PasswordModel.Password = value; }
        }
        private PasswordModel _PasswordModel { get; set; } = new PasswordModel("");

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword
        {
            get { return _ConfirmPasswordModel.Password; }
            set { _ConfirmPasswordModel.Password = value; }
        }
        private PasswordModel _ConfirmPasswordModel { get; set; } = new PasswordModel("");

        public string Nickname { get; set; }
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public string Title { get; set; }
        public string BloodType { get; set; }
        public Gender? Gender { get; set; }
        public RoleType RoleType { get; set; }
        public int? PrescriptionSourceId { get; set; }

        public Nullable<DateTime> Birthday { get; set; }

        public int CountryId { get; set; }
        public int DoctorId { get; set; }
        public bool IsPartner { get; set; }

        public string InvitationCode { get; set; }

        public string Language { get; set; }

        public string Specialty { get; set; }

    
        public IEnumerable<int> SpecialityId { get; set; }


        [Display(Name = "Praciticing Since")]
        public DateTime Practicing { get; set; }

        [Display(Name = "Medical School")]
        public string MedicalSch { get; set; }

        [Display(Name = "About Me")]
        public string AboutMe { get; set; }

        [Required]
        public string IC { get; set; }

        [Display(Name = "Practicing Address")]
        public string Address { get; set; }
        [Display(Name = "Clinic")]
        public string HospitalName { get; set; }
        public int? CategoryId { get; set; }
        public int? GroupId { get; set; }
        [Display(Name = "Contact No.")]
        [MaxLength(20, ErrorMessage = "The contact number must not exceed 20 digits.")]
        public string PhoneNumber { get; set; }
        public string Qualification { get; set; }


        public IEnumerable<int> QualificationId { get; set; }

        [Display(Name = "Reg. No:")]
        public string RegisterNumber { get; set; }

        public bool? IsDependent { get; set; }
        public int? CorporateId { get; set; }
        public int? BranchId { get; set; }
        public string EmployeeDependantName { get; set; }
        public string EmployeeDependantIC { get; set; }
        public CorporateUserType CorporateUserType { get; set; }
        public string SignUpPurpose { get; set; }
        public Guid? ReferralCode { get; set; }

        public IEnumerable<int> FacilitiesId { get; set; }
    
 
        public int StateId { get; set; }
        public int TownshipId { get; set; }
        public string CorporateSecret { get; set; }
        public string CorporatePositionId { get; set; }
        public bool ShowInApp { get; set; }
        [Display(Name = "Can Approve EPS")]
        public bool CanApproveEPS { get; set; }
        [Display(Name = "Enable Chat Assistant")]
        public bool IsChatBotEnabled { get; set; }
        public string VideoChatURL { get; set; }
        public bool createUserisAdmin { get; set; }

        public int? CompanyId { get; set; }
        public void Validate()
        {
            Device.Validate();

            //if (string.IsNullOrEmpty(Email))
            //{
            //    throw new WebApiException(
            //        new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorEmailNull));
            //}
            if (Email.Length > 450)
            {
                throw new WebApiException(
                        new WebApiError(WebApiErrorCode.InvalidArguments, Account.ErrorEmailTooLong));
            }
            _PasswordModel.Validate();
        }

        public void CorporateValidate(SignUpOptionalFields optionalFields)
        {
            if (!optionalFields.HasFlag(SignUpOptionalFields.Address) && string.IsNullOrEmpty(Address))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorAddressNull));
            }
            if (!optionalFields.HasFlag(SignUpOptionalFields.IC) && string.IsNullOrEmpty(IC))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorICNull));
            }
            if (!optionalFields.HasFlag(SignUpOptionalFields.IC) && IC.Length > 15)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorICInvalid));
            }
            if (!optionalFields.HasFlag(SignUpOptionalFields.PhoneNumber) && string.IsNullOrEmpty(PhoneNumber))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorPhoneNull));
            }
            if (!optionalFields.HasFlag(SignUpOptionalFields.PhoneNumber) && PhoneNumber.Length > 12)
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, Forms.ErrorPhoneInvalid));
            }
            if (!string.IsNullOrEmpty(SignUpPurpose) && !Int32.TryParse(SignUpPurpose, out var _))
            {
                throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments, "Invalid Sign Up Purpose value"));
            }
        }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }

    // Minimal registration model for management accounts
    public class ManagementRegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public RoleType RoleType { get; set; }

        public string Address { get; set; }
        [Display(Name = "Contact No.")]
        [MaxLength(20, ErrorMessage = "The contact number must not exceed 20 digits.")]
        public string PhoneNumber { get; set; }
    }

    public class TPRegisterModel
    {
        public DeviceModel Device { get; set; }
        public string TPUserId { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string FullName { get; set; }
        public Gender Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public int? CountryId { get; set; }
        public string IC { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string TPCorporateId { get; set; }
        public string CorporateName { get; set; }
        public string CorporateAddress { get; set; }
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
        public string BranchPhoneNumber { get; set; }
        public string EmployeeDependantName { get; set; }
        public string EmployeeDependantIC { get; set; }
        public CorporateUserType? CorporateUserType { get; set; }
    }
}
