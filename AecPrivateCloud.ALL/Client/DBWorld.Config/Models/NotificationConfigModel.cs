using System.Xml.Serialization;

namespace DBWorld.Config.Models
{
    [XmlRootAttribute("NotificationConfig")]
    public class NotificationConfigModel
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [XmlElementAttribute("UserId", IsNullable = false)]
        public long UserId { get; set; }

        /// <summary>
        /// 新任务邮件通知
        /// </summary>
        [XmlElementAttribute("NewTaskEmail")]
        public long NewTaskEmail { get; set; }

        /// <summary>
        /// 新任务托盘通知
        /// </summary>
        [XmlElementAttribute("NewTaskTray")]
        public long NewTaskTray { get; set; }

        /// <summary>
        /// 新共享文档邮件通知
        /// </summary>
        [XmlElementAttribute("NewShareDocEmail")]
        public long NewShareDocEmail { get; set; }

        /// <summary>
        /// 新共享文档托盘通知
        /// </summary>
        [XmlElementAttribute("NewShareDocTray")]
        public long NewShareDocTray { get; set; }

        /// <summary>
        /// 新邮件托盘通知
        /// </summary>
        [XmlElementAttribute("NewEmailTray")]
        public long NewEmailTray { get; set; }

        /// <summary>
        /// 任务快过期邮件通知
        /// </summary>
        [XmlElementAttribute("ExpiredTaskEmail")]
        public long ExpiredTaskEmail { get; set; }

        /// <summary>
        /// 任务快过期托盘通知
        /// </summary>
        [XmlElementAttribute("ExpiredTaskTray")]
        public long ExpiredTaskTray { get; set; }
    }
}
