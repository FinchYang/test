using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Client.Models;
using AecCloud.WebAPI.Models;

namespace AecCloud.Client.Util
{

    internal static class HttpUrlUtilEx
    {
        public static MFilesAPI.Vault GetDiskCloudVault(this UserModel model)
        {
            return GetAppVaults(model, 1, true).FirstOrDefault();
        }

        public static MFilesAPI.Vault GetKnowledgeVault(this UserModel model)
        {
            return GetAppVaults(model, 2, true).FirstOrDefault();
        }

        public static List<MFilesAPI.Vault> GetAppVaults(this UserModel model, long appId, bool login)
        {
            var app = model.UserApp.Apps.FirstOrDefault(c => c.App.Id == appId);
            var guids = app.Projects.Select(c => c.Vault.Guid);
            return guids.Select(c => MfilesClientCore.MFilesVault.GetVault(model.UserWeb, c, login)).Where(c => c != null).ToList();
        }
    }
}
