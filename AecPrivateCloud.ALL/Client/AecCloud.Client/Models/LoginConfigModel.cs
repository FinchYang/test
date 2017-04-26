using System.Xml.Serialization;

namespace AecCloud.Client.Models
{
    [XmlRootAttribute("LoginConfig")]
    public class LoginConfigModel
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [XmlElementAttribute("UserId", IsNullable = false)] 
        public long UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [XmlElementAttribute("UserName")] 
        public string UserName { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
         [XmlElementAttribute("UserPwd")] 
        public string UserPwd { get; set; }

        /// <summary>
        /// 是否记住密码
        /// </summary>
        [XmlElementAttribute("RememberPwd")] 
        public int RememberPwd { get; set; }

        /// <summary>
        /// 是否自动登录
        /// </summary>
        [XmlElementAttribute("AutoLogin")] 
        public int AutoLogin { get; set; }

        /// <summary>
        /// 最后一个登录
        /// </summary>
        [XmlElementAttribute("LastLoginTime")] 
        public string LastLoginTime { get; set; }
    }
}
