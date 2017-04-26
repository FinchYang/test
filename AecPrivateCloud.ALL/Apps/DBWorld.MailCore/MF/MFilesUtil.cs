using System;
using MFilesAPI;

namespace DBWorld.MailCore.MF
{
    public static class MFilesUtil
    {
        public static string VaultName { get; set; }

        public static Vault GetVaultByName()
        {
            if (VaultName == null) throw new ArgumentException("VaultName");
            return GetVaultByName(VaultName);
        }

        /// <summary>
        /// 通过库连接名称获取库
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Vault GetVaultByName(string name)
        {
            var clientApp = new MFilesClientApplication();
            var vc = clientApp.GetVaultConnection(name);
            return vc.BindToVault(IntPtr.Zero, true, true);
        }
        /// <summary>
        /// 获取MFiles中的库
        /// </summary>
        /// <param name="guid">唯一标示</param>
        /// <returns></returns>
        public static Vault GetVaultWithGuid(string guid)
        {
            Vault vault = null;
            var clientApp = new MFilesClientApplication();
            VaultConnections conns = clientApp.GetVaultConnections();
            foreach (VaultConnection vConn in conns)
            {
                if (vConn.GetGUID() == guid)
                {
                    vault = vConn.BindToVault(IntPtr.Zero, true, true);
                    break;
                }
            }

            return vault;
        }

        /// <summary>
        /// 根据ID获取MFiles对象
        /// </summary>
        /// <param name="vault">库</param>
        /// <param name="objType">对象类型</param>
        /// <param name="objId">对象ID</param>
        /// <returns></returns>
        public static ObjectVersionAndProperties GetVerAndProperties(MFilesAPI.Vault vault, int objType, int objId)
        {
            try
            {
                var obj = new ObjID();
                obj.SetIDs(objType, objId);
                return vault.ObjectOperations.GetLatestObjectVersionAndProperties(obj, false, false);
            }
            catch (Exception)
            {
                throw ;
            }
        }
    }
}
