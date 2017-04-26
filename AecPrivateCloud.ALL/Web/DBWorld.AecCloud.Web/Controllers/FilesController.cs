using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AecCloud.BaseCore;
using AecCloud.Service.Projects;
using DBWorld.AecCloud.Web.Models;

namespace DBWorld.AecCloud.Web.Controllers
{
    public class FilesController : Controller
    {
        private readonly ISharedFileService _shareService;

        public FilesController(ISharedFileService shareService)
        {
            _shareService = shareService;
        }
        // GET: Files
        public ActionResult Index(string id)
        {
            var fileModel = new FileGetModel {Item = id};
            return View(fileModel);
        }

        [HttpPost]
        public ActionResult Download(FileGetModel model)
        {
            var sharedFile = _shareService.GetByUrlHash(model.Item, model.Key);
            if (sharedFile == null) return RedirectToAction("Index", "Files", new { id = model.Item });
            var urlPart = Utility.FromHexStr(sharedFile.UrlPart);//Utility.DecryptFromHex(model.Item, sharedFile.UrlKey);
            urlPart = urlPart.Replace(@"\", "/");
            //var filePath = GetHost() + "/" + StorageUtility.VaultSharedPath + "/" + urlPart;
            //return Redirect(filePath);
            var sharedRootPath = StorageUtility.GetVaultSharedRootPath();
            var filePath = Path.Combine(sharedRootPath, urlPart);
            //var filePath = Path.Combine(Server.MapPath("~"), StorageUtility.VaultSharedPath + "\\" + urlPart);
            var downloadName = Path.GetFileName(filePath);
            if (!StorageUtility.NeedImpersonation(filePath))
            {
                return File(filePath, "application/octet-stream", downloadName);
            }
            using (StorageUtility.GetImpersonator())
            {
                var bytes = System.IO.File.ReadAllBytes(filePath);
                var fcr = new FileContentResult(bytes, "application/octet-stream") {FileDownloadName = downloadName};
                return fcr;
                //return File(filePath, "application/octet-stream", downloadName);
            }
        }
    }
}