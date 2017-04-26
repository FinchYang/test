
using System;
using System.Windows.Documents;

namespace DBWorld.DesignCloud.Models
{
    public class UserModel : ICloneable
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; } 

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public byte[] Image { get; set; }

        /// <summary>
        /// 用户email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 用户是否被选中
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string MobilePhoneNum { get; set; }

        /// <summary>
        /// 电话号
        /// </summary>
        public string TelephoneNum { get; set; }

        /// <summary>
        /// QQ号
        /// </summary>
        public string QQNum { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// 身份
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var obj = MemberwiseClone() as UserModel;
            if (obj != null)
            {
                if (obj.Image != null)
                {
                    var bytes = new byte[obj.Image.Length];
                    Array.Copy(obj.Image, bytes, obj.Image.Length);
                    obj.Image = bytes;
                }
            }

            return obj;
        }
    }
}
