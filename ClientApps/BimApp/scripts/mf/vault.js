/**********************************************************
* M-Files Vault Operation...
* like Object URL, ValueList, CreateProperty, get Template etc...
* export name: MF.vault
* dependency: client.js
***********************************************************/
var MF = MF || {}; //必须预先声明，否则M-Files会报未定义变量
(function(u, undefined){
    var vault = {
        getVaultURL: function(mfVault) {
            ///<summary>M:\库名称\ </summary>
            return u.getDriveLetter() + ":\\" + mfVault.Name + "\\";
        },

        isNull: function (propertyValue) {
            ///<summary>属性值是否为null</summary>
            return propertyValue.Value.IsNull() || propertyValue.Value.IsUninitialized();
        },

        getNamedValue: function(mfVault, type, ns, key) {
            ///<summary>获取共享变量的值</summary>
            ///<param name="type">MFUserDefinedValue</param>
            ///<param name="ns">namespace</param>
            ///<param name="key">存储的Key</param>
            var nvs = mfVault.NamedValueStorageOperations.GetNamedValues(type,
                    ns);
            return nvs.Value(key);
        },

        setNamedValues: function (mfVault, type, ns, obj) {
            ///<summary>设置共享变量的值</summary>
            ///<param name="type">MFUserDefinedValue</param>
            ///<param name="ns">namespace</param>
            ///<param name="obj">存储的(Key,Value)</param>
            var namedValues = mfVault.NamedValueStorageOperations.GetNamedValues(type,
                    ns);
            if (!namedValues) {
                namedValues = u.createObject('NamedValues');
            }
            for (var k in obj) {
                if (!obj.hasOwnProperty(k)) continue;
                namedValues.Values(k) = obj[k]; // This is OK in M-Files
            }
            mfVault.NamedValueStorageOperations.SetNamedValues(type, ns, namedValues);
        },

        createProperty: function(propId, valueType, value) {
            ///<summary>生成M-Files中的PropertyValue对象实例</summary>
            ///<param name="propId" type="long">PropertyDef</param>
            ///<param name="valueType" type="int">属性值的类型，如：MFDatatypeLookup</param>
            var pv = u.createObject('PropertyValue');
            pv.PropertyDef = propId;
            if (value || value === 0 || value === '') {
                pv.Value.SetValue(valueType, value);
            } else {
                pv.Value.SetValueToNULL(valueType);
            }
            return pv;
        },

        getValueListItemId: function(mfVault, valueListId, itemName) {
            ///<summary>根据值列表项的名称获取对应的值列表项ID</summary>
            ///<param name="valueListId" type="long">值列表的ID</param>
            ///<param name="itemName" type="String">值列表项的名称</param>
            var values = mfVault.ValueListItemOperations.GetValueListItems(valueListId);
            var id;
            for (var i = 1; i <= values.Count; i++) {
                if (values.Item(i).Name === itemName) {
                    id = values.Item(i).ID;
                    break;
                }
            }
            return id;
        },

        getTemplates: function(mfVault, classId) {
            ///<summary>获得对应类别或类别组的模板列表</summary>
            ///<param name="classObj">{type:MFBuiltInPropertyDefClass, value: classId},
            /// {type:MFBuiltInPropertyDefClassGroups, value: classId}</param>
            ///<returns>ObjectSearchResults</returns>
            var scs = u.createObject('SearchConditions');
            var scDEL = u.createObject('SearchCondition');
            scDEL.ConditionType = MFConditionTypeEqual;
            scDEL.Expression.DataStatusValueType = MFStatusTypeDeleted;
            scDEL.TypedValue.SetValue(MFDatatypeBoolean, false);
            scs.Add(-1, scDEL);
            var scCLASS = u.createObject('SearchCondition');
            scCLASS.ConditionType = MFConditionTypeEqual;
            scCLASS.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass; //MFBuiltInPropertyDefClass;
            scCLASS.TypedValue.SetValue(MFDatatypeLookup, classId);
            scs.Add(-1, scCLASS);
            var scTemp = u.createObject('SearchCondition');
            scTemp.ConditionType = MFConditionTypeEqual;
            scTemp.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefIsTemplate;
            scTemp.TypedValue.SetValue(MFDatatypeBoolean, true);
            scs.Add(-1, scTemp);
            var results = mfVault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlagNone, false);
            return results;
        },

        getObjectFromLookup: function(mfVault, lookup, includeProps) {
            var idObj = u.createObject('ObjID');
            idObj.SetIDs(lookup.ObjectType, lookup.Item);
            if (includeProps) {
                return mfVault.ObjectOperations.GetLatestObjectVersionAndProperties(idObj, true, false);
            } else {
                var objVer = u.createObject('ObjVer');
                objVer.SetObjIDAndVersion(idObj, lookup.Version);
                return mfVault.ObjectOperations.GetObjectInfo(objVer, true, false);
            }
        },

        lookup2ObjVer: function(lookup) {
            var objVer = u.createObject('ObjVer');
            objVer.SetIDs(lookup.ObjectType, lookup.Item, lookup.Version);
            return objVer;
        },

        getMFilesObject: function(mfVault, objType, objId, includeProps) {
            ///<summary>获得M-Files对象实例，包括属性列表</summary>
            ///<param name="objType" type="int">对象的类型</param>
            ///<param name="objId" type="long">对象ID</param>
            ///<param name="includeProps" type="bool">是否包含属性</param>
            ///<returns>ObjectVersionAndProperties</returns>
            var idObj = u.createObject('ObjID');
            idObj.SetIDs(objType, objId);
            if (includeProps) {
                return mfVault.ObjectOperations.GetLatestObjectVersionAndProperties(idObj, true, false);
            } else {
                var objVer = u.createObject('ObjVer');
                objVer.SetObjIDAndVersion(idObj, -1);
                return mfVault.ObjectOperations.GetObjectInfo(objVer, true, false);
            }
        },

        getVaultGUID: function(mfVault) {
            ///<summary>获得库的GUID，不包含'{'和'}'</summary>
            try {
                return mfVault.GetGUID().replace('{', '').replace('}', ''); //shell module        
            } catch(e) {
                return mfVault.SessionInfo.VaultGUID; //vault module
            }
        },

        _getIdRange: function(id) {
            var count = 1000;
            var sep = 999;
            var i = 0;
            while (true) {
                var start = i * count;
                var end = start + sep;
                if (id >= start && id <= end) {
                    return start.toString() + '-' + end.toString();
                }
                i = i + 1;
            }
        },

        _getFilenameWithId: function (objectType, objTitle, fileName, objectId) {
            var index = fileName.lastIndexOf('.');
            if (objectType == MFBuiltInObjectTypeDocument) {
                return fileName.substring(0, index) + ' (ID ' + objectId + ')' + fileName.substring(index);
            } else {
                return objTitle + ' (ID ' + objectType + '-' + objectId + ')\\' + fileName.substring(index);
            }
        },

        _getFilePathWithId: function(objectType, objectId, objTitle, fileName) {
            ///<summary>[ObjectTypeID]\[ObjectIDRange]\[ObjectID]\L\L\Filename</summary>
            ///<param name="fileName">带有扩展名的文件名称</param>
            var idRange = this._getIdRange(objectId);
            return objectType + '\\' + idRange + '\\' + objectId + '\\L\\L\\' + this._getFilenameWithId(objectType, objTitle, fileName, objectId);
        },

        getObjectFilePath: function(mfVault, objVer, fileIndex) {
            ///<summary>获得M-Files对象的ID映射路径，可能仅支持单文档</summary>
            if (objVer.Type != 0) {
                throw new Error('不是文档类型!');
            }
            var objVersion = mfVault.ObjectOperations.GetObjectInfo(objVer, true, false);
            if (objVersion.FilesCount === 0) {
                throw new Error('对象中没有文件!');
            }
            fileIndex = fileIndex || 1;
            var drive = u.getDriveLetter();
            var vaultName = mfVault.Name;
            var guid = this.getVaultGUID(mfVault);
            var idPath = this._getFilePathWithId(0, objVer.ID, objVersion.Title, objVersion.Files.Item(fileIndex).GetNameForFileSystem());
            return drive + ":\\" + vaultName + '\\ID2\\' + guid + '\\' + idPath;
        },

        addBaseConditions: function(searchConditions, objType, objClass, deleted) {
            var objTypeSC = u.createObject('SearchCondition');
            objTypeSC.ConditionType = MFConditionTypeEqual;
            objTypeSC.Expression.DataStatusValueType = MFStatusTypeObjectTypeID;
            objTypeSC.TypedValue.SetValue(MFDatatypeLookup, objType);
            searchConditions.Add(-1, objTypeSC);

            var classSC = u.createObject('SearchCondition');
            classSC.ConditionType = MFConditionTypeEqual;
            classSC.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
            classSC.TypedValue.SetValue(MFDatatypeLookup, objClass);
            searchConditions.Add(-1, classSC);

            var deletedSC = u.createObject('SearchCondition');
            deletedSC.ConditionType = MFConditionTypeEqual;
            deletedSC.Expression.DataStatusValueType = MFStatusTypeDeleted;
            deletedSC.TypedValue.SetValue(MFDatatypeBoolean, deleted);
            searchConditions.Add(-1, deletedSC);

        },

        getCurrentUserId: function(mfVault) {
            try {
                return mfVault.SessionInfo.UserID;
            } catch(e) {
                return mfVault.CurrentLoggedInUserID;
            }
        },

        getAccountName: function (mfVault) {
            var index = mfVault.SessionInfo.AccountName.indexOf('\\');
            return mfVault.SessionInfo.AccountName.substring(index+1);
        },

        getUserName : function(mfVault) {
            var userList = mfVault.UserOperations.GetUserList();
            var userId = this.getCurrentUserId(mfVault);
            try {
                for (var i = 1; i <= userList.Count; i++) {
                    if (userList.Item(i).Key === userId) {
                        return userList.Item(i).Name;
                    }
                }
            } catch (e) {
                
            }
            return this.getAccountName(mfVault);
        },

        getSearchView: function (mfVault, search) {

            var now = new Date();
            var year = now.getFullYear().toString();
            var month = now.getMonth().toString();
            var hour = now.getHours().toString();
            var minute = now.getMinutes().toString();
            var second = now.getSeconds().toString();

            var view = u.createObject('View');
            view.Name = "搜索：" + search + " - " + year + month + hour + minute + second;
            
            var sc = u.createObject('SearchCriteria');
            if (search) {
                sc.FullTextSearchString = search + '*';
            } else {
                sc.FullTextSearchString = '';
            }
            sc.FullTextSearchFlags = (MFFullTextSearchFlagsStemming
                | MFFullTextSearchFlagsLookInMetaData
                | MFFullTextSearchFlagsLookInFileData
                | MFFullTextSearchFlagsTypeAnyWord);
            sc.SearchFlags = MFSearchFlagNone;


            var expression = u.createObject('Expression');
            expression.DataStatusValueType = MFStatusTypeDeleted;
            expression.DataStatusValueDataFunction = MFDataFunctionNoOp;
            var tv = u.createObject('TypedValue');
            tv.SetValue(MFDatatypeBoolean, false);
            var deletedEx = u.createObject('SearchConditionEx');
            deletedEx.SearchCondition.Set(expression, MFConditionTypeEqual, tv);
            deletedEx.Enabled = true;
            sc.AdditionalConditions.Add(-1, deletedEx);
            var oViewNew1 = mfVault.ViewOperations.AddTemporarySearchView(view, sc, u.createObject('SearchConditions'));
            return mfVault.ViewOperations.GetViewLocationInClient(oViewNew1.ID);
        }
        
    };
    u.vault = vault;
}(MF));