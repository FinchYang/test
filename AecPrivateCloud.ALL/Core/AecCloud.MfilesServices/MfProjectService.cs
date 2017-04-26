using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore.Metadata;
using log4net;
using MFilesAPI;
using Newtonsoft.Json;

namespace AecCloud.MfilesServices
{
    public class MfProjectService : IMfProjectService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Result Create(Project proj, User creator, VaultTemplate template, MFilesVault vault, MFSqlDatabase sqlDb, 
            string userName, string password, ProjectParty party)
        {
            var app = MFServerUtility.ConnectToMfApp(vault);
            //1. 判断库是否已存在
            var hasVault = MFVaultService.HasVault(app, vault);
            if (hasVault)
            {
                return new Result {Message = "已存在同名的库！"};
            }
            //2.创建库
            Vault mVault = null;
            try
            {
                mVault = MFVaultService.Create(app, vault, template.StructurePath, userName, password, sqlDb, null, false);
            }
            catch (Exception ex)
            {
                Log.Error("创建文档库失败：" + ex.Message, ex);
                return new Result {Message = ex.Message, Exception = ex};
            }
            if (mVault == null)
            {
                mVault = app.LogInToVault(vault.Guid);
            }
            //3.创建MF用户及库用户
            int mfUserId = -1;
            try
            {
                mfUserId = MFUserService.CreateVaultUser(mVault, creator);
            }
            catch(Exception ex)
            {
                var err = "创建或启用库账户失败：" + ex.Message;
                Log.Error(err, ex);
                return new Result {Message = err, Exception = ex};
            }
            if (mfUserId == -1)
            {
                return new Result { Message = "创建或启用库账户失败"};
            }
            var aliases = JsonConvert.DeserializeObject<MetadataAliases>(template.MetadataJson);
            //4.创建联系人对象
            string partyName = null;
            if (party != null) partyName = party.Name;
            var contact = new MfContact
            {
                User = creator,
                Id = mfUserId,
                IsCreator = true,
                RoleAlias = aliases.UserGroups["UGroupPM"]
            };
            if (partyName != null)
            {
                contact.PartName = partyName;
            }
            try
            {
                MFObjectService.Create(mVault, aliases, contact);
            }
            catch (Exception ex)
            {
                var err = "创建库的联系人失败：" + ex.Message;
                Log.Error(err, ex);
                return new Result { Message = err, Exception = ex };
            }
            //5.创建项目对象
            try
            {
                ToLocalTimeProj(proj);
                MFObjectService.Create(mVault, aliases, proj);
                ToUtcTimeProj(proj);
            }
            catch (Exception ex)
            {
                var err = "创建库中项目对象失败：" + ex.Message;
                Log.Error(err, ex);
                return new Result { Message = err, Exception = ex };
            }
            //install vaultapp
            //try
            //{
            //    var tmpfile = System.IO.Path.Combine(HttpContext.Current.Server.MapPath("~"), "vault.mfappx");
            //    mVault.CustomApplicationManagementOperations.InstallCustomApplication(tmpfile);
            //}
            //catch (Exception ex)
            //{
            //    var err = "创建库中vaultapp error：" + ex.Message;
            //    Log.Error(err, ex);
            //    return new Result { Message = err, Exception = ex };
            //}
            return new Result {Success = true, Contact = contact};
        }

        private void ToLocalTimeProj(Project proj)
        {
            proj.StartDateUtc = proj.StartDateUtc.ToLocalTime();
            proj.EndDateUtc = proj.EndDateUtc.ToLocalTime();
        }

        private void ToUtcTimeProj(Project proj)
        {
            proj.StartDateUtc = proj.StartDateUtc.ToUniversalTime();
            proj.EndDateUtc = proj.EndDateUtc.ToUniversalTime();
        }
    }
}
