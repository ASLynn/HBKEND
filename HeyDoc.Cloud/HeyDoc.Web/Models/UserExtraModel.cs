using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class UserExtraModel
    {
        public int UserExtraId { get; set; }
        public int UserId { get; set; }
        public string OTP { get; set; }
        public int OTPVerified { get; set; }
        public DateTime OTPCreateDT { get; set; }
        public int Status { get; set; }
        public string PhoneNumber { get; set; }

        public UserExtraModel()
        {

        }

        public UserExtraModel(Entity.UserExtra entityUserExtra)
        {
            UserExtraId = entityUserExtra.UserExtraId;
            UserId = entityUserExtra.UserId;
            OTP = entityUserExtra.OTP;
            OTPVerified = entityUserExtra.OTPVerified;
            OTPCreateDT = entityUserExtra.OTPCreateDT;
            Status = entityUserExtra.Status;

        }




    }

   
}