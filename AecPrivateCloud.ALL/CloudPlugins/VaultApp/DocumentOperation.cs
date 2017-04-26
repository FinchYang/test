using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AecCloud.MFilesCore;
using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.Office.Interop.Word;

namespace VaultApp
{
    public class SignatureResource
    {
        public SignatureResource()
        {
            IsFile = false;
            ContentText = string.Empty;
        }
        public string ContentText { set; get; }
        public bool IsFile { set; get; }
    }
    class DocumentOperation
    {
        public Vault vault { set; get; }
        public ObjVer objver { set; get; }
        public Application app { set; get; }
        public Table table { set; get; }
        public Document doc { set; get; }
        public string filepath { set; get; }
        public string SignaturePicturePath { set; get; }
        public PropertyValues pvs { set; get; }
        public ProjectInMfiles Project = new ProjectInMfiles();
        public DocumentOperation(StateEnvironment stateEnvironment)
        {
            vault = stateEnvironment.Vault;
            objver = stateEnvironment.ObjVer;

            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                SignaturePicturePath = string.Empty;
                var spp = config.AppSettings.Settings["SignaturePicturePath"];
                if (spp != null) SignaturePicturePath = spp.Value;
             
                var files = vault.ObjectFileOperations.GetFiles(objver);
                filepath = Path.GetTempFileName();

                foreach (ObjectFile objectFile in files)
                {
                    vault.ObjectFileOperations.DownloadFile(objectFile.ID, objectFile.Version, filepath);
                    break;
                }
                var sc = new SearchCondition();
                sc.ConditionType = MFConditionType.MFConditionTypeEqual;
                sc.Expression.DataStatusValueType = MFStatusType.MFStatusTypeObjectTypeID;
                sc.TypedValue.SetValueToLookup(new Lookup
                {
                    Item =
                        vault.GetMetadataStructureItemIDByAlias(
                            MFMetadataStructureItem.MFMetadataStructureItemObjectType, MfilesAliasConfig.ObjProject)
                });
                var sr = vault.ObjectSearchOperations.SearchForObjectsByCondition(sc, false).ObjectVersions;
                if (sr.Count < 1)
                    Writelog(string.Format("DocumentOperation- can't find project object in the vault <{0}>--", vault.Name));
                foreach (ObjectVersion objectVersion in sr)
                {
                    pvs = vault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer);

                    Project = new ProjectInMfiles
                    {
                        PropProjName =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropProjName, true).GetValueAsLocalizedText(),
                        PropProjNum = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropProjNum, true).GetValueAsLocalizedText(),
                        PropDescription =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropDescription, true).GetValueAsLocalizedText(),
                        PropProprietorUnit =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropProprietorUnit, true).GetValueAsLocalizedText(),
                        PropSupervisorUnit =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropSupervisorUnit, true).GetValueAsLocalizedText(),
                        PropDesignUnit =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropDesignUnit, true).GetValueAsLocalizedText(),
                        PropStartDate =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropStartDate, true).GetValueAsLocalizedText(),
                        Deadline =
                            pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefDeadline)
                                .GetValueAsLocalizedText(),
                    };
                    break;
                }
                pvs = vault.ObjectPropertyOperations.GetProperties(objver);
                app = new Application();
                object unknow = Type.Missing;
                doc = app.Documents.Open(filepath,
                    ref unknow, false, ref unknow, ref unknow, ref unknow,
                    ref unknow, ref unknow, ref unknow, ref unknow, ref unknow,
                    ref unknow, ref unknow, ref unknow, ref unknow, ref unknow);
                table = doc.Tables[doc.Tables.Count];
            }
            catch (Exception ex)
            {
                Writelog(string.Format("DocumentOperation-- vault={0},type={1}，id={2}，version={3}，CurrentUserID={4},StateID={5},DisplayID={6},{7}",
                    vault.Name,objver.Type,objver.ID,objver.Version,stateEnvironment.CurrentUserID,stateEnvironment.StateID,stateEnvironment.DisplayID, ex.Message));
            }
        }
        public void SignatureDateEx(Cell cell, string startstr)
        {
            var text = cell.Range.Text;
            var start = text.IndexOf(startstr);
            Regex reg = new Regex(@"([\d]{4}年[\d]*月[\d]*日\s+)|([\s]{1,4}年[\s]*月[\s]*日\s+)");
            cell.Range.Text = reg.Replace(text, PrintDateNow(), 1, start);
            Writelog("SignatureDateEx:" + startstr + start);
        }
        public void PictureOrLiteralSignatureIncludeDateBaseOnCreator(Cell cell, string literalAnchor,bool eraseBlank=true)
        {
            SignatureResource sr = GetSignatureResourceByCreator();
            if (sr.IsFile)
            {
                if (doc.Bookmarks.Exists(literalAnchor+"creator")) return;
                SignatureDateEx(cell, literalAnchor);
                InsertPictureSignature(cell, literalAnchor, sr.ContentText, eraseBlank);
                doc.Bookmarks.Add(literalAnchor + "creator");
            }
            else
            {
                InsertLiteralSignature(cell, sr.ContentText, literalAnchor);
                SignatureDateEx(cell, literalAnchor);
            }
        }
      
      
        public void PictureOrLiteralSignatureIncludeDateByAlias(Cell cell, string ugAlias, string literalAnchor)
        {
            try
            {
                SignatureResource sr = GetSignatureResource(ugAlias);
                if (sr.IsFile)
                {
                    if (doc.Bookmarks.Exists(ugAlias)) return;
                    SignatureDateEx(cell, literalAnchor);
                    InsertPictureSignature(cell, literalAnchor, sr.ContentText);
                    doc.Bookmarks.Add(ugAlias);
                }
                else
                {
                    InsertLiteralSignature(cell, sr.ContentText, literalAnchor);
                    SignatureDateEx(cell, literalAnchor);
                }
            }
            catch (Exception ex)
            {
                Writelog(string.Format("PictureOrLiteralSignatureIncludeDateByAlias-{0},{1},{2}" , ugAlias , literalAnchor , ex.Message));
            }
        }
        public void PictureOrLiteralSignatureIncludeDate(Cell cell, string ugAlias,  string literalAnchor, string dateAnchor)
        {
           
            SignatureResource sr = GetSignatureResource(ugAlias);
            if (sr.IsFile)
            {
                SignatureDate(cell, dateAnchor);
               // InsertPictureSignatureByRange(cell.Range, sr.ContentText, left, top);
                InsertPictureSignature(cell, literalAnchor, sr.ContentText);
            }
            else
            {
                InsertLiteralSignature(cell, sr.ContentText, literalAnchor);
                SignatureDate(cell, dateAnchor);
            }
        }
        //public void PictureOrLiteralSignatureIncludeDateBaseOnTransition(Cell cell, string literalAnchor)
        //{
        //    SignatureResource sr = GetSignatureResource();
        //    if (sr.IsFile)
        //    {
        //        SignatureDateEx(cell, literalAnchor);
        //        InsertPictureSignature(cell, literalAnchor, sr.ContentText, false);
        //    }
        //    else
        //    {
        //        InsertLiteralSignature(cell, sr.ContentText, literalAnchor);
        //        SignatureDateEx(cell, literalAnchor);
        //    }
        //}
        public void InsertPictureSignature(Cell cell, string literalAnchor, string picturePath, bool first = true)
        {
            cell.Select();
            object oMissing = System.Reflection.Missing.Value;

            app.Selection.Find.ClearFormatting();
            app.Selection.Find.Text = literalAnchor;
            var found = app.Selection.Find.Execute(
                                             ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                             ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                             ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            if (!found)
            {
                Writelog(string.Format("there is no the identifer {0}", app.Selection.Find.Text));
                return;
            }
            Writelog(string.Format("found the identifer {0}", app.Selection.Find.Text));
            object dummy = System.Reflection.Missing.Value;
            object count = 1;
            object Unit = WdUnits.wdCharacter;
            app.Selection.MoveRight(ref Unit, ref count, ref dummy);
            InsertPictureSignatureByRange(app.Selection.Range, picturePath);

            if (first)
            {
                object replaceAll = WdReplace.wdReplaceOne;
                app.Selection.Find.ClearFormatting();
                app.Selection.Find.Text = "          ";
                app.Selection.Find.Replacement.ClearFormatting();
                app.Selection.Find.Replacement.Text = "";
                app.Selection.Find.Execute(
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref replaceAll, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            }
        }
        public SignatureResource GetSignatureResource(string ugalias)
        {
            var sr = new SignatureResource();
            try
            {
                var ug = vault.UserGroupOperations.GetUserGroupIDByAlias(ugalias);
                var members = vault.UserGroupOperations.GetUserGroup(ug).Members;

                foreach (var member in members)
                {
                    var loginAccount = vault.UserOperations.GetLoginAccountOfUser((int) member);
                    sr.ContentText = loginAccount.FullName;
                    var di = new DirectoryInfo(SignaturePicturePath);
                    foreach (FileInfo fi in di.GetFiles())
                    {
                        if (fi.Name.StartsWith(loginAccount.AccountName))
                        {
                            sr.ContentText = fi.FullName;
                            sr.IsFile = true;
                            break;
                        }
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                Writelog(string.Format("GetSignatureResource, {0},{1}",ugalias,ex.Message));
            }

            return sr;
        }
        public SignatureResource GetSignatureResource()
        {
            var sr = new SignatureResource();

            var signature = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSignatureManifestation).GetValueAsLocalizedText();
            Writelog(string.Format("signature is  <{0}>,{1},{2},{3}", signature, signature.Length, signature.LastIndexOf('('), signature.LastIndexOf(')')));
            var start = signature.LastIndexOf('(') + 1;
            var length = signature.LastIndexOf(')') - start;
            var username = signature.Substring(start, length);


             start = signature.IndexOf(')') + 1;
             length = signature.LastIndexOf('(') - start;

             sr.ContentText = signature.Substring(start, length); ;
                var di = new DirectoryInfo(SignaturePicturePath);
                foreach (FileInfo fi in di.GetFiles())
                {
                    if (fi.Name.StartsWith(username))
                    {
                        sr.ContentText = fi.FullName;
                        sr.IsFile = true;
                        break;
                    }
                }

            return sr;
        }
        public SignatureResource GetSignatureResourceByCreator()
        {
            var sr = new SignatureResource();

            var creator =
                    vault.ObjectPropertyOperations.GetProperty(objver,
                        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).Value.GetLookupID();
            var logaccount = vault.UserOperations.GetLoginAccountOfUser(creator);
            var username = logaccount.AccountName;

            sr.ContentText = logaccount.FullName;
            var di = new DirectoryInfo(SignaturePicturePath);
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Name.StartsWith(username))
                {
                    sr.ContentText = fi.FullName;
                    sr.IsFile = true;
                    break;
                }
            }

            return sr;
        }
        public SignatureResource GetSignatureResourceByLoginAccount(LoginAccount loginaccount)
        {
            var sr = new SignatureResource();

            sr.ContentText = loginaccount.FullName;
            var di = new DirectoryInfo(SignaturePicturePath);
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Name.StartsWith(loginaccount.AccountName))
                {
                    sr.ContentText = fi.FullName;
                    sr.IsFile = true;
                    break;
                }
            }

            return sr;
        }
        public string PrintDateNow()
        {
            return DateTime.Now.ToString("D");
        }
        public void PictureOrLiteralSignatureBaseOnCreator(Cell cell)
        {
            SignatureResource sr = GetSignatureResourceByCreator();
            if (sr.IsFile)
            {
                InsertPictureSignatureByRange(cell.Range, sr.ContentText);
            }
            else
            {
                cell.Range.Text = sr.ContentText;
            }
        }
        public void PictureOrLiteralSignature(Cell cell, string ugAlias)
        {
            SignatureResource sr = GetSignatureResource(ugAlias);
            if (sr.IsFile)
            {
                InsertPictureSignatureByRange(cell.Range, sr.ContentText);
            }
            else
            {
                cell.Range.Text=sr.ContentText;
            }
        }

      
      
       
        public bool LocateAnchorPosition(string literalAnchor)
        {
            object oMissing = System.Reflection.Missing.Value;
            app.Selection.Find.ClearFormatting();
            app.Selection.Find.Text = literalAnchor;
            var found = app.Selection.Find.Execute(
                                             ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                             ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                             ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            if (!found)
            {
                Writelog(string.Format("there is no the identifer {0}", app.Selection.Find.Text));
            }
            else
            {
                Writelog(string.Format("found the identifer {0}", app.Selection.Find.Text));
            }
            return found;
        }
           public void InsertLiteralSignature( string fullName, string literalAnchor)
        {
            if(LocateAnchorPosition(literalAnchor+fullName)) return;
            if (!LocateAnchorPosition(literalAnchor)) return;
            object dummy = System.Reflection.Missing.Value;
            object count = 1;
            object Unit = WdUnits.wdCharacter;
            app.Selection.MoveRight(ref Unit, ref count, ref dummy);
            app.Selection.TypeText(fullName);
        }
           public void MEInsertLiteralSignature(Cell cell, string fullName, string literalAnchor)
           {
               cell.Select();
               if (LocateAnchorPosition(literalAnchor + fullName)) return;
               if (!LocateAnchorPosition(literalAnchor)) return;
               object dummy = System.Reflection.Missing.Value;
               object count = 1;
               object Unit = WdUnits.wdCharacter;
               app.Selection.MoveRight(ref Unit, ref count, ref dummy);
               app.Selection.TypeText(fullName);
           }
        public void InsertLiteralSignature(Cell cell, string fullName, string literalAnchor)
        {
            cell.Select();
            if (LocateAnchorPosition(literalAnchor + fullName)) return;
            var text = cell.Range.Text;
          //  Regex reg = new Regex(@"(" + literalAnchor + @"\s{3," + fullName.Length*2 + "})|(" +literalAnchor+ fullName + ")");
          //  Regex reg = new Regex(@"(" + literalAnchor + @"\s{" + fullName.Length * 2 + "})|(" + literalAnchor + fullName + ")");
            Regex reg = new Regex( literalAnchor + @"\s{" + fullName.Length * 2 + "}");
          //  if(reg.Match(text).Captures.Count>0)
                  
            cell.Range.Text = reg.Replace(text, literalAnchor+fullName+"   ");
            //else
            //{
            //    reg = new Regex(@"(" + literalAnchor + @"\s{1," + fullName.Length * 2 + "})|(" + literalAnchor + fullName + ")");
            //}           
        }
        public void SignPicture(Cell cell, string ugAlias, int left, int top, string literalAnchor)
        {
            //日期
            var dateText = cell.Range.Text;
            if (dateText.Contains("日期："))
            {
                cell.Range.Text = dateText.Replace("日期：", "日期：" + PrintDateNow());
            } 
            SignatureResource sr = GetSignatureResource(ugAlias);
            if (sr.IsFile)
            {
                object Anchor = cell.Range;
                object LinkToFile = false;
                object SaveWithDocument = true;
                object width = 37;
                object height = 26;
                object leftobj = left;
                object topobj = top;
                doc.Shapes.AddPicture(sr.ContentText, ref LinkToFile, ref SaveWithDocument, ref leftobj, ref topobj, ref width, ref height, ref Anchor);                
            }
            else
            {
              // InsertLiteralSignature(cell, sr.ContentText, literalAnchor);
                //var text = cell.Range.Text;
                //cell.Range.Text = text.Replace(literalAnchor, literalAnchor + sr.ContentText).TrimEnd(); 
                //Regex rgx = new Regex(literalAnchor);
                //cell.Range.Text = rgx.Replace(cell.Range.Text, literalAnchor + sr.ContentText);
                if (!string.IsNullOrEmpty(sr.ContentText))
                {
                    var text = cell.Range.Text;
                    Regex reg = new Regex(literalAnchor);
                    cell.Range.Text = reg.Replace(text, literalAnchor + sr.ContentText); 
                    
                }              
            }          
        }
      
         public void SignatureDate(Cell cell)
         {
             var text = cell.Range.Text;
          //   Regex reg = new Regex(@"[\d{4}|\s{1,4}]年[\s|\d]*月[\s|\d]*日\s*");
             Regex reg = new Regex(@"([\d]{4}年[\d]*月[\d]*日\s*)|([\s]{1,4}年[\s]*月[\s]*日\s*)");
             cell.Range.Text = reg.Replace(text, PrintDateNow());
         }
         public void SignatureDate(Cell cell, string dateAnchor)
        {
            var text = cell.Range.Text;
            Regex reg = new Regex(@"(" + dateAnchor + @"[\d]{4}年[\d]{1,2}月[\d]{1,2}日\s*)|([\s]{14}" + dateAnchor + @"\s+)");
            cell.Range.Text = reg.Replace(text, dateAnchor+PrintDateNow());
        }
        public void InsertPictureSignatureByRange(Range range, string pictureFullPath, object left, object top)
        {
            object Anchor = range;
            object LinkToFile = false;
            object SaveWithDocument = true;
            object width = 57;
            object height = 40;
            doc.Shapes.AddPicture(pictureFullPath, ref LinkToFile, ref SaveWithDocument, ref left, ref top, ref width, ref height, ref  Anchor);
        }
        public void InsertPictureSignatureByRange(Range range, string pictureFullPath)
        {
            try
            {
                object Anchor = range;
                object LinkToFile = false;
                object SaveWithDocument = true;
                InlineShape insh = doc.InlineShapes.AddPicture(pictureFullPath, ref LinkToFile, ref SaveWithDocument,
                    ref Anchor);
                insh.Height = 40;
                insh.Width = 57;
            }
            catch (Exception ex)
            {
                Writelog(string.Format("InsertPictureSignatureByRange {0} error:{1}", pictureFullPath,ex.Message));
            }
        }
    
     
        public void UpDateFile()
        {

            var filesup = vault.ObjectFileOperations.GetFilesForModificationInEventHandler(objver);
            foreach (ObjectFile objectFile in filesup)
            {
                vault.ObjectFileOperations.UploadFile(objectFile.ID, objectFile.Version, filepath);
                break;
            }
        }

        public void CloseWord()
        {
            doc.Save();
            doc.Close();
            app.Quit();
        }
      


        public static void Writelog(string logtext)
        {
            try
            {
                using (
                    StreamWriter sw =
                        File.AppendText(Path.Combine(Path.GetTempPath(),
                            DateTime.Now.Date.ToString("yyyy-MM-dd") + "VaultappDocumentOperationLog.txt")))
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
