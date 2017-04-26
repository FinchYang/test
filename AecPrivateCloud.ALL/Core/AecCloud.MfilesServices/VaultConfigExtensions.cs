using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Projects;
using AecCloud.MFilesCore;
using AecCloud.MFilesCore.Metadata;
using MFilesAPI;

namespace AecCloud.MfilesServices
{
    public static class VaultConfigExtensions
    {
        public static MFObject GetMfProjectObject(this VaultConfig config, Project proj)
        {
            var projObjDef = config.Project;
            var obj = new MFObject(projObjDef);

            obj.Properties.Add("PropProjName", proj.Name);
            obj.Properties.Add("PropProjNum", proj.Number);
            obj.Properties.Add("PropDescription", proj.Description);
            obj.Properties.Add("PropDeadLine", proj.EndDateUtc.ToLocalTime());
            obj.Properties.Add("PropStartDate", proj.StartDateUtc.ToLocalTime());
            obj.Properties.Add("PropProjPhase", proj.StatusId);
            obj.Properties.Add("PropProprietorUnit", proj.OwnerUnit);
            obj.Properties.Add("PropDesignUnit", proj.DesignUnit);
            obj.Properties.Add("PropBuilderUnit", proj.ConstructionUnit);
            obj.Properties.Add("PropSupervisorUnit", proj.SupervisionUnit);

            return obj;
        }

        public static MFObject GetMfProject(this MetadataAliases aliases, Project proj)
        {
            var od = GetMfObjDef(aliases, "ObjProject", "ClassProject");
            var obj = new MFObject(od){Id = proj.InternalId};
            var projClass = aliases.Objects["ObjProject"].ClassDict["ClassProject"]; //projClass.PropDict[
            obj.Properties.Add("PropProjName", proj.Name);
            obj.Properties.Add("PropDescription", proj.Description);
            obj.Properties.Add("PropStartDate", proj.StartDateUtc);
            obj.Properties.Add("MFBuiltInPropertyDefDeadline", proj.EndDateUtc);
            if (projClass.PropDict.ContainsKey("PropProjNum"))
            {
                obj.Properties.Add("PropProjNum", proj.Number);
            }
            if (projClass.PropDict.ContainsKey("PropProjPhase"))
            {
                obj.Properties.Add("PropProjPhase", proj.StatusId);
            }
            if (projClass.PropDict.ContainsKey("PropProprietorUnit"))
            {
                obj.Properties.Add("PropProprietorUnit", proj.OwnerUnit);
            }
            if (projClass.PropDict.ContainsKey("PropDesignUnit"))
            {
                obj.Properties.Add("PropDesignUnit", proj.DesignUnit);
            }
            if (projClass.PropDict.ContainsKey("PropBuilderUnit"))
            {
                obj.Properties.Add("PropBuilderUnit", proj.ConstructionUnit);
            }
            if (projClass.PropDict.ContainsKey("PropSupervisorUnit"))
            {
                obj.Properties.Add("PropSupervisorUnit", proj.SupervisionUnit);
            }
            //obj.Properties.Add(projClass.PropDict["PropProjName"], proj.Name);

            return obj;
        }

        public static MFObjectDef GetMfObjDef(this MetadataAliases aliases, string typeKey, string classKey)
        {
            var od = new MFObjectDef();
            var contact = aliases.Objects[typeKey];
            od.TypeAlias = contact.Alias;
            var contactClass = contact.ClassDict[classKey];
            od.ClassAlias = contactClass.Alias;
            foreach (var p in contactClass.PropDict)
            {
                od.Properties.Add(p);
            }
            return od;
        }

        internal static int GetPartyId(this MetadataAliases aliases, Vault vault, string partyName)
        {
            var typeId = MfAlias.GetObjType(vault, aliases.Objects["ObjParticipant"].Alias);
            var scs = new SearchConditions();
            scs.Add(-1, MFSearchConditionUtils.ObjType(typeId));
            scs.Add(-1, MFSearchConditionUtils.Deleted(false));

            var res = vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone,
                false);
            foreach (ObjectVersion ov in res)
            {
                if (ov.Title == partyName)
                {
                    return ov.ObjVer.ID;
                }
            }
            return -1;
        }

        public static MFObject GetMfContact(this MetadataAliases aliases, Vault vault, MfContact contact)
        {
            var user = contact.User;
            var loginAccount = contact.Id;
            var od = GetMfObjDef(aliases, "ObjContacts", "ClassContacts");
            var obj = new MFObject(od){Id = contact.InternalId};
            var contactClass = aliases.Objects["ObjContacts"].ClassDict["ClassContacts"];
            if (loginAccount > 0)
            {
                obj.Properties.Add(contactClass.PropDict["PropAccount"], loginAccount);
            }
            if (!String.IsNullOrEmpty(contact.RoleAlias))
            {
                var roleId = MfAlias.GetUsergroup(vault, contact.RoleAlias);
                obj.Properties.Add(contactClass.PropDict["PropProjectRole"], roleId);
            }
            if (user != null) //contactClass.PropDict[
            {
                var name = String.IsNullOrEmpty(user.FullName) ? user.UserName : user.FullName;
                obj.Properties.Add("PropLinkmanName", name);
                obj.Properties.Add("PropTelPhone", user.Phone ?? "");
                obj.Properties.Add("PropEmail", user.Email);
                if (contactClass.PropDict.ContainsKey("PropQQ"))
                {
                    obj.Properties.Add("PropQQ", user.QQ ?? "");
                }
                if (contactClass.PropDict.ContainsKey("PropUnit"))
                {
                    var company = String.Empty;
                    if (user.Company != null)
                    {
                        company = user.Company.Name;
                    }
                    obj.Properties.Add("PropUnit", company);
                }
                if (contactClass.PropDict.ContainsKey("PropDepartment"))
                {
                    var department = String.Empty;
                    if (user.Department != null)
                    {
                        department = user.Department.Name;
                    }
                    obj.Properties.Add("PropDepartment", department);
                }
                if (contactClass.PropDict.ContainsKey("PropPosition"))
                {
                    obj.Properties.Add("PropPosition", user.Post ?? "");
                }
                if (contactClass.PropDict.ContainsKey("PropRemarks"))
                {
                    obj.Properties.Add("PropRemarks", user.Description);
                }
            }
            if (contactClass.PropDict.ContainsKey("PropUserStatus"))
            {
                obj.Properties.Add("PropUserStatus", contact.Disabled);
            }
            if (contactClass.PropDict.ContainsKey("PropIsProjCreator"))
            {
                obj.Properties.Add("PropIsProjCreator", contact.IsCreator);
            }

            if (!String.IsNullOrEmpty(contact.PartName))
            {
                var ownerAlias = aliases.Objects["ObjContacts"].Owner;
                if (ownerAlias != null)
                {
                    var objTypeId = MfAlias.GetObjType(vault, ownerAlias);
                    var objType = vault.ObjectTypeOperations.GetObjectType(objTypeId);
                    var pwnerPropertyId = objType.OwnerPropertyDef; //所属属性
                    obj.Properties.Add(pwnerPropertyId.ToString(), GetPartyId(aliases, vault, contact.PartName));
                    od.Properties.Add(pwnerPropertyId.ToString(), pwnerPropertyId.ToString());
                }
            }

            return obj;
        }
    }

    public class MfContact : InternalEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public bool IsCreator { get; set; }

        public bool Disabled { get; set; }

        public string PartName { get; set; }

        public string RoleAlias { get; set; }

        public string RoleName { get; set; }
    }
}
