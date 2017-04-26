using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using AecCloud.MFilesCore;
using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.Office.Interop.Word;
using Newtonsoft.Json;


namespace VaultApp
{
    public class ReportInput
    {
        public string EndDate = DateTime.Now.ToString("d");
        public int Principal = -1;
        public int Receiver = -1;

        public string StartDate = DateTime.Now.ToString("d");
    }

    public class ReportPrintData
    {
        public int Fileid = -1;
        public int Fileversion = -1;
        public int Objid = -1;
        public int Objtype = -1;
        public int Objversion = -1;
    }

    public class MfUser
    {
        public int Id;
        public string Name;
    }

    internal class SecureNotice
    {
      //  [MFState(Required = true)] public static MFIdentifier WfsSecureAdjust = MfilesAliasConfig.WfsSecureAdjust;

        [MFPropertyDef(Required = true)] public static MFIdentifier PropAdjustMeasure = MfilesAliasConfig.PropAdjustMeasure;

        [MFPropertyDef(Required = true)] public static MFIdentifier PropPrincipal = MfilesAliasConfig.PropPrincipal;
        [MFPropertyDef(Required = true)] public static MFIdentifier PropSecureReceiver = MfilesAliasConfig.PropSecureReceiver;
        [MFPropertyDef(Required = true)] public static MFIdentifier PropCheckDate = MfilesAliasConfig.PropCheckDate;
        [MFPropertyDef(Required = true)]
        public static MFIdentifier PropIssueCategory = MfilesAliasConfig.PropIssueCategory;
        [MFPropertyDef(Required = true)] public static MFIdentifier PropSecureAdjustDate = MfilesAliasConfig.PropSecureAdjustDate;

        [MFPropertyDef(Required = true)] public static MFIdentifier PropRectificationConclusion =
            MfilesAliasConfig.PropRectificationConclusion;
        [MFPropertyDef(Required = true)]
        public static MFIdentifier PropCountercheckDescription =MfilesAliasConfig.PropCountercheckDescription;
        
        [MFPropertyDef(Required = true)] public static MFIdentifier PropRectificationCount = MfilesAliasConfig.PropRectificationCount;
        [MFPropertyDef(Required = true)] public static MFIdentifier PropReviewDate = MfilesAliasConfig.PropReviewDate;

        [MFPropertyDef(Required = true)] public static MFIdentifier PropAdjustMan = MfilesAliasConfig.PropAdjustMan;
        [MFPropertyDef(Required = true)] public static MFIdentifier PropZhengGaiQiXin = MfilesAliasConfig.PropZhengGaiQiXin;
        [MFPropertyDef(Required = true)] public static MFIdentifier PropFuChaRen = MfilesAliasConfig.PropFuChaRen;
        [MFPropertyDef(Required = true)] public static MFIdentifier PropSecureIssues = MfilesAliasConfig.PropSecureIssues;

        [MFClass(Required = true)] public static MFIdentifier ClassSecureReport = MfilesAliasConfig.ClassSecureReport;

        [MFObjType(Required = true)]
        public static MFIdentifier OtSecureAdjustNotice = MfilesAliasConfig.OtSecureAdjustNotice;
        
        public static string GetSecureNoticeNew(EventHandlerEnvironment env) //程序划图片表格
        {
            var rpd = new ReportPrintData();
            Writelog(env.Vault.Name + env.Input + "GetSecureNotice : 查询条件");
            try
            {
                var input = JsonConvert.DeserializeObject<ReportInput>(env.Input);

                #region search issuenotice

                var conditions = new SearchConditions();
                {
                    var condition = new SearchCondition
                    {
                        ConditionType = MFConditionType.MFConditionTypeEqual,
                        Expression =
                        {
                            DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID
                        }
                    };
                    condition.TypedValue.SetValueToLookup(new Lookup { Item = OtSecureAdjustNotice.ID });
                //    Writelog("OtSecureAdjustNotice=" + OtSecureAdjustNotice.ID);
                    conditions.Add(-1, condition);
                }
                {
                    var sc = new SearchCondition
                    {
                        ConditionType = MFConditionType.MFConditionTypeNotEqual,
                        Expression = { DataStatusValueType = MFStatusType.MFStatusTypeDeleted }
                    };
                    sc.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, true);
                    conditions.Add(-1, sc);
                }
                {
                    var condition = new SearchCondition
                    {
                        ConditionType = MFConditionType.MFConditionTypeGreaterThanOrEqual,
                        Expression = { DataPropertyValuePropertyDef = PropCheckDate.ID }
                    };
                 //   Writelog("PropCheckDate=" + PropCheckDate.ID);
                    condition.TypedValue.SetValue(MFDataType.MFDatatypeDate, input.StartDate);
                    conditions.Add(-1, condition);
                }
                {
                    var condition = new SearchCondition
                    {
                        ConditionType = MFConditionType.MFConditionTypeLessThanOrEqual,
                        Expression = { DataPropertyValuePropertyDef = PropCheckDate.ID }
                    };
                    condition.TypedValue.SetValue(MFDataType.MFDatatypeDate, input.EndDate);
                    conditions.Add(-1, condition);
                }
                if (input.Principal != 0)
                {
                    var condition = new SearchCondition();
                  //  Writelog("PropPrincipal=" + PropPrincipal.ID);
                    condition.ConditionType = MFConditionType.MFConditionTypeEqual;
                    condition.Expression.DataPropertyValuePropertyDef = PropPrincipal.ID;
                    condition.TypedValue.SetValueToLookup(new Lookup { Item = input.Principal });
                    conditions.Add(-1, condition);
                }
                if (input.Receiver != 0)
                {
                    var condition = new SearchCondition();
                //    Writelog("PropSecureReceiver=" + PropSecureReceiver.ID);
                    condition.ConditionType = MFConditionType.MFConditionTypeEqual;
                    condition.Expression.DataPropertyValuePropertyDef = PropSecureReceiver.ID;
                    condition.TypedValue.SetValueToLookup(new Lookup { Item = input.Receiver });
                    conditions.Add(-1, condition);
                }
                ObjectVersions allwork = env.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(conditions,
                    MFSearchFlags.MFSearchFlagNone, false, 0, 0).GetAsObjectVersions();

                #endregion search issuenotice

              //  Writelog("allwork=" + allwork.Count);

                string templatefile = GetTemplateFile(env);

                try
                {
                    object oMissing = Missing.Value;
                    object objWhat = WdGoToItem.wdGoToPage;
                    object objWhich = WdGoToDirection.wdGoToLast;
                    var app = new Application();
                    object unknow = Type.Missing;
                    //  var msocoding = MsoEncoding.msoEncodingSimplifiedChineseGB18030;
                    Document doc = app.Documents.Open(templatefile,
                        ref unknow, false, ref unknow, ref unknow, ref unknow,
                        //        ref unknow, ref unknow, ref unknow, ref unknow, msocoding,
                        ref unknow, ref unknow, ref unknow, ref unknow, ref unknow,
                        ref unknow, ref unknow, ref unknow, ref unknow, ref unknow);

                    int issueindex = 0;//问题唯一序号，跨页接续
                    int tableindex = 1;
                    string temppath = Path.GetTempPath();
                    doc.Content.Copy();

                    Writelog(String.Format("vault:{0},conditions{1},results:{2}", env.Vault.Name, env.Input,
                        allwork.Count));
                    int rowpos = 1;//问题填写位置，每页刷新
                    bool newpage = false;
                    foreach (ObjectVersion objectVersion in allwork)
                    {
                       // Writelog("debug info aaaa");
                        if (newpage)
                        {
                            newpage = false;
                         //   Writelog("debug info bbbb");
                            object nothing = Missing.Value;
                            Paragraph para = doc.Content.Paragraphs.Add(ref nothing);
                            object pBreak = (int)WdBreakType.wdSectionBreakNextPage;
                            para.Range.InsertBreak(ref pBreak);
                         //   Writelog("debug info bbbb1111");

                            app.Selection.GoTo(ref objWhat, ref objWhich, ref unknow, ref unknow);
                        //    Writelog("debug info dddd");

                            app.Selection.Paste();
                         //   Writelog("debug info ffff");
                            tableindex ++;
                            rowpos = 1;
                        }

                        PropertyValues onepvs = env.Vault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer);
                        issueindex++;

                        string issuename = env.Vault.Name;
                      //  Writelog("debug info 6666");
                        doc.Tables[tableindex].Cell(4, 2).Range.Text = issuename;


                     //   Writelog("debug info 7777");
                        int rowindex = 6 + rowpos;

                        string secureissuename = onepvs.SearchForProperty(PropIssueCategory).GetValueAsLocalizedText();
                        doc.Tables[tableindex].Cell(rowindex, 1).Range.Text =
                            issueindex.ToString(CultureInfo.InvariantCulture);
                       
                        doc.Tables[tableindex].Cell(rowindex, 2).Range.Text = secureissuename;
                        doc.Tables[tableindex].Cell(rowindex, 3).Range.Text =
                            onepvs.SearchForProperty(PropSecureIssues.ID).GetValueAsLocalizedText();
                        doc.Tables[tableindex].Cell(rowindex, 4).Range.Text =
                            onepvs.SearchForProperty(PropAdjustMeasure.ID).GetValueAsLocalizedText();

                        doc.Tables[tableindex].Cell(rowindex, 5).Range.Text =
                            onepvs.SearchForProperty(PropPrincipal.ID)
                                .GetValueAsLocalizedText();
                        

                        doc.Tables[tableindex].Cell(rowindex, 6).Range.Text =
                            onepvs.SearchForProperty(PropSecureReceiver.ID)
                                .GetValueAsLocalizedText();
                        doc.Tables[tableindex].Cell(rowindex, 7).Range.Text =
                         onepvs.SearchForProperty(PropAdjustMan.ID)
                             .GetValueAsLocalizedText();


                        doc.Tables[tableindex].Cell(rowindex, 8).Range.Text =
                            onepvs.SearchForProperty(PropFuChaRen.ID)
                                .GetValueAsLocalizedText();
                        doc.Tables[tableindex].Cell(rowindex, 9).Range.Text =
                            onepvs.SearchForProperty(PropCountercheckDescription.ID)
                                .GetValueAsLocalizedText();
                 //       Writelog(string.Format("表 {0}， 行 {1}，序号 {2}, 行号 {3}",tableindex,rowindex,issueindex,rowpos));
                        if (rowpos++ >=10)
                        {
                            newpage = true;
                        }
                    }

                    int index = 0;
                    foreach (ObjectVersion objectVersion in allwork)
                    {
                        PropertyValues onepvs = env.Vault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer);

                        object nothing = Missing.Value;
                        Paragraph para = doc.Content.Paragraphs.Add(ref nothing);
                        object pBreak = (int)WdBreakType.wdSectionBreakNextPage;
                        para.Range.InsertBreak(ref pBreak);

                        app.Selection.GoTo(ref objWhat, ref objWhich, ref unknow, ref unknow);

                        app.Selection.PageSetup.Orientation = WdOrientation.wdOrientPortrait;
                        Range range = app.Selection.Range;
                        Table table = app.Selection.Tables.Add(range,7, 1, ref oMissing, ref oMissing);

                        table.Borders.OutsideLineStyle = WdLineStyle.wdLineStyleDouble;
                        table.Borders.InsideLineStyle = WdLineStyle.wdLineStyleSingle;
                        table.Cell(1, 1).Split(1, 2);
                        for (int i = 2; i <=3; i++)
                        {
                            table.Cell(i, 1).Split(1, 4);
                        }
                      
                        //Writelog("debug info 888111");
                        //app.Selection.TypeText("序号：" + page);
                        //Writelog("debug info 999000 v1");
                        table.Cell(1, 1).Range.Text = "序号：";
                        table.Cell(1, 2).Range.Text = (++index).ToString(CultureInfo.InvariantCulture);
                        //table.Cell(1, 3).Range.Text = "存在问题：";
                        //table.Cell(1, 4).Range.Text =
                        //    onepvs.SearchForProperty(PropSecureIssues.ID).GetValueAsLocalizedText();
                        //Writelog("debug info 1111 v1-" + tableindex);
                        //table.Cell(rowindex, 1).Range.Text = "检查负责人：";
                        table.Cell(2, 1).Range.Text = "检查日期：";
                        //table.Cell(2, 2).Range.Text =
                        //    onepvs.SearchForProperty(PropPrincipal.ID)
                        //        .GetValueAsLocalizedText();
                        table.Cell(2, 2).Range.Text =
                            onepvs.SearchForProperty(PropCheckDate.ID)
                                .GetValueAsLocalizedText();
                        //Writelog("debug info 222 v1-" + tableindex);
                        //table.Cell(rowindex, 1).Range.Text = "接收人  ：";
                        //table.Cell(rowindex++, 3).Range.Text = "整改人：";
                        //table.Cell(3, 2).Range.Text =
                        //    onepvs.SearchForProperty(PropSecureReceiver.ID)
                        //        .GetValueAsLocalizedText();
                        //table.Cell(3, 4).Range.Text =
                        //    onepvs.SearchForProperty(PropAdjustMan.ID)
                        //        .GetValueAsLocalizedText();
                     //   Writelog("debug info 333 v1-" + tableindex);
                        table.Cell(2, 3).Range.Text = "整改期限  ：";
                        table.Cell(3, 3).Range.Text = "整改次数：";
                        table.Cell(2, 4).Range.Text =
                            onepvs.SearchForProperty(PropZhengGaiQiXin.ID)
                                .GetValueAsLocalizedText();
                        table.Cell(3, 4).Range.Text =
                            onepvs.SearchForProperty(PropRectificationCount.ID)
                                .GetValueAsLocalizedText();
                        //Writelog("debug info 444 v1-" + tableindex);
                        //table.Cell(rowindex, 1).Range.Text = "复查人  ：";
                        table.Cell(3, 1).Range.Text = "复查日期：";
                        //table.Cell(5, 2).Range.Text =
                        //    onepvs.SearchForProperty(PropFuChaRen.ID)
                        //        .GetValueAsLocalizedText();
                        table.Cell(3, 2).Range.Text =
                            onepvs.SearchForProperty(PropReviewDate.ID)
                                .GetValueAsLocalizedText();
                    //    Writelog("debug info 555 v1-" + tableindex);
                       // int rowindex = 2;
                        table.Cell(4, 1).Range.Text = "整改前照片：";
                        table.Cell(6, 1).Range.Text = "复查照片：";
                        ObjectFiles files = env.Vault.ObjectFileOperations.GetFiles(objectVersion.ObjVer);
                        int picrow = 5;
                      //  Writelog("before 000000000000");
                        foreach (ObjectFile objectFile in files)
                        {
                            string apicture = temppath + objectFile.GetNameForFileSystem();
                            env.Vault.ObjectFileOperations.DownloadFile(objectFile.ID,
                                objectFile.Version, apicture);
                            object linkToFile = false;
                            object saveWithDocument = true;
                            object anchor = table.Cell(picrow, 1).Range;
                            InlineShape insh = doc.InlineShapes.AddPicture(apicture, ref linkToFile,
                                ref saveWithDocument,
                                ref anchor);
                            insh.Height = 259;
                            insh.Width = 416;
                            picrow += 2;
                            if (picrow > 7) break;
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
                var pv = new PropertyValue { PropertyDef = 0 };
                pv.Value.SetValue(MFDataType.MFDatatypeText, "securenoticereport");
                pvs.Add(-1, pv);
                pv.PropertyDef = 100;
                pv.Value.SetValueToLookup(new Lookup { Item = ClassSecureReport });
                pvs.Add(-1, pv);
                var file = new SourceObjectFile { Title = "report", SourceFilePath = templatefile, Extension = "docx" };

                try
                {
                    ObjectVersionAndProperties t = env.Vault.ObjectOperations.CreateNewSFDObject(0, pvs, file, true);
                    ObjectFiles f = env.Vault.ObjectFileOperations.GetFiles(t.ObjVer);

                    rpd.Objid = t.ObjVer.ID;
                    rpd.Objtype = t.ObjVer.Type;
                    rpd.Objversion = t.ObjVer.Version;
                    rpd.Fileid = f[1].FileVer.ID;
                    rpd.Fileversion = f[1].FileVer.Version;
                }
                catch (Exception ex)
                {
                    Writelog("getsecurenotice - create object :" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Writelog(env.Input + "GetSecureNotice error:" + ex.Message);
            }
            var ret=JsonConvert.SerializeObject(rpd, Formatting.None);
            Writelog( "GetSecureNotice ok return:" + ret);
            return ret;
        }
     
        public static string GetFilterPrincipal(EventHandlerEnvironment env)
        {
            var plist = new List<MfUser>();
            try
            {
                var conditions = new SearchConditions();
                {
                    var condition = new SearchCondition
                    {
                        ConditionType = MFConditionType.MFConditionTypeEqual,
                        Expression =
                        {
                            DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID
                        }
                    };
                    condition.TypedValue.SetValueToLookup(new Lookup { Item = OtSecureAdjustNotice.ID });
                    conditions.Add(-1, condition);
                }
                ObjectVersions allwork = env.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(conditions,
                    MFSearchFlags.MFSearchFlagNone, false, 0, 0).GetAsObjectVersions();
                foreach (ObjectVersion objectVersion in allwork)
                {
                    PropertyValue pv = env.Vault.ObjectPropertyOperations.GetProperty(objectVersion.ObjVer,
                        PropPrincipal.ID);
                    int userid = pv.Value.GetLookupID();
                    bool havethisuser = plist.Any(mfUser => mfUser.Id == userid);
                    if (!havethisuser)
                    {
                        //  var name = env.Vault.UserOperations.GetUserAccount(userid).LoginName;
                        plist.Add(new MfUser
                        {
                            Id = userid,
                            Name = env.Vault.UserOperations.GetLoginAccountOfUser(userid).FullName
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Writelog(String.Format("VaultExtensionMethodName=GetFilterPrincipal error :Vault={0},{1}",
                    env.Vault.Name, ex.Message));
            }
            return JsonConvert.SerializeObject(plist, Formatting.None);
        }

        public static string GetFilterReceiver(EventHandlerEnvironment env)
        {
            var conditions = new SearchConditions();
            {
                var condition = new SearchCondition
                {
                    ConditionType = MFConditionType.MFConditionTypeEqual,
                    Expression =
                    {
                        DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID
                    }
                };
                condition.TypedValue.SetValueToLookup(new Lookup { Item = OtSecureAdjustNotice.ID });
                conditions.Add(-1, condition);
            }

            ObjectVersions allwork = env.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(conditions,
                MFSearchFlags.MFSearchFlagNone, false, 0, 0).GetAsObjectVersions();
            var plist = new List<MfUser>();
            try
            {
                foreach (ObjectVersion objectVersion in allwork)
                {
                    PropertyValue pv = env.Vault.ObjectPropertyOperations.GetProperty(objectVersion.ObjVer,
                        PropSecureReceiver.ID);
                    Lookups ulist = pv.Value.GetValueAsLookups();
                    foreach (Lookup lookup in ulist)
                    {
                        Lookup lookup1 = lookup;
                        bool havethisuser = plist.Any(mfUser => mfUser.Id == lookup1.Item);
                        if (!havethisuser)
                        {
                            //   var name = env.Vault.UserOperations.GetUserAccount(lookup.Item).LoginName;
                            plist.Add(new MfUser
                            {
                                Id = lookup.Item,
                                Name = env.Vault.UserOperations.GetLoginAccountOfUser(lookup.Item).FullName
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Writelog(String.Format("GetFilterReceiver error: {0}", ex.Message));
            }
            return JsonConvert.SerializeObject(plist, Formatting.None);
        }

        private static string GetTemplateFile(EventHandlerEnvironment env)
        {
            var conditions = new SearchConditions();
            {
                var condition = new SearchCondition
                {
                    ConditionType = MFConditionType.MFConditionTypeEqual,
                    Expression = {DataPropertyValuePropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass}
                };
                condition.TypedValue.SetValueToLookup(new Lookup {Item = ClassSecureReport.ID});
                conditions.Add(-1, condition);
            }
            {
                var condition = new SearchCondition
                {
                    ConditionType = MFConditionType.MFConditionTypeEqual,
                    Expression =
                    {
                        DataPropertyValuePropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefIsTemplate
                    }
                };
                //  var isornot = true;
                condition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, true);
                conditions.Add(-1, condition);
            }
            ObjectVersions allwork =
                env.Vault.ObjectSearchOperations.SearchForObjectsByConditions(conditions, MFSearchFlags.MFSearchFlagNone,
                    false).GetAsObjectVersions();
            if (allwork.Count < 1)
            {
                Writelog(String.Format("can't find the template for class [{0}]", ClassSecureReport.Alias));
                return null;
            }
            string tmpfile = Path.GetTempFileName();
            string file = tmpfile + ".docx";
            foreach (ObjectVersion objectVersion in allwork)
            {
                foreach (ObjectFile objectFile in objectVersion.Files)
                {
                    env.Vault.ObjectFileOperations.DownloadFile(objectFile.ID, objectFile.Version, file);
                    File.Copy(file, tmpfile, true);
                    break;
                }

                break;
            }

            return tmpfile;
        }

        private static void Writelog(string logtext)
        {
            try
            {
                using (
                    StreamWriter sw =
                        File.AppendText(Path.Combine(Path.GetTempPath(),
                            DateTime.Now.Date.ToString("yyyy-MM-dd") + "vaultapplog.txt")))
                {
                    sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}