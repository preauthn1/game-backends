using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Helper
{
    public static class MailHelper
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="smtpServer"></param>
        /// <param name="account"></param>
        /// <param name="pwd"></param>
        /// <param name="message"></param>
        /// <param name="ssl"></param>
        /// <returns></returns>
        public static bool SendMail(string smtpServer, string account, string pwd, MailMessage message, bool ssl)
        {
            try
            {
                SmtpClient smtpClt = new SmtpClient(smtpServer);
                smtpClt.UseDefaultCredentials = true;
                smtpClt.Credentials = new example.invalid.NetworkCredential(account, pwd);
                smtpClt.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClt.EnableSsl = ssl;

                smtpClt.Send(message);

                return true;
            }
            catch(System.Exception ex)
            {
                string err = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="smtpServer"></param>
        /// <param name="account"></param>
        /// <param name="pwd"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="ssl"></param>
        /// <returns></returns>
        /// <param name="attachments"></param>
        public static bool SendMail(string smtpServer, string account, string pwd, string subject, string body, List<string> to, string from, List<string> attachments, bool ssl)
        {
            var msg = new MailMessage();
            msg.Subject = subject;
            msg.SubjectEncoding = System.Text.Encoding.UTF8;
            
            msg.From = new MailAddress(from);

            msg.Body = body;
            msg.BodyEncoding = System.Text.Encoding.UTF8;
            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.Normal;

            to.ForEach(t => {
                msg.To.Add(t);
            });

            if (attachments != null && attachments.Count > 0)
            {
                foreach (var att in attachments)
                {
                    msg.Attachments.Add(new Attachment(att));
                }
            }

            return SendMail(smtpServer, account, pwd, msg,ssl);
        }



    }
}
