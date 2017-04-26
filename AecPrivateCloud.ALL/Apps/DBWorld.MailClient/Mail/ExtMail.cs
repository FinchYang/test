using System;
using System.Collections.Generic;

namespace DBWorld.MailClient.Mail
{
    public class ExtMail : MailObject
    {
        public override MailCore.Models.MailInfo GetMailContext()
        {
            MailContext.Content = GetSignatureStyle();
            MailContext.Sender = String.Format("{0}<{1}>;", MsgConfig.UserName, MsgConfig.Email);
            MailContext.Recivers = "";
            MailContext.CC = "";
            MailContext.BCC = "";
            MailContext.AttachsPath = new List<string>();
            MailContext.MailPath = "";

            return MailContext;
        }
    }
}
