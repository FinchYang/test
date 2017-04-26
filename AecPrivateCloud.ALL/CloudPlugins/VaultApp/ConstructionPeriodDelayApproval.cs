using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AecCloud.MFilesCore;
using MFiles.VAF.Common;
using Microsoft.Office.Interop.Word;

namespace VaultApp
{
    class ConstructionPeriodDelayApproval : DocumentOperation
    {
        public ConstructionPeriodDelayApproval(StateEnvironment stateEnvironment):base(stateEnvironment)
        {
            //  
        }

        public void SecondLevelCounterSignManagerDirectlyControl()
        {
            try
            {
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(11, 2), MfilesAliasConfig.UGroupPM, "项目经理：");
             //   CscecSignature(table.Cell(11, 2), MfilesAliasConfig.UGroupPM,"（盖章）", "项目经理：  ");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("SecondLevelCounterSignManagerDirectlyControl" + ex.Message);
            }
        }
        

        public void PushOtherDepartments()
        {
            try
            {
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(17, 2), MfilesAliasConfig.UgViceExecutive,  "副总经理（生产）：");
              //  CscecSignature(table.Cell(17, 2), MfilesAliasConfig.UgViceExecutive, "", "副总经理（生产）：  ");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("PushOtherDepartments" + ex.Message);
            }
        }
        public void CounterSignVicePresident()
        {
            try
            {
              //  CscecSignature(table.Cell(16, 2), MfilesAliasConfig.UgHeadquartersEngineeringManager, "", "经    理：  ");
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(16, 2), MfilesAliasConfig.UgHeadquartersEngineeringManager, "经    理：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("CounterSignVicePresident" + ex.Message);
            }
        }
        public void CounterSignConstructionManager()
        {
            try
            {
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(15, 2), MfilesAliasConfig.UgLegalServiceManager, "经    理：");
             //   CscecSignature(table.Cell(15, 2), MfilesAliasConfig.UgLegalServiceManager, "", "经    理：  ");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("CounterSignConstructionManager" + ex.Message);
            }
        }
        public void CounterSignContractLegalManager()
        {
            try
            {
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(14, 2), MfilesAliasConfig.UgSecondLevelManager,  "经    理：");
             //   CscecSignature(table.Cell(14, 2), MfilesAliasConfig.UgSecondLevelManager, "（二级单位盖章）", "经    理：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("CounterSignContractLegalManager" + ex.Message);
            }
        }
        public void CounterSignContractLegalManagerDirectlyControl()
        {
            try
            {
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(11, 2), MfilesAliasConfig.UGroupPM, "项目经理：");
              //  PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(14, 2), MfilesAliasConfig.UgSecondLevelManager, "经    理：");
                CloseWord();
                UpDateFile();
            }
          
            catch (Exception ex)
            {
                Writelog("CounterSignContractLegalManagerDirectlyControl" + ex.Message);
            }
        }
        public void SecondLevelCounterSignManager()
        {
            try
            {
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(13, 2), MfilesAliasConfig.UgSecondLevelDeputyManager,  "副经理（生产）：");
               
              //  CscecSignature(table.Cell(13, 2), MfilesAliasConfig.UgSecondLevelDeputyManager, "", "副经理（生产）：  ");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("SecondLevelCounterSignManager" + ex.Message);
            }
        }

     

     
        public void SecondLevelCounterSignVicePresident()
        {
            try
            {
              //  CscecSignature(table.Cell(12, 2), MfilesAliasConfig.UgSecondLevelChiefEconomist, "", "总经济师： ");
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(12, 2), MfilesAliasConfig.UgSecondLevelChiefEconomist,  "总经济师：");
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("SecondLevelCounterSignVicePresident" + ex.Message);
            }
        }
        public void SecondLevelChiefEconomistCounterSign()
        {
            try
            {
              //  CscecSignature(table.Cell(11, 2), MfilesAliasConfig.UGroupPM, "（盖章）", "项目经理：  ");
                PictureOrLiteralSignatureIncludeDateByAlias(table.Cell(11, 2), MfilesAliasConfig.UGroupPM,  "项目经理：");
              
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("SecondLevelChiefEconomistCounterSign" + ex.Message);
            }
        }

    

      
        public void WfsCreateConstructionPeriodDelayApproval()
        {
            try
            {
                table.Cell(4, 2).Range.Text = Project.PropProjName + Environment.NewLine
                + Project.PropProjNum;
                table.Cell(6, 3).Range.Text = Project.PropStartDate;
                table.Cell(6, 5).Range.Text = Project.Deadline;

                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("WfsCreateConstructionPeriodDelayApproval" + ex.Message);
            }
        }
    }
}
