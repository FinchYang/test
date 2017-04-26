using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SimulaDesign.RevitBimInfo
{
    /// <summary>
    /// 显示
    /// </summary>
    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            return true;
        }
    }
    /// <summary>
    /// 窗口信息
    /// </summary>
    public class ButtonInfo
    {
        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// 提示
        /// </summary>
        public string Tip { get; set; }
        /// <summary>
        /// 大图片
        /// </summary>
        public string Large { get; set; }
        /// <summary>
        /// 小图片
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// 文字
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 命令
        /// </summary>
        public string Type { get; set; }
    }

    class RibbonUtils
    {
        /// <summary>
        /// 修改指定名称状态
        /// </summary>
        /// <param name="app"></param>
        /// <param name="tabName"></param>
        /// <param name="panelNames"></param>
        /// <param name="b"></param>
        public static void SetPanels(UIApplication app, string tabName, string[] panelNames, bool b)
        {
            var panels = app.GetRibbonPanels(tabName);
            var ps = panels.Where(c => panelNames.Contains(c.Name)).ToArray();
            foreach (var p in ps)
            {
                p.Enabled = b;
            }
        }
        public static void SetPanels(UIControlledApplication app, string tabName, string[] panelNames, bool b)
        {
            var panels = app.GetRibbonPanels(tabName);
            var ps = panels.Where(c => panelNames.Contains(c.Name)).ToArray();
            foreach (var p in ps)
            {
                p.Enabled = b;
            }
        }
        public static void SetPanels(UIControlledApplication app, string tabName, string panelNames, int[] itemIndices, bool b)
        {
            var panels = app.GetRibbonPanels(tabName);
            var ps = panels.Where(c => panelNames.Equals(c.Name)).ToArray();
            foreach (var p in ps)
            {
                SetItems(p, itemIndices, b);
            }
        }
        public static void SetItems(UIApplication app, string tabName, string panelNames, int[] itemIndices, bool b)
        {
            var panels = app.GetRibbonPanels(tabName);
            var ps = panels.Where(c => panelNames.Equals(c.Name)).ToArray();
            foreach (var p in ps)
            {
                SetItems(p, itemIndices, b);
            }
        }

        /// <summary>
        /// 修改指定位置状态
        /// </summary>
        /// <param name="app"></param>
        /// <param name="tabName"></param>
        /// <param name="indices"></param>
        public static void SetPanels(UIApplication app, string tabName, int[] indices, bool b)
        {
            var panels = app.GetRibbonPanels(tabName);
            for (int i = 0; i < panels.Count; i++)
            {
                int index = Array.IndexOf(indices, i);
                if (index != -1)
                {
                    panels[i].Enabled = b;
                }
            }
        }
        /// <summary>
        /// 修改指定位置状态
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="itemIndices"></param>
        /// <param name="b"></param>
        internal static void SetItems(RibbonPanel panel, int[] itemIndices, bool b)
        {
            var items = panel.GetItems();
            for (int i = 0; i < items.Count; i++)
            {
                if (Array.IndexOf(itemIndices, i) != -1)
                {
                    items[i].Enabled = b;
                }
            }
        }
        /// <summary>
        /// 修改指定名称状态
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="itemNames"></param>
        /// <param name="b"></param>
        internal static void SetItems(RibbonPanel panel, string[] itemNames, bool b)
        {
            var items = panel.GetItems();
            var its = items.Where(c => itemNames.Contains(c.Name)).ToArray();
            foreach (var item in its)
            {
                item.Enabled = b;
            }
        }
        /// <summary>
        /// return the RibbonItem by the input name in a specific panel
        /// </summary>
        /// <param name="panelRibbon">RibbonPanel which contains the RibbonItem </param>
        /// <param name="itemName">name of RibbonItem</param>
        /// <return>RibbonItem whose name is same with input string</return>
        public static RibbonItem GetRibbonItemByName(RibbonPanel panelRibbon, string itemName)
        {
            return panelRibbon.GetItems().FirstOrDefault(c => c.Name == itemName);
        }
        //http://thebuildingcoder.typepad.com/blog/2010/09/ribbon-icon-resolution.html
        public static void CreateRibbonControls(RibbonPanel panel, IEnumerable<ButtonInfo> buttons)
        {
            string assemPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appPath = Path.GetDirectoryName(assemPath);
            string buttonIImageFolder = Path.Combine(appPath, "Images\\Small");
            string buttonILargeFolder = Path.Combine(appPath, "Images\\Large");

            foreach (var info in buttons)
            {
                var pushButtonData = new PushButtonData(info.Tag, info.Text, assemPath, info.Type);
                pushButtonData.AvailabilityClassName = typeof(Availability).FullName;
                pushButtonData.ToolTip = info.Tip;
                if (!string.IsNullOrEmpty(info.Large))
                {
                    pushButtonData.LargeImage =
                        new BitmapImage(new Uri(Path.Combine(buttonILargeFolder, info.Large), UriKind.Absolute));
                }
                if (!string.IsNullOrEmpty(info.Image))
                {
                    pushButtonData.Image =
                        new BitmapImage(new Uri(Path.Combine(buttonIImageFolder, info.Image), UriKind.Absolute));
                }

                panel.AddItem(pushButtonData);
            }

        }
    }
}
