using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using AecCloud.BaseCore;
using AecCloud.WebAPI.Models;

namespace DBWorld.AecCloud.Web.Models
{
    public class InviteModel
    {
        [Required(ErrorMessage = "{0}必填.")]
        [Display(Name = "被邀请人的邮箱")]
        [RegularExpression(Utility.EmailPattern, ErrorMessage = "{0}的格式不正确")]
        public string InviteEmail { get; set; }
        //[Required(ErrorMessage = "{0}必填.")]
        [Display(Name = "邀请人的邮箱")]
        //[RegularExpression(Utility.EmailPattern, ErrorMessage = "{0}的格式不正确")]
        public string Email { get; set; }
        [Display(Name = "认证信息")]
        public string TokenJson { get; set; }
        [Display(Name = "项目名称")]
        //[Required(ErrorMessage = "{0}必填.")]
        public string ProjectName { get; set; }
        [Display(Name = "项目ID")]
        public int ProjectId { get; set; }
        [Display(Name = "被邀请人的用户ID")]
        public int UserId { get; set; }
        [Display(Name = "参与方ID")]
        public int PartyId { get; set; }
        [Display(Name = "参与方名称")]
        //[Required(ErrorMessage = "{0}必填.")]
        public string PartyName { get; set; }
        [Display(Name = "项目库中的用户ID")]
        [Required(ErrorMessage = "必须指定{0}")]
        [Range(1, int.MaxValue, ErrorMessage = "{0}必须是正整数")]
        public int MFUserId { get; set; }

        public int BidProjId { get; set; }
    }
    public class BIMModel
    {
        public List<ProjectDto> ProjectDto { get; set; }

        public ProjectCreateModel ProjectCreateModel { get; set; }
    }
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "旧密码")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0}至少包含 {2} 个字符。", MinimumLength = Utility.MinimumLength)]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        [Compare("NewPassword", ErrorMessage = "两次输入的密码不同.")]
        public string ConfirmPassword { get; set; }
    }
    public class LoginViewModel
    {
        public LoginViewModel()
        {
            IsDomainUser = false;
        }

        [Required]
        [Display(Name = "账号")]
        public string UsernameOrEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我（下次自动登录）")]
        public bool RememberMe { get; set; }

        [Display(Name="是否是域账户?")]
        public bool IsDomainUser { get; set; }
    }

    public class ForgetPasswordModel
    {
        public EmailCodeModel EmailCodeModel { get; set; }
        public ValidateCodeModel ValidateCodeModel { get; set; }

        public ChangePasswordModel ChangePasswordModel { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "请输入密码.")]
        [StringLength(100, ErrorMessage = "{0}至少包含 {2} 个字符。", MinimumLength = Utility.MinimumLength)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "请再次输入密码.")]
        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("NewPassword", ErrorMessage = "两次输入密码不同.")]
        public string ConfirmPassword { get; set; }
    }

    //找回密码，邮箱验证码
    public class EmailCodeModel
    {
        [Required(ErrorMessage = "验证码必填.")]
        [Display(Name = "验证码：")]
        public string Code { get; set; }

        public string Email { get; set; }
    }

    public class ValidateCodeModel
    {
        [Required(ErrorMessage = "验证码必填.")]
        [Display(Name = "验证码：")]
        public string Code { get; set; }

        [Required(ErrorMessage = "邮箱必填.")]
        [Display(Name = "邮 箱：")]
        [RegularExpression(@"([\.a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+((\.[a-zA-Z0-9_-]{2,3}){1,2})$", ErrorMessage = "{0}的格式不正确")]
        public string Email { get; set; }
    }

    public class ProjectLoginViewModel
    {
        [Required(ErrorMessage = "账号必填.")]
        [Display(Name = "账号")]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "密码必填.")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        //项目ID
        public int ProjectId { get; set; }

        public int UserId { get; set; }
        //参与方ID
        public int PartyId { get; set; }

    }

    public class RegisterViewModel
    {

        [Required(ErrorMessage = "用户名必填.")]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "邮箱必填.")]
        [Display(Name = "邮箱")]
        [RegularExpression(@"([\.a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+((\.[a-zA-Z0-9_-]{2,3}){1,2})$", ErrorMessage = "{0}的格式不正确")]
        public string Email { get; set; }

        [Required(ErrorMessage = "请输入密码.")]
        [StringLength(100, ErrorMessage = "{0}至少包含 {2} 个字符。", MinimumLength = Utility.MinimumLength)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required(ErrorMessage = "请再次输入密码.")]
        [StringLength(100, ErrorMessage = "{0}至少包含 {2} 个字符。", MinimumLength = Utility.MinimumLength)]
        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "两次输入密码不同.")]
        public string ConfirmPassword { get; set; }
    }

    public class UserInfoViewModel
    {
        //public UserInfoViewModel()
        //{
        //    UserImage_Big = string.Empty;
        //    UserImage_Mid = string.Empty;
        //    UserImage_Small = string.Empty;
        //    UserImage_Mini = string.Empty;
        //}
        public long Id { get; set; }
        [Display(Name = "姓名")]
        public string Name { get; set; }

        [Display(Name = "工号")]
        public string WorkerIdentity { get; set; }

        [Display(Name = "个人说明")]
        public string Description { get; set; }

        [Display(Name = "电话")]
        public string Phone { get; set; }

        [Display(Name = "邮箱")]
        public string Email { get; set; }

        [Display(Name = "QQ")]
        [RegularExpression(@"[1-9][0-9]{4,14}", ErrorMessage = "{0}格式不正确")]
        public string QQ { get; set; }

        [Display(Name = "行业")]
        public string Industry { get; set; }

        [Display(Name = "单位")]
        public string Company { get; set; }

        [Display(Name = "部门")]
        public string Department { get; set; }

        [Display(Name = "职位")]
        public string Post { get; set; }
        //  public string UserImage_Big { get; set; }
        //public string UserImage_Mid { get; set; }
        //public string UserImage_Small { get; set; }
        //public string UserImage_Mini { get; set; }
       
    }

    public class ValidateCode
    {
        public ValidateCode()
        {
        }
        /// <summary>
        /// 验证码的最大长度
        /// </summary>
        public int MaxLength
        {
            get { return 10; }
        }
        /// <summary>
        /// 验证码的最小长度
        /// </summary>
        public int MinLength
        {
            get { return 1; }
        }
        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="length">指定验证码的长度</param>
        /// <returns></returns>
        public string CreateValidateCode(int length)
        {
            int[] randMembers = new int[length];
            int[] validateNums = new int[length];
            string validateNumberStr = "";
            //生成起始序列值
            int seekSeek = unchecked((int)DateTime.Now.Ticks);
            Random seekRand = new Random(seekSeek);
            int beginSeek = (int)seekRand.Next(0, Int32.MaxValue - length * 10000);
            int[] seeks = new int[length];
            for (int i = 0; i < length; i++)
            {
                beginSeek += 10000;
                seeks[i] = beginSeek;
            }
            //生成随机数字
            for (int i = 0; i < length; i++)
            {
                Random rand = new Random(seeks[i]);
                int pownum = 1 * (int)Math.Pow(10, length);
                randMembers[i] = rand.Next(pownum, Int32.MaxValue);
            }
            //抽取随机数字
            for (int i = 0; i < length; i++)
            {
                string numStr = randMembers[i].ToString();
                int numLength = numStr.Length;
                Random rand = new Random();
                int numPosition = rand.Next(0, numLength - 1);
                validateNums[i] = Int32.Parse(numStr.Substring(numPosition, 1));
            }
            //生成验证码
            for (int i = 0; i < length; i++)
            {
                validateNumberStr += validateNums[i].ToString();
            }
            return validateNumberStr;
        }

        /// <summary>
        /// 创建验证码的图片
        /// </summary>
        /// <param name="containsPage">要输出到的page对象</param>
        /// <param name="validateNum">验证码</param>
        public byte[] CreateValidateGraphic(string validateCode)
        {
            Bitmap image = new Bitmap((int)Math.Ceiling(validateCode.Length * 12.0), 22);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成随机生成器
                Random random = new Random();
                //清空图片背景色
                g.Clear(Color.White);
                //画图片的干扰线
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }
                Font font = new Font("Arial", 12, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),
                 Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(validateCode, font, brush, 3, 2);
                //画图片的前景干扰点
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                //保存图片数据
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);
                //输出图片流
                return stream.ToArray();
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }
    }
}