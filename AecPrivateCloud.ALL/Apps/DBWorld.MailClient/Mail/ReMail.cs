using System;

namespace DBWorld.MailClient.Mail
{
    class ReMail : MailObject
    {
        public override MailCore.Models.MailInfo GetMailContext()
        {
            MailContext.Content = GetSignatureStyle() + GetReferenceStyle() + GetMailBody();
            MailContext.Subject = "回复：" + MailContext.Subject;
            MailContext.Recivers = MailContext.Sender;
            MailContext.Sender = String.Format("{0}<{1}>;", MsgConfig.UserName, MsgConfig.Email);
            MailContext.CC = "";
            MailContext.BCC = "";
            MailContext.AttachsPath = null;

            return MailContext;
        }
    }
}
