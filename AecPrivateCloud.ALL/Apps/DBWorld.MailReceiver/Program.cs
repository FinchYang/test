﻿using System;
using System.Threading;
using System.Windows.Forms;
using DBWorld.MailCore.MF;

namespace DBWorld.MailReceiver
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool exist;
            var handle = new EventWaitHandle(false, EventResetMode.AutoReset, "MailReceiverEvent", out exist);
            // 如果该命名事件已经存在(存在有前一个运行实例)，则发事件通知并退出  
            if (!exist)
            {
                handle.Set();
                return;
            }  

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                MFilesUtil.VaultName = args[1];
            }
            MailCore.Common.Logger.Configure();
            Application.Run(new MainFrm());
        }
    }
}
