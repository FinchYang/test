using System;
using System.Collections.Generic;
using System.IO;

namespace DBWorld.MailClient
{
    /// <summary>
    /// 发件人： xxx@xxx.com
    /// 发送时间： 2015-06-19 10:10
    /// 收件人： xxx
    /// 抄送 : xxx
    /// 密送 : xxx
    /// 主题： 2015年6月19日10:10:34
    /// </summary>
    public class MailReference
    {
        public string Subject { get; set; }

        public MailContact Sender { get; set; }

        public string SentTime { get; set; }

        public readonly List<MailContact> Receivers = new List<MailContact>();

        public readonly List<MailContact> CC = new List<MailContact>();

        public readonly List<MailContact> BCC = new List<MailContact>();

        /// <summary>
        /// 格式引用内容
        /// </summary>
        /// <returns></returns>
        public string FormatReference()
        {
            var content = string.Empty;

            try
            {
                var templatePath = Environment.CurrentDirectory + "\\style\\mail_head_ref.htm";
                using (var sr = new StreamReader(templatePath))
                {
                    content = sr.ReadToEnd();
                    sr.Close();
                }

                content = FormatSender(content);
                content = FormatSentTime(content);
                content = FormatReceivers(content);
                content = FormatCC(content);
                content = FormatBCC(content);
                content = FormatSubject(content);
            }
            catch (Exception)
            {
            }

            return content;
        }

        private string FormatSender(string html)
        {
            if (Sender == null)
            {
                return html.Replace("{{mail_sender}}", ""); ;
            }
            var addr = Sender.UserName + "@" + Sender.Host;
            var divContent = string.Format("<b>发件人：</b>&nbsp;<a href=\"mailto:{0}\">{1}</a>", addr, Sender.DisplayName);
            return html.Replace("{{mail_sender}}", divContent);
        }

        private string FormatSentTime(string html)
        {
            if (string.IsNullOrEmpty(SentTime))
            {
                return html.Replace("{{mail_senttime}}", "");
            }
            var divContent = string.Format("<b>发送时间：</b>&nbsp;{0}", SentTime);
            return html.Replace("{{mail_senttime}}", divContent);
        }

        private string FormatReceivers(string html)
        {
            if (Receivers == null || Receivers.Count == 0)
            {
                return html.Replace("{{mail_receiver}}", ""); ;
            }
            var divContent = "<b>收件人：</b>";
            for (int i = 0; i < Receivers.Count; i++)
            {
                var contact = Receivers[i];
                 var addr = contact.UserName + "@" + contact.Host;
                var item = string.Format("&nbsp;<a href=\"mailto:{0}\">{1}</a>", addr, contact.DisplayName);
                if (i != Receivers.Count - 1)
                {
                    item += ";";
                }

                divContent += item;
            }

            return html.Replace("{{mail_receiver}}", divContent);
        }

        private string FormatCC(string html)
        {
            if (CC == null || CC.Count == 0)
            {
                return html.Replace("{{mail_cc}}", ""); ;
            }
            var divContent = "<b>抄送：</b>";
            for (int i = 0; i < CC.Count; i++)
            {
                var contact = CC[i];
                var addr = contact.UserName + "@" + contact.Host;
                var item = string.Format("&nbsp;<a href=\"mailto:{0}\">{1}</a>", addr, contact.DisplayName);
                if (i != CC.Count - 1)
                {
                    item += ";";
                }
                divContent += item;
            }
            return html.Replace("{{mail_cc}}", divContent);
        }

        private string FormatBCC(string html)
        {
            if (BCC == null || BCC.Count == 0)
            {
                return html.Replace("{{mail_bcc}}", ""); ;
            }
            var divContent = "<b>密送：</b>";
            for (int i = 0; i < BCC.Count; i++)
            {
                var contact = BCC[i];
                var addr = contact.UserName + "@" + contact.Host;
                var item = string.Format("&nbsp;<a href=\"mailto:{0}\">{1}</a>", addr, contact.DisplayName);
                if (i != BCC.Count - 1)
                {
                    item += ";";
                }
                divContent += item;
            }
            return html.Replace("{{mail_bcc}}", divContent);
        }

        private string FormatSubject(string html)
        {
            var divContent = string.Format("<b>主题：</b>&nbsp;{0}", Subject);
            return html.Replace("{{mail_subject}}", divContent);
        }
    }
}
