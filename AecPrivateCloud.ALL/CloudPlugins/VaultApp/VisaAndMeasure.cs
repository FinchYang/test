using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.MFilesCore;
using MFiles.VAF.Common;
using MFilesAPI;

namespace VaultApp
{
    class VisaAndMeasure : DocumentOperation
    {
        public VisaAndMeasure(StateEnvironment stateEnvironment)
            : base(stateEnvironment)
        {
            //  S.VAM.CounterSign.SecondLevelEnd
        }
        public void EndDirectlyControl()
        {
            try
            {
                var count = table.Rows.Count;
               // Writelog("VisaAndMeasure VAMCounterSignSecondLevelEnd-count:" + count);
               // table.Cell(count - 1, 3).Range.Text = "经理： " + GetManager(MfilesAliasConfig.UgBusinessManager, vault) + PrintDateNow();
              //  CscecSignature(table.Cell(count - 1, 3), MfilesAliasConfig.UgBusinessManager, "", "经理：  ");
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(count - 1, 3), MfilesAliasConfig.UgBusinessManager,  "经理：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("VisaAndMeasure VAMCounterSignSecondLevelEnd" + ex.Message);
            }
        }

        public void BusinessDirectlyControl()
        {
            try
            {
                var count = table.Rows.Count;
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(count - 2, 2), MfilesAliasConfig.UGroupPM,  "项目经理：");
              //  CscecSignature(table.Cell(count - 2, 2), MfilesAliasConfig.UGroupPM, "", "项目经理：  ");
               // table.Cell(count - 2, 2).Range.Text = "项目经理： " + GetManager(MfilesAliasConfig.UGroupPM, vault) + PrintDateNow();

                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("VisaAndMeasure VAMCounterSignSecondLevelBusiness" + ex.Message);
            }
        }
        public void CreateDirectlyControl()
        {
            try
            {
                table.Cell(5, 2).Range.Text = Project.PropProjName + Project.PropProjNum;
                //var creator =
                //    vault.ObjectPropertyOperations.GetProperty(objver,
                //        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).Value.GetLookupID();
                var count = table.Rows.Count;
                //var username = vault.UserOperations.GetUserAccount(creator).LoginName;
                //CscecSignatureByUsername(table.Cell(count - 3, 2), username, "", "商务经理：  ");
                PictureOrLiteralSignatureIncludeDateBaseOnCreator(table.Cell(count - 3, 2),   "商务经理：");
             //   table.Cell(count - 3, 2).Range.Text = "商务经理： " + creator + PrintDateNow();

                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("VisaAndMeasure Create" + ex.Message);
            }
        }
        public void SecondLevelEnd()
        {
            try
            {
                var count = table.Rows.Count;
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(count - 1, 2), MfilesAliasConfig.UgSecondLevelChiefEconomist,  "总经济师：");
                //Writelog("VisaAndMeasure VAMCounterSignSecondLevelEnd-count:" + count);
                //table.Cell(count - 1, 2).Range.Text = "总经济师： " + GetManager(MfilesAliasConfig.UgSecondLevelChiefEconomist, vault) + PrintDateNow();
              //  CscecSignature(table.Cell(count - 1, 2), MfilesAliasConfig.UgSecondLevelChiefEconomist, "", "总经济师：  ");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("VisaAndMeasure VAMCounterSignSecondLevelEnd" + ex.Message);
            }
        }
        public void VAMCounterSignSecondLevelChiefEconomist()
        {
            try
            {
                var count = table.Rows.Count;
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(count - 2, 2), MfilesAliasConfig.UgSecondLevelBusinessManager, "经    理：");
                //Writelog("VisaAndMeasure VAMCounterSignSecondLevelChiefEconomist-count:" + count);
                //table.Cell(count - 2, 2).Range.Text = "经  理： " + GetManager(MfilesAliasConfig.UgSecondLevelBusinessManager, vault) + PrintDateNow();
              //  CscecSignature(table.Cell(count - 2, 2), MfilesAliasConfig.UgSecondLevelBusinessManager, "", "经  理：  ");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("VisaAndMeasure VAMCounterSignSecondLevelChiefEconomist" + ex.Message);
            }
        }
        public void VAMCounterSignSecondLevelBusiness()
        {
            try
            {
                var count = table.Rows.Count;
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(count - 3, 2), MfilesAliasConfig.UGroupPM,  "项目经理：");
                //Writelog("VisaAndMeasure Create-count:" + count);
                //table.Cell(count - 3, 2).Range.Text = "项目经理： " + GetManager(MfilesAliasConfig.UGroupPM, vault) + PrintDateNow();
              //  CscecSignature(table.Cell(count - 3, 2), MfilesAliasConfig.UGroupPM, "", "项目经理：  ");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("VisaAndMeasure VAMCounterSignSecondLevelBusiness" + ex.Message);
            }
        }
        public void Create()
        {
            try
            {
                table.Cell(5, 2).Range.Text = Project.PropProjName + Project.PropProjNum;
                //var creator =
                //    vault.ObjectPropertyOperations.GetProperty(objver,
                //        (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy).Value.GetLookupID();
                var count = table.Rows.Count;
                //Writelog("VisaAndMeasure Create-count:" + count);
                //table.Cell(count - 4, 2).Range.Text = "商务经理： " + creator + PrintDateNow();
                //var username = vault.UserOperations.GetUserAccount(creator).LoginName;
                //CscecSignatureByUsername(table.Cell(count - 4, 2), username, "", "商务经理：  ");

                PictureOrLiteralSignatureIncludeDateBaseOnCreator(table.Cell(count - 4, 2),  "商务经理：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("VisaAndMeasure Create" + ex.Message);
            }
        }
    }
}
