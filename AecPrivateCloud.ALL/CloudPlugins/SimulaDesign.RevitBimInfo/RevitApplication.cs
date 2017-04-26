using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Events;

namespace SimulaDesign.RevitBimInfo
{
    public class RevitApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            CreateRibbonPanel(application, PanelName);

            //application.ControlledApplication.DocumentOpening += ControlledApplication_DocumentOpening;
            //application.ControlledApplication.DocumentClosing += ControlledApplication_DocumentClosing;
            //application.ControlledApplication.DocumentClosed += ControlledApplication_DocumentClosed;

            return Result.Succeeded;
        }

        //private class ClosingDoc
        //{
        //    public string Path { get; set; }

        //    public int DocId { get; set; }

        //    public System.Windows.Forms.DialogResult Res { get; set; }
        //}

        //private ClosingDoc _doc;

        //public static bool NeedCheck = true;

        //private void ControlledApplication_DocumentClosing(object sender, DocumentClosingEventArgs e)
        //{
        //    if (!NeedCheck) return;
        //    var inMF = ClientUtils.IsInMF(_doc.Path);
        //    if (inMF)
        //    {
        //        _doc = new ClosingDoc { Path = e.Document.PathName, DocId = e.DocumentId };
        //        var dr = System.Windows.Forms.MessageBox.Show("该文件位于DBWorld系统内，是否签入保存所做的修改？", "DBWorld提示",
        //            System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);
        //        _doc.Res = dr;
        //    }
        //    else
        //    {
        //        _doc = null;
        //    }

        //}

        //private void ControlledApplication_DocumentClosed(object sender, DocumentClosedEventArgs e)
        //{
        //    if (!NeedCheck) return;
        //    if (_doc == null || _doc.DocId != e.DocumentId) return;


        //    var mfObj = MfUtils.GetObjectFromURL(_doc.Path);
        //    if (mfObj.ObjectVersion.VersionData.ObjectCheckedOut)
        //    {

        //        if (_doc.Res == System.Windows.Forms.DialogResult.Yes)
        //        {
        //            var vault = mfObj.ObjectVersion.Vault;
        //            vault.ObjectOperations.CheckIn(mfObj.ObjectVersion.ObjVer);
        //        }
        //    }
        //}

        //private void ControlledApplication_DocumentOpening(object sender, DocumentOpeningEventArgs e)
        //{
        //    if (!NeedCheck) return;
        //    //if (!e.Cancellable) return;
        //    var inMF = MfUtils.IsInMF(e.PathName);
        //    if (!inMF) return;

        //    var mfObj = MfUtils.GetObjectFromURL(e.PathName);
        //    if (!mfObj.ObjectVersion.VersionData.ObjectCheckedOut)
        //    {
        //        var dr = System.Windows.Forms.MessageBox.Show("该文件位于DBWorld系统内，是否签出以编辑文件？", "DBWorld提示",
        //            System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);
        //        if (dr == System.Windows.Forms.DialogResult.Yes)
        //        {
        //            var vault = mfObj.ObjectVersion.Vault;
        //            vault.ObjectOperations.CheckOut(mfObj.ObjectVersion.ObjVer.ObjID);
        //        }

        //    }
        //}

        public static readonly string TabName = "DBWorldBIM信息";

        public static readonly string PanelName = "DBWorldBIM";

        /// <summary>
        /// 添加Ribbon菜单
        /// </summary>
        /// <param name="application"></param>
        /// <param name="panelName"></param>
        private static void CreateRibbonPanel(UIControlledApplication application, string panelName)
        {
            //添加选项卡
            application.CreateRibbonTab(TabName);
            //添加面板.
            var panel = application.CreateRibbonPanel(TabName, panelName);

            var bom = new ButtonInfo
            {
                Tag = "显示信息",
                Tip = "显示模型相关的数据",
                Large = "信息.ico",
                Image = "信息.ico",
                Text = "显示信息",
                Type = typeof(ShowInfoCommand).FullName
            };

            var bomMF = new ButtonInfo
            {
                Tag = "发布模型",
                Tip = "读取模型相关的数据，并发布到BIM云",
                Large = "模型.ico",
                Image = "模型.ico",
                Text = "发布模型",
                Type = typeof(ExportInfo2MfCommand).FullName
            };

            //添加按钮
            RibbonUtils.CreateRibbonControls(panel, new[] { bom, bomMF });

        }
    }
}
