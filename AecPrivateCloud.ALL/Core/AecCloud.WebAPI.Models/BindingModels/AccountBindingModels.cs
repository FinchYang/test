using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.BaseCore;
using Newtonsoft.Json;


namespace AecCloud.WebAPI.Models
{
    public class TokenModel
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        //[JsonProperty(PropertyName = "userName")]
        //public string UserName { get; set; }

        //[JsonProperty(PropertyName = "activated")]
        //public string Activated { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        public string GetErrorMessage()
        {
            if (Success) return String.Empty;
            if (Errors.Count > 0)
            {
                return String.Join("; ", Errors);
            }
            return String.Empty;
        }

        public bool Success { get; set; }

        private IDictionary<string, string> _errors;

        public IDictionary<string, string> Errors
        {
            get { return _errors ?? (_errors = new Dictionary<string, string>()); }
            set { _errors = value; }
        }

    }

    public class EmailActivateModel
    {
        [Required(ErrorMessage = "邮箱必填.")]
        [Display(Name = "邮箱")]
        [RegularExpression(Utility.EmailPattern, ErrorMessage = "{0}的格式不正确")]
        public string Email { get; set; }
    }
    public class RegisterBindingModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "邮箱必填.")]
        [Display(Name = "邮箱")]
        [RegularExpression(Utility.EmailPattern, ErrorMessage = "{0}的格式不正确")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0}至少{2}位。", MinimumLength = Utility.MinimumLength)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码必须一致！")]
        public string ConfirmPassword { get; set; }
    }
    public class RegisterErrorModel
    {
        public RegisterErrorModel()
        {
            Message = String.Empty;
        }

        public bool Success { get; set; }

        public string Message { get; set; }
        public RegisterError Error { get; set; }
    }

    public enum RegisterError
    {
        Username,
        Email
    }

    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }
                                                                                                                                                      
        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }

    public class SetPasswordModel
    {
        [Required(ErrorMessage = "邮箱必填.")]
        [Display(Name = "邮 箱：")]
        [RegularExpression(Utility.EmailPattern, ErrorMessage = "{0}的格式不正确")]
        public string Email { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }

    public class LoginStatusModel
    {
        public string Ip { get; set; }

        public DateTime LoginDateUtc { get; set; }


    }
}
