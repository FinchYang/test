using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SimulaDesign.MfBimInfo
{
    public class CustomTextListener : TextWriterTraceListener
    {
        public CustomTextListener()
            : base()
        {

        }

        public CustomTextListener(string fileName) : base(fileName) { }

        private string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("HH:mm:ss") + "\t";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">should not be whitespaces</param>
        /// <returns></returns>
        private string AddTimeStr(string message)
        {
            var timeStr = GetCurrentDateTime();
            return timeStr + message;
        }

        public override void Write(string message)
        {
            message = AddTimeStr(message);
            base.Write(message);
        }

        /// <summary>
        /// probably all writeline method will call this method
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string message)
        {
            //message = AddTimeStr(message);
            base.WriteLine(message);
        }

        //public override void WriteLine(string message, string category)
        //{
        //    base.WriteLine(message, category); // will call WriteLine(string message)
        //}
    }
}
