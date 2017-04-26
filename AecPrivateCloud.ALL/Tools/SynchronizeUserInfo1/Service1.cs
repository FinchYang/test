using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Configuration;
namespace SynchronizeUserInfo
{
    public partial class SynchronizeUserInfo : ServiceBase
    {
       public SynchronizeUserInfo()
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
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Trace.TraceInformation("开始执行---"+basePath);
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
                 OneSynchronization();
                }
                catch (Exception ex)
                {
                    Trace.TraceInformation("【未知错误】" + ex.Message);
                }
                finally
                {
                    Thread.Sleep(1000*60*60);
                }
            }
        }

        private void OneSynchronization()
        {
            try
            {
                var ServerIp = ConfigurationManager.AppSettings["DbServer"];
                var user = ConfigurationManager.AppSettings["UserName"];
                var pwd = ConfigurationManager.AppSettings["UserPwd"];
                var DataSource = ConfigurationManager.AppSettings["DataSource"];
                var SleepTime = ConfigurationManager.AppSettings["SleepTime"];
                // <add key="notificationdataserver" value="server=WIN-CFRBNKMQS53\AECCLOUD;uid=sa;pwd=cadsimula123A;database=MfNotification" />
                // <add key="DataSource" value="jdbc:oracle:thin:@10.8.143.207:1521:orcl" /> 
                var stringconstring = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.8.143.207) (PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=orcl)));Persist Security Info=True;User Id=NC633GOLD; Password=1";
                var OracleConnectionconn = new OracleConnection(DataSource);//进行连接           
                try
                {
                    OracleConnectionconn.Open();//打开指定的连接   
                    Trace.TraceInformation("open ok");
                    OracleCommand com = OracleConnectionconn.CreateCommand();
                    Trace.TraceInformation("CreateCommand ok");
                    com.CommandText = "Select * From bd_psndoc ";//写好想执行的Sql语句                   
                    OracleDataReader odr = com.ExecuteReader();
                    Trace.TraceInformation("ExecuteReader ok");
                    while (odr.Read())//读取数据，如果返回为false的话，就说明到记录集的尾部了                    
                    {
                        var tmp = string.Empty;
                        for (int i = 0; i < odr.FieldCount; i++)
                        {
                            tmp += odr.GetString(i)+",";
                        }
                        Trace.TraceInformation(tmp);
                    }
                    odr.Close();//关闭reader.这是一定要写的  
                }
                catch (Exception eex)
                {
                 Trace.TraceError("oracle operation:"+eex.Message);             
                }
                finally
                {
                    OracleConnectionconn.Close();//关闭打开的连接              
                } 
                //var sqlc = new SqlConnection(connstr);
                //sqlc.Open(); ;
                //var str = string.Format("select * from tasks where userid = '{0}' and vaultguid = '{1}'", "", "");
                //var sqlcommand = new SqlCommand(str, sqlc);
                //var rds = new SqlDataAdapter(sqlcommand);
                //var dt = new DataTable();
                //rds.Fill(dt);
                //sqlc.Close();

                //foreach (DataRow row in dt.Rows)
                //{
                //  //  var ot = new MfTask();
                //    var serial = 0;
                //    foreach (DataColumn column in dt.Columns)
                //    {
                //        switch (serial)
                //        {
                //            case 0:
                //             //   ot.Name = row[column].ToString();
                //                break;
                //            case 1:
                //             //   ot.Time = row[column].ToString();
                //                break;
                //            case 2:
                //              //  ot.UserId = row[column].ToString();
                //                break;
                //            case 3:
                //                //ot.IsNoticed  = row[column].ToString()==MfTask;
                //                break;
                //            case 4:
                //             //   ot.NotificationType = getntype(row[column]);
                //                break;
                //            case 5:
                //              //  ot.Id = int.Parse(row[column].ToString());
                //                break;
                //            case 6:
                //              //  ot.Type = int.Parse(row[column].ToString());
                //                break;
                //            case 7:
                //            //    ot.Version = int.Parse(row[column].ToString());
                //                break;
                //            case 8:
                //              //  ot.Url = row[column].ToString();
                //                break;
                //        }
                //        //  Writelog(string.Format( "{0}",row[column]));
                //        serial++;
                //    }
                //  //  lt.Add(ot);
                //}
            }
            catch (Exception ex)
            {
                Trace.TraceError("alll error" + ex.Message,ex);
            }
        }
     
        private string GetTraceFile(long i = 0)
        {
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var temp = Path.GetTempPath();
            var date = DateTime.Now.Date.ToString("yyyy-MM-dd");
            var traceFile = temp + "\\SynchronizationLog" + date + ".txt";
            return traceFile;
        }
    }
}
