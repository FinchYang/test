using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SimulaDesign.ImportUICore
{
    public class MessageBoxUtil
    {
        public static DialogResult Error(string message)
        {
            return MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static DialogResult Warn(string message)
        {
            return MessageBox.Show(message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static DialogResult Exclamation(string message)
        {
            return MessageBox.Show(message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static DialogResult Question(string message)
        {
            return MessageBox.Show(message, "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
    }
}
