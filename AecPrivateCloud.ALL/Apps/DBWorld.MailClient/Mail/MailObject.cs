using System;
using System.IO;
using System.Text;
using DBWorld.MailClient.Util;

namespace DBWorld.MailClient.Mail
{
    public abstract class MailObject
    {
        public MailCore.Common.MailConfig MsgConfig { get; set; }

        public MailCore.Models.MailInfo MailContext { get; set; }

        public virtual MailCore.Models.MailInfo GetMailContext()
        {
            return MailContext;
        }

        protected virtual string GetSignatureStyle()
        {
            return "<div>&nbsp;</div><div>&nbsp;</div>" + MsgConfig.Signature;
        }

        protected virtual string GetReferenceStyle()
        {
            if (MailContext == null) return null;

            var refer = new MailReference
            {
                Subject = MailContext.Subject,
                SentTime = MailContext.SentTime,
                Sender = MailUtil.FormatToContacts(MailContext.Sender)
            };

            foreach (var str in MailContext.Recivers.Split(';'))
            {
                if (!string.IsNullOrEmpty(str))
                {
                    refer.Receivers.Add(MailUtil.FormatToContacts(str));
                }
            }

            foreach (var str in MailContext.CC.Split(';'))
            {
                if (!string.IsNullOrEmpty(str))
                {
                    refer.CC.Add(MailUtil.FormatToContacts(str));
                }
            }

            return refer.FormatReference();
        }

        protected string GetMailBody()
        {
            if (String.IsNullOrEmpty(MailContext.MailPath)) return null;

            string content;
            var encode = Encoding.GetEncoding(MailUtil.GetEncodeString(MailContext.MailPath));
            using (var reader = new StreamReader(MailContext.MailPath, encode))
            {
                content = reader.ReadToEnd();
            }

            return content;
        }
    }
}
