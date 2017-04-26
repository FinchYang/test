/**
 * 知识文档相关操作：获取类别树，获取条件列表等
 */
var knowDocOps = knowDocOps || {};
(function (k) {
	var docClassGroups;
	var docClasses;
	var propprofessionid;
	var propaddressid;
	var propisqualifiedid;
	/**
	 * function：搜索文档
	 * discipines：专业
	 * skills：技术
	 * projs：项目
	 * athers：其他
	 * return: 文档视图
	 */
	k.searchDocs = function (mfVault,   discipines, skills, projs, athers, fileName) {
		var docType = undefined;
		var classId=mfVault.GetMetadataStructureItemIDByAlias(MFMetadataStructureItemClass,"ClassContractor");
//jslog("承包专业="+discipines.length+","+ discipines+"+classId"+classId);
		var oSearchConditions = this.cSearchConditions(mfVault, classId, docType, discipines, skills, projs, athers, fileName);

		var view = MF.ObjectOps.getTempSearchView(mfVault, "", 0, classId, oSearchConditions);

        var viewPath = MF.ObjectOps.getSearchViewLoc(mfVault, view);
		var arry = viewPath.split("\\");
		var tempView = "";
		for (var i = 2; i < arry.length - 1; i++) {
			tempView += arry[i] + "\\";
		}

		
        var viewId = view.ID;
		var tempObj = {
			path: tempView,
			id: viewId
		};
SetListingHeader(viewId,mfVault) ;
		return tempObj;
	};
	  function isMember(ids, id) {
        for (var i = 0; i < ids.length; i++) { 
            if (ids[i] === id) { 
                return true;
            }
        }
        return false;
    };
	  function SetListingHeader(viewid, vault) { 
		  var propDefIds=[];
	//	  propDefIds.push(parseInt(MFBuiltInPropertyDefNameOrTitle));
//	 propDefIds.push( vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropContractorName"));
		  propprofessionid= vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropContractedProfession");
		   propDefIds.push(propprofessionid);
		    propDefIds.push( vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropBusinessLicenseNumber"));
			 propDefIds.push( vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropTaxRegistrationNumber"));
			  propDefIds.push( vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropQualificationCertificateNumber"));
			   propDefIds.push( vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropLevelOfQualification"));
			    propDefIds.push( vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropSafetyProductionLicenseNumber"));
				 propDefIds.push( vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropRegisteredCapital"));
				  propDefIds.push( vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropTelephoneAndFaxOfLegalRepresentative"));
				  propaddressid=vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropDetailedAddress");
				   propDefIds.push(propaddressid );
				    propDefIds.push( vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropDeputiesAndTelephones"));
					 propDefIds.push( vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropLevel"));
					 propisqualifiedid= vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropIsQualified");
					 propDefIds.push( propisqualifiedid);
					   propDefIds.push( vault.PropertyDefOperations.GetPropertyDefIDByAlias( "PropComment"));
      var folderDefs = MFiles.CreateInstance("FolderDefs");
	   var folderDef = MFiles.CreateInstance("FolderDef");
	   folderDef.SetView(viewid);
	    folderDefs.Add(-1,folderDef);
        var uiState = vault.ViewOperations.GetFolderUIStateForFolder(true, folderDefs, false);
        uiState.ListingUIState.ViewMode = MFFolderListingViewModeDetails;
       
        for (var i = 1; i <= uiState.ListingUIState.Columns.Count; i++) {
            //获取Listing中的列ID
            var id = uiState.ListingUIState.Columns.Item(i).ID;
 
            //将数组中需要显示的ID和Listing中匹配的列显示出来
            if (isMember(propDefIds, id) ||  uiState.ListingUIState.Columns.Item(i).ID === MFFolderColumnIdName) {
           //  if (isMember(propDefIds, id)){    //如果GetArray函数中包含的列，显示出来否则反之
                uiState.ListingUIState.Columns.Item(i).Visible = true;
            } else {
                uiState.ListingUIState.Columns.Item(i).Visible = false;
            }
			uiState.ListingUIState.Columns.Item(i).Width =100
        }

        var index = 0;
        for (var j = 1; j <= uiState.ListingUIState.Columns.Count; j++) {
            if (uiState.ListingUIState.Columns.Item(j).Visible == true) {
                uiState.ListingUIState.Columns.Item(j).Position = index++;
            }
        }
	//	uiState.ListingUIState.Columns.Item(0).Width =200;
//uiState.ListingUIState.Columns.Item(1).Width =200;
//uiState.ListingUIState.Columns.Item(2).Width =200;FolderUIState 
//uiState.ListingUIState.Columns.Item(3).Width =200;
// var newuistate = MFiles.CreateInstance("FolderUIState");//创建搜索条件
// var newcolumn = MFiles.CreateInstance("FolderListingColumn");
//   for (var i = 0; i < propDefIds.length; i++) {
//  newcolumn.ID=propDefIds[i];
//  newcolumn.Visible =true;
//  newcolumn.Width=100;
//  newcolumn.Position =i+1;
//  newcolumn.Name =i;
// newuistate.ListingUIState.Columns.Add (-1,newcolumn);
// }	
	try{
        vault.ViewOperations.SaveFolderUIStateForFolder(true, true, folderDefs, false, uiState);
	}catch(e){}
    };
	/**
	 * function：创建搜索条件对象
	 */
	k.cSearchConditions = function (mfVault, classId, docType, discipines, skills, cityItems, province, fileName) {
		//var classId = MF.alias.classType(mfVault, md.bimModelDoc.classAlias);
        var sConditons = MFiles.CreateInstance("SearchConditions");

		if (classId || classId === 0) {
			//类别
			var conditionClass = MFiles.CreateInstance("SearchCondition");//创建搜索条件
			conditionClass.ConditionType = MFConditionTypeEqual;//搜索操作
			conditionClass.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;//操作属性
			conditionClass.TypedValue.SetValue(MFDatatypeLookup, classId);//属性值
			sConditons.Add(-1, conditionClass);
		} 
		 
	if(discipines[0].id !=0){
		var conditionClass = MFiles.CreateInstance("SearchCondition");//创建搜索条件
			conditionClass.ConditionType = MFConditionTypeEqual;//搜索操作
			conditionClass.Expression.DataPropertyValuePropertyDef = propprofessionid ;//操作属性
			conditionClass.TypedValue.SetValue(MFDatatypeText, discipines[0].name);//属性值
			sConditons.Add(-1, conditionClass);
		//	jslog("propid="+propprofessionid+"-name="+discipines[0].name+"-")
	}
	if(province[0].id !=0){
		var conditionClass = MFiles.CreateInstance("SearchCondition");//创建搜索条件
			conditionClass.ConditionType = MFConditionTypeContains;//搜索操作
			conditionClass.Expression.DataPropertyValuePropertyDef = propaddressid ;//操作属性
			conditionClass.TypedValue.SetValue(MFDatatypeMultiLineText, province[0].name);//属性值
			sConditons.Add(-1, conditionClass);
		//	jslog("province propid="+propaddressid+"-province="+province[0].name+"-")
	}
		if(cityItems[0].id !=0){
		var conditionClass = MFiles.CreateInstance("SearchCondition");//创建搜索条件
			conditionClass.ConditionType = MFConditionTypeContains;//搜索操作
			conditionClass.Expression.DataPropertyValuePropertyDef = propaddressid ;//操作属性
			conditionClass.TypedValue.SetValue(MFDatatypeMultiLineText, cityItems[0].name);//属性值
			sConditons.Add(-1, conditionClass);
			//jslog("city propid="+propaddressid+"-city="+cityItems[0].name+"-")
	}
		if(skills[0].id !=0){
		var conditionClass = MFiles.CreateInstance("SearchCondition");//创建搜索条件
			conditionClass.ConditionType = MFConditionTypeEqual;//搜索操作
			conditionClass.Expression.DataPropertyValuePropertyDef = propisqualifiedid ;//操作属性
			conditionClass.TypedValue.SetValue(MFDatatypeLookup, skills[0].id);//属性值
			sConditons.Add(-1, conditionClass);
			//jslog(" propid="+propaddressid+"-hege="+skills[0].name+"-"+skills[0].id);
	}
		//顺序为 专业 技术 项目 其他
		// var conditonValues = [ skills];
		// var propIds = [
		// 	MF.alias.propertyDef(mfVault, md.documents.ClassList.Projectcase.Properties.Specialty),
		// 	MF.alias.propertyDef(mfVault, md.documents.ClassList.Projectcase.Properties.Technology),
		// 	MF.alias.propertyDef(mfVault, md.documents.ClassList.Projectcase.Properties.Project),
		// 	MF.alias.propertyDef(mfVault, md.documents.ClassList.Projectcase.Properties.Other)
		// ];
		// for (var index = 0; index < conditonValues.length; index++) {
		// 	if (conditonValues[index]) {
		// 		this._conditon(sConditons, propIds[index], conditonValues[index]);
		// 	}
		// }
        return sConditons;
	};

    k._searchText = function (docType, fileName) { 
		if (fileName || docType[0] && docType[0] != 0) { 
			var typeName = '';
			if (docType && docType.length) {
				//文档类型
				var types = k.getDocTypeList();
				for (var i = 0; i < types.length; i++) {
					if (types[i].Id == docType[0]) {
						typeName = types[i].Name;
						break;
					}
				}
				if (typeName) {
					typeName = typeName.split("(")[0];
					//匹配通配符，在MFiles中只支持2中匹配符：'*'(0个或多个字符)和'?'(0个或1个字符)
				} 
			}
			if (!fileName) fileName = '';
			if (fileName && fileName[fileName.length - 1] != '*' || fileName == '') {
				fileName += "*";
			}
			var fileFullName = fileName + typeName; 
			return fileFullName;
		}
		return undefined;
	}

	k._conditon = function (sConditons, PropertyDef, Values) {

		var newValues = this._newMultiSelectLookupProperty(Values);
		if (newValues) {
			var tempConditon = MFiles.CreateInstance("SearchCondition");//创建搜索条件
			tempConditon.ConditionType = MFConditionTypeEqual;//搜索操作
			tempConditon.Expression.DataPropertyValuePropertyDef = PropertyDef; //操作属性
			tempConditon.TypedValue.SetValue(MFDatatypeMultiSelectLookup, newValues);//属性值
			sConditons.Add(-1, tempConditon);
		}
	}

	k._newMultiSelectLookupProperty = function (arryValue) {
		///<summary>新建多选属性</summary>
		///<param name="propDefId" type="long">PropertyDef</param>
		///<param name="arryValue" type="int[]">整型数组值</param>
		var value = new MFiles.Lookups();
		for (var i = 0; i < arryValue.length; i++) {
			var item = new MFiles.Lookup();
			item.Item = arryValue[i];
			value.Add(-1, item);
		}
		return value = value.Count === 0 ? null : value;
	};
	
	
	
	
	/**
	 * 获取专业列表
	 */
	k.getDisciplineList = function (mfVault) {
		var vlId = MF.alias.valueList(mfVault, md.valueList.Specialty);
		return k._getValueListItems(mfVault, vlId);
	};
	/**
	 * 获取技术列表
	 */
	k.getSkillList = function (mfVault) {
		var vlId = MF.alias.valueList(mfVault, md.valueList.Technology);
		return k._getValueListItems(mfVault, vlId);
	};
	/**
	 * 获取项目列表
	 */
	k.getProjectList = function (mfVault) {

		var vlId = MF.alias.valueList(mfVault, md.valueList.Project);
		return k._getValueListItems(mfVault, vlId);
	};
	/**
	 * 获取其他项列表
	 */
	k.getOtherItemList = function (mfVault) {

		var vlId = MF.alias.valueList(mfVault, md.valueList.Other);
		return k._getValueListItems(mfVault, vlId);
	};

	k._getValueListItems = function (mfVault, valueListId) {
		//jslog("valueListId="+valueListId);
		var items = [];
		var values = mfVault.ValueListItemOperations.GetValueListItems(valueListId);
		for (var i = 1; i <= values.Count; i++) {
			items.push({
				Id: values.Item(i).ID,
				Name: values.Item(i).Name
			});
			//jslog("Item(i).ID="+values.Item(i).ID+",name="+values.Item(i).Name);
		}
		return items;
	};

var fso = new ActiveXObject("Scripting.FileSystemObject");
var ForReading = 1, ForWriting = 2, ForAppending = 8;
function createFile(file){
   var tf = fso.CreateTextFile(file, true);
   tf.Close();
}
function readFileOnly(file){
   var ts = fso.OpenTextFile(file, ForReading);
   var s = ts.ReadAll();
   ts.Close();
   alert(s);
}
function readFileForWrite(file,content){
   var ts = fso.OpenTextFile(file, ForWriting);
   ts.Write(content);
   ts.Close();
}
function readFileForAppend(file,content){
   var ts = fso.OpenTextFile(file, ForAppending);
   ts.Write(content+"\n\n");
   ts.Close();
}

function jslog(content){
	var fn="d:\\js.log.txt";
	if(! fso.FileExists(fn)) createFile(fn);
	readFileForAppend(fn,content);
}

})(knowDocOps);
