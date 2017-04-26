using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AecCloud.MFilesCore;
using MFiles.VAF.Common;
using Microsoft.Office.Interop.Word;

namespace VaultApp
{
    class MainControlPointPlan :  DocumentOperation
    {
        public MainControlPointPlan(StateEnvironment stateEnvironment)
            : base(stateEnvironment)
        {
            //  
        }
     
        public void InsertPictureSignature( string literalAnchor, string picturePath, bool first = true)
        {
            if (doc.Bookmarks.Exists(literalAnchor)) return;

            LocateAnchorPosition(literalAnchor);

            object dummy = System.Reflection.Missing.Value;
            object count = 1;
            object Unit = WdUnits.wdCharacter;
            app.Selection.MoveRight(ref Unit, ref count, ref dummy);

            InsertPictureSignatureByRange(app.Selection.Range, picturePath);
            doc.Bookmarks.Add(literalAnchor);
            if (first)
            {
                object replaceAll = WdReplace.wdReplaceOne;
                app.Selection.Find.ClearFormatting();
                app.Selection.Find.Text = "          ";
                app.Selection.Find.Replacement.ClearFormatting();
                app.Selection.Find.Replacement.Text = "";
                object oMissing = System.Reflection.Missing.Value;
                app.Selection.Find.Execute(
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref replaceAll, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            }
        }

      
        public void PictureOrLiteralSignature( string ugAlias, string literalAnchor)
        {
            SignatureResource sr = GetSignatureResource(ugAlias);
            if (sr.IsFile)
            {
                InsertPictureSignature( literalAnchor, sr.ContentText);
            }
            else
            {
                InsertLiteralSignature( sr.ContentText, literalAnchor);
            }
        }
        public void DirectlyControlOver()
        {
            try
            {
                PictureOrLiteralSignature(MfilesAliasConfig.UGroupPM, "项目经理：");
                PictureOrLiteralSignature(MfilesAliasConfig.UgGeneralContractingManager, "总承包项目经理：");
                PictureOrLiteralSignature(MfilesAliasConfig.UgHeadquartersEngineeringManager, "公司工程管理部经理：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("DirectlyControlOver" + ex.Message);
            }
        }
        public void SecondLevelOver()
        {
            try
            {
                PictureOrLiteralSignature(MfilesAliasConfig.UGroupPM, "项目经理：");
                PictureOrLiteralSignature(MfilesAliasConfig.UgGeneralContractingManager, "总承包项目经理：");
                PictureOrLiteralSignature(MfilesAliasConfig.UgSecondLevelManager, "二级单位经理：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("SecondLevelOver" + ex.Message);
            }
        }
        public void MCPPCreatePlan()
        {
            try
            {
                table.Cell(5, 2).Range.Text = Project.PropProprietorUnit + Environment.NewLine
                + Project.PropProjName;

                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("MCPPCreatePlan" + ex.Message);
            }
        }
    }
}
