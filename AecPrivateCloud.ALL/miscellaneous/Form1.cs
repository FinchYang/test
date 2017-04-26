using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore;
using AecCloud.MfilesServices;
using AecCloud.Service.Projects;
using AecCloud.Service.Users;
using MFilesAPI;

namespace miscellaneous
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonenableusers_Click(object sender, EventArgs e)
        {
            try
            {
                var app = new MFilesServerApplication();
                app.Connect(MFAuthType.MFAuthTypeSpecificMFilesUser, textBoxuser.Text, textBoxpass.Text, "",
                    "ncacn_ip_tcp",
                    textBoxmfserver.Text);
                var accs = app.LoginAccountOperations.GetLoginAccounts();
                foreach (LoginAccount acc in accs)
                {
                    acc.Enabled = true;
                    app.LoginAccountOperations.ModifyLoginAccount(acc);
                    richTextBox1.AppendText(Environment.NewLine + string.Format("{0},{1},{2} enabled", acc.FullName, acc.UserName, acc.AccountName));
                }
                app.Disconnect();
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText(Environment.NewLine + ex.Message);
            }
        }
        class CreateRes
        {
            public string Err { get; set; }

            public MFilesVault Vault { get; set; }
        }
        private void CreateVaultForAllBackup(MFilesVault vault, VaultTemplate template, MFSqlDatabase sqlDb, IMFVaultService _vaultService)
        {
            //3. 创建相应的M-Files库
            try
            {


                _vaultService.CreateForAllBackup(vault, template.StructurePath, textBoxuser.Text, textBoxpass.Text, sqlDb, null);
            }
            catch (Exception ex)
            {
                //  Log.Error("创建项目库失败：" + ex.Message, ex);
                throw;
            }
        }
        private static void CreateHeadquatersStaff(Vault mVault, IList<global::AecCloud.Core.Domain.User> list, List<Company> companies, Project proj)
        {
            //  Log.Info(string.Format("in CreateHeadquatersStaff, {0}", mVault.Name));
            var UgHLeaders =
                mVault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                    MfilesAliasConfig.UgHLeaders);
            if (UgHLeaders < 0)
            {
                //   Log.Info(string.Format("CreateHeadquatersStaff, alias {0} is not found  in vault {1}", MfilesAliasConfig.UgHLeaders, mVault.Name));
                return;
            }
            try
            {
                foreach (Company headquarter in companies)
                {
                    if (headquarter.Code == "0001A210000000002OSD")// 0001A210000000002OSD,总部机关, 0001A210000000002ORS,中建八局第二建设有限公司,
                    {
                        var ug = mVault.UserGroupOperations.GetUserGroupAdmin(UgHLeaders);
                        var headquarterusercount = 0;
                        var correctcount = 0;
                        foreach (User user in list)
                        {
                            if (user.CompanyId == headquarter.Id)
                            {
                                headquarterusercount++;
                                var account = new UserAccount
                                {
                                    LoginName = user.UserName,
                                    InternalUser = true,
                                    VaultRoles = MFUserAccountVaultRole.MFUserAccountVaultRoleDefaultRoles
                                };
                                if (user.Disabled) account.Enabled = false;
                                try
                                {
                                    //   account.AddVaultRoles(MFUserAccountVaultRole.MFUserAccountVaultRoleManageCommonViews);
                                    var ua = mVault.UserOperations.AddUserAccount(account);
                                    if (user.Department.Code == "1001A210000000001M1E") //公司领导{
                                    {
                                        ug.UserGroup.AddMember(ua.ID);
                                    }
                                    //  if(user.CscecRoleId)
                                    UserGroupProcessing(ua, user, mVault);
                                    //      _projectMemberService.AddMember(proj.Id, user.Id, 0);
                                    //       _projectMemberService.AddMember(proj.Id, user.Id, 0, false, false);
                                    correctcount++;
                                }
                                catch (Exception) { }
                            }
                        }
                        mVault.UserGroupOperations.UpdateUserGroupAdmin(ug);
                        //  Log.Info(string.Format("CreateHeadquatersStaff vault {0}, all headquarter employees {1}, created users {2}", mVault.Name, headquarterusercount, correctcount));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //  Log.Info("CreateHeadquatersStaff error：" + ex.Message);
            }
        }
        private static void UserGroupProcessing(UserAccount ua, global::AecCloud.Core.Domain.User user, Vault vault)
        {
            try
            {
                //  Log.Info(string.Format("UserGroupProcessing  UserAccount {0},aecUser {1},vault {2},FullName={3},Company={4},PositionInfo={5},Department={6}", ua.LoginName, user.CscecRole.Name,
                //   vault.Name, user.FullName, user.Company.Name, user.PositionInfo.Name, user.Department.Name));
                var ugs = vault.UserGroupOperations.GetUserGroups();

                var ugkey_department = new string[]
                {
                    "安全环境部","安全生产管理部",
                    "施工管理部","工程管理部",
                    "成本管理部","工程结算部","商务管理部",
                    "工会","工会工作部",
                    "党委工作部","政工部",
                     "纪检监察室", "纪检监察部",
                    "办公室","人力资源部","市场一部","市场二部","物资部","技术中心","财务部","审计部", "合约法务部","投资与资金部","投标管理部","科技部","海外部"
                };
                foreach (string s in ugkey_department)
                {
                    if (user.Department.Name != s) continue;
                    var groupidentifer = s;
                    switch (s)
                    {
                        case "安全环境部":
                            groupidentifer = "安全生产管理部";
                            break;
                        case "施工管理部":
                            groupidentifer = "工程管理部";
                            break;
                        case "成本管理部":
                        case "工程结算部":
                            groupidentifer = "商务管理部";
                            break;
                        case "工会":
                            groupidentifer = "工会工作部";
                            break;
                        case "党委工作部":
                        case "政工部":
                            groupidentifer = "党委工作部";
                            break;
                        case "纪检监察室":
                            groupidentifer = "纪检监察部";
                            break;
                    }
                    foreach (UserGroup userGroup in ugs)
                    {
                        if (userGroup.Name == "公司总部-" + groupidentifer)
                        {
                            AddUserIntoGroup(ua, userGroup, vault);
                            break;
                        }
                    }
                    if ((user.CscecRole.Name == "经理" || user.PositionInfo.Name == "经理" || user.CscecRole.Name == "主任" || user.PositionInfo.Name == "主任"))
                    {
                        foreach (UserGroup userGroup in ugs)
                        {
                            if (userGroup.Name.Contains("公司总部-" + groupidentifer + "经理"))
                            {
                                AddUserIntoGroup(ua, userGroup, vault);
                                break;
                            }
                        }
                    }
                }

                var ugkey = new string[]
                {
                    "总经理", "副总经理", "总工程师", "党委副书记", "总经济师", "董事长", "党委书记","纪委书记","工会主席"
                };
                foreach (string s in ugkey)
                {
                    if (user.CscecRole.Name == s || user.PositionInfo.Name == s || user.Department.Name == s)
                    {
                        //   Log.Info(string.Format("UserGroupProcessing  UserAccount {0},aecUser {1},vault {2},match group {3}",ua.LoginName, user.CscecRole.Name, vault.Name, s));
                        foreach (UserGroup userGroup in ugs)
                        {
                            if (userGroup.Name.Contains("公司总部-" + s))
                            {
                                AddUserIntoGroup(ua, userGroup, vault);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //  Log.Info(string.Format("UserGroupProcessing error {0},{1},{2}:" + ex.Message, user.UserName, user.FullName, ua.ID));
            }
        }
        private static void AddUserIntoGroup(UserAccount ua, UserGroup userGroup, Vault vault)
        {
            try
            {
                var uga = vault.UserGroupOperations.GetUserGroupAdmin(userGroup.ID);
                uga.UserGroup.AddMember(ua.ID);
                vault.UserGroupOperations.UpdateUserGroupAdmin(uga);
                //  Log.Info(string.Format("AddUser {0}  IntoGroup {1} UpdateUserGroupAdmin ok {2}", ua.LoginName,userGroup.Name, vault.Name));
            }
            catch (Exception ex)
            {
                //     Log.Info(string.Format("AddUser {0}  IntoGroup {1} UpdateUserGroupAdmin error {2},{3}", ua.LoginName,userGroup.Name, vault.Name, ex.Message));
            }
        }
        private static void RemoveSecondLevelGroups(Vault mVault)
        {
            try
            {
                var groups = mVault.UserGroupOperations.GetUserGroups();
                foreach (UserGroup userGroup in groups)
                {
                    if (userGroup.Name.StartsWith("二级单位"))
                    {
                        mVault.UserGroupOperations.RemoveUserGroupAdmin(userGroup.ID);
                    }
                }
            }
            catch (Exception ex)
            {
                //  Log.Info("RemoveSecondLevelGroups error:" + ex.Message);
            }
        }
        private static void SecondLevelUserAndGroupProcessing(Vault mVault, IList<global::AecCloud.Core.Domain.User> users, List<Company> companies, Project proj)
        {
            var ugkey_department = new string[]
                {
                   "施工管理部","工程部","经营部","商务部","物资部","财务部","办公室","综合部","生产部","物资合约部","租赁经营部","幕墙设计部","装饰设计部","领导班子"
             
                };

            var UgSecondLevelLeaders =
                mVault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                    MfilesAliasConfig.UgSecondLevelLeaders);
            var uga = mVault.UserGroupOperations.GetUserGroupAdmin(UgSecondLevelLeaders);
            var ugs = mVault.UserGroupOperations.GetUserGroups();
            var allcount = 0;
            var invaultcount = 0;
            foreach (User user in users)
            {
                if (user.CompanyId != proj.Company.Id) continue;
                allcount++;
                foreach (string s in ugkey_department)
                {
                    if (user.Department.Name != s) continue;
                    var account = new UserAccount
                    {
                        LoginName = user.UserName,
                        InternalUser = true,
                        VaultRoles = MFUserAccountVaultRole.MFUserAccountVaultRoleDefaultRoles
                    };

                    try
                    {
                        var ua = mVault.UserOperations.AddUserAccount(account);
                        //    Log.Info(string.Format("SecondLevelUserAndGroupProcessing  UserAccount {0},aecUser {1},vault {2},FullName={3},Company={4},PositionInfo={5},Department={6}", ua.LoginName, user.CscecRole.Name,
                        // mVault.Name, user.FullName, user.Company.Name, user.PositionInfo.Name, user.Department.Name));
                        if (user.Department.Name.Contains("领导班子"))
                        {
                            uga.UserGroup.AddMember(ua.ID);
                        }


                        var groupidentifer = (s == "施工管理部" || s == "施工管理部") ? "工程管理部" : s;
                        if (user.CscecRole.Name == "经理" || user.PositionInfo.Name == "经理" || user.CscecRole.Name == "主任" || user.PositionInfo.Name == "主任")
                        {
                            foreach (UserGroup userGroup in ugs)
                            {
                                if (userGroup.Name.Contains("二级单位-" + groupidentifer + "经理"))
                                {
                                    AddUserIntoGroup(ua, userGroup, mVault);
                                    break;
                                }
                            }
                        }
                        foreach (UserGroup userGroup in ugs)
                        {
                            if (userGroup.Name == "二级单位-" + groupidentifer)
                            {
                                AddUserIntoGroup(ua, userGroup, mVault);
                                break;
                            }
                        }
                        SecondLevelGroupProcessing(ua, user, mVault);

                        //      _projectMemberService.AddMember(proj.Id, user.Id, 0, false, false);
                        invaultcount++;
                        break;
                    }
                    catch (Exception) { }
                }
            }
            mVault.UserGroupOperations.UpdateUserGroupAdmin(uga);
            //     Log.Info(string.Format("SecondLevelUserAndGroupProcessing vault {0}, all employees {1}, created users {2},{3},{4}", mVault.Name, allcount, invaultcount, proj.Company.Name, proj.Name));

        }
        private static void SecondLevelGroupProcessing(UserAccount ua, global::AecCloud.Core.Domain.User user, Vault vault)
        {
            try
            {
                var ugs = vault.UserGroupOperations.GetUserGroups();

                var ugkey_chiefpostion = new string[]
                {
                   "总工程师", "总会计师", "总经济师"
             
                };
                foreach (string chief in ugkey_chiefpostion)
                {
                    if ((user.CscecRole.Name.Contains(chief) || user.PositionInfo.Name.Contains(chief)) &&
                        user.Department.Name == "领导班子")
                    {
                        foreach (UserGroup userGroup in ugs)
                        {
                            if (userGroup.Name.Contains("二级单位-" + chief))
                            {

                                AddUserIntoGroup(ua, userGroup, vault);
                                break;
                            }
                        }
                    }
                }

                ManagerGroupProcessing("经理", ugs, user, vault, ua);
                ManagerGroupProcessing("书记", ugs, user, vault, ua);
                var ugkey = new string[]
                {
                    "分公司经理", "副经理",  "副经理（生产）", "工会主席",
                "财务部","审计部"
             
                };
                foreach (string s in ugkey)
                {
                    if (user.CscecRole.Name == s || user.Department.Name == s || user.PositionInfo.Name == s)
                    {

                        foreach (UserGroup userGroup in ugs)
                        {
                            if (userGroup.Name.Contains("二级单位-" + s))
                            {
                                AddUserIntoGroup(ua, userGroup, vault);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //     Log.Info(string.Format("SecondLevelGroupProcessing error {0},{1},{2}:" + ex.Message, user.UserName, vault.Name, ua.ID));
            }
        }
        private static void ManagerGroupProcessing(string smanager, UserGroups ugs, global::AecCloud.Core.Domain.User user, Vault vault, UserAccount ua)
        {
            if ((user.CscecRole.Name == smanager || user.PositionInfo.Name == smanager) && user.Department.Name == "领导班子")
            {
                foreach (UserGroup userGroup in ugs)
                {
                    if (userGroup.Name.Contains("二级单位-" + smanager))
                    {
                        var uga = vault.UserGroupOperations.GetUserGroupAdmin(userGroup.ID);
                        uga.UserGroup.AddMember(ua.ID);
                        vault.UserGroupOperations.UpdateUserGroupAdmin(uga);
                        break;
                    }
                }
            }
        }
        public class UserGroupDb
        {
            public UserGroupDb()
            {
                Dbusers = new List<Dbuser>();
            }
            public long Id { get; set; }
            public long CompanyId { get; set; }
            public long UserId { get; set; }
            public long GroupId { get; set; }
            public string GroupName { get; set; }
            public List<Dbuser> Dbusers { get; set; }
        }
        public class Dbuser
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
        private string GetMfGroupName(long groupid)
        {
            //   var connstr = ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString;
            var ret = string.Empty;
            var sqlc = new SqlConnection(textBoxsqlserver.Text);
            sqlc.Open();
            try
            {
                var select = string.Format("select name from groupcategory where id = '{0}' ", groupid);
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

                            ret = row[column].ToString();
                            break;

                        }
                        break;
                    }
                }
                else
                {
                    //   Log.Info(select + "GetMfGroupName there is no name for --" + groupid);

                }
            }
            catch (Exception ex)
            {
                //   Log.Info("GetMfGroupName error:" + ex.Message);
            }
            sqlc.Close();
            return ret;
        }

        private void SetVicePresidentByCompany(Project proj, SqlConnection sqlc, IUserService _userService, IMfUserGroupService _mfusergroupService, long companyid)
        {
            try
            {
                var select1 = string.Format("select userid,groupid from usergroup where id = '{0}' ", companyid);
                var sqlcommand1 = new SqlCommand(select1, sqlc);
                var rds1 = new SqlDataAdapter(sqlcommand1);
                var dt1 = new DataTable();
                rds1.Fill(dt1);
                if (dt1.Rows.Count > 0)
                {
                    foreach (DataRow row in dt1.Rows)
                    {
                        var index = 0;
                        var userg = new UserGroupDb();
                        foreach (DataColumn column in dt1.Columns)
                        {
                            switch (index)
                            {
                                case 0:
                                    userg.UserId = int.Parse(row[column].ToString());
                                    break;
                                case 1:
                                    userg.GroupId = int.Parse(row[column].ToString());
                                    break;
                            }
                            index++;
                        }
                        var username = _userService.GetUserById(userg.UserId).UserName;
                        string mfigroupid = GetMfGroupName(userg.GroupId);
                        _mfusergroupService.AddUserToGroup(proj.Vault, username, mfigroupid);
                    }
                }
            }
            catch (Exception ex)
            {
                //   Log.Info(companyid + "SetVicePresidentByCompany error:" + ex.Message);
            }
        }

        private void SetVicePresident(Vault mVault, Project proj, IUserService _userService, IMfUserGroupService _mfusergroupService)
        {
            //   var connstr = ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString;

            var sqlc = new SqlConnection(textBoxsqlserver.Text);
            sqlc.Open();
            SetVicePresidentByCompany(proj, sqlc, _userService, _mfusergroupService, proj.CompanyId);
            if (proj.Company.Code != "0001A210000000002ORS")
            {

                SetVicePresidentByCompany(proj, sqlc, _userService, _mfusergroupService, 1);
            }
            //}
            //catch (Exception ex)
            //{
            //    Log.Info("SetVicePresident error:" + ex.Message);
            //}
            SetErpPmUser(proj, sqlc, _userService, _mfusergroupService, mVault);
            sqlc.Close();
        }
        private static void SetErpPmUser(Project proj, SqlConnection sqlc, IUserService _userService, IMfUserGroupService _mfusergroupService, Vault ovault)
        {
            try
            {
                var select1 = string.Format("select userid from erppm  ");
                var sqlcommand1 = new SqlCommand(select1, sqlc);
                var rds1 = new SqlDataAdapter(sqlcommand1);
                var dt1 = new DataTable();
                rds1.Fill(dt1);
                var userid = 1;
                if (dt1.Rows.Count > 0)
                {
                    foreach (DataRow row in dt1.Rows)
                    {
                        foreach (DataColumn column in dt1.Columns)
                        {
                            userid = int.Parse(row[column].ToString());
                            break;

                        }
                        var username = _userService.GetUserById(userid).UserName;
                        var userId = _mfusergroupService.GetUserId(ovault, username);
                        var ugid =
                            ovault.GetMetadataStructureItemIDByAlias(
                                MFMetadataStructureItem.MFMetadataStructureItemUserGroup,
                                MfilesAliasConfig.UgErpPrincipal);
                        _mfusergroupService.AddUserToGroup(ovault, userId, ugid);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //   Log.Info("SetErpPmUser error:" + ex.Message);
            }
        }


        private void buttoncreateprojects_Click(object sender, EventArgs e)
        {
            var cr = new CreateRes();
            var vault = new MFilesVault();
            CreateVaultForAllBackup(vault, new VaultTemplate(), null, new MFVaultService());
            cr.Vault = vault;


            try
            {
                var server = vault.Server;
                var app = MFServerUtility.ConnectToServer(server.AdminName, server.AdminPwd,
                    MFServerUtility.GetVaultServerLocalIp(server), server.ServerPort);
                var mVault = app.LogInToVault(vault.Guid);
                var views = mVault.ViewOperations.GetViews();

                IList<global::AecCloud.Core.Domain.User> users = GetAllUsers();
                List<Company> companies = GetAllCompany();
                var proj = new Project();
                CreateHeadquatersStaff(mVault, users, companies, proj);
                //    UserAndGroupProcessing(mVault, proj.CompanyId);//need modification,to be continued//deprecated
                if (proj.Company.Code == "0001A210000000002ORS") //0001A210000000002ORS,中建八局第二建设有限公司,
                {

                }
                else //二级单位
                {
                    SecondLevelUserAndGroupProcessing(mVault, users, companies, proj);

                }

                // SetVicePresident(mVault, proj, new UserService( ), new MfUserGroupService() );

            }
            catch (Exception ex)
            {
                var err = " private static CreateRes CreateProject 创建库中vaultapp error：" + ex.Message;
                //    Log.Error(err);
                throw;
            }


        }

        private List<Company> GetAllCompany()
        {
            throw new NotImplementedException();
        }

        private IList<User> GetAllUsers()
        {
            var ret = new List<User>();
            try
            {
                using (SqlConnection connection = new SqlConnection(textBoxsqlserver.Text))
                using (SqlCommand command = new SqlCommand("SELECT Id, username, companyid, disabled, departmentid FROM aecuser",connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var a = reader.GetString(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Something went wrong while opening a connection tothe database: { ex.Message }");
            }
            return ret;
        }


    }
}
