using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MFilesAPI;
namespace CreateMfilesAllBackupTemplate
{
    public partial class Form1 : Form
    {
        private MFilesServerApplication app;
        public Form1()
        {
            InitializeComponent();
            app = new MFilesServerApplication();
            var conn = app.Connect( MFAuthType.MFAuthTypeSpecificMFilesUser,"admin","111111");
            var vaults = app.GetOnlineVaults();
            foreach (VaultOnServer vaultOnServer in vaults)
            {
                comboBox1.Items.Add(vaultOnServer.GUID + vaultOnServer.Name);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var vaultguid = comboBox1.Text.Substring(0, 38);
            var vault = app.LogInToVault(vaultguid);
            if (RemoveGarbageData(vault)) return;
            try
            {
                var bu = new BackupJob
                {
                    VaultGUID = vaultguid,
                    BackupType = MFBackupType.MFBackupTypeFull,
                    OverwriteExistingFiles = true,
                    TargetFile = @"C:\privatecloud\0installersource\templates\fullbackup\cscec8.mfb"
                };
                app.VaultManagementOperations.BackupVault(bu);
                richTextBox1.AppendText(Environment.NewLine + "ok");
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText(Environment.NewLine + "error:" + ex.Message);
            }
        }

        private bool RemoveGarbageData(Vault vault)
        {
            try//only keep admin user
            {
                var users = vault.UserOperations.GetUserAccounts();
                foreach (UserAccount userAccount in users)
                {
                    if (userAccount.LoginName == "admin") continue;
                    vault.UserOperations.RemoveUserAccount(userAccount.ID);
                }
            }
            catch (Exception exx)
            {
                richTextBox1.AppendText(Environment.NewLine + " RemoveUserAccount error ：" + exx.Message);
            }
            try
            {

                //var company =
                //    vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemObjectType,
                //        "OtCompanies");
                //if (company < 0) return true;
                //var OtDepartment =
                //   vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemObjectType,
                //       "OtDepartment");
                //if (OtDepartment < 0) return true;
                //var OtEmployee =
                //   vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemObjectType,
                //       "OtEmployee");
                //if (OtEmployee < 0) return true;
                var scs = new SearchConditions();
                var sc = new SearchCondition();
                //sc.ConditionType= MFConditionType.MFConditionTypeEqual;
                //sc.Expression.DataStatusValueType= MFStatusType.MFStatusTypeObjectTypeID;
                //sc.TypedValue.SetValueToLookup(new Lookup{Item=0});
                //scs.Add(-1,sc);
                //  var sr = vault.ObjectSearchOperations.SearchForObjectsByString("", false,
                //MFFullTextSearchFlags.MFFullTextSearchFlagsLookInMetaData);
                var sr = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs, MFSearchFlags.MFSearchFlagNone,
                    false, 0, 0);
                richTextBox1.AppendText(Environment.NewLine + sr.ObjectVersions.Count);
                foreach (ObjectVersion ov in sr.ObjectVersions)
                {
                    //if (ov.ObjVer.Type == company || ov.ObjVer.Type == OtDepartment || ov.ObjVer.Type == OtEmployee)
                    //    continue;
                    try
                    {
                        var pv = vault.ObjectPropertyOperations.GetProperty(ov.ObjVer,
                            (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefIsTemplate);
                        richTextBox1.AppendText(Environment.NewLine + pv.GetValueAsLocalizedText() + ov.ObjVer.Type + "-" + ov.ObjVer.ID);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            richTextBox1.AppendText(Environment.NewLine + "not template" + ov.ObjVer.Type + "-" + ov.ObjVer.ID + "name-" + ov.GetNameForFileSystem());
                            vault.ObjectOperations.DestroyObject(ov.ObjVer.ObjID, true, -1);
                            richTextBox1.AppendText("--DestroyObjected");
                        }
                        catch (Exception eex)
                        {
                            richTextBox1.AppendText(Environment.NewLine + "not template" + ov.ObjVer.Type + "-" + ov.ObjVer.ID + "DestroyObject failure" + ex.Message);
                        }
                    }
                }
                richTextBox1.AppendText(Environment.NewLine + "result number:" + sr.ObjectVersions.Count);
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText(Environment.NewLine + "error and exit:" + ex.Message);
                return true;
            }

            // if (sr.MoreResults) checkmoresr(vault);
            return false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.AppendText(Environment.NewLine + DateTime.Now.ToString("d"));
            richTextBox1.AppendText(Environment.NewLine + DateTime.Now.ToString("yyyy MMMM dd"));
            richTextBox1.AppendText(Environment.NewLine + DateTime.Now.ToString("D"));
            richTextBox1.AppendText(Environment.NewLine + DateTime.Now.ToString("F"));
            var str = "NoticeSetup1.0.0.23.exe";
            var tmp = str.Replace(".", "");
            richTextBox1.AppendText(Environment.NewLine + tmp);
            var reg = new Regex(@"\d+");
           var m= reg.Match(tmp);
           richTextBox1.AppendText(Environment.NewLine + m);
        }

        private void buttonrefreshemployee_Click(object sender, EventArgs e)
        {
            var sourcepath = textBoxsourcepath.Text;

        }
    }
}
