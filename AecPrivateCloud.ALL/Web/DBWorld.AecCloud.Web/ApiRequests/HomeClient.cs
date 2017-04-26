using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore.Metadata;
using AecCloud.Service.Vaults;
using Newtonsoft.Json;

namespace DBWorld.AecCloud.Web.ApiRequests
{
    public class HomeClient
    {
        private static string GetJson(string structRootPath)
        {
            Impersonator imp = null;
            if (StorageUtility.NeedImpersonation(structRootPath))
            {
                imp = StorageUtility.GetImpersonator();
            }
            try
            {
                var structXml = Path.Combine(structRootPath, @"Metadata\Structure.xml");
                var structure = MetadataStructure.GetFromFile(structXml);
                //var dir = Path.GetDirectoryName(structXml);
                //var jsonFile = Path.Combine(dir, "metadataAlias.json");
                //structure.ToFile(jsonFile, false, true);
                var jsonFile = JsonConvert.SerializeObject(structure.GetAliases(), Formatting.None);
                return jsonFile;
            }
            finally
            {
                if (imp != null) imp.Dispose();
            }
        }

        public static List<VaultTemplate> GetProjectTemplates(IVaultTemplateService vtService)
        {
            var tempList = new List<VaultTemplate>();
            var vts = vtService.GetTemplates();
            foreach (var vt in vts)
            {
                //if (string.IsNullOrEmpty(vt.MetadataJson))
                //{
                //    var path = vt.StructurePath;
                //    var jsonFile = GetJson(path);
                //    vt.MetadataJson = jsonFile;
                //    vtService.UpdateTemplate(vt);
                //}
                tempList.Add(vt);
            }
            return tempList;
        }

    }
    
}