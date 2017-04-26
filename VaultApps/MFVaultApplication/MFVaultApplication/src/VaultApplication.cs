using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF;
using MFiles.VAF.Common;
using MFilesAPI;
using MFVaultApplication.src;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using Newtonsoft.Json;

namespace MFVaultApplication
{
   
    /// <summary>
    /// Simple vault application to demonstrate VAF.
    /// </summary>
    public class VaultApplication : VaultApplicationBase
    {
        [MFState(Required = true)]
        public MFIdentifier WfsSecureAdjust = "WfsSecureAdjust";

        [MFPropertyDef(Required = true)]
        public MFIdentifier PropSecureAdjustDate = "PropSecureAdjustDate";
        [MFPropertyDef(Required = true)]
        public MFIdentifier PropPrincipal = "PropPrincipal";
        [MFPropertyDef(Required = true)]
        public MFIdentifier PropSecureReceiver = "PropSecureReceiver";
        [MFPropertyDef(Required = true)]
        public MFIdentifier PropCheckDate = "PropCheckDate";
        [MFPropertyDef(Required = true)]
        public MFIdentifier PropAdjustMeasure = "PropAdjustMeasure";
        [MFPropertyDef(Required = true)]
        public MFIdentifier PropAdjustMan = "PropAdjustMan";
        [MFPropertyDef(Required = true)]
        public MFIdentifier PropZhengGaiQiXin = "PropZhengGaiQiXin";
        [MFPropertyDef(Required = true)]
        public MFIdentifier PropFuChaRen = "PropFuChaRen";
        [MFPropertyDef(Required = true)]
        public MFIdentifier PropSecureIssues = "PropSecureIssues";
        [MFPropertyDef(Required = true)]
        public MFIdentifier PropBeforePictures = "PropBeforePictures";
        [MFPropertyDef(Required = true)]
        public MFIdentifier PropAfterPictures = "PropAfterPictures";

        [MFClass(Required = true)]
        public MFIdentifier ClassSecureNotice = "ClassSecureNotice";
        [MFClass(Required = true)]
        public MFIdentifier ClassSecureReport = "ClassSecureReport";

        [MFObjType(Required = true)]
        public MFIdentifier OtSecureIssue = "OtSecureIssue";

        #region biminfo
        //关联较多,暂不合并
        #endregion biminfo

        #region Property Attributes
        //[PropertyValueValidation("PropertyAlias")]
        //private bool PropertyValueValidation(PropertyEnvironment env, out string message)
        //{
        //    // Message to the user.
        //    message = "This property value will never be valid.";
        //    Log(MethodBase.GetCurrentMethod().Name, env.ObjVer);

        //    return true;
        //}

        //[PropertyAutomaticNumbering("PropertyAlias")]
        //private TypedValue PropertyAutomaticNumbering(PropertyEnvironment env)
        //{
        //    Log(MethodBase.GetCurrentMethod().Name, env.ObjVer);
        //    TypedValue ret = new TypedValue();
        //    ret.SetValue(MFDataType.MFDatatypeText, "PropertyAutomaticNumbering");
        //    return ret;
        //}

        //[PropertyCustomValue("PropAcceptDate")]
        //private TypedValue PropAcceptDatePropertyCustomValue(PropertyEnvironment env)
        //{
        //    Log(MethodBase.GetCurrentMethod().Name, env.ObjVer);
        //    TypedValue ret = new TypedValue();
        //    ret.SetValue(MFDataType.MFDatatypeDate,DateTime.Now.ToString("d"));
        //    return ret;
        //}

        #endregion Property Attributes
        #region State Attributes

        [StateAction("WfsSecureAdjust")]
        private void WfsSecureAdjustStateAction(StateEnvironment ehe)
        {
            Writelog(string.Format("{1},StateEnvironment.ObjVer.Version={0},StateEnvironment.ObjVer.Type={2},StateEnvironment.ObjVer.ID={3}",
         ehe.ObjVer.Version, MethodBase.GetCurrentMethod().Name, ehe.ObjVer.Type, ehe.ObjVer.ID));


            try
            {
                var pv = ehe.PropertyValues.SearchForProperty(PropSecureAdjustDate.ID);
                pv.Value.SetValue(MFDataType.MFDatatypeDate, DateTime.Now.ToString("d"));
                ehe.Vault.ObjectPropertyOperations.SetProperty(ehe.ObjVer, pv);
            }
            catch (Exception ex)
            {
                Writelog(PropSecureAdjustDate.Alias + PropSecureAdjustDate.ID + ex.Message);
            }
        }

        //[StatePreConditions("WfsAppraised")]
        //private bool WfsAppraisedStatePreConditions(StateEnvironment ehe, out string message)
        //{
        //    message = "ok";
        //    try
        //    {
        //        var currenuser = ehe.CurrentUserID;
        //        var alluser = ehe.Vault.UserOperations.GetUserAccounts();
        //        var isengineer = false;
        //        foreach (UserAccount engineer in alluser)
        //        {
        //            if (engineer.ID == currenuser)
        //            {
        //                isengineer = true;
        //                break;
        //            }
        //        }
        //        var satisfaction = ehe.PropertyValues.SearchForProperty(PropSatisfaction.ID);
        //        Writelog("WfsAppraisedStatePreConditions,userid=" + currenuser + ehe.CurrentUserSessionInfo.AccountName + satisfaction.GetValueAsLocalizedText());
        //        if (isengineer)
        //        {
        //            if (satisfaction.Value.IsNULL() || satisfaction.Value.IsEmpty())
        //            {
        //                message = string.Format("属性\"{0}\"不能是空的，请选择输入，谢谢！",
        //                    ehe.Vault.PropertyDefOperations.GetPropertyDef(PropSatisfaction.ID).Name);
        //                return false;
        //            }
        //            else
        //            {
        //                Writelog("WfsAppraisedStatePreConditions,have value? userid=" + currenuser + ehe.CurrentUserSessionInfo.AccountName + satisfaction.GetValueAsLocalizedText());
        //            }
        //        }
        //        else
        //        {
        //            satisfaction.Value.SetValueToLookup(new Lookup { Item = 1 });
        //            ehe.Vault.ObjectPropertyOperations.SetProperty(ehe.ObjVer, satisfaction);
        //            Writelog("自动状态转换userid=" + currenuser + ehe.CurrentUserSessionInfo.AccountName);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Writelog(WfsAppraised.Alias + WfsAppraised.ID + PropTimeAppraised.Alias + PropTimeAppraised.ID + ex.Message);
        //    }
        //    return true;
        //}
        //[StatePostConditions("StateAction1")]
        //private bool StatePostConditions(StateEnvironment ehe, out string message)
        //{
        //    message =
        //        string.Format(
        //            "{1},StateEnvironment.ObjVer.Version={0},StateEnvironment.ObjVer.Type={2},StateEnvironment.ObjVer.ID={3}",
        //            ehe.ObjVer.Version, MethodBase.GetCurrentMethod().Name, ehe.ObjVer.Type, ehe.ObjVer.ID);
        //    Writelog(message);
        //    return true;
        //}

        #endregion State Attributes
        public static void Writelog(string logtext)
        {
            try
            {
                using (
                    var sw =
                        System.IO.File.AppendText(System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                            "vaultapplog.txt")))
                {
                    sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
                    sw.Close();
                }

            }
            catch (Exception) { }
        }
        public class MFUser
        {
            public int Id;
            public string Name;
        }
        /// <summary>
        /// A vault extension method, that will be installed to the vault with the application.
        /// The vault extension method can be called through the API.
        /// </summary>
        /// <param name="env">The event handler environment for the method.</param>
        /// <returns>The output string to the caller.</returns>
        [VaultExtensionMethod("GetFilterPrincipal", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        private string GetFilterPrincipal(EventHandlerEnvironment env)
        {
            var plist = new List<MFUser>();
            try
            {
                var conditions = new SearchConditions();
                {
                    var condition = new SearchCondition();
                    condition.ConditionType = MFConditionType.MFConditionTypeEqual;
                    condition.Expression.DataPropertyValuePropertyDef =
                        (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                    condition.TypedValue.SetValueToLookup(new Lookup {Item = ClassSecureNotice.ID});
                    conditions.Add(-1, condition);
                }

                var allwork = env.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(conditions,
                    MFSearchFlags.MFSearchFlagNone, false, 0, 0).GetAsObjectVersions();

                foreach (ObjectVersion objectVersion in allwork)
                {
                    var pv = env.Vault.ObjectPropertyOperations.GetProperty(objectVersion.ObjVer, PropPrincipal.ID);
                    var userid = pv.Value.GetLookupID();
                    var havethisuser = false;
                    foreach (MFUser mfUser in plist)
                    {
                        if (mfUser.Id == userid)
                        {
                            havethisuser = true;
                            break;
                        }
                    }
                    if (!havethisuser)
                    {
                        var name = env.Vault.UserOperations.GetUserAccount(userid).LoginName;
                        plist.Add(new MFUser {Id = userid, Name = name});
                    }

                }
            }
            catch (Exception ex)
            {
                Writelog(string.Format("VaultExtensionMethodName={2} error :Vault={0},Input={1},Type={3},UserAccount={4},{5}", env.Vault.Name, env.Input, 
                    env.VaultExtensionMethodName,env.Type,env.UserAccount.LoginName,ex.Message));
            }
            return JsonConvert.SerializeObject(plist, Formatting.None);
        }
        [VaultExtensionMethod("GetFilterReceiver", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        private string GetFilterReceiver(EventHandlerEnvironment env)
        {
            var conditions = new SearchConditions();
            {
                var condition = new SearchCondition();
                condition.ConditionType = MFConditionType.MFConditionTypeEqual;
                condition.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                condition.TypedValue.SetValueToLookup(new Lookup { Item = ClassSecureNotice.ID });
                conditions.Add(-1, condition);
            }

            var allwork = env.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(conditions,
                          MFSearchFlags.MFSearchFlagNone, false, 0, 0).GetAsObjectVersions();
            var plist = new List<MFUser>();
            try
            {
                foreach (ObjectVersion objectVersion in allwork)
                {
                    var pv = env.Vault.ObjectPropertyOperations.GetProperty(objectVersion.ObjVer, PropSecureReceiver.ID);
                    var ulist = pv.Value.GetValueAsLookups();
                    foreach (Lookup lookup in ulist)
                    {
                        var havethisuser = false;
                        foreach (MFUser mfUser in plist)
                        {
                            if (mfUser.Id == lookup.Item)
                            {
                                havethisuser = true;
                                break;
                            }
                        }
                        if (!havethisuser)
                        {
                            var name = env.Vault.UserOperations.GetUserAccount(lookup.Item).LoginName;
                            plist.Add(new MFUser { Id = lookup.Item, Name = name });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Writelog(string.Format("GetFilterReceiver error: {0}", ex.Message));
            }
            return JsonConvert.SerializeObject(plist, Formatting.None);
        }
        public class ReportInput
        {
            public int principal = -1;
            public int receiver = -1;

            public string startDate = DateTime.Now.ToString("d");
            public string endDate = DateTime.Now.ToString("d");

        }
       
        public class ReportPrintData
        {
            public int objid = -1;
            public int objtype = -1;
            public int objversion = -1;
            public int fileid = -1;
            public int fileversion = -1;
        }
       
        [VaultExtensionMethod("GetSecureNotice", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        private string GetSecureNotice(EventHandlerEnvironment env)
        {
            ReportInput input = new ReportInput();
            Writelog(env.Input + "GetSecureNotice :");
            try
            {
                input = JsonConvert.DeserializeObject<ReportInput>(env.Input);
            }
            catch (Exception ex)
            {
                Writelog(env.Input + "GetSecureNotice error:" + ex.Message);
                return JsonConvert.SerializeObject("-1", Formatting.None);
            }
            #region search issuenotice
            var conditions = new SearchConditions();
            {
                var condition = new SearchCondition();
                condition.ConditionType = MFConditionType.MFConditionTypeEqual;
                condition.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                condition.TypedValue.SetValueToLookup(new Lookup { Item = ClassSecureNotice.ID });
                conditions.Add(-1, condition);
            }
            {
                var condition = new SearchCondition();
                condition.ConditionType = MFConditionType.MFConditionTypeGreaterThanOrEqual;
                condition.Expression.DataPropertyValuePropertyDef = PropCheckDate.ID;
                condition.TypedValue.SetValue(MFDataType.MFDatatypeDate, input.startDate);
                conditions.Add(-1, condition);
            }
            {
                var condition = new SearchCondition();
                condition.ConditionType = MFConditionType.MFConditionTypeLessThanOrEqual;
                condition.Expression.DataPropertyValuePropertyDef = PropCheckDate.ID;
                condition.TypedValue.SetValue(MFDataType.MFDatatypeDate, input.endDate);
                conditions.Add(-1, condition);
            }
            if (input.principal != 0)
            {
                var condition = new SearchCondition();

                condition.ConditionType = MFConditionType.MFConditionTypeEqual;
                condition.Expression.DataPropertyValuePropertyDef = PropPrincipal.ID;
                condition.TypedValue.SetValueToLookup(new Lookup { Item = input.principal });
                conditions.Add(-1, condition);

            }
            if (input.receiver != 0)
            {
                var condition = new SearchCondition();

                condition.ConditionType = MFConditionType.MFConditionTypeEqual;
                condition.Expression.DataPropertyValuePropertyDef = PropSecureReceiver.ID;
                condition.TypedValue.SetValueToLookup(new Lookup { Item = input.receiver });
                conditions.Add(-1, condition);

            }
            var allwork = env.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(conditions,
                         MFSearchFlags.MFSearchFlagNone, false, 0, 0).GetAsObjectVersions();
            #endregion search issuenotice

            var templatefile = GetTemplateFile(env);
            if (string.IsNullOrEmpty(templatefile))
            return JsonConvert.SerializeObject("-2", Formatting.None);
            try
            {
            var app = new Microsoft.Office.Interop.Word.Application();
            var unknow = Type.Missing;
            //  var msocoding = MsoEncoding.msoEncodingSimplifiedChineseGB18030;
            var doc = app.Documents.Open(templatefile,
                                         ref unknow, false, ref unknow, ref unknow, ref unknow,
                //        ref unknow, ref unknow, ref unknow, ref unknow, msocoding,
                 ref unknow, ref unknow, ref unknow, ref unknow, ref unknow,
                                         ref unknow, ref unknow, ref unknow, ref unknow, ref unknow);
            //var num = 0;
            //foreach (Table table in doc.Tables)
            //{
            //    Writelog(string.Format("table[{0}],Columns[{1}],,Rows[{2}]", num, table.Columns.Count, table.Rows.Count));
            //    for (int i = 0; i < table.Columns.Count; i++)
            //    {
            //        for (int j = 0; j < table.Rows.Count; j++)
            //        {
            //            try
            //            {
            //                var text = table.Cell(j, i).Range.Text;

            //                Writelog(string.Format("table[{0}],Columns[{1}],Rows[{2}],text[{3}]", num, i, j, text));
            //            }
            //            catch (Exception ex)
            //            {
            //                Writelog(string.Format("table[{0}],Columns[{1}],Rows[{2}],message[{3}]", num, i, j, ex.Message));
            //            }
            //        }
            //    }
            //    num++;
            //}
            //Writelog("num=" + num);


         var page = 0;
         var tableindex = 0;
            var temppath = System.IO.Path.GetTempPath();
         doc.Tables[1].Range.Copy();
            var issuename = string.Empty;
            var secureissuename = string.Empty;
           
                foreach (ObjectVersion objectVersion in allwork)
                {
                    if (page > 0)
                    {
                        doc.Content.InsertParagraphAfter();
                        doc.Content.InsertAfter(" ");
                        object WdLine = Microsoft.Office.Interop.Word.WdUnits.wdLine;//换一行;
                        var movedown = app.Selection.MoveDown(ref WdLine, 21);//移动焦点
                        Writelog(string.Format(" before paste table num=[{0}],count=[{1}],down==[{2}]", doc.Tables.Count.ToString(),  movedown));
                        app.Selection.Paste();
                    }

                    var onepvs = env.Vault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer);
                    page++;
                    tableindex++;
                    issuename=onepvs.SearchForProperty(0).GetValueAsLocalizedText();

                    doc.Tables[tableindex].Cell(4, 2).Range.Text = issuename;
                    doc.Tables[tableindex].Cell(4, 4).Range.Text = DateTime.Now.ToShortDateString();
                    doc.Tables[tableindex].Cell(4, 6).Range.Text = page.ToString();
                    doc.Tables[tableindex].Cell(0, 1).Range.Text = "整改期限：" + onepvs.SearchForProperty(PropZhengGaiQiXin.ID).GetValueAsLocalizedText(); 
                    doc.Tables[tableindex].Cell(0, 2).Range.Text = "接收人：" + onepvs.SearchForProperty(PropSecureReceiver.ID).GetValueAsLocalizedText(); 
                    doc.Tables[tableindex].Cell(0, 3).Range.Text = "检查负责人：" + onepvs.SearchForProperty(PropPrincipal.ID).GetValueAsLocalizedText(); 
                    doc.Tables[tableindex].Cell(0, 4).Range.Text = "复查人：" + onepvs.SearchForProperty(PropFuChaRen.ID).GetValueAsLocalizedText();

                  var serial = 1;
                    var lus = onepvs.SearchForProperty(PropSecureIssues.ID).Value.GetValueAsLookups();
                    foreach (Lookup lookup in lus)
                    {
                        var rowindex = 6 + serial;
                        var objid = new ObjID();
                        objid.SetIDs(OtSecureIssue.ID,lookup.Item);
                        var issuepvs = env.Vault.ObjectOperations.GetLatestObjectVersionAndProperties(objid,true).Properties;
                        secureissuename=issuepvs.SearchForProperty(0).GetValueAsLocalizedText();
                        doc.Tables[tableindex].Cell(rowindex, 1).Range.Text = serial.ToString();
                        doc.Tables[tableindex].Cell(rowindex, 2).Range.Text = secureissuename;
                        doc.Tables[tableindex].Cell(rowindex, 3).Range.Text = issuepvs.SearchForProperty(PropAdjustMeasure.ID).GetValueAsLocalizedText();
                        doc.Tables[tableindex].Cell(rowindex, 4).Range.Text = issuepvs.SearchForProperty(PropAdjustMan.ID).GetValueAsLocalizedText();
                        doc.Tables[tableindex].Cell(rowindex, 5).Range.Text = issuepvs.SearchForProperty(PropSecureAdjustDate.ID).GetValueAsLocalizedText();

                        var beforepictures = issuepvs.SearchForProperty(PropBeforePictures.ID).Value.GetValueAsLookups();
                        foreach (Lookup beforepicture in beforepictures)
                        {
                            var objver =
                                env.Vault.ObjectOperations.GetLatestObjVer(
                                    new ObjID {ID = beforepicture.Item, Type = beforepicture.ObjectType}, true);
                            var files = env.Vault.ObjectFileOperations.GetFiles(objver);
                            foreach (ObjectFile objectFile in files)
                            {
                                var apicture = temppath + objectFile.GetNameForFileSystem();
                                 env.Vault.ObjectFileOperations.DownloadFile(objectFile.ID,
                                    objectFile.Version, apicture);
                                 object unite = Microsoft.Office.Interop.Word.WdUnits.wdStory;
                                object nothing = System.Reflection.Missing.Value;
                            //    doc.Content.InsertParagraphAfter();
                                doc.Content.InsertAfter(string.Format("{3}安全隐患整改名称：{0}，{3}安全问题名称：{1}，{3}整改前照片名称：{2}{3}",issuename,secureissuename,beforepicture.DisplayValue,Environment.NewLine));
                                app.Selection.EndKey(ref unite, ref nothing);//将光标移至文末

                                object LinkToFile = false;
                                object SaveWithDocument = true;
                                object Anchor = app.Selection.Range;

                                doc.InlineShapes.AddPicture(apicture, ref LinkToFile, ref SaveWithDocument, ref Anchor);
                            }
                        }

                        var afterpictures = issuepvs.SearchForProperty(PropAfterPictures.ID).Value.GetValueAsLookups();
                        foreach (Lookup after in afterpictures)
                        {
                            var objver =
                                env.Vault.ObjectOperations.GetLatestObjVer(
                                    new ObjID { ID = after.Item, Type = after.ObjectType }, true);
                            var files = env.Vault.ObjectFileOperations.GetFiles(objver);
                            foreach (ObjectFile objectFile in files)
                            {
                                var apicture = temppath + objectFile.GetNameForFileSystem();
                                env.Vault.ObjectFileOperations.DownloadFile(objectFile.ID,
                                   objectFile.Version, apicture);
                                object unite = Microsoft.Office.Interop.Word.WdUnits.wdStory;
                                object nothing = System.Reflection.Missing.Value;
                          //      doc.Content.InsertParagraphAfter();
                                doc.Content.InsertAfter(string.Format("{3}安全隐患整改名称：{0}，{3}安全问题名称：{1}，{3}复查现场照片名称：{2}{3}", issuename, secureissuename, after.DisplayValue,Environment.NewLine));
                                app.Selection.EndKey(ref unite, ref nothing);//将光标移至文末

                                object LinkToFile = false;
                                object SaveWithDocument = true;
                                object Anchor = app.Selection.Range;

                                doc.InlineShapes.AddPicture(apicture, ref LinkToFile, ref SaveWithDocument, ref Anchor);
                            }
                        }

                        serial++;
                        if (serial > 12)
                        {
                            doc.Content.InsertParagraphAfter();
                            doc.Content.InsertAfter(" ");
                            object WdLine = Microsoft.Office.Interop.Word.WdUnits.wdLine;//换一行;
                            var movedown = app.Selection.MoveDown(ref WdLine, 21);//移动焦点
                            Writelog(string.Format(" before paste table num=[{0}],count=[{1}],down==[{2}]", doc.Tables.Count.ToString(), movedown));
                            app.Selection.Paste();
                            page++;
                            tableindex++;
                            doc.Tables[tableindex].Cell(4, 2).Range.Text = "project name";
                            doc.Tables[tableindex].Cell(4, 4).Range.Text = DateTime.Now.ToShortDateString();
                            doc.Tables[tableindex].Cell(4, 6).Range.Text = page.ToString();
                            doc.Tables[tableindex].Cell(0, 1).Range.Text = "整改期限：" + onepvs.SearchForProperty(PropZhengGaiQiXin.ID).GetValueAsLocalizedText();
                            doc.Tables[tableindex].Cell(0, 2).Range.Text = "接收人：" + onepvs.SearchForProperty(PropSecureReceiver.ID).GetValueAsLocalizedText();
                            doc.Tables[tableindex].Cell(0, 3).Range.Text = "检查负责人：" + onepvs.SearchForProperty(PropPrincipal.ID).GetValueAsLocalizedText();
                            doc.Tables[tableindex].Cell(0, 4).Range.Text = "复查人：" + onepvs.SearchForProperty(PropFuChaRen.ID).GetValueAsLocalizedText();
                        }
                    }
                }

                doc.Close();
                app.Quit();
            }
            catch (Exception ex)
            {
                Writelog(ex.Message);
            }
        

            var pvs = new PropertyValues();
            var pv = new PropertyValue();
            pv.PropertyDef = 0;
            pv.Value.SetValue(MFDataType.MFDatatypeText, "securenoticereport" );
            pvs.Add(-1, pv);
            pv.PropertyDef = 100;
            pv.Value.SetValueToLookup(new Lookup { Item = ClassSecureReport });
            pvs.Add(-1, pv);
            var file = new SourceObjectFile();
            file.Title = "report";
            file.SourceFilePath = templatefile;
            file.Extension = "docx";
       
            var t = env.Vault.ObjectOperations.CreateNewSFDObject(0, pvs, file, true);
            var f = env.Vault.ObjectFileOperations.GetFiles(t.ObjVer);
            var rpd = new ReportPrintData();
            rpd.objid = t.ObjVer.ID;
            rpd.objtype = t.ObjVer.Type;
            rpd.objversion = t.ObjVer.Version;
            rpd.fileid = f[1].FileVer.ID;
            rpd.fileversion = f[1].FileVer.Version;

            return JsonConvert.SerializeObject(rpd, Formatting.None);
        }

        private string GetTemplateFile(EventHandlerEnvironment env)
        {
            var conditions = new SearchConditions();
            {
                var condition = new SearchCondition();
                condition.ConditionType = MFConditionType.MFConditionTypeEqual;
                condition.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
                condition.TypedValue.SetValueToLookup(new Lookup { Item = ClassSecureReport.ID });
                conditions.Add(-1, condition);
            }
            {
                var condition = new SearchCondition();
                condition.ConditionType = MFConditionType.MFConditionTypeEqual;
                condition.Expression.DataPropertyValuePropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefIsTemplate;
                //  var isornot = true;
                condition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, true);
                conditions.Add(-1, condition);
            }
            var allwork = env.Vault.ObjectSearchOperations.SearchForObjectsByConditions(conditions, MFSearchFlags.MFSearchFlagNone, false).GetAsObjectVersions();
            if (allwork.Count < 1)
            {
                Writelog(string.Format("can't find the template for class [{0}]", ClassSecureReport.Alias));
                return null;
            }
            var tmpfile = System.IO.Path.GetTempFileName();
            var file =  tmpfile+ ".docx";
            foreach (ObjectVersion objectVersion in allwork)
            {
                foreach (ObjectFile objectFile in objectVersion.Files)
                {
                    env.Vault.ObjectFileOperations.DownloadFile(objectFile.ID, objectFile.Version, file);
                    System.IO.File.Copy(file,tmpfile,true);
                    break;
                }

                break;
            }

            return tmpfile;
        }

        [VaultExtensionMethod("getWorkFlowStates", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        public string getWorkFlowStates(EventHandlerEnvironment env)
        {
            return MfWorkflow.GetWorkflow(env);
        }

    }
}