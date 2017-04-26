using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using AecCloud.Core.Domain.Vaults;
using Ionic.Zip;
using AecCloud.Core;

namespace AecCloud.Service.Vaults
{
    public static class VaultAndAppExtensions
    {
        /// <summary>
        /// 必须先设置UNCPath属性
        /// </summary>
        /// <param name="app"></param>
        public static void SetPropertiesFromAppDefFile(this VaultApp app)
        {
            if (app == null) throw new ArgumentNullException("app");
            //if (app.File == null || app.File.Length == 0) throw new ArgumentException("File");
            //var tempPath = Path.GetTempPath();
            var appPath = app.Filepath;//Path.Combine(tempPath, DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip");
            //File.WriteAllBytes(appPath, app.File);
            if (!File.Exists(appPath))
            {
                throw new FileNotFoundException("VaultApp的路径不存在：" + appPath);
            }
            using (var appStream = new MemoryStream())
            {
                //string filePath = null;
                using (var zipFile = new ZipFile(appPath, Encoding.Default))
                {
                    var entry = zipFile.Entries.FirstOrDefault(c => c.FileName.ToUpper().Contains("APPDEF.XML"));
                    if (entry == null) throw new FileNotFoundException("缺少appdef.xml文件");
                    //filePath = Path.Combine(tempPath, entry.FileName.Replace('/', '\\'));

                    entry.Extract(appStream);
                    //entry.Extract(tempPath, ExtractExistingFileAction.OverwriteSilently);
                }
                appStream.Position = 0;
                //var appdefFile = filePath;
                var appElem = XElement.Load(appStream);
                if (appElem.Name.LocalName.ToUpper() != "APPLICATION")
                {
                    throw new Exception("appdef.xml的格式不正确，缺少application节点");
                }
                var guidElem = appElem.Element("guid");
                if (guidElem != null) app.Guid = guidElem.Value;
                var nameElem = appElem.Element("name");
                if (nameElem != null) app.Name = nameElem.Value;
                var descElem = appElem.Element("description");
                if (descElem != null) app.Description = descElem.Value;
                var publisherElem = appElem.Element("publisher");
                if (publisherElem != null) app.Publisher = publisherElem.Value;
                var versionElem = appElem.Element("version");
                if (versionElem != null) app.Version = versionElem.Value;
                else app.Version = "1.1";
            }
        }
    }
}
