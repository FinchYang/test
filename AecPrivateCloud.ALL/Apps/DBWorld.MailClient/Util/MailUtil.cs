using System;
using System.IO;
using System.Text;

namespace DBWorld.MailClient.Util
{
    public static class MailUtil
    {
        public static MailContact FormatToContacts(string str)
        {
            var contact = new MailContact();

            try
            {
                var pos1 = str.IndexOf('<');
                if (pos1 != -1)
                {
                    contact.DisplayName = str.Substring(0, pos1);
                }

                var pos2 = str.LastIndexOf('@');
                if (pos2 != -1)
                {
                    contact.UserName = str.Substring(pos1 + 1, pos2 - pos1 - 1);
                }

                contact.Host = str.Substring(pos2 + 1, str.Length - pos2 - 2);
            }
            catch (Exception)
            {
                MailCore.Common.Logger.Log.DebugFormat("Exception : Mail address '{0}' is invalid.", str);
            }

            return contact;
        }

        public static string FormatToContacts(string userName, string address, bool isFlag)
        {
            if (isFlag)
            {
                return String.Format("{0}<{1}>;", userName, address);
            }
            else
            {
                return String.Format("{0}<{1}>", userName, address);
            }
        }

        public static string GetEncodeString(string htmlPath)
        {
            using (var reader = new StreamReader(htmlPath, Encoding.Default))
            {
                var line = reader.ReadLine();
                if (line != null)
                {
                    var pos1 = line.IndexOf("charset=", System.StringComparison.OrdinalIgnoreCase);
                    if (pos1 != -1)
                    {
                        var temp = line.Substring(pos1);
                        var pos2 = temp.IndexOf('=');
                        var pos3 = temp.IndexOf('"');
                        return temp.Substring(pos2 + 1, pos3 - pos2 -1);
                    }
                }
            }

            return "gb2132";
        }
    }
}
