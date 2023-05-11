using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace HeyDoc.Web.Helpers
{
    public class EmailHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void SendViaSendGrid(IEnumerable<string> tos, string subject, string html, string text, string ccEmail)
        {
            SendViaSendGrid(tos, subject, html, text, ccEmail != null ? new List<string> { ccEmail } : null);
        }

        public static void SendViaSendGrid(IEnumerable<string> tos, string subject, string html, string text, IEnumerable<string> ccEmails = null, IEnumerable<string> bccEmails = null, bool allowDoc2Us = false)
        {
            // TODO M UNBLANK: SendGrid API key
            string apiKey = "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";
            // TODO M UNBLANK: Sender email address
            var from = new EmailAddress("info@hope.com.mm", "HOPE");

            // Default ccEmail and bccEmail to empty lists as it's easier to work with
            ccEmails = ccEmails ?? new List<string>();
            //bccEmails = bccEmails ?? new List<string>();
            if (tos == null || tos.Count() == 0)
            {
                return;
            }

            List<EmailAddress> toAddress = new List<EmailAddress>();

            // doc2us.com domain is filtered out by default as some users get automatically assigned dummy doc2us.com emails that do not exist
            // and sending to these non-existant emails hurts the SendGrid reputation
            foreach (var toEmail in tos.Where(e => allowDoc2Us || !e.ToLower().Contains("hope.com")))
            {
                toAddress.Add(new EmailAddress(toEmail));
            }
            if (toAddress.Count == 0)
            {
                return;
            }
            if (tos.Any(e => e.ToLower().Contains("mcmc.gov.my")) ||
                (ccEmails.Any(e => e.ToLower().Contains("mcmc.gov.my"))))
            {
                SendViaSmtp(tos, subject, html, text, ccEmails);
                return;
            }

            var client = new SendGridClient(apiKey);
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, toAddress, subject, "", html);
            foreach (var email in ccEmails.Where(e => allowDoc2Us || !e.ToLower().Contains("HOPE.com")).Except(tos, StringComparer.InvariantCultureIgnoreCase))
            {
                msg.AddCc(new EmailAddress(email));
            }
            //foreach (var email in bccEmails.Where(e => allowDoc2Us || !e.ToLower().Contains("HOPE.com")).Except(tos.Concat(ccEmails), StringComparer.InvariantCultureIgnoreCase))
            //{
            //    msg.AddBcc(new EmailAddress(email));
            //}
            
            if (!string.IsNullOrEmpty(text))
            {
                msg.PlainTextContent = text;
            }
            try
            {
           
         
                var response = client.SendEmailAsync(msg).Result;
                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    var resp = response.Body.ReadAsStringAsync().Result;
                    log.Info(response.StatusCode.ToString() + resp);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void SendViaSmtp(IEnumerable<string> tos, string subject, string html, string text, IEnumerable<string> ccEmail = null)
        {
            // Default ccEmail to empty list as it's easier to work with
            ccEmail = ccEmail ?? new List<string>();
            MailMessage myMessage = new MailMessage();
            foreach (var to in tos.Where(e => !e.ToLower().Contains("HOPE.com")))
            {
                myMessage.To.Add(to);
            }
            foreach (var email in ccEmail.Except(tos, StringComparer.InvariantCultureIgnoreCase))
            {
                if (!string.IsNullOrEmpty(email) && !email.ToLower().Contains("HOPE.com"))
                {
                    myMessage.CC.Add(email);
                }
            }
            // TODO M UNBLANK: Sender email address
            myMessage.From = new MailAddress("", "HOPE");
            myMessage.Subject = subject;           

            myMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            myMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            // Init SmtpClient and send
            using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.EnableSsl = true;

                // TODO M UNBLANK: Credentials for SMTP
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("", "");
                smtpClient.Credentials = credentials;
                try
                {
                    smtpClient.Send(myMessage);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ConvertEmailHtmlToString(string emailHtmlRelativePath)
        {
            string emailTemplatePath = "";
            //if (System.Web.HttpContext.Current.Request.Url.ToString().Contains("localhost"))
          //  {
                emailTemplatePath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath.ToString();
          //  }


            //begin:original code
            //string actualPath = Path.Combine(@"C:\Home\site\wwwroot", emailHtmlRelativePath);
            //end::original code
            string actualPath = Path.Combine(emailTemplatePath, emailHtmlRelativePath);
            return File.ReadAllText(actualPath);
        }
    }
}