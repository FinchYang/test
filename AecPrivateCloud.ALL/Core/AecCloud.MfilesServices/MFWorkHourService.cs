using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;
using AecCloud.Core.Domain.WorkingHour;
using AecCloud.MFilesCore;
using MFilesAPI;

namespace AecCloud.MfilesServices
{
    public interface IMFWorkHourService
    {
        IEnumerable<VaultKey> GetCurrentVaults(string userName, string password, bool isAdUser, VaultServer server);
        IEnumerable<ProjectHourInfo> GetHourInfo(IEnumerable<VaultKey> vaultKeys, VaultServer server, DateTime? beginDate = null, DateTime? deadline = null);
    }
    public class MFWorkHourService: IMFWorkHourService
    {
        public IEnumerable<ProjectHourInfo> GetHourInfo(IEnumerable<VaultKey> vaultKeys, VaultServer server, DateTime? beginDate = null, DateTime? deadline = null)
        {
            var serverApp = MFServerUtility.ConnectToServer(server.AdminName, server.AdminPwd, server.LocalIp,
                                                        server.ServerPort, false);
            var res = new List<ProjectHourInfo>();
            foreach (VaultKey v in vaultKeys)
            {
                Vault vault = null;
                try
                {
                    vault = serverApp.LogInToVault(v.Guid);
                }
                catch{}
                if(!IsValid(vault)) continue;
                var tBudget = GetTotalBudget(vault);
                if (beginDate == null)
                {
                    beginDate = tBudget.BeginDate;
                }
                if (deadline == null)
                {
                    deadline = tBudget.Deadline;
                }
                var hLogs = GetHourLogs(vault, tBudget.BeginDate, tBudget.Deadline);
                var pBudget = GetPersonalBudgets(vault);
                res.Add(new ProjectHourInfo
                        {
                            TotalBudget = tBudget,
                            HourLogs = hLogs,
                            PersonalBudgets = pBudget
                        });
            }
            serverApp.Disconnect();
            return res;
        }
        public IEnumerable<VaultKey> GetCurrentVaults(string userName, string password, bool isAdUser, VaultServer server)
        {
            var vaults = GetUserVaults(userName, password, isAdUser, server);
            LogOut();
            return vaults;

        }

        private IEnumerable<VaultKey> GetUserVaults(string userName, string password, bool isAdUser, VaultServer server)
        {                  
            var vaults = new List<VaultKey>();
            try
            {
                var serverApp = MFServerUtility.ConnectToServer(userName, password, server.LocalIp,
                                                        server.ServerPort,isAdUser);
                var keys = serverApp.GetOnlineVaults();
                vaults.AddRange(from VaultOnServer key in keys 
                                select new VaultKey { Name = key.Name, Guid = key.GUID });
                _serverApp = serverApp;
            }
            catch (Exception)
            {
            }
            return vaults;
        }

        
        private MFilesServerApplication _serverApp = null;
        private void LogOut()
        {
            if (_serverApp != null)
            {
                try
                {
                    _serverApp.Disconnect();
                }
                catch
                {
                }
            }
        }

        private bool IsValid(Vault vault)
        {
            if (vault == null) return false;
            var typeId = MfAlias.GetObjType(vault, "ObjBudgetHours", false);
            return typeId != -1;
        }
        internal TotalBudget GetTotalBudget(Vault vault)
        {            
            var projDate = GetProjectDate(vault);            
            var budget = new TotalBudget
                         {
                             BeginDate = String2Date(projDate.BeginDate),
                             Deadline = String2Date(projDate.Deadline),
                             ProjectName = projDate.ProjName,
                             VaultGuid = vault.GetGUID(),
                             TotalHours = GetBudgetTotalHours(vault)
                         };
            return budget;

        }
        //获取项目起止日期
        private ProjectDate GetProjectDate(Vault vault)
        {
            var typeId = MfAlias.GetObjType(vault, "ObjProject");
            var objVns = MFSearchConditionUtils.SearchObjectsByType(vault, typeId);
            if (objVns.Count == 0)
            {
                throw new NoNullAllowedException(vault.Name+"无对象项目");
            }
            var projDate = new ProjectDate
                           {
                               ProjName = objVns[1].Title
                           };
            var propIdBeginDate = MfAlias.GetPropDef(vault, "PropStartDate");
            var propIdEndDate = MfAlias.GetPropDef(vault, "42");
            var tValue1 = vault.ObjectPropertyOperations.GetProperty(objVns[1].ObjVer, propIdBeginDate).Value;
            projDate.BeginDate = tValue1.DisplayValue;
            var tValue2 = vault.ObjectPropertyOperations.GetProperty(objVns[1].ObjVer, propIdEndDate).Value;
            projDate.Deadline = tValue2.DisplayValue;

            return projDate;
        }
        //获取总工时
        private double GetBudgetTotalHours(Vault vault)
        {
            var typeId = MfAlias.GetObjType(vault, "ObjBudgetHours");
            var classId = MfAlias.GetObjectClass(vault, "ClassTotalBudgetHours");
            var objVns = MFSearchConditionUtils.SearchObjectsByClass(vault, typeId, classId);
            if (objVns.Count > 0)
            {
                var propIdTotalHours = MfAlias.GetPropDef(vault, "PropTotalBudgetHours");
                return vault.ObjectPropertyOperations.GetProperty(objVns[1].ObjVer, propIdTotalHours).Value.Value;
            }
            return 0.0;
        }

        internal IList<PersonalBudget> GetPersonalBudgets(Vault vault)
        {
            var members = GetProjectMembers(vault);
            var budgets = GetPersonalBudgetHourList(vault);
            var res = new List<PersonalBudget>();
            foreach (Member m in members)
            {
                var item = budgets.FirstOrDefault(c => c.MemberID == m.ID);
                if (item != null)
                {
                    item.UserID = m.UserId;
                    res.Add(item);
                }
                else
                {
                    item = new PersonalBudget
                           {
                               MemberID = m.ID,
                               MemberName = m.Name,
                               UserID = m.UserId,
                               VaultGuid = vault.GetGUID(),
                               HoursDetail = ""
                           };
                    res.Add(item);
                }
            }
            return res;
        }
        //获取项目成员
        private IEnumerable<Member> GetProjectMembers(Vault vault)
        {
            var typeId = MfAlias.GetObjType(vault, "ObjContacts");
            var objVns = MFSearchConditionUtils.SearchObjectsByType(vault, typeId);

            var propIdAccount = MfAlias.GetPropDef(vault, "PropAccount");
            return (from ObjectVersion o in objVns
                let userId = vault.ObjectPropertyOperations.GetProperty(o.ObjVer, propIdAccount).Value.GetLookupID()
                select new Member
                       {
                           ID = o.ObjVer.ID,
                           Name = o.Title, 
                           UserId = userId
                       }).ToList();
        }
        private IList<PersonalBudget> GetPersonalBudgetHourList(Vault vault)
        {
            var typeId = MfAlias.GetObjType(vault, "ObjBudgetHours");
            var classId = MfAlias.GetObjectClass(vault, "ClassBudgetHours");
            var objVns = MFSearchConditionUtils.SearchObjectsByClass(vault, typeId, classId);
            var budgets = new List<PersonalBudget>();
            if (objVns.Count > 0)
            {
                var guid = vault.GetGUID();
                var propIdHoursDetail = MfAlias.GetPropDef(vault, "PropBudgetHoursDetail");
                var propIdMember = MfAlias.GetPropDef(vault, "PropMember");
                for (int i = 1; i <= objVns.Count; i++)
                {
                    var props = vault.ObjectPropertyOperations.GetProperties(objVns[i].ObjVer, false);
                    var member = props.SearchForProperty(propIdMember).Value.GetValueAsLookup();
                    var hoursDetail = props.SearchForProperty(propIdHoursDetail).Value.DisplayValue;
                    budgets.Add(new PersonalBudget
                                {
                                    MemberID = member.Item,
                                    MemberName = member.DisplayValue,
                                    HoursDetail = hoursDetail,
                                    VaultGuid = guid
                                });
                }
            }
            return budgets;
        }

        //获取工时日志
        internal IList<HourLog> GetHourLogs(Vault vault, DateTime? startDate, DateTime? endDate)
        {
            var typeId = MfAlias.GetObjType(vault, "ObjJobLog");
            var propIdLogDate = MfAlias.GetPropDef(vault, "PropJobDate");
            var scs = new SearchConditions();
            if (startDate != null)
            {
                var sdCondition = new SearchCondition();
                sdCondition.ConditionType = MFConditionType.MFConditionTypeGreaterThanOrEqual;
                sdCondition.Expression.DataPropertyValuePropertyDef = propIdLogDate;
                sdCondition.TypedValue.SetValue(MFDataType.MFDatatypeDate, startDate.Value);
                scs.Add(-1, sdCondition);
            }
            if (endDate != null)
            {
                var edCondition = new SearchCondition();
                edCondition.ConditionType = MFConditionType.MFConditionTypeLessThanOrEqual;
                edCondition.Expression.DataPropertyValuePropertyDef = propIdLogDate;
                edCondition.TypedValue.SetValue(MFDataType.MFDatatypeDate, endDate.Value);
                scs.Add(-1, edCondition);
            }
            var objVns = MFSearchConditionUtils.SearchObjects(vault, typeId, null, scs);
            var res = new List<HourLog>();
            var guid = vault.GetGUID();
            foreach (ObjectVersion o in objVns)
            {
                int userId = vault.ObjectPropertyOperations.GetProperty(o.ObjVer, 25).Value.GetLookupID();
                var titleArr = o.Title.Split(new []{'_'});
                DateTime logDate = String2Date(titleArr[1]).GetValueOrDefault();
                var log = new HourLog
                          {
                              MemberName = titleArr[0],
                              LogDate = logDate,
                              LogHours = double.Parse(titleArr[2]),
                              VaultGuid = guid,
                              UserID = userId
                          };
                res.Add(log);
            }
            return res;
        }

        private DateTime? String2Date(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
            {
                return null;
            }
            var dtFormat = new DateTimeFormatInfo();
            dtFormat.ShortDatePattern = "yyyy/MM/dd";
            //Convert.ToDateTime(dateStr);
            return DateTime.Parse(dateStr, dtFormat);         
        }
    }
    public class VaultKey
    {
        public string Guid { get; set; }
        public string Name { get; set; }
    }

    public class ProjectDate
    {
        public string ProjName { get; set; }
        /// <summary>
        /// 项目开始日期字符串
        /// </summary>
        public string BeginDate { get; set; }
        /// <summary>
        /// 项目截止日期
        /// </summary>
        public string Deadline { get; set; }
    }

    public struct Member
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
    }

    public class ProjectHourInfo
    {
        public TotalBudget TotalBudget { get; set; }
        public IList<PersonalBudget> PersonalBudgets { get; set; }
        public IList<HourLog> HourLogs { get; set; }
    }
}
