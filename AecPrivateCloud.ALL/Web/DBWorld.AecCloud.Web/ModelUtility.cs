using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using DBWorld.AecCloud.Web.Models;
using log4net;
using Xbim.COBieLite;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.XbimExtensions;
using XbimGeometry.Interfaces;

namespace DBWorld.AecCloud.Web
{
    public class ModelUtility
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static string _rootFolder;
        private static string _hostFolder;
        public static void SetRootFolder(string hostFolder, string folderName)
        {
            _hostFolder = hostFolder;
            if (!_hostFolder.EndsWith("\\"))
            {
                _hostFolder = _hostFolder.TrimEnd('\\');
            }
            _rootFolder = Path.Combine(hostFolder, folderName);
            if (!Directory.Exists(_rootFolder))
            {
                Directory.CreateDirectory(_rootFolder);
            }
        }

        public static List<ModelFile> GetAllModels()
        {
            var files = Directory.GetFiles(_rootFolder).Select(Path.GetFileNameWithoutExtension).Distinct().Select(ModelFile.GetFromName).ToList();
            return files;
        }

        public static string GetRelativePath(string modelPath)
        {
            return modelPath.Substring(_hostFolder.Length);
        }

        public static string GetModel(ModelFile model, string extWithoutDot)
        {
            var path = model.Guid + "-" + model.TypeId + "-" + model.ObjId + "." + extWithoutDot;
            var modelPath = Path.Combine(_rootFolder, path);
            return modelPath;
        }

        public static string GetModelPath(ModelFile model)
        {
            var path = model.Guid + "-" + model.TypeId + "-" + model.ObjId;
            var absPath = Path.Combine(_rootFolder, path);
            if (File.Exists(absPath + ".wexbim"))
            {
                return GetRelativePath(absPath);
            }
            return String.Empty;
        }

        public static string ConvertModel(string filePath)
        {
            try
            {
                ModelConvert.Convert(filePath, true);
            }
            catch(Exception ex)
            {
                Log.Error("转换模型失败：" + ex.Message, ex);
                return ex.Message;
            }
            return String.Empty;
        }
    }

    public class ModelConvert
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static void Convert(string fileName, bool deleteTempFile)
        {
            using (var model = GetModel(fileName))
            {
                CreateWexBIM(model, fileName);
                CreateJsonBIM(model, fileName);
                var xbimFile = Path.ChangeExtension(fileName, "xbim");
                if (deleteTempFile && File.Exists(xbimFile))
                {
                    try
                    {
                        File.Delete(xbimFile);
                    }
                    catch
                    {
                    }

                }
            }
        }

        private static string CreateJsonBIM(XbimModel model, string fileName)
        {
            try
            {
                var cobieFileName = Path.ChangeExtension(fileName, "json");//"semantics.json";
                using (var cobieFile = new FileStream(cobieFileName, FileMode.Create))
                {
                    var helper = new CoBieLiteHelper(model, "UniClass");
                    var facility = helper.GetFacilities().FirstOrDefault();
                    if (facility != null)
                    {
                        using (var writer = new StreamWriter(cobieFile))
                        {
                            CoBieLiteHelper.WriteJson(writer, facility);
                            writer.Close();
                        }
                    }
                }
                return cobieFileName;
            }
            catch (Exception ex)
            {
                Log.Error("转换Cobie文件失败：" + ex.Message, ex);
                throw;
            }
        }

        private static string CreateWexBIM(XbimModel model, string fileName)
        {
            try
            {
                var context = new Xbim3DModelContext(model);
                context.CreateContext(geomStorageType: XbimGeometryType.PolyhedronBinary);
                var wexBimFilename = Path.ChangeExtension(fileName, "wexBIM");
                using (var wexBiMfile = new FileStream(wexBimFilename, FileMode.Create, FileAccess.Write))
                {
                    using (var wexBimBinaryWriter = new BinaryWriter(wexBiMfile))
                    {
                        //Console.WriteLine("Creating " + wexBimFilename);
                        context.Write(wexBimBinaryWriter);
                        wexBimBinaryWriter.Close();
                    }
                    wexBiMfile.Close();
                }

                return wexBimFilename;
            }
            catch (Exception ex)
            {
                Log.Error("转换xBIM文件失败：" + ex.Message, ex);
                throw;
            }
        }
        private static XbimModel GetModel(string fileName)
        {
            XbimModel openModel = null;
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                if (File.Exists(Path.ChangeExtension(fileName, "xbim"))) //use xBIM if exists
                    fileName = Path.ChangeExtension(fileName, "xbim");
                else if (File.Exists(Path.ChangeExtension(fileName, "ifc"))) //use ifc if exists
                    fileName = Path.ChangeExtension(fileName, "ifc");
                else if (File.Exists(Path.ChangeExtension(fileName, "ifczip"))) //use ifczip if exists
                    fileName = Path.ChangeExtension(fileName, "ifczip");
                else if (File.Exists(Path.ChangeExtension(fileName, "ifcxml"))) //use ifcxml if exists
                    fileName = Path.ChangeExtension(fileName, "ifcxml");
            }

            if (File.Exists(fileName))
            {
                extension = Path.GetExtension(fileName).TrimStart('.');
                if (String.Compare(extension, "xbim", StringComparison.OrdinalIgnoreCase) == 0) //just open xbim
                {

                    try
                    {
                        var model = new XbimModel();
                        Log.Info("Opening " + fileName);
                        model.Open(fileName, XbimDBAccess.ReadWrite);
                        //delete any geometry
                        openModel = model;
                    }
                    catch (Exception e)
                    {
                        Log.Error("无法打开文件：" + fileName, e);
                    }

                }
                else //we need to create the xBIM file
                {
                    var model = new XbimModel();
                    try
                    {
                        Log.Info("Creating " + Path.ChangeExtension(fileName, ".xBIM"));
                        model.CreateFrom(fileName, null, null, true);
                        openModel = model;
                    }
                    catch (Exception e)
                    {
                        Log.Error("无法打开文件：" + fileName, e);
                    }

                }
            }
            return openModel;
        }
    }
}