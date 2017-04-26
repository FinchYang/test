using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Service
{
    public class EmailService : IEmailService
    {
        private static void SendEmail(string host, string mailFrom, string fromDisplayName, string fromUser, string fromPassword,
            string mailTo, string title, string body, bool isHtml, Encoding textEncoding)
        {
            using (var client = new SmtpClient(host))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(fromUser, fromPassword);
                var from = new MailAddress(mailFrom, fromDisplayName);
                var to = new MailAddress(mailTo);

                var mail = new MailMessage(from, to)
                {
                    Subject = title,
                    SubjectEncoding = textEncoding,
                    Body = body,
                    BodyEncoding = textEncoding,
                    IsBodyHtml = isHtml,
                    Priority = MailPriority.Normal
                };
                client.Send(mail);
                mail.Dispose();
            }
        }

        private string ActivateEmailHost = "cadscloud.cn"; //"192.168.1.106";
        private string ActivateEmail = "aeccloud@simuladesign.com";
        private string ActivatePassword = "simuladesign123?";
        private string ActivateUserName = "aeccloud";
        private string ActivateDisplayName = "DBWorld";

        private string PasswordEmailHost = "cadscloud.cn"; //"192.168.1.106";
        private string PasswordEmail = "aeccloud@simuladesign.com";
        private string PasswordPassword = "simuladesign123?";
        private string PasswordUserName = "aeccloud";
        private string PasswordDisplayName = "DBWorld";

        private string InviteEmailHost = "cadscloud.cn"; //"192.168.1.106";
        private string InviteEmail = "aeccloud@simuladesign.com";
        private string InvitePassword = "simuladesign123?";
        private string InviteUserName = "aeccloud";
        private string InviteDisplayName = "DBWorld";
        public void SendActivateAccountEmail(EmailSendingModel model)
        {
            if (String.IsNullOrEmpty(model.Title))
            {
                model.Title = "DBWorld工程云【DBWorld】注册激活";
            }
            SendEmail(ActivateEmailHost, ActivateEmail, ActivateDisplayName, ActivateUserName, ActivatePassword,
                model.MailTo, model.Title, model.Body, model.IsHtml, model.TextEncoding);
        }



        public void SendInviteMemberEmail(EmailSendingModel model)
        {
            if (String.IsNullOrEmpty(model.Title))
            {
                model.Title = "DBWorld工程云【DBWorld】项目邀请";
            }
            SendEmail(InviteEmailHost, InviteEmail, InviteDisplayName, InviteUserName, InvitePassword,
                model.MailTo, model.Title, model.Body, model.IsHtml, model.TextEncoding);
        }


        public void SendActivateCode(EmailSendingModel model)
        {
            if (String.IsNullOrEmpty(model.Title))
            {
                model.Title = "DBWorld工程云【DBWorld】密码找回";
            }
            SendEmail(ActivateEmailHost, ActivateEmail, ActivateDisplayName, ActivateUserName, ActivatePassword,
                model.MailTo, model.Title, model.Body, model.IsHtml, model.TextEncoding);
        }

        public void SendForgotPasswordEmail(EmailSendingModel model)
        {
            if (String.IsNullOrEmpty(model.Title))
            {
                model.Title = "DBWorld工程云【DBWorld】找回密码";
            }
            SendEmail(PasswordEmailHost, PasswordEmail, PasswordDisplayName, PasswordUserName, PasswordPassword,
                model.MailTo, model.Title, model.Body, model.IsHtml, model.TextEncoding);
        }

        public void SetAccountEmailInfo(string host, string emailAddress, string password, string userName,
            string displayName)
        {
            ActivateEmailHost = host;
            ActivateEmail = emailAddress;
            ActivatePassword = password;
            ActivateUserName = userName;
            ActivateDisplayName = displayName;
        }

        public void SetInvitationEmailInfo(string host, string emailAddress, string password, string userName,
            string displayName)
        {
            InviteEmailHost = host;
            InviteEmail = emailAddress;
            InvitePassword = password;
            InviteUserName = userName;
            InviteDisplayName = displayName;
        }

        public void SetPasswordEmailInfo(string host, string emailAddress, string password, string userName,
            string displayName)
        {
            PasswordEmailHost = host;
            PasswordEmail = emailAddress;
            PasswordPassword = password;
            PasswordUserName = userName;
            PasswordDisplayName = displayName;
        }
    }
}
