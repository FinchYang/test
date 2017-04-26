
namespace DBWorld.MailClient
{
    /// <summary>
    /// 联系人格式 : displayname<username@host>
    /// </summary>
    public class MailContact
    {
        public int Id { get; set; }

        public string DisplayName { get; set; }

        public string UserName { get; set; }

        public string Host { get; set; }

    }
}
