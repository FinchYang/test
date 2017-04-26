namespace DBWorld.MailClient.Mail
{
    public class DraftMail : MailObject
    {
        public override MailCore.Models.MailInfo GetMailContext()
        {
            MailContext.Content = GetMailBody();

            return MailContext;
        }
    }
}
