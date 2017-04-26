using System;
using System.Net.Mail;
using System.Text;
using MailKit.Net.Pop3;

namespace DBWorld.MailCore.Common
{
    public class MailConfig : IEquatable<MailConfig>
    {
        public const int PopDefPort = 110;
        public const int PopSSLPort = 995;
        public const int SmtpDefPort = 25;
        public const int SmtpSSLPort = 465;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get; set; }

        /// <summary>
        /// 接收邮件服务器地址
        /// </summary>
        public string RecvAddr { get; set; }

        /// <summary>
        /// 发送邮件服务器地址
        /// </summary>
        public string SendAddr { get; set; }

        /// <summary>
        /// 接收邮件是否开启SSL链接
        /// </summary>
        public bool RecvSSL { get; set; }

        /// <summary>
        /// 发送邮件是否开启SSL链接
        /// </summary>
        public bool SendSSL { get; set; }

        /// <summary>
        /// 接收邮件服务器端口号
        /// </summary>
        public int RecvPort { get; set; }

        /// <summary>
        /// 发送邮件服务器端口号
        /// </summary>
        public int SendPort { get; set; }

        /// <summary>
        /// 标记
        /// </summary>
        public string MarkUp { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }


        public bool Equals(MailConfig other)
        {
            if (other == null) return false;
            var objB = other;
            var objA = this;
            var strEqual = StringComparer.OrdinalIgnoreCase;
            if (!strEqual.Equals(objA.UserName, objB.UserName)) return false;
            if (!strEqual.Equals(objA.Email, objB.Email)) return false;
            if (!strEqual.Equals(objA.PassWord,objB.PassWord)) return false;
            if (!strEqual.Equals(objA.RecvAddr,objB.RecvAddr)) return false;
            if (!strEqual.Equals(objA.SendAddr,objB.SendAddr)) return false;
            if (objA.RecvSSL != objB.RecvSSL) return false;
            if (objA.SendSSL != objB.SendSSL) return false;
            if (objA.RecvPort != objB.RecvPort) return false;
            if (objA.SendPort != objB.SendPort) return false;
            if (!strEqual.Equals(objA.MarkUp,objB.MarkUp)) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as MailConfig;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Email == null ? 0 : Email.GetHashCode();
        }
        /// <summary>
        /// 测试接收邮件
        /// </summary>
        public string TestReceiveEmails()
        {
            try
            {
                using (var client = new Pop3Client())
                {
                    client.Connect(RecvAddr, RecvPort, RecvSSL);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(Email, PassWord);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return String.Empty;
        }

        public void TestSendEmail()
        {
            using (var msg = new MailMessage
            {
                From = new MailAddress(Email),
                Subject = "DBWorld测试邮件",
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                Body = ""
            })
            {
                msg.To.Add(new MailAddress(Email));
                SendEmail(msg);
            }
        }

        private void SendEmail(MailMessage mail)
        {
            using (var client = new SmtpClient
            {
                Host = SendAddr,
                Credentials = new System.Net.NetworkCredential(Email, PassWord),
                EnableSsl = SendSSL
            })
            {
                client.Send(mail);
            }
        }


    }
}
