using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using log4net;

namespace DBWorld.AecCloud.Web.Controllers
{
    public class AECCloudClientController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 下载客户端
        /// </summary>
        /// <returns></returns>
        public FilePathResult DownloadClient()
        {
            string type = Request.QueryString["type"];
            string[] fileNames = null;
            var filenameFile = Server.MapPath("/Installer/fileName.txt"); //保存下载文件文件名的文本文件
            if (System.IO.File.Exists(filenameFile))
            {
                fileNames = System.IO.File.ReadAllLines(filenameFile);
            }
            else
            {
                var error = string.Format("file {0} doesn't exist!", filenameFile);
                Log.Info(error);
                throw new Exception(error);
            }
            string fileName = "";
            switch (type)
            {
                case "x64":
                    fileName = fileNames[0]; //filenameFile中第一行文本为64位安装包的文件名称(带扩展名)
                    break;
                case "x86":
                    fileName = fileNames[1]; //filenameFile中第二行文本为32位安装包的文件名称(带扩展名)
                    break;
            }
            var path = Server.MapPath("~/Installer/" + fileName); //真实下载文件所在的路径
            var name = Path.GetFileName(path);
            return File(path, "application/octet-stream", Url.Encode(name));
        }
    }
}