using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace DBWorld.DesignCloud.Util
{
    public class NotNullValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(value as string) || string.IsNullOrWhiteSpace(value as string))
            {
                return new ValidationResult(false, "不能为空！");
            }
            return new ValidationResult(true, null);
        }
    }

    public class IPAddressRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            var ipAddress = value as string;

            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                var IPAddressFormartRegex =
                    @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$";

                // 检查输入的字符串是否符合IP地址格式
                if (!Regex.IsMatch(ipAddress, IPAddressFormartRegex))
                {
                    return new ValidationResult(false, "IP地址格式不正确！");
                }
            }
            return new ValidationResult(true, null);
        }
    }

    public class EmailAddressRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            var emailAddress = value as string;

            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                var EmailAddressFormartRegex =
                    @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

                // 检查输入的字符串是否符合IP地址格式
                if (!Regex.IsMatch(emailAddress, EmailAddressFormartRegex))
                {
                    return new ValidationResult(false, "Email地址格式不正确！");
                }
            }
            return new ValidationResult(true, null);
        }
    }
}

