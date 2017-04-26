using System;

namespace DBWorld.MailClient.Mail
{
    public class FwMail : MailObject
    {
        public override MailCore.Models.MailInfo GetMailContext()
        {
            MailContext.Content = GetSignatureStyle() + GetReferenceStyle() + GetMailBody();
            MailContext.Subject = "转发：" + MailContext.Subject;
            MailContext.Sender = String.Format("{0}<{1}>;", MsgConfig.UserName, MsgConfig.Email);
            MailContext.Recivers = "";
            MailContext.CC = "";
            MailContext.BCC = "";
           

            return MailContext;
        }
    }
}
