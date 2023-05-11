//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HeyDoc.Web.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class ChatBotResponse
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ChatBotResponse()
        {
            this.ChatBotResponseTags = new HashSet<ChatBotResponseTag>();
        }
    
        public int Id { get; set; }
        public int ChatBotSessionId { get; set; }
        public System.DateTime QuestionSentDate { get; set; }
        public Nullable<System.DateTime> ResponseReceivedDate { get; set; }
        public int QuestionId { get; set; }
        public Nullable<int> AnswerId { get; set; }
        public string JsonValue { get; set; }
        public bool IsRolledback { get; set; }
    
        public virtual ChatBotAnswer ChatBotAnswer { get; set; }
        public virtual ChatBotQuestion ChatBotQuestion { get; set; }
        public virtual ChatBotSession ChatBotSession { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChatBotResponseTag> ChatBotResponseTags { get; set; }
    }
}