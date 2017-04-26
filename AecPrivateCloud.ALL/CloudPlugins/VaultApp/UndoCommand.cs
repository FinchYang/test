using System;
using AecCloud.MFilesCore;
using MFiles.VAF.Common;
using MFilesAPI;

namespace VaultApp
{
    class UndoCommand : DocumentOperation
    {

        public UndoCommand(StateEnvironment stateEnvironment)
            : base(stateEnvironment)
        {
        }



        public void FillApprovedInfo()
        {
            try
            {
                //var rowindex = table.Rows.Count - 2;
                //var therange = table.Cell(++rowindex, 6).Range;
                //therange.Text = "";
             //   InsertSignature(therange, MfilesAliasConfig.UgHeadquartersGeneralManager);
                table.Cell(table.Rows.Count - 1, 6).Range.Text = string.Empty;
                PictureOrLiteralSignature(table.Cell(table.Rows.Count-1,6), MfilesAliasConfig.UgHeadquartersGeneralManager);
                //  table.Cell(++rowindex, 6).Range.Text = GetManager(MfilesAliasConfig.UgHeadquartersGeneralManager, vault);
                table.Cell(table.Rows.Count, 6).Range.Text = PrintDateNow();

                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("FillApprovedInfo" + ex.Message);
            }
        }
        public void FillAuditedInfo()
        {
            try
            {
                //var rowindex = table.Rows.Count - 2;
                //var therange = table.Cell(++rowindex, 4).Range;
                //therange.Text = "";
                //InsertSignature(therange, MfilesAliasConfig.UgHeadquartersEngineeringManager);
                table.Cell(table.Rows.Count - 1, 4).Range.Text = string.Empty;
                PictureOrLiteralSignature(table.Cell(table.Rows.Count - 1, 4), MfilesAliasConfig.UgHeadquartersEngineeringManager);
                table.Cell(table.Rows.Count, 4).Range.Text = PrintDateNow();

                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("FillAuditedInfo" + ex.Message);
            }
        }
        public void FillCreateInfo()
        {
            try
            {
                //var rowindex = table.Rows.Count - 2;
                //var creator =
                //    vault.ObjectPropertyOperations.GetProperty(objver,
                //        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).Value.GetLookupID();
                //var username = vault.UserOperations.GetUserAccount(creator).LoginName;
                //var therange = table.Cell(++rowindex, 2).Range;
                //therange.Text = "";
                //InsertSignatureByUsername(therange, username);
                //  table.Cell(++rowindex, 2).Range.Text = creator;
                table.Cell(table.Rows.Count - 1, 2).Range.Text = string.Empty;
                PictureOrLiteralSignatureBaseOnCreator(table.Cell(table.Rows.Count - 1, 2));
                table.Cell(table.Rows.Count, 2).Range.Text = PrintDateNow();

                table.Cell(5, 2).Range.Text = Project.PropProjName; //pvs.SearchForPropertyByAlias(vault, "PropProjName", true).GetValueAsLocalizedText();
                table.Cell(5, 4).Range.Text = Project.PropProjNum; //pvs.SearchForPropertyByAlias(vault, "PropProjNum", true).GetValueAsLocalizedText();

                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("FillCreateInfo" + ex.Message);
            }
        }

    }
}
