using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MSMQ.Core;

namespace MsmqWinServer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Start();
        }
        private static void Start()
        {
            System.Console.WriteLine("开始执行");
            var i = 0L;
            while (true)
            {
                try
                {
                    i++;
                    System.Console.WriteLine("第{0}次执行，{1}", i, DateTime.Now);
                    RunMfileOps();
                }
                catch (Exception ex)
                {
                    //var ex0 = ex.InnerException ?? ex;
                    System.Console.WriteLine("【未知错误(root)】" + ex.Message);
                }
                finally
                {
                    Thread.Sleep(LocalConfig.GetSleepTime());
                }
            }
        }

        private static void RunMfileOps()
        {
            try
            {
                var adminUser = LocalConfig.GetAdminUser();
                var pcName = LocalConfig.GetComputerFullName();
                //Trace.TraceInformation("【消息队列】{0}", adminUser.ServerIp);

                var mqs = MsmqOps.GetPrivateMqList(pcName);
                System.Console.WriteLine("【私有队列】队列数量：{0}个",mqs.Count);
                foreach (string mq in mqs)
                {
                    System.Console.WriteLine("【私有队列】" + mq);
                }
                
                var mfManage = new VaultManagement(adminUser);
                if (mfManage.GetServerApplication() == null)
                {
                    System.Console.WriteLine("【连接失败】{0}:{1},连接MFiles服务器失败，{2}", adminUser.Name, adminUser.Pwd, DateTime.Now);
                    return;
                }
                var vaults = mfManage.GetVaultList();
                var mfOps = new MfOperations();
                foreach (MFVaultInfo v in vaults)
                {
                    //var queueName = v.Guid+"_10";
                    var queueName = "private$\\" + v.Guid + "_10";
                    //var queueName = pcName + "\\" + v.Guid + "_10";
                    var queueName2 = @"FormatName:Direct=OS:" + pcName + "\\private$\\" + v.Guid + "_10";
                    if (!MsmqOps.IsExisted(mqs, queueName))
                    {
                        continue;
                    }
                    System.Console.WriteLine("【消息队列】{0},已匹配到", queueName);

                    var msgCount = MsmqOps.GetMsgCount(queueName2);
                    System.Console.WriteLine("【消息队列】{0},消息数量为：{1}", queueName, msgCount);
                    if (msgCount == 0)
                    {
                        continue;
                    }

                    var mfVault = mfManage.GetServerVault(v.Guid);
                    for (int i = 0; i < msgCount; i++)
                    {
                        var mfTask = MsmqOps.ReceiveMfTaskMsg(queueName2);
                        if (mfTask == null) continue;
                        System.Console.WriteLine("【读取消息】[MFiles]{0}，{1}", mfTask.Title, DateTime.Now);
                        try
                        {
                            var objVern = mfOps.CreateNotice(mfVault, mfTask);
                            System.Console.WriteLine("【新建成功(Vault)】{0}，{1}", objVern.Title, DateTime.Now);
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine("【新建通知对象错误(Vault)】" + ex.Message);
                            MsmqOps.SendComplexMsg(queueName2, mfTask);
                            System.Console.WriteLine("【消息入队列】[MFiles]{0}，{1}", mfTask.Title, DateTime.Now);
                        }
                    }

                    mfManage.VaultLogOut(mfVault);
                }
                mfManage.ServerAppDisconnect();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("【未知错误(vault)】" + ex.Message);
            }
            finally
            {
                GC.Collect(0, GCCollectionMode.Forced);
            }

        }
    }
}
