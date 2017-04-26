using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace AecCloud.MFilesCore
{
    public static class ZipUtils
    {
        /// <summary>
        /// 模板的顶层文件夹
        /// </summary>
        /// <param name="file"></param>
        /// <param name="destPath"></param>
        public static void ExtractTemplate(string file, string destPath)
        {
            using (var zip = new ZipFile(file))
            {
                var dirEntries = zip.Entries;
                var noDir = dirEntries.FirstOrDefault(c => c.FileName.Contains("Index.xml") || c.FileName.Contains("index.xml"));
                if (noDir == null) throw new Exception("未找到Index.XML文件！");
                var prefix = noDir.FileName.Substring(0, noDir.FileName.Length - "index.xml".Length);
                foreach (var d in dirEntries.ToList())
                {
                    if (d.FileName.Length < prefix.Length) continue;
                    var fileName = d.FileName.Substring(prefix.Length);
                    if (String.IsNullOrEmpty(fileName)) continue;
                    d.FileName = fileName;
                    try
                    {
                        d.Extract(destPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                    catch(Exception ex)
                    {
                        throw new Exception(fileName, ex);
                        //errFileList.Add(zipFilename + " # " + fileName);
                    }
                }
            }
        }
        /// <summary>
        /// 模板的顶层文件夹
        /// </summary>
        /// <param name="file"></param>
        /// <param name="destPath"></param>
        public static void ExtractTemplate(Stream file, string destPath)
        {
            using (var zip = ZipFile.Read(file, new ReadOptions{Encoding = new UTF8Encoding()}))
            {
                var dirEntries = zip.Entries;
                var noDir = dirEntries.FirstOrDefault(c => c.FileName.Contains("Index.xml") || c.FileName.Contains("index.xml"));
                var prefix = noDir.FileName.Substring(0, noDir.FileName.Length-"index.xml".Length);
                foreach (var d in dirEntries)
                {
                    var fileName = d.FileName.Substring(prefix.Length);
                    d.FileName = fileName;
                    try
                    {
                        d.Extract(destPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                    catch
                    {
                        //errFileList.Add(zipFilename + " # " + fileName);
                    }
                }
            }
        }
    }
}
