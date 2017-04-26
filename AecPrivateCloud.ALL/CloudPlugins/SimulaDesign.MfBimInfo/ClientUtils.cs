using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MFilesAPI;

namespace SimulaDesign.MfBimInfo
{
    /// <summary>
    /// 关于客户端的帮助类
    /// </summary>
    public static class ClientUtils
    {
        private static MFilesClientApplication _app;

        public static MFilesClientApplication GetClientApp()
        {
            return _app ?? (_app = new MFilesClientApplication());
        }

        private static string _versionString;
        /// <summary>
        /// version.Major + "." + version.Minor + "." + version.Build + "." + version.Patch
        /// </summary>
        /// <returns></returns>
        public static string GetVersionString()
        {
            if (_versionString == null)
            {
                var app = GetClientApp();
                var version = app.GetClientVersion();
                _versionString = version.Major + "." + version.Minor + "." + version.Build + "." + version.Patch;
            }
            return _versionString;
        }

        private static string _driveLetter;

        public static string GetDriveLetter()
        {
            if (_driveLetter == null)
            {
                var app = GetClientApp();
                _driveLetter = app.GetDriveLetter();
            }
            return _driveLetter;
        }

        public static bool IsInMf(string path)
        {
            return GetClientApp().IsObjectPathInMFiles(path);
        }

        public static ObjectVersionFile GetObjectFile(string objURL, bool updateFromServer = false)
        {
            try
            {
                var app = GetClientApp();
                return app.FindFile(objURL, updateFromServer);
            }
            catch
            {
                return null;
            }
        }

        public static ObjectVersionAndProperties GetObjectFromURL(string objURL, bool updateFromServer = false)
        {
            try
            {
                var app = GetClientApp();
                return app.FindObjectVersionAndProperties(objURL, updateFromServer);
            }
            catch
            {
                return null;
            }
        }

        public static string GetFilename(string filePath, bool updateFromServer = false)
        {
            var file = GetObjectFile(filePath, updateFromServer);
            if (file == null) return Path.GetFileName(filePath);
            return file.ObjectFile.Title + "." + file.ObjectFile.Extension;
        }

        /// <summary>
        /// 给对象添加文件
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="obj"></param>
        /// <param name="removeFiles">是否删除现有文件</param>
        /// <param name="filePath"></param>
        /// <param name="pvs"></param>
        /// <returns></returns>
        public static ObjectVersion AddFiles(Vault vault, ObjectVersion obj, bool removeFiles, string[] filePath, PropertyValues pvs)
        {
            if (filePath == null || filePath.Length == 0) return null;
            var file0 = filePath[0];
            var title = Path.GetFileNameWithoutExtension(file0);
            var extension = Path.GetExtension(file0).TrimStart(new char[] { '.' });

            var checkedOut = vault.ObjectOperations.IsCheckedOut(obj.ObjVer.ObjID);
            var objVersion = obj;
            if (!checkedOut)
            {
                objVersion = vault.ObjectOperations.CheckOut(obj.ObjVer.ObjID);
            }
            if (objVersion.SingleFile && objVersion.Files.Count > 0)
            {
                var oFileVer = new FileVer { ID = objVersion.Files[1].FileVer.ID, Version = -1 };
                //先设置为多文档
                vault.ObjectOperations.SetSingleFileObject(objVersion.ObjVer, false);
                //删除原文件
                vault.ObjectFileOperations.RemoveFile(objVersion.ObjVer, oFileVer);
                //添加新文件
                vault.ObjectFileOperations.AddFile(objVersion.ObjVer, title, extension, file0);
                for (var i = 1; i < filePath.Length; i++)
                {
                    var title1 = Path.GetFileNameWithoutExtension(filePath[i]);
                    var extension1 = Path.GetExtension(filePath[i]).TrimStart(new char[] { '.' });
                    vault.ObjectFileOperations.AddFile(objVersion.ObjVer, title1, extension1, filePath[i]);
                }
                //还原为单文档
                if (filePath.Length == 1) vault.ObjectOperations.SetSingleFileObject(objVersion.ObjVer, true);
            }
            else
            {
                if (removeFiles)
                {
                    foreach (ObjectFile file in objVersion.Files)
                    {
                        vault.ObjectFileOperations.RemoveFile(objVersion.ObjVer, file.FileVer);
                    }
                }
                //添加新文件
                vault.ObjectFileOperations.AddFile(objVersion.ObjVer, title, extension, file0);
                for (var i = 1; i < filePath.Length; i++)
                {
                    var title1 = Path.GetFileNameWithoutExtension(filePath[i]);
                    var extension1 = Path.GetExtension(filePath[i]).TrimStart(new char[] { '.' });
                    vault.ObjectFileOperations.AddFile(objVersion.ObjVer, title1, extension1, filePath[i]);
                }
                if (removeFiles && filePath.Length == 1)
                {
                    var singleFilePv = new PropertyValue
                    {
                        PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject
                    };
                    singleFilePv.Value.SetValue(MFDataType.MFDatatypeBoolean, true);
                    if (pvs == null)
                    {
                        objVersion = vault.ObjectPropertyOperations.SetProperty(objVersion.ObjVer, singleFilePv).VersionData;
                    }
                    else
                    {
                        pvs.Add(-1, singleFilePv);
                    }
                }
                if (pvs != null)
                {
                    objVersion = vault.ObjectPropertyOperations.SetProperties(objVersion.ObjVer, pvs).VersionData;
                }
            }
            if (!checkedOut)
            {
                obj = vault.ObjectOperations.CheckIn(objVersion.ObjVer);
            }
            return obj;
        }

        //private static string _installDir;

        //public static string GetMFilesInstallDir()
        //{
        //    if (_installDir == null)
        //    {
        //        var versionStr = GetVersionString();
        //        var lm = Registry.LocalMachine;
        //        var mfiles = lm.OpenSubKey(@"SOFTWARE\Motive\M-Files\" + versionStr);
        //        if (mfiles == null)
        //        {
        //            throw new Exception("mfiles注册表项丢失");
        //        }
        //        var dirValue = mfiles.GetValue("InstallDir");
        //        mfiles.Close();
        //        if (dirValue != null)
        //        {
        //            _installDir = dirValue.ToString();
        //        }
        //    }
        //    return _installDir;
        //}


        //public static int GetMajorVersion()
        //{
        //    var app = GetClientApp();
        //    var version = app.GetClientVersion();
        //    return version.Major;
        //}

        
    }
}
