using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

using System.Web.Mvc;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.MfilesServices;
using AecCloud.Service.Projects;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using AecCloud.WebAPI.Models;

using DBWorld.AecCloud.Web.Models;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;

namespace DBWorld.AecCloud.Web.Controllers
{
    [System.Web.Mvc.Authorize]
    public class ManageController : BaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IProjectMemberService _projMemberService;
        private readonly IProjectService _projService;
        private readonly IUserService _userService;
        private readonly IMFilesVaultService _mfvaultService;
        private readonly IVaultServerService _mfvaultServerService;
        private readonly IVaultTemplateService _vaultTempService;
        private readonly IRepository<Company> _companyRepo;
        private readonly IRepository<Department> _departmentRepo;
        public ManageController(IAuthenticationManager authManager, SignInManager<User, long> signInManager, UserManager<User, long> userManager
            , IProjectMemberService projMemberService, IProjectService projService, IUserService userService,IVaultServerService mfvaultServerService
            , IMFilesVaultService mfvaultService, IVaultTemplateService vaultTempService
            , IRepository<Company> companyRepo, IRepository<Department> departmentRepo)
            : base(authManager, signInManager, userManager)
        {
            _projMemberService = projMemberService;
            _projService = projService;
            _userService = userService;
            _mfvaultService = mfvaultService;
            _mfvaultServerService = mfvaultServerService;
            _vaultTempService = vaultTempService;
            _companyRepo = companyRepo;
            _departmentRepo = departmentRepo;
        }
        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }
       
        public ActionResult ChangePassword(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "密码已成功修改."
                : message == ManageMessageId.Error ? "密码修改没有成功."
                : "";
            ViewBag.HasLocalPassword =true;
            ViewBag.ReturnUrl = Url.Action("ChangePassword");
            SimpleLog("in get changepassword");
            var m = new DBWorld.AecCloud.Web.Models.ChangePasswordViewModel();
            return View(m);
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        //
        // POST: /Account/Manage
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            bool hasPassword = true;
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("ChangePassword");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    SimpleLog("in post changepassword");
                    SimpleLog("in post changepassword" + model.OldPassword);
                    SimpleLog("in post changepassword" + model.NewPassword);
                    SimpleLog("in post changepassword" + model.ConfirmPassword);
                    var result = true;
                    try
                    {
                        var userid = User.Identity.GetUserName();
                        SimpleLog("in post 11 name="+userid);
                        var pass = DBWorldCache.Get(User.Identity.GetUserId());
                        SimpleLog("in post 22 pass="+pass);
                        var vs = _mfvaultServerService.GetServer();
                        SimpleLog("in post 33"+vs.Ip+vs.Port);
                       // var app = MFServerUtility.ConnectToServer(userid, model.OldPassword, vs.Ip, vs.Port);
                        var app = MFServerUtility.ConnectToServer(vs);
                        SimpleLog("in post 44");
                        app.LoginAccountOperations.UpdateLoginPassword(userid, model.NewPassword);
                        SimpleLog("in post 55");
                    }
                    catch (Exception ex)
                    {
                        SimpleLog(ex.Message);
                         result = false;
                    }
                    
                    if (result)
                    {
                        SimpleLog("in post success");
                        return RedirectToAction("ChangePassword", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        SimpleLog("in post error");
                       // AddErrors(result);
                        return RedirectToAction("ChangePassword", new { Message = ManageMessageId.Error });
                    }
                }
            }
           

            // If we got this far, something failed, redisplay form
            return View(model);
        }
      

        //[HttpPost]
        //public ActionResult ChangePassword(ChangePasswordViewModel model)
        //{
        //     SimpleLog("in post changepassword");
        //     SimpleLog("in post changepassword"+model.OldPassword);
        //     SimpleLog("in post changepassword" + model.NewPassword);
        //     SimpleLog("in post changepassword" + model.ConfirmPassword);

        //     try
        //     {
        //         var userid = User.Identity.GetUserName();
        //         SimpleLog("in post 11");
        //         var pass = DBWorldCache.Get(User.Identity.GetUserId());
        //         SimpleLog("in post 22");
        //         var vs = _mfvaultServerService.GetServer();
        //         SimpleLog("in post 33");
        //         var app = MFServerUtility.ConnectToServer(userid, pass, vs.Ip, vs.Port);
        //         SimpleLog("in post 44");
        //         app.LoginAccountOperations.UpdateLoginPassword(userid, model.NewPassword);
        //         SimpleLog("in post 55");
        //     }
        //     catch (Exception ex)
        //     {
        //         return View(model);
        //     }
        //     return RedirectToAction("Welcome", "Manage");
        //}


        //[HttpPost]
        //public async Task<ActionResult> CheckProjectCreationPermission(int userid)
        //{
        //    var res = await Task.Run(() => ProjectCreationPermission(userid));

        //    if (!String.IsNullOrEmpty(res))
        //    {
        //        return Content(res);
        //    }
        //    return Content("success");
        //}
        //private string ProjectCreationPermission(int userid)
        //{
        //    var connstr = ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString;
        //    var ret = string.Empty;
        //    var sqlc = new SqlConnection(connstr);
        //    sqlc.Open();
        //    try
        //    {
        //        var select = string.Format("select id from erppm where userid ={0} ", userid);
        //        var sqlcommand = new SqlCommand(select, sqlc);
        //        var rds = new SqlDataAdapter(sqlcommand);
        //        var dt = new DataTable();
        //        rds.Fill(dt);

        //        if (dt.Rows.Count <1)
        //        {
        //            ret = string.Format("您没有创建项目的权限，请联系公司总部工程管理部ERP立项信息负责人! ");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Info(string.Format("ProjectCreationPermission {0}  ", ex.Message));
        //        ret = ex.Message;
        //    }
        //    sqlc.Close();
        //    return ret;
        //}
        public ActionResult GetDbUserGroup(string code)
        {
            var connstr = ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString;
            Log.Info("--" + connstr);
            var sqlc = new SqlConnection(connstr);
            Log.Info("333");
            sqlc.Open();
          
            var ugs = new List<UserGroupDb>();
            try
            {
               var companyid = int.Parse(GetCompanyId(sqlc, code));
                Log.Info("555");
                var select = string.Format("select * from usergroup where companyid = '{0}' ", companyid);
                var sqlcommand = new SqlCommand(select, sqlc);
                var rds = new SqlDataAdapter(sqlcommand);
                var dt = new DataTable();
                rds.Fill(dt);
                
           if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var index = 0;
                        var userg = new UserGroupDb();
                        foreach (DataColumn column in dt.Columns)
                        {
                            switch (index)
                            {
                                case 0:
                                    userg.Id = int.Parse(row[column].ToString());
                                    break;
                                case 1:
                                    userg.CompanyId = int.Parse(row[column].ToString());
                                    break;
                                case 2:
                                    userg.UserId = int.Parse(row[column].ToString());
                                    break;
                                case 3:
                                    userg.GroupId = int.Parse(row[column].ToString());
                                    break;
                            }
                            index++;
                        }
                        userg.GroupName = GetUgName(sqlc, userg.GroupId);
                        userg.Dbusers = GetUsersForUg(sqlc,  code);
                        ugs.Add(userg);
                    }
                }
           else
           {
               Log.Info(select+"GetDbUserGroup there is no usegroup for --" + companyid);
           }
              
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("ConCompanyUg {0}  ", ex.Message));
            }
            sqlc.Close();
           // return Json(ugs,JsonRequestBehavior.AllowGet);
            var json = JsonConvert.SerializeObject(ugs,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.None
                });
            return Content(json);
        }
        [HttpPost]
        public async Task<ActionResult> PermissionSeterp( int userid)
        {
            var res = await Task.Run(() => UpdateDbUsererp( userid));

            if (!String.IsNullOrEmpty(res))
            {
                return Content(res);
            }
            return Content("success");
        }
        private string UpdateDbUsererp(int userid)
        {
            var connstr = ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString;
            var ret = string.Empty;
            var sqlc = new SqlConnection(connstr);
            sqlc.Open();
            try
            {
                var select = string.Format("update erppm set userid ={0} ", userid);
                var sqlcommand = new SqlCommand(select, sqlc);
                var rowcount = sqlcommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("UpdateDbUsererp {0}  ", ex.Message));
                ret = ex.Message;
            }
            sqlc.Close();
            return ret;
        }
        [HttpPost]
        public async Task<ActionResult> PermissionSet(int groupid, int userid)
        {
            var res = await Task.Run(() => UpdateDbUserGroup(userid, groupid));

            if (!String.IsNullOrEmpty(res))
            {
                return Content(res);
            }
            return Content("success");
        }
        private string UpdateDbUserGroup(int userid, int groupid)
        {
            var connstr = ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString;
            var ret=string.Empty;
            var sqlc = new SqlConnection(connstr);
            sqlc.Open();
            try
            {
                var select = string.Format("update usergroup set userid ='{0}' where id = '{1}' ", userid, groupid);
                var sqlcommand = new SqlCommand(select, sqlc);
                var rowcount = sqlcommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("UpdateDbUserGroup {0}  ", ex.Message));
                ret= ex.Message;
            }
            sqlc.Close();
            return ret;
        }

        private List<Dbuser> GetUsersForUg(SqlConnection sqlc,  string code)
        {
            var ret = new List<Dbuser>();
            var users = _userService.GetAllUsers();
            if (code == "0001A210000000002ORS") //总公司,
            {
                var vicepresident = "副总经理";
                foreach (User user in users)
                {
                    if (user.Company.Code != "0001A210000000002OSD") continue;
                    if (user.PositionInfo.Name.Contains(vicepresident) || user.CscecRole.Name.Contains(vicepresident))
                    {
                        ret.Add(new Dbuser
                        {
                            Id=user.Id,
                       Name = user.UserName
                        });
                    }
                }
            }
            else
            {
                var viceexecutive = "副经理";
                foreach (User user in users)
                {
                    if (user.Company.Code != code) continue;
                    if (user.PositionInfo.Name==viceexecutive || user.CscecRole.Name==viceexecutive)
                    {
                        ret.Add(new Dbuser
                        {
                            Id = user.Id,
                            Name = user.UserName
                        });
                    }
                }
            }
            return ret;
        }

        private string GetUgName(SqlConnection sqlc, long p)
        {
              var ret=string.Empty;
            try
            {
                var select = string.Format("select name from groupcategory where id = '{0}' ", p);
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
                    Log.Info(select + "GetUgName there is no name for --" + p);

                }
            }
            catch (Exception ex)
            {
                Log.Info("GetUgName error:" + p + ex.Message);
            }
            return ret;
        }
        private string GetCompanyId(SqlConnection sqlc, string code)
        {
            var ret = string.Empty;
            try
            {
                var select = string.Format("select id from company where code = '{0}' ", code);
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
                    Log.Info(select + "GetCompanyId there is no id for --" + code);

                }
            }
            catch (Exception ex)
            {
                Log.Info("GetCompanyId error:" + code + ex.Message);
            }
            return ret;
        }
        public ActionResult GetErpPmData()
        {
            var connstr = ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString;
            var sqlc = new SqlConnection(connstr);
            sqlc.Open();
            var userid = 1;
            try
            {
                var select = string.Format("select UserId from erppm  ");
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
                            userid = int.Parse(row[column].ToString());
                            break;
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info("ErpPmSet error:" + connstr + ex.Message);
            }
            var departmentid = 46;
            try
            {
                var select = string.Format("select id from department where  code='1001A2100000000005ZD' ");
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
                            departmentid = int.Parse(row[column].ToString());
                            break;
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info("ErpPmSet select id from department error:" + connstr + ex.Message);
            }
            var epulist = new List<ErpPmUser>();
            try
            {
                var select = string.Format("select id,username,fullname from aecuser where departmentid={0}  ", departmentid);
                var sqlcommand = new SqlCommand(select, sqlc);
                var rds = new SqlDataAdapter(sqlcommand);
                var dt = new DataTable();
                rds.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var index = 0;
                        var epu = new ErpPmUser();
                        foreach (DataColumn column in dt.Columns)
                        {
                            switch (index)
                            {
                                case 0:
                                    epu.Id = long.Parse(row[column].ToString());
                                    break;
                                case 1:
                                    epu.UserName = row[column].ToString();
                                    break;
                                case 2:
                                    epu.Fullname = row[column].ToString();
                                    break;
                            }
                            index++;
                        }
                        if (userid == epu.Id) epu.Selected = true;
                        epulist.Add(epu);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info("ErpPmSet select users of 工程管理部 error:" + connstr + ex.Message);
            }
            sqlc.Close();

            var json = JsonConvert.SerializeObject(epulist,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.None
                });
            return Content(json);
        }
        public ActionResult ErpPmSet()
        {
            return View();
        }
        public ActionResult PermissionManagement()
        {
            var companies = _projService.GetAllCompany();
            var clist = new List<CompanyManager>();
            var connstr = ConfigurationManager.ConnectionStrings["AecCloudObjects"].ConnectionString;
             var sqlc = new SqlConnection(connstr);
                sqlc.Open();
            try
            {
                foreach (Company company in companies)
                {
                    var select = string.Format("select * from usergroup where companyid = '{0}' ", company.Id);
                    var sqlcommand = new SqlCommand(select, sqlc);
                    var rds = new SqlDataAdapter(sqlcommand);
                    var dt = new DataTable();
                    rds.Fill(dt);

                    var cm = new CompanyManager
                    {
                        Code = company.Code,
                        Id = company.Id,
                        Name = company.Name
                    };
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var index = 0;
                            var userg = new UserGroupDb();
                            foreach (DataColumn column in dt.Columns)
                            {
                                switch (index)
                                {
                                    case 0:
                                        userg.Id = int.Parse(row[column].ToString());
                                        break;
                                    case 1:
                                        userg.CompanyId = int.Parse(row[column].ToString());
                                        break;
                                    case 2:
                                        userg.UserId = int.Parse(row[column].ToString());
                                        break;
                                    case 3:
                                        userg.GroupId = int.Parse(row[column].ToString());
                                        break;
                                }
                                index++;
                            }
                            cm.UserGroups.Add(userg);
                        }
                    }
                    clist.Add(cm);
                }
            }
            catch (Exception ex)
            {
                Log.Info("PermissionManagement error:" +connstr+ ex.Message);
            }
            sqlc.Close();
            return View(clist);
        }
        public ActionResult UserInfoEdit()
        {
           SimpleLog(string.Format("in ActionResult UserInfoEdit(): _userManager={0}", _userManager));
            var res = GetUserProfile(_userManager);//UserClient.GetProfile(baseUri, token);//client.GetUserProfile(token);
            var info =res; //JsonConvert.DeserializeObject<UserDto>(json);
            var profile = info;
            var m = new UserInfoViewModel
            {
                Company = profile.Company,
                Department = profile.Department,
                Description = profile.Description,
                Industry = profile.Industry,
                Name = profile.FullName,
                Phone = profile.Phone,
                QQ = profile.QQ,
                Email = profile.Email,
                Post = profile.Post,
                WorkerIdentity=profile.UserName,
                Id = info.Id
            };
            return View(m);
        }
        private void SimpleLog(string logtext)
        {
            Log.Info("SimpleLog:"+logtext);
            //var tmpfile = System.IO.Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~"), "log", DateTime.Now.Date.ToString("yyyy-MM-dd") + "ManageControllerLog.xml");
            //try
            //{
            //    using (var sw = System.IO.File.AppendText(tmpfile))
            //    {
            //        sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
            //        sw.Close();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Error("SimpleLog ManageControllerLog-" + ex.Message, ex);
            //}
        }
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> UserInfoEdit(UserInfoViewModel model)
        {
           SimpleLog(string.Format("in ActionResult UserInfoEdit(UserInfoViewModel model): UserInfoViewModel={0}", model.Name));
            HttpPostedFileBase file = Request.Files["projectImg"];
            Stream fileStream = file.InputStream;
            int fileLength = file.ContentLength;
            //如果前端没有本地上传图片，则保留原图片
            if (fileLength == 0)
            {
                //var client = GetClient();
                var res1 = GetUserProfile(_userManager); //UserClient.GetProfile(baseUri, token);//client.GetUserProfile(token);
                //var json = res1.Content;
                var info = res1; //JsonConvert.DeserializeObject<UserDto>(json);
                var m = new UserProfileModel()
                {
                    Company = model.Company,
                    Department = model.Department,
                    Description = model.Description,
                    Industry = model.Industry,
                    Name = model.Name,
                    Phone = model.Phone,
                    QQ = model.QQ,
                    Email = model.Email,
                    Post = model.Post,
                    Image = info.Image
                };
                var res = await Api.AccountController.UpdateProfile(m, res1.Id, _userManager, _userService, _companyRepo, _departmentRepo);//UserClient.ChangeUserProfile(baseUri, token, m);//client.ChangeUserProfile(m, token);
                
                if (res != null)
                {
                    return RedirectToAction("UserInfo", "Manage");
                }
                else
                {
                    ModelState.AddModelError("", "出现错误");
                    return View(model);
                }
            }
            else
            {
                try
                {
                    byte[] img = new byte[fileLength];
                    if (img.Length > 204800)
                    {
                        ModelState.AddModelError("", "头像大小不能超过200K");
                        return View(model);
                    }
                    fileStream.Read(img, 0, fileLength);
                    fileStream.Close();
                    //var client = GetClient();
                    var m = new UserProfileModel()
                    {
                        Company = model.Company,
                        Department = model.Department,
                        Description = model.Description,
                        Industry = model.Industry,
                        Name = model.Name,
                        Phone = model.Phone,
                        QQ = model.QQ,
                        Email = model.Email,
                        Post = model.Post,
                        Image = img
                    };
                    var res = await Api.AccountController.UpdateProfile(m, User.Identity.GetUserId<long>(), _userManager, _userService, _companyRepo, _departmentRepo);//UserClient.ChangeUserProfile(baseUri, token, m);//client.ChangeUserProfile(m, token);
                    if (res != null)
                    {
                        return RedirectToAction("UserInfo", "Manage");
                    }
                    else
                    {
                        ModelState.AddModelError("", "出现错误");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("编辑用户信息失败："+ex.Message,ex);
                    ModelState.AddModelError("", ex);
                    throw;
                }
            }
            //return View(model);
        }
     
        public ActionResult UserInfo()
        {
            SimpleLog(string.Format("in ActionResult UserInfo(): _userManager={0}", _userManager));
            //  var res = GetUserProfile(_userManager);

            var res = _userManager.FindById(User.Identity.GetUserId<long>()).ToDto();
            //UserClient.GetProfile(baseUri, token);
            // client.GetUserProfile(token);
            var profile = res;
            var m = new UserInfoViewModel
            {
                Company = profile.Company,
                Department = profile.Department,
                Description = profile.Description,
                Industry = profile.Industry,
                Name = profile.FullName,
                Phone = profile.Phone,
                QQ = profile.QQ,
                Email = profile.Email,
                Post = profile.Post,
                Id = res.Id
            };
            return View(m);
        }
        [System.Web.Mvc.HttpPost]
        public ActionResult UserInfo(UserInfoViewModel model)
        {
            SimpleLog(string.Format("in ActionResult UserInfo(UserInfoViewModel model): UserInfoViewModel={0}", model.Name));
            return RedirectToAction("UserInfoEdit", "Manage");
        }
        public ActionResult UserImage()
        {
            var res = _userManager.FindById(User.Identity.GetUserId<long>()).ToDto();
            //UserClient.GetProfile(baseUri, token);
            // client.GetUserProfile(token);
            var profile = res;
            var m = new UserInfoViewModel
            {
                Company = profile.Company,
                Department = profile.Department,
                Description = profile.Description,
                Industry = profile.Industry,
                Name = profile.FullName,
                Phone = profile.Phone,
                QQ = profile.QQ,
                Email = profile.Email,
                Post = profile.Post,
                Id = res.Id
            };
            return View(m);
        }
        #region 裁剪图片
        /// <summary>  
        /// 裁剪图片  
        /// </summary>  
        /// <param name="sourceImg">原图片路径</param>  
        /// <param name="desImg">裁剪图片路径</param>  
        /// <param name="left">X</param>  
        /// <param name="top">Y</param>  
        /// <param name="width">宽</param>  
        /// <param name="height">高</param>  
        private static byte[] CutImage(Stream sourceImg, int left, int top, int width, int height)
        {
            byte[] bt = null;
            Image img = Bitmap.FromStream(sourceImg);

            Image imgToSave = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(imgToSave);

            RectangleF sourceRect = new RectangleF(left, top, width, height);
            RectangleF destinationRect = new RectangleF(0, 0, width, height);

            g.DrawImage(img,
                  destinationRect,
                  sourceRect,
                  GraphicsUnit.Pixel
                  );
            g.Save();


            MemoryStream mstream = new MemoryStream();
            imgToSave.Save(mstream, System.Drawing.Imaging.ImageFormat.Jpeg);
            bt = mstream.ToArray();
            //using (MemoryStream mostream = new MemoryStream())
            //{
            //    Bitmap bmp = new Bitmap(imgToSave);
            //    bmp.Save(mostream, System.Drawing.Imaging.ImageFormat.Jpeg);//将图像以指定的格式存入缓存内存流
            //    bt = new byte[mostream.Length];
            //    //mostream.Position = 0;//设置留的初始位置
            //    //mostream.Read(bt, 0, Convert.ToInt32(bt.Length));
            //}

            g.Dispose();
            imgToSave.Dispose();
            img.Dispose();

            return bt;
        }
        #endregion
        [ValidateInput(false)]
        [System.Web.Mvc.HttpPost]
        public ActionResult CustomUpLoadUserImage(string x, string y, string w, string h)
        {
            if (Request.Files.Count == 1 && Request.Files[0].FileName != string.Empty)
            {
                HttpPostedFileBase file = Request.Files[0];

                int left = (int)double.Parse(x);
                int top = (int)double.Parse(y);
                int with = (int)double.Parse(w);
                int hight = (int)double.Parse(h);

                byte[] bytes = CutImage(file.InputStream, left, top, with, hight);

                //byte[] bytes = new byte[file.InputStream.Length];

                //file.InputStream.Seek(0, SeekOrigin.Begin);

                //file.InputStream.Read(bytes, 0, bytes.Length);

             //   var result = this.User_Service.UpdateUserImage(this.CurrentUser.UserId, bytes);

                try
                {
                 //   var cfUserId = User.Identity.GetCommunityUserId();
                 //   Trace.Write("社区用户ID:" + cfUserId + "\t");
                    var fileName = file.FileName.Substring(0, file.FileName.LastIndexOf('.')) + ".jpg";
                 //   CfUserUtility.UpdateUserImage(cfUserId, fileName, bytes);

                }
                catch (Exception ex)
                {
                    Trace.TraceError("更新社区头像失败：{0}, StackTrace: {1}", ex.Message, ex.StackTrace);
                }

                return RedirectToAction("UserImage");
            }

            return RedirectToAction("UserImage");
        }
     
        public ViewResult Welcome()
        {
            string userName = User.Identity.Name;
            ViewBag.userName = userName;
            return View();
        }

        //消息列表
        public ActionResult UserNews()
        {
            return View();
        }

        private static string RemoveQuotes(string str)
        {
            return str.Replace("'", "\\'");
        }

       

        //[AllowAnonymous]
        [System.Web.Mvc.HttpPost]
        public ActionResult GetUserNews(int pageSize, int pageIndex)
        {
            var res = Api.ProjectMembersController.GetInvitations(User.Identity.GetUserId<long>(), _projMemberService,
                _projService); //await ProjectClient.GetInvitations(baseUri, token);//projectClient.GetInvitationsByInviter(token);
            //var respModel = res;
            var invitations = res; //JsonConvert.DeserializeObject<List<ProjectInvitationDto>>(respModel.Content);
            Log.Info("GetUserNews GetInvitations: " + invitations.Count);
            string tempStr = "[";
            for (int i = 0; i < invitations.Count; i++)
            {
                //找出已接受邀请邮件和还未接受验证的信息
                if (invitations[i].InviteeId != 0 && invitations[i].Accepted == false)
                {
                    var inviteeId = invitations[i].InviteeId;
                    var inviterRes = GetUserProfile(_userManager, inviteeId);//UserClient.GetProfile(baseUri, token, inviteeId);//client.GetUserProfile(inviteeId, token);
                    //var json = inviterRes.Content;
                    var inviterProfile = inviterRes; //JsonConvert.DeserializeObject<UserDto>(json);
                    var projectId = invitations[i].ProjectId;
                    var projRes = Api.ProjectController.GetProject(projectId, _projService, _mfvaultService,
                        _vaultTempService);//await ProjectClient.GetProject(baseUri, token, projectId);//projectClient.GetProject(projectId, token).Result;
                    //var projectJson = projRes.Content;
                    var project = projRes;//JsonConvert.DeserializeObject<ProjectDto>(projectJson);
                    tempStr += "{'id':'" + invitations[i].Id + "',";
                    tempStr += "'ProjectId':'" + invitations[i].ProjectId + "',";
                    tempStr += "'ProjectName':'" + project.Name + "',";
                    tempStr += "'InviteeEmail':'" + invitations[i].InviteeEmail + "',";
                    tempStr += "'InviteeId':'" + invitations[i].InviteeId + "',";
                    tempStr += "'PartyId':'" + invitations[i].InviteePartId + "',";
                    tempStr += "'InviteeName':'" + inviterProfile.UserName + "',";
                    tempStr += "'InvitationMessage':'" + RemoveQuotes(invitations[i].InvitationMessage) + "',";
                    tempStr += "'InviteeConfirmMessage':'" + RemoveQuotes(invitations[i].InviteeConfirmMessage) + "'},";
                }
            }
            tempStr = tempStr.Substring(0, tempStr.Length - 1);
            tempStr += "]";
            //List<ProjectInvitationDto> pi = invitations.FindAll(c => c.InviteeId != 0 && c.Accepted == false);
            //var returnStr = JsonConvert.SerializeObject(pi);
            return Content(tempStr);
        }

        //个人中心
        public ActionResult Personal()
        {
            var authRes = GetUserProfile(_userManager);//UserClient.GetProfile(baseUri, token);
            //var authJson = authRes.Content;
            var profile = authRes; //JsonConvert.DeserializeObject<UserDto>(authJson);
            ViewBag.userId = profile.Id;
            var res = Api.ProjectController.GetProjectsByUser(User.Identity.GetUserId<long>(), _projMemberService,
                _projService, _mfvaultService, _vaultTempService);//await ProjectClient.GetProjects(baseUri, token);

            //var json = res.Content;
            var projs = res; //JsonConvert.DeserializeObject<List<ProjectDto>>(json);
            var bim = new BIMModel()
            {
                ProjectDto = projs
            };
            return View(bim);
        }
    }
}