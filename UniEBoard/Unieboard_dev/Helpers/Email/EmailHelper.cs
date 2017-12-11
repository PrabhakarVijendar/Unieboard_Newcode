using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace UniEBoard.Helpers.Email
{
    public static class EmailHelper
    {

        public static bool SendEmail(SmtpClient client, MailMessage message)
        {
            bool IsSend = false;
            try
            {
                client.Send(message);
                IsSend = true;
            }
            catch (Exception ex)
            {

            }
            return IsSend;
        }

        public static SmtpClient GetSmtpClient(string server, int port, string userName, string password, bool sslEnabled = false, 
            bool useDefaultCredentials = false, SmtpDeliveryMethod method = SmtpDeliveryMethod.Network)
        {
            SmtpClient client = new SmtpClient(server, port);
            client.UseDefaultCredentials = useDefaultCredentials;
            client.EnableSsl = sslEnabled;
            client.Credentials = new System.Net.NetworkCredential(userName, password);
            client.DeliveryMethod = method;
            return client;
        }

        public static MailMessage GetMailMessage(string FromEmail, string DisplayName, string ToEmail, string Subject, string Body, string CC, string Bcc,  bool IsHtml = true)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(FromEmail);
            message.To.Add(ToEmail);

            if (!String.IsNullOrEmpty(CC))
                message.CC.Add(CC);

            if (!String.IsNullOrEmpty(Bcc))
                message.Bcc.Add(Bcc);

            message.Subject = Subject;
            message.Body = Body;
            message.IsBodyHtml = IsHtml;

            return message;
        }
    }
}