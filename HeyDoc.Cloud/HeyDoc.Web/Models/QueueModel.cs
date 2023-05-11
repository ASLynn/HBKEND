namespace HeyDoc.Web.Models
{
    public class QueueMessage
    {
        public QueueMessage(string action, object messageData)
        {
            Action = action;
            MessageData = messageData;
        }

        public string Action { get; set; }

        public object MessageData { get; set; }
    }

    public class QueueUpdateMessage : QueueMessage
    {
        public QueueUpdateMessage(string email, string action, object messageData) : base(action, messageData)
        {
            Email = email;
        }

        public string Email { get; set; }
    }
}