using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.MfilesServices
{
    public interface IMFVaultService
    {
        bool HasVault(MFilesVault vault);

        /// <summary>
        /// 1、 创建(From Template)
        /// 2、全备份还原建库，存储全备份文件全路径，形如 D:\ttt\1.mfb
        /// 3、导出数据和结构建库，存储导入路径，形如 D:\ttt\{6F0AB68C-2A32-44D1-A39D-32D9F8F433F2}_20150120_153050
        /// </summary>
        void Create(MFilesVault vault, string templateRootPath, string impersonationUserName, string impersonationPassword, MFSqlDatabase sqlDb = null,
            string vaultIndexRootPath = null, bool fullBackupOrStructure = false);
        void CreateForAllBackup(MFilesVault vault, string templateRootPath, string impersonationUserName, string impersonationPassword, MFSqlDatabase sqlDb = null,
          string vaultIndexRootPath = null);

        void CreateDefaultVault(MFilesVault vault);

        /// <summary>
        /// 执行vault全备份，备份时放存储目录，形如 D:\ttt
        /// </summary>
        bool  Backup(MFilesVault vault);

        ///  /// <summary>
        /// 执行//安装库App 
        /// </summary>
        /// <param name="zipPath">需打包并安装的所有应用的总目录，形如 D:\ttt，每个字目录代表一个mfiles应用</param>
        /// <param name="vault">MFilesVault对象 </param>
        bool ImportApp(string zipPath, MFilesVault vault);

        /////  /// <summary>
        ///// 执行//卸载库的一个App 
        ///// </summary>
        ///// <param name="name">应用名称</param>
        ///// <param name="oVault">MFiles Vault对象 </param>
        //bool UninstallApp(string name, Vault oVault);

        ///  /// <summary>
        /// 执行//卸载库的所有App  
        /// </summary>
        /// <param name="vault">MFilesVault对象 </param>
        bool UninstallApps(MFilesVault vault);


        /////  <summary>
        ///// 使用过滤器ExportStructureItems导出库结构
        /////  </summary>
        /////  <param name="vault">MFilesVault对象 </param>
        ///// <param name="exportStructureItems">MFiles ExportStructureItems对象</param>
        //bool ExportContent(MFilesVault vault);

        /////  <summary>
        ///// 导入库结构、库对象
        /////  </summary>
        /////  <param name="vault">MFilesVault对象 </param>
        //bool ImportContent(MFilesVault vault);

        /////  <summary>
        /////优化 
        ///// </summary>
        ///// <param name="vault">MFilesVault对象 </param>
        ///// <param name="metadata">If this parameter is True,  the full-text search index is rebuilt for metadata.  </param>
        ///// <param name="fileContents">If this parameter is True, the full-text search index is rebuilt for file data.   </param>
        //bool RebuildFullTextSearchIndex(MFilesVault vault, bool metadata, bool fileContents);

        void ChangeVaultName(MFilesVault vault);


        bool HasUser(User user, string password, MFilesVault vault);

    }
}
