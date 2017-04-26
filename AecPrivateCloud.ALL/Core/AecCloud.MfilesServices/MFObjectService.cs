using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AecCloud.Core;
using AecCloud.Core.Domain.Projects;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore;
using AecCloud.MFilesCore.Metadata;
using log4net;
using MFilesAPI;

namespace AecCloud.MfilesServices
{
    public class MFObjectService : IMFObjectService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static void SetProperties(Vault vault, PropertyValues pvs, MFObject obj)
        {
            foreach (var key in obj.Properties.Keys)
            {
                var value = obj.Properties[key];
                try
                {
                    //if (!obj.ObjDef.Properties.ContainsKey(key))
                    //{
                        
                    //}
                    var propId = MfAlias.GetPropDef(vault, obj.ObjDef.Properties[key]);
                    var pv = MFPropertyUtils.Create(vault, propId, value);
                    pvs.Add(-1, pv);
                }
                catch(Exception ex)
                {
                    Log.Error("创建MF对象失败：" + ex.Message, ex);
                    throw;
                }
            }
        }
        internal static void Create(Vault vault, MFObject obj)
        {
          //  Writelog(string.Format("--11--{0},{1},{2}",obj.Id,obj.Properties.Count,obj));
            var objType = MfAlias.GetObjType(vault, obj.ObjDef.TypeAlias);
         //   Writelog("--22--");
            var objClass = MfAlias.GetObjectClass(vault, obj.ObjDef.ClassAlias);
         //   Writelog("--33--");
            var pvs = new PropertyValues();
            var classPV = MFPropertyUtils.Class(objClass);
            pvs.Add(-1, classPV);
         //   Writelog("--44--");
            SetProperties(vault, pvs, obj);

            var isSingleFile = false;
         //   Writelog("--55--");
            SourceObjectFiles files = null;
            if (obj.Filepaths != null && obj.Filepaths.Count > 0)
            {
                files = new SourceObjectFiles();
                //todo
                if (obj.Filepaths.Count == 1)
                {
                    isSingleFile = true;
                }
            //    Writelog("--66--");
            }
            var singleFilePV = MFPropertyUtils.SingleFile(isSingleFile);
            pvs.Add(-1, singleFilePV);
         //   Writelog("--77--");
            try
            {
                var objVersion = vault.ObjectOperations.CreateNewObject(objType, pvs, files);
           //     Writelog("--88--");
                var newObjVersion = vault.ObjectOperations.CheckIn(objVersion.ObjVer);

                obj.Id = newObjVersion.ObjVer.ID;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("CreateNewObject error:{0},{1},{2}",objType,pvs.Count,ex.Message));
            }
        }
        public static void Writelog(string logtext)
        {
            //var tmpfile = System.IO.Path.Combine(HttpContext.Current.Server.MapPath("~"), "aecloudweblog.txt");
            ////    var tmpfile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "aecloudweblog.txt");
            //try
            //{
            //    using (var sw = System.IO.File.AppendText(tmpfile))
            //    {
            //        sw.WriteLine(DateTime.Now.ToLocalTime() + "---" + logtext);
            //        sw.Close();
            //    }
            //}
            //catch (Exception) { }
        }

        public void Create<T>(MFilesVault vault, MetadataAliases aliases, T entity) where T : InternalEntity
        {
           // Writelog("in mfs.mfos.create 11");
            var projObj = GetMFObject(vault, aliases, entity);
           // Writelog("in mfs.mfos.create 22");
            Create(vault, projObj);
          //  Writelog("in mfs.mfos.create 33");
            entity.InternalId = projObj.Id;
        }

        internal static void Create<T>(Vault vault, MetadataAliases aliases, T entity) where T : InternalEntity
        {
            var projObj = GetMFObject(vault, aliases, entity);
            Create(vault, projObj);
            entity.InternalId = projObj.Id;
        }

        internal static void Update(Vault vault, MFObject obj)
        {
            var objType = MfAlias.GetObjType(vault, obj.ObjDef.TypeAlias);
            var pvs = new PropertyValues();
            SetProperties(vault, pvs, obj);
            ObjID objID = null;
            if (obj.Id <= 0)
            {
                var objClass = MfAlias.GetObjectClass(vault, obj.ObjDef.ClassAlias);
                objID = SearchObjectByClass(vault, objType, objClass); //new ObjID();
                //objID.SetIDs(objType, obj.Id);
            }
            else
            {
                objID = SearchObjectByObjId(vault, objType, obj.Id);
            }

            var objVersion = vault.ObjectOperations.CheckOut(objID);
            var newobjVersion = vault.ObjectPropertyOperations.SetProperties(objVersion.ObjVer, pvs);
            vault.ObjectOperations.CheckIn(newobjVersion.ObjVer);
        }

        public void Update<T>(MFilesVault vault, MetadataAliases aliases, T entity) where T : InternalEntity
        {
            var obj = GetMFObject(vault, aliases, entity);
            Update(vault, obj);
        }

        private static ObjID SearchObjectByClass(Vault vault, int objType, int objClass)
        {
            var scs = new SearchConditions();
            MFSearchConditionUtils.AddBaseConditions(scs, objType, objClass);

            var res = vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone,
                false);
            return res[1].ObjVer.ObjID;
        }

        private static ObjID SearchObjectByObjId(Vault vault, int objType, int objId)
        {
            var scs = new SearchConditions();
            var typeSc = MFSearchConditionUtils.ObjType(objType);
            scs.Add(-1, typeSc);
            var idSc = MFSearchConditionUtils.ObjId(objId);
            scs.Add(-1, idSc);
            var res = vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone,
                false);
            return res[1].ObjVer.ObjID;
        }


        internal void Delete(Vault vault, int objType, int objId)
        {
            throw new NotImplementedException();
        }

        public MFObject GetMFObject<T>(MFilesVault vault, MetadataAliases aliases, T entity) where T : InternalEntity
        {
            if (entity is Project)
            {
                //var config = VaultConfig.GetConfigFromString(vault.Template.MetadataJson);
                var projObj = aliases.GetMfProject(entity as Project);// config.GetMfProjectObject(entity as Project);
                return projObj;
            }
            if (entity is MfContact)
            {
                var mfVault = MFServerUtility.GetVault(vault);
                var contactObj = aliases.GetMfContact(mfVault, entity as MfContact);
                return contactObj;
            }
            return null;
        }

        internal static MFObject GetMFObject<T>(Vault mfVault, MetadataAliases aliases, T entity) where T : InternalEntity
        {
            if (entity is Project)
            {
                //var config = VaultConfig.GetConfigFromString(vault.Template.MetadataJson);
                var projObj = aliases.GetMfProject(entity as Project);// config.GetMfProjectObject(entity as Project);
                return projObj;
            }
            if (entity is MfContact)
            {
                var contactObj = aliases.GetMfContact(mfVault, entity as MfContact);
                return contactObj;
            }
            return null;
        }

        public MFObject GetMFObject(MFilesVault vault, MetadataAliases aliases, InternalEntity entity)
        {
            return GetMFObject<InternalEntity>(vault, aliases, entity);
        }

        internal void Create(MFilesVault vault, MFObject obj)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            try
            {
                Create(mfVault, obj);
            }
            catch (Exception ex)
            {
                throw new Exception(obj.ToString(), ex);
            }
        }

        internal void Update(MFilesVault vault, MFObject obj)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            Update(mfVault, obj);
        }

        public void Delete(MFilesVault vault, int objType, int objId)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            Delete(mfVault, objType, objId);
        }

        private static int GetContactAccount(Vault vault, MetadataAliases aliases, int contactId)
        {
            var objId = new ObjID();
            var objAlias = aliases.Objects["ObjContacts"];
            var objType = MfAlias.GetObjType(vault, objAlias.Alias);
            var classAlias = objAlias.ClassDict["ClassContacts"]; //PropAccount
            objId.SetIDs(objType, contactId);
            var objVer = vault.ObjectOperations.GetLatestObjVer(objId, false);
            var propId = MfAlias.GetPropDef(vault, classAlias.PropDict["PropAccount"]);
            try
            {
                var tv = vault.ObjectPropertyOperations.GetProperty(objVer, propId).Value;
                return tv.GetLookupID();
            }
            catch (Exception ex)
            {
                var err = "获取登录账户失败： " + objType + " # " + contactId + " # " + ex.Message;
                Log.Error(err, ex);
                throw;
            }
        }

        public MFProjectParty GetParty(MFilesVault vault, string partyName, MetadataAliases aliases, int? currentUserId=null)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            var partyObj = aliases.Objects["ObjParticipant"];
            var partyClass = partyObj.ClassDict["ClassParticipant"];
            var typeId = MfAlias.GetObjType(mfVault, partyObj.Alias);
            var classId = MfAlias.GetObjectClass(mfVault, partyClass.Alias);

            var scs = new SearchConditions();
            MFSearchConditionUtils.AddBaseConditions(scs, typeId, classId);

            var namePropId = MfAlias.GetPropDef(mfVault, partyClass.PropDict["PropParticipantName"]);
            var nameSC = MFSearchConditionUtils.Property(MFConditionType.MFConditionTypeEqual, namePropId,
                MFDataType.MFDatatypeText, partyName);
            scs.Add(-1, nameSC);

            var res = mfVault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone,
                false);
            if (res.Count != 1) return null;

            var party = new MFProjectParty {Name = partyName};
            var obj = res[1];
            party.InternalId = obj.ObjVer.ID;
            var managerPropId = MfAlias.GetPropDef(mfVault, partyClass.PropDict["PropProjPM"]);
            var managerTV = mfVault.ObjectPropertyOperations.GetProperty(obj.ObjVer, managerPropId).Value;
            if (!managerTV.IsNULL() && !managerTV.IsUninitialized())
            {
                party.ManagerCount = 1;
                var contactId = managerTV.GetLookupID();
                var userId = GetContactAccount(mfVault, aliases, contactId);
                if (currentUserId != null && userId == currentUserId.Value)
                {
                    party.IsCurrentManager = true;
                }
            }
            var vmPropId = MfAlias.GetPropDef(mfVault, partyClass.PropDict["PropProjVicePMs"]);
            var vmTV = mfVault.ObjectPropertyOperations.GetProperty(obj.ObjVer, vmPropId).Value;
            if (!vmTV.IsNULL() && !vmTV.IsUninitialized())
            {
                var lkps = vmTV.GetValueAsLookups();
                party.ViceManagerCount = lkps.Count;
                if (currentUserId != null && !party.IsCurrentManager)
                {
                    for (var i = 1; i <= lkps.Count; i++)
                    {
                        var contactId = lkps[i].Item;
                        var userId = GetContactAccount(mfVault, aliases, contactId);
                        if (userId == currentUserId.Value)
                        {
                            party.IsCurrentViceManager = true;
                            break;
                        }
                    }
                }
            }
            var mmPropId = MfAlias.GetPropDef(mfVault, partyClass.PropDict["PropMembers"]);
            var mmTV = mfVault.ObjectPropertyOperations.GetProperty(obj.ObjVer, mmPropId).Value;
            if (!mmTV.IsNULL() && !mmTV.IsUninitialized())
            {
                var lkps = mmTV.GetValueAsLookups();
                party.MemberCount = lkps.Count;
                if (currentUserId != null && !party.IsCurrentViceManager && !party.IsCurrentManager)
                {
                    for (var i = 1; i <= lkps.Count; i++)
                    {
                        var contactId = lkps[i].Item;
                        var userId = GetContactAccount(mfVault, aliases, contactId);
                        if (userId == currentUserId.Value)
                        {
                            party.IsCurrentMember = true;
                            break;
                        }
                    }
                }
            }

            return party;
        }



        public MfContact GetContact(MFilesVault vault, MetadataAliases aliases, string userName)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            var userId = MfUserUtils.GetUserAccount(mfVault, userName);
            if (userId == null) return null;
            var uid = userId.Value; //userid
            return GetContactByUserId(mfVault, aliases, uid);

        }

        public MfContact GetContactByUserId(MFilesVault vault, MetadataAliases aliases, int userId)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            return GetContactByUserId(mfVault, aliases, userId);
        }

        public MfContact GetContactByContactId(MFilesVault vault, MetadataAliases aliases, int contactId)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            return GetContactById(mfVault, aliases, contactId);
        }

        internal static MfContact GetContactById(Vault mfVault, MetadataAliases aliases, int contactId)
        {
            var objAlias = aliases.Objects["ObjContacts"];
            var classAlias = objAlias.ClassDict["ClassContacts"];
            var typeId = MfAlias.GetObjType(mfVault, objAlias.Alias);

            var objId = new ObjID();
            objId.SetIDs(typeId, contactId);
            var objVer = mfVault.ObjectOperations.GetLatestObjVer(objId, false);

            var accountPDId = MfAlias.GetPropDef(mfVault, classAlias.PropDict["PropAccount"]);
            var accountTV = mfVault.ObjectPropertyOperations.GetProperty(objVer, accountPDId).Value;
            var userId = accountTV.GetLookupID();
            var contact = new MfContact
            {
                UserId = userId,
                InternalId = objVer.ID,
                RoleAlias = classAlias.PropDict["PropProjectRole"]
            };
            var roleId = MfAlias.GetPropDef(mfVault, contact.RoleAlias);//mfVault.UserGroupOperations.GetUserGroupIDByAlias(contact.RoleAlias);
            contact.RoleName = mfVault.ObjectPropertyOperations.GetProperty(objVer, roleId).GetValueAsLocalizedText();
            if (!String.IsNullOrEmpty(objAlias.Owner))
            {
                var partTypeId = MfAlias.GetObjType(mfVault, objAlias.Owner);
                var partType = mfVault.ObjectTypeOperations.GetObjectType(partTypeId);
                var partOwnerPropId = partType.OwnerPropertyDef;
                var partyTV = mfVault.ObjectPropertyOperations.GetProperty(objVer, partOwnerPropId).Value;
                contact.PartName = partyTV.DisplayValue;
            }

            return contact;
        }
        internal static MfContact GetContactByUserId(Vault mfVault, MetadataAliases aliases, int userId)
        {
            var objAlias = aliases.Objects["ObjContacts"];
            var classAlias = objAlias.ClassDict["ClassContacts"];
            var typeId = MfAlias.GetObjType(mfVault, objAlias.Alias);
            var classId = MfAlias.GetObjectClass(mfVault, classAlias.Alias);

            var scs = new SearchConditions();
            MFSearchConditionUtils.AddBaseConditions(scs, typeId, classId);

            var userPropId = MfAlias.GetPropDef(mfVault, classAlias.PropDict["PropAccount"]);
            scs.Add(-1,
                MFSearchConditionUtils.Property(MFConditionType.MFConditionTypeEqual, userPropId,
                    MFDataType.MFDatatypeLookup, userId));

            var res = mfVault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone,
                false);

            if (res.Count != 1) return null;
            var obj = res[1];

            var contact = new MfContact
            {
                UserId = userId,
                InternalId = obj.ObjVer.ID,
                RoleAlias = classAlias.PropDict["PropProjectRole"]
            };

            var roleId = MfAlias.GetPropDef(mfVault, contact.RoleAlias);//mfVault.UserGroupOperations.GetUserGroupIDByAlias(contact.RoleAlias);
            contact.RoleName = mfVault.ObjectPropertyOperations.GetProperty(obj.ObjVer, roleId).GetValueAsLocalizedText();

            if (!String.IsNullOrEmpty(objAlias.Owner))
            {
                var partTypeId = MfAlias.GetObjType(mfVault, objAlias.Owner);
                var partType = mfVault.ObjectTypeOperations.GetObjectType(partTypeId);
                var partOwnerPropId = partType.OwnerPropertyDef;
                var partyTV = mfVault.ObjectPropertyOperations.GetProperty(obj.ObjVer, partOwnerPropId).Value;
                contact.PartName = partyTV.DisplayValue;
            }
            

            return contact;
        }


        public MFDownloadFile DownloadFile(MFilesVault vault, int objType, int objId, int fileId)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            var objID = new ObjID();
            objID.SetIDs(objType, objId);
            var objVer = mfVault.ObjectOperations.GetLatestObjVer(objID, false, false);
            var objVersion = mfVault.ObjectOperations.GetObjectInfo(objVer, true, false);
            if (objVersion.FilesCount == 0) return null;
            if (fileId > 0)
            {
                ObjectFile fv = null;
                foreach (ObjectFile file in objVersion.Files)
                {
                    if (file.FileVer.ID == fileId)
                    {
                        fv = file;
                        break;
                    }
                }
                if (fv == null)
                {
                    fv = objVersion.Files[1];
                }
                var session = mfVault.ObjectFileOperations.DownloadFileInBlocks_Begin(fv.FileVer.ID, fv.FileVer.Version);
                var content = mfVault.ObjectFileOperations.DownloadFileInBlocks_ReadBlock(session.DownloadID, session.FileSize32, 0);
                return new MFDownloadFile
                {
                    Content = content,
                    Name = fv.Title,
                    Extension = fv.Extension,
                    Version = objVer.Version
                };
            }
            return null;
        }


        public int GetObjectVersion(MFilesVault vault, int objType, int objId)
        {
            var mfVault = MFServerUtility.GetVault(vault);
            var objID = new ObjID();
            objID.SetIDs(objType, objId);
            var objVer = mfVault.ObjectOperations.GetLatestObjVer(objID, false, false);
            return objVer.Version;
        }
    }
}
