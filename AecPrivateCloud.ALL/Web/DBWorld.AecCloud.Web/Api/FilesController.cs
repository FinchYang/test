using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using AecCloud.BaseCore;
using AecCloud.Core.Domain.Projects;
using AecCloud.MfilesServices;
using AecCloud.Service.Projects;
using AecCloud.Service.Vaults;
using DBWorld.AecCloud.Web.Models;
using DBWorld.AecCloud.Web.Providers;
using log4net;
using Microsoft.Owin.Security;

namespace DBWorld.AecCloud.Web.Api
{
    public class FilesController : ErrorHandlingApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMFilesVaultService _mfvaultService;
        private readonly IMFObjectService _mfobjService;
        private readonly ISharedFileService _fileshareService;

        public FilesController(IMFilesVaultService mfvaultService, IMFObjectService mfobjService, 
            ISharedFileService fileshareService, IAuthenticationManager authManager)
            : base(authManager)
        {
            _mfvaultService = mfvaultService;
            _mfobjService = mfobjService;
            _fileshareService = fileshareService;
        }
        /// <summary>
        /// VaultGuid\ObjType\ObjId\ObjVersion\File
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> Share(FileMFModel model)
        {
            var urlKey = "14795193";//"0303516714795193";
            var vault = await Task.Run(() => _mfvaultService.GetVaultByGuid(model.Guid));
            var version = await Task.Run(() => _mfobjService.GetObjectVersion(vault, model.ObjType, model.ObjId));
            Log.InfoFormat("开始分享文件：GUID:{0}, OBJID: {1}, Version: {2}", model.Guid,
                model.ObjId, version);
            //var serverPath = Global.ServerPath;
            var rootPath = StorageUtility.GetVaultSharedRootPath(); //Path.Combine(serverPath, StorageUtility.VaultSharedPath);
            var guid = model.Guid.TrimStart('{').TrimEnd('}');
            var objUriPart = guid + "\\" + model.ObjType + "\\" + model.ObjId + "\\" + version;
            var objFolderPath = Path.Combine(rootPath, objUriPart);
            StorageUtility.EnsureFolderExists(objFolderPath);
            var files = StorageUtility.GetFiles(objFolderPath);
            SharedFile sharedFile = null;
            string urlPart = null;
            string urlHash = null;
            if (files.Length == 0)
            {
                var file =
                    await Task.Run(() =>
                    {
                        try
                        {
                            return _mfobjService.DownloadFile(vault, model.ObjType, model.ObjId, model.FileId);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("下载MF文件失败：" + ex.Message, ex);
                            throw;
                        }
                    });
                urlPart = objUriPart + "\\" + file.Name + "." + file.Extension;
                var filePath = Path.Combine(rootPath, urlPart);
                StorageUtility.WriteBytes(filePath, file.Content, false);
                //if (!File.Exists(filePath))
                //{
                //    File.WriteAllBytes(filePath, file.Content);
                //}

                
            }
            else
            {
                var fileName = Path.GetFileNameWithoutExtension(files[0]);
                var fileExtension = Path.GetExtension(files[0]).TrimStart('.');
                urlPart = objUriPart + "\\" + fileName + "." + fileExtension;
                urlPart = Utility.ToHexStr(urlPart);
                urlHash = Utility.Hash2HexStr(urlPart);
                sharedFile = _fileshareService.GetByUrlPart(urlPart, urlKey);
            }
            if (sharedFile == null)
            {
                sharedFile = new SharedFile
                {
                    CreatedUtc = DateTime.UtcNow,
                    UrlKey = urlKey,
                    Password = Utility.GenerateRandomDigitCode(10)
                };
                sharedFile.ExpiredTimeUtc = sharedFile.CreatedUtc.AddDays(model.ExpiredDays);
                try
                {
                    sharedFile.UrlPart = Utility.ToHexStr(urlPart); //Utility.Encrypt2Hex(urlPart, sharedFile.UrlKey);
                    sharedFile.UrlHash = Utility.Hash2HexStr(sharedFile.UrlPart);
                }
                catch (Exception ex)
                {
                    Log.Error("加密失败： " + ex.Message, ex);
                    throw;
                }

                _fileshareService.Insert(sharedFile);
            }

            var host = GetHost();
            var linkModel = new FileShareLinkModel
            {
                Url = host + "/Files/" + sharedFile.UrlHash,
                Password = sharedFile.Password
            };

            return Ok(linkModel);
        }
        /// <summary>
        /// 上传BIM预览模型
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        //[AllowAnonymous]
        public async Task<IHttpActionResult> UploadPreviewFiles()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }
            string root = Path.Combine(Global.ServerPath, StorageUtility.BIMPreviewPath);//HttpContext.Current.Server.MapPath("~/App_Data");
            var headers = Request.Headers;
            if (!headers.Contains("guid")) return BadRequest("缺少guid头");
            string guid = headers.GetValues("guid").FirstOrDefault();
            if (guid != null) guid = guid.TrimStart('{').TrimEnd('}');
            if (!headers.Contains("objtype")) return BadRequest("缺少objtype头");
            string objType = headers.GetValues("objtype").FirstOrDefault();
            if (!headers.Contains("objid")) return BadRequest("缺少objid头");
            string objId = headers.GetValues("objid").FirstOrDefault();
            //if (!headers.Contains("objversion")) return BadRequest("缺少objversion头");
            //string objVersion = headers.GetValues("objversion").FirstOrDefault();
            root += "\\" + guid + "\\" + objType + "\\" + objId; // + "\\" + objVersion;
            StorageUtility.EnsureFolderExists(root);
            //if (!Directory.Exists(root)) Directory.CreateDirectory(root);

            var provider = new CustomMultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                
                //// This illustrates how to get the form data.
                //foreach (var key in provider.FormData.AllKeys)
                //{
                //    foreach (var val in provider.FormData.GetValues(key))
                //    {
                //        //sb.Append(string.Format("{0}: {1}\n", key, val));
                //    }
                //}

                // This illustrates how to get the file names.
                foreach (MultipartFileData file in provider.FileData)
                {
                    //Trace.WriteLine(file.Headers.ContentDisposition.FileName);
                    //Trace.WriteLine("Server file path: " + file.LocalFileName);
                }
                return Ok();
            }
            catch (Exception e)
            {
                return CreateErrorResponse("上传文件失败：", HttpStatusCode.InternalServerError, e, Log);
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> CloudApp(CloudAppFileModel model)
        {
            var userId = GetUserId();
            var userName = GetUserName();
            var pwd = DBWorldCache.Get(userId.ToString());
            if (String.IsNullOrEmpty(pwd))
            {
                return new HttpResponseMessage {StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("请从客户端登陆")};
            }

            var cloudUrl = System.Configuration.ConfigurationManager.AppSettings["cloudappweb"];
            using (var client = new HttpClient {BaseAddress = new Uri(cloudUrl)})
            {
                var res = await client.GetStreamAsync("Common/launch.aspx?Domain=" + model.Domain + "&UserName=" + userName +
                                      "&Password=" + pwd + "&ApplicationID=" + model.ApplicationID + "&AppName=" +
                                      model.AppName + "&ProjectName="+model.ProjectName+"&FilePath="+model.FilePath);

                return new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StreamContent(res)};
            }

        }
    }
}
