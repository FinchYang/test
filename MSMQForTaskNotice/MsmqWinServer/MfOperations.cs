using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;
using MSMQ.Core;

namespace MsmqWinService
{
    public class MfOperations
    {
        public ObjectVersion CreateNotice(Vault mfVault, MfTask notice)
        {
            var oPropValues = new PropertyValues();

            if (!string.IsNullOrEmpty(notice.Content))
            {
                var oPropContent = new PropertyValue {PropertyDef = 41};
                oPropContent.TypedValue.SetValue(MFDataType.MFDatatypeMultiLineText, notice.Content);
                oPropValues.Add(-1, oPropContent);
            }
            if (notice.AssignTo > 0)
            {
                var assignTos = new List<int> { notice.AssignTo };
                var oPropAssignTo = new PropertyValue {PropertyDef = 44};
                oPropAssignTo.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, assignTos.ToArray());
                oPropValues.Add(-1, oPropAssignTo);
            }
            foreach (MfProperty p in notice.OtherProps)
            {
                var type = GetPropDefType(mfVault, p.PropDef);
                var oPropValue = new PropertyValue
                {
                    PropertyDef = p.PropDef
                };

                if (type == MFDataType.MFDatatypeLookup)
                {
                    int val;
                    if (int.TryParse(p.Value, out val))
                    {
                        oPropValue.TypedValue.SetValue(MFDataType.MFDatatypeLookup, val);
                        oPropValues.Add(-1, oPropValue);
                    }
                    
                }
                else if (type == MFDataType.MFDatatypeMultiSelectLookup)
                {
                    if (!string.IsNullOrEmpty(p.Value))
                    {
                        var ids = new List<int>();
                        var vArr = p.Value.Split(new[] {'_'});
                        foreach (string s in vArr)
                        {
                            int val;
                            if (int.TryParse(s, out val))
                            {
                                ids.Add(val);
                            }
                        }
                        if(ids.Count > 0)
                        {
                            oPropValue.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, ids.ToArray());
                            oPropValues.Add(-1, oPropValue);
                        }  
                    } 
                }
                else if (type == MFDataType.MFDatatypeText)
                {
                    oPropValue.TypedValue.SetValue(MFDataType.MFDatatypeText, p.Value);
                    oPropValues.Add(-1, oPropValue);
                }
                else if (type == MFDataType.MFDatatypeMultiLineText)
                {
                    oPropValue.TypedValue.SetValue(MFDataType.MFDatatypeMultiLineText, p.Value);
                    oPropValues.Add(-1, oPropValue);
                }
            }
            //if (notice.OtherPropDef > 0 && notice.OtherPropValue > 0)
            //{
            //    var type = GetPropDefType(mfVault, notice.OtherPropDef);
            //    var oPropValue = new PropertyValue
            //                     {
            //                         PropertyDef = notice.OtherPropDef
            //                     };
            //    if (type == MFDataType.MFDatatypeLookup)
            //    {
            //        oPropValue.TypedValue.SetValue(MFDataType.MFDatatypeLookup, notice.OtherPropValue);
            //        oPropValues.Add(-1, oPropValue);
            //    }
            //    else if (type == MFDataType.MFDatatypeMultiSelectLookup)
            //    {
            //        var ids = new List<int> { notice.OtherPropValue };
            //        oPropValue.TypedValue.SetValue(MFDataType.MFDatatypeMultiSelectLookup, ids.ToArray());
            //        oPropValues.Add(-1, oPropValue);
            //    }      
            //}

            return CreateMfObj(mfVault, notice.ObjType, notice.ObjClass, notice.Title, oPropValues);
        }
        /// <summary>
        /// 新建对象
        /// </summary>
        private ObjectVersion CreateMfObj(Vault mfVault, int typeId, int classId, string title, PropertyValues oPropValues = null)
        {
            if (oPropValues == null)
            {
                oPropValues = new PropertyValues();
            }
            //名称(*)
            var oPropValueTitle = new PropertyValue();
            oPropValueTitle.PropertyDef = 0;
            oPropValueTitle.TypedValue.SetValue(MFDataType.MFDatatypeText, title);
            oPropValues.Add(-1, oPropValueTitle);
            //是否为单文档(*)
            var oPropValueSingle = new PropertyValue();
            oPropValueSingle.PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject;
            oPropValueSingle.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
            oPropValues.Add(-1, oPropValueSingle);
            //类别(*)
            var oPropValueClass = new PropertyValue();
            oPropValueClass.PropertyDef = (int)MFBuiltInPropertyDef.MFBuiltInPropertyDefClass;
            oPropValueClass.TypedValue.SetValue(MFDataType.MFDatatypeLookup, classId);
            oPropValues.Add(-1, oPropValueClass);

            //创建
            var oObjVnAndProps = mfVault.ObjectOperations.CreateNewObject(typeId, oPropValues);
            return mfVault.ObjectOperations.CheckIn(oObjVnAndProps.ObjVer);
        }

        private MFDataType GetPropDefType(Vault mfVault, int propDef)
        {
            return mfVault.PropertyDefOperations.GetPropertyDef(propDef).DataType;
        }
    }
}
