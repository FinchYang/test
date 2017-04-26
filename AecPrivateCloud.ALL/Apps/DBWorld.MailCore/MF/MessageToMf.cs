using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using MFilesAPI;
using MailKit.Net.Pop3;
using MimeKit;
using MimeKit.Text;

namespace DBWorld.MailCore.MF
{
    public static class MessageToMf
    {
        /// <summary>
        /// 在MFiles中搜索
        /// </summary>
        /// <param name="vault">库</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public static ObjectSearchResults SearchMailFromMf(Vault vault, string value)
        {
            try
            {
                var oSearchCondition = new SearchCondition();
                oSearchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
                oSearchCondition.Expression.DataPropertyValuePropertyDef =
                    vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropTags");
                oSearchCondition.TypedValue.SetValue(MFDataType.MFDatatypeText, value);

                return vault.ObjectSearchOperations.SearchForObjectsByCondition(oSearchCondition, false);
            }
            catch (Exception ex)
            {
                Common.Logger.Log.ErrorFormat("exception. search email from mfiles error: {0}", ex.Message);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLastTimeFromMail(Vault vault)
        {
            var lastTime = new DateTime();

            try
            {
                //获取收件箱中的邮件对象
                var oSearchCondition = new SearchCondition();
                oSearchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
                oSearchCondition.Expression.DataPropertyValuePropertyDef =
                    vault.PropertyDefOperations.GetPropertyDefIDByAlias("PropMailFolders");
                oSearchCondition.TypedValue.SetValue(MFDataType.MFDatatypeText, "收件箱");
                var objs = vault.ObjectSearchOperations.SearchForObjectsByCondition(oSearchCondition, false);

                //获取邮件对象的创建时间
                foreach (ObjectVersion obj in objs)
                {
                    var createTime = obj.CreatedUtc.ToLocalTime();
                    if (lastTime < createTime)
                    {
                        lastTime = createTime;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.Logger.Log.ErrorFormat("exception. search email from mfiles error: {0}", ex.Message);
            }

            return lastTime;
        }

        /// <summary>
        /// 接收的邮件写入MFiles
        /// </summary>
        /// <param name="vault">库</param>
        /// <param name="msg">邮件对象</param>
        /// <param name="folderId">文件ID</param>
        public static bool SaveRecvMailToMf(Vault vault, MimeMessage msg, int folderId)
        {
            var verIds = new List<int>();
            var fileList = new List<string>();

            try
            {
                //保存附件
                var attachments = msg.Attachments.OfType<MimePart>().ToList();
                for (int i = 0; i < attachments.Count; i++)
                {
                    var attach = attachments[i];
                    var filePath = GetTempFilePath(Path.GetExtension(attach.FileName));
                    MimePartToFile(attach, filePath);
                    fileList.Add(filePath);

                    //保存到MFiles
                    var sourceFiles = new SourceObjectFiles();
                    var sourceFile = new SourceObjectFile
                    {
                        SourceFilePath = filePath,
                        Title = attach.FileName.Substring(0, attach.FileName.LastIndexOf('.')),
                        Extension = attach.FileName.Substring(attach.FileName.LastIndexOf('.') + 1),
                    };
                    sourceFiles.Add(i, sourceFile);

                    var versionAndProperties = vault.ObjectOperations.CreateNewObject(
                        (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument,
                        GetAttachmentPropertyValues(vault, sourceFile.Title, "ClassMailAttachments"),
                        sourceFiles);
                    vault.ObjectOperations.CheckIn(versionAndProperties.ObjVer);
                    verIds.Add(versionAndProperties.ObjVer.ObjID.ID);
                }

                {
                    //保存邮件
                    var filePath = GetTempFilePath(".html");
                    using (var writer = File.CreateText(filePath))
                    {
                        writer.Write(GetMailHtml(msg));
                        writer.Flush();
                        writer.Close();
                    }
                    fileList.Add(filePath);

                    //保存到MFiles
                    var properties = new List<MfProperty>
                    {
                        new MfProperty("PropMailSender", MFDataType.MFDatatypeLookup, ConvertAddrsToIds(vault, msg.From.Mailboxes)),
                        new MfProperty("PropMailReceiver", MFDataType.MFDatatypeMultiSelectLookup, ConvertAddrsToIds(vault, msg.To.Mailboxes)),
                        new MfProperty("PropMailCc", MFDataType.MFDatatypeMultiSelectLookup, ConvertAddrsToIds(vault, msg.Cc.Mailboxes)),
                        new MfProperty("PropMailSubject", MFDataType.MFDatatypeText, msg.Subject),
                        new MfProperty("PropMailCreatedTime", MFDataType.MFDatatypeDate, msg.Date.DateTime),
                        new MfProperty("PropMailFolders", MFDataType.MFDatatypeLookup, folderId),
                        new MfProperty("PropTags", MFDataType.MFDatatypeText, msg.MessageId),
                        new MfProperty("PropEmailAttachments", MFDataType.MFDatatypeMultiSelectLookup, verIds.ToArray()),
                        new MfProperty("PropIsRead", MFDataType.MFDatatypeBoolean, false)
                    };

                    var sourceFiles = new SourceObjectFiles();
                    var sourceFile = new SourceObjectFile
                    {
                        SourceFilePath = filePath,
                        Title = msg.Subject,
                        Extension = "html"
                    };
                    sourceFiles.Add(0, sourceFile);

                    var versionAndProperties = vault.ObjectOperations.CreateNewObject(
                        (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument,
                        GetMailContentPropertyValues(vault, "ClassProjMail", properties),
                        sourceFiles);
                    vault.ObjectOperations.CheckIn(versionAndProperties.ObjVer);
                }
            }
            catch (Exception ex)
            {
                Common.Logger.Log.ErrorFormat("exception. save received email to mfiles error: {0}", ex.Message);
                return false;
            }

            //删除临时文件
            DelFileList(fileList);
            return true;
        }

        /// <summary>
        /// 发送的邮件写入MFiles
        /// </summary>
        /// <param name="vault">库</param>
        /// <param name="msg">邮件对象</param>
        /// <param name="folderId">文件夹ID</param>
        public static bool SaveSendMailToMf(Vault vault, MailMessage msg, int folderId)
        {
            var result = false;
            var verIds = new List<int>();
            var fileList = new List<string>();

            try
            {
                //保存附件
                var attachs = msg.Attachments;
                if (attachs.Count > 0)
                {
                    for (var i = 0; i < attachs.Count; i++)
                    {
                        //保存到本地
                        var attach = attachs[i];
                        var filePath = GetTempFilePath(Path.GetExtension(attach.Name));
                        var fileStream = attach.ContentStream as FileStream;
                        fileList.Add(filePath);

                        //保存到MFiles
                        var sourceFiles = new SourceObjectFiles();
                        if (fileStream != null)
                        {
                            var sourceFile = new SourceObjectFile
                            {
                                SourceFilePath = fileStream.Name,
                                Title = attach.Name.Substring(0, attach.Name.LastIndexOf('.')),
                                Extension = attach.Name.Substring(attach.Name.LastIndexOf('.') + 1),
                            };
                            sourceFiles.Add(i, sourceFile);

                            var versionAndProperties = vault.ObjectOperations.CreateNewObject(
                                (int)MFilesAPI.MFBuiltInObjectType.MFBuiltInObjectTypeDocument,
                                GetAttachmentPropertyValues(vault, sourceFile.Title, "ClassMailAttachments"),
                                sourceFiles);
                            vault.ObjectOperations.CheckIn(versionAndProperties.ObjVer);
                            verIds.Add(versionAndProperties.ObjVer.ObjID.ID);
                        }
                    }
                }

                //保存邮件
                var html = CombineHtmlString(msg.Body);
                if (html.Length > 0)
                {
                    //保存到本地
                    var filePath = GetTempFilePath(".html");
                    using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        writer.Write(html);
                        writer.Flush();
                        writer.Close();
                    }
                    fileList.Add(filePath);

                    //保存到MFiles
                    var properties = new List<MfProperty>
                    {
                        new MfProperty("PropMailSender", MFDataType.MFDatatypeLookup, ConvertAddrToId(vault, msg.From)),
                        new MfProperty("PropMailReceiver", MFDataType.MFDatatypeMultiSelectLookup, ConvertAddrsToIds(vault, msg.To)),
                        new MfProperty("PropMailCc", MFDataType.MFDatatypeMultiSelectLookup, ConvertAddrsToIds(vault, msg.CC)),
                        new MfProperty("PropMailSubject", MFDataType.MFDatatypeText, msg.Subject),
                        new MfProperty("PropMailCreatedTime", MFDataType.MFDatatypeDate, DateTime.Now.ToUniversalTime()),
                        new MfProperty("PropMailFolders", MFDataType.MFDatatypeLookup, folderId),
                        new MfProperty("PropTags", MFDataType.MFDatatypeText, msg.Headers.Get("MessageId")),
                        new MfProperty("PropEmailAttachments", MFDataType.MFDatatypeMultiSelectLookup, verIds.ToArray()),
                        new MfProperty("PropIsRead", MFDataType.MFDatatypeBoolean, true)
                    };

                    var sourceFiles = new SourceObjectFiles();
                    var sourceFile = new SourceObjectFile
                    {
                        SourceFilePath = filePath,
                        Title = msg.Subject,
                        Extension = "html"
                    };
                    sourceFiles.Add(0, sourceFile);

                    //
                    var searchObj = SearchMailFromMf(vault, msg.Headers.Get("MessageId"));
                    if (searchObj.Count == 0)
                    {
                        //创建邮件对象
                        result = CreateMailToMf(vault, GetMailContentPropertyValues(vault, "ClassProjMail", properties), sourceFiles); 
                    }
                    else
                    {
                        //更新邮件对象
                        result = UpdateMailToMf(vault, searchObj[1], GetMailContentPropertyValues(vault, properties), sourceFiles);
                    }

                    //Todo
                    //发送草稿
                }
            }
            catch (Exception ex)
            {
                Common.Logger.Log.ErrorFormat("exception. save sent email to mfiles error: {0}", ex.Message);
                result = false;
            }
            
            //删除临时文件
            DelFileList(fileList);
            return result;
        }

        /// <summary>
        /// 创建邮件对象
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="properties"></param>
        /// <param name="sourceFiles"></param>
        private static bool CreateMailToMf(Vault vault, PropertyValues properties, SourceObjectFiles sourceFiles)
        {
            try
            {
                var versionAndProperties = vault.ObjectOperations.CreateNewObject(
                    (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument,
                    properties,
                    sourceFiles);
                vault.ObjectOperations.CheckIn(versionAndProperties.ObjVer);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

       /// <summary>
        /// 更新邮件对象
       /// </summary>
       /// <param name="vault"></param>
       /// <param name="objVer"></param>
       /// <param name="properties"></param>
       /// <param name="sourceFiles"></param>
        private static bool UpdateMailToMf(Vault vault, ObjectVersion objVer, PropertyValues properties, SourceObjectFiles sourceFiles)
        {
           
            //签出对象邮件对象
            ObjectVersion checkOutVn = null;
            if (!objVer.ObjectCheckedOut)
            {
                try
                {
                    checkOutVn = vault.ObjectOperations.CheckOut(objVer.ObjVer.ObjID);
                }
                catch (Exception ex)
                {
                    Common.Logger.Log.ErrorFormat("exception. update email to mfiles error: {0}", ex.Message);
                    return false;
                }

            }
            else if (objVer.CheckedOutTo == vault.SessionInfo.UserID || objVer.CheckedOutTo == -103)
            {
                checkOutVn = objVer;
            }
            else
            {
                ObjVer oldObjVer = null;
                try
                {
                    oldObjVer = vault.ObjectOperations.ForceUndoCheckout(objVer.ObjVer).ObjVer;
                }
                catch (Exception ex)
                {
                    Common.Logger.Log.ErrorFormat("exception. update email to mfiles error: {0}", ex.Message);
                    return false;
                }

                try
                {
                    if (oldObjVer != null) checkOutVn = vault.ObjectOperations.CheckOut(oldObjVer.ObjID);
                }
                catch (Exception ex)
                {
                    Common.Logger.Log.ErrorFormat("exception. update email to mfiles error: {0}", ex.Message);
                    return false;
                }
            }

            try
            {
                //更新邮件对象属性
                if (checkOutVn != null)
                {
                    //获取邮件正文文件对象
                    var objFiles = vault.ObjectFileOperations.GetFiles(checkOutVn.ObjVer);
                    vault.ObjectPropertyOperations.SetProperties(checkOutVn.ObjVer, properties);
                    vault.ObjectOperations.SetSingleFileObject(checkOutVn.ObjVer, false);
                    vault.ObjectFileOperations.RemoveFile(checkOutVn.ObjVer, objFiles[1].FileVer);
                    vault.ObjectFileOperations.AddFile(checkOutVn.ObjVer,
                        sourceFiles[1].Title,
                        sourceFiles[1].Extension,
                        sourceFiles[1].SourceFilePath);
                    vault.ObjectOperations.SetSingleFileObject(checkOutVn.ObjVer, true);

                    //签入对象
                    vault.ObjectOperations.CheckIn(checkOutVn.ObjVer);
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (checkOutVn != null) vault.ObjectOperations.ForceUndoCheckout(checkOutVn.ObjVer);
                Common.Logger.Log.ErrorFormat("exception. update email to mfiles error: {0}", ex.Message);
                return false;
            }

            return false;
        }

        /// <summary>
        /// 从MFiles中删除邮件，彻底删除
        /// </summary>
        /// <param name="vault">库</param>
        /// <param name="msg">邮件</param>
        /// <returns></returns>
        public static bool DeleteMailFromMf(Vault vault, MailMessage msg)
        {
            try
            {
                var result = SearchMailFromMf(vault, msg.Headers.Get("MessageId"));

                if (result.Count > 0)
                {
                    for (int i = 1; i <= result.Count; i++)
                    {
                        var objIDs = new ObjIDs();
                        objIDs.Add(-1, result[i].ObjVer.ObjID);
                        vault.ObjectOperations.DestroyObjects(objIDs);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.Logger.Log.ErrorFormat("exception. delete email from mfiles error: {0}", ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 从MFiles中移除邮件，非彻底删除
        /// </summary>
        /// <param name="vault">库</param>
        /// <param name="msg">邮件</param>
        /// <returns></returns>
        public static bool RemoveMailFromMf(Vault vault, MailMessage msg)
        {
            try
            {
                var result = SearchMailFromMf(vault, msg.Headers.Get("MessageId"));
                if (result.Count > 0)
                {
                    for (int i = 1; i <= result.Count; i++)
                    {
                        vault.ObjectOperations.RemoveObject(result[i].ObjVer.ObjID);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.Logger.Log.ErrorFormat("exception. remove email from mfiles error: {0}", ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 根据文件夹名称获取文件夹ID
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static int GetFolderIdByName(Vault vault, string folderName)
        {
            var id = vault.ValueListOperations.GetValueListIDByAlias("VLMailFolders");
            var items = vault.ValueListItemOperations.GetValueListItems(id);
            for (int i = 1; i <= items.Count; i++)
            {
                var name = items[i].Name;
                if (name == folderName)
                {
                    return items[i].ID;
                }
            }

            return 0;
        }

        /// <summary>
        /// 生成对象属性
        /// </summary>
        /// <param name="vault">MFiles库对象</param>
        /// <param name="title">对象名称</param>
        /// <param name="aliasName">对象别名</param>
        /// <returns></returns>
        private static PropertyValues GetAttachmentPropertyValues(Vault vault, string title, string aliasName)
        {
            var oPropValues = new PropertyValues();
            var propValue1 = AecCloud.MFilesCore.MFPropertyUtils.Text(
                (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle, title);
            oPropValues.Add(-1, propValue1);

            var propValue2 = AecCloud.MFilesCore.MFPropertyUtils.Lookup(
                (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass,
                vault.ClassOperations.GetObjectClassIDByAlias(aliasName));
            oPropValues.Add(-1, propValue2);

            var propValue3 = AecCloud.MFilesCore.MFPropertyUtils.SingleFile(true);
            oPropValues.Add(-1, propValue3);

            return oPropValues;
        }

        /// <summary>
        /// 生成对象属性
        /// </summary>
        /// <param name="vault">MFiles库对象</param>
        /// <param name="aliasName">对象别名</param>
        /// <param name="properties">属性集合</param>
        /// <returns></returns>
        private static PropertyValues GetMailContentPropertyValues(Vault vault, string aliasName, IEnumerable<MfProperty> properties)
        {
            var oPropValues = new PropertyValues();

            var propValue1 = AecCloud.MFilesCore.MFPropertyUtils.Lookup(
               (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass,
               vault.ClassOperations.GetObjectClassIDByAlias(aliasName));
            oPropValues.Add(-1, propValue1);

            var propValue2 = AecCloud.MFilesCore.MFPropertyUtils.SingleFile(true);
            oPropValues.Add(-1, propValue2);

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
        private static PropertyValues GetMailContentPropertyValues(Vault vault, IEnumerable<MfProperty> properties)
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

        /// <summary>
        /// 生成临时文件名
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static string GetTempFilePath(string extension)
        {
            return String.Format("{0}{1}{2}", Path.GetTempPath(), Guid.NewGuid(), extension);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="list">文件列表</param>
        private static void DelFileList(IEnumerable<string> list)
        {
            try
            {
                //删除临时文件
                foreach (var path in list)
                {
                    File.Delete(path);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 给字符串添加html标记
        /// </summary>
        /// <param name="html">html正文内容</param>
        /// <returns></returns>
        private static string CombineHtmlString(string html)
        {
            if (html.StartsWith("<html>") || html.StartsWith("<!DOCTYPE html"))
            {
                return html;
            }
            return String.Format("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/></head><body>{0}</body></html>", html);
        }

        /// <summary>
        /// 多个地址转换成多个通讯录ID
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private static IEnumerable<int> ConvertAddrsToIds(Vault vault, IEnumerable<MailAddress> list)
        {
            var bookIds = new List<int>();
            foreach (var item in list)
            {
                //var value = String.Format("{0}<{1}>", item.DisplayName, item.Address);
                var id = LinkmanByMf.SearchLinkManId(vault, item.DisplayName, item.Address);
                bookIds.Add(id);
            }

            return bookIds.ToArray();
        }

        /// <summary>
        /// 多个地址转换成多个通讯录ID
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private static IEnumerable<int> ConvertAddrsToIds(Vault vault, IEnumerable<MailboxAddress> list)
        {
            var bookIds = new List<int>();
            foreach (var item in list)
            {
                var displayName = item.Name;
                if (String.IsNullOrEmpty(displayName))
                {
                    var pos = item.Address.IndexOf('@');
                    displayName = item.Address.Substring(0, pos);
                }

                var id = LinkmanByMf.SearchLinkManId(vault, displayName, item.Address);
                bookIds.Add(id);
            }

            return bookIds.ToArray();
        }

        /// <summary>
        /// 单个接收者转换成通讯录ID
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        private static int ConvertAddrToId(Vault vault, MailAddress addr)
        {
            return LinkmanByMf.SearchLinkManId(vault, addr.DisplayName, addr.Address);
        }

        /// <summary>
        /// 附件写入文件
        /// </summary>
        /// <param name="part"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static void MimePartToFile(MimeKit.MimePart part, string filePath)
        {
            using (var ms = new MemoryStream())
            {
                part.ContentObject.DecodeTo(ms);
                ms.Position = 0;
                var ws = File.Create(filePath);
                ms.WriteTo(ws);
                ws.Flush();
                ws.Close();
            }
        }

        /// <summary>
        /// 将html中img src解析为datauri格式
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string GetMailHtml(MimeMessage message)
        {
            var html = message.HtmlBody;
            if (!html.StartsWith("<html>") && !html.StartsWith("<!DOCTYPE html"))
            {
                html = String.Format("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/></head><body>{0}</body></html>", html);
            }

            var dic = new Dictionary<string, string>();
            var parts = message.BodyParts;
            foreach (var part in parts)
            {
                if (!String.IsNullOrEmpty(part.ContentId) && !part.IsAttachment)
                {
                    var dataUri = GetDataUri(part as MimePart);
                    dic.Add(part.ContentId, dataUri);
                }
            }

            if (dic.Count > 0)
            {
                var matches = Regex.Matches(html, @"<img[^>]*?src\s*=\s*([""']?[^'"">]+?['""])[^>]*?>",
                    RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
                foreach (Match match in matches)
                {
                    string src = match.Groups[1].Value;
                    src = src.Trim('\'', '\"');
                    src = src.Replace("cid:", "");
                    if (dic.ContainsKey(src))
                    {
                        html = html.Replace(match.Groups[1].Value, dic[src]);
                    } 
                }
            }
           
            return html;
        }

        /// <summary>
        /// 解析 “<img src=...>” 为Data Uri
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        private static string GetDataUri(MimePart attachment)
        {
            using (var memory = new MemoryStream())
            {
                attachment.ContentObject.DecodeTo(memory);
                var buffer = memory.GetBuffer();
                var length = (int)memory.Length;
                var base64 = Convert.ToBase64String(buffer, 0, length);

                return string.Format("data:{0};base64,{1}", attachment.ContentType.MimeType, base64);
            }
        }
    }
}
