using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Common;
using MFilesAPI;
using Newtonsoft.Json;

namespace VaultApp
{
    class WorkFlowNew
    {
        public class WfnState
        {
            public WfnState()
            {
                MFBuiltInPropertyDefWorkflowAssignment = string.Empty;
                MFBuiltInPropertyDefAssignmentDescription = string.Empty;
                MFBuiltInPropertyDefSignatureManifestation = string.Empty;
                Time = string.Empty;
                User = string.Empty;
                State = string.Empty;
            }
            public string State { set; get; }
            public string User { set; get; }
            public string Time { set; get; }
            public string MFBuiltInPropertyDefSignatureManifestation { set; get; }
        //    public string MFBuiltInPropertyDefStateTransition { set; get; }
            public string MFBuiltInPropertyDefWorkflowAssignment { set; get; }
            public string MFBuiltInPropertyDefAssignmentDescription { set; get; } 
        }

        public static string GetWfnStates(EventHandlerEnvironment env)
        {
            var ret = new List<WfnState>();
            try
            {
                var vault = env.Vault;
                var input = env.Input;

                Writelog(string.Format("vault={0},input={1}", vault.Name, input));
                var pos = input.IndexOf('-');
                var type = int.Parse(input.Substring(0, pos));
                var id = int.Parse(input.Substring(pos+1, input.Length - pos-1));
                var objid = new ObjID();
                objid.SetIDs(type, id);
                var history = vault.ObjectOperations.GetHistory(objid);
                Writelog(string.Format("type={0},id={1}", type, id));
                foreach (ObjectVersion objectVersion in history)
                {
                    Writelog(string.Format("Version={0}", objectVersion.ObjVer.Version));
                    var pvs = vault.ObjectOperations.GetObjectVersionAndProperties(objectVersion.ObjVer).Properties;
                    var state = GetPropertyValue(pvs, (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefState, vault);
                    var lastmodified = GetPropertyValue(pvs, (int) MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified,vault);
                    var lastmodifiedby = GetPropertyValue(pvs,(int) MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModifiedBy, vault);
                    //var stateentered = GetPropertyValue(pvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefStateEntered, vault);
                    //var statechanged = GetPropertyValue(pvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefStatusChanged, vault);
                    var taskdescription = GetPropertyValue(pvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription, vault);
                    //var deadline = GetPropertyValue(pvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefDeadline, vault);
                    //var monitor = GetPropertyValue(pvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefMonitoredBy, vault);
                    var assignto = GetPropertyValue(pvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo, vault);
                    var task = GetPropertyValue(pvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefWorkflowAssignment, vault);
                    var signature = GetPropertyValue(pvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSignatureManifestation, vault);
            //        var statetransition = GetPropertyValue(pvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefStateTransition, vault);
                    var wfnstate = new WfnState();
                    var creator = GetPropertyValue(pvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy, vault).GetValueAsLocalizedText();
                    var modifiedby = lastmodifiedby.GetValueAsLocalizedText();
               //     Writelog(string.Format("111 Version={0}", objectVersion.ObjVer.Version));
                    wfnstate.User = modifiedby.Contains("DBWorld") ? creator : modifiedby;
                    wfnstate.Time = lastmodified.GetValueAsLocalizedText();
                    wfnstate.State = state.GetValueAsLocalizedText();
                //    wfnstate.MFBuiltInPropertyDefStateTransition = statetransition.GetValueAsLocalizedText();
                    var tmpstr = "-> \"" + wfnstate.State;
                    var signtext=signature.GetValueAsLocalizedText();
                    wfnstate.MFBuiltInPropertyDefSignatureManifestation = signtext.Contains(tmpstr) ? signtext : "无";
                  //  wfnstate.MFBuiltInPropertyDefSignatureManifestation =  signtext;
              //      Writelog(string.Format("{0},{1}",tmpstr,signtext));
                    var text=task.GetValueAsLocalizedText();
                //    Writelog(string.Format("222 Version={0}", objectVersion.ObjVer.Version));
                    if (text != string.Empty)
                    {
                        var tasks = task.Value.GetValueAsLookups();
                   //     Writelog(string.Format("444 Version={0}", objectVersion.ObjVer.Version));
                        foreach (Lookup lookup in tasks)
                        {
                          //  Writelog(string.Format("666 Version={0}", objectVersion.ObjVer.Version));
                            var taskobjid = new ObjID();
                            taskobjid.SetIDs((int)MFBuiltInObjectType.MFBuiltInObjectTypeAssignment, lookup.Item);
                            var taskobjpvs = vault.ObjectOperations.GetLatestObjectVersionAndProperties(taskobjid, true).Properties;
                            var assignee = GetPropertyValue(taskobjpvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo, vault).GetValueAsLocalizedText();
                            var tasktext = GetPropertyValue(taskobjpvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription, vault).GetValueAsLocalizedText();
                            var taskname = GetPropertyValue(taskobjpvs, (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle, vault).GetValueAsLocalizedText();
                            wfnstate.MFBuiltInPropertyDefWorkflowAssignment += assignee != string.Empty ? "分配给：" + assignee + "," + taskname + "," + tasktext : taskname + "," + tasktext;
                        }
                    }
                    else
                    {
                      //  Writelog(string.Format("555 Version={0}", objectVersion.ObjVer.Version));
                        wfnstate.MFBuiltInPropertyDefWorkflowAssignment = text;
                    }
                  //  Writelog(string.Format("333 Version={0}", objectVersion.ObjVer.Version));
                    var temp = assignto.GetValueAsLocalizedText();
                    var taskcontext= taskdescription.GetValueAsLocalizedText();
                    wfnstate.MFBuiltInPropertyDefAssignmentDescription = temp != string.Empty?"分配给："+temp+","+taskcontext:taskcontext;
                    ret.Add(wfnstate);
                }
            }
            catch (Exception ex)
            {
                Writelog("GetWfnStates error:" + ex.Message);
            }
            var retjson = JsonConvert.SerializeObject(ret, Formatting.None);
        //    Writelog("GetWfnStates-"+retjson);
            return retjson;
        }

        public static PropertyValue GetPropertyValue(PropertyValues pvs, int propertydef, Vault vault)
        {
            var pv = new PropertyValue();
            try
            {
               return pvs.SearchForProperty(propertydef);
            }
            catch (Exception ex)
            {
             //   Writelog(string.Format("GetPropertyValue {0} error: {1}",propertydef, ex.Message));
            }
            return pv;
        }
        public static void Writelog(string logtext)
        {
            try
            {
                using (
                    var sw =
                        System.IO.File.AppendText(System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                            DateTime.Now.ToString("yyyy-MM-dd")+"MaintenanceReport.txt")))
                {
                    sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
