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
                var oServerApp = new MFilesServerApplication();
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

                var mfsc = oServerApp.Connect(GlobalVar.GetMfAuthType(), GlobalVar.UserName, GlobalVar.Password, GlobalVar.Domain,
                                     GlobalVar.ProtocolSequence,
                                     GlobalVar.NetworkAdress);


                GlobalVar.gVaultsOnServer = oServerApp.GetVaults();
                comboBox1.Items.Clear();
                foreach (VaultOnServer vos in GlobalVar.gVaultsOnServer)
                {
                    comboBox1.Items.Add(vos.Name + vos.GUID);
                }
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
        public  DataTable GetExcelTableByOleDb(string strExcelPath, string tableName)
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
                richTextBoxlog.AppendText(Environment.NewLine + string.Format( "GetExcelTableByOleDb error: {0}", ex.Message));
                return dtExcel;
            }
        }
        private  string Specialsplit(string lastmemtels)
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
        private void button3_Click(object sender, EventArgs e)
        {
            var oVault = GlobalVar.OVault;
            if (oVault == null)
            {
                MessageBox.Show("请先连接MFiles服务器，并选择相应的vault！");
                return;
            }
            if (_filePath == string.Empty)
            {
                MessageBox.Show("请先选择相应的Excel文件！");
                return;
            }
            int PropContractedProfessionid = -1;
            var PropContractorNameid = -1;
            var PropBusinessLicenseNumber = -1;
            var PropTaxRegistrationNumber = -1;
            var PropQualificationCertificateNumber = -1;
            var PropLevelOfQualification = -1;
            var PropSafetyProductionLicenseNumber = -1;
            var PropTelephoneAndFaxOfLegalRepresentative = -1;
            var PropDetailedAddress = -1;
            var PropDeputiesAndTelephones = -1;
            var PropLevel = -1;
            var classID = -1;
            var typeid = -1;
            var PropRegisteredCapital = -1;
            var VlContractedProfessionid = -1;
            var vlid = -1;
            try
            {
                 vlid = oVault.ValueListOperations.GetValueListIDByAlias("VlLevel");
                VlContractedProfessionid =
                   oVault.ValueListOperations.GetValueListIDByAlias("VlContractedProfession");
                PropContractedProfessionid =
                 oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropContractedProfession");
                PropContractorNameid =
                   oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropContractorName");
                PropBusinessLicenseNumber =
                   oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropBusinessLicenseNumber");
                PropTaxRegistrationNumber =
                   oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropTaxRegistrationNumber");
                PropQualificationCertificateNumber =
                   oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropQualificationCertificateNumber");
                PropLevelOfQualification =
                   oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropLevelOfQualification");
                PropSafetyProductionLicenseNumber =
                   oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropSafetyProductionLicenseNumber");
                PropRegisteredCapital =
                   oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropRegisteredCapital");
                PropTelephoneAndFaxOfLegalRepresentative =
                   oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropTelephoneAndFaxOfLegalRepresentative");
                PropDetailedAddress =
                   oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropDetailedAddress");
                PropDeputiesAndTelephones =
                   oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropDeputiesAndTelephones");
                PropLevel =
                   oVault.PropertyDefOperations.GetPropertyDefIDByAlias("PropLevel");
                classID = oVault.ClassOperations.GetObjectClassIDByAlias("ClassContractor");
                typeid = oVault.ObjectTypeOperations.GetObjectTypeIDByAlias("OtContractor");
            }
            catch (Exception alex)
            {
                richTextBoxlog.AppendText(Environment.NewLine + string.Format("取别名定义错: {0}", alex.Message));
                return;
            }
            if (vlid < 0 || VlContractedProfessionid < 0 || PropContractedProfessionid < 0 ||
             PropContractorNameid <0||
             PropBusinessLicenseNumber<0||
             PropTaxRegistrationNumber <0||
             PropQualificationCertificateNumber<0||
             PropLevelOfQualification<0||
             PropSafetyProductionLicenseNumber<0||
             PropTelephoneAndFaxOfLegalRepresentative<0||
             PropDetailedAddress <0||
             PropDeputiesAndTelephones <0||
             PropLevel<0||
             classID <0||
             typeid<0||
             PropRegisteredCapital<0)
            {
                richTextBoxlog.AppendText(Environment.NewLine +
                                          string.Format("如下别名定义有错或没有定义完全！对象别名{0},类别别名{1},级别{2},级别值列表{15},现场负责人及电话{3}," +
                                                        "详细地址{4},法人代表及电话、传真{5},注册资金（万元）{6},安全生产许可证编号{7},资质等级{8}," +
                                                        "资质证书编号{9},税务登记证编号{10},营业执照编号{11},分包商名称{12},承包专业{13},承包专业值列表{14}",
                                          "OtContractor", "ClassContractor","PropLevel", "PropDeputiesAndTelephones", "PropDetailedAddress",
                                              "PropTelephoneAndFaxOfLegalRepresentative", "PropRegisteredCapital","PropSafetyProductionLicenseNumber", "PropLevelOfQualification",
                                              "PropQualificationCertificateNumber", "PropTaxRegistrationNumber","PropBusinessLicenseNumber", "PropContractorName","PropContractedProfession",
                                              "VlContractedProfession", "VlLevel"));
                return;
            }
            richTextBoxlog.AppendText(Environment.NewLine +string.Format("： {0}", "开始循环读取sheet并导入"));
            foreach (var tablename in GetExcelTableName(_filePath))
            {
              //  BeginInvoke(new UpdateStatusDelegate(UpdateStatus), new object[] { string.Format("GetExcelTableName: {0}-开始导入", tablename)});
               richTextBoxlog.AppendText(Environment.NewLine + string.Format("GetExcelTableName: {0}-开始导入", tablename));
                if (tablename == "规定$") continue;
                // if (a != "主体土建工程$") continue; 
                //  if (tablename != "智能化工程$") continue;
               // if (tablename != "地基基础工程$") continue;
                var dt = GetExcelTableByOleDb(_filePath, tablename);
                var num = 0;
                var objectnum = 0;
                var mempvs = new PropertyValues();
                var currentlineisadditionline = false;
                var memtels = string.Empty;
                var lastmemtels = string.Empty;
              //  richTextBoxlog.AppendText(Environment.NewLine + string.Format("GetExcelTableName111: {0}", dt.Rows.Count));
                foreach (DataRow row in dt.Rows)
                {
                   // richTextBoxlog.AppendText(Environment.NewLine + string.Format("GetExcelTableName222: {0}", tablename));
                    var newPropertyValues = new PropertyValues();
                    var classid = new PropertyValue
                    {
                        PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass
                    };
                    classid.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classID);
                    newPropertyValues.Add(-1, classid);

                    var numcol = 0;
                    num++;
                    if (num < 6) continue;
                    #region begin one line
                 //   richTextBoxlog.AppendText(Environment.NewLine + string.Format("GetExcelTableName333: {0}", tablename));
                    foreach (DataColumn column in dt.Columns)
                    {
                        numcol++;
                        if (numcol < 2 || numcol > 16) continue;
                        if (numcol == 3)
                        {
                            currentlineisadditionline = row[column].ToString() == string.Empty;
                        }

                     //   if (row[column].ToString() == string.Empty) continue;

                        #region begin deal with properties
                        var newvalue = new PropertyValue();
                      //  richTextBoxlog.AppendText(Environment.NewLine + string.Format("GetExcelTableName444: {0}", tablename));
                        switch (numcol)
                        {
                            case 2:// VlContractedProfession
                                newvalue.PropertyDef = PropContractedProfessionid;
                                newvalue.TypedValue.SetValue(MFDataType.MFDatatypeText, row[column]);
                                newPropertyValues.Add(-1, newvalue);
                                var tmp = row[column].ToString().Trim();
                                if(tmp.Length>0)
                                dealVlContractedProfession(VlContractedProfessionid, tmp);
                                break;
                            case 3:
                                newvalue.PropertyDef = PropContractorNameid;
                                newvalue.TypedValue.SetValue(MFDataType.MFDatatypeText, row[column]);
                                newPropertyValues.Add(-1, newvalue);
                                break;
                            case 4:
                                newvalue.PropertyDef = PropQualificationCertificateNumber;
                                newvalue.TypedValue.SetValue(MFDataType.MFDatatypeText, row[column]);
                                newPropertyValues.Add(-1, newvalue);
                                break;
                            case 5:
                                newvalue.PropertyDef = PropLevelOfQualification;
                                newvalue.TypedValue.SetValue(MFDataType.MFDatatypeMultiLineText, row[column]);
                                newPropertyValues.Add(-1, newvalue);
                                break;
                            case 6:
                                newvalue.PropertyDef = PropBusinessLicenseNumber;
                                newvalue.TypedValue.SetValue(MFDataType.MFDatatypeText, row[column]);
                                newPropertyValues.Add(-1, newvalue);
                                break;
                            case 7:
                                newvalue.PropertyDef = PropSafetyProductionLicenseNumber;
                                newvalue.TypedValue.SetValue(MFDataType.MFDatatypeText, row[column]);
                                newPropertyValues.Add(-1, newvalue);
                                break;
                            case 9:
                                newvalue.PropertyDef = PropTaxRegistrationNumber;
                                newvalue.TypedValue.SetValue(MFDataType.MFDatatypeText, row[column]);
                                newPropertyValues.Add(-1, newvalue);
                                break;
                            case 10:
                                newvalue.PropertyDef = PropRegisteredCapital;
                                newvalue.TypedValue.SetValue(MFDataType.MFDatatypeText, row[column]);
                                newPropertyValues.Add(-1, newvalue);
                                break;
                            case 11:
                                newvalue.PropertyDef = PropDetailedAddress;
                                newvalue.TypedValue.SetValue(MFDataType.MFDatatypeMultiLineText, row[column]);
                                newPropertyValues.Add(-1, newvalue);
                                break;
                            case 12:
                                newvalue.PropertyDef = PropTelephoneAndFaxOfLegalRepresentative;
                                newvalue.TypedValue.SetValue(MFDataType.MFDatatypeMultiLineText, row[column].ToString().Replace("\r", " ").Replace("\n", " "));
                                newPropertyValues.Add(-1, newvalue);
                                break;
                            case 16:
                                //newvalue.PropertyDef = PropDeputiesAndTelephones;
                                //newvalue.TypedValue.SetValue(MFDataType.MFDatatypeMultiLineText, row[column]);
                                //newPropertyValues.Add(-1, newvalue);
                                // lastmemtels += memtels + "//r//n" + '-' + "\\r\\n"+"-\r\n-/r/n";
                                lastmemtels += memtels;
                                if (row[column].ToString().Trim() != string.Empty)
                                    memtels = row[column].ToString().Replace("\r", "").Replace("\n", "").Replace("  ", " ") + "\r\n";
                                else
                                    memtels = row[column].ToString();
                                break;
                            case 14:
                                var slevel = row[column].ToString().Replace(" ","");
                                if (slevel == string.Empty) break;
                                try
                                {
                                   
                                    var values = oVault.ValueListItemOperations.GetValueListItems(vlid);
                                    var lu = new Lookup();
                                    lu.Item = 4;
                                    var found = false;
                                    foreach (ValueListItem vlitem in values)
                                    {
                                        if (vlitem.Name == slevel)
                                        {
                                            lu.Item = vlitem.ID;
                                            found = true;
                                            break;
                                        }
                                    }
                                    newvalue.PropertyDef = PropLevel;
                                    newvalue.TypedValue.SetValue(MFDataType.MFDatatypeLookup, lu);
                                    newPropertyValues.Add(-1, newvalue);
                                    if (!found )
                                    {
                                        richTextBoxlog.AppendText(Environment.NewLine +
                                                                  string.Format("-级别未定义，请手动修改-: {0}，{1}，-{2}-", num,
                                                                      numcol, slevel));
                                    }
                                }
                                catch (Exception eee)
                                {
                                    richTextBoxlog.AppendText(Environment.NewLine + string.Format("--: {0}，{1}，-{2}-{3}", num, numcol, slevel, eee.Message));
                                }
                                break;
                            default:
                                break;
                        }
                        #endregion end deal with properties
                      //  richTextBoxlog.AppendText(Environment.NewLine + string.Format("--: {0}，{1}，-{2}-", num, numcol, row[column]));
                    }
                    if (!currentlineisadditionline)
                    {
                        var newvalue = new PropertyValue();
                        newvalue.PropertyDef = PropDeputiesAndTelephones;
                        newvalue.TypedValue.SetValue(MFDataType.MFDatatypeMultiLineText, Specialsplit(lastmemtels));
                        mempvs.Add(-1, newvalue);
                        lastmemtels = string.Empty;

                        if (mempvs.Count > 5)
                        {
                            try
                            {
                                var oObjectVersionAndProperties = oVault.ObjectOperations.CreateNewObject(typeid,
                                    mempvs);
                                var objVersion =
                                    oVault.ObjectOperations.CheckIn(oObjectVersionAndProperties.ObjVer);
                                objectnum++;
                            }
                            catch (Exception ex)
                            {
                                richTextBoxlog.AppendText(ex.Message+Environment.NewLine);
                            }
                        }
                        mempvs = newPropertyValues;
                    }

                    #endregion end one line
                }
                {
                    var newvalue = new PropertyValue();
                    newvalue.PropertyDef = PropDeputiesAndTelephones;
                    newvalue.TypedValue.SetValue(MFDataType.MFDatatypeMultiLineText, Specialsplit(lastmemtels + memtels));
                    mempvs.Add(-1, newvalue);

                    if (mempvs.Count > 5)
                    {
                        try { 
                        var oObjectVersionAndProperties = oVault.ObjectOperations.CreateNewObject(typeid,
                            mempvs);
                        var objVersion =
                            oVault.ObjectOperations.CheckIn(oObjectVersionAndProperties.ObjVer);
                        }
                        catch (Exception ex)
                        {
                            richTextBoxlog.AppendText("second"+ex.Message + Environment.NewLine);
                        }
                    }
                }
                richTextBoxlog.AppendText(Environment.NewLine +string.Format("GetExcelTableName end: {0}-sheet页总计导入记录-{1}条", tablename, objectnum + 1));
              //  break;//for debug one sheet
            } 
            richTextBoxlog.AppendText(Environment.NewLine + string.Format("all done!"));
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
