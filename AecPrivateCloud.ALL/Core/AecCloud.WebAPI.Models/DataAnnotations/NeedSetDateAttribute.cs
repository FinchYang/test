using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Models.DataAnnotations
{
    public class NeedSetDateAttribute : ValidationAttribute
    {
        private readonly DateTime _minValue = DateTime.MinValue;

        public override bool IsValid(object value)
        {
            if (value == null) return false;
            var val = (DateTime)value;
            return val > _minValue;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessage, name);
        }
    }
}
