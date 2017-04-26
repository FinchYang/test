
using DBWorld.MailCore.Models;

namespace DBWorld.MailClient.Mail
{
    public class MailFactory
    {
        public static MailObject CreateInstance(int type)
        {
            MailObject mailContext = null;
            switch (type)
            {
                case MailInfo.MAILTYPE_NEW:
                    mailContext = new NewMail();
                    break;
                case MailInfo.MAILTYPE_DARFT:
                    mailContext = new DraftMail();
                    break;
                case MailInfo.MAILTYPE_RE:
                    mailContext = new ReMail();
                    break;
                case MailInfo.MAILTYPE_REALL:
                    mailContext = new ReAllMail();
                    break;
                case MailInfo.MAILTYPE_FW:
                    mailContext = new FwMail();
                    break;
                case MailInfo.MAILTYPE_EXT:
                    mailContext = new ExtMail();
                    break;
            }

            return mailContext;
        }
    }
}
