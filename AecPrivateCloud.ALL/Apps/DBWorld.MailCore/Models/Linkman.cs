using System;

namespace DBWorld.MailCore.Models
{
    public class Linkman : IEquatable<Linkman>
    {
        /// <summary>
        /// 联系人名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 联系人ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 联系人邮箱
        /// </summary>
        public string Mail { get; set; }

        /// <summary>
        /// 内部用户
        /// </summary> 
        public string InnerUser { get; set; }

        /// <summary>
        /// 抄送
        /// </summary>
        public string Cc { get; set; }

        public bool Equals(Linkman other)
        {
            if (other == null) return false;
            return Mail == other.Mail;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Linkman;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        } 
    }
}
