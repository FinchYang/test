using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.BaseCore;

namespace AecCloud.WebAPI.Models
{
    public class SendEmailMessage
    {
        public SendEmailMessage()
        {
            IsHtml = true;
        }

        [Required(ErrorMessage = "发送到的邮箱必填.")]
        [Display(Name = "发送到的邮箱")]
        [RegularExpression(Utility.EmailPattern, ErrorMessage = "{0}的格式不正确")]
        public string MailTo { get; set; }
        [Display(Name="标题")]
        public string Title { get; set; }
        [Display(Name = "邮件内容")]
        public string Body { get; set; }

        public bool IsHtml { get; set; }

        public string EncodingName { get; set; }
    }
   
}
