using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace msmqtest
{

    class Program
    {

        static void Main(string[] args)
        {

         //   string database = args[0];



            string path = ".\\private$\\" + "uuuuu";



            if (!MessageQueue.Exists(path))

                MessageQueue.Create(path);



          //  string strsql = "select md5 from video with(nolock) order by id";



            int i = 0;

            //using (SqlDataReader dr = SqlHelper.ExecuteReader(string.Format(System.Configuration.ConfigurationSettings.AppSettings["db"], database), CommandType.Text, strsql))
            //{

                //while (dr.Read())
                //{

                    Console.WriteLine(--i);

                    SendMessage(path, "jadfjlakjdfk", string.Empty);

                //}

                //dr.Close();

                Console.WriteLine("完成");
            Console.ReadKey();
            // }

        }



        private static void SendMessage(string path, string label, object body)
        {

            //new MessageQueue(path).Send(body, label);

            //return;

            MessageQueue mq = new MessageQueue(path);

            System.Messaging.Message msg = new System.Messaging.Message();

            msg.Label = label;

            msg.Body = body;

            msg.Recoverable = true;

            mq.Send(msg);

            msg = null;

            mq.Close();

            mq = null;

        }

    }

    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        try
    //        {
    //            Console.WriteLine("3223");
    //            MessageQueue MQ = new MessageQueue(@".\private$\MsgQueue");
    //            Console.WriteLine("hahah");
    //            var mq = MessageQueue.Create(".\\Public$\\haha111");
    //            Console.WriteLine("ok,{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",mq.AccessMode,mq.Authenticate,mq.BasePriority,mq.Category,
    //                mq.CanRead,mq.CanWrite,mq.FormatName,mq.QueueName,mq.Path,mq.Id);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(ex.Message);
    //        }
    //        Console.WriteLine("any key to return");
    //        Console.ReadKey();
    //    }
    //}
}
