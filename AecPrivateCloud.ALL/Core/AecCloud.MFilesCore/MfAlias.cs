using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFilesAPI;

namespace AecCloud.MFilesCore
{
    /// <summary>
    /// 别名 -> ID
    /// </summary>
    [Serializable]
    public class MfAlias
    {
        /// <summary>
        /// 若为数字字符串，则直接当成ID
        /// </summary>
        public string NameOrId { get; private set; }

        public bool IsId { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aliasOrId">若为数字字符串，则直接当成ID</param>
        public MfAlias(string aliasOrId)
        {
            int id;
            var ok = Int32.TryParse(aliasOrId, out id);
            if (ok)
            {
                IsId = true;
            }
            NameOrId = aliasOrId;
        }

        public MfAlias(string aliasOrId, bool isId)
        {
            NameOrId = aliasOrId;
            IsId = isId;
        }

        public int GetPropDef(Vault vault, bool throwOnError = true)
        {
            var invalidValue = -1;
            int id = invalidValue;
            if (!IsId)
            {
                id = vault.PropertyDefOperations.GetPropertyDefIDByAlias(NameOrId);
                if (id != invalidValue) return id;
            }
            else
            {
                id = Int32.Parse(NameOrId);
                if (id != invalidValue)
                {
                    try
                    {
                        var p = vault.PropertyDefOperations.GetPropertyDef(id);
                        if (p != null) return id;
                    }
                    catch
                    {
                    }
                }
            }
            if (throwOnError)
            {
                throw new Exception(String.Format("未找到属性：{0}, ID:{1}", NameOrId, id));
            }
            return id;
        }
        public int GetObjType(Vault vault, bool throwOnError = true)
        {
            var invalidValue = -1;
            int id = invalidValue;
            if (!IsId)
            {
                id = vault.ObjectTypeOperations.GetObjectTypeIDByAlias(NameOrId);
                if (id != invalidValue) return id;
            }
            else
            {
                id = Int32.Parse(NameOrId);
                if (id != invalidValue)
                {
                    try
                    {
                        var o = vault.ObjectTypeOperations.GetObjectType(id);
                        if (o != null) return id;
                    }
                    catch
                    { }
                }
            }
            if (throwOnError)
            {
                throw new Exception(String.Format("未找到对象类型：{0}, ID:{1}", NameOrId, id));
            }
            return id;
        }

        public int GetValueList(Vault vault, bool throwOnError = true)
        {
            var invalidValue = -1;
            int id = invalidValue;
            if (!IsId)
            {
                id = vault.ObjectTypeOperations.GetObjectTypeIDByAlias(NameOrId);
                if (id != invalidValue) return id;
                return id;
            }
            else
            {
                id = Int32.Parse(NameOrId);
                if (id != invalidValue)
                {
                    try
                    {
                        var v = vault.ValueListOperations.GetValueList(id);
                        if (v != null) return id;
                    }
                    catch { }
                }
            }
            if (throwOnError)
            {
                throw new Exception(String.Format("未找到值列表：{0}, ID:{1}", NameOrId, id));
            }
            return id;
        }
        public int GetObjectClass(Vault vault, bool throwOnError = true)
        {
            var invalidValue = -1;
            int id = invalidValue;
            if (!IsId)
            {
                id = vault.ClassOperations.GetObjectClassIDByAlias(NameOrId);
                if (id != invalidValue) return id;
            }
            else
            {
                id = Int32.Parse(NameOrId);
                if (id != invalidValue)
                {
                    try
                    {
                        var o = vault.ClassOperations.GetObjectClass(id);
                        if (o != null) return id;
                    }
                    catch { }
                }
            }
            if (throwOnError)
            {
                throw new Exception(String.Format("未找到值列表：{0}, ID:{1}", NameOrId, id));
            }
            return id;
        }
        public int GetWorkflow(Vault vault, bool throwOnError = true)
        {
            var invalidValue = -1;
            int id = invalidValue;
            if (!IsId)
            {
                id = vault.WorkflowOperations.GetWorkflowIDByAlias(NameOrId);
                if (id != invalidValue) return id;
            }
            else
            {
                id = Int32.Parse(NameOrId);
                if (id != invalidValue)
                {
                    try
                    {
                        var w = vault.WorkflowOperations.GetWorkflowStates(id);
                        if (w != null) return id;
                    }
                    catch { }
                }
            }
            if (throwOnError)
            {
                throw new Exception(String.Format("未找到工作流：{0}, ID:{1}", NameOrId, id));
            }
            return id;
        }
        public int GetWorkflowState(Vault vault, bool throwOnError = true)
        {
            var invalidValue = -1;
            int id = invalidValue;
            if (!IsId)
            {
                id = vault.WorkflowOperations.GetWorkflowStateIDByAlias(NameOrId);
            }
            else
            {
                id = Int32.Parse(NameOrId);
            }
            if (throwOnError && id == invalidValue)
            {
                throw new Exception(String.Format("未找到工作流状态：{0}, ID:{1}", NameOrId, id));
            }
            return id;
        }
        public int GetNamedACL(Vault vault, bool throwOnError = true)
        {
            var invalidValue = -1000;
            int id = invalidValue;
            if (!IsId)
            {
                id = vault.NamedACLOperations.GetNamedACLIDByAlias(NameOrId);
                if (id != invalidValue) return id;
            }
            else
            {
                id = Int32.Parse(NameOrId);
                if (id != invalidValue)
                {
                    try
                    {
                        var n = vault.NamedACLOperations.GetNamedACL(id);
                        if (n != null) return id;
                    }
                    catch { }
                }
            }
            if (throwOnError)
            {
                throw new Exception(String.Format("未找到命名访问控制：{0}, ID:{1}", NameOrId, id));
            }
            return id;
        }
        public int GetUsergroup(Vault vault, bool throwOnError = true)
        {
            var invalidValue = -1;
            int id = invalidValue;
            if (!IsId)
            {
                id = vault.UserGroupOperations.GetUserGroupIDByAlias(NameOrId);
                if (id != invalidValue) return id;
            }
            else
            {
                id = Int32.Parse(NameOrId);
                if (id != invalidValue)
                {
                    try
                    {
                        var u = vault.UserGroupOperations.GetUserGroup(id);
                        if (u != null) return id;
                    }
                    catch { }
                }
            }
            if (throwOnError)
            {
                throw new Exception(String.Format("未找到用户组：{0}, ID:{1}", NameOrId, id));
            }
            return id;
        }

        /// <summary>
        /// 属性定义
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="aliasOrId"></param>
        /// <returns></returns>
        public static int GetPropDef(Vault vault, string aliasOrId, bool throwOnError=true)
        {
            var ma = new MfAlias(aliasOrId);
            return ma.GetPropDef(vault, throwOnError);
        }
        /// <summary>
        /// 对象类型
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="aliasOrId"></param>
        /// <returns></returns>
        public static int GetObjType(Vault vault, string aliasOrId, bool throwOnError = true)
        {
            var ma = new MfAlias(aliasOrId);
            return ma.GetObjType(vault, throwOnError);
        }        
        /// <summary>
        /// 值列表
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="aliasOrId"></param>
        /// <returns></returns>
        public static int GetValueList(Vault vault, string aliasOrId, bool throwOnError = true)
        {
            var ma = new MfAlias(aliasOrId);
            return ma.GetValueList(vault, throwOnError);
        }        
        /// <summary>
        /// 对象类别
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="aliasOrId"></param>
        /// <returns></returns>
        public static int GetObjectClass(Vault vault, string aliasOrId, bool throwOnError = true)
        {
            var ma = new MfAlias(aliasOrId);
            return ma.GetObjectClass(vault, throwOnError);
        }        
        /// <summary>
        /// 工作流
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="aliasOrId"></param>
        /// <returns></returns>
        public static int GetWorkflow(Vault vault, string aliasOrId, bool throwOnError = true)
        {
            var ma = new MfAlias(aliasOrId);
            return ma.GetWorkflow(vault, throwOnError);
        }        
        /// <summary>
        /// 工作流状态
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="aliasOrId"></param>
        /// <returns></returns>
        public static int GetWorkflowState(Vault vault, string aliasOrId, bool throwOnError = true)
        {
            var ma = new MfAlias(aliasOrId);
            return ma.GetWorkflowState(vault, throwOnError);
        }        
        /// <summary>
        /// 命名对象控制权限
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="aliasOrId"></param>
        /// <returns></returns>
        public static int GetNamedACL(Vault vault, string aliasOrId, bool throwOnError = true)
        {
            var ma = new MfAlias(aliasOrId);
            return ma.GetNamedACL(vault, throwOnError);
        }       
        /// <summary>
        /// 用户组
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="aliasOrId"></param>
        /// <returns></returns>
        public static int GetUsergroup(Vault vault, string aliasOrId, bool throwOnError = true)
        {
            var ma = new MfAlias(aliasOrId);
            return ma.GetUsergroup(vault, throwOnError);
        }
    }
}
