using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class ScanUserModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string FullName { get; set; }
        public DateTime ScannedTime { get; set; }
        public string Title { get; set; }
        public ScanUserModel()
        {

        }
        public ScanUserModel(Entity.UserProfile entityUser)
        {
            UserId = entityUser.UserId;
            UserName = entityUser.UserName;
            NickName = entityUser.Nickname;
            FullName = entityUser.FullName;
            Title = entityUser.Title;
        }
    }
}