using System;
using System.Collections.Generic;
using System.Globalization;
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
    class MonthlyEvaluation:DocumentOperation
    {
      
        private bool import = false;
        private string PropTeamBuildingOpinion,
            PropTeamBuildingScore,
            PropContractAdministrationOpinion,
            PropContractAdministrationScore,
            PropCostManagementOpinion,
            PropCostManagementScore,
            PropProgressContractorPlanOpinion,
            PropProgressContractorPlanScore,
            PropSecureEnvironmentalManagementOpinion,
            PropSecureEnvironmentalManagementScore,
            PropMaterialManagementOpinion,
            PropMaterialManagementScore,
            PropInformatizationManagementOpinion,
            PropInformatizationManagementScore,
            PropQualityTechnologyManagementOpinion,
            PropQualityTechnologyManagementScore,
            PropFundsManagementOpinion,
            PropFundsManagementScore, 
            PropGreenConstructionManagementScore;
        public MonthlyEvaluation(StateEnvironment stateEnvironment, bool oneandtwo = true, bool SecondLevel=true)
            : base(stateEnvironment)//
        {
            vault = stateEnvironment.Vault;
            objver = stateEnvironment.ObjVer;
            pvs = vault.ObjectPropertyOperations.GetProperties(objver);
            if (oneandtwo)
            {
                if (SecondLevel)
                {
                    try
                    {
                        PropTeamBuildingOpinion =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropTeamBuildingOpinionSecondLevel,
                                true).GetValueAsLocalizedText();
                        PropTeamBuildingScore =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropTeamBuildingScoreSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropContractAdministrationOpinion =
                            pvs.SearchForPropertyByAlias(vault,
                                MfilesAliasConfig.PropContractAdministrationOpinionSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropContractAdministrationScore =
                            pvs.SearchForPropertyByAlias(vault,
                                MfilesAliasConfig.PropContractAdministrationScoreSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropCostManagementOpinion =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropCostManagementOpinionSecondLevel,
                                true).GetValueAsLocalizedText();
                        PropCostManagementScore =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropCostManagementScoreSecondLevel,
                                true).GetValueAsLocalizedText();
                        PropProgressContractorPlanOpinion =
                            pvs.SearchForPropertyByAlias(vault,
                                MfilesAliasConfig.PropProgressContractorPlanOpinionSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropProgressContractorPlanScore =
                            pvs.SearchForPropertyByAlias(vault,
                                MfilesAliasConfig.PropProgressContractorPlanScoreSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropSecureEnvironmentalManagementOpinion =
                            pvs.SearchForPropertyByAlias(vault,
                                MfilesAliasConfig.PropSecureEnvironmentalManagementOpinionSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropSecureEnvironmentalManagementScore =
                            pvs.SearchForPropertyByAlias(vault,
                                MfilesAliasConfig.PropSecureEnvironmentalManagementScoreSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropMaterialManagementOpinion =
                            pvs.SearchForPropertyByAlias(vault,
                                MfilesAliasConfig.PropMaterialManagementOpinionSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropMaterialManagementScore =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropMaterialManagementScoreSecondLevel,
                                true).GetValueAsLocalizedText();
                        PropInformatizationManagementOpinion =
                            pvs.SearchForPropertyByAlias(vault,
                                MfilesAliasConfig.PropInformatizationManagementOpinionSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropInformatizationManagementScore =
                            pvs.SearchForPropertyByAlias(vault,
                                MfilesAliasConfig.PropInformatizationManagementScoreSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropQualityTechnologyManagementOpinion =
                            pvs.SearchForPropertyByAlias(vault,
                                MfilesAliasConfig.PropQualityTechnologyManagementOpinionSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropQualityTechnologyManagementScore =
                            pvs.SearchForPropertyByAlias(vault,
                                MfilesAliasConfig.PropQualityTechnologyManagementScoreSecondLevel, true)
                                .GetValueAsLocalizedText();
                        PropFundsManagementOpinion =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropFundsManagementOpinionSecondLevel,
                                true).GetValueAsLocalizedText();
                        PropFundsManagementScore =
                            pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropFundsManagementScoreSecondLevel,
                                true).GetValueAsLocalizedText();
                    }
                    catch (Exception ex)
                    {
                        Writelog("secondlevel property processing "+ex.Message);
                    }
                }
                else
                {
                    PropTeamBuildingOpinion = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropTeamBuildingOpinion, true).GetValueAsLocalizedText();
                    PropTeamBuildingScore = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropTeamBuildingScore, true).GetValueAsLocalizedText();
                    PropContractAdministrationOpinion = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropContractAdministrationOpinion, true).GetValueAsLocalizedText();
                    PropContractAdministrationScore = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropContractAdministrationScore, true).GetValueAsLocalizedText();
                    PropCostManagementOpinion = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropCostManagementOpinion, true).GetValueAsLocalizedText();
                    PropCostManagementScore = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropCostManagementScore, true).GetValueAsLocalizedText();
                    PropProgressContractorPlanOpinion = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropProgressContractorPlanOpinion, true).GetValueAsLocalizedText();
                    PropProgressContractorPlanScore = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropProgressContractorPlanScore, true).GetValueAsLocalizedText();
                    PropSecureEnvironmentalManagementOpinion = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropSecureEnvironmentalManagementOpinion, true).GetValueAsLocalizedText();
                    PropSecureEnvironmentalManagementScore = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropSecureEnvironmentalManagementScore, true).GetValueAsLocalizedText();
                    PropMaterialManagementOpinion = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropMaterialManagementOpinion, true).GetValueAsLocalizedText();
                    PropMaterialManagementScore = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropMaterialManagementScore, true).GetValueAsLocalizedText();
                    PropInformatizationManagementOpinion = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropInformatizationManagementOpinion, true).GetValueAsLocalizedText();
                    PropInformatizationManagementScore = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropInformatizationManagementScore, true).GetValueAsLocalizedText();
                    PropQualityTechnologyManagementOpinion = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropQualityTechnologyManagementOpinion, true).GetValueAsLocalizedText();
                    PropQualityTechnologyManagementScore = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropQualityTechnologyManagementScore, true).GetValueAsLocalizedText();
                    PropFundsManagementOpinion = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropFundsManagementOpinion, true).GetValueAsLocalizedText();
                    PropFundsManagementScore = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropFundsManagementScore, true).GetValueAsLocalizedText();
                }
             

                var ClassMonthlyEvaluationSecondLevel =
                    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemClass,
                        MfilesAliasConfig.ClassMonthlyEvaluationSecondLevelImportant);
                var ClassMonthlyEvaluationDirectlyControl =
                   vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemClass,
                       MfilesAliasConfig.ClassMonthlyEvaluationDirectlyControlImportant);
                var objectclass =
                    pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass).Value.GetLookupID();

                if (objectclass == ClassMonthlyEvaluationDirectlyControl || objectclass == ClassMonthlyEvaluationSecondLevel)
                {
                    import = true;
                     if (SecondLevel)
                {
                    try
                    {
                        PropGreenConstructionManagementScore = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropGreenConstructionManagementScoreSecondLevel, true).GetValueAsLocalizedText();
                    }
                    catch (Exception ex)
                    {
                        Writelog("secondlevel property PropGreenConstructionManagementScoreSecondLevel processing " + ex.Message);
                    }
                }
                     else PropGreenConstructionManagementScore = pvs.SearchForPropertyByAlias(vault, MfilesAliasConfig.PropGreenConstructionManagementScore, true).GetValueAsLocalizedText();
                }
            }
        }
        public bool FillDocDirectlyControl(StateEnvironment stateEnvironment)
        {
            try
            {
                DirectlyTablePartOne();

                CunterMark();
                if (import)
                    TablePartTwo();

                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog(ex.Message);
                return false;
            }
            return true;
        }


        private void TablePartTwo()
        {
            var rowindex = 19;
            table.Cell(rowindex, 3).Range.Text = PropTeamBuildingScore;

            var subscore1 = (GetActualScore(PropTeamBuildingScore) * 0.05);
            table.Cell(rowindex++, 5).Range.Text = subscore1.ToString(CultureInfo.InvariantCulture);

            table.Cell(rowindex, 3).Range.Text = PropContractAdministrationScore;
            var subscore2 = (GetActualScore(PropContractAdministrationScore) * 0.05);
            table.Cell(rowindex++, 5).Range.Text = subscore2.ToString(CultureInfo.InvariantCulture);
            table.Cell(rowindex, 3).Range.Text = PropCostManagementScore;
            var subscore3 = (GetActualScore(PropCostManagementScore) * 0.15);
            table.Cell(rowindex++, 5).Range.Text = subscore3.ToString(CultureInfo.InvariantCulture);
            table.Cell(rowindex, 3).Range.Text = PropProgressContractorPlanScore;
            var subscore4 = (GetActualScore(PropProgressContractorPlanScore) * 0.2);
            table.Cell(rowindex++, 5).Range.Text = subscore4.ToString(CultureInfo.InvariantCulture);
            table.Cell(rowindex, 3).Range.Text = PropGreenConstructionManagementScore;
            var subscore5 = (GetActualScore(PropGreenConstructionManagementScore) * 0.1);
            table.Cell(rowindex++, 5).Range.Text = subscore5.ToString(CultureInfo.InvariantCulture);
            table.Cell(rowindex, 3).Range.Text = PropSecureEnvironmentalManagementScore;
            var subscore6 = (GetActualScore(PropSecureEnvironmentalManagementScore) * 0.1);
            table.Cell(rowindex++, 5).Range.Text = subscore6.ToString(CultureInfo.InvariantCulture);
            table.Cell(rowindex, 3).Range.Text = PropMaterialManagementScore;
            var subscore7 = (GetActualScore(PropMaterialManagementScore) * 0.1);
            table.Cell(rowindex++, 5).Range.Text = subscore7.ToString(CultureInfo.InvariantCulture);
            table.Cell(rowindex, 3).Range.Text = PropQualityTechnologyManagementScore;
            var subscore8 = (GetActualScore(PropQualityTechnologyManagementScore) * 0.2);
            table.Cell(rowindex++, 5).Range.Text = subscore8.ToString(CultureInfo.InvariantCulture);
            table.Cell(rowindex, 3).Range.Text = PropInformatizationManagementScore;
            var subscore9 = (GetActualScore(PropInformatizationManagementScore) * 0.05);
            table.Cell(rowindex++, 5).Range.Text = subscore9.ToString(CultureInfo.InvariantCulture);
            table.Cell(rowindex, 3).Range.Text = PropFundsManagementScore;
            var subscore10 = (GetActualScore(PropFundsManagementScore) * 0.05);
            table.Cell(rowindex, 5).Range.Text = subscore10.ToString(CultureInfo.InvariantCulture);
            var netscore = subscore1 + subscore2 + subscore3 + subscore4 + subscore5 + subscore6 + subscore7 + subscore8 +
                           subscore9 + subscore10;

            table.Cell(29, 5).Range.Text = netscore.ToString(CultureInfo.InvariantCulture);
        }

        public void DirectlyTablePartThree()
        {
            //var rowindex = 29;
            //CscecSignatureByAliasNoDate(table.Cell(++rowindex, 1), MfilesAliasConfig.UgBigChiefEngineer, "公司总工程师：");
            //CscecSignatureByAliasNoDate(table.Cell(++rowindex, 1), MfilesAliasConfig.UgChiefEconomist, "公司总经济师：");
            //CscecSignatureByAliasNoDate(table.Cell(++rowindex, 1), MfilesAliasConfig.UgChiefAccountant, "公司总会计师：");
            MEPictureOrLiteralSignature(table.Cell(table.Rows.Count - 3, 1), MfilesAliasConfig.UgBigChiefEngineer, "公司总工程师：");
            MEPictureOrLiteralSignature(table.Cell(table.Rows.Count - 2, 1), MfilesAliasConfig.UgChiefEconomist, "公司总经计师：");
            MEPictureOrLiteralSignature(table.Cell(table.Rows.Count - 1, 1), MfilesAliasConfig.UgChiefAccountant, "公司总会计师：");
         //   CscecSignature(table.Cell(++rowindex, 1), MfilesAliasConfig.UgViceExecutive, "公司分管副总经理（生产）：", " ");
            PictureOrLiteralSignatureIncludeDate(table.Cell(table.Rows.Count, 1), MfilesAliasConfig.UgViceExecutive,  "公司分管副总经理（生产）：", "日期：");
            //table.Cell(++rowindex, 1).Range.Text = "公司总工程师：" + GetManager(MfilesAliasConfig.UgBigChiefEngineer, vault);
            //table.Cell(++rowindex, 1).Range.Text = "公司总经济师：" + GetManager(MfilesAliasConfig.UgChiefEconomist, vault);
            //table.Cell(++rowindex, 1).Range.Text = "公司总会计师：" + GetManager(MfilesAliasConfig.UgChiefAccountant, vault);
            //table.Cell(++rowindex, 1).Range.Text = "公司分管副总经理（生产）：" + GetManager(MfilesAliasConfig.UgViceExecutive, vault)
            //    + Environment.NewLine + "日期：" + pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified).GetValueAsLocalizedText();
            CloseWord();
            UpDateFile();
        }
        public void SecondLevelTablePartThree()
        {
            try
            {
             //   var rowindex = 29;
             //   CscecSignatureByAliasNoDate(table.Cell(++rowindex, 1), MfilesAliasConfig.UgSecondLevelChiefEngineer, "二级单位总工程师：");
                MEPictureOrLiteralSignature(table.Cell(table.Rows.Count - 3, 1), MfilesAliasConfig.UgSecondLevelChiefEngineer, "二级单位总工程师：");
                MEPictureOrLiteralSignature(table.Cell(table.Rows.Count - 2, 1), MfilesAliasConfig.UgSecondLevelChiefEconomist, "二级单位总经计师：");
              //  CscecSignatureByAliasNoDate(table.Cell(++rowindex, 1), MfilesAliasConfig.UgSecondLevelChiefEconomist, "二级单位总经济师：");
            //    CscecSignatureByAliasNoDate(table.Cell(++rowindex, 1), MfilesAliasConfig.UgSecondLevelChiefAccountant, "二级单位总会计师：");
                MEPictureOrLiteralSignature(table.Cell(table.Rows.Count - 1, 1), MfilesAliasConfig.UgSecondLevelChiefAccountant, "二级单位总会计师：");
              //  CscecSignature(table.Cell(++rowindex, 1), MfilesAliasConfig.UgSecondLevelDeputyManager, "二级单位分管副总经理（生产）：", " ");
                PictureOrLiteralSignatureIncludeDate(table.Cell(table.Rows.Count, 1), MfilesAliasConfig.UgSecondLevelDeputyManager,  "二级单位分管副总经理（生产）：", "日期：");
                //table.Cell(++rowindex, 1).Range.Text = "二级单位总工程师：" + GetManager(MfilesAliasConfig.UgSecondLevelChiefEngineer, vault);
                //table.Cell(++rowindex, 1).Range.Text = "二级单位总经济师：" + GetManager(MfilesAliasConfig.UgSecondLevelChiefEconomist, vault);
                //table.Cell(++rowindex, 1).Range.Text = "二级单位总会计师：" + GetManager(MfilesAliasConfig.UgSecondLevelChiefAccountant, vault);
                //table.Cell(++rowindex, 1).Range.Text = "二级单位分管副总经理（生产）：" + GetManager(MfilesAliasConfig.UgSecondLevelDeputyManager, vault)
                //    + Environment.NewLine + "日期：" + pvs.SearchForProperty((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified).GetValueAsLocalizedText();
                CloseWord();
                UpDateFile();
            }
            catch (Exception ex)
            {
               Writelog("SecondLevelTablePartThree:"+ex.Message);
            }
         
        }
        public void MECscecSignatureByAliasNoDate(Cell cell, string ugalias, string line2string, string anchor)
        {
            try
            {
                cell.Range.Text = line2string + anchor;
                MEPictureOrLiteralSignature(cell, ugalias, anchor);
            }
            catch (Exception ex)
            {
                Writelog(string.Format("MECscecSignatureByAliasNoDate error:{0},{1},{2},{3}", ugalias, anchor, line2string,ex.Message));
            }
        }
        public void MEPictureOrLiteralSignature(Cell cell, string ugAlias, string literalAnchor)
        {
            SignatureResource sr = GetSignatureResource(ugAlias);
            if (sr.IsFile)
            {
                InsertPictureSignature(cell, literalAnchor, sr.ContentText, false);
            }
            else
            {
                MEInsertLiteralSignature(cell, sr.ContentText, literalAnchor);
            }
        }
     
        private void DirectlyTablePartOne()
        {
            var rowindex = 8;
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgHumenResourceManager, "意见：" + PropTeamBuildingOpinion + Environment.NewLine + GetScore(PropTeamBuildingScore), "人力资源部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgLegalServiceManager, "意见：" + PropContractAdministrationOpinion + Environment.NewLine + GetScore(PropContractAdministrationScore), "合约法务部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgBusinessManager, "意见：" + PropCostManagementOpinion + Environment.NewLine + GetScore(PropCostManagementScore), "商务管理部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgHeadquartersEngineeringManager, "意见：" + PropProgressContractorPlanOpinion + Environment.NewLine + GetScore(PropProgressContractorPlanScore), "工程管理部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgSafetyProductionManagementDeptManager, "意见：" + PropSecureEnvironmentalManagementOpinion + Environment.NewLine + GetScore(PropSecureEnvironmentalManagementScore), "安全生产管理部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgMaterialManager, "意见：" + PropMaterialManagementOpinion + Environment.NewLine + GetScore(PropMaterialManagementScore), "物资部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgHeadquartersScienceManager, "意见：" + PropQualityTechnologyManagementOpinion + Environment.NewLine + GetScore(PropQualityTechnologyManagementScore), "科技部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgHeadquartersTechCenterManager, "意见：" + PropInformatizationManagementOpinion + Environment.NewLine + GetScore(PropInformatizationManagementScore), "技术中心：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgHeadquartersFundsManager, "意见：" + PropFundsManagementOpinion + Environment.NewLine + GetScore(PropFundsManagementScore), "投资与资金部：");
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropTeamBuildingOpinion + Environment.NewLine + GetScore(PropTeamBuildingScore) + "人力资源部：" + GetManager(MfilesAliasConfig.UgHumenResourceManager, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropContractAdministrationOpinion + Environment.NewLine + GetScore(PropContractAdministrationScore) + "合约法务部：" + GetManager(MfilesAliasConfig.UgLegalServiceManager, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropCostManagementOpinion + Environment.NewLine + GetScore(PropCostManagementScore) + "商务管理部：" + GetManager(MfilesAliasConfig.UgBusinessManagementManager, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropProgressContractorPlanOpinion + Environment.NewLine + GetScore(PropProgressContractorPlanScore) + "工程管理部：" + GetManager(MfilesAliasConfig.UgHeadquartersEngineeringManagementDepartment, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropSecureEnvironmentalManagementOpinion + Environment.NewLine + GetScore(PropSecureEnvironmentalManagementScore) + "安全生产管理部：" + GetManager(MfilesAliasConfig.UgSafetyProductionManagementDepartment, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropMaterialManagementOpinion + Environment.NewLine + GetScore(PropMaterialManagementScore) + "物资部：" + GetManager(MfilesAliasConfig.UgMaterialManager, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropQualityTechnologyManagementOpinion + Environment.NewLine + GetScore(PropQualityTechnologyManagementScore) + "科技部：" + GetManager(MfilesAliasConfig.UgHeadquartersScienceManager, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropInformatizationManagementOpinion + Environment.NewLine + GetScore(PropInformatizationManagementScore) + "技术中心：" + GetManager(MfilesAliasConfig.UgHeadquartersTechCenterManager, vault);
            //table.Cell(rowindex, 2).Range.Text = "意见：" + PropFundsManagementOpinion + Environment.NewLine + GetScore(PropFundsManagementScore) + "投资与资金部：" + GetManager(MfilesAliasConfig.UgHeadquartersFundsManager, vault);

        }
        public bool FillDocSecondLevel(StateEnvironment stateEnvironment)
        {
            try
            {
                SecondLevelTablePartOne();

                CunterMark();
                if (import) TablePartTwo();
                CloseWord();

                UpDateFile();
            }
            catch (Exception ex)
            {
                Writelog("FillDocSecondLevel " + ex.Message);
                return false;
            }
            return true;
        }

      

        private void SecondLevelTablePartOne()
        {
            var rowindex = 8;
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropTeamBuildingOpinion + Environment.NewLine + GetScore(PropTeamBuildingScore) + "办公室：" + GetManager(MfilesAliasConfig.UgOfficeManager, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropContractAdministrationOpinion + Environment.NewLine + GetScore(PropContractAdministrationScore) + "商务部：" + GetManager(MfilesAliasConfig.UgBusinessManager, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropCostManagementOpinion + Environment.NewLine + GetScore(PropCostManagementScore) + "商务部：" + GetManager(MfilesAliasConfig.UgBusinessManager, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropProgressContractorPlanOpinion + Environment.NewLine + GetScore(PropProgressContractorPlanScore) + "施工管理部：" + GetManager(MfilesAliasConfig.UgConstructionManager, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropSecureEnvironmentalManagementOpinion + Environment.NewLine + GetScore(PropSecureEnvironmentalManagementScore) + "施工管理部：" + GetManager(MfilesAliasConfig.UgConstructionManager, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropMaterialManagementOpinion + Environment.NewLine + GetScore(PropMaterialManagementScore) + "物资部：" + GetManager(MfilesAliasConfig.UgMaterialManagerSecondLevel, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropQualityTechnologyManagementOpinion + Environment.NewLine + GetScore(PropQualityTechnologyManagementScore) + "施工管理部：" + GetManager(MfilesAliasConfig.UgConstructionManager, vault);
            //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropInformatizationManagementOpinion + Environment.NewLine + GetScore(PropInformatizationManagementScore) + "施工管理部：" + GetManager(MfilesAliasConfig.UgConstructionManager, vault);
            //table.Cell(rowindex, 2).Range.Text = "意见：" + PropFundsManagementOpinion + Environment.NewLine + GetScore(PropFundsManagementScore) + "财务部：" + GetManager(MfilesAliasConfig.UgFinanceManager, vault);
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgOfficeManagerSecondLevel, "意见：" + PropTeamBuildingOpinion + Environment.NewLine + GetScore(PropTeamBuildingScore), 
                "办公室：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgSecondLevelBusinessManager, "意见：" + PropContractAdministrationOpinion + Environment.NewLine + GetScore(PropContractAdministrationScore),
                "商务部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgSecondLevelBusinessManager, "意见：" + PropCostManagementOpinion + Environment.NewLine + GetScore(PropCostManagementScore),
                "商务部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgEngineeringManagerSecond, "意见：" + PropProgressContractorPlanOpinion + Environment.NewLine + GetScore(PropProgressContractorPlanScore),
                "施工管理部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgEngineeringManagerSecond, "意见：" + PropSecureEnvironmentalManagementOpinion + Environment.NewLine + GetScore(PropSecureEnvironmentalManagementScore), 
                "施工管理部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgMaterialManagerSecondLevel, "意见：" + PropMaterialManagementOpinion + Environment.NewLine + GetScore(PropMaterialManagementScore),
                "物资部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgEngineeringManagerSecond, "意见：" + PropQualityTechnologyManagementOpinion + Environment.NewLine + GetScore(PropQualityTechnologyManagementScore),
                "施工管理部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgEngineeringManagerSecond, "意见：" + PropInformatizationManagementOpinion + Environment.NewLine + GetScore(PropInformatizationManagementScore), 
                "施工管理部：");
            MECscecSignatureByAliasNoDate(table.Cell(rowindex++, 2), MfilesAliasConfig.UgSecondLevelFinanceManager, "意见：" + PropFundsManagementOpinion + Environment.NewLine + GetScore(PropFundsManagementScore),
                "财务部：");
          
        }
        private static int GetActualScore(string score)
        {
            var ret = 0;

            switch (score)
            {
                case "90分":
                    ret = 90;
                    break;
                case "80分":
                    ret = 80;
                    break;
                case "70分":
                    ret = 70;
                    break;
                case "60分":
                    ret = 60;
                    break;
                case "100分":
                    ret = 100;
                    break;
                default:
                    ret = 60;
                    break;
            }
            return ret;
        }

        //private static string GetManager(string ugalias, Vault vault)
        //{
        //    var ug = vault.UserGroupOperations.GetUserGroupIDByAlias(ugalias);
        //    var members = vault.UserGroupOperations.GetUserGroup(ug).Members;
        //    var ret = string.Empty;
        //    foreach (var member in members)
        //    {
        //        ret = vault.UserOperations.GetLoginAccountOfUser((int)member).FullName;
        //        break;
        //    }
        //    return ret;
        //}

        //public bool FillDocDirectlyControlNotImportant(StateEnvironment stateEnvironment)
        //{
        //    try
        //    {

        //        //var rowindex = 8;
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropTeamBuildingOpinion + Environment.NewLine + GetScore(PropTeamBuildingScore) + "人力资源部：" + GetManager("UgHumenResourceManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropContractAdministrationOpinion + Environment.NewLine + GetScore(PropContractAdministrationScore) + "合约法务部：" + GetManager("UgLegalServiceManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropCostManagementOpinion + Environment.NewLine + GetScore(PropCostManagementScore) + "商务管理部：" + GetManager("UgBusinessManagementManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropProgressContractorPlanOpinion + Environment.NewLine + GetScore(PropProgressContractorPlanScore) + "工程管理部：" + GetManager("UgHeadquartersEngineeringManagementDepartment", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropSecureEnvironmentalManagementOpinion + Environment.NewLine + GetScore(PropSecureEnvironmentalManagementScore) + "安全生产管理部：" + GetManager("UgSafetyProductionManagementDepartment", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropMaterialManagementOpinion + Environment.NewLine + GetScore(PropMaterialManagementScore) + "物资部：" + GetManager("UgMaterialManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropQualityTechnologyManagementOpinion + Environment.NewLine + GetScore(PropQualityTechnologyManagementScore) + "科技部：" + GetManager("UgHeadquartersScienceManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropInformatizationManagementOpinion + Environment.NewLine + GetScore(PropInformatizationManagementScore) + "技术中心：" + GetManager("UgHeadquartersTechCenterManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropFundsManagementOpinion + Environment.NewLine + GetScore(PropFundsManagementScore) + "投资与资金部：" + GetManager("UgHeadquartersFundsManager", vault);
        //        //Writelog("88");
        //        DirectlyTablePartOne(table, vault);
        //        CunterMark(app);

        //        doc.Save();
        //        doc.Close();
        //        app.Quit();
        //        UpDateFile();
        //    }
        //    catch (Exception ex)
        //    {
        //        Writelog(ex.Message);
        //        return false;
        //    }
        //    return true;
        //}
        //public bool FillDocSecondLevelNotImportant(StateEnvironment stateEnvironment)
        //{
        //    try
        //    {

        //        //var rowindex = 8;
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropTeamBuildingOpinion + Environment.NewLine + GetScore(PropTeamBuildingScore) + "办公室：" + GetManager("UgOfficeManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropContractAdministrationOpinion + Environment.NewLine + GetScore(PropContractAdministrationScore) + "商务部：" + GetManager("UgBusinessManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropCostManagementOpinion + Environment.NewLine + GetScore(PropCostManagementScore) + "商务部：" + GetManager("UgBusinessManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropProgressContractorPlanOpinion + Environment.NewLine + GetScore(PropProgressContractorPlanScore) + "施工管理部：" + GetManager("UgConstructionManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropSecureEnvironmentalManagementOpinion + Environment.NewLine + GetScore(PropSecureEnvironmentalManagementScore) + "施工管理部：" + GetManager("UgConstructionManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropMaterialManagementOpinion + Environment.NewLine + GetScore(PropMaterialManagementScore) + "物资部：" + GetManager("UgMaterialManagerSecondLevel", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropQualityTechnologyManagementOpinion + Environment.NewLine + GetScore(PropQualityTechnologyManagementScore) + "施工管理部：" + GetManager("UgConstructionManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropInformatizationManagementOpinion + Environment.NewLine + GetScore(PropInformatizationManagementScore) + "施工管理部：" + GetManager("UgConstructionManager", vault);
        //        //table.Cell(rowindex++, 2).Range.Text = "意见：" + PropFundsManagementOpinion + Environment.NewLine + GetScore(PropFundsManagementScore) + "财务部：" + GetManager("UgFinanceManager", vault);
        //        //Writelog("88");
        //        SecondLevelTablePartOne();
        //        CunterMark(app);

        //        Writelog("88 11");

        //        doc.Save();
        //        doc.Close();
        //        app.Quit();
        //        UpDateFile();
        //    }
        //    catch (Exception ex)
        //    {
        //        Writelog(ex.Message);
        //        return false;
        //    }
        //    return true;
        //}

        private  void CunterMark()
        {
            doc.Tables[1].Select();
            app.Selection.Find.Replacement.ClearFormatting();
            app.Selection.Find.ClearFormatting();
            app.Selection.Find.Text = "{cuntermark}";//需要被替换的文本

            app.Selection.Find.Replacement.Font.Name = "Wingdings 2";//这里设置字体类型
            app.Selection.Find.Replacement.Text = "\u0052";

            object replace = WdReplace.wdReplaceAll;
            object oMissing = Missing.Value;
            //执行替换操作
            app.Selection.Find.Execute(
            ref oMissing, ref oMissing,
            ref oMissing, ref oMissing,
            ref oMissing, ref oMissing,
            ref oMissing, ref oMissing, ref oMissing,
            ref oMissing, ref replace,
            ref oMissing, ref oMissing,
            ref oMissing, ref oMissing);
        }

        private static string GetScore(string score)
        {
            string ret;
            const string cuntermark = "{cuntermark}";
            switch (score)
            {
                case "90分":
                    ret = string.Format("100分□ 90分{0} 80分□ 70分□ 60分□ 60分以下□ ", cuntermark);
                    break;
                case "80分":
                    ret = string.Format("100分□ 90分□ 80分{0} 70分□ 60分□ 60分以下□ ", cuntermark);
                    break;
                case "70分":
                    ret = string.Format("100分□ 90分□ 80分□ 70分{0} 60分□ 60分以下□ ", cuntermark);
                    break;
                case "60分":
                    ret = string.Format("100分□ 90分□ 80分□ 70分□ 60分{0} 60分以下□ ", cuntermark);
                    break;
                case "100分":
                    ret = string.Format("100分{0} 90分□ 80分□ 70分□ 60分□ 60分以下□ ", cuntermark);
                    break;
                default:
                    ret = string.Format("100分□ 90分□ 80分□ 70分□ 60分□ 60分以下{0} ", cuntermark);
                    break;
            }
            return ret;
        }
      
    }
}
