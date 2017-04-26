using System;
using System.Text.RegularExpressions;
using AecCloud.MFilesCore;
using MFiles.VAF.Common;
using Microsoft.Office.Interop.Word;

namespace VaultApp
{
    class ProjectCompletionConfirm : DocumentOperation
    {

        public ProjectCompletionConfirm(StateEnvironment stateEnvironment)
            : base(stateEnvironment)
        {
        }


        public void WfsCreatePccDirectlyControl()
        {
            try
            {
                //var creator =
                //    vault.ObjectPropertyOperations.GetProperty(objver,
                //        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).GetValueAsLocalizedText();
                //table.Cell(13, 1).Range.Text = "项目副经理（生产）： " + Environment.NewLine
                //+ "签字：" + creator + " 日期：" + PrintDateNow();
                //var creator =
                //     vault.ObjectPropertyOperations.GetProperty(objver,
                //         (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).Value.GetLookupID();
                //var username = vault.UserOperations.GetUserAccount(creator).LoginName;
                //CscecSignatureByUsername(table.Cell(13, 1), username, "项目副经理（生产）： ", " 签字：");

                PictureOrLiteralSignatureIncludeDateBaseOnCreator(table.Cell(table.Rows.Count - 3, 1),  "签字：");

                table.Cell(5, 2).Range.Text = Project.PropProjName;
                table.Cell(6, 2).Range.Text = Project.PropDescription;
                table.Cell(7, 2).Range.Text = Project.PropProprietorUnit;
                table.Cell(8, 2).Range.Text = Project.PropSupervisorUnit;
                table.Cell(9, 2).Range.Text = Project.PropDesignUnit;
                table.Cell(10, 2).Range.Text = Project.PropStartDate;
                table.Cell(10, 4).Range.Text = Project.Deadline;

                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("WfsCreatePccDirectlyControl" + ex.Message);
            }
        }
        public void InsertPictureSignatureByBookMark(object bookMark, string replacePic)
        {
            try
            {
                object linkToFile = false;
                object saveWithDocument = true;
                object Nothing = System.Reflection.Missing.Value;
                if (doc.Bookmarks.Exists(Convert.ToString(bookMark)) == true)
                {
                    //查找书签
                    doc.Bookmarks.get_Item(ref bookMark).Select();
                    //设置图片位置
                    app.Selection.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphRight;
                    //在书签的位置添加图片
                    InlineShape insh = app.Selection.InlineShapes.AddPicture(replacePic, ref linkToFile, ref saveWithDocument, ref Nothing);

                    insh.Height = 40;
                    insh.Width = 57;
                }
                else
                {
                    Writelog(string.Format("there  is  no the bookmark named {0}", bookMark));
                }
            }
            catch (Exception ex)
            {
                Writelog(string.Format("InsertPictureSignatureByBookMark error:{0}", ex.Message));
            }
        }
    
      
      

        public void FillCreatePcc()
        {
            try
            {
                //var creator =
                //    vault.ObjectPropertyOperations.GetProperty(objver,
                //        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).GetValueAsLocalizedText();
                //table.Cell(13, 1).Range.Text = "项目经理/（直属项目总工、商务经理、项目经理）： " + Environment.NewLine
                //+ "签字：" + creator + " 日期：" + PrintDateNow();
                //var creator =
                //    vault.ObjectPropertyOperations.GetProperty(objver,
                //        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).Value.GetLookupID();
                //var username = vault.UserOperations.GetUserAccount(creator).LoginName;
                //CscecSignatureByUsername(table.Cell(13, 1), username, "项目经理/（直属项目总工、商务经理、项目经理）： ", " 签字：");

                PictureOrLiteralSignatureIncludeDateBaseOnCreator(table.Cell(table.Rows.Count - 3, 1),  "签字：");

                table.Cell(5, 2).Range.Text = Project.PropProjName;
                table.Cell(6, 2).Range.Text = Project.PropDescription;
                table.Cell(7, 2).Range.Text = Project.PropProprietorUnit;
                table.Cell(8, 2).Range.Text = Project.PropSupervisorUnit;
                table.Cell(9, 2).Range.Text = Project.PropDesignUnit;
                table.Cell(10, 2).Range.Text = Project.PropStartDate;
                table.Cell(10, 4).Range.Text = Project.Deadline;


                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("FillCreatePcc" + ex.Message);
            }
        }


      
        public void WfsForTheRecordDirectlyControl()
        {
            try
            {
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(table.Rows.Count - 2, 1), MfilesAliasConfig.UgProjectBusinessManager, "签字：");
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(table.Rows.Count - 1, 1), MfilesAliasConfig.UgChiefEngineer, "签字：");
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(table.Rows.Count, 1), MfilesAliasConfig.UGroupPM, "签字：");
                //CscecSignature(table.Cell(14, 1), MfilesAliasConfig.UgProjectBusinessManager, "项目商务经理", "签字：  ");
                //CscecSignature(table.Cell(15, 1), MfilesAliasConfig.UgChiefEngineer, "项目总工程师", "签字：  ");
                //CscecSignature(table.Cell(16, 1), MfilesAliasConfig.UGroupPM, "项目经理", "签字：  ");
               // table.Cell(14, 1).Range.Text = "项目商务经理： " + Environment.NewLine
               // + "签字：" + GetManager(MfilesAliasConfig.UgBusinessManager, vault) + " 日期：" + PrintDateNow();
               // table.Cell(15, 1).Range.Text = "项目总工程师： " + Environment.NewLine
               //+ "签字：" + GetManager(MfilesAliasConfig.UgChiefEngineer, vault) + " 日期：" + PrintDateNow();
               // table.Cell(16, 1).Range.Text = "项目经理： " + Environment.NewLine
               //+ "签字：" + GetManager(MfilesAliasConfig.UGroupPM, vault) + " 日期：" + PrintDateNow();
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("FillCounterSign" + ex.Message);
            }
        }
        public void FillCounterSign()
        {
            try
            {
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(table.Rows.Count - 2, 1), MfilesAliasConfig.UgSecondLevelDeputyManager, "签字：");
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(table.Rows.Count - 1, 1), MfilesAliasConfig.UgSecondLevelChiefEngineer, "签字：");
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(table.Rows.Count, 1), MfilesAliasConfig.UgSecondLevelChiefEconomist, "签字：");
                //CscecSignature(table.Cell(14, 1), MfilesAliasConfig.UgSecondLevelDeputyManager, "二级单位副经理（生产）：", "签字：  ");
                //CscecSignature(table.Cell(15, 1), MfilesAliasConfig.UgSecondLevelChiefEngineer, "二级单位总工程师：", "签字：  ");
                //CscecSignature(table.Cell(16, 1), MfilesAliasConfig.UgSecondLevelChiefEconomist, "二级单位总经济师：", "签字：  ");
               // table.Cell(14, 1).Range.Text = "二级单位副经理（生产）： " + Environment.NewLine
               //                                + "签字：" + GetManager(MfilesAliasConfig.UgSecondLevelDeputyManager, vault) +
               //                                " 日期：" + PrintDateNow();
               // table.Cell(15, 1).Range.Text = "二级单位总工程师： " + Environment.NewLine
               //+ "签字：" + GetManager(MfilesAliasConfig.UgSecondLevelChiefEngineer, vault) + " 日期：" + PrintDateNow();
               // table.Cell(16, 1).Range.Text = "二级单位总经济师： " + Environment.NewLine
               //+ "签字：" + GetManager(MfilesAliasConfig.UgSecondLevelChiefEconomist, vault) + " 日期：" + PrintDateNow();
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("FillCounterSign" + ex.Message);
            }
        }

    }
}
