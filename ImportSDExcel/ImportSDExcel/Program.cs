using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;
using DataSet = System.Data.DataSet;
namespace ImportSDExcel
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var count = args.Count();
                var appname = AppDomain.CurrentDomain.SetupInformation.ApplicationName;
                Console.WriteLine("Usage : " + Environment.NewLine
                + "\t1) 如果使用域用户登陆并且当前登陆的域用户在目标vault中存在，命令格式为：" + appname + "  excel文件全名包括扩展名" + "  目标vault的guid" + Environment.NewLine
                + "\t2）否则命令格式为：" + appname + "  excel文件全名包括扩展名" + "  目标vault的guid" + "  M-Files用户  用户密码");
                if (count != 4 && count != 2)
                {
                    Console.WriteLine("Please specify command line parameters correctly {0}!",count);
                    Console.WriteLine("Any key to exit !");
                    Console.ReadKey();
                    return;
                }
                var filename = args[0];
                 var oServerApp = new MFilesServerApplication();
                var oVault = new Vault();
                try
                {
                    switch (count)
                    {
                        case 2:
                        {
                            oServerApp.Connect(MFAuthType.MFAuthTypeLoggedOnWindowsUser);
                            var a= oServerApp.GetVaults();
                            oVault = a.GetVaultByGUID(args[1]).LogIn();
                            //登录到M-files文档库
                        }
                            break;
                        case 4:
                        {
                            oServerApp.Connect(MFAuthType.MFAuthTypeSpecificMFilesUser, args[2], args[3]);
                            var gVaultsOnServer = oServerApp.GetVaults();
                            oVault = gVaultsOnServer.GetVaultByGUID(args[1]).LogIn();
                        }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("vault connect error! {0}", ex.Message);
                    return;
                }
                if (!File.Exists(filename))
                {
                    Console.WriteLine("please input correct excel filename! {0}", filename);
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
                try
                {
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
                    Console.WriteLine("取别名定义错: {0}",alex.Message);
                    return;
                }
                foreach (var tablename in GetExcelTableName(filename))
                {
                    Console.WriteLine("GetExcelTableName: {0}-开始导入", tablename);
                    if (tablename == "规定$") continue;
                 //   if (tablename != "主体土建工程$") continue; 
                    //  if (tablename != "智能化工程$") continue;
                   // if (tablename != "地基基础工程$") continue;
                   //   if (tablename != "装饰装修工程$") continue;
                  //  if (tablename != "防水防腐保温工程$") continue;
                  // // if (tablename != "电力工程$") continue;
                    var dt = GetExcelTableByOleDb(filename, tablename);
                    var num = 0;
                    var mempvs = new PropertyValues();
                    var currentlineisadditionline = false;
                    var memtels = string.Empty;
                    var lastmemtels = string.Empty;
                //    Console.WriteLine("GetExcelTableName111: {0}", dt.Rows.Count);
                    foreach (DataRow row in dt.Rows)
                    {
                      //  Console.WriteLine("GetExcelTableName222: {0}", tablename);
                        var newPropertyValues = new PropertyValues();
                        var classid = new PropertyValue
                        {
                            PropertyDef = (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefClass
                        };
                        classid.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classID);
                        newPropertyValues.Add(-1, classid);

                        var numcol = 0;
                        num++;
                        if (num <6) continue;
                 //       if (num > 11) continue;//only for debug
                        #region begin one line
                    //    Console.WriteLine("GetExcelTableName333: {0}", tablename);
                        foreach (DataColumn column in dt.Columns)
                        {
                            numcol++;
                            if (numcol < 2 || numcol > 16) continue;
                            if (numcol == 3)
                            {
                                currentlineisadditionline = row[column].ToString() == string.Empty;
                            }

                       //     if (row[column].ToString() == string.Empty) continue;

                            #region begin deal with properties
                            var newvalue = new PropertyValue();
                         //   Console.WriteLine("GetExcelTableName444: {0}", tablename);
                            switch (numcol)
                            {
                                case 2:
                                    newvalue.PropertyDef = PropContractedProfessionid;
                                    newvalue.TypedValue.SetValue(MFDataType.MFDatatypeText, row[column]);
                                    newPropertyValues.Add(-1, newvalue);
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
                                    lastmemtels += memtels ;
                                    if (row[column].ToString().Trim() != string.Empty)
                                        memtels = row[column].ToString().Replace("\r", "").Replace("\n", "").Replace("  "," ") + "\r\n";
                                    else 
                                        memtels = row[column].ToString();
                                    break;
                                case 14:
                                    newvalue.PropertyDef = PropLevel;
                                    try
                                    {
                                        var vlid = oVault.ValueListOperations.GetValueListIDByAlias("VlLevel");
                                        var values = oVault.ValueListItemOperations.GetValueListItems(vlid);
                                        var lu = new Lookup();
                                        lu.Item = 4;
                                        var found = false;
                                        foreach (ValueListItem vlitem in values)
                                        {
                                            if (vlitem.Name == row[column].ToString())
                                            {
                                                lu.Item = vlitem.ID;
                                                found = true;
                                                break;
                                            }
                                        }
                                        newvalue.TypedValue.SetValue(MFDataType.MFDatatypeLookup, lu);
                                        newPropertyValues.Add(-1, newvalue);
                                        if (!found && row[column].ToString()!=string.Empty) Console.WriteLine("-级别未定义，请手动修改-: {0}，{1}，-{2}-", num, numcol, row[column]);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("--: {0}，{1}，-{2}-{3}", num, numcol, row[column], e.Message);
                                    }
                                    break;
                                default:
                                    break;
                            }
                            #endregion end deal with properties
                         // Console.WriteLine("--: {0}，{1}，-{2}-", num, numcol, row[column]);
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
                                var oObjectVersionAndProperties = oVault.ObjectOperations.CreateNewObject(101,
                                    mempvs);
                                var objVersion =
                                    oVault.ObjectOperations.CheckIn(oObjectVersionAndProperties.ObjVer);
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
                            var oObjectVersionAndProperties = oVault.ObjectOperations.CreateNewObject(typeid,
                                mempvs);
                            var objVersion =
                                oVault.ObjectOperations.CheckIn(oObjectVersionAndProperties.ObjVer);
                        }
                    }
                    Console.WriteLine("GetExcelTableName end: {0}-sheet页总计导入记录-{1}条", tablename,num);
                  //  break;//for debug one sheet
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("big end exception: {0}", ex.Message);
            }
            Console.WriteLine("Done, Any key to exit !");
            Console.ReadKey();
        }
     
        private static string Specialsplit(string lastmemtels)
        {
            var ret = string.Empty;
            var memachar = '0';
            var count = 0;
            var mempos = 0;
            foreach (var achar in lastmemtels)
            {
                if (memachar >= '0' && memachar <= '9'&&count>1)
                {
                    if (((achar < '0' || achar > '9') && achar != '\r' && achar != '-' && achar != '/' && achar != 'X' &&
                        achar != ';' && achar != '；' && achar != ' ') || (achar == ' ' && (lastmemtels[count + 1] < '0' || lastmemtels[count + 1] > '9')))
                    {
                        ret += lastmemtels.Substring(mempos, count-mempos);
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
       
        public static DataTable GetExcelTableByOleDb(string strExcelPath, string tableName)
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
              //  string strSql = "select $1 from [" + tableName + "]";
                //获取Excel指定Sheet表中的信息
                OleDbCommand objCmd = new OleDbCommand(strSql, objConn);
                OleDbDataAdapter myData = new OleDbDataAdapter(strSql, objConn);
                myData.Fill(ds, tableName);//填充数据
             //   Console.WriteLine("GetExcelTableByOleDb --SelectCommand--: {0}", myData.);
                objConn.Close();
                //dtExcel即为excel文件中指定表中存储的信息
                dtExcel = ds.Tables[tableName];
                //for(int i=0;i<5;i++)
                //{
                //    Console.WriteLine("GetExcelTableByOleDb --row0--: {0}", ds.Tables[0].Rows[0][i]);
                //}
              
                
                return dtExcel;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetExcelTableByOleDb error: {0}", ex.Message);
                return dtExcel;
            }
        }
        ///   
        /// 获取EXCEL的表 表名字列    
        ///   
        /// Excel文件   
        /// 数据表   
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
        /// <summary>  
        /// 读取Excel文件到DataSet中  
        /// </summary>  
        /// <param name="filePath">文件路径</param>  
        /// <returns></returns>  
        public static System.Data.DataSet ToDataTable(string filePath)
        {
            string connStr = "";
            string fileType = System.IO.Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(fileType)) return null;

            if (fileType == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + filePath + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filePath + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            string sql_F = "Select * FROM [{0}]";

            OleDbConnection conn = null;
            OleDbDataAdapter da = null;
            DataTable dtSheetName = null;

            System.Data.DataSet ds = new System.Data.DataSet();
            try
            {
                // 初始化连接，并打开  
                conn = new OleDbConnection(connStr);
                conn.Open();

                // 获取数据源的表定义元数据                         
                string SheetName = "";
                dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                Console.WriteLine("111 {0}", dtSheetName.TableName);
                // 初始化适配器  
                da = new OleDbDataAdapter();
                for (int i = 0; i < dtSheetName.Rows.Count; i++)
                {
                    SheetName = (string)dtSheetName.Rows[i]["TABLE_NAME"];

                    if (SheetName.Contains("$") && !SheetName.Replace("'", "").EndsWith("$"))
                    {
                        continue;
                    }

                    da.SelectCommand = new OleDbCommand(String.Format(sql_F, SheetName), conn);
                    DataSet dsItem = new DataSet();
                    //  da.Fill(dsItem, tblName);  

                    ds.Tables.Add(dsItem.Tables[0].Copy());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ToDataTable exception: {0}", ex.Message);
            }
            finally
            {
                // 关闭连接  
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    da.Dispose();
                    conn.Dispose();
                }
            }
            return ds;
        }
    }
}
