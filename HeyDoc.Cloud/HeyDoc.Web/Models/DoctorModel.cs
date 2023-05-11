using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HeyDoc.Web.Entity;

namespace HeyDoc.Web.Models
{
    public class DoctorModel : UserModel
    {
        public int DoctorId { get; set; }
        public string Specialty { get; set; }
        public System.DateTime? Practicing { get; set; }
        public string MedicalSch { get; set; }
        public string AboutMe { get; set; }
        public int? AttendedUser { get; set; }
        public float? AverageRating { get; set; }
        public int? ReviewCount { get; set; }
        public DateTime? LastSeenDate { get; set; }

        public DoctorReviewModel MyReview { get; set; }
        public CategoryModel Category { get; set; }
        public GroupModel Group { get; set; }

        public string Qualification { get; set; }
        public string RegisterNumber { get; set; }
        public string HospitalName { get; set; }

        public bool IsVerified { get; set; }
        public string Certificate { get; set; }
        public int NumberOfCancelRequest { get; set; }

        public int PatientsAttended { get; set; }

        public DateTime? DutyDate { get; set; }

        public string SignatureUrl { get; set; }

        public bool ShowInApp { get; set; }

        public bool? CanApproveEPS { get; set; }
        public bool? IsChatBotEnabled { get; set; }
        public string VideoChatURL { get; set; }

        public List<DoctorSpecialityModel> DoctorSpecialitiesList { get; set; }
        public List<DoctorQualificationModel> DoctorQualificationList { get; set; }
        public List<DoctorFacilityModel> DoctorFacilityList { get; set; }

        public List<DoctorDutyModel> DoctorDutyList { get; set; }
        public DoctorModel()
        {

        }

        public DoctorModel(Entity.Doctor entityDoctor, int attendedUser, int reviewCount, int avgRating)
            : this(entityDoctor.UserProfile, entityDoctor)
        {
            AttendedUser = attendedUser;
            ReviewCount = reviewCount;
            AverageRating = avgRating;
            if (entityDoctor.DoctorSpecialities != null)
            {
                DoctorSpecialitiesList = new List<DoctorSpecialityModel>();
                foreach (var item in entityDoctor.DoctorSpecialities)
                {         
                    DoctorSpecialityModel dsm = new DoctorSpecialityModel(item);
                    DoctorSpecialitiesList.Add(dsm);
                }
                 
            }
            if (entityDoctor.DoctorQualifications != null)
            {
                DoctorQualificationList = new List<DoctorQualificationModel>();
                foreach (var item in entityDoctor.DoctorQualifications)
                {
                    DoctorQualificationModel dsm = new DoctorQualificationModel(item);
                    DoctorQualificationList.Add(dsm);
                }
            }
            if (entityDoctor.DocFacilityAccessments != null)
            {
                DoctorFacilityList = new List<DoctorFacilityModel>();
                foreach (var item in entityDoctor.DocFacilityAccessments)
                {
                    DoctorFacilityModel dsm = new DoctorFacilityModel(item);
                    DoctorFacilityList.Add(dsm);
                }
            }
        }

        public DoctorModel(Entity.Doctor entityDoctor)
            : base(entityDoctor.UserProfile)
        {
            DoctorId = entityDoctor.UserId;
            Specialty = entityDoctor.Specialty;
            Practicing = entityDoctor.Practicing;
            MedicalSch = entityDoctor.MedicalSch;
            AboutMe = entityDoctor.AboutMe;
            if (entityDoctor.Category != null)
            {
                Category = new CategoryModel(entityDoctor.Category);
            }
            if (entityDoctor.Group != null)
            {
                Group = new GroupModel(entityDoctor.Group);
            }

            if (entityDoctor.DoctorSpecialities != null)
            {
                DoctorSpecialitiesList = new List<DoctorSpecialityModel>();
                foreach (var item in entityDoctor.DoctorSpecialities)
                {
                    DoctorSpecialityModel dsm = new DoctorSpecialityModel(item);
                    DoctorSpecialitiesList.Add(dsm);
                }

            }
            if (entityDoctor.DoctorQualifications != null)
            {
                DoctorQualificationList = new List<DoctorQualificationModel>();
                foreach (var item in entityDoctor.DoctorQualifications)
                {
                    DoctorQualificationModel dsm = new DoctorQualificationModel(item);
                    DoctorQualificationList.Add(dsm);
                }
            }
            if (entityDoctor.DocFacilityAccessments != null)
            {
                DoctorFacilityList = new List<DoctorFacilityModel>();
                foreach (var item in entityDoctor.DocFacilityAccessments)
                {
                    DoctorFacilityModel dsm = new DoctorFacilityModel(item);
                    DoctorFacilityList.Add(dsm);
                }
            }
            Qualification = entityDoctor.Qualifications;
            RegisterNumber = entityDoctor.RegisterNumber;
            HospitalName = entityDoctor.HospitalName;
            SignatureUrl = entityDoctor.SignatureUrl;
            ShowInApp = entityDoctor.ShowInApp;
        }

        public DoctorModel(Entity.UserProfile entityUser, Entity.Doctor entityDoctor)
            : base(entityUser)
        {
            DoctorId = entityDoctor.UserId;
            Specialty = entityDoctor.Specialty;
            Practicing = entityDoctor.Practicing;
            MedicalSch = entityDoctor.MedicalSch;
            AboutMe = entityDoctor.AboutMe;
            if (entityDoctor.Category != null)
            {
                Category = new CategoryModel(entityDoctor.Category);
            }
            if (entityDoctor.Group != null)
            {
                Group = new GroupModel(entityDoctor.Group);
            }

            if (entityDoctor.DoctorSpecialities != null)
            {
                DoctorSpecialitiesList = new List<DoctorSpecialityModel>();
                foreach (var item in entityDoctor.DoctorSpecialities)
                {
                    DoctorSpecialityModel dsm = new DoctorSpecialityModel(item);
                    DoctorSpecialitiesList.Add(dsm);
                }

            }
            if (entityDoctor.DoctorQualifications != null)
            {
                DoctorQualificationList = new List<DoctorQualificationModel>();
                foreach (var item in entityDoctor.DoctorQualifications)
                {
                    DoctorQualificationModel dsm = new DoctorQualificationModel(item);
                    DoctorQualificationList.Add(dsm);
                }
            }
            if (entityDoctor.DocFacilityAccessments != null)
            {
                DoctorFacilityList = new List<DoctorFacilityModel>();
                foreach (var item in entityDoctor.DocFacilityAccessments)
                {
                    DoctorFacilityModel dsm = new DoctorFacilityModel(item);
                    DoctorFacilityList.Add(dsm);
                }
            }
            Qualification = entityDoctor.Qualifications;
            RegisterNumber = entityDoctor.RegisterNumber;
            HospitalName = entityDoctor.HospitalName;
            IsVerified = entityDoctor.IsVerified;
            Certificate = entityDoctor.CerttificateUrl;
            ShowInApp = entityDoctor.ShowInApp;
            CanApproveEPS = entityDoctor.CanApproveEPS;
            IsChatBotEnabled = entityDoctor.IsChatBotEnabled;
            VideoChatURL = entityDoctor.VideoChatUrl;

            if (entityUser.DoctorDuties != null)
            {
                DoctorDutyList = new List<DoctorDutyModel>();
                foreach (var item in entityUser.DoctorDuties)
                {
                    DoctorDutyModel dsm = new DoctorDutyModel(item);
                    DoctorDutyList.Add(dsm);
                }
            }
        }

        public DoctorModel(Entity.UserProfile entityUser) : base(entityUser)
        {
        }
    }

    public class DoctorUpdateModel
    {
        public string BloodType { get; set; }
        public string Email { get; set; }
        public int DoctorId { get; set; }
        public PhotoModel Photo { get; set; }
        public string Nickname { get; set; }
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        public Gender Gender { get; set; }
        public Nullable<DateTime> Birthday { get; set; }
        public int CountryId { get; set; }
        public string InvitationCode { get; set; }
        public string Language { get; set; }      
        public string Specialty { get; set; }
        public string Title { get; set; }
        public string SignatureUrl { get; set; }
        [Display(Name = "Show in App")]
        public bool ShowInApp { get; set; }
        public long GroupId { get; set; }
        [Display(Name = "IsPartner")]
        public bool IsPartner { get; set; }

        [Display(Name = "Praciticing Since")]
        public DateTime? Practicing { get; set; }

        [Display(Name = "Medical School")]
        public string MedicalSch { get; set; }

        [Display(Name = "About Me")]
        public string AboutMe { get; set; }

        [Display(Name = "Practicing Address")]
        public string Address { get; set; }
        [Display(Name = "Clinic")]
        public string HospitalName { get; set; }
        public int? CategoryId { get; set; }
        [Display(Name = "Contact No.")]
        public string PhoneNumber { get; set; }
        public string Qualification { get; set; }
        [Display(Name = "Reg. No:")]
        public string RegisterNumber { get; set; }

        [Required]
        public string IC { get; set; }

        [Display(Name = "Digital Signature Enabled")]
        public bool IsDigitalSignatureEnabled { get; set; }
        [Display(Name = "Can Approve EPS")]
        public bool CanApproveEPS { get; set; }
        [Display(Name = "Enable Chat Assistant")]
        public bool IsChatBotEnabled { get; set; }
        [Display(Name = "Video Chat URL")]
        public string VideoChatURL { get; set; }

        public IEnumerable<int> SpecialityId { get; set; }
        public List<SelectListItem> Specilities { get; set; }
        public IEnumerable<int> QualificationId { get; set; }
        public List<SelectListItem> Qualifications { get; set; }
        public IEnumerable<int> FacilitiesId { get; set; }
      
        public List<SelectListItem> Facilities { get; set; }

        public List<CertiModel> certiModels = new List<CertiModel>();
        public int? StateId { get; set; }
        public int? TownshipId { get; set; }
        public int FacilityId { get; set; }
        public int FacilityTypeId { get; set; }
        public DoctorUpdateModel()
        {
            Photo = new PhotoModel();
        }
        public DoctorUpdateModel(Entity.UserProfile entityUser)
        {
            Email = entityUser.UserName;
            DoctorId = entityUser.UserId;
            Photo = new PhotoModel(entityUser.Photo);
            Nickname = entityUser.Nickname;
            Gender = entityUser.Gender.Value;
            Birthday = entityUser.Birthday;
            CountryId = entityUser.CountryId.Value;
            Language = entityUser.Language;
            FullName = entityUser.FullName;          
            IsPartner = entityUser.Doctor.IsPartner;
            Specialty = entityUser.Doctor.Specialty;
            Practicing = entityUser.Doctor.Practicing;
            MedicalSch = entityUser.Doctor.MedicalSch;
            AboutMe = entityUser.Doctor.AboutMe;
            BloodType = entityUser.BloodType;
            Address = entityUser.Address;
            HospitalName = entityUser.Doctor.HospitalName;
            CategoryId = entityUser.Doctor.CategoryId ?? 0;
            PhoneNumber = entityUser.PhoneNumber;
            Qualification = entityUser.Doctor.Qualifications;
            RegisterNumber = entityUser.Doctor.RegisterNumber;
            Title = entityUser.Title;
            GroupId = entityUser.Doctor.GroupId ?? 0;
            SignatureUrl = entityUser.Doctor.SignatureUrl;
            IC = entityUser.IC;
            IsDigitalSignatureEnabled = entityUser.Doctor.IsDigitalSignatureEnabled;
            CanApproveEPS = entityUser.Doctor.CanApproveEPS ?? false;
            IsChatBotEnabled = entityUser.Doctor.IsChatBotEnabled;
            ShowInApp = entityUser.Doctor.ShowInApp;
            VideoChatURL = entityUser.Doctor.VideoChatUrl;
        }
    }

    // hw 20150716 : create
    public class StatisticsModel
    {
        public int AverageResponse { get; set; } // average time of response
        public int AveragePatients { get; set; }  // average time spent daily number of patient and replies
        public int AveragePatientReplies { get; set; }
        public StatisticsModel() { }
        public StatisticsModel(Entity.db_HeyDocEntities db, int doctorId)
        {
            var entityChats = db.Chats.Where(e => e.FromUserId == doctorId);
            int totalChats = entityChats.Count(); // total chats
            int totalPatients = entityChats.GroupBy(e => new { Patient = e.ToUserId }).Count();
            var totalDate = entityChats.GroupBy(e => new { y = e.CreateDate.Year, m = e.CreateDate.Month, d = e.CreateDate.Day }).Count();

            AverageResponse = totalDate != 0 ? (totalChats / totalDate) : 0; // average time of response
            AveragePatients = totalDate != 0 ? (totalPatients / totalDate) : 0; // average time spent daily number of patient and replies
            AveragePatientReplies = totalDate != 0 ? (totalChats / totalPatients) : 0;
        }
    }
    public class DoctorReplyModel
    {
        public string image { get; set; }
        public string text { get; set; }
        public int chatRoomId { get; set; }
        public int patientId { get; set; }
        public int doctorId { get; set; }
    }


}