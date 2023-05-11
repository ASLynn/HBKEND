using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class VoiceModel
    {
        public long VoiceId { get; set; }
        public string VoiceFileUrl { get; set; }

        public HttpPostedFileBase VoiceUrl { get; set; }

        public VoiceModel()
        {

        }

        public VoiceModel(string voiceUrl)
        {
            VoiceFileUrl = voiceUrl;
        }

        public VoiceModel(Entity.Voice entityVoice)
        {
            VoiceId = entityVoice.VoiceId;
            VoiceFileUrl = entityVoice.VoiceUrl;

        }
    }
}