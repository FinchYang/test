using System;
using System.Collections.Generic;
using System.Text;
using AecCloud.MFilesCore;
using DBWorld.MailCore.Common;
using MFilesAPI;

namespace DBWorld.MailCore.MF
{
    public static class MfMailConfig
    {
        /// <summary>
        /// 查找当前登录用户的邮箱设置
        /// </summary>
        /// <param name="vault"></param>
        /// <returns></returns>
        private static ObjectSearchResults SearchMailConfig(Vault vault)
        {
            try
            {
                var oSearchConditions = new SearchConditions();
                var oSearchCondition1 = new SearchCondition();
                oSearchCondition1.ConditionType = MFConditionType.MFConditionTypeEqual;
                oSearchCondition1.Expression.DataPropertyValuePropertyDef =
                    (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                oSearchCondition1.TypedValue.SetValue(MFDataType.MFDatatypeLookup,
                    GetObjectClassId(vault, "ClassMailSettings"));
                oSearchConditions.Add(-1, oSearchCondition1);

                var oSearchCondition2 = new SearchCondition();
                oSearchCondition2.ConditionType = MFConditionType.MFConditionTypeEqual;
                oSearchCondition2.Expression.DataPropertyValuePropertyDef =
                    (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy;
                oSearchCondition2.TypedValue.SetValue(MFDataType.MFDatatypeLookup,
                    vault.SessionInfo.UserID);
                oSearchConditions.Add(-1, oSearchCondition2);
                return vault.ObjectSearchOperations.SearchForObjectsByConditions(oSearchConditions,
                    MFilesAPI.MFSearchFlags.MFSearchFlagNone,
                    false);
            }
            catch (Exception ex)
            {
                Common.Logger.Log.ErrorFormat("exception. search object from mfiles error: {0}", ex.Message);
            }

            return null;
        }

        /// <summary>
        /// 获取邮箱配置
        /// </summary>
        /// <returns></returns>
        public static MailConfig GetMailConfig(Vault vault)
        {
            var defConfig = new MailConfig
            {
                RecvPort = MailConfig.PopDefPort,
                SendPort = MailConfig.SmtpDefPort,
            };

            try
            {
                var result = SearchMailConfig(vault);
                if (result.Count > 0)
                {
                    var configs = new List<MailConfig>();
                    foreach (ObjectVersion obj in result)
                    {
                        var config = new MailConfig();
                        var properties = vault.ObjectPropertyOperations.GetProperties(obj.ObjVer, false);
                        SetMailConfigItem(vault, config, properties);
                        configs.Add(config);
                    }
                    return configs[0];
                }

                return defConfig;
            }
            catch (Exception ex)
            {
                Logger.Log.ErrorFormat("exception. get email config from mfiles error: {0}", ex.Message);
                return defConfig;
            }
        }

        /// <summary>
        /// 设置邮箱配置
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="config"></param>
        public static bool SetMailConfig(Vault vault, MailConfig config)
        {
            try
            {
                //密码加密
                var pwd = config.PassWord;
                var encode = Encoding.ASCII;
                var byteDate = encode.GetBytes(pwd);
                var encodePwd = Convert.ToBase64String(byteDate, 0, byteDate.Length);

                //属性列表
                var properties = new List<MfProperty>
                {
                    new MfProperty("PropMailFullname", MFDataType.MFDatatypeText, config.UserName),
                    new MfProperty("PropMailBox", MFDataType.MFDatatypeText, config.Email),
                    new MfProperty("PropPassword", MFDataType.MFDatatypeText, encodePwd),
                    new MfProperty("PropMailPop", MFDataType.MFDatatypeText, config.RecvAddr),
                    new MfProperty("PropMailSmtp", MFDataType.MFDatatypeText, config.SendAddr),
                    new MfProperty("PropMailPopPort", MFDataType.MFDatatypeText, config.RecvPort.ToString()),
                    new MfProperty("PropMailSmtpPort", MFDataType.MFDatatypeText, config.SendPort.ToString()),
                    new MfProperty("PropSSLReceive", MFDataType.MFDatatypeBoolean, config.RecvSSL),
                    new MfProperty("PropSSLSend", MFDataType.MFDatatypeBoolean, config.SendSSL),
                    new MfProperty("PropMailContains", MFDataType.MFDatatypeText, config.MarkUp),
                    new MfProperty("PropSignature", MFDataType.MFDatatypeMultiLineText, config.Signature)
                };

                var result = SearchMailConfig(vault);
                if (result.Count > 0)
                {
                    //更新设置
                    UpdatePropertis(vault,
                        vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjMailSettings"),
                        result[1].ObjVer.ObjID.ID,
                        GetMailConfigPropertyValues(vault, properties));

                    return true;
                }

                //创建设置
                var versionAndProperties = vault.ObjectOperations.CreateNewObject(
                    vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjMailSettings"),
                    GetMailConfigPropertyValues(vault, "ClassMailSettings", properties));
                vault.ObjectOperations.CheckIn(versionAndProperties.ObjVer);

                return true;

            }
            catch (Exception ex)
            {
                Common.Logger.Log.ErrorFormat("exception. set email config to mfiles error: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 通过库获取当前登录用户的用户名和Email
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        public static void GetNameAndEmailFromVault(Vault vault, ref string name, ref string email)
        {
            try
            {
                //获取登录用户ID
                var userId = vault.SessionInfo.UserID;

                //通过用户ID搜索邮件联系人
                var oSearchCondition = new SearchCondition();
                oSearchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
                oSearchCondition.Expression.DataPropertyValuePropertyDef =
                    vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropInnerUser");
                oSearchCondition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, userId);
                var result = vault.ObjectSearchOperations.SearchForObjectsByCondition(oSearchCondition, false);
                if (result.Count > 0)
                {
                    //获取邮件联系人的“姓名”和“邮箱”
                    var properties = vault.ObjectPropertyOperations.GetProperties(result[1].ObjVer);
                    if (properties == null || properties.Count == 0) return;
                    foreach (PropertyValue item in properties)
                    {
                        if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropLinkmanName"))
                        {
                            name = item.TypedValue.DisplayValue;
                        }

                        if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropEmail"))
                        {
                            email = item.TypedValue.DisplayValue;
                        }
                    }
                }
                else
                {
                    //创建“邮箱设置”时会自动创建“邮箱联系人”
                }
            }
            catch (Exception ex)
            {
                Logger.Log.ErrorFormat("exception. get username or email from mfiles error: {0}", ex.Message);
            }
        }

        /// <summary>
        /// 根据别名获取对象ID
        /// </summary>
        /// <param name="vault">库</param>
        /// <param name="aliasOrId">别名</param>
        /// <param name="throwOnError">异常</param>
        /// <returns></returns>
        private static int GetObjectClassId(Vault vault, string aliasOrId, bool throwOnError = true)
        {
            var ma = new MfAlias(aliasOrId);
            return ma.GetObjectClass(vault, throwOnError);
        }

        /// <summary>
        /// 获取对象的属性值
        /// </summary>
        /// <param name="vault">库</param>
        /// <param name="config">返回的对象</param>
        /// <param name="oProperties">对象属性</param>
        private static void SetMailConfigItem(Vault vault, MailConfig config, PropertyValues oProperties)
        {
            if (oProperties == null || oProperties.Count == 0) return;
            foreach (PropertyValue item in oProperties)
            {
                if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropMailFullname", true))
                {
                    config.UserName = item.TypedValue.DisplayValue;
                }
                else if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropMailBox", true))
                {
                    config.Email = item.TypedValue.DisplayValue;
                }
                else if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropPassword", true))
                {
                    //密码解密
                    var pwd = Convert.FromBase64String(item.TypedValue.DisplayValue);
                    config.PassWord = Encoding.Default.GetString(pwd);
                }
                else if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropMailPop", true))
                {
                    config.RecvAddr = item.TypedValue.DisplayValue;
                }
                else if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropMailSmtp", true))
                {
                    config.SendAddr = item.TypedValue.DisplayValue;
                }
                else if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropMailPopPort", true))
                {
                    if (String.IsNullOrEmpty(item.TypedValue.DisplayValue))
                    {
                        config.RecvPort = MailConfig.PopDefPort;
                    }
                    else
                    {
                        config.RecvPort = Convert.ToInt32(item.TypedValue.DisplayValue);
                    }
                }
                else if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropMailSmtpPort", true))
                {
                    if (String.IsNullOrEmpty(item.TypedValue.DisplayValue))
                    {
                        config.SendPort = MailConfig.SmtpDefPort;
                    }
                    else
                    {
                        config.SendPort = Convert.ToInt32(item.TypedValue.DisplayValue);
                    }
                }
                else if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropSSLReceive", true))
                {
                    try
                    {
                        config.RecvSSL = Convert.ToBoolean(item.TypedValue.Value);
                    }
                    catch (Exception)
                    {
                        config.RecvSSL = false;
                    }
                }
                else if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropSSLSend", true))
                {
                    try
                    {
                        config.SendSSL = Convert.ToBoolean(item.TypedValue.Value);
                    }
                    catch (Exception)
                    {
                        config.SendSSL = false;
                    }
                }
                else if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropMailContains", true))
                {
                    config.MarkUp = item.TypedValue.DisplayValue;
                }
                else if (item.PropertyDef == MfAlias.GetPropDef(vault, "PropSignature", true))
                {
                    config.Signature = item.TypedValue.DisplayValue;
                }
            }
        }

        /// <summary>
        /// 更新对象属性
        /// </summary>
        /// <param name="vault">库</param>
        /// <param name="typeId">类型ID</param>
        /// <param name="objId">对象ID</param>
        /// <param name="properties">属性列表</param>
        private static void UpdatePropertis(Vault vault, int typeId, int objId, PropertyValues properties)
        {
            var oObjVer = new ObjVer();
            oObjVer.SetIDs(typeId, objId, -1);
            var oObjVn = vault.ObjectOperations.GetObjectInfo(oObjVer, true, true);
            ObjectVersion checkOutVn;
            if (!oObjVn.ObjectCheckedOut)
            {
                try
                {
                    checkOutVn = vault.ObjectOperations.CheckOut(oObjVer.ObjID);
                }
                catch (Exception ex)
                {
                    throw new Exception("当前账户无权限签出对象:" + oObjVn.Title + "\r\n" + ex.Message);
                }

            }
            else if (oObjVn.CheckedOutTo == vault.SessionInfo.UserID || oObjVn.CheckedOutTo == -103)
            {
                checkOutVn = oObjVn;
            }
            else
            {
                ObjVer oldObjVer;
                try
                {
                    oldObjVer = vault.ObjectOperations.ForceUndoCheckout(oObjVn.ObjVer).ObjVer;
                }
                catch (Exception ex)
                {
                    throw new Exception("当前账户无权限撤销对象:" + oObjVn.Title + " 的签出\r\n" + ex.Message);
                }
                try
                {
                    checkOutVn = vault.ObjectOperations.CheckOut(oldObjVer.ObjID);
                }
                catch (Exception ex)
                {
                    throw new Exception("当前账户无权限签出对象:" + oObjVn.Title + "\r\n" + ex.Message);
                }
            }
            vault.ObjectPropertyOperations.SetProperties(checkOutVn.ObjVer, properties);
            try
            {
                vault.ObjectOperations.CheckIn(checkOutVn.ObjVer);
            }
            catch (Exception ex)
            {
                vault.ObjectOperations.ForceUndoCheckout(checkOutVn.ObjVer);
                throw new Exception("签入修改对象:" + oObjVn.Title + "失败！\r\n" + ex.Message); ;
            }
        }

        /// <summary>
        /// 生成对象属性
        /// </summary>
        /// <param name="vault">MFiles库对象</param>
        /// <param name="aliasName">对象别名</param>
        /// <param name="properties">属性集合</param>
        /// <returns></returns>
        private static PropertyValues GetMailConfigPropertyValues(Vault vault, string aliasName, IEnumerable<MfProperty> properties)
        {
            var oPropValues = new PropertyValues();

            var propValue1 = MFPropertyUtils.Lookup(
               (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass,
               vault.ClassOperations.GetObjectClassIDByAlias(aliasName));
            oPropValues.Add(-1, propValue1);

            foreach (var item in properties)
            {
                var propValue3 = new PropertyValue
                {
                    PropertyDef = vault.PropertyDefOperations.GetPropertyDefIDByAlias(item.PropertyName)
                };
                propValue3.TypedValue.SetValue(item.PropertyType, item.PropertyValue);
                oPropValues.Add(-1, propValue3);
            }

            return oPropValues;
        }

        /// <summary>
        /// 生成对象属性
        /// </summary>
        /// <param name="vault">MFiles库对象</param>
        /// <param name="properties">属性集合</param>
        /// <returns></returns>
        private static PropertyValues GetMailConfigPropertyValues(Vault vault, IEnumerable<MfProperty> properties)
        {
            var oPropValues = new PropertyValues();
            foreach (var item in properties)
            {
                var propValue = new PropertyValue
                {
                    PropertyDef = vault.PropertyDefOperations.GetPropertyDefIDByAlias(item.PropertyName)
                };
                propValue.TypedValue.SetValue(item.PropertyType, item.PropertyValue);
                oPropValues.Add(-1, propValue);
            }

            return oPropValues;
        }

    }
}
