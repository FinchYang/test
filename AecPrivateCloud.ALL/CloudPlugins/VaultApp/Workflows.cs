using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Common;
using Newtonsoft.Json;
using MFilesAPI;

namespace VaultApp
{
    public class MfWorkflow
    {
        public MfWorkflow()
        {
            States = new List<MfState>();
            Transitions = new List<MfStateTransition>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public List<MfState> States { get; set; }

        public List<MfStateTransition> Transitions { get; set; }

        public string ToString(bool format)
        {
            if (!format)
            {
                return JsonConvert.SerializeObject(this, Formatting.None);
            }
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static string GetWorkflow(EventHandlerEnvironment env)
        {
            var input = env.Input;
            int id;
            var ok = int.TryParse(input, out id);
            if (!ok) return String.Empty;

            try
            {
                var vault = env.Vault;
                var wf = GetWorkflow(vault, id);
                return JsonConvert.SerializeObject(wf, Formatting.None);
            }
            catch (Exception ex)
            {
                Writelog(string.Format("static string GetWorkflow(EventHandlerEnvironment env) error : {0}", ex.Message));
                return String.Empty;
            }
        }
        public static void Writelog(string logtext)
        {
            try
            {
                using (
                    var sw =
                        System.IO.File.AppendText(System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                            "MaintenanceReport.txt")))
                {
                    sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
                    sw.Close();
                }

            }
            catch (Exception) { }
        }
        public static MfWorkflow GetWorkflow(Vault vault, int workflowId)
        {
            var workflow = vault.WorkflowOperations.GetWorkflowAdmin(workflowId);
            var wf = new MfWorkflow { Id = workflowId, Name = workflow.Workflow.Name };
            try
            {
                foreach (StateAdmin s in workflow.States)
                {
                    var state = new MfState { Id = s.ID, Name = s.Name };
                    var at = new List<AssignTo>();
                    var ps = new List<AssignedProperty>();
                    GetAssignTo(vault, s, at, ps);
                    state.To.AddRange(at);
                    state.AssignProps.AddRange(ps);
                    wf.States.Add(state);
                }
                foreach (StateTransition st in workflow.StateTransitions)
                {
                    wf.Transitions.Add(new MfStateTransition { From = st.FromState, To = st.ToState });
                }
            }
            catch (Exception ex)
            {
                Writelog(string.Format("GetWorkflow error : {0}", ex.Message));
            }
            return wf;
        }

        private static void SetAssignTo(Vault vault, UserOrUserGroupIDExs ug, List<AssignTo> assignList, List<AssignedProperty> assignProps)
        {
            foreach (UserOrUserGroupIDEx u in ug)
            {
                var id = u.UserOrGroupID;
                var type = u.UserOrGroupType;
                if (type == MFUserOrUserGroupType.MFUserOrUserGroupTypeUserAccount)
                {
                    var account = vault.UserOperations.GetLoginAccountOfUser(id);
                    var fullName = !String.IsNullOrEmpty(account.FullName) ? account.FullName : account.AccountName;
                    assignList.Add(new AssignTo { UserId = id, Fullname = fullName });
                }
                else if (type == MFUserOrUserGroupType.MFUserOrUserGroupTypeUserGroup)
                {
                    var userList = GetUsersFromGroup(vault, id);
                    foreach (var uId in userList)
                    {
                        var account = vault.UserOperations.GetLoginAccountOfUser(uId);
                        var fullName = !String.IsNullOrEmpty(account.FullName) ? account.FullName : account.AccountName;
                        assignList.Add(new AssignTo { UserId = uId, Fullname = fullName });
                    }
                }
                else
                {
                    if (u.IndirectProperty != null && u.IndirectProperty.Count > 0)
                    {
                        var ap = new AssignedProperty();
                        foreach (IndirectPropertyIDLevel pId in u.IndirectProperty)
                        {
                            var al = new PropertyLevel { Level = (int)pId.LevelType, Id = pId.ID };
                            ap.Levels.Add(al);
                        }
                        assignProps.Add(ap);
                    }
                }
            }
        }

        private static List<int> GetUsersFromGroup(Vault ovault, int ugid)
        {
            var ret = new List<int>();
            var uga = ovault.UserGroupOperations.GetUserGroupAdmin(ugid);
            foreach (int id in uga.UserGroup.Members)
            {
                if (id < 0)
                {
                    ret.AddRange(GetUsersFromGroup(ovault, -id));
                }
                else
                {
                    ret.Add(id);
                }
            }
            return ret;
        }

        private static void GetAssignTo(Vault vault, StateAdmin state, List<AssignTo> at, List<AssignedProperty> ps)
        {
            if (state.ActionAssignToUser)
            {
                SetAssignTo(vault, state.ActionAssignToUserDefinition.AssignedTo, at, ps);
            }
            if (state.ActionCreateSeparateAssignment)
            {
                var tasks =
                    state.ActionDefinitions.Cast<ActionDefinition>().ToArray();
                foreach (ActionDefinition ad in tasks)
                {
                    var ac = ad.ActionCreateSeparateAssignment;
                    var ug = ac.AssignedTo;
                    SetAssignTo(vault, ug, at, ps);
                }

            }
        }
    }
    /// <summary>
    /// 工作流状态定义
    /// </summary>
    public class MfState
    {
        public int Id { get; set; }

        public string Name { get; set; }

        private readonly List<AssignTo> _to = new List<AssignTo>();

        public List<AssignTo> To
        {
            get { return _to; }
        }

        private readonly List<AssignedProperty> _ps = new List<AssignedProperty>();

        public List<AssignedProperty> AssignProps
        {
            get { return _ps; }
        }
    }
    /// <summary>
    /// 指派给用户
    /// </summary>
    public class AssignTo
    {
        public int UserId { get; set; }

        public string Fullname { get; set; }
    }
    /// <summary>
    /// 通过元数据指派给的属性ID层级关系
    /// </summary>
    public class PropertyLevel
    {
        public int Level { get; set; }

        public int Id { get; set; }
    }
    /// <summary>
    /// 通过元数据的指派给
    /// </summary>
    public class AssignedProperty
    {
        private readonly List<PropertyLevel> _levels = new List<PropertyLevel>();

        public List<PropertyLevel> Levels
        {
            get { return _levels; }
        }
    }
    /// <summary>
    /// 状态转换
    /// </summary>
    public class MfStateTransition
    {
        public int From { get; set; }

        public int To { get; set; }
    }
}
