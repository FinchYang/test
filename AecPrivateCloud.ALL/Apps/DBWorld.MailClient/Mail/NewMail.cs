using System;

namespace DBWorld.MailClient.Mail
{
    public class NewMail : MailObject
    {
        public override MailCore.Models.MailInfo GetMailContext()
        {
            MailContext.Content = GetSignatureStyle();
            MailContext.Sender = String.Format("{0}<{1}>;", MsgConfig.UserName, MsgConfig.Email);
            MailContext.Recivers = "";
            MailContext.CC = "";
            MailContext.BCC = "";
            MailContext.AttachsPath = null;
            MailContext.MailPath = "";

            return MailContext;
        }
    }
}
