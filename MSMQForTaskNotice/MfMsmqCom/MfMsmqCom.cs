using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using MFilesAPI;

namespace MfMsmqCom
{
    [Guid("91EDA4AA-16FF-4F3A-9935-E0E6E24B0EAD")]
    public interface IMsmqTask
    {
        [DispId(1)]
        //void AddNotice2Msmq(Vault vault, int objType, int objClass, string title, string content, int assignTo, int otherPropDef, int otherPropValue);
        void AddNotice2Msmq(Vault vault, int objType, int objClass, string title, string content, int assignTo, string otherPropValues);
    }

    [Guid("9DB474EF-3439-4C7F-BC30-E3A6953D7D61")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("MfMsmqCom.MsmqTask")]
    public class MsmqTask : IMsmqTask
    {
        public void Writelog(string tmpfile, string logtext)
        {
            try
            {
                using (var sw = System.IO.File.AppendText(tmpfile))
                {
                    sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
                    sw.Close();
                }
            }
            catch (Exception) { }
        }
        public void Log(string logtext)
        {
            try
            {
                using (var sw = System.IO.File.AppendText( Path.Combine( Path.GetTempPath(),"msmqtaskcomlog.txt")))
                {
                    sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
                    sw.Close();
                }
            }
            catch (Exception) { }
        }
        public void AddNotice2Msmq(Vault vault, int objType, int objClass, string title, string content, int assignTo, string otherPropValues) 
        {
            var traceFile = GetTraceFile();
            Log(string.Format("begin {0},{1},{2},{3},{4},{5},{6}", traceFile, objType,  objClass,  title,  content,  assignTo,  otherPropValues));
            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener(traceFile));

            var vualtGuid = vault.GetGUID();
            var pcName = LocalConfig.GetComputerFullName();

            var queueName = ".\\private$\\" + vualtGuid + "_" + objType;
            var queueName2 = @"FormatName:Direct=OS:" + pcName + "\\private$\\" + vualtGuid + "_" + objType;
            Trace.TraceInformation("【消息队列】{0}", queueName);
            try
            {
                //CreateNewQueue(queueName);
                var mqs = GetPrivateMqList(pcName);
                Trace.TraceInformation("【私有队列】共有：{0}个" , mqs.Count);
                foreach (string mq in mqs)
                {
                    Trace.TraceInformation("【私有队列】{0}",mq);
                }
                if (!IsExisted(mqs, queueName))
                {
                    Log(string.Format("before {0}",queueName));
                    CreateNewQueue(queueName);
                    Log(string.Format("after {0}", queueName));
                  //  throw new Exception("【消息队列】" + queueName + "不存在");
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("queue operation error {0},{1} ", queueName, ex.Message));
                Trace.TraceError("【消息队列】{0},{1}", queueName, ex.Message);
                throw;
            }

            var msg = new MfTask
            {
                VualtGuid = vualtGuid,
                ObjType = objType,
                ObjClass = objClass,
                Title = title,
                Content = content,
                AssignTo = assignTo
                ,OtherProps = Trans2MfProps(otherPropValues)
                //OtherPropDef = otherPropDef,
                //OtherPropValue = otherPropValue
            };
            try
            {
                SendComplexMsg(queueName2, msg);
            }
            catch (Exception ex)
            {
                Trace.TraceError("【发送消息失败】{0},{1}", queueName, ex.Message);
                throw;
            }

            GC.Collect(0, GCCollectionMode.Forced);
        }

        private void SendComplexMsg(string queueName, MfTask task)
        {
            using (var mq = new MessageQueue(queueName))
            {
                var msg = new Message
                {
                    Label = "[MFiles]" + task.Title,
                    Recoverable = true,
                    Body = task
                };
                mq.Send(msg);
                Trace.TraceInformation("【成功发送消息】{0}，{1}", msg.Body, DateTime.Now);
            }
        }
        private void CreateNewQueue(string name)
        {
            if (MessageQueue.Exists(name))
            {
                Trace.TraceInformation("【消息队列】{0}，已经存在", name);
            }
            else
            {
                Log("11--"+name);
                var mq = MessageQueue.Create(name);
                Log("22");
                mq.Label = "[MFiles]" + name;
                Log("33");
                mq.SetPermissions("Everyone", MessageQueueAccessRights.FullControl);
                Log("44");
                mq.SetPermissions("ANONYMOUS LOGON", MessageQueueAccessRights.FullControl);
                Log("55");
                mq.SetPermissions("ENTERPRISE DOMAIN CONTROLLERS", MessageQueueAccessRights.FullControl);
                Log("66");
                mq.SetPermissions("SELF", MessageQueueAccessRights.FullControl);
                Log("77");
                //mq.Authenticate = true;
                Trace.TraceInformation("【消息队列】{0}，创建成功", name);
            }
        }

        private List<MfProperty> Trans2MfProps(string otherPropValues)
        {
            var res = new List<MfProperty>();
            if (string.IsNullOrEmpty(otherPropValues)) return res;
            var pArr = otherPropValues.Split(new[] { '%' });
            foreach (string a in pArr)
            {
                if(string.IsNullOrEmpty(a)) continue;
                var index = a.IndexOf('_');
                if(index == -1) continue;
                var defStr = a.Substring(0, index);
                int defVal;
                if (int.TryParse(defStr, out defVal))
                {
                    res.Add(new MfProperty
                    {
                        PropDef = defVal,
                        Value = a.Substring(index + 1)
                    });
                }
            }
            return res;
        }

        private string GetTraceFile()
        {
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var date = DateTime.Now.Date.ToString("yy-MM-dd");
            var traceFile = basePath + "\\MsmqComLog" + date + ".txt";
            return traceFile;
        }
        public bool IsExisted(IList<string> items, string name)
        {
            if (items.Any(item => item.ToLower() == name.ToLower()))
            {
                return true;
            }
            return false;
        }
        public IList<string> GetPrivateMqList(string machine)//lmtbert-PC
        {
            return (from mq in MessageQueue.GetPrivateQueuesByMachine(machine)
                    select mq.QueueName).ToList();
        }
    }
}
