/*************************
*新建MFiles对象的各类型属性
**************************/
var MF = MF || {};

(function() {
    var p = {
        //新建文本属性
        newTextProperty: function (propDefId, value) {
            ///<summary>新建文本属性</summary>
            ///<param name="propDefId" type="long">PropertyDef</param>
            ///<param name="value" type="text">值</param>
            return this.createProperty(propDefId, MFDatatypeText, value);
        },
        //新建多文本属性
        newMultiLineTextProperty: function (propDefId, value) {
            ///<summary>新建多文本属性</summary>
            ///<param name="propDefId" type="long">PropertyDef</param>
            ///<param name="value" type="text">值</param>
            return this.createProperty(propDefId, MFDatatypeMultiLineText, value);
        },
        //新建单选属性
        newLookupProperty: function (propDefId, value) {
            ///<summary>新建单选-Lookup属性</summary>
            ///<param name="propDefId" type="long">PropertyDef</param>
            ///<param name="value" type="int or Lookup">值</param>
            return this.createProperty(propDefId, MFDatatypeLookup, value);
        },
        //新建多选属性
        newMultiSelectLookupProperty: function (propDefId, arryValue) {
            ///<summary>新建多选属性</summary>
            ///<param name="propDefId" type="long">PropertyDef</param>
            ///<param name="arryValue" type="int[]">整型数组值</param>
            var value = new MFiles.Lookups();
            for (var i = 0; i < arryValue.length; i++) {
                var item = new MFiles.Lookup();
                item.Item = arryValue[i];
                value.Add(-1, item);
            }
            value = value.Count === 0 ? null : value;
            return this.createProperty(propDefId, MFDatatypeMultiSelectLookup, value);
        },
        //新建整型属性
        newIntegerProperty: function (propDefId, value) {
            ///<summary>新建整型属性</summary>
            ///<param name="propDefId" type="long">PropertyDef</param>
            ///<param name="value" type="int">整型值</param>
            return this.createProperty(propDefId, MFDatatypeInteger, value);
        },
        //新建浮点型属性
        newFloatProperty: function (propDefId, value) {
            ///<summary>新建浮点型属性</summary>
            ///<param name="propDefId" type="long">PropertyDef</param>
            ///<param name="value" type="float">浮点型值</param>
            return this.createProperty(propDefId, MFDatatypeFloating, value);
        },
        //新建布尔型属性
        newBooleanProperty: function (propDefId, value) {
            ///<summary>新建布尔型属性</summary>
            ///<param name="propDefId" type="long">PropertyDef</param>
            ///<param name="value" type="bool">布尔型值</param>
            return this.createProperty(propDefId, MFDatatypeBoolean, value);
        },
        //新建date属性
        newDateProperty: function (propDefId, value) {
            ///<summary>新建date属性</summary>
            ///<param name="propDefId" type="long">PropertyDef</param>
            ///<param name="value" type="DateTime">js的DateTime值</param>
            var pvDate = new MFiles.PropertyValue();
            pvDate.PropertyDef = propDefId;
            if (value) {
                //SetValue时转换日期，才不会报错
                var mfValue = this._getMfDate(value);
                pvDate.TypedValue.SetValue(MFDatatypeDate, mfValue);
            } else {
                pvDate.TypedValue.SetValueToNULL(MFDatatypeDate);
            }
            return pvDate;
        },
        //新建time属性
        newTimeProperty: function (propDefId, value) {
            ///<summary>新建time属性</summary>
            ///<param name="propDefId" type="long">PropertyDef</param>
            ///<param name="value" type="DateTime">js的DateTime值</param>
            var pvTime = new MFiles.PropertyValue();
            pvTime.PropertyDef = propDefId;
            if (value) {
                //SetValue时转换日期，才不会报错
                var mfValue = this._getMfTime(value);
                pvTime.TypedValue.SetValue(MFDatatypeTime, mfValue);
            } else {
                pvTime.TypedValue.SetValueToNULL(MFDatatypeTime);
            }
            return pvTime;
        },
        //将js的Date日期部分转换成Mfiles日期
        _getMfDate : function (jsDate) {
            var ts = new MFiles.Timestamp();
            //ts.SetValue(jsDate);
            ts.Year = jsDate.getFullYear();
            ts.Month = jsDate.getMonth() + 1;
            ts.Day = jsDate.getDate();
            return ts.GetValue();
        },
        //将js的Date时间部分转换成Mfiles时间
        _getMfTime: function (jsDate) {
            var ts = new MFiles.Timestamp();
            //ts.SetValue(jsDate);
            ts.Hour = jsDate.getHours();
            ts.Minute = jsDate.getMinutes();
            ts.Second = jsDate.getSeconds();
            return ts.GetValue();
        },
        createProperty: function (propDefId, valueType, value) {
            ///<summary>生成M-Files中的PropertyValue对象实例</summary>
            ///<param name="propDefId" type="long">PropertyDef</param>
            ///<param name="valueType" type="int">属性值的类型，如：MFDatatypeLookup</param>
            var pv = new MFiles.PropertyValue();
            pv.PropertyDef = propDefId;
            if (value || value === 0) {
                pv.Value.SetValue(valueType, value);
            } else {
                pv.Value.SetValueToNULL(valueType);
            }
            return pv;
        }
    };
    MF.property = p;
})(MF);