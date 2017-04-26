using System;
using System.Collections.Generic;
using DBWorld.MailCore.Models;
using MFilesAPI;

namespace DBWorld.MailCore.MF
{
    public static class MessageFromMf
    {
        public static MailInfo GetMailFromMf(Vault vault, int msgId)
        {
            var mail = new MailInfo();

            try
            {
                var obj = MFilesUtil.GetVerAndProperties(vault,
                      (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument,
                      msgId);

                //邮件属性
                var properties = obj.Properties;
                if (properties == null || properties.Count == 0) return null;

                foreach (PropertyValue item in properties)
                {
                    if (item.PropertyDef == vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropMailSubject"))//邮件标题
                    {
                        mail.Subject = item.TypedValue.DisplayValue;
                       
                    }
                    else if (item.PropertyDef == vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropMailSender"))//发件人
                    {
                        mail.Sender = ConcatSemicolon(item.TypedValue.DisplayValue);
                    }
                    else if (item.PropertyDef == vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropMailReceiver"))//收件人
                    {
                        mail.Recivers = ConcatSemicolon(item.TypedValue.DisplayValue);
                    }
                    else if (item.PropertyDef == vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropMailCc"))//抄送人
                    {
                        mail.CC = ConcatSemicolon(item.TypedValue.DisplayValue);
                    }
                    else if (item.PropertyDef == vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropTags"))//标签
                    {
                        mail.Tag = item.TypedValue.DisplayValue;
                    }
                    else if (item.PropertyDef == vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropEmailAttachments"))//附件
                    {
                        mail.AttachsPath = new List<string>();
                        foreach (Lookup t in item.TypedValue.GetValueAsLookups())
                        {
                            var attachObj = MFilesUtil.GetVerAndProperties(vault, (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument, t.Item);
                            var attachFile = attachObj.VersionData.Files[1];
                            var attachPath = vault.ObjectFileOperations.GetPathInDefaultView(
                                attachObj.ObjVer.ObjID, 
                                attachObj.ObjVer.Version,
                                attachFile.FileVer.ID, attachFile.FileVer.Version);
                            mail.AttachsPath.Add(attachPath);
                        }
                    }
                    else if (item.PropertyDef == vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropMailCreatedTime"))//创建时间
                    {
                        mail.SentTime = item.TypedValue.DisplayValue;
                    }
                }

                //邮件文本
                var file = obj.VersionData.Files[1];
                mail.MailPath = vault.ObjectFileOperations.GetPathInDefaultView(obj.ObjVer.ObjID,
                    obj.ObjVer.Version,
                    file.FileVer.ID,
                    file.FileVer.Version);
            }
            catch (Exception ex)
            {
                Common.Logger.Log.ErrorFormat("Exception : Load email infomation error {0}.", ex.Message);
            }

            return mail;
        }

        public static string ConcatSemicolon(string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                if (!str.EndsWith(";"))
                {
                    str += ";";
                }
            }

            return str;
        }
    }
}
