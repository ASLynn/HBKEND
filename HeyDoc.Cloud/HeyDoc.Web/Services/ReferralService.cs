using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class ReferralService
    {
        public static StringResult InvitationCode(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var invitationCode = Encoder.Conceal(entityUser.UserId);
                
                return new StringResult(invitationCode);
            }
        }

        public static List<ReferralModel> InviteList(string accessToken)
        {
            using (Entity.db_HeyDocEntities db = new Entity.db_HeyDocEntities())
            {
                var entityUser = AccountService.GetEntityUserByAccessToken(db, accessToken, false);

                var entityInviteList = db.Invitations.Where(e => e.UserId == entityUser.UserId && !e.IsRegistered);

                List<ReferralModel> result = new List<ReferralModel>();

                foreach (var entityInvite in entityInviteList)
                { 
                    result.Add(new ReferralModel(entityInvite));
                }

                return result;
            }
        }
    }
}