using System.Collections.Generic;

namespace DBWorld.MailCore.Models
{
    public class MailInfo
    {
        /// <summary>
        /// 新邮件
        /// </summary>
        public const int MAILTYPE_NEW = 0;

        /// <summary>
        /// 草稿邮件
        /// </summary>
        public const int MAILTYPE_DARFT = 1;

        /// <summary>
        /// 回复的邮件
        /// </summary>
        public const int MAILTYPE_RE = 2;

        /// <summary>
        /// 回复所有的邮件
        /// </summary>
        public const int MAILTYPE_REALL = 3;

        /// <summary>
        /// 转发的邮件
        /// </summary>
        public const int MAILTYPE_FW = 4;

        /// <summary>
        /// 外部邮件
        /// </summary>
        public const int MAILTYPE_EXT = 5;

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 发送者
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// 接收者
        /// </summary>
        public string Recivers { get; set; }

        /// <summary>
        /// 抄送
        /// </summary>
        public string CC { get; set; }

        /// <summary>
        /// 密送
        /// </summary>
        public string BCC { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 附件路径
        /// </summary>
        public List<string> AttachsPath { get; set; }

        /// <summary>
        /// 邮件路径
        /// </summary>
        public string MailPath { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public string SentTime { get; set; }

        /// <summary>
        /// 邮件正文
        /// </summary>
        public string Content { get; set; }

    }
}
