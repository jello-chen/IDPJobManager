using System;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace IDPJobManager.Core.Utils
{
    public class EmailHelper
    {
        public string From { get; set; }
        public string[] To { get; set; }
        public string[] CC { get; set; }
        public string[] BCC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string FromEmailPassword { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsBodyHtml { get; set; }
        public string[] Attachments { get; set; }
        
        public bool Send()
        {
            try
            {
                MailMessage message = new MailMessage();

                if (To != null && To.Length > 0)
                {
                    for (int i = 0; i < To.Length; i++)
                    {
                        message.To.Add(To[i]);
                    }
                }

                if (CC != null && CC.Length > 0)
                {
                    for (int i = 0; i < CC.Length; i++)
                    {
                        message.CC.Add(CC[i]);
                    }
                }

                if (BCC != null && BCC.Length > 0)
                {
                    for (int i = 0; i < BCC.Length; i++)
                    {
                        message.Bcc.Add(BCC[i]);
                    }
                }

                if (Attachments != null && Attachments.Length > 0)
                {
                    for (int i = 0; i < Attachments.Length; i++)
                    {
                        Attachment attachment = new Attachment(Attachments[i], MediaTypeNames.Application.Octet);
                        message.Attachments.Add(attachment);
                    }
                }

                MailAddress address = new MailAddress(From);
                message.From = address;
                message.Subject = Subject;
                message.SubjectEncoding = Encoding.UTF8;
                message.Body = Body;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = IsBodyHtml;

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(From, FromEmailPassword);
                smtpClient.Host = Host;
                smtpClient.Port = Port == 0 ? 25 : Port;

                smtpClient.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex.Message);
            }
            return false;
        }
    }
}
