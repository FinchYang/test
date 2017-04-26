using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using MFilesAPI;
using Oracle.ManagedDataAccess.Client;

namespace SynchronizeUserInfo
{
    public partial class SynchronizeUserInfo : ServiceBase
    {
        MFilesServerApplication app;
        private string connstr;
        private List<CommonInfo> organizations=new List<CommonInfo>();
        public SynchronizeUserInfo()
        {
            InitializeComponent();
            CanStop = true;
            connstr = ConfigurationManager.AppSettings["dbworldDatabase"];
        }

        private Thread _thread;

        protected override void OnStart(string[] args)
        {

            if (_thread == null)
            {
                _thread = new Thread(Start);
                _thread.Start();
            }
        }

        protected override void OnStop()
        {
            if (_thread != null)
            {
                _thread.Abort();
            }
        }

        private void Start()
        {
            var traceFile = GetTraceFile();
            Trace.AutoFlush = true;
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(traceFile));
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Trace.TraceInformation("开始执行---" + basePath);
            var i = 0L;
            try
            {
                app = new MFilesServerApplication();
                app.Connect(MFAuthType.MFAuthTypeSpecificMFilesUser, "admin", "cadsimula@123A");
            }
            catch (Exception ex)
            {
                Trace.TraceError("MFilesServerApplication Connect error:", ex);
                return;
            }
            while (true)
            {
                //设置Log
                var traceFileTemp = GetTraceFile();
                if (traceFile != traceFileTemp)
                {
                    traceFile = traceFileTemp;
                    Trace.Listeners.Clear();
                    Trace.Listeners.Add(new TextWriterTraceListener(traceFile));
                }

                try
                {
                    i++;
                    Trace.TraceInformation("第{0}次执行，{1}", i, DateTime.Now);
                    OneSynchronization();
                }
                catch (Exception ex)
                {
                    Trace.TraceInformation("未知错误" + ex.Message);
                }
                finally
                {
                    Thread.Sleep(1000 * 60 * 60 * 24);
                }
            }
        }
        public class CommonInfo
        {
            public CommonInfo()
            {
            }
            public string key { set; get; }
            public string name { set; get; }
        }
        public class MfilesLogAccount
        {
            public MfilesLogAccount()
            {
                matched = false;
            }
            public string name { set; get; }
            public LoginAccount Account { set; get; }
            public string fullname { set; get; }
            public string email { set; get; }
            public bool matched { set; get; }
        }
        public class DbAccount
        {
            public DbAccount()
            {
                matched = false;
            }
            public bool matched { set; get; }
            public string name { set; get; }
            public string pk_psndoc { set; get; }
            public string email { set; get; }
            public string code { set; get; }
            public string mobile { set; get; }
            public string pk_psncl { set; get; }
            public string pk_dept { set; get; }
            public string pk_job { set; get; }
            public string pk_org { set; get; }
            public string pk_post { set; get; }
            //public string actual_pk_psncl { set; get; }
            //public string actual_pk_dept { set; get; }
            //public string actual_pk_job { set; get; }
            //public string actual_pk_org { set; get; }
            //public string actual_pk_post { set; get; }
        }
        private void OneSynchronization()
        {
            try
            {
                var DataSource = ConfigurationManager.AppSettings["DataSource"];
                var allaccount = new List<MfilesLogAccount>();
                allaccount = GetAllAccounts();//获得mfiles所有登录账户，admin除外
                Trace.TraceInformation("用户基本信息----");
                var dbaccounts = GetStaffBasicInformation(DataSource);
                Trace.TraceInformation("用户工作信息----");
                GetStaffWorkInformation(DataSource, dbaccounts);
                var CommandText = "Select pk_psncl , name   From bd_psncl ";
                Trace.TraceInformation("人员类别信息----");
                var personnelCategorys = GetCommonInformation(DataSource, CommandText);
                CommandText = "Select pk_job  , jobname    From om_job ";
                Trace.TraceInformation("职位信息----");
                var jobs = GetCommonInformation(DataSource, CommandText);
                CommandText = "Select pk_dept   , name     From org_dept ";
                Trace.TraceInformation("用户部门信息----");
                var departments = GetCommonInformation(DataSource, CommandText);
                CommandText = "Select pk_post   , postname     From om_post ";
                Trace.TraceInformation("岗位信息----");
                var posts = GetCommonInformation(DataSource, CommandText);
                CommandText = "Select pk_hrorg   , name     From org_hrorg ";
                Trace.TraceInformation("组织信息----");
                 organizations = GetCommonInformation(DataSource, CommandText);

                syncAccounts(allaccount, dbaccounts);//更新mfiles登录账户
          //      UpdateVaults(dbaccounts);
                UpdatePersonnelCategory(personnelCategorys);
                UpdatePositionInfo(posts);
                    UpdateCscecRole(jobs);
                UpdateCompany(organizations);//更新数据库company
                Updatedepartment(departments);//更新数据库department
                if (UpdateDatabaseUser(dbaccounts)) return;//更新数据库用户
                //Trace.TraceInformation("OneSynchronization done!");
                //foreach (DbAccount dbaccount in dbaccounts)
                //{
                //    var tmp = dbaccount.code;
                //    tmp += ","+dbaccount.name;
                //    tmp += "," + dbaccount.email;
                //    tmp += "," + dbaccount.mobile;
                //    tmp += "," + dbaccount.pk_psndoc;
                //    tmp += "," +GetCategory( dbaccount.pk_psncl,personnelCategorys);
                //    tmp += "," + GetCategory(dbaccount.pk_post, posts);
                //    tmp += "," + GetCategory(dbaccount.pk_org, organizations);
                //    tmp += "," + GetCategory(dbaccount.pk_job, jobs);
                //    tmp += "," + GetCategory(dbaccount.pk_dept, departments);
                //    Trace.TraceInformation(tmp);
                //}
                Trace.TraceInformation("OneSynchronization all done!");
            }
            catch (Exception ex)
            {
                Trace.TraceError("alll error" + ex.Message, ex);
            }
        }

        //private void UpdateVaults(List<DbAccount> dbaccounts)
        //{
        //    var vaults = app.GetOnlineVaults();
        //    foreach (VaultOnServer vaultOnServer in vaults)
        //    {
        //        var vault = app.LogInToVault(vaultOnServer.GUID);
        //        var employee =
        //            vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemObjectType,
        //                "OtEmployee");
        //        var scs = new SearchConditions();
        //        var sc = new SearchCondition();
        //        sc.ConditionType = MFConditionType.MFConditionTypeEqual;
        //        sc.Expression.DataStatusValueType= MFStatusType.MFStatusTypeObjectTypeID;
        //        sc.TypedValue.SetValueToLookup(new Lookup{Item=employee} );
        //        scs.Add(-1,sc);
        //        var sr =
        //            vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(scs,
        //                MFSearchFlags.MFSearchFlagReturnLatestVisibleVersion, false, 0, 0).ObjectVersions;

        //        foreach (DbAccount dbaccount in dbaccounts)
        //        {
        //            dbaccount.matched = false;
        //        }
        //        var PropPersonnelClassification =
        //            vault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemPropertyDef,
        //                "PropPersonnelClassification");
        //        foreach (ObjectVersion objectVersion in sr)
        //        {
        //            var pvs = vault.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer);
        //            var code = pvs.SearchForPropertyByAlias(vault, "PropEmployeeCode", true);
        //            if (code == null) continue;
        //            foreach (DbAccount dbaccount in dbaccounts)
        //            {
        //                if (dbaccount.matched) continue;
        //                if (dbaccount.code == code.GetValueAsLocalizedText())
        //                {
        //                    var ov=vault.ObjectOperations.CheckOut(objectVersion.ObjVer.ObjID);
        //                    var newpvs = new PropertyValues();
        //                    var pv = new PropertyValue();
        //                    pv.PropertyDef = PropPersonnelClassification;
        //                    dbaccount.pk_psncl
        //                    pv.TypedValue.SetValueToLookup(new Lookup{Item=});
        //                    var ovap = vault.ObjectPropertyOperations.SetProperties(ov.ObjVer, newpvs);
        //                    vault.ObjectOperations.CheckIn(ovap.ObjVer);
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}

        private bool UpdateCompany(List<CommonInfo> organizations)
        {
            try
            {
                var sqlc = new SqlConnection(connstr);
                sqlc.Open();
                foreach (CommonInfo organization in organizations)
                {
                    var select = string.Format("select * from company where code = '{0}' ", organization.key);
                    var sqlcommand = new SqlCommand(select, sqlc);
                 //   var rowcount = sqlcommand.ExecuteNonQuery();
                     var rds = new SqlDataAdapter(sqlcommand);
                var dt = new DataTable();
                
                rds.Fill(dt);

                if (dt.Rows.Count > 0)
                    {
                        var update =
                            string.Format("update company set  name = '{1}' where code = '{0}' ", organization.key, organization.name);
                        sqlcommand = new SqlCommand(update, sqlc);
                        sqlcommand.ExecuteNonQuery();
                        Trace.TraceInformation("update company {0},{1} ok", organization.key, organization.name);
                    }
                    else
                    {
                        var insert =
                            string.Format(
                                "insert company (name,code) " +
                                "values( '{0}','{1}')   ",
                                organization.name, organization.key);

                        sqlcommand = new SqlCommand(insert, sqlc);
                        sqlcommand.ExecuteNonQuery();
                        Trace.TraceInformation("insert company {0},{1} ok", organization.key, organization.name);
                    }
                }

                sqlc.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("UpdateCompany:" + ex.Message);
                return true;
            }
            return false;
        }
        //private bool UpdateUserRole(List<CommonInfo> organizations)
        //{
        //    try
        //    {
        //        var sqlc = new SqlConnection(connstr);
        //        sqlc.Open();
        //        foreach (CommonInfo organization in organizations)
        //        {
        //            var select = string.Format("select * from userrole where name = '{0}' ", organization.key);
        //            var sqlcommand = new SqlCommand(select, sqlc);
        //            //   var rowcount = sqlcommand.ExecuteNonQuery();
        //            var rds = new SqlDataAdapter(sqlcommand);
        //            var dt = new DataTable();

        //            rds.Fill(dt);

        //            if (dt.Rows.Count > 0)
        //            {
        //                var update =
        //                    string.Format("update userrole set  displayname = '{1}' where name = '{0}' ", organization.key, organization.name);
        //                sqlcommand = new SqlCommand(update, sqlc);
        //                sqlcommand.ExecuteNonQuery();
        //                Trace.TraceInformation("update userrole {0},{1} ok", organization.key, organization.name);
        //            }
        //            else
        //            {
        //                var insert =
        //                    string.Format(
        //                        "insert userrole (name,displayname) " +
        //                        "values( '{0}','{1}')   ",
        //                        organization.key, organization.name);

        //                sqlcommand = new SqlCommand(insert, sqlc);
        //                sqlcommand.ExecuteNonQuery();
        //                Trace.TraceInformation("insert userrole {0},{1} ok", organization.key, organization.name);
        //            }
        //        }

        //        sqlc.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceInformation("UpdateUserRole:" + ex.Message);
        //        return true;
        //    }
        //    return false;
        //}
        private bool UpdatePositionInfo(List<CommonInfo> organizations)
        {
            try
            {
                var sqlc = new SqlConnection(connstr);
                sqlc.Open();
                foreach (CommonInfo organization in organizations)
                {
                    var select = string.Format("select * from PositionInfo where code = '{0}' ", organization.key);
                    var sqlcommand = new SqlCommand(select, sqlc);
                    var rds = new SqlDataAdapter(sqlcommand);
                    var dt = new DataTable();

                    rds.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        var update =
                            string.Format("update PositionInfo set  name = '{1}' where code = '{0}' ", organization.key, organization.name);
                        sqlcommand = new SqlCommand(update, sqlc);
                        sqlcommand.ExecuteNonQuery();
                        Trace.TraceInformation("update PositionInfo {0},{1} ok", organization.key, organization.name);
                    }
                    else
                    {
                        var insert =
                            string.Format(
                                "insert PositionInfo (name,code) " +
                                "values( '{0}','{1}')   ", organization.name,organization.key);

                        sqlcommand = new SqlCommand(insert, sqlc);
                        sqlcommand.ExecuteNonQuery();
                        Trace.TraceInformation("insert PositionInfo {0},{1} ok", organization.key, organization.name);
                    }
                }

                sqlc.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Update PositionInfo:" + ex.Message);
                return true;
            }
            return false;
        }
        private bool UpdateCscecRole(List<CommonInfo> organizations)
        {
            try
            {
                var sqlc = new SqlConnection(connstr);
                sqlc.Open();
                foreach (CommonInfo organization in organizations)
                {
                    var select = string.Format("select * from CscecRole where code = '{0}' ", organization.key);
                    var sqlcommand = new SqlCommand(select, sqlc);
                    var rds = new SqlDataAdapter(sqlcommand);
                    var dt = new DataTable();

                    rds.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        var update =
                            string.Format("update CscecRole set  name = '{1}' where code = '{0}' ", organization.key, organization.name);
                        sqlcommand = new SqlCommand(update, sqlc);
                        sqlcommand.ExecuteNonQuery();
                        Trace.TraceInformation("update CscecRole {0},{1} ok", organization.key, organization.name);
                    }
                    else
                    {
                        var insert =
                            string.Format(
                                "insert CscecRole (name,code) " +
                                "values( '{0}','{1}')   ", organization.name, organization.key);

                        sqlcommand = new SqlCommand(insert, sqlc);
                        sqlcommand.ExecuteNonQuery();
                        Trace.TraceInformation("insert CscecRole {0},{1} ok", organization.key, organization.name);
                    }
                }

                sqlc.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Update CscecRole:" + ex.Message);
                return true;
            }
            return false;
        }
        private bool UpdatePersonnelCategory(List<CommonInfo> organizations)
        {
            try
            {
                var sqlc = new SqlConnection(connstr);
                sqlc.Open();
                foreach (CommonInfo organization in organizations)
                {
                    var select = string.Format("select * from personnelcategory where code = '{0}' ", organization.key);
                    var sqlcommand = new SqlCommand(select, sqlc);
                    var rds = new SqlDataAdapter(sqlcommand);
                    var dt = new DataTable();

                    rds.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        var update =
                            string.Format("update personnelcategory set  name = '{1}' where code = '{0}' ", organization.key, organization.name);
                        sqlcommand = new SqlCommand(update, sqlc);
                        sqlcommand.ExecuteNonQuery();
                        Trace.TraceInformation("update personnelcategory {0},{1} ok", organization.key, organization.name);
                    }
                    else
                    {
                        var insert =
                            string.Format(
                                "insert personnelcategory (code,name) " +
                                "values( '{0}','{1}')   ",
                                organization.key, organization.name);

                        sqlcommand = new SqlCommand(insert, sqlc);
                        sqlcommand.ExecuteNonQuery();
                        Trace.TraceInformation("insert personnelcategory {0},{1} ok", organization.key, organization.name);
                    }
                }

                sqlc.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Update personnelcategory:" + ex.Message);
                return true;
            }
            return false;
        }
        private bool Updatedepartment(List<CommonInfo> organizations)
        {
            try
            {
                var sqlc = new SqlConnection(connstr);
                sqlc.Open();
                foreach (CommonInfo organization in organizations)
                {
                    var select = string.Format("select * from department where code = '{0}' ", organization.key);
                    var sqlcommand = new SqlCommand(select, sqlc);
                    var rds = new SqlDataAdapter(sqlcommand);
                    var dt = new DataTable();

                    rds.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        var update =
                            string.Format("update department set  name = '{1}' where code = '{0}' ", organization.key, organization.name);
                        sqlcommand = new SqlCommand(update, sqlc);
                        sqlcommand.ExecuteNonQuery();
                        Trace.TraceInformation("update department {0},{1} ok", organization.key, organization.name);
                    }
                    else
                    {
                        var insert =
                            string.Format(
                                "insert department (name,code) " +
                                "values( '{0}','{1}')   ",
                                organization.name, organization.key);

                        sqlcommand = new SqlCommand(insert, sqlc);
                        sqlcommand.ExecuteNonQuery();
                        Trace.TraceInformation("insert department {0},{1} ok", organization.key, organization.name);
                    }
                }

                sqlc.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Updatedepartment:" + ex.Message);
                return true;
            }
            return false;
        }

        private bool UpdateDatabaseUser(List<DbAccount> dbaccounts)
        {
            Trace.TraceInformation("connect str {0}", connstr);
            foreach (DbAccount dbaccount in dbaccounts)
            {
                if (UpdateOrInsertUser(dbaccount)) return true;
            }
            return false;
        }

        private bool UpdateOrInsertUser(DbAccount dbaccount)
        {
            SqlConnection sqlc=new SqlConnection();
            try
            {
                var companyid = GetCompanyId(dbaccount.pk_org);
                if (companyid < 0 || dbaccount.pk_org.Trim() == string.Empty)
                {
                    Trace.TraceInformation("company 信息不完整,{0},{1},{2},{3} ,discard:", dbaccount.pk_org, companyid,
                        dbaccount.name, dbaccount.code);
                    return false;
                }
                var departmentid = GetDepartmentId(dbaccount.pk_dept);
                if (departmentid < 0 || dbaccount.pk_dept.Trim() == string.Empty)
                {
                    Trace.TraceInformation("department 信息不完整,{0},{1},{2},{3} ,discard:", dbaccount.pk_dept, departmentid,
                        dbaccount.name, dbaccount.code);
                    return false;
                }
                var cscecrole = GetCscecRoleId(dbaccount.pk_job);
                if (cscecrole < 0 || dbaccount.pk_job.Trim() == string.Empty)
                {
                    Trace.TraceInformation("cscecrole 信息不完整,{0},{1},{2},{3} ,warning:", dbaccount.pk_job, cscecrole,
                        dbaccount.name, dbaccount.code);
                    cscecrole = 1;
                    //  return false;
                }
                var positioninfo = GetPositionId(dbaccount.pk_post);
                if (positioninfo < 0 || dbaccount.pk_post.Trim() == string.Empty)
                {
                    Trace.TraceInformation("positioninfo 信息不完整,{0},{1},{2},{3} ,warning:", dbaccount.pk_post,
                        positioninfo, dbaccount.name, dbaccount.code);
                    positioninfo = 1;
                    // return false;
                }
                var personnelcategory = GetPersonnelCategoryId(dbaccount.pk_psncl);
                if (personnelcategory < 0 || dbaccount.pk_psncl.Trim() == string.Empty)
                {
                    Trace.TraceInformation("personnelcategory 信息不完整,{0},{1},{2},{3} ,discard:", dbaccount.pk_psncl,
                        personnelcategory, dbaccount.name, dbaccount.code);
                    return false;
                }
              
                bool disabled = IsDisabled(dbaccount);
                var select = string.Format("select * from aecuser where username = '{0}' ", dbaccount.code);
                 sqlc = new SqlConnection(connstr);
                //   Trace.TraceInformation("aftr SqlConnection str {0}", connstr);
                sqlc.Open();
                var sqlcommand = new SqlCommand(select, sqlc);
                var rds = new SqlDataAdapter(sqlcommand);
                var dt = new DataTable();

                rds.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    var update =
                        string.Format(
                            "update aecuser set  email = '{1}',  phone = '{2}',disabled='{3}',CscecRoleId='{4}',PositionInfoId='{5}',PersonnelCategoryId='{6}'" +
                            ",CompanyId='{7}',DepartmentId='{8}' where username = '{0}' ",
                            dbaccount.code, dbaccount.email, dbaccount.mobile, disabled, cscecrole, positioninfo,
                            personnelcategory, companyid, departmentid);
                    sqlcommand = new SqlCommand(update, sqlc);
                    sqlcommand.ExecuteNonQuery();
                    //  Trace.TraceInformation(string.Format("update user {0},{1} ok", dbaccount.code, dbaccount.name));
                }
                else
                {

                    var insert =
                        string.Format(
                            "insert aecuser (UserName,Email,Disabled,MaxProjectCount,CreatedTimeUtc,LastActivityDateUtc,DomainUser,CompanyId,DepartmentId,FullName,Phone" +
                            ",PersonnelCategoryId,CscecRoleId,PositionInfoId) " +
                            "values( '{0}','{1}','{2}','{3}','{10}','{4}','{5}','{6}','{7}','{8}','{9}','{11}','{12}','{13}')   ",
                            dbaccount.code, dbaccount.email, disabled, 100, DateTime.Now.Date, 0, companyid,
                            departmentid, dbaccount.name,
                            dbaccount.mobile, DateTime.Now.Date, personnelcategory, cscecrole, positioninfo);
                    //   Trace.TraceInformation("insert sql :" + insert + "-pk_org:"+dbaccount.pk_org);

                    sqlcommand = new SqlCommand(insert, sqlc);
                    sqlcommand.ExecuteNonQuery();
                    Trace.TraceInformation(string.Format("insert user {0},{1} ok", dbaccount.code, dbaccount.name));
                }
                sqlc.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("UpdateOrInsertUser:" + ex.Message);
                sqlc.Close();
                return true;
            }
        
            return false;
        }

        private bool IsDisabled(DbAccount dbaccount)
        {
            switch (dbaccount.pk_psncl)
            {
                case "1001A210000000000KQQ":
                case "1001A210000000000G5Z":
                case "1001A210000000005SUN":
                case "1001A210000000000VF5":
                case "1001A210000000000WY2":
                case "1001A210000000000YGZ":
                case "1001A210000000000M9N":

                    return true;
                default:
                    return false;
            }
        }
        private int GetCscecRoleId(string p)
        {
            int ret = -1;
            try
            {
                var sqlc = new SqlConnection(connstr);
                sqlc.Open();
                var select = string.Format("select id from cscecrole where code = '{0}' ", p);
                var sqlcommand = new SqlCommand(select, sqlc);
                var rds = new SqlDataAdapter(sqlcommand);
                var dt = new DataTable();

                rds.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (DataColumn column in dt.Columns)
                        {
                            ret = int.Parse(row[column].ToString());
                            break;
                        }
                        break;
                    }
                }
                sqlc.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.Message);
            }
            return ret;
        }
        private int GetDepartmentId(string p)
        {
            int ret = -1;
            try
            {
                var sqlc = new SqlConnection(connstr);
                sqlc.Open();
                var select = string.Format("select id from department where code = '{0}' ", p);
                var sqlcommand = new SqlCommand(select, sqlc);
                var rds = new SqlDataAdapter(sqlcommand);
                var dt = new DataTable();

                rds.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (DataColumn column in dt.Columns)
                        {
                            ret =int.Parse(row[column].ToString());
                            break;
                        }
                        break;
                    }
                }
                sqlc.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.Message);
            }
            return ret;
        }
        private int GetPersonnelCategoryId(string p)
        {
            int ret = -1;
            try
            {
                var sqlc = new SqlConnection(connstr);
                sqlc.Open();
                var select = string.Format("select id from personnelcategory where code = '{0}' ", p);
                var sqlcommand = new SqlCommand(select, sqlc);
                var rds = new SqlDataAdapter(sqlcommand);
                var dt = new DataTable();

                rds.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (DataColumn column in dt.Columns)
                        {
                            ret = int.Parse(row[column].ToString());
                            break;
                        }
                        break;
                    }
                }
                sqlc.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.Message);
            }
            return ret;
        }
        private int GetPositionId(string p)
        {
            int ret = -1;
            try
            {
                var sqlc = new SqlConnection(connstr);
                sqlc.Open();
                var select = string.Format("select id from positioninfo where code = '{0}' ", p);
                var sqlcommand = new SqlCommand(select, sqlc);
                var rds = new SqlDataAdapter(sqlcommand);
                var dt = new DataTable();

                rds.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (DataColumn column in dt.Columns)
                        {
                            ret = int.Parse(row[column].ToString());
                            break;
                        }
                        break;
                    }
                }
                sqlc.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.Message);
            }
            return ret;
        }
        private int GetCompanyId(string p)
        {
            int ret = -1;
            try
            {
                var sqlc = new SqlConnection(connstr);
                sqlc.Open();
                var select = string.Format("select id from company where code = '{0}' ", p);
                var sqlcommand = new SqlCommand(select, sqlc);
                var rds = new SqlDataAdapter(sqlcommand);
                var dt = new DataTable();

                rds.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (DataColumn column in dt.Columns)
                        {
                            ret = int.Parse(row[column].ToString());
                            break;
                        }
                        break;
                    }
                }
                sqlc.Close();
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.Message);
            }
            return ret;
        }

        private string GetCategory(string p, List<CommonInfo> personnelCategorys)
        {
            foreach (CommonInfo personnelCategory in personnelCategorys)
            {
                if (personnelCategory.key == p) return personnelCategory.name;
            }
            return string.Empty;
        }
        private void GetStaffWorkInformation(string DataSource, List<DbAccount> dbaccounts)
        {

            var OracleConnectionconn = new OracleConnection(DataSource);//进行连接           
            try
            {
                OracleConnectionconn.Open();//打开指定的连接  
                OracleCommand com = OracleConnectionconn.CreateCommand();
                com.CommandText = "Select pk_psndoc ,pk_psncl  ,pk_dept  ,pk_job  ,pk_org ,pk_post  From bd_psnjob ";//写好想执行的Sql语句                   
                OracleDataReader odr = com.ExecuteReader();
                while (odr.Read())//读取数据，如果返回为false的话，就说明到记录集的尾部了                    
                {
                    var tmp = string.Empty;
                    var dbaccount = new DbAccount();
                    for (int i = 0; i < odr.FieldCount; i++)
                    {
                        var thefield = string.Empty;
                        try
                        {
                            thefield = odr.GetString(i);
                            tmp += thefield + ",";
                        }
                        catch { }
                        switch (i)
                        {
                            case 0:
                                dbaccount.pk_psndoc = thefield;
                                break;
                            case 1:
                                dbaccount.pk_psncl = thefield;
                                break;
                            case 2:
                                dbaccount.pk_dept = thefield;
                                break;
                            case 3:
                                dbaccount.pk_job = thefield;
                                break;
                            case 4:
                                dbaccount.pk_org = thefield;
                                break;
                            case 5:
                                dbaccount.pk_post = thefield;
                                break;
                        }
                    }
                    //   Trace.TraceInformation(tmp);
                    updateOneEmployee(dbaccount, dbaccounts);
                }
                odr.Close();//关闭reader.这是一定要写的  
            }
            catch (Exception eex)
            {
                Trace.TraceError("GetStaffWorkInformation operation:" + eex.Message);
            }
            finally
            {
                OracleConnectionconn.Close();//关闭打开的连接              
            }
        }

        private void updateOneEmployee(DbAccount dbaccount, List<DbAccount> dbaccounts)
        {
            foreach (DbAccount dbAccount in dbaccounts)
            {
                if (dbAccount.matched) continue;
                if (dbAccount.pk_psndoc == dbaccount.pk_psndoc)
                {
                    dbAccount.pk_dept = dbaccount.pk_dept;
                    dbAccount.pk_job = dbaccount.pk_job;
                    dbAccount.pk_org = dbaccount.pk_org;
                    dbAccount.pk_post = dbaccount.pk_post;
                    dbAccount.pk_psncl = dbaccount.pk_psncl;
                    dbAccount.matched = true;
                    break;
                }
            }
        }
        private List<CommonInfo> GetCommonInformation(string DataSource, string CommandText)
        {
            var dbaccounts = new List<CommonInfo>();
            var OracleConnectionconn = new OracleConnection(DataSource);//进行连接           
            try
            {
                OracleConnectionconn.Open();//打开指定的连接  
                OracleCommand com = OracleConnectionconn.CreateCommand();
                com.CommandText = CommandText;//写好想执行的Sql语句                   
                OracleDataReader odr = com.ExecuteReader();
                while (odr.Read())//读取数据，如果返回为false的话，就说明到记录集的尾部了                    
                {
                    var tmp = string.Empty;
                    var dbaccount = new CommonInfo();
                    for (int i = 0; i < odr.FieldCount; i++)
                    {
                        var thefield = string.Empty;
                        try
                        {
                            thefield = odr.GetString(i);
                            tmp += thefield + ",";
                        }
                        catch { }
                        switch (i)
                        {
                            case 0:
                                dbaccount.key = thefield;
                                break;
                            case 1:
                                dbaccount.name = thefield;
                                break;
                        }
                    }
                    dbaccounts.Add(dbaccount);
                    //   Trace.TraceInformation(tmp);
                }
                odr.Close();//关闭reader.这是一定要写的  
            }
            catch (Exception eex)
            {
                Trace.TraceError("GetCommonInformation operation:" + CommandText + eex.Message);
            }
            finally
            {
                OracleConnectionconn.Close();//关闭打开的连接              
            }
            return dbaccounts;
        }
        private List<DbAccount> GetStaffBasicInformation(string DataSource)
        {
            var dbaccounts = new List<DbAccount>();
            var OracleConnectionconn = new OracleConnection(DataSource);//进行连接           
            try
            {
                OracleConnectionconn.Open();//打开指定的连接   
                OracleCommand com = OracleConnectionconn.CreateCommand();
                com.CommandText = "Select pk_psndoc ,code ,name ,email ,mobile  From bd_psndoc ";//写好想执行的Sql语句                   
                OracleDataReader odr = com.ExecuteReader();
                while (odr.Read())//读取数据，如果返回为false的话，就说明到记录集的尾部了                    
                {
                    var tmp = string.Empty;
                    var dbaccount = new DbAccount();
                    for (int i = 0; i < odr.FieldCount; i++)
                    {
                        var thefield = string.Empty;
                        try
                        {
                            thefield = odr.GetString(i);
                            tmp += thefield + ",";
                        }
                        catch { }
                        switch (i)
                        {
                            case 0:
                                dbaccount.pk_psndoc = thefield;
                                break;
                            case 1:
                                dbaccount.code = thefield;
                                break;
                            case 2:
                                dbaccount.name = thefield;
                                break;
                            case 3:
                                dbaccount.email = thefield;
                                break;
                            case 4:
                                dbaccount.mobile = thefield;
                                break;
                        }
                    }
                    dbaccounts.Add(dbaccount);
                    //  Trace.TraceInformation(tmp);
                }
                odr.Close();//关闭reader.这是一定要写的  
            }
            catch (Exception eex)
            {
                Trace.TraceError("oracle operation:" + eex.Message);
            }
            finally
            {
                OracleConnectionconn.Close();//关闭打开的连接              
            }
            return dbaccounts;
        }

        private void syncAccounts(List<MfilesLogAccount> allaccount, List<DbAccount> dbaccounts)
        {
            foreach (DbAccount dbaccount in dbaccounts)
            {
                mfilesAccountProcessing(allaccount, dbaccount);
            }
            foreach (MfilesLogAccount mfilesLogAccount in allaccount)//disable useless account
            {
                if (mfilesLogAccount.matched) continue;
                try
                {
                    mfilesLogAccount.Account.Enabled = false;
                    app.LoginAccountOperations.ModifyLoginAccount(mfilesLogAccount.Account);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("ModifyLoginAccount disable error:" + ex.Message, ex);
                }

            }
        }

        private void mfilesAccountProcessing(List<MfilesLogAccount> allaccount, DbAccount dbaccount)
        {
            var notfound = true;
            foreach (MfilesLogAccount mfilesLogAccount in allaccount)
            {
                if (mfilesLogAccount.matched) continue;
                if (mfilesLogAccount.name == dbaccount.code)
                {
                    checkUpdate(mfilesLogAccount, dbaccount);
                    mfilesLogAccount.matched = true;
                    notfound = false;
                    break;
                }
            }
            if (notfound)//add new account
            {
                try
                {
                    app.LoginAccountOperations.AddLoginAccount(
                        new LoginAccount
                        {
                            AccountType = MFLoginAccountType.MFLoginAccountTypeMFiles,
                            FullName = dbaccount.name,
                            Enabled = true,
                            LicenseType = MFLicenseType.MFLicenseTypeConcurrentUserLicense,
                            ServerRoles = MFLoginServerRole.MFLoginServerRoleLogIn,
                            UserName = dbaccount.code,
                            EmailAddress = dbaccount.email
                        }, "admin");
                }
                catch (Exception ex)
                {
                    Trace.TraceError("AddLoginAccount  error:" + dbaccount.name, ex);
                }
            }
        }

        private void checkUpdate(MfilesLogAccount mfilesLogAccount, DbAccount dbaccount)
        {
            var updateflag = false;
            if (mfilesLogAccount.email != dbaccount.email) updateflag = true;
            if (mfilesLogAccount.fullname != dbaccount.name) updateflag = true;
            if (updateflag)
            {
                mfilesLogAccount.Account.FullName = dbaccount.name;
                mfilesLogAccount.Account.EmailAddress = dbaccount.email;
                try
                {
                    app.LoginAccountOperations.ModifyLoginAccount(mfilesLogAccount.Account);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("ModifyLoginAccount error:" + ex.Message, ex);
                }
            }
        }

        private List<MfilesLogAccount> GetAllAccounts()
        {

            var accounts = app.LoginAccountOperations.GetLoginAccounts();
            var ret = new List<MfilesLogAccount>();
            foreach (LoginAccount loginAccount in accounts)
            {
                if (loginAccount.AccountName == "admin" || loginAccount.UserName == "admin") continue;
                ret.Add(new MfilesLogAccount { email = loginAccount.FullName, fullname = loginAccount.FullName, name = loginAccount.AccountName, Account = loginAccount });
            }
            return ret;
        }

        private string GetTraceFile()
        {
            var temp = Path.GetTempPath();
            var date = DateTime.Now.Date.ToString("yyyy-MM-dd");
            var traceFile = temp + "\\SynchronizationLog" + date + ".txt";
            return traceFile;
        }

    }
}
