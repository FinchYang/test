using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AecCloud.MFilesCore;
using MFiles.VAF.Common;
using MFilesAPI;
using Microsoft.Office.Interop.Word;

namespace VaultApp
{
    class QualityAdjustmentNotice:DocumentOperation
    {
        public QualityAdjustmentNotice(StateEnvironment stateEnvironment) : base(stateEnvironment)
        {
            //
        }
        public void WfsAssignAdjustmentSelfInspection()
        {
            try
            {
                var verifier = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy);
                var content = pvs.SearchForPropertyByAlias(vault, "PropAdjustmentContent", false).GetValueAsLocalizedText();
                var thecell = table.Cell(table.Rows.Count - 3, 1);
                thecell.Range.Text = "整改内容：" + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                                     Environment.NewLine + Environment.NewLine + Environment.NewLine +
                                     Environment.NewLine + Environment.NewLine + Environment.NewLine +
                                     Environment.NewLine + "                                       检查人：                 年   月   日"+Environment.NewLine;
                QualityAdjustmentNoticeSignatureNorepeat(thecell, 150, 150, "整改内容：", content, "检查人：", verifier);
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("WfsAssignAdjustmentSelfInspection" + ex.Message);
            }
        }
        public void CreateQualityNotice()
        {
            try
            {
                table.Cell(5, 2).Range.Text = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropNameAndCoding,false).GetValueAsLocalizedText();
                table.Cell(6, 2).Range.Text = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropProjectBasicInfo, false).GetValueAsLocalizedText();
                table.Cell(7, 2).Range.Text = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropAcceptingUnit, false).GetValueAsLocalizedText();
                table.Cell(7, 4).Range.Text = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropQualityAdjustmentReceiver, false).GetValueAsLocalizedText();
                table.Cell(9, 2).Range.Text = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropCompletionDate, false).GetValueAsLocalizedText();
                table.Cell(9, 4).Range.Text = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropSpecifiedVerifier, false).GetValueAsLocalizedText();
                var verifier = pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy);
                var content = pvs.SearchForPropertyByAlias(vault, "PropAdjustmentContent", false).GetValueAsLocalizedText();
            Writelog(string.Format("creater:{0},adjustmentcontent:{1},{2}",verifier.GetValueAsLocalizedText(),content,vault.Name));
                QualityAdjustmentNoticeSignatureNorepeat(table.Cell(table.Rows.Count - 3, 1), 150, 150, "整改内容：", content, "检查人：", verifier);
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("CreateQualityNotice---" + ex.Message);
            }
        }
        private void QualityAdjustmentNoticeSignatureNorepeat(Microsoft.Office.Interop.Word.Cell cell, int left, int top, string contentAnckor, string content, string signatureAnckor, PropertyValue userPropertyValue)
        {
            var users = userPropertyValue.Value.GetValueAsLookups();
            var literals = string.Empty;
            var lists = new List<string>();
            foreach (Lookup lookup in users)
            {
                var loginaccount = vault.UserOperations.GetLoginAccountOfUser(lookup.Item);
                SignatureResource sr = GetSignatureResourceByLoginAccount(loginaccount);
                if (sr.IsFile)
                {
                    lists.Add(sr.ContentText);
                }
                else
                {
                    literals += " " + sr.ContentText;
                }
            }
            if (literals != string.Empty)
            {
                QualityAdjustmentNoticeInsertLiteralSignature(cell, literals, signatureAnckor);
            }

            var text = cell.Range.Text;
            Regex reg = new Regex(contentAnckor + @"\s" );
            cell.Range.Text = reg.Replace(text, contentAnckor + content );

            SignatureDate(cell);

            if (lists.Count > 0)
                foreach (string fullpath in lists)
                {
                    InsertPictureSignatureByRange(cell.Range, fullpath, left, top);
                    left += 60;
                }
        }
        private void QualityAdjustmentNoticeInsertLiteralSignature(Cell cell, string fullName, string literalAnchor)
        {
            cell.Select();
            if (LocateAnchorPosition(literalAnchor + fullName)) return;
            var text = cell.Range.Text;
            Regex reg = new Regex(literalAnchor + @"\s{" + fullName.Length * 2 + "}");
            cell.Range.Text = reg.Replace(text, literalAnchor + fullName );
        }
    
       
        public void WfsSelfInspection()
        {
            try
            {
                var verifier = pvs.SearchForPropertyByAlias(vault, "PropSelfInspector", false);
                var content = pvs.SearchForPropertyByAlias(vault, "PropSituationsAndResults", false).GetValueAsLocalizedText();
                var thecell = table.Cell(table.Rows.Count - 1, 1);
                thecell.Range.Text = "处理情况和自检结果：" + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                                     Environment.NewLine + Environment.NewLine + Environment.NewLine
                                     + "                                       自检人：                 年   月   日" +
                                     Environment.NewLine;
                QualityAdjustmentNoticeSignatureNorepeat(thecell, 150, 50, "处理情况和自检结果：", content, "自检人：", verifier);
                //    PictureOrLiteralSignatureByUsers(table.Cell(table.Rows.Count-1, 1), verifier, 150, 50, "自检人："); 
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("WfsSelfInspection" + ex.Message);
            }
        }
        public void WfsQualityAdjustmentAcceptance()
        {
            try
            {
                var verifier = pvs.SearchForPropertyByAlias(vault, "PropSpecifiedVerifier", false);
                var content = pvs.SearchForPropertyByAlias(vault, "PropAcceptanceRecord", false).GetValueAsLocalizedText();
                var thecell = table.Cell(table.Rows.Count, 1);
                thecell.Range.Text = "验收记录：" + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                                     "                                       验证人：                 年   月   日"+Environment.NewLine;
                QualityAdjustmentNoticeSignatureNorepeat(thecell, 50, 30, "验收记录：", content, "验证人：", verifier);
              //  PictureOrLiteralSignatureByUsers(table.Cell(table.Rows.Count, 1), verifier, 50, 10, "验证人：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("WfsQualityAdjustmentAcceptance" + ex.Message);
            }
        }
    }
}
