using System.Xml.Serialization;

namespace DBWorld.Config.Models
{
    [XmlRootAttribute("UserConfig")]
    public class UserConfigModel
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
        /// 开机自动运行
        /// </summary>
        [XmlElementAttribute("AutoRun")]
        public long AutoRun { get; set; }

        /// <summary>
        /// 关闭时最小化到系统托盘
        /// </summary>
        [XmlElementAttribute("ToTray")]
        public long ToTray { get; set; }

        /// <summary>
        /// 启动后小窗口运行
        /// </summary>
        [XmlElementAttribute("NomalWindow")]
        public long NomalWindow { get; set; }

        /// <summary>
        /// 最后一个登录
        /// </summary>
        [XmlElementAttribute("LastLoginTime")] 
        public string LastLoginTime { get; set; }

        /// <summary>
        /// 登录中
        /// </summary>
        [XmlElementAttribute("OnLine")] 
        public long OnLine { get; set; }

    }
}
