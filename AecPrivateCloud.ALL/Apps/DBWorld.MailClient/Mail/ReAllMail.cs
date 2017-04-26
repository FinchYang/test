using System;

namespace DBWorld.MailClient.Mail
{
    class ReAllMail : MailObject
    {
        public override MailCore.Models.MailInfo GetMailContext()
        {
            var receivers = MailContext.Sender;
            receivers += MailContext.Recivers;
            receivers = receivers.Replace(String.Format("{0}<{1}>;", MsgConfig.UserName, MsgConfig.Email), "");
            receivers = receivers.Replace(" ", "");

            MailContext.Content = GetSignatureStyle() + GetReferenceStyle() + GetMailBody();
            MailContext.Subject = "回复：" + MailContext.Subject;
            MailContext.Recivers = receivers;
            MailContext.Sender = String.Format("{0}<{1}>;", MsgConfig.UserName, MsgConfig.Email);
            MailContext.AttachsPath = null;

            return MailContext;
        }
    }
}
