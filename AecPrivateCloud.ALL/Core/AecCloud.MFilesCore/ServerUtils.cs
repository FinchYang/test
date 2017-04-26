using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;

namespace AecCloud.MFilesCore
{
    public class ServerUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="port"></param>
        /// <param name="authType"></param>
        /// <returns>[GUID, NAME]</returns>
        public static Dictionary<string,string> GetVaults(string host, string username, string password, string port = "2266",
            MFAuthType authType = MFAuthType.MFAuthTypeSpecificWindowsUser)
        {
            var serverApp = new MFilesServerApplication();
            var connectRes = serverApp.Connect(authType, username, password, "", "ncacn_ip_tcp", host, port);
            if (connectRes == MFServerConnection.MFServerConnectionAnonymous) return null;
            return serverApp.GetOnlineVaults().Cast<VaultOnServer>().ToDictionary(c=>c.GUID, c=>c.Name);
        }
        public static void ImportContentsByStructure(Vault vault, string contentPath)
        {
            var import = new ImportContentJob
            {
                ActivateAutomaticPermissionsForNewOrChangedDefinitions = true,
                DisableImportedExternalObjectTypeConnections = false,
                DisableImportedExternalUserGroups = false,
                DisableImportedVaultEventHandlers = false,
                Flags = MFImportContentFlag.MFImportContentFlagNone,
                IgnoreAutomaticPermissionsDefinedByObjects = false,
                SourceLocation = contentPath + @"\index"
            };
            vault.ManagementOperations.ImportContent(import);
        }
        public static void ExportContents(Vault vault, string exportPath, ExportStructureItems exportStructureItems=null,
            SearchConditions searchConditions=null)
        {
            var guid = vault.GetGUID();
            var now = DateTime.Now;
            var ecj = new ExportContentJob
            {
                ExportContent = false,
                ExportStructureItems = true,
                IgnoreChangesBefore = new Timestamp(),
                TargetLocation = exportPath,
                UseIgnoreChangesBefore = false,
                UseSearchConditions = false,
                IncludeValueListItemsWithStructure = true,
                Flags = MFExportContentFlag.MFExportContentFlagNone,
                IgnoreVersionsBefore = new Timestamp(),
                TargetFile = Path.Combine(exportPath, string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}",
                    guid, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second), "Index.xml"),
                VaultGUID = guid
            };
            if (exportStructureItems != null && exportStructureItems.Count > 0)
            {
                ecj.StructureItems = exportStructureItems;
            }
            if (searchConditions != null && searchConditions.Count > 0)
            {
                ecj.SearchConditions = searchConditions;
            }

            vault.ManagementOperations.ExportContent(ecj);
        }

        public static void InstallApp(Vault vault, string zipFilepath)
        {
            if (vault == null) throw new ArgumentNullException("vault");
            vault.CustomApplicationManagementOperations.InstallCustomApplication(zipFilepath);
        }

        public static void UninstallAppById(Vault vault, string applicationId)
        {
            if (vault == null) throw new ArgumentNullException("vault");
            vault.CustomApplicationManagementOperations.UninstallCustomApplication(applicationId);
        }

        public static void UninstallAppByName(Vault vault, string applicationName)
        {
            if (vault == null) throw new ArgumentNullException("vault");
            var apps = vault.CustomApplicationManagementOperations.GetCustomApplications();
            foreach (CustomApplication ca in apps)
            {
                if (applicationName != ca.Name) continue;
                vault.CustomApplicationManagementOperations.UninstallCustomApplication(ca.ID);
            }
        }
    }
}
