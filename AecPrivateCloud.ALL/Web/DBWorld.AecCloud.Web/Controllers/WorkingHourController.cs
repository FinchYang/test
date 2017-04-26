using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;
using AecCloud.Core.Domain.WorkingHour;
using AecCloud.Service.Projects;
using AecCloud.Service.Vaults;
using DBWorld.AecCloud.Web.Models;
using AecCloud.MfilesServices;
using Newtonsoft.Json;

namespace DBWorld.AecCloud.Web.Controllers
{
    [Authorize]
    public class WorkingHourController: Controller
    {
        private readonly IProjectService _projService;
        private readonly IUserVaultService _userVaultService;

        private readonly IVaultServerService _vaultServer;
        private readonly IMFWorkHourService _workHourService;
        public WorkingHourController(IVaultServerService vaultServer, IMFWorkHourService workHourService
            , IProjectService projService, IUserVaultService userVaultService)

        {
            _projService = projService;
            _userVaultService = userVaultService;
            _vaultServer = vaultServer;
            _workHourService = workHourService;
        }

        // GET: WorkingHour
        public ActionResult Index()
        {           
            //var vaultGuids = GetCurrentVaultsEx();
            //var status = GetProjStatus();
            return View();
        }
        //GET: WorkingHour/ReportData
        [ActionName("ReportData")]
        public async Task<ActionResult> ReportDataAsync(string vaultGuids, string showType, string beginDate, string endDate)
        {
            return await Task.Run(() =>
            {
                //vaultGuids:"[\"{285A5F72-4BCB-470B-938D-11C2F74F6E71}\",\"{111111111111111111112}\"]"
                vaultGuids = vaultGuids.TrimStart('[').TrimEnd(']').Replace("\"", "");
                var guidArr = vaultGuids.Split(new[] {',', ';', '，'});
                var server = _vaultServer.GetServer();
                var vaultKeys = GetCurrentVaultsEx();
                var vkeys = (from v in vaultKeys
                    let t = guidArr.FirstOrDefault(g => g == v.Guid)
                    where !string.IsNullOrEmpty(t)
                    select v).ToList();

                int sType = int.Parse(showType);
                DateTime bDate = DateTime.Parse(beginDate);
                DateTime eDate = DateTime.Parse(endDate);
                var data = GetTableData(vkeys, server, bDate, eDate, sType);

                return Json(data, JsonRequestBehavior.AllowGet);
            });

        }
        //GET: WorkingHour/ProjStatus
        public ActionResult ProjStatus()
        {
            var status = GetProjStatus();
            return Json(status, JsonRequestBehavior.AllowGet);
        }
        //GET: WorkingHour/Projects
        public ActionResult Projects()
        {
            //var projs = GetCurrentVaultsEx();
            var projs = GetCurrentVaults();
            return Json(projs, JsonRequestBehavior.AllowGet);
        }

        //项目状态
        private IEnumerable<ProjectStatus> GetProjStatus()
        {
            return _projService.GetStatuses();
        }
        private IEnumerable<VaultKey> GetCurrentVaults(long? statusId=null)
        {
            var userId = AuthUtility.GetUserId(User);
            var uvs = _userVaultService.GetVaults(userId);
            return (from v in uvs
                let proj = _projService.GetProjectByVault(v.Id)
                where statusId == null || proj.StatusId == statusId.Value
                select new VaultKey
                       {
                           Guid = v.Guid,
                           Name = proj.Name
                       }).ToList();
        }
        private IEnumerable<VaultKey> GetCurrentVaultsEx()
        {
            var userId = AuthUtility.GetUserId(User);
            var userName = AuthUtility.GetUserName(User);
            var password = DBWorldCache.Get(userId.ToString());
            var isAdUser = !String.IsNullOrEmpty(AuthUtility.GetUserDomain(User.Identity));
            var server = _vaultServer.GetServer();
            var vaultGuids = _workHourService.GetCurrentVaults(userName, password, isAdUser, server);
            return vaultGuids;
        }

        internal async Task<IEnumerable<ProjectHour>> GetTableDataAsync(IEnumerable<VaultKey> vaultGuids,
            VaultServer server,
            DateTime? beginDate = null, DateTime? deadline = null, int showingType = 1)
        {
            return await Task.Run(() => GetTableData(vaultGuids, server, beginDate, deadline, showingType));
        }

        private IEnumerable<ProjectHour> GetTableData(IEnumerable<VaultKey> vaultGuids, VaultServer server, 
            DateTime? beginDate = null, DateTime? deadline = null, int showingType =1)
        {
            
            var vaultInfo = _workHourService.GetHourInfo(vaultGuids, server, beginDate, deadline);
            var projHourList = new List<ProjectHour>();
            foreach (ProjectHourInfo pInfo in vaultInfo)
            {
                var dbTotalBudget = pInfo.TotalBudget;
                var dbPersonalBudgets = pInfo.PersonalBudgets;
                var dbHourLogs = pInfo.HourLogs;

                if (beginDate == null) beginDate = dbTotalBudget.BeginDate;
                if (deadline == null) deadline = dbTotalBudget.Deadline;
                var projHour = new ProjectHour()
                {
                    ProjName = dbTotalBudget.ProjectName
                    //,BudgetHours = dbTotalBudget.TotalHours
                    ,TimeSpans = new List<string>()
                };
                double tHoursBuget = 0.0;
                double tHoursActual = 0.0;
                var userList = new List<UserHour>();
                var timeSpans = DateTimeTool.GetTimespanList(beginDate.GetValueOrDefault(),
                                   deadline.GetValueOrDefault(), showingType).ToList();
                foreach (TimeArea span in timeSpans)
                {
                    projHour.TimeSpans.Add(span.Title);
                }

                foreach (PersonalBudget b in dbPersonalBudgets)
                {
                    var userHour = new UserHour
                    {
                        UserName = b.MemberName
                    };
                    var uLogs = GetUserLogs(b.VaultGuid, b.UserID, dbHourLogs);

                    var bHours = new List<UnitHour>();
                    var aHours = new List<UnitHour>();
                    var mBudgets = Convert2MonthBudgets(b.HoursDetail);
                    foreach (TimeArea t in timeSpans)
                    {
                        var bUnit = GetBudgetUnit(t, mBudgets);
                        var aUnit = GetActualUnit(t, uLogs);
                        bHours.Add(bUnit);
                        aHours.Add(aUnit);
                    }
                    userHour.BudgetHours = bHours;
                    userHour.ActualHours = aHours;

                    tHoursBuget += userHour.BudgetTotal;
                    tHoursActual += userHour.ActualTotal;

                    userList.Add(userHour);
                }
                projHour.UserList = userList;
                projHour.BudgetHours = tHoursBuget;
                projHour.ActualHours = tHoursActual;
                projHourList.Add(projHour);
            }
            return projHourList;
        }
        private UnitHour GetBudgetUnit(TimeArea t, IEnumerable<MonthBudget> budgets)
        {
            var res = new UnitHour
            {
                Title = t.Title,
                Hours = 0.0
            };
            foreach (MonthBudget b in budgets)
            {
                if (t.BeginDate <= b.Month && t.EndDate >= b.Month)
                {
                    res.Hours += b.Hours;
                }
            }
            return res;
        }
        private UnitHour GetActualUnit(TimeArea t, IEnumerable<HourLog> logs)
        {
            var res = new UnitHour
            {
                Title = t.Title,
                Hours = 0.0
            };
            foreach (HourLog l in logs)
            {
                if (t.BeginDate <= l.LogDate && t.EndDate >= l.LogDate)
                {
                    res.Hours += l.LogHours;
                }
            }
            return res;
        }
        private IList<MonthBudget> Convert2MonthBudgets(string hoursDetail)
        {
            var mbudgets = new List<MonthBudget>();
            if (string.IsNullOrEmpty(hoursDetail)) return mbudgets;
            var dic = JsonConvert.DeserializeObject<Dictionary<string,string>>(hoursDetail);//Json字符串默认解析为字典
            foreach (KeyValuePair<string, string> kv in dic)
            {
                var mb = new MonthBudget();
                DateTime date;
                var ok = DateTime.TryParse(kv.Key + "-1",out date);
                if(!ok) continue;
                double hours;
                double.TryParse(kv.Value, out hours);

                mb.Month = date;
                mb.Hours = hours;
                mbudgets.Add(mb);
            }
            return mbudgets;
        } 
        internal IList<HourLog> GetUserLogs(string vaultGuid, int userId, IEnumerable<HourLog> allLogs)
        {
            //var ls = (from log in allLogs
            //    where log.UserID == userId && log.VaultGuid == vaultGuid
            //    select log).ToList();
            //return ls;
            return allLogs.Where(l => l.VaultGuid == vaultGuid && l.UserID == userId).ToList();
        }
        
    }
}