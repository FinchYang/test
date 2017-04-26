using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MSMQ.Core
{
    public class MsmqOps
    {
        public static void SendComplexMsg(string queueName, MfTask task)
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
                //Trace.TraceInformation("【成功发送消息】{0}，{1}", msg.Body, DateTime.Now);
            }
        }
        private void CreateNewQueue(string name)
        {
            if (MessageQueue.Exists(name))
            {
                //Trace.TraceInformation(name + "已经存在");
            }
            else
            {
                var mq = MessageQueue.Create(name);
                mq.Label = name;
                //Trace.TraceInformation(name + "创建成功");
            }
        }

        public static bool IsExisted(string name)
        {
            return MessageQueue.Exists(name);
        }
        public static bool IsExisted(IList<string> items,string name)
        {
            if (items.Any(item => item.ToLower() == name.ToLower()))
            {
                return true;
            }
            return false;
        }

        public static int GetMsgCount(string queueName)
        {
            using (var mq = new MessageQueue(queueName))
            {
                return mq.GetAllMessages().Length;
            }
        }

        //public static IList<string> GetAllMq(string mcName = null)
        //{
        //    return (from mq in MessageQueue.GetPrivateQueuesByMachine("lmtbert-PC")
        //            let count = mq.GetAllMessages().Length
        //            select "消息队列：" + mq.QueueName + "，消息数量：" + count).ToList();
        //}
        public static IList<string> GetPrivateMqList(string machine)//lmtbert-PC
        {
            return (from mq in MessageQueue.GetPrivateQueuesByMachine(machine)
                    select mq.QueueName).ToList();
        }
        public static IList<MfTask> ReceiveMfTaskMsgList(string queueName)
        {
            var res = new List<MfTask>();
            using (var mq = new MessageQueue(queueName))
            {
                var len = mq.GetAllMessages().Length;
                if (len > 0)
                {
                    for (int i = 0; i < len; i++)
                    {
                        var m = mq.Receive(TimeSpan.FromSeconds(5));
                        if (m != null)
                        {
                            m.Formatter = new System.Messaging.XmlMessageFormatter(new Type[] { typeof(MfTask), typeof(MfProperty) });//消息类型转换  
                            var msg = (MfTask)m.Body;
                            res.Add(msg); 
                        }
                    }
                }
            }
            return res;
        }

        public static MfTask ReceiveMfTaskMsg(string queueName)
        {
            using (var mq = new MessageQueue(queueName))
            {
                    var len = mq.GetAllMessages().Length;
                    if (len > 0)
                    {
                        var m = mq.Receive(TimeSpan.FromSeconds(5));
                        if (m != null)
                        {
                            m.Formatter = new System.Messaging.XmlMessageFormatter(new Type[] { typeof(MfTask), typeof(MfProperty)});//消息类型转换  
                            var msg = (MfTask)m.Body;
                            return msg;
                        }
                    }
            }
            return null;
        }
    }
}
