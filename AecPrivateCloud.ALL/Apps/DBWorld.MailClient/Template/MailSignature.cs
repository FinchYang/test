using System;
using System.IO;

namespace DBWorld.MailClient
{
    public class MailSignature
    {
        public string Date { get; set; }
        public string Address { get; set; }


        /// <summary>
        /// 签名
        /// </summary>
        /// <returns></returns>
        public string FormatSign()
        {
            string content;

            try
            {
                var templatePath = Environment.CurrentDirectory + "\\style\\mail_sign_def.htm";
                using (var sr = new StreamReader(templatePath))
                {
                    content = sr.ReadToEnd();
                    sr.Close();
                }

                content = content.Replace("{{sign_date}}", Date);
                content = content.Replace("{{sign_addr}}", Address);
            }
            catch (Exception)
            {
                return null;
            }

            return content;
        }
    }
}
