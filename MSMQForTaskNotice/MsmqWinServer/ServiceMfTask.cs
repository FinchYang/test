using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MSMQ.Core;

namespace MsmqWinService
{
    public partial class ServiceMfTask : ServiceBase
    {
        public ServiceMfTask()
        {
            InitializeComponent();
            CanStop = true;
        }

        private Thread _thread;

        protected override void OnStart(string[] args)
        {

            if (_thread == null)
            {
                _thread = new Thread(Start);
                _thread.Start();
            }
        }

        protected override void OnStop()
        {
            if (_thread != null)
            {
                _thread.Abort();
            }
        }

        private void Start()
        {
            var traceFile = GetTraceFile();
            Trace.AutoFlush = true;
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(traceFile));

            Trace.TraceInformation("开始执行");
            var i = 0L;
            while (true)
            {
                //设置Log
                var traceFileTemp = GetTraceFile();
                if (traceFile != traceFileTemp)
                {
                    traceFile = traceFileTemp;
                    Trace.Listeners.Clear();
                    Trace.Listeners.Add(new TextWriterTraceListener(traceFile));
                }

                try
                {
                    i++;
                    Trace.TraceInformation("第{0}次执行，{1}", i, DateTime.Now);
                    RunMfileOps();
                }
                catch (Exception ex)
                {
                    //var ex0 = ex.InnerException ?? ex;
                    Trace.TraceInformation("【未知错误(root)】" + ex.Message);
                }
                finally
                {
                    Thread.Sleep(LocalConfig.GetSleepTime());
                }
            }
        }

        private string GetTraceFile(long i = 0)
        {
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var date = DateTime.Now.Date.ToString("yy-MM-dd");
            var traceFile = basePath + "\\winServerLog" + date + ".txt";
            //if (i > 0)
            //{
            //    traceFile = basePath + "\\winServerLog" + date +i+ ".txt";
            //}
            return traceFile;
        }
        private void RunMfileOps()
        {
            try
            {
                var adminUser = LocalConfig.GetAdminUser();
                var pcName = LocalConfig.GetComputerFullName();
                //Trace.TraceInformation("【消息队列】{0}", adminUser.ServerIp);

                var mqs = MsmqOps.GetPrivateMqList(pcName);
                Trace.TraceInformation("【私有队列】队列数量：{0}个", mqs.Count);
                foreach (string mq in mqs)
                {
                    Trace.TraceInformation("【私有队列】" + mq);
                }

                var mfManage = new VaultManagement(adminUser);
                if (mfManage.GetServerApplication() == null)
                {
                    Trace.TraceError("【连接失败】{0}:{1},连接MFiles服务器失败，{2}", adminUser.Name, adminUser.Pwd, DateTime.Now);
                    return;
                }

                var vaults = mfManage.GetVaultList();
                var mfOps = new MfOperations();
                foreach (MFVaultInfo v in vaults)
                {
                    var queueName = "private$\\" + v.Guid + "_10";
                    //var queueName = pcName + "\\" + v.Guid + "_10";
                    var queueName2 = @"FormatName:Direct=OS:" + pcName + "\\private$\\" + v.Guid + "_10";
                    if (!MsmqOps.IsExisted(mqs, queueName))
                    {
                        continue;
                    }
                    Trace.TraceInformation("【消息队列】{0},已匹配到", queueName);

                    var msgCount = MsmqOps.GetMsgCount(queueName2);
                    Trace.TraceInformation("【消息队列】{0},消息数量为：{1}", queueName, msgCount);
                    if (msgCount == 0)
                    {
                        continue;
                    }

                    var mfVault = mfManage.GetServerVault(v.Guid);
                    for (int i = 0; i < msgCount; i++)
                    {
                        var mfTask = MsmqOps.ReceiveMfTaskMsg(queueName2);
                        if (mfTask == null) continue;
                        Trace.TraceInformation("【读取消息】[MFiles]{0}，{1}", mfTask.Title, DateTime.Now);
                        try
                        {
                            var objVern = mfOps.CreateNotice(mfVault, mfTask);
                            Trace.TraceInformation("【新建成功(Vault)】{0}，{1}", objVern.Title, DateTime.Now);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("【新建错误(Vault)】" + ex.Message);

                            MsmqOps.SendComplexMsg(queueName2, mfTask);
                            Trace.TraceInformation("【消息入队列】[MFiles]{0}，{1}", mfTask.Title, DateTime.Now);
                        }
                    }

                    mfManage.VaultLogOut(mfVault);
                }
                mfManage.ServerAppDisconnect();
            }
            catch (Exception ex)
            {
                Trace.TraceError("【未知错误(vault)】" + ex.Message);
            }
            finally
            {
                GC.Collect(0, GCCollectionMode.Forced);
            }

        }
    }
}
