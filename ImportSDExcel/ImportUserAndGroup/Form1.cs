using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MFilesAPI;
using System.Configuration;
using DataSet = System.Data.DataSet;

namespace ImportSdExcelForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private delegate void UpdateStatusDelegate(string status);
        private void UpdateStatus(string status)
        {
            richTextBoxlog.AppendText(Environment.NewLine + string.Format("{0}--{1}", DateTime.Now, status));
        }
        private class GlobalVar
        {
            public static Vault OVault = null;
            public static MFilesServerApplication oServerApp;
            public static VaultsOnServer gVaultsOnServer = null;
            #region 连接M-Files设置(MFilesServerApplication::connect(),详见M-filesAPI)
            public static readonly string VaultGUID = System.Configuration.ConfigurationManager.AppSettings["VaultGUID"];
            public static string MfAuthType = System.Configuration.ConfigurationManager.AppSettings["MFAuthType"];//连接M-Files的身份有四种类型， //详见M-filesAPI
            public static string UserName = System.Configuration.ConfigurationManager.AppSettings["UserName"];
            public static string Password = System.Configuration.ConfigurationManager.AppSettings["Password"];
            public static string Domain = System.Configuration.ConfigurationManager.AppSettings["Domain"];   //域名
            public static readonly string ProtocolSequence = System.Configuration.ConfigurationManager.AppSettings["ProtocolSequence"];
            public static string NetworkAdress = System.Configuration.ConfigurationManager.AppSettings["NetworkAdress"];

            #endregion
            #region 根据配置文件读取信息获取M-files用户类型
            public static MFAuthType GetMfAuthType()
            {
                switch (MfAuthType)
                {
                    case "MFAuthTypeLoggedOnWindowsUser":
                        return MFAuthType.MFAuthTypeLoggedOnWindowsUser;
                    case "MFAuthTypeSpecificMFilesUser":
                        return MFAuthType.MFAuthTypeSpecificMFilesUser;
                    case "MFAuthTypeSpecificWindowsUser":
                        return MFAuthType.MFAuthTypeSpecificWindowsUser;
                    case "MFAuthTypeUnknown":
                        return MFAuthType.MFAuthTypeUnknown;
                    default:
                        return MFAuthType.MFAuthTypeLoggedOnWindowsUser;//默认情况下
                }
            }
            #endregion
        }
        private string _filePath = "";
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                GlobalVar.OVault = null;
                GlobalVar.oServerApp = new MFilesServerApplication();
                if (radioButton2.Checked == true)
                {
                    GlobalVar.Domain = textBox4.Text;
                    GlobalVar.MfAuthType = "MFAuthTypeLoggedOnWindowsUser";
                }
                else if (radioButton1.Checked == true)
                {
                    GlobalVar.MfAuthType = "MFAuthTypeSpecificMFilesUser";
                }
                GlobalVar.UserName = textBox2.Text;
                GlobalVar.Password = textBox3.Text;
                GlobalVar.NetworkAdress = textBox1.Text;

                var mfsc = GlobalVar.oServerApp.Connect(GlobalVar.GetMfAuthType(), GlobalVar.UserName, GlobalVar.Password, GlobalVar.Domain,
                                     GlobalVar.ProtocolSequence,
                                     GlobalVar.NetworkAdress);


                GlobalVar.gVaultsOnServer = GlobalVar.oServerApp.GetVaults();
                comboBox1.Items.Clear();
                foreach (VaultOnServer vos in GlobalVar.gVaultsOnServer)
                {
                    comboBox1.Items.Add(vos.Name + vos.GUID);
                }
                comboBox1.SelectedIndex = 0;
                richTextBoxlog.AppendText(Environment.NewLine + "成功连接" + GlobalVar.NetworkAdress + "MFiles服务器!");

                comboBox1.Focus();
            }
            catch (Exception ex)
            {
                richTextBoxlog.AppendText(Environment.NewLine + ex.Message);
                if (radioButton2.Checked == true)
                {
                    MessageBox.Show("域用户登陆方式连接MFiles服务器失败,用户名=" + GlobalVar.UserName + "，密码=" + GlobalVar.Password +
                        "，ip地址或机器名=" + GlobalVar.NetworkAdress + "，域名=" + GlobalVar.Domain + ex.Message);
                }
                else
                {
                    MessageBox.Show("MFiles用户登陆方式连接MFiles服务器失败,用户名=" + GlobalVar.UserName + "，密码=" + GlobalVar.Password +
                           "，ip地址或机器名=" + GlobalVar.NetworkAdress + ex.Message);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string filePath = string.Empty;
            var fileDialog = new OpenFileDialog { Filter = "Excel文件(*.xlsx/*.xls)|*.xls*" };
            fileDialog.FileName = _filePath;
            fileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = fileDialog.FileName;
                textBox5.Text = filePath;
                _filePath = filePath;
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBoxlog_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                textBox4.Visible = false;
                label5.Visible = false;
                textBox3.Visible = false;
                label4.Visible = false;
                textBox2.Visible = false;
                label3.Visible = false;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                textBox4.Visible = false;
                label5.Visible = false;
                textBox3.Visible = true;
                label4.Visible = true;
                textBox2.Visible = true;
                label3.Visible = true;
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        public static string[] GetExcelTableName(string strExcelFileName)
        {
            var listTableName = new List<string>();
            try
            {
                if (System.IO.File.Exists(strExcelFileName))
                {
                    OleDbConnection excelCon = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Extended Properties=\"Excel 8.0\";Data Source=" + strExcelFileName);
                    excelCon.Open();
                    DataTable dt = excelCon.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    excelCon.Close();
                    for (int i = 0; i != dt.Rows.Count; i++)
                    {
                        listTableName.Add(dt.Rows[i]["Table_Name"].ToString());
                    }
                    return listTableName.ToArray();
                }
                return listTableName.ToArray();
            }
            catch
            {
                return listTableName.ToArray();
            }
        }
        public DataTable GetExcelTableByOleDb(string strExcelPath, string tableName)
        {
            var dtExcel = new DataTable();
            //数据表
            try
            {
                DataSet ds = new DataSet();
                //获取文件扩展名
                string strExtension = System.IO.Path.GetExtension(strExcelPath);
                string strFileName = System.IO.Path.GetFileName(strExcelPath);
                //Excel的连接
                OleDbConnection objConn = null;
                switch (strExtension)
                {
                    case ".xls":
                        objConn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strExcelPath + ";" + "Extended Properties=\"Excel 8.0;HDR=NO;IMEX=1;\"");
                        break;
                    case ".xlsx":
                        objConn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + strExcelPath + ";" + "Extended Properties=\"Excel 12.0;HDR=NO;IMEX=1;\"");
                        break;
                    default:
                        objConn = null;
                        break;
                }
                if (objConn == null)
                {
                    return dtExcel;
                }
                objConn.Open();
                //获取Excel中所有Sheet表的信息
                //System.Data.DataTable schemaTable = objConn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
                //获取Excel的第一个Sheet表名
                //string tableName = schemaTable.Rows[0][2].ToString().Trim();
                string strSql = "select * from [" + tableName + "]";
                //获取Excel指定Sheet表中的信息
                OleDbCommand objCmd = new OleDbCommand(strSql, objConn);
                OleDbDataAdapter myData = new OleDbDataAdapter(strSql, objConn);
                myData.Fill(ds, tableName);//填充数据
                objConn.Close();
                //dtExcel即为excel文件中指定表中存储的信息
                dtExcel = ds.Tables[tableName];
                return dtExcel;
            }
            catch (Exception ex)
            {
                richTextBoxlog.AppendText(Environment.NewLine + string.Format("GetExcelTableByOleDb error: {0}", ex.Message));
                return dtExcel;
            }
        }
        private string Specialsplit(string lastmemtels)
        {
            var ret = string.Empty;
            var memachar = '0';
            var count = 0;
            var mempos = 0;
            foreach (var achar in lastmemtels)
            {
                if (memachar >= '0' && memachar <= '9' && count > 1)
                {
                    //  if ((achar < '0' || achar > '9') && achar != '\r' && achar != '-' && achar != '/' && achar != 'X' &&
                    //      achar != ';' && achar != '；' && (achar == ' ' && (lastmemtels[count + 1] < '0' || lastmemtels[count + 1] > '9')))
                    if (((achar < '0' || achar > '9') && achar != '\r' && achar != '-' && achar != '/' && achar != 'X' &&
                       achar != ';' && achar != '；' && achar != ' ') || (achar == ' ' && (lastmemtels[count + 1] < '0' || lastmemtels[count + 1] > '9')))
                    {
                        ret += lastmemtels.Substring(mempos, count - mempos);
                        ret += "\r\n";
                        mempos = count;
                    }
                }
                memachar = achar;

                count++;
            }
            ret += lastmemtels.Substring(mempos);
            // ret = lastmemtels;
            return ret;
        }
        public class userandgroup
        {
            public userandgroup()
            {
                company = string.Empty;
                groups = new List<onegroup>();
                users = new List<oneuser>();
            }
            public string company { set; get; }
            public List<onegroup> groups { set; get; }
            public List<oneuser> users { set; get; }
        }
        public class onegroup
        {
            public onegroup()
            {
                name = string.Empty;
                alias = string.Empty;
                users = new List<oneuser>();
            }
            public string name { set; get; }
            public string alias { set; get; }
            public List<oneuser> users { set; get; }
        }
        public class oneuser
        {
            public oneuser()
            {
                name = string.Empty;
                fullname = string.Empty;
                password = string.Empty;
            }
            public string name { set; get; }
            public string fullname { set; get; }
            public string password { set; get; }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            var oVault = GlobalVar.OVault;
            if (oVault == null)
            {
                MessageBox.Show("请先连接MFiles服务器，并选择相应的vault！");
                return;
            }
            _filePath = textBox5.Text;
            //if (_filePath == string.Empty)
            //{
            //    MessageBox.Show("请先选择相应的Excel文件！");
            //    return;
            //}

            try
            {

            }
            catch (Exception alex)
            {
                richTextBoxlog.AppendText(Environment.NewLine + string.Format("取别名定义错: {0}", alex.Message));
                return;
            }

            richTextBoxlog.AppendText(Environment.NewLine + string.Format("{1}： {0}", "开始循环读取sheet并导入",_filePath));
            var all = new List<userandgroup>();
            var onecompany = new userandgroup();
            foreach (var tablename in GetExcelTableName(_filePath))
            {
                richTextBoxlog.AppendText(Environment.NewLine + string.Format("GetExcelTableName: {0}-开始导入", tablename));
                if (tablename != "账号$") continue;

                var dt = GetExcelTableByOleDb(_filePath, tablename);
                var rowindex = 0;
                richTextBoxlog.AppendText(Environment.NewLine + string.Format("------{1}： {0}", dt.Rows.Count, tablename));
                foreach (DataRow row in dt.Rows)
                {
                    if (++rowindex<2)
                    {
                        continue;
                    }
                    var numcol = 0;
                    var oneug = new onegroup();
                    var oneuser = new oneuser();
                    foreach (DataColumn column in dt.Columns)
                    {
                        ++numcol;
                        var onetext = row[column].ToString().Trim();
                        if (onetext == string.Empty) continue;
                        switch (numcol)
                        {
                            case 1:
                                if (!string.IsNullOrEmpty(onecompany.company))
                                {
                                    all.Add(onecompany);
                                }
                                onecompany = new userandgroup();
                                onecompany.company = onetext;
                                break;
                            case 2:
                                oneug = new onegroup();
                                oneug.name = onetext;
                                break;
                            case 3:
                                oneuser = new oneuser();
                                oneuser.fullname = onetext;
                                break;
                            case 4:
                                oneuser.name = onetext;
                                break;
                            case 5:
                                oneuser.password = onetext;
                                break;
                            case 6:
                                oneug.alias = onetext;
                                break;
                            default:
                                break;
                        }
                    }
                    if (!string.IsNullOrEmpty(oneuser.fullname))
                    {
                        oneug.users.Add(oneuser);
                    }
                    if (!string.IsNullOrEmpty(oneug.name))
                    {
                        onecompany.groups.Add(oneug);
                    }
                }
                if (!string.IsNullOrEmpty(onecompany.company))
                {
                    all.Add(onecompany);
                }
            }
            createaccount(all);
            richTextBoxlog.AppendText(Environment.NewLine + string.Format("all done!"));
        }

        private void createaccount(List<userandgroup> all)
        {
            var oVault = GlobalVar.OVault;
            var MFLicenseTypeNamedUserLicense = 0;
            var changelicense = false;
            var companyindex = 0;
            richTextBoxlog.AppendText(Environment.NewLine + "公司---" + all.Count);
            foreach (userandgroup userandgroup in all)
            {
                if (userandgroup.company == string.Empty)
                {
                    continue;
                }
                richTextBoxlog.AppendText(Environment.NewLine+"公司---"+userandgroup.company+userandgroup.groups.Count);
                var bigug = new UserGroup();
                bigug.Name = userandgroup.company;

                foreach (onegroup ogOnegroup in userandgroup.groups)
                {
                    if (ogOnegroup.name == string.Empty)
                    {
                        continue;
                    }
                    richTextBoxlog.AppendText(Environment.NewLine + "用户组" + ogOnegroup.name + ogOnegroup.users.Count + ogOnegroup.alias);
                    var ug = new UserGroup();
                    ug.Name = ogOnegroup.name;
                    foreach (oneuser ou in ogOnegroup.users)
                    {
                        if (ou.fullname == string.Empty)
                        {
                            continue;
                        }
                        richTextBoxlog.AppendText(Environment.NewLine + "用户" + ou.name + ou.fullname + ou.password);
                        var la = new LoginAccount();
                        la.UserName = ou.name;
                        la.FullName = ou.fullname;
                        la.AccountType = MFLoginAccountType.MFLoginAccountTypeMFiles;
                        la.LicenseType = changelicense ? MFLicenseType.MFLicenseTypeNamedUserLicense : MFLicenseType.MFLicenseTypeConcurrentUserLicense ;
                        la.Enabled = true;
                        la.ServerRoles = MFLoginServerRole.MFLoginServerRoleLogIn;
                        la.EmailAddress = "test@simuladesign.com";
                        try
                        {
                            GlobalVar.oServerApp.LoginAccountOperations.AddLoginAccount(la, ou.password);
                            richTextBoxlog.AppendText(Environment.NewLine + "AddLoginAccount ok-" + la.UserName);
                            MFLicenseTypeNamedUserLicense++;
                            if (MFLicenseTypeNamedUserLicense > 108) changelicense = true;
                        }
                        catch (Exception ex)
                        {
                            richTextBoxlog.AppendText(Environment.NewLine + "AddLoginAccount error-" + la.UserName);
                            try
                            {
                                GlobalVar.oServerApp.LoginAccountOperations.ModifyLoginAccount(la);
                                richTextBoxlog.AppendText(Environment.NewLine + "ModifyLoginAccount ok-" + la.UserName);
                            }
                            catch (Exception eex)
                            {
                                richTextBoxlog.AppendText(Environment.NewLine + "ModifyLoginAccount error-" + la.UserName);
                            }
                        }

                        var vu = new UserAccount();
                        vu.Enabled = true;
                        vu.LoginName = ou.name;
                        vu.VaultRoles = MFUserAccountVaultRole.MFUserAccountVaultRoleDefaultRoles;
                        try
                        {
                            var newuser = oVault.UserOperations.AddUserAccount(vu);
                            richTextBoxlog.AppendText(Environment.NewLine + "AddUserAccount ok-" + vu.LoginName);
                            ug.AddMember(newuser.ID);
                            bigug.AddMember(newuser.ID);
                        }
                        catch (Exception ex)
                        {
                            richTextBoxlog.AppendText(Environment.NewLine + "AddUserAccount error-" + vu.LoginName);
                            try
                            {
                                var userid = getuserid(ou.fullname, oVault);
                               
                                ug.AddMember(userid);
                                bigug.AddMember(userid);
                                richTextBoxlog.AppendText(Environment.NewLine + "AddMember ok-" + vu.LoginName + userid);
                            }
                            catch (Exception eex)
                            {
                                richTextBoxlog.AppendText(Environment.NewLine + "AddMember error-" + vu.LoginName + eex.Message);
                            }
                        }
                    }
                    try
                    {
                        if (ogOnegroup.alias != string.Empty)
                        {
                            try
                            {
                                var ugid =
                                    oVault.GetMetadataStructureItemIDByAlias(
                                        MFMetadataStructureItem.MFMetadataStructureItemUserGroup, ogOnegroup.alias);

                                var uga =
                                    oVault.UserGroupOperations.GetUserGroupAdmin(ugid);
                                uga.UserGroup.Members = ug.Members;
                                uga.SemanticAliases.Value = ogOnegroup.alias;

                                oVault.UserGroupOperations.UpdateUserGroupAdmin(uga);
                                richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin ok-" + ug.Name +
                                                          uga.UserGroup.ID);
                                bigug.AddMember(-uga.UserGroup.ID);
                            }
                            catch (Exception ex)
                            {
                                var groups = oVault.UserGroupOperations.GetUserGroupList();
                                foreach (KeyNamePair keyNamePair in groups)
                                {
                                    if (keyNamePair.Name == ogOnegroup.name)
                                    {
                                        var uga =
                                   oVault.UserGroupOperations.GetUserGroupAdmin(keyNamePair.Key);
                                        uga.UserGroup.Members = ug.Members;
                                        uga.SemanticAliases.Value = ogOnegroup.alias;
                                        oVault.UserGroupOperations.UpdateUserGroupAdmin(uga);
                                        richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin ok-" + ug.Name +
                                                                  uga.UserGroup.ID);
                                        bigug.AddMember(-uga.UserGroup.ID);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var groups = oVault.UserGroupOperations.GetUserGroupList();
                            foreach (KeyNamePair keyNamePair in groups)
                            {
                                if (keyNamePair.Name == ogOnegroup.name)
                                {
                                    var uga =
                               oVault.UserGroupOperations.GetUserGroupAdmin(keyNamePair.Key);
                                    uga.UserGroup.Members = ug.Members;

                                    oVault.UserGroupOperations.UpdateUserGroupAdmin(uga);
                                    richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin ok-" + ug.Name +
                                                              uga.UserGroup.ID);
                                    bigug.AddMember(-uga.UserGroup.ID);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin error-" + ug.Name+ex.Message);
                        try
                        {
                            var uga=new UserGroupAdmin();
                            uga.UserGroup = ug;
                            uga.SemanticAliases.Value = ogOnegroup.alias;
                            var aaa=oVault.UserGroupOperations.AddUserGroupAdmin(uga);
                            richTextBoxlog.AppendText(Environment.NewLine + "AddUserGroupAdmin ok-" + ug.Name + aaa.UserGroup.ID);
                            bigug.AddMember(-aaa.UserGroup.ID);
                        }
                        catch (Exception )
                        {
                            richTextBoxlog.AppendText(Environment.NewLine + "AddUserGroupAdmin error-" + ug.Name);
                        }
                    }
                 //   break;//for debug
                }
                var biguga = new UserGroupAdmin();
                biguga.UserGroup = bigug;
                companyindex++;
                biguga.SemanticAliases.Value = companyindex.ToString();
                try
                {
                    oVault.UserGroupOperations.AddUserGroupAdmin(biguga);
                    richTextBoxlog.AppendText(Environment.NewLine + "AddUserGroupAdmin ok-" + biguga.UserGroup.Name);
                }
                catch (Exception ex)
                {
                    richTextBoxlog.AppendText(Environment.NewLine + "AddUserGroupAdmin error-" + biguga.UserGroup.Name+ex.Message);
                    try
                    {
                          richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin alias-" + companyindex.ToString());
                        var ugid =
                            oVault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemUserGroup, companyindex.ToString());
                        richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin ugid-" + bigug.Name + ugid);
                        var uga =
                            oVault.UserGroupOperations.GetUserGroupAdmin(ugid);
                        richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin 111-" + uga.UserGroup.ID + "-" + uga.UserGroup.Members.Count + "-" + bigug.Members.Count);
                       // uga.UserGroup.Name = bigug.Name;
                        //foreach (int member in bigug.Members)
                        //{
                        //     uga.UserGroup.Members.Add(-1,member);
                        //     richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin :" +  member);
                        //}
                        //foreach (int member in uga.UserGroup.Members)
                        //{
                        //    richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin -all:" + member);
                        //}
                        uga.UserGroup.Members = bigug.Members;
                        uga.SemanticAliases.Value = companyindex.ToString();
                        richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin 222-" + "-" + uga.UserGroup.Members.Count);
                        oVault.UserGroupOperations.UpdateUserGroupAdmin(uga);
                        richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin ok-" +
                                                  biguga.UserGroup.Name);
                    }
                    catch (Exception eex)
                    {
                        richTextBoxlog.AppendText(Environment.NewLine + "UpdateUserGroupAdmin error-" +
                                                 biguga.UserGroup.Name + eex.Message);
                    }
                }
              //  break;//for debug
            }
            richTextBoxlog.AppendText(Environment.NewLine + "公司---" + all.Count);
        }

        private int getuserid(string username, Vault oVault)
        {
            var ret = -1;
           var  ulist= oVault.UserOperations.GetUserList();
            foreach (KeyNamePair keyNamePair in ulist)
            {
              //  richTextBoxlog.AppendText(Environment.NewLine + "getuserid info-" + keyNamePair.Key + keyNamePair.Name);
                if (keyNamePair.Name == username) return keyNamePair.Key;
            }
            if (ret < 0)
            {
                throw new Exception(string.Format("用户：{0}，没有找到",username));
            }
            return ret;
        }

        private void dealVlContractedProfession(int VlContractedProfessionid, string value)
        {
            var listitems = GlobalVar.OVault.ValueListItemOperations.GetValueListItems(VlContractedProfessionid);
            var haveit = false;
            foreach (ValueListItem item in listitems)
            {
                if (item.Name == value)
                {
                    haveit = true;
                    break;
                }
            }
            if (!haveit)
            {
                try
                {
                    ValueListItem newv = new ValueListItem();
                    newv.Name = value;
                    GlobalVar.OVault.ValueListItemOperations.AddValueListItem(VlContractedProfessionid, newv);
                }
                catch (Exception ex)
                {
                    richTextBoxlog.AppendText(Environment.NewLine + string.Format("-值列表插入新值出错-: value={0}，VlContractedProfessionid={1}，-{2}-", value, VlContractedProfessionid, ex.Message));
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count < 1)
            {
                MessageBox.Show("请先连接MFiles服务器，如已经连接，则当前用户没有任何vault可操作！");
                return;
            }
            try
            {
                GlobalVar.OVault = null;
                // listView1.Items.Clear();
                // richTextBoxlog.AppendText(Environment.NewLine + "perform comboBox1_SelectedIndexChanged" + comboBox1.Text);

                var ss = comboBox1.Text;
                var pos = ss.IndexOf('{');
                var guid = ss.Substring(pos);
                GlobalVar.OVault = GlobalVar.gVaultsOnServer.GetVaultByGUID(guid).LogIn(); //登录到M-files文档库

            }
            catch (Exception ex)
            {
                MessageBox.Show("当前MFiles登陆用户没有足够的库操作权限，请使用有库操作权限的用户重新连接！");
                richTextBoxlog.AppendText(Environment.NewLine + ex.Message);
            }
        }
    }
}
