using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore;
using Ionic.Zip;
using log4net;
using MFilesAPI;

namespace AecCloud.MfilesServices
{
    public class MFVaultService : IMFVaultService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool HasVault(MFilesVault vault)
        {
            var app = MFServerUtility.ConnectToMfApp(vault);

            return HasVault(app, vault);
        }

        internal static bool HasVault(MFilesServerApplication app, MFilesVault vault)
        {
            foreach (VaultOnServer vos in app.GetVaults())
            {
                if (vos.GUID == vault.Guid || vos.Name == vault.Name) return true;
            }
            return false;
        }
        

        public void CreateDefaultVault(MFilesVault vault)
        {
            if (HasVault(vault))
            {
                throw new Exception(string.Format("与{0}同名的vault已存在或guid={1}重复！", vault.Name, vault.Guid));
            }
            var app = MFServerUtility.ConnectToMfApp(vault);
            var vp = new VaultProperties
            {
                DisplayName = vault.Name,
                ExtendedMetadataDrivenPermissions = true,
                FileDataStorageType = MFFileDataStorage.MFFileDataStorageDefault,
                MainDataFolder = Path.Combine(vault.ServerPath, vault.Name),
                VaultGUID = Guid.NewGuid().ToString()
            };

            var version = app.GetServerVersion().Major;
            vp.FullTextSearchLanguage = version < 11 ? "other" : "chs";

            var guid = app.VaultManagementOperations.CreateNewVault(vp);
            vault.Guid = guid;
        }
        internal static void CreateForAllBackup(MFilesServerApplication app, MFilesVault vault, string templateRootPath,
         string impersonationUserName, string impersonationPassword, MFSqlDatabase sqlDb,
         string vaultIndexRootPath = null)
        {
            var vp = new VaultProperties
            {
                DisplayName = vault.Name,
                ExtendedMetadataDrivenPermissions = true,
                FileDataStorageType = MFFileDataStorage.MFFileDataStorageDisk,
                MainDataFolder = Path.Combine(@"C:\Program Files\M-Files\Server Vaults", vault.Name),
                VaultGUID = Guid.NewGuid().ToString()
            };
            if (vp.SeparateLocationForFileData == null)
            {
                vp.SeparateLocationForFileData = new AdditionalFolders();
            }

            var af = new AdditionalFolder
            {
                Folder = Path.Combine(vault.ServerPath, vault.Name),

            };
            if (!String.IsNullOrEmpty(impersonationUserName) && !String.IsNullOrEmpty(impersonationPassword))
            {
                af.Impersonation = new Impersonation
                {
                    Account = impersonationUserName,
                    Password = impersonationPassword,
                    ImpersonationType = MFImpersonationType.MFImpersonationTypeSpecificAccount
                };
            }
            vp.SeparateLocationForFileData.Add(-1, af);
            var version = app.GetServerVersion().Major;

            if (sqlDb != null)
            {
                var admin = new Impersonation
                {
                    Account = sqlDb.AdminUserName,
                    Password = sqlDb.AdminPassword,
                    ImpersonationType = sqlDb.SqlserverUser
                        ? MFImpersonationType.MFImpersonationTypeExtAccount
                        : MFImpersonationType.MFImpersonationTypeSpecificAccount
                };
                var mfsqldb = new SQLDatabase
                {
                    Server = sqlDb.Server,
                    Name = sqlDb.Catelog,
                    Engine = MFDBEngine.MFDBEngineMSSQLServer
                };
                mfsqldb.AdminUser = admin; //2015
                vp.SQLDatabase = mfsqldb;
            }

            vp.FullTextSearchLanguage = version < 11 ? "other" : "chs";

            
                var rj = new RestoreJob { BackupFileFull = templateRootPath, VaultProperties = vp, OverwriteExistingFiles = true };
                app.VaultManagementOperations.RestoreVault(rj);
                vault.Guid = vp.VaultGUID;
                //return app.LogInToVault(vp.VaultGUID);
           
        }
        internal static Vault Create(MFilesServerApplication app, MFilesVault vault, string templateRootPath,
            string impersonationUserName, string impersonationPassword, MFSqlDatabase sqlDb,
            string vaultIndexRootPath = null, bool fullBackupOrStructure = false)
        {
            var vp = new VaultProperties
            {
                DisplayName = vault.Name,
                ExtendedMetadataDrivenPermissions = true,
                FileDataStorageType = MFFileDataStorage.MFFileDataStorageDisk,
                MainDataFolder = Path.Combine(@"C:\Program Files\M-Files\Server Vaults", vault.Name),
                VaultGUID = Guid.NewGuid().ToString()
            };
            if (vp.SeparateLocationForFileData == null)
            {
                vp.SeparateLocationForFileData = new AdditionalFolders();
            }

            var af = new AdditionalFolder
            {
                Folder = Path.Combine(vault.ServerPath, vault.Name),

            };
            if (!String.IsNullOrEmpty(impersonationUserName) && !String.IsNullOrEmpty(impersonationPassword))
            {
                af.Impersonation = new Impersonation
                {
                    Account = impersonationUserName,
                    Password = impersonationPassword,
                    ImpersonationType = MFImpersonationType.MFImpersonationTypeSpecificAccount
                };
            }
            vp.SeparateLocationForFileData.Add(-1, af);
            var version = app.GetServerVersion().Major;

            if (sqlDb != null)
            {
                var admin = new Impersonation
                {
                    Account = sqlDb.AdminUserName,
                    Password = sqlDb.AdminPassword,
                    ImpersonationType = sqlDb.SqlserverUser
                        ? MFImpersonationType.MFImpersonationTypeExtAccount
                        : MFImpersonationType.MFImpersonationTypeSpecificAccount
                };
                var mfsqldb = new SQLDatabase
                {
                    Server = sqlDb.Server,
                    Name = sqlDb.Catelog,
                    Engine = MFDBEngine.MFDBEngineMSSQLServer
                };
                mfsqldb.AdminUser = admin; //2015
                vp.SQLDatabase = mfsqldb;
            }

            vp.FullTextSearchLanguage = version < 11 ? "other" : "chs";

            if (fullBackupOrStructure)
            {
                var rj = new RestoreJob { BackupFileFull = templateRootPath, VaultProperties = vp, OverwriteExistingFiles = true };
                app.VaultManagementOperations.RestoreVault(rj);
                vault.Guid = vp.VaultGUID;
                return null;
            }

            var guid = app.VaultManagementOperations.CreateNewVault(vp);
            vault.Guid = guid;
            var import = new ImportContentJob
            {
                ActivateAutomaticPermissionsForNewOrChangedDefinitions = true,
                DisableImportedExternalObjectTypeConnections = true,
                DisableImportedExternalUserGroups = true,
                DisableImportedVaultEventHandlers = false,
                Flags = MFImportContentFlag.MFImportContentFlagNone,
                IgnoreAutomaticPermissionsDefinedByObjects = false,
                SourceLocation = Path.Combine(templateRootPath, "Index")
            };
            if (!String.IsNullOrEmpty(impersonationUserName) && !String.IsNullOrEmpty(impersonationPassword))
            {
                import.Impersonation = new Impersonation
                {
                    Account = impersonationUserName,
                    Password = impersonationPassword,
                    ImpersonationType = MFImpersonationType.MFImpersonationTypeSpecificAccount
                };
            }
            var newvault = app.LogInToVault(guid);
            //todo,会有数据超限的问题, 2015貌似没有问题，但更新模板数据时可能会有这个问题
            try
            {
                newvault.ManagementOperations.ImportContent(import);
            }
            catch (Exception ex)
            {
                Log.Error("导入模版元数据出错：" + ex.Message, ex);
            }
            return newvault;
        }

        public void Create(MFilesVault vault, string templateRootPath, 
            string impersonationUserName, string impersonationPassword, MFSqlDatabase sqlDb = null, string vaultIndexRootPath=null, bool fullBackupOrStructure = false)
        {
            var app = MFServerUtility.ConnectToMfApp(vault);
            Create(app, vault, templateRootPath, impersonationUserName, impersonationPassword, sqlDb,
                vaultIndexRootPath, fullBackupOrStructure);
        }
        public void CreateForAllBackup(MFilesVault vault, string templateRootPath,
          string impersonationUserName, string impersonationPassword, MFSqlDatabase sqlDb = null, string vaultIndexRootPath = null)
        {
            var app = MFServerUtility.ConnectToMfApp(vault);
            CreateForAllBackup(app, vault, templateRootPath, impersonationUserName, impersonationPassword, sqlDb,
                vaultIndexRootPath);
        }
        public bool Backup(MFilesVault vault)
        {
            var path = Path.Combine(Path.GetTempPath(), vault.Guid + ".mfb");
            var ovault = MFServerUtility.GetVault(vault);
            var backup = new BackupJob
                         {
                             BackupType = MFBackupType.MFBackupTypeFull,
                             TargetFile = path,
                             VaultGUID = ovault.GetGUID(),
                             OverwriteExistingFiles = true
                         };

            var app = MFServerUtility.ConnectToMfApp(vault);
            app.VaultManagementOperations.BackupVault(backup);
            return true;
        }

        public bool ImportApp(string zipPath, MFilesVault vault)
        {
            var zipDirectory = new DirectoryInfo(zipPath);
            var ovault = MFServerUtility.GetVault(vault);
            foreach (var di in zipDirectory.GetDirectories())
            {
                var targetPath = Path.Combine(Path.GetTempPath(), di.Name + ".zip");
                var zip = new ZipFile();
                zip.AddSelectedFiles("name!=*.zip and name!=*.pdb and name!=*.log", di.FullName, di.Name, true);
                zip.Save(targetPath);
                ImportApp(targetPath, ovault);
            }
            return true;
        }

        internal static bool ImportApp(string zipFile, Vault oVault)
        {
            try
            {
                oVault.CustomApplicationManagementOperations.InstallCustomApplication(zipFile);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Install app error, filename = " + zipFile, ex);
            }
            
        }

        internal static bool UninstallApp(string name, Vault oVault)
        {
            ServerUtils.UninstallAppByName(oVault, name);
            return true;
        }

        public bool UninstallApps(MFilesVault vault)
        {
            var ovault = MFServerUtility.GetVault(vault);
            var apps = ovault.CustomApplicationManagementOperations.GetCustomApplications();
            foreach (CustomApplication ca in apps)
            {
                ovault.CustomApplicationManagementOperations.UninstallCustomApplication(ca.ID);
            }
            return true;
        }

       

        public void ChangeVaultName(MFilesVault vault)
        {
            var app = MFServerUtility.ConnectToMfApp(vault);
            var props = app.VaultManagementOperations.GetVaultProperties(vault.Guid);
            props.DisplayName = vault.Name;
            var mfVault = app.LogInToVault(vault.Guid);
            mfVault.ManagementOperations.UpdateVaultProperties(props);
        }

        public bool HasUser(User user, string password, MFilesVault vault)
        {
            try
            {
                var serverApp = MFServerUtility.ConnetToMfApp(user, password, vault.Server);
                var mfvault= serverApp.LogInToVault(vault.Guid);
                return mfvault != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
