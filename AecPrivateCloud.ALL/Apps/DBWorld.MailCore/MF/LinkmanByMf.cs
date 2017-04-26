using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AecCloud.MFilesCore;
using DBWorld.MailCore.Models;
using MFilesAPI;

namespace DBWorld.MailCore.MF
{
    public class LinkmanByMf
    {
        #region 所有联系人

        /// <summary>
        /// 得到所有联系人
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="onlyIdAndTitle"></param>
        /// <returns></returns>
        public static IEnumerable<Linkman> GetLinkman(Vault vault, bool onlyIdAndTitle = false)
        {
            var linkman = GetObjectClass(vault, "ClassEmailAddressBook");
            var sConditions = new SearchConditions();
            AddSearchBaseCondition(sConditions, linkman);
            var sResults = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(sConditions,
                                  MFSearchFlags.MFSearchFlagNone, false, 0, 0);
            var lstLinkmans = new Collection<Linkman>();
            foreach (ObjectVersion objVn in sResults)
            {
                //lnkman.Name = objVn.Title;  
                var lnkman = new Linkman { Id = objVn.ObjVer.ID };

                if (!onlyIdAndTitle)
                {
                    var properties = vault.ObjectPropertyOperations.GetProperties(objVn.ObjVer, false);
                    LinkmanInfo(vault, lnkman, properties);
                }

                lstLinkmans.Add(lnkman);
            }
            return lstLinkmans;
        }

        /// <summary>
        /// 解析出联系人信息
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="lnkman"></param>
        /// <param name="oProperties"></param>
        private static void LinkmanInfo(Vault vault, Linkman lnkman, PropertyValues oProperties)
        {
            if (oProperties == null || oProperties.Count == 0) return;
            var propLinkmanName = MfAlias.GetPropDef(vault, "PropLinkmanName", true);  //姓名
            var propEmail = MfAlias.GetPropDef(vault, "PropEmail", true); //邮箱
            var propInerUse = MfAlias.GetPropDef(vault, "PropInnerUser", true); //内部用户
            foreach (PropertyValue item in oProperties)
            {
                if (item.PropertyDef == propLinkmanName)
                {
                    lnkman.Name = item.TypedValue.DisplayValue;
                }

                if (item.PropertyDef == propEmail)
                {
                    lnkman.Mail = item.TypedValue.DisplayValue;
                }

                if (item.PropertyDef == propInerUse)
                {
                    lnkman.InnerUser = item.TypedValue.DisplayValue;
                }
            }
        }

        /// <summary>
        /// 根据类别搜索对象
        /// </summary>
        /// <param name="oSearchConditions"></param>
        /// <param name="classId"></param>
        private static void AddSearchBaseCondition(SearchConditions oSearchConditions, int classId)
        {
            var oSearchCondition1 = new SearchCondition();
            oSearchCondition1.ConditionType = MFConditionType.MFConditionTypeEqual;
            oSearchCondition1.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
            oSearchCondition1.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classId);
            oSearchConditions.Add(-1, oSearchCondition1);
            var oSearchCondition2 = new SearchCondition();
            oSearchCondition2.ConditionType = MFConditionType.MFConditionTypeEqual;
            oSearchCondition2.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            oSearchCondition2.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            oSearchConditions.Add(-1, oSearchCondition2);
        }
        #endregion

        #region 最近联系人

        //最近联系人:在发件箱中找邮件对象，从邮件对象中找出收件人，找出的前五个最近时间联系的人。

        /// <summary>
        /// 最近联系人（已发邮件中查找对象）
        /// </summary>
        /// <param name="vault">库</param>
        /// <param name="count">读取个数(索引0开始)</param>
        /// <returns></returns>
        public static List<Linkman> SearchSendmailObj(Vault vault, int count)
        {
            var lstLinkmans = new Collection<Linkman>();
            var lstTemp = new List<Linkman>();
            var results = new List<Linkman>();
            try
            {
                var prjMail = GetObjectClass(vault, "ClassProjMail");//搜索项目邮件
                var sConditions = new SearchConditions();
                AddSearchBaseCondition(sConditions, prjMail, vault);
                var sResults = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(sConditions,
                                  MFSearchFlags.MFSearchFlagNone, false, 0, 0);
                foreach (ObjectVersion item in sResults)//邮件
                {
                    var lnkman = new Linkman { Id = item.ObjVer.ID };
                    var properties = vault.ObjectPropertyOperations.GetProperties(item.ObjVer, false);
                    SendMailInfo(vault, lnkman, properties);
                 
                    var mal = lnkman.Mail.Split(';');
                    foreach (var item2 in mal)
                    {
                        var tempLnkMan = new Linkman
                        {
                            Mail = item2.Trim()
                        };

                        if (!lstLinkmans.Contains(tempLnkMan))
                        {
                            lstLinkmans.Add(tempLnkMan);
                        } 
                    }
                }

                lstTemp = lstLinkmans.OrderByDescending(i => i.Id).ToList();
                for (int i = 0; i < lstTemp.Count; i++)
                {
                    if (i > count)
                    {
                        break;
                    }
                    results.Add(lstTemp[i]);
                }
            }
            catch (Exception ex)
            { 
                Common.Logger.Log.ErrorFormat("exception. search email from mfiles error: {0}", ex.Message);
            }

            return results;
        }


        /// <summary>
        /// 解析:发送邮件信息
        /// </summary>
        private static void SendMailInfo(Vault vault, Linkman lnkman, PropertyValues oProperties)
        {
            if (oProperties == null || oProperties.Count == 0) return;
            var propMailReceiver = MfAlias.GetPropDef(vault, "PropMailReceiver", true);  //收件人 
            var propMailCc = MfAlias.GetPropDef(vault, "PropMailCc", true);  //抄送
            foreach (PropertyValue item in oProperties)
            {
                if (item.PropertyDef == propMailReceiver)
                {
                    lnkman.Mail = item.TypedValue.DisplayValue;
                }

                if (item.PropertyDef == propMailCc)
                {
                    lnkman.Cc = item.TypedValue.DisplayValue;
                }
            }
        }

        /// <summary>
        /// 搜索发件箱
        /// </summary>
        /// <param name="oSearchConditions"></param>
        /// <param name="classId"></param>
        /// <param name="vault"></param>
        private static void AddSearchBaseCondition(SearchConditions oSearchConditions, int classId, Vault vault)
        {
            var oSearchCondition1 = new SearchCondition();
            oSearchCondition1.ConditionType = MFConditionType.MFConditionTypeEqual;
            oSearchCondition1.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
            oSearchCondition1.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classId);
            oSearchConditions.Add(-1, oSearchCondition1);
            var oSearchCondition2 = new SearchCondition();
            oSearchCondition2.ConditionType = MFConditionType.MFConditionTypeEqual;
            oSearchCondition2.Expression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            oSearchCondition2.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            oSearchConditions.Add(-1, oSearchCondition2);
            //只搜索发件箱
            var oSearchCondition3 = new SearchCondition();
            oSearchCondition3.ConditionType = MFConditionType.MFConditionTypeEqual;
            oSearchCondition3.Expression.DataPropertyValuePropertyDef =
                vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropMailFolders");
            oSearchCondition3.TypedValue.SetValue(MFDataType.MFDatatypeLookup, MailCore.MF.MessageToMf.GetFolderIdByName(vault, "发件箱"));
            oSearchConditions.Add(-1, oSearchCondition3);
        }

        #endregion

        #region 公共

        /// <summary>
        /// 对象类别
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="aliasOrId"></param>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        private static int GetObjectClass(Vault vault, string aliasOrId, bool throwOnError = true)
        {
            var ma = new MfAlias(aliasOrId);
            return ma.GetObjectClass(vault, throwOnError);
        }

        /// <summary>
        /// 生成对象属性
        /// </summary>
        /// <param name="vault">MFiles库对象</param>
        /// <param name="aliasName">类的别名</param>
        /// <param name="properties">属性集合</param>
        /// <returns></returns>
        private static PropertyValues GetContactsPropertyValues(Vault vault, string aliasName, IEnumerable<MfProperty> properties)
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

        #endregion

        #region 操作函数

        /// <summary>
        /// 查询id
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static int SearchLinkManId(Vault vault, string name, string email)
        {
            try
            {
                var oSearchCondition = new SearchCondition();
                oSearchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
                oSearchCondition.Expression.DataPropertyValuePropertyDef =
                    vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropAddressBookTitle");
                oSearchCondition.TypedValue.SetValue(MFDataType.MFDatatypeText,
                    String.Format("{0}<{1}>", name, email));
                var result = vault.ObjectSearchOperations.SearchForObjectsByCondition(oSearchCondition, false);

                if (result.Count == 0)
                {
                    //属性列表
                    var properties = new List<MfProperty> 
                    {
                        new MfProperty("PropAddressBookTitle", MFDataType.MFDatatypeText, String.Format("{0}<{1}>", name, email)),
                        new MfProperty("PropLinkmanName", MFDataType.MFDatatypeText, name),
                        new MfProperty("PropEmail", MFDataType.MFDatatypeText, email)
                    };

                    //创建联系人
                    var versionAndProperties = vault.ObjectOperations.CreateNewObject(
                        vault.ObjectTypeOperations.GetObjectTypeIDByAlias("ObjEmailAdressBook"),
                        GetContactsPropertyValues(vault, "ClassEmailAddressBook", properties));
                    vault.ObjectOperations.CheckIn(versionAndProperties.ObjVer);
                    return versionAndProperties.ObjVer.ObjID.ID;
                }
                else
                {
                    //获取联系人
                    for (int i = 1; i <= result.Count; i++)
                    {
                        return result[i].ObjVer.ObjID.ID;
                    }
                }

            }
            catch (Exception ex)
            {
                Common.Logger.Log.ErrorFormat("exception. search object from mfiles error: {0}", ex.Message);
            }

            return -1;
        }

        #endregion

    }
}
