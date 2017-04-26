using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;

namespace AecCloud.MFilesCore
{
    public static class VaultExtensions
    {
        public static string GetVaultURL(this Vault vault)
        {
            if (vault == null) throw new ArgumentNullException("vault");
            var dl = ClientUtils.GetDriveLetter();
            return dl + ":\\" + vault.Name;
        }

        public static int GetCurrentUserId(this Vault vault)
        {
            if (vault == null) throw new ArgumentNullException("vault");
            return vault.SessionInfo.UserID;
        }

        public static string GetCurrentAccountName(this Vault vault)
        {
            if (vault == null) throw new ArgumentNullException("vault");
            var name = vault.SessionInfo.AccountName;
            var index = name.IndexOf('\\');
            return vault.SessionInfo.AccountName.Substring(index+1);
        }

        private const int LogoutTryCount = 5;

        public static bool Logout(this Vault vault, int retryCount = LogoutTryCount)
        {
            if (vault == null) throw new ArgumentNullException("vault");
            var ok = true;
            if (vault.LoggedIn)
            {
                int count = 0;
                while (count < retryCount)
                {
                    ok = vault.LogOutWithDialogs(IntPtr.Zero);
                    if (ok) return true;
                    count++;
                }
            }
            return ok;
        }

        public static string GetSearchView(this Vault vault, string search, bool onlyDoc=true)
        {
            var viewName = "搜索：" + search + " - " + DateTime.Now.ToString("yyyyMMddHHmmss");
            var oViewNew = new View { Name = viewName };
            var searchStr = search ?? String.Empty;
            if (!String.IsNullOrWhiteSpace(searchStr))
            {
                searchStr += "*";
            }
            else
            {
                searchStr = String.Empty;
            }
            var oSc = new SearchCriteria
            {
                FullTextSearchString = searchStr,
                FullTextSearchFlags = (MFFullTextSearchFlags.MFFullTextSearchFlagsStemming
                                       | MFFullTextSearchFlags.MFFullTextSearchFlagsLookInMetaData
                                       | MFFullTextSearchFlags.MFFullTextSearchFlagsLookInFileData
                                       | MFFullTextSearchFlags.MFFullTextSearchFlagsTypeAnyWords),
                SearchFlags = MFSearchFlags.MFSearchFlagNone,
                ExpandUI = false
            };

            var oExpression = new Expression();
            oExpression.DataStatusValueType = MFStatusType.MFStatusTypeDeleted;
            oExpression.DataStatusValueDataFunction = MFDataFunction.MFDataFunctionNoOp;
            var oTypedValue = new TypedValue();
            oTypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            var oDeletedEx = new SearchConditionEx();
            oDeletedEx.SearchCondition.Set(oExpression, MFConditionType.MFConditionTypeEqual, oTypedValue);
            oDeletedEx.Enabled = true;
            oDeletedEx.Ignored = false;
            oDeletedEx.SpecialNULL = false;
            oSc.AdditionalConditions.Add(-1, oDeletedEx);

            SearchConditions baseConditions = null;

            if (onlyDoc)
            {
                baseConditions = new SearchConditions();
                var objTypeSc =
                    MFSearchConditionUtils.ObjType((int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument);
                baseConditions.Add(-1, objTypeSc);
            }
            var oViewNew1 = vault.ViewOperations.AddTemporarySearchView(oViewNew, oSc, baseConditions);
            return vault.ViewOperations.GetViewLocationInClient(oViewNew1.ID);
        }

        public static ObjectVersion CheckoutObject(this Vault vault, ObjID objId, bool updateFromServer=false)
        {
            if (vault == null) throw new ArgumentNullException("vault");
            if (objId == null) throw new ArgumentNullException("objId");
            var checkedOut = vault.ObjectOperations.IsCheckedOut(objId, updateFromServer);
            if (checkedOut)
            {
                var objVer = new ObjVer();
                objVer.SetObjIDAndVersion(objId, -1);
                return vault.ObjectOperations.GetObjectInfo(objVer, true, updateFromServer);
            }
            return vault.ObjectOperations.CheckOut(objId);
        }

        public static ObjectSearchResults GetObjectsCheckedoutByMe(this Vault vault)
        {
            var scs = new SearchConditions();
            var currentUserId = vault.GetCurrentUserId();
            var checkedoutSC = MFSearchConditionUtils.Status(MFConditionType.MFConditionTypeEqual,
                MFStatusType.MFStatusTypeCheckedOutTo, MFDataType.MFDatatypeLookup, currentUserId);

            scs.Add(-1, checkedoutSC);

            return vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlags.MFSearchFlagNone, false);
        }

        public static ObjectVersionAndProperties CreateSingleFileObject(this Vault vault, int objType, int classId,
            PropertyValues pvs, SourceObjectFile file, bool checkIn)
        {
            var classPV = MFPropertyUtils.Class(classId);
            pvs.Add(0, classPV);

            return vault.ObjectOperations.CreateNewSFDObject(objType, pvs, file, checkIn);
        }


        public static ObjectVersionAndProperties CreateNewObject(this Vault vault, int objType, int classId,
            PropertyValues pvs, SourceObjectFiles files=null)
        {
            var classPV = MFPropertyUtils.Class(classId);
            pvs.Add(0, classPV);

            if (files == null || files.Count != 0)
            {
                var singleFilePV = MFPropertyUtils.SingleFile(false);
                pvs.Add(-1, singleFilePV);
            }

            return vault.ObjectOperations.CreateNewObject(objType, pvs, files);
        }
        
    }
}
