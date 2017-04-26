using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.BaseCore;

namespace AecCloud.WebAPI.Models
{
    public class UserProfileModel
    {
        //[Obsolete("从Token中获取，不需要提供")]
        //public int Id { get; set; }
        [Display(Name = "姓名")]
        public string Name { get; set; }
        [Display(Name = "头像")]
        public byte[] Image { get; set; }
        [Display(Name = "邮箱")]
        [RegularExpression(Utility.EmailPattern, ErrorMessage = "{0}的格式不正确")]
        public string Email { get; set; }
        [Display(Name = "电话")]
        public string Phone { get; set; }
        [Display(Name = "QQ")]
        public string QQ { get; set; }
        [Display(Name = "行业")]
        public string Industry { get; set; }
        [Display(Name = "公司")]
        public string Company { get; set; }
        [Display(Name = "部门")]
        public string Department { get; set; }
        [Display(Name = "职位")]
        public string Post { get; set; }
        [Display(Name = "个人说明")]
        public string Description { get; set; }
    }
}
