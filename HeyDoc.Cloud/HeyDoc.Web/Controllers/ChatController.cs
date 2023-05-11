using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace HeyDoc.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Doctor")]
    
    public class ChatController : BaseController
    {
        public ActionResult Index()
        {
            List<SelectListItem> doctorList = new List<SelectListItem>();
            doctorList.Add(new SelectListItem
            {
                Text = "All",
                Value = string.Empty
            });
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {

                var entityDoctors = db.UserProfiles.AsQueryable();
                if (User.IsInRole("Doctor"))
                {
                    entityDoctors = entityDoctors.Where(x => x.UserName == User.Identity.Name);

                }
               var doctors = entityDoctors
                    .Where(e => e.webpages_Roles.FirstOrDefault(f => f.RoleName == "Doctor") != null&&!e.IsBan&&!e.IsDelete)
                    .Select(e => new SelectListItem()
                    {
                        Text = e.FullName,
                        Value = e.UserId.ToString(),
                    });
               
                foreach (var doctor in doctors)
                {
                    doctorList.Add(doctor);
                }
            }
            ViewBag.Doctors = doctorList;
            var list=new List<SelectListItem>(){new SelectListItem(){Text="Text",Value="2"},new SelectListItem(){Text="Image",Value="1"}};
            ViewBag.Reply = new SelectList(list, "Value", "Text");
            return View();
        }

        [HttpPost]
        public JsonResult GetChatRoomList()
        {
            int take, skip, recordsTotal, recordsFiltered, doctorId;
            List<ChatRoomModel> data = new List<ChatRoomModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);
                int order_col = Convert.ToInt32(Request.Form["order[0][column]"]);
                string order_dir = Request.Form["order[0][dir]"];
                var entityChatRoomList = db.ChatRooms.AsQueryable();

                if (!string.IsNullOrEmpty(Request.Form["doctorId"]))
                {
                    doctorId = Convert.ToInt32(Request.Form["doctorId"]);
                    entityChatRoomList = entityChatRoomList.Where(e => e.DoctorId == doctorId);
                }
                if (!string.IsNullOrEmpty(Request.Form["PatientNameOrEmail"]))
                {
                    var patientNameOrEmail = Convert.ToString(Request.Form["PatientNameOrEmail"]);
                    entityChatRoomList = entityChatRoomList.Where(e => e.Patient.UserProfile.FullName.Contains(patientNameOrEmail) || e.Patient.UserProfile.UserName.Contains(patientNameOrEmail));
                }

                recordsTotal = entityChatRoomList.Count();
                recordsFiltered = recordsTotal;
                switch (order_dir)
                {
                    case "asc":
                        switch (order_col)
                        {
                          case 2: entityChatRoomList = entityChatRoomList.OrderBy(e => e.Patient.UserProfile.FullName).ThenBy(e => e.PatientId).Skip(skip).Take(take);
                                break;
                          default: entityChatRoomList = entityChatRoomList.OrderByDescending(e => e.LastChatId).ThenBy(e => e.PatientId).Skip(skip).Take(take);
                                break;
                        }
                        break;
                    case "desc":
                        switch (order_col)
                        {
                             case 2: entityChatRoomList = entityChatRoomList.OrderByDescending(e => e.Patient.UserProfile.FullName).ThenBy(e => e.PatientId).Skip(skip).Take(take);
                                break;
                             default: entityChatRoomList = entityChatRoomList.OrderByDescending(e => e.Patient.UserProfile.FullName).ThenBy(e => e.PatientId).Skip(skip).Take(take);
                                break;
                        }
                        break;
                    default: entityChatRoomList = entityChatRoomList.OrderBy(e => e.LastChatId).ThenBy(e => e.PatientId).Skip(skip).Take(take);
                        break;
                }


                foreach (var entityChatRoom in entityChatRoomList)
                {
                    var Data = new ChatRoomModel(entityChatRoom);
                    data.Add(Data);
                }
            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

        #region Chat
        public ActionResult Chat()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetChatList(int chatRoomId)
        {
            int take, skip, recordsTotal, recordsFiltered;
            List<ChatModel> data = new List<ChatModel>();
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                take = Convert.ToInt32(Request.Form["length"]);
                skip = Convert.ToInt32(Request.Form["start"]);

                var entityChatList = db.Chats.Where(e => e.ChatRoomId == chatRoomId);

                recordsTotal = entityChatList.Count();
                recordsFiltered = recordsTotal;

                entityChatList = entityChatList.OrderByDescending(e => e.CreateDate).Skip(skip).Take(take);

                foreach (var entityChat in entityChatList)
                {
                    var Data = new ChatModel(entityChat);
                  //  Data.CreateDate = Data.CreateDate.AddHours(8);
                    data.Add(Data);
                }
            }

            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data,
            });
        }

        [HttpPost]
        public JsonResult DeleteChat(long chatId)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityChat = db.Chats.FirstOrDefault(e => e.ChatId == chatId && !e.IsDelete);
                if (entityChat != null)
                {
                    entityChat.IsDelete = true;                  
                    db.SaveChanges();
                }
                else
                {
                    return Json("Server Error!", JsonRequestBehavior.AllowGet);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public void DoctorChat(DoctorReplyModel modelData)
        {

            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                if (!(string.IsNullOrEmpty(modelData.image) && string.IsNullOrEmpty(modelData.text)))
                {
                    MessageType msgType;
                    if (!string.IsNullOrEmpty(modelData.image))
                    {
                        msgType = MessageType.Photo;
                    }
                    else
                    {
                        msgType = MessageType.Message;
                    }
                    var entityChat = db.Chats.Create();
                    entityChat.ChatRoomId = modelData.chatRoomId;
                    entityChat.MessageType = msgType;
                    entityChat.CreateDate = DateTime.UtcNow;
                    entityChat.FromUserId = modelData.doctorId;
                    entityChat.ToUserId = modelData.patientId;
                    if (msgType == MessageType.Photo)
                    {
                        string containerName = "s" +modelData.doctorId.ToString("D5");
                        var data = Regex.Replace(modelData.image, @"^data:image/[a-z]+;base64,", "");
                        Stream photoStream = null;
                        byte[] buffer = null;

                        try
                        {
                            buffer = Convert.FromBase64String(data);
                        }
                        catch
                        {
                            throw new WebApiException(new WebApiError(WebApiErrorCode.InvalidArguments));
                        }
                        photoStream = new MemoryStream(buffer);
                        var entityPhoto = PhotoHelper.UploadImageWithThumbnail(db, containerName, Guid.NewGuid().ToString(), photoStream);
                        entityChat.PhotoId = entityPhoto.PhotoId;

                    }
                    else
                    {
                        entityChat.Message = modelData.text.Trim();
                    }
                    db.Chats.Add(entityChat);

                    //update last job date of doctor
                    var entityUpdateDoc = db.Doctors.FirstOrDefault(e => e.UserId ==modelData.doctorId);
                    if (entityUpdateDoc != null)
                    {
                        entityUpdateDoc.LastJobDate = DateTime.UtcNow;
                    }

                    db.SaveChanges();
                    db.SP_RepliedAllMessage(modelData.doctorId, modelData.chatRoomId);
                }
            }
            
        }
        #endregion Chat

    }
}
