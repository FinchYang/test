using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.MFilesCore;
using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.Office.Interop.Word;

namespace VaultApp
{
    class ConstructionPeriodDelayAnalysis : DocumentOperation
    {
        public ConstructionPeriodDelayAnalysis(StateEnvironment stateEnvironment)
            : base(stateEnvironment)
        {
            //  
        }
        public void DelayAnalysisDelayDirectlyControlEnd()
        {
            try
            {
                //var signature = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSignatureManifestation);
                //var count = table.Rows.Count;
                //table.Cell(count, 2).Range.Text = "副总经理（生产）: " + signature.GetValueAsLocalizedText() + Environment.NewLine + PrintDateNow();
            //    CscecSignatureByTransition(table.Cell(table.Rows.Count, 2), "副总经理（生产）: ", " ");
                DANCPictureOrLiteralSignatureIncludeDateBaseOnTransition(table.Cell(table.Rows.Count, 2), "副总经理（生产）：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisDelayDirectlyControlEnd" + ex.Message);
            }
        }
      
        public void DelayAnalysisDelayGeneralEnd()
        {
            try
            {
                //var signature = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSignatureManifestation);
                //var count = table.Rows.Count;
                //table.Cell(count, 2).Range.Text = "副经理（生产）/总工程师: " + signature.GetValueAsLocalizedText() + Environment.NewLine + PrintDateNow();
             //   CscecSignatureByTransition(table.Cell(table.Rows.Count, 2), "副经理（生产）/总工程师: ", " ");
                DANCPictureOrLiteralSignatureIncludeDateBaseOnTransition(table.Cell(table.Rows.Count, 2), "副经理（生产）/总工程师：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisDelayGeneralEnd" + ex.Message);
            }
        }
        public void DelayAnalysisNormalEnd()
        {
            try
            {
                //var signature = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSignatureManifestation);
                //var count = table.Rows.Count;
                //table.Cell(count, 3).Range.Text = "项目经理: " + signature.GetValueAsLocalizedText() + Environment.NewLine + PrintDateNow();
             //   CscecSignatureByTransition(table.Cell(table.Rows.Count, 3), "项目经理: ", " ");
                DANCPictureOrLiteralSignatureIncludeDateBaseOnTransition(table.Cell(table.Rows.Count, 3), "项目经理：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisNormalEnd" + ex.Message);
            }
        }
        public void DelayAnalysisNormalCounterSignPM()
        {
            try
            {
                //Writelog("DelayAnalysisNormalCounterSignPM" + " in");
                //var signature=pvs.SearchForProperty((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefSignatureManifestation);
                //Writelog("DelayAnalysisNormalCounterSignPM" + " 11");
                //var count = table.Rows.Count;
                //Writelog("DelayAnalysisNormalCounterSignPM" + " 22");
                //table.Cell(count, 2).Range.Text = "项目副经理（生产）/总工程师： " + signature.GetValueAsLocalizedText() + Environment.NewLine + PrintDateNow();
               // CscecSignatureByTransition(table.Cell(table.Rows.Count, 2), "项目副经理（生产）/总工程师： ", " ");
                DANCPictureOrLiteralSignatureIncludeDateBaseOnTransition(table.Cell(table.Rows.Count, 2), "项目副经理（生产）/总工程师：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisNormalCounterSignPM" + ex.Message);
            }
        }
        public void DANCPictureOrLiteralSignatureIncludeDateBaseOnTransition(Cell cell, string literalAnchor)
        {
            SignatureResource sr = GetSignatureResource();
            if (sr.IsFile)
            {
                SignatureDateEx(cell, literalAnchor);
                InsertPictureSignature(cell, literalAnchor, sr.ContentText, false);
            }
            else
            {
                MEInsertLiteralSignature(cell, sr.ContentText, literalAnchor);
                SignatureDateEx(cell, literalAnchor);
            }
        }
        public void DelayAnalysisDelayDirectlyControlCreate()
        {
            try
            {
                table.Cell(5, 1).Range.Text = "项目名称及编码:" + Project.PropProjName + Project.PropProjNum;
                //var creator =
                //    vault.ObjectPropertyOperations.GetProperty(objver,
                //        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).GetValueAsLocalizedText();
                //var count = table.Rows.Count;
                //table.Cell(count, 1).Range.Text = "公司工程管理部经理： " + creator + Environment.NewLine + PrintDateNow();
                //var creator =
                //     vault.ObjectPropertyOperations.GetProperty(objver,
                //         (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).Value.GetLookupID();
                //var username = vault.UserOperations.GetUserAccount(creator).LoginName;
                //CscecSignatureByUsername(table.Cell(table.Rows.Count, 1), username, "公司工程管理部经理： ", " ");

                PictureOrLiteralSignatureIncludeDateBaseOnCreator(table.Cell(table.Rows.Count, 1),  "公司工程管理部经理：",false);
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisDelayDirectlyControlCreate" + ex.Message);
            }
        }
        public void DelayAnalysisDelayGeneralCreate()
        {
            try
            {
                table.Cell(5, 1).Range.Text = "项目名称及编码:" + Project.PropProjName + Project.PropProjNum;
                //var creator =
                //    vault.ObjectPropertyOperations.GetProperty(objver,
                //        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).GetValueAsLocalizedText();
                //var count = table.Rows.Count;
               // table.Cell(count, 1).Range.Text = "二级单位施工管理部经理： " + creator + Environment.NewLine + PrintDateNow();
                //var creator =
                //    vault.ObjectPropertyOperations.GetProperty(objver,
                //        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).Value.GetLookupID();
                //var username = vault.UserOperations.GetUserAccount(creator).LoginName;
                //CscecSignatureByUsername(table.Cell(table.Rows.Count, 1), username, "二级单位施工管理部经理： ", " ");
                PictureOrLiteralSignatureIncludeDateBaseOnCreator(table.Cell(table.Rows.Count, 1),  "二级单位施工管理部经理：",false);
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisDelayGeneralCreate" + ex.Message);
            }
        }
        public void DelayAnalysisNormalCreate()
        {
            try
            {
                table.Cell(5, 1).Range.Text = "项目名称及编码:" + Project.PropProjName+ Project.PropProjNum;
                //var creator =
                //    vault.ObjectPropertyOperations.GetProperty(objver,
                //        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).Value.GetLookupID();
                //var username = vault.UserOperations.GetUserAccount(creator).LoginName;
                //CscecSignatureByUsername(table.Cell(table.Rows.Count, 1), username, "工长： ", " ");
                PictureOrLiteralSignatureIncludeDateBaseOnCreator(table.Cell(table.Rows.Count, 1), "工长：",false);
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("DelayAnalysisNormalCreate" + ex.Message);
            }
        }
    }
}
