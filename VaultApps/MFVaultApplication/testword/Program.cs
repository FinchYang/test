using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using MFilesAPI;
using Newtonsoft.Json;

namespace testword
{
    public class ReportPrint
    {
        public ObjVer ObjVer;
        public FileVer FileVer;
    }
    public class ReportPrintData
    {
        public int objid = -1;
        public int objtype = -1;
        public int objversion = -1;
        public int fileid = -1;
        public int fileversion = -1;
    }
    class Program
    {
         static void Writelog(string text)
        {
            Console.WriteLine(text);
        }

        static void offp()
        {
            var pid = 1;
            var rid = 1;
            var sdate = "2016/01/01";
            var edate = "2016/08/08";
                var input="{\"principal\":"+pid+",\"receiver\":"+rid+",\"startdate\":\""+sdate+"\",\"enddate\":\""+edate+"\"}";
            var app = new MFilesClientApplication();
          var vault=  app.BindToVault("8211", IntPtr.Zero, true, true);
    var FilterReceiver = vault.ExtensionMethodOperations.ExecuteVaultExtensionMethod("GetSecureNotice", input);
    Console.WriteLine(FilterReceiver);
        //  var filedata = eval('(' + FilterReceiver + ')'); 
    var filedata = JsonConvert.DeserializeObject<ReportPrintData>(FilterReceiver);
           // alert(filedata.objver.id);
          	// var ObjID  = MFiles.CreateInstance("ObjID");
            //   ObjID.SetIDs (0,filedata);
            var objver = new ObjVer();
            objver.SetIDs(filedata.objtype,filedata.objid,filedata.objversion);
            var filever = new FileVer();
            filever.ID = filedata.fileid;
            filever.Version = filedata.fileversion;
            vault.ObjectFileOperations.OpenFileInDefaultApplication(IntPtr.Zero, objver, filever,
                MFFileOpenMethod.MFFileOpenMethodOpen);
        }

        static void doctest()
        {
            var templatefile = "d:\\report1";

            var app = new Microsoft.Office.Interop.Word.Application();
            var unknow = Type.Missing;
            var msocoding = MsoEncoding.msoEncodingSimplifiedChineseGB18030;


            var doc = app.Documents.Open(templatefile,
                                         ref unknow, false, ref unknow, ref unknow, ref unknow,
                //        ref unknow, ref unknow, ref unknow, ref unknow, msocoding,
                 ref unknow, ref unknow, ref unknow, ref unknow, ref unknow,
                                         ref unknow, ref unknow, ref unknow, ref unknow, ref unknow);
            var num = 0;
            object count;
            //doc.Tables[1].Range.InsertBefore("??before table ??");
            //doc.Tables[1].Range.InsertAfter("??after table ??");
            //doc.Content.InsertBefore("??before table ??");
            //doc.Content.InsertAfter("??after table ??");
            Writelog("111 table num=" + doc.Tables.Count.ToString());
            doc.Tables[1].Range.Copy();
            Writelog("222+table num=" + doc.Tables.Count.ToString());

            var page = 0;
            var tableindex = 0;
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    page++;
                    tableindex++;
                    doc.Tables[tableindex].Cell(4, 2).Range.Text = "project name";
                    doc.Tables[tableindex].Cell(4, 4).Range.Text = DateTime.Now.ToShortDateString();
                    doc.Tables[tableindex].Cell(4, 6).Range.Text = page.ToString();
                    doc.Tables[tableindex].Cell(0, 1).Range.Text = "整改期限：" + "2002/02/02";
                    doc.Tables[tableindex].Cell(0, 2).Range.Text = "接收人：" + "ni";
                    doc.Tables[tableindex].Cell(0, 3).Range.Text = "检查负责人：" + "wo";
                    doc.Tables[tableindex].Cell(0, 4).Range.Text = "复查人：" + "ta";

                    var serial = 1;
                    for (int j = 0; j < 3; j++)
                    {
                        var rowindex = 6 + serial;
                        doc.Tables[tableindex].Cell(rowindex, 1).Range.Text = serial.ToString();
                        doc.Tables[tableindex].Cell(rowindex, 2).Range.Text = "problem";
                        doc.Tables[tableindex].Cell(rowindex, 3).Range.Text = "measure";
                        doc.Tables[tableindex].Cell(rowindex, 4).Range.Text = "man";
                        doc.Tables[tableindex].Cell(rowindex, 5).Range.Text = "time";
                        serial++;
                        if (serial > 11)
                        {
                            break;
                            serial = 1;
                        }
                    }

                    doc.Content.InsertParagraphAfter();
                    doc.Content.InsertAfter(" ");

                    count = 21;
                    object WdLine = Microsoft.Office.Interop.Word.WdUnits.wdLine;//换一行;
                    var movedown = app.Selection.MoveDown(ref WdLine, ref count);//移动焦点
                    Writelog(string.Format(" before paste table num=[{0}],count=[{1}],down==[{2}]", doc.Tables.Count.ToString(), count, movedown));

                    app.Selection.Paste();

                    // doc.Tables[tableindex].Range.Paste();
                    Writelog("after paste+table num=" + doc.Tables.Count.ToString());
                    //   doc.Save();
                }
            }
            catch (Exception ex)
            {
                Writelog(ex.Message);
            }
            Writelog("11=table num=" + doc.Tables.Count.ToString());
            doc.SaveAs("rr1r.docx");
            //  Writelog("22=table num=" + doc.Tables.Count.ToString());
            doc.Close();
            app.Quit();
            //  Writelog("33=" + num);
            Console.WriteLine("any key");
            Console.ReadKey();
        }
        static void Main(string[] args)
        {
           offp();
           Console.WriteLine("any key");
           Console.ReadKey();
        }

    }
}
