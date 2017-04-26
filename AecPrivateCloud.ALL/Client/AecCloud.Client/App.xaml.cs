using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using AecCloud.Client.Models;
using AecCloud.Client.Util;
using AecCloud.Client.ViewModels;
using AecCloud.MFilesCore;
using log4net;
using MFilesAPI;
using SimulaDesign.WPFCustomUI.Controls;
using SimulaDesign.WPFPluginCore;

namespace AecCloud.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool SignalExternalCommandLineArgs(System.Collections.Generic.IList<string> args)
        {
            // Bring window to foreground
            if (this.MainWindow.WindowState == WindowState.Minimized)
            {
                this.MainWindow.WindowState = WindowState.Normal;
            }
            else if (this.MainWindow.Visibility == Visibility.Hidden)
            {
                this.MainWindow.Visibility = Visibility.Visible;
            }

            this.MainWindow.Activate();

            return true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Configure();

            base.OnStartup(e);

            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is AggregateException)
            {
                var ex = e.Exception as AggregateException;
                if (ex.InnerException is System.Runtime.InteropServices.COMException)
                {
                    return;
                }
            }
            else if (e.Exception is System.Runtime.InteropServices.COMException)
            {
                return;
            }
            MetroMessageBox.Show("抱歉，DBWorld遇到一些问题，该操作已经终止，请重新再试！",
                "DBWorld 意外操作",
                MetroMessageBoxButton.OK,
                MetroMessageBoxImage.Error,
                MetroMessageBoxDefaultButton.OK);
            Log.Error("current dispatcher unhandled exception: " + e.Exception.Message, e.Exception);
            e.Handled = true;
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex;
            if (e.ExceptionObject is AggregateException)
            {
                ex = e.ExceptionObject as AggregateException;
                if (ex.InnerException is System.Runtime.InteropServices.COMException)
                {
                    return;
                }
            }
            else if (e.ExceptionObject is System.Runtime.InteropServices.COMException)
            {
                return;
            }
            MetroMessageBox.Show("抱歉，DBWorld遇到一些问题，该操作已经终止，请重新再试！",
                "DBWorld 意外操作",
                MetroMessageBoxButton.OK,
                MetroMessageBoxImage.Error,
                MetroMessageBoxDefaultButton.OK);
            ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                Log.Error("curren domain unhandle exception." + ex.Message, ex);
            }   
        }

        internal static UserModel UserInfo;

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                //OffMF();

              NotifyPluginExit();

            }
            catch
            {
            }

            base.OnExit(e);
        }

        /// <summary>
        /// 通知插件退出
        /// </summary>
        private void NotifyPluginExit()
        {
            //
            var hwnd = IPCUtil.FindWindow(null, "写信");
            if (hwnd != 0)
            {
                IPCUtil.SendMessage(hwnd, IPCUtil.WM_CLOSE, 0, IntPtr.Zero);
            }

            hwnd = IPCUtil.FindWindow(null, "邮箱设置");
            if (hwnd != 0)
            {
                IPCUtil.SendMessage(hwnd, IPCUtil.WM_CLOSE, 0, IntPtr.Zero);
            }

            hwnd = IPCUtil.FindWindow(null, "收取邮件");
            if (hwnd != 0)
            {
                IPCUtil.SendMessage(hwnd, IPCUtil.WM_CLOSE, 0, IntPtr.Zero);
            }
        }

    }
}
