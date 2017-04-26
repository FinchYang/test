/* ***************************
 * 所有命令放到此文件内
 * ************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using SimulaDesign.BimInfo;
using SimulaDesign.MfBimInfo;
using Element = Autodesk.Revit.DB.Element;

namespace SimulaDesign.RevitBimInfo
{
    public abstract class AvailableCommand : IExternalCommand
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            return true;
        }

        internal static TaskDialogResult ShowMessage(string message, TaskDialogIcon icon = TaskDialogIcon.TaskDialogIconNone, TaskDialogCommonButtons btn = TaskDialogCommonButtons.Ok)
        {
            var d = new TaskDialog("提示") { MainContent = message, MainIcon = icon, CommonButtons = btn };
            return d.Show();
        }

        public abstract Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements);
    }

    [Transaction(TransactionMode.Manual)]
    //[Regeneration(RegenerationOption.Manual)]
    //[Journaling(JournalingMode.NoCommandData)]
    public class ExportInfo2MfCommand : AvailableCommand
    {
        private Func<object, string> ToJson = o => JsonConvert.SerializeObject(o, Formatting.None);

        private Func<string, MfModelDicts> ToDict = s => JsonConvert.DeserializeObject<MfModelDicts>(s);

        private Task<string> UploadXbim(MfModel model, string ifcFilepath)
        {
            var url = model.GetXbimUploadUrl();
            if (String.IsNullOrEmpty(url))
            {
                return Task.Factory.StartNew(() => "没有找到上传的网站地址！");
            }
            return ExportUtility.UploadXbim(url, ifcFilepath);
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;

            var sw = new Stopwatch();
            sw.Start();
            var revit = new RevitModel(doc);
            var model = revit.GetData();
            var modelErr = model.GetErr();
            if (!String.IsNullOrEmpty(modelErr))
            {
                ShowMessage(modelErr);
                return Result.Failed;
            }
            var folder = Path.GetTempPath();
            MfModel.SetModelId(model);
            File.WriteAllText(Path.Combine(folder, model.Name+".json"), JsonConvert.SerializeObject(model, Formatting.None));
            //var jCount = Utility.GetBytes(json).LongLength;
            //var zCount = Utility.Zip(json).LongLength;
            //ShowMessage(jCount + " vs "+zCount + ": " + (jCount > zCount), TaskDialogIcon.TaskDialogIconWarning);
            var mfModel = MfModel.GetClientModel(model);
            if (mfModel.IsPublished())
            {
                var res = ShowMessage("此模型已发布,是否重新发布?", btn:TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
                if (res != TaskDialogResult.Yes)
                {
                    return Result.Cancelled;
                }
            }
            string err = String.Empty;
            var log = MfProjectModel.GetTrace<ExportUtility>();
            try
            {
                if (model.IsProject)
                {
                    var ifcPath = Path.Combine(folder, model.Name + ".ifc");
                    ExportUtility.ExportIFC(doc, ifcPath, true);
                    log.TraceInformation("添加IFC模型...");
                    mfModel.AddIfc(ifcPath);
                    log.TraceInformation("开始上传预览模型...");
                    var task = UploadXbim(mfModel, ifcPath);
                    log.TraceInformation("开始转换模型...");
                    err = mfModel.ServerRun(ToJson, ToDict, mfModel.GetModelUrl());
                    string uploadRes = String.Empty;
                    if (task.Exception != null)
                    {
                        uploadRes = task.Exception.InnerException.Message;
                    }
                    else
                    {
                        uploadRes = task.Result;
                    }
                    var res = uploadRes;

                    log.TraceInformation(String.Format("上传IFC文件({0})为Web模型：{1}", ifcPath, res));
                    ShowMessage("发布预览模型: " + res);
                }
                else
                {
                    mfModel.ToMf();
                }
            }
            finally
            {
                log.Close();
            }
            
            
            //mfModel.ToMf();
            sw.Stop();
            var msg = "运行时间：" + sw.ElapsedMilliseconds/1000 + "s";
            if (!String.IsNullOrEmpty(err))
            {
                msg += "\r\nError: " + err;
            }
            ShowMessage(msg);
            return Result.Succeeded;
        }

        
    }

    [Transaction(TransactionMode.Manual)]
    //[Regeneration(RegenerationOption.Manual)]
    //[Journaling(JournalingMode.NoCommandData)]
    public class ShowInfoCommand : AvailableCommand
    {
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;
            Element selElem;
#if R2014
            var elems = uiDoc.Selection.Elements;
            if (elems.Size != 1)
#else
            var elemIds = uiDoc.Selection.GetElementIds();
            if (elemIds.Count != 1)
#endif
            {
                ShowMessage("请选择一个构件！");
                return Result.Failed;
            }
            #if R2014
             selElem = elems.OfType<Element>().First();
#else
            selElem = doc.GetElement(elemIds.First());
#endif

            var guid = selElem.UniqueId;

            var mfModel = MfModel.GetModel(doc.PathName);

            try
            {
                var url = mfModel.GetPart(guid);
                if (url == null)
                {
                    ShowMessage("系统中未能找到相应构件：" + guid, TaskDialogIcon.TaskDialogIconWarning);
                    return Result.Failed;
                }
                Process.Start(url);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, TaskDialogIcon.TaskDialogIconWarning);
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    //[Regeneration(RegenerationOption.Manual)]
    //[Journaling(JournalingMode.NoCommandData)]
    public class ExportInfoCommand : AvailableCommand
    {
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;

            var revit = new RevitModel(doc);
            var model = revit.GetData();
            var folder = Path.GetTempPath();
            var filePath = Path.Combine(folder, model.Name + ".json");
            var str = JsonConvert.SerializeObject(model,
                new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText(filePath, str);

            var ifcPath = Path.Combine(folder, model.Name + ".ifc");
            ExportUtility.ExportIFC(doc, ifcPath, true);

            return Result.Succeeded;
        }
    }
}
