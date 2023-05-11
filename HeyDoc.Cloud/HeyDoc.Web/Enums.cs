using System.ComponentModel;

namespace HeyDoc.Web
{
    public enum DeviceType : byte
    {
        Invalid = 0,
        IOS = 1,
        Android = 2,
        Windows = 3,
        Web = 6,
        PWA = 7,
    }

    public enum RoleType : byte
    {
        Admin = 1,
        Doctor = 2,
        User = 3,
        SuperAdmin = 4,
        Pharmacy = 5,
        PharmacyManagement = 6,
    }

    public enum MessageType : byte
    {
        Message = 1,
        Photo = 2,
        Voice = 3,
        ChatBotSession = 4
    }

    public enum Gender : byte
    {
        Male = 1,
        Female = 2,
    }

    public enum PackageType : byte
    {
        Free = 1,
        Adhoc = 2,
        SelectDoctor = 3,
        Premium = 4
    }

    public enum BioDataType : byte
    {
        All = 0,
        Weight = 1,
        Height = 2,
        BMI = 3,
        HeartRate = 4,
        BloodPressure = 5,
        BodyTemperature = 6,
        BloodGluccose = 7,
        BloodGluccoseFasting = 8,
        MenstrualPeriod = 9,
        MenstrualDuration = 10
    }

    public enum PnActionType : byte
    {
        Message = 0,
        URL = 1,
        Screen = 2,
        Chat = 3,
        Gift = 4,
        Goal = 5,
        Remark = 6,
        ChatAccepted = 7,
        ChatRejected = 8,
        ChatCanceled = 9,
        ChatNoResponse = 10,
        ChatRequest = 11,
        Prescription = 12,
        ChatEnded = 13,
        Admin = 14,
        ChatNotActive = 15,
        CorporateUserPrescription = 17,
        PrescriptionStatus = 18,
        PrescriptionApproved = 19,
        PrescriptionRejected = 20,

        //QueueMedService PN
        AppointmentMade = 21,
        AppointmentStatusUpdate = 22
    }

    public enum ChatFeeType
    {
        Chargeable = 0,
        Free = 1
    }

    public enum RequestStatus : byte
    {
        Canceled = 0,
        Requested = 1,
        Accepted = 2,
        Rejected = 3,
        Completed = 4
    }

    public enum PaymentStatus : byte
    {
        Authorised = 0,
        Canceled = 1,
        Paid = 2,
        Failed = 3,
        Requested = 4,
        Pending = 5,
        Result = 6
    }

    public enum CashOutRequestStatus : byte
    {
        Requested = 0,
        Processed = 1,
        Rejected = 2
    }

    public enum UserCardType
    {
        Credit = 0,
        Debit = 1,
        Unknown = 2
    }

    public enum NotificationType : byte
    {
        New = 0,
        Notified = 1
    }

    public enum OAuthType : byte
    {
        Facebook = 1,
        Google = 2,
        Microsoft = 3,
        Twitter = 4,
        Yahoo = 5,
    }

    public enum PromoDiscountType : byte
    {
        Amount = 0,
        Percentage = 1
    }

    public enum PromoStatus : byte
    {
        Active = 0,
        Disabled = 1,
        MaxUserLimitReached = 2,
        Expired = 3
    }

    public enum FileType : byte
    {
        Audio = 1,
        Video = 2
    }

    public enum PatientType : byte
    {
        User = 1,
    }

    public enum CallParticipantType : byte
    {
        User = 1,
        Doctor = 3
    }

    public enum KioskCallRejectReason : byte
    {
        UnknownOrNotRejected = 0,
        DoctorEngaged = 1,
        DoctorRejected = 2,
        DoctorTimeout = 3,
        PatientCancelledOrTimeout = 4
    }

    public enum ResultFileType : byte
    {
        Pdf = 1,
        Other = 2
    }

    public enum PrescriptionStatus : byte
    {
        NoStatus = 0,
        SelfCollection = 1,
        Delivery = 2,
        OnSite = 3
    }

    public enum DispatchStatus : byte
    {
        NoStatus = 0,
        Requested = 1,
        Approved = 2,
        Ready = 3,
        Completed = 4,
        Rejected = 5,
        Pending = 6
    }
    public enum ListUserType : byte
    {
        All = 0,
        NonCorporateUser = 1,
        CorporateUser = 2
    }
    public enum CorporateUserType : byte
    {
        Unknown = 0,
        Employee = 1,
        EmployeeDependants = 2,
        PublicUser = 3,
        EmployeeChild = 4
    }

    public enum PrescriptionMedicationType : byte
    {
        Unspecified = 0,
        MinorAilment = 1,
        LTM = 2
    }

    public enum PrescriptionAvailabilityStatus : byte
    {
        NotApplicable = 0, // Might be better to just have prescriptions created by doctors have Approved status automatically, but this is necessary for compatibility with older prescriptions
        New = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }

    public enum Answer : byte
    {
        No = 0,
        Yes = 1
    }

    public enum SourceType : byte
    {
        [Description("HOPE")]
        Apps = 2,
    }

    public enum AccessTokenType : byte
    {
        Doc2Us = 0, // Access tokens granted to our own apps
    }

    public enum AuditLogType : byte
    {
        [Description("Search for IC")]
        SearchIC = 1,
        [Description("Added User")]
        AddUser = 2,
        [Description("Added Medication Record")]
        AddMedicationRecord = 3,
        [Description("Search for User")]
        SearchUser = 4,
        [Description("Prescription Dispensed")]
        PrescriptionDispensed = 5,
        [Description("Search for Prescription by QR")]
        SearchPrescriptionQr = 6
    }

    public enum PackingType : byte
    {
        NotSpecified = 0,
        PillCubePacking = 1
    }

    public enum DigitalSignatureLevel
    {
        Doctor = 1,
        Pharmacist = 2
    }

    public enum ChatBotAnswerType : byte
    {
        DISPLAY = 0,
        LIST_SELECT = 1,
        CHECK_SUCCESS = 2,
        CHECK_FAILED = 3
    }

    public enum ChatBotQuestionType : byte
    {
        FIRST = 0,
        NORMAL = 1,
        LAST_SUCCESS_DEFAULT = 2,
        LAST_FAIL_DEFAULT = 3,
        CALL_NEXT_QUESTION = 4,
        // Unused
        //VIEW_PRESCRIPTION = 5,
        //CHECKOUT_PAGE = 6,
        //CALL_ANSWER = 7,
        LAST = 8
    }

    public enum ChatBotCheckApi : byte
    {
        CHECK_USER_HAS_PRESCRIPTION = 0,
        CHECK_USER_IS_CORPORATE_USER = 1,
        CHECK_USER_PROFILE_HAS_INFO_FOR_PRESCRIPTION = 2,
        CHECK_USER_PRESCRIBED_MEDICATION_BEFORE = 3,
        CREATE_PRESCRIPTION = 4,
        GET_MEDICATION_SPECIFIC_QUESTIONS = 5,
        CHECK_MEDICATIONS_AVAILABLE = 6
    }

    public enum ChatBotSessionStatus : byte
    {
        ACTIVE = 0,
        CANCELLED = 1,
        ENDED = 2
    }

    public enum PrescriptionFrontEndSource : byte
    {
        UNKNOWN = 0,
        CHAT = 1,
        CHATBOT = 2,
        EPS = 4,
    }

    public enum PrescriptionSortField
    {
        CreateDate = 0,
        PatientName = 1,
        DoctorName = 2,
        PrescriptionId = 3
    }

    public enum OnlineStatusChangeSource : byte
    {
        Login = 0,
        Logout = 1,
        SetOnlineStatus = 2
    }
    public enum SortOrder
    {
        Ascending = 1,
        Descending = 2
    }

    public enum PartnerType : byte
    {
        // TODO M: Possibly rename or remove
        Partner1 = 1
    }

    public enum MedicationSourceType : byte
    {
        Doc2Us = 1,
    }

    public enum ThirdPartyLoginResultCode : byte
    {
        Success = 0,
        UserNotFound = 1,
        UserBanned = 2
    }

    public enum DoctorGroupUserTypeCategories : byte
    {
        Doc2Us = 0,
        UserType1 = 1,
        UserType2 = 2
    }

    public enum ActivityAuditEvent : byte
    {
        RegisterSuccess = 1,
        RegisterFail = 2,
        LoginSuccess = 3,
        LoginFail = 4,
        IntegratedRegisterLoginSuccess = 5,
        IntegratedRegisterLoginFail = 6,
        ApiCallSuccess = 7,
        ApiCallFail = 8,
        Logout = 9,
        PasswordResetSuccess = 10,
        PasswordResetFail = 11,
        ResendVerificationEmailSuccess = 12,
        ResendVerificationEmailFail = 13
    }

    [System.Flags]
    public enum SignUpOptionalFields : byte
    {
        None = 0,
        Address = 1,
        IC = 2,
        PhoneNumber = 4,
        Gender = 8,
        Birthday = 16
    }

    public enum GdexShipmentStatus : byte
    {
        Pending = 0,
        Pickup = 1,
        [Description("In Transit")]
        InTransit = 2,
        [Description("Out For Delivery")]
        OutForDelivery = 3,
        Delivered = 4,
        Returned = 5,
        Claim = 6
    }

    public enum ProcessingStatus : byte
    {
        Processing = 1, // before creating Gdex CN
        [Description("Ready to Ship")]
        ToShip = 2, // equivalent to GdexShipmentStatus Pending, still rejectable
        Shipped = 3, // equivalent to GdexShipmentStatus Pickup and beyond, no longer rejectable
        Rejected = 4
    }
}
