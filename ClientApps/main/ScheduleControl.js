var model = model || {};
//获取单体列表
model.GetSingleBodyList = function(shellFrame){
	//搜索单体
	var vault = shellFrame.ShellUI.Vault;	
	var typeId = MF.alias.objectType(vault,md.unit.typeAlias);
	var classId = MF.alias.classType(vault,md.unit.classAlias);	
	var scs = MFiles.CreateInstance('SearchConditions');
	MF.vault.addBaseConditions(scs, typeId, classId, false);
	var res = vault.ObjectSearchOperations.SearchForObjectsByConditions(scs, MFSearchFlagNone, false);
	for(var i =0;i< res.Count;i++){
	   var objId =	res[i].ObjVer.ID;
	   var title = res[i].Title;
	   if(i == 0){
		   var option = '<option selected="selected" style="width:200px;" value="' + objId + '">' + title + '</option>'
		   $('#sBodySelect').append(option);
	   }else{
		   var option = '<option style="width:200px;" value="' + objId + '">' + title + '</option>'
		   $('#sBodySelect').append(option);
	   } 	   
	}
	//初始化表			
    if($('#sBodySelect option:selected').text() != ""){   
	   var name = $('#sBodySelect option:selected').text() + "工期模块节点计划";
	   $('#name').val(name);			
	   model.GetBidAndComeinDateFromSB(shellFrame);		   
	   model.loadTable(shellFrame);
	}
};
//创建或更新对象
model.createObj = function(shellFrame,version){	
	var vault = shellFrame.ShellUI.Vault;
	var sBodyObjId = $('#sBodySelect option:selected').val();
	var table = document.getElementById("mainTable");	
	var subNodes = []; //临时存放子节点的数组	
	for(var i =table.rows.length-1;i>= 0;i--){		
		var tr = table.rows[i];
		if(tr.getAttribute("class") == 'hiddenTr') continue; //隐藏的删除节点
		var sn = tr.cells[1].innerHTML; //序号
		var name = tr.cells[2].innerHTML; //名称
		var startTd = tr.cells[3].innerHTML;
		var startDate = "";//开始日期
		if(startTd.indexOf("<input") >= 0){ //开始日期为输入框
			startDate = tr.cells[3].children[0].value;
		}
		else{//开始日期为固定值
			startDate = startTd;
		}
		var periodTd = tr.cells[4].innerHTML;
		var period = "" //工期
		if(periodTd.indexOf("<input") >=0){//输入框
			var day = tr.cells[4].children[0].value;     
			if(IsNum(day)){
				period = day;				
			}
			}else if(periodTd.indexOf("<select") >=0)//选择列表
			{
				var index = tr.cells[4].children[0].selectedIndex ;  
				period = tr.cells[4].children[0].options[index].text;
			}
			else{//固定值
				period = periodTd;
			}
		var endDate = tr.cells[5].innerHTML; //完成日期
		var pvs = model.GetPvs(vault,sBodyObjId,sn,name,startDate,period,endDate,version,subNodes); //获取属性
		var oldObjId = tr.cells[0].getAttribute("title"); 
		if(oldObjId){//更新对象
			var typeId = MF.alias.objectType(vault,md.scheduleControl.typeAlias);
			var ObjID = MFiles.CreateInstance('ObjID');
			ObjID.SetIDs(typeId,oldObjId);
			var objVer = vault.ObjectOperations.GetLatestObjVer(ObjID,false,false);
			var objectVersion = vault.ObjectOperations.GetObjectInfo(objVer,true,false);
			var objversion = MF.ObjectOps.updateObject(vault, objectVersion, pvs);
			if(sn.indexOf("_")>=0){//二级节点
				subNodes.push(objversion.ObjVer.ID);
			}else{
				subNodes = []; //清空
			}
		}else{//创建对象
			var typeId = MF.alias.objectType(vault,md.scheduleControl.typeAlias);
			var classId = MF.alias.classType(vault,md.scheduleControl.classAlias);		
			var objversion = MF.ObjectOps.createObject(vault, typeId, classId,pvs,null);
			if(sn.indexOf("_")>=0){//二级节点
				subNodes.push(objversion.ObjVer.ID);
			}else{
				subNodes = [];//清空
			}
		}		
	}
	//更新单体对象中的中标日和进场日
	var bidDate = $('#bidDate').val();
	var comeinDate = $('#comeinDate').val();
	
	var sBodyObjId = $('#sBodySelect option:selected').val();   
    var bidDatePropId = MF.alias.propertyDef(vault,md.unit.propDefs.BidDate);
    var comeinDatePropId = MF.alias.propertyDef(vault,md.unit.propDefs.ComeinDate);
    var sbType = MF.alias.objectType(vault,md.unit.typeAlias); 
    var sbObjID = MFiles.CreateInstance('ObjID');
    sbObjID.SetIDs(sbType,sBodyObjId);
    var sbObjVer = vault.ObjectOperations.GetLatestObjVer(sbObjID,false,false);
	var objectVersion = vault.ObjectOperations.GetObjectInfo(sbObjVer,true,false);
	
	var pvs = MFiles.CreateInstance('PropertyValues');
	var pvBidDate = MF.property.newDateProperty(bidDatePropId,NewDate(bidDate));
	pvs.Add(-1,pvBidDate);
	var pvCominDate = MF.property.newDateProperty(comeinDatePropId,NewDate(comeinDate));
	pvs.Add(-1,pvCominDate);
	MF.ObjectOps.updateObject(vault, objectVersion, pvs);	
};
//生成节点propertyValues
model.GetPvs = function(vault,sBodyObjId,sn,name,startDate,period,endDate,version,subNodes){
	var pvs = MFiles.CreateInstance('PropertyValues');
	//单体
	var unitPropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.ModelUnit);
	var unitPv = MF.property.newLookupProperty(unitPropId,sBodyObjId);
	pvs.Add(-1,unitPv);
	//序号
	var snPropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.SN);
	var snPv = MF.property.newTextProperty(snPropId,sn);
	pvs.Add(-1,snPv);
	//名称
	var namePropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.NameOrTitle);
	var namePv = MF.property.newTextProperty(namePropId,StandFormat(sn) + "-" + name);
	pvs.Add(-1,namePv);
	//开始日期
	if(startDate != ''){
		var startDatePropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.PlanStartDate);
		var startDatePv = MF.property.newDateProperty(startDatePropId,NewDate(startDate));
		pvs.Add(-1,startDatePv);
	}	
	//完成日期
	if(endDate != ''){
		var endDatePropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.PlanEndDate);
		var endDatePv = MF.property.newDateProperty(endDatePropId,NewDate(endDate));
		pvs.Add(-1,endDatePv);
	}
	//工期
	if(period !='')
	{
		var periodPropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.PlanPeriod);
		var periodPv = MF.property.newIntegerProperty(periodPropId,period);
		pvs.Add(-1,periodPv);
	}
	//版本
	var versionPropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.Version);
	var versionPv = MF.property.newTextProperty(versionPropId,version);
	pvs.Add(-1,versionPv);
	//子节点
	if(sn.indexOf('_')<0){//一级节点
		if(subNodes.length >0){
			var scnLevelPropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.SndLevelNode);
			// var lookups = MFiles.CreateInstance('Lookups');
			// for(var i =0;i<subNodes.length;i++){
			// 	var lookup = MFiles.CreateInstance('Lookup');
			// 	lookup.Item  = subNodes[i];
			// 	lookups.Add(-1,lookup);
			// }
			subNodesPv = MF.property.newMultiSelectLookupProperty(scnLevelPropId,subNodes);
			pvs.Add(-1,subNodesPv);
			subNodes = [];			
		}
	}
	//是否是一级节点 
	if(sn.indexOf('_')<0){
		var isFstLevelPropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.IsFstLevel);
		var isFstLevelPv = MF.property.newBooleanProperty(isFstLevelPropId,true);
		pvs.Add(-1,isFstLevelPv);
	}
	return pvs;
};
//搜索单体的节点计划
model.SearchForPvsInVault = function(vault){
	//创建对象
	var typeId = MF.alias.objectType(vault,md.scheduleControl.typeAlias);
	var classId = MF.alias.classType(vault,md.scheduleControl.classAlias);	
	var scs = MFiles.CreateInstance('SearchConditions');
	MF.vault.addBaseConditions(scs, typeId, classId, false);
	var sc = MFiles.CreateInstance('SearchCondition'); //单体搜索
	var unitPropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.ModelUnit);
	var unitObjId = $('#sBodySelect option:selected').val();
	sc.ConditionType = MFConditionTypeEqual;
	sc.Expression.DataPropertyValuePropertyDef = unitPropId;
	sc.TypedValue.SetValue(MFDatatypeLookup, unitObjId);
	scs.Add(-1,sc);
	var res = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx (scs, MFSearchFlagNone, false,0,0);
	var objArr = [];
	for(var i =0;i<res.Count;i++){
		var objVersion = res[i];
		var objVer = objVersion.ObjVer;
		var pvs = vault.ObjectPropertyOperations.GetProperties(objVer,false);
		var snPropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.SN); //序号
		var sn = pvs.SearchForProperty(snPropId).GetValueAsLocalizedText();
		var startDatePropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.PlanStartDate);//开始时间
		var startDate = pvs.SearchForProperty(startDatePropId).GetValueAsLocalizedText();
		var endDatePropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.PlanEndDate);//结束事件
		var endDate = pvs.SearchForProperty(endDatePropId).GetValueAsLocalizedText();
		var periodPropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.PlanPeriod);//工期
		var period = pvs.SearchForProperty(periodPropId).GetValueAsLocalizedText();
		var versionPropId = MF.alias.propertyDef(vault,md.scheduleControl.propDefs.Version);//版本
		var version = pvs.SearchForProperty(versionPropId).GetValueAsLocalizedText();
		objArr.push({sn:sn,startDate:ChangeFormat(startDate),endDate:ChangeFormat(endDate),period:period,objId:objVer.ID,version:version});		
	}	
	return objArr;	
};
//在数组中查找给定sn相同的对象
model.SearchObj = function(scObjsPvsArr,sn){
	for(var i=0;i<scObjsPvsArr.length;i++){
		if(scObjsPvsArr[i].sn == sn){
			return scObjsPvsArr[i];
		}
	}
	return null;
}
//删除对象
model.DeleteObj = function(vault,typeAlias,objId){
	if(!IsNum(objId)) return;
	var typeId = MF.alias.objectType(vault,md.scheduleControl.typeAlias);
	MF.ObjectOps.DeleteObject(vault, typeId, objId);	
}
//获取单体的进场日和中标日
model.GetBidAndComeinDateFromSB = function(shellFrame){
   var vault = shellFrame.ShellUI.Vault;
   var sBodyObjId = $('#sBodySelect option:selected').val();   
   var bidDatePropId = MF.alias.propertyDef(vault,md.unit.propDefs.BidDate);
   var comeinDatePropId = MF.alias.propertyDef(vault,md.unit.propDefs.ComeinDate);
   var sbType = MF.alias.objectType(vault,md.unit.typeAlias); 
   var sbObjID = MFiles.CreateInstance('ObjID');
   sbObjID.SetIDs(sbType,sBodyObjId);
   var sbObjVer = vault.ObjectOperations.GetLatestObjVer(sbObjID,false,false);
   var pvs = vault.ObjectPropertyOperations.GetProperties(sbObjVer,false);
   var bidDate = pvs.SearchForProperty(bidDatePropId).GetValueAsLocalizedText();
   var comeinDate = pvs.SearchForProperty(comeinDatePropId).GetValueAsLocalizedText();
   $('#bidDate').val(ChangeFormat(bidDate));
   $('#comeinDate').val(ChangeFormat(comeinDate));	
}
//载入主表
model.loadTable = function(shellFrame){
	var vault = shellFrame.ShellUI.Vault;
	$('#version').val("无");
	//删除主表工期节点元素
	$("#mainTable tr").remove();
	//清空删除项列表
	$('#AddItem').children().remove();
	//单项和中标日 进场日都选择
	if($('#sBodySelect option').size() != 0 && $('#bidDate').val() !="" && $('#comeinDate').val() !="" ){		
		var scObjsPvsArr = model.SearchForPvsInVault(vault); //库中的对象		
		var bidDate = $('#bidDate').val();
		var comeinDate = $('#comeinDate').val();
		
		//搜索单体的工期控制节点对象	
		//var select7T10 = "<select class='daySelect'><option value='7'>7</option><option value='8' selected='true'>8</option><option value='9'>9</option><option value='10'>10</option></select>";		
		//var dayInput = "<input class='dayInput' type='text'/>";
		var nodeInfo = [];
		nodeInfo.push({sn:'1',name:'项目班子组建',startDate:bidDate,startNotice:'',period:'5',periodNotice:'',endDate:addDate(bidDate,4)});
		nodeInfo.push({sn:'2',name:'项目施工管理策划',startDate:bidDate,startNotice:'',period:'15',periodNotice:'',endDate:addDate(bidDate,14)});
		nodeInfo.push({sn:'2_1',name:'总体部署策划',startDate:bidDate,startNotice:'',period:'7',periodNotice:'',endDate:addDate(bidDate,6)});
		nodeInfo.push({sn:'2_2',name:'平面及临建设施策划',startDate:bidDate,startNotice:'',period:'10',periodNotice:'',endDate:addDate(bidDate,9)});
		nodeInfo.push({sn:'2_3',name:'工期节重要点计划策划',startDate:bidDate,startNotice:'',period:'10',periodNotice:'',endDate:addDate(bidDate,9)});
		nodeInfo.push({sn:'3',name:'机电安装施工管理策划',startDate:bidDate,startNotice:'',period:'15',periodNotice:'',endDate:addDate(bidDate,14)});
		nodeInfo.push({sn:'3_1',name:'临水、临电策划',startDate:'',startNotice:'项目班子组建后',period:'7',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'4',name:'图纸的深化设计（土建、机电）',startDate:bidDate,startNotice:'',period:'60',periodNotice:'',endDate:addDate(bidDate,59)});
		nodeInfo.push({sn:'5',name:'项目部组建',startDate:bidDate,startNotice:'',period:'15',periodNotice:'',endDate:addDate(bidDate,14)});
		nodeInfo.push({sn:'6',name:'施工管理实施计划（可分阶段）',startDate:bidDate,startNotice:'',period:'15',periodNotice:'',endDate:addDate(bidDate,14)});
		nodeInfo.push({sn:'7',name:'现场临时设施（四通一平、五区一路）',startDate:comeinDate,startNotice:'',period:'60',periodNotice:'',endDate:addDate(comeinDate,59)});
		nodeInfo.push({sn:'8',name:'三通一平（水电接入、内外主路、场平）',startDate:comeinDate,startNotice:'',period:'30',periodNotice:'',endDate:addDate(comeinDate,29)});
		nodeInfo.push({sn:'9',name:'围墙、大门、临时办公（集装箱）、临时卫生间',startDate:comeinDate,startNotice:'',period:'20',periodNotice:'',endDate:addDate(comeinDate,19)});
		nodeInfo.push({sn:'10',name:'临水系统、临电系统、网络系统、新能源系统',startDate:comeinDate,startNotice:'',period:'',periodNotice:'进场日起60天',endDate:''});
		nodeInfo.push({sn:'10_1',name:'临水系统、临电系统、网络系统、新能源系统',startDate:comeinDate,startNotice:'',period:'',periodNotice:'进场日起60天',endDate:''});
		nodeInfo.push({sn:'10_2',name:'临水系统——临时生活、施工及消防给水系统、储水系统、增压系统、喷淋系统、排水系统、水处理系统施工',startDate:comeinDate,startNotice:'',period:'',periodNotice:'进场日起60天',endDate:''});
		nodeInfo.push({sn:'10_3',name:'临电系统——配电系统、接地系统、高低压线路系统、照明系统（标配LED）',startDate:comeinDate,startNotice:'',period:'',periodNotice:'进场日起60天',endDate:''});
		nodeInfo.push({sn:'10_4',name:'网络系统——网络办公、监控系统、门禁及考勤系统、安防系统、广播系统',startDate:'',startNotice:'安装作业面具备',period:'',periodNotice:'进场日起60天',endDate:''});
		nodeInfo.push({sn:'10_5',name:'新能源系统——太阳能、空气能、光伏、新能源机械车辆',startDate:'',startNotice:'安装作业面具备',period:'',periodNotice:'进场日起60天',endDate:''});
		nodeInfo.push({sn:'10_6',name:'其他智慧工地系统应用',startDate:'',startNotice:'现场具备条件',period:'',periodNotice:'进场日起80天',endDate:''});
		nodeInfo.push({sn:'11',name:'其余规划正式临建、道路、各功能区设施、安全通道、水电设施、网通、门禁、监控、绿色施工设施',startDate:comeinDate,startNotice:'',period:'60',periodNotice:'',endDate:addDate(comeinDate,59)});
		nodeInfo.push({sn:'12',name:'大型垂直运输机械设备的招标、采购',startDate:addDate(comeinDate,19),startNotice:'',period:'',periodNotice:'主体结构施工具备条件结束',endDate:''});
		nodeInfo.push({sn:'12_1',name:'塔吊、砼输送泵',startDate:addDate(comeinDate,19),startNotice:'',period:'30',periodNotice:'',endDate:addDate(comeinDate,49)});
		nodeInfo.push({sn:'12_2',name:'施工电梯',startDate:"",startNotice:'主体结构具备条件开始',period:'',periodNotice:'主体结构具备安装条件结束',endDate:''});
		nodeInfo.push({sn:'13',name:'基坑工程（土方、桩基、支护、降水）',startDate:"",startNotice:'进场日（具备作业条件）',period:'<select class="daySelect"><option title="若选择有桩基">180</option><option title="若选择无桩基">90</option></select>',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'14',name:'塔吊安装、验收',startDate:'',startNotice:'',period:CreateSelect(7,10),periodNotice:'7-10天内（起重作业前）',endDate:''});
		nodeInfo.push({sn:'14_1',name:'底板（分段）',startDate:'',startNotice:'',period:'',periodNotice:'基坑工程结束后7~20天（分段）',endDate:''});
		nodeInfo.push({sn:'15',name:'地下管线综合排布策划（管井、公共空间）',startDate:'',startNotice:'底板施工前（结构确认），图纸确认',period:'',periodNotice:'分段正负零封闭结束30天内完成',endDate:''});
		nodeInfo.push({sn:'15_1',name:'地下安装工程设备选型、招标、订货及排产',startDate:'',startNotice:'',period:'',periodNotice:'按地下设备安装节点，提前（设备排产周期+运输周期）通知并确认排产',endDate:''});
		nodeInfo.push({sn:'16',name:'内、外装饰深化设计',startDate:addDate(comeinDate,59),startNotice:'',period:'',periodNotice:'正负零封闭结束30天内完成',endDate:''});
		nodeInfo.push({sn:'16_1',name:'内装机房装饰大样图',startDate:addDate(comeinDate,59),startNotice:'',period:'',periodNotice:'正负零封闭结束30天内完成',endDate:''});
		nodeInfo.push({sn:'16_2',name:'内装卫生间墙、地面排版图',startDate:addDate(comeinDate,59),startNotice:'',period:'',periodNotice:'正负零封闭结束45天内完成',endDate:''});
		nodeInfo.push({sn:'16_3',name:'内装墙面镶贴排版图',startDate:addDate(comeinDate,59),startNotice:'',period:'',periodNotice:'正负零封闭结束45天内完成',endDate:''});
		nodeInfo.push({sn:'16_4',name:'内装吊顶排版图',startDate:addDate(comeinDate,59),startNotice:'',period:'',periodNotice:'正负零封闭结束45天内完成',endDate:''});
		nodeInfo.push({sn:'16_5',name:'外装幕墙龙骨及面层图',startDate:addDate(comeinDate,59),startNotice:'',period:'',periodNotice:'正负零封闭结束50天内完成',endDate:''});
		nodeInfo.push({sn:'17',name:'正负零封闭（分段）',startDate:'',startNotice:'',period:'',periodNotice:'底板结束后30天内（分段）（暂按地下2层，如大于2层，每层增加为10天）',endDate:''});
		nodeInfo.push({sn:'18',name:'地上管线综合排布策划',startDate:'',startNotice:'主体施工前（结构确认），图纸确认',period:'',periodNotice:'30天，第一分段主体结构结束前/地上10层前完成',endDate:''});
		nodeInfo.push({sn:'18_1',name:'地上及屋面安装工程设备选型、招标、订货及排产',startDate:'',startNotice:'',period:'',periodNotice:'按地上设备安装节点，提前（设备排产周期+运输周期）通知并确认排产',endDate:''});
		nodeInfo.push({sn:'18_2',name:'地下室外墙、肥槽清理、防护及验收',startDate:'',startNotice:'',period:'',periodNotice:'地下室外墙施工结束后15天',endDate:''});
		nodeInfo.push({sn:'19',name:'地下室外墙外防水及外回填（车辆坡道、施工电梯地基优先）',startDate:'',startNotice:'地下室外墙清理验收后',period:'',periodNotice:'正负零结构完成45天内完成/地上十层前结束',endDate:''});
		nodeInfo.push({sn:'20',name:'地库正负零板防水及保护',startDate:'',startNotice:'正负零板结构完成且监督验收后开始',period:'',periodNotice:'7天（分段）',endDate:''});
		nodeInfo.push({sn:'21',name:'基础分部验收（分段）',startDate:'',startNotice:'',period:'',periodNotice:'基础结构（分段）施工完成后60天（含砌体）/30天（不含砌体）',endDate:''});
		nodeInfo.push({sn:'21_1',name:'地下室内回填',startDate:'',startNotice:'基础验收完成后',period:'30',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'22',name:'地下机电主管线安装',startDate:'',startNotice:'土建地下室结构验收后/土建移交作业面后',period:'90',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'22_1',name:'地下设备基础',startDate:'',startNotice:'基础验收结束一个月内开始',period:'15',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'22_2',name:'地下设备安装',startDate:'',startNotice:'机房作业面条件具备/设备进场/机房设备安装样板验收',period:'',periodNotice:'设备进场后30天完成',endDate:''});
		nodeInfo.push({sn:'23',name:'主体裙房及标准层',startDate:'',startNotice:'',period:'',periodNotice:'普通层每层5~7天，钢筋层每层7~10天',endDate:''});
		nodeInfo.push({sn:'23_1',name:'地下室雨污水系统完成',startDate:'',startNotice:'土建地下室结构验收后/土建移交作业面后',period:'90',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'23_2',name:'楼层清理验收 （拆模、清理、修补、检验标识、安全文明及临设、验收）',startDate:'',startNotice:'满足拆模时间开始',period:'',	          periodNotice:'每层5~7天，与上步结构作业层数差不大于5层，且执行F5标准即：F5~F3层作业及模架层（冬季、大跨公建等增加模架层按方案执行）、F2层拆模层、F1层清理验收层。',endDate:''});
		nodeInfo.push({sn:'23_3',name:'楼层机电安装工程线盒清理、防腐，管路疏通、保护，预留孔洞清理、复核，接地埋件清理、保护',startDate:'',startNotice:'与土建同步',period:'',periodNotice:'与土建同步',endDate:''});
		nodeInfo.push({sn:'24',name:'施工电梯投入使用',startDate:'',startNotice:'地上主体6~8层时进场安装',period:'',periodNotice:'最迟至主体10层时完成，保持顶段挑架底层同步到达',endDate:''});
		nodeInfo.push({sn:'24_1',name:'地下室通风系统的完成',startDate:'',startNotice:'土建地下室结构验收后/土建移交作业面后',period:'90',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'25',name:'重要实体样板',startDate:'',startNotice:'',period:'',periodNotice:'重要分项工程、工序正式展开施工前完成并验收',endDate:''});
		nodeInfo.push({sn:'25_1',name:'基础及结构分项首次工序样板（桩、垫层、防水、钢筋、模板、砼）',startDate:'',startNotice:'分项首次工序作业开始',period:'',periodNotice:'分项工序展开施工前完成并验收',endDate:''});
		nodeInfo.push({sn:'25_2',name:'机电安装工程配套工序样板施工及验收（预留预埋）',startDate:'',startNotice:'土建作业面交接',period:'',periodNotice:'与土建样板穿插同步完成',endDate:''});
		nodeInfo.push({sn:'25_3',name:'重要单项样板（含安装）（墙砌体、湿作业、门窗）',startDate:'',startNotice:'主体第一分段验收（具备条件可提前开始）/最迟样板分项工程开始施工前一个月开始',period:'',periodNotice:'7天~15天，按计划/分项工序展开施工前完成/不影响工序正式展开',endDate:''});
		nodeInfo.push({sn:'25_4',name:'机电安装工程重要单项样板（水平及竖向风管、桥架、管道，小型设备单机安装）',startDate:'',startNotice:'土建作业面交接',period:'',periodNotice:'5~10天，按计划',endDate:''});
		nodeInfo.push({sn:'25_5',name:'内装饰样板（卫生间、墙地面等）',startDate:'',startNotice:'土建、安装作业面交接',period:'',periodNotice:'5~10天，按计划',endDate:''});
		nodeInfo.push({sn:'26',name:'综合样板层/户（间）施工及验收（含安装、装饰）',startDate:'',startNotice:'第一段主体分段验收结束（具备条件应提前开始）',period:'',periodNotice:'15天~30天，按计划',endDate:''});
		nodeInfo.push({sn:'26_1',name:'配套装饰样板施工及验收',startDate:'',startNotice:'按土建样板施工计划',period:'',periodNotice:'与土建进度同步完成',endDate:''});
		nodeInfo.push({sn:'26_2',name:'配套机电安装工程样板施工及验收',startDate:'',startNotice:'按土建样板施工计划',period:'',periodNotice:'与土建进度同步完成',endDate:''});
		nodeInfo.push({sn:'26_3',name:'管井、电井、设备间样板（同样板层）机电安装配套',startDate:'',startNotice:'跟随土建样板层穿插施工',period:'',periodNotice:'与土建装饰同步完成',endDate:''});			
		nodeInfo.push({sn:'27',name:'砌体工程',startDate:'',startNotice:'主体施工最迟至十层前开始穿插',period:'',periodNotice:'主体封顶后30天结束',endDate:''});
		nodeInfo.push({sn:'27_1',name:'围护墙体（砌体、轻墙板）单项样板施工及验收（含安装）',startDate:'',startNotice:'地上主体至六层时开始穿插',period:'',periodNotice:'15天第一分段主体结构完成前完成/地上十层主体结构结束前完成',endDate:''});
		nodeInfo.push({sn:'27_2',name:'地下砌体',startDate:'',startNotice:'砌体样板验收后',period:'30',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'27_3',name:'地上砌体',startDate:'',startNotice:'砌体样板验收后/主体施工至十层后开始穿插',period:'',periodNotice:'主体封顶后30天结束',endDate:''});
		nodeInfo.push({sn:'28',name:'主体分段验收（20层以下可整体，超20层主体按照10~15层分段）',startDate:'',startNotice:'主体分段内部清理、验收完成',period:'',periodNotice:'分段主体结构完成后30天（含砌体）/20天（不含砌体）',endDate:''});
		nodeInfo.push({sn:'28_1',name:'地上分段机电安装竖向主管线安装（从下至上）',startDate:'',startNotice:'土建移交管井、电井作业面后/管井、电井样板验收后/分段主体验收后/',period:CreateSelect(15,30),periodNotice:'',endDate:''});		
		nodeInfo.push({sn:'28_2',name:'地上分段机电安装水平主管线安装（从下至上）',startDate:'',startNotice:'机电样板验收后/分段主体验收后',period:CreateSelect(15,60),periodNotice:'',endDate:''});
		nodeInfo.push({sn:'28_3',name:'地上分段机电安装支管线及设备安装',startDate:'',startNotice:'分段主管线安装结束/湿作业开始前/',period:'',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'28_4',name:'单竖向卫生间排水系统分段完成',startDate:'',startNotice:'机电样板验收后/分段主体验收后',period:CreateSelect(15,60),periodNotice:'',endDate:''});
		nodeInfo.push({sn:'28_5',name:'主体首段外架（落地或悬挑架、爬架）首层样板',startDate:'',startNotice:'按计划开始，跟随作业面进度',period:'',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'28_6',name:'分段外架清理、拆除、撤场',startDate:'',startNotice:'按计划，分段主体外墙砌体完成/外墙湿作业完成/外装龙骨安装完成',period:'7',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'28_7',name:'屋面设备基础、出屋面风道、通风井',startDate:'',startNotice:'主体封顶后',period:'15',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'28_8',name:'竖向雨水系统完成（室内）',startDate:'',startNotice:'机电样板验收后/分段主体验收后',period:CreateSelect(15,60),periodNotice:'',endDate:''});
		nodeInfo.push({sn:'28_9',name:'竖向雨水系统完成（外墙）',startDate:'',startNotice:'外墙具备安装条件开始',period:CreateSelect(15,30),periodNotice:'',endDate:''});
		nodeInfo.push({sn:'29',name:'屋面防水及保护层',startDate:'',startNotice:'主体顶部分段验收后（含砌体）/出屋面机电安装分项完成/屋面板设备基础、出屋面风道、通风井完成',period:'',periodNotice:'30天/结构封顶60天内（完成防水及保护）',endDate:''});		
		nodeInfo.push({sn:'30',name:'外装（分段，悬挑外架、爬架',startDate:'',startNotice:'分段主体验收后开始/作业分段顶部水平封闭防护完成',period:'',periodNotice:'分段作业面移交后90天~120天内（主体封顶验收同）',endDate:''});
		nodeInfo.push({sn:'30_1',name:'分段水平悬挑封闭硬防护平台',startDate:'',startNotice:'与主体分段规划位置挑架施工同步',period:'',periodNotice:'7天下段外装（吊篮）施工前',endDate:''});
		nodeInfo.push({sn:'30_2',name:'主体分段外悬挑架拆除（由下至上）（爬架顶段拆除）',startDate:'',startNotice:'分段主体外墙砌体完成/外墙湿作业完成',period:'7',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'30_3',name:'主体分段外装吊篮安装',startDate:'',startNotice:'作业分段顶部水平封闭防护完成/安装作业面清理完成/屋面防水及保护完成',period:'',periodNotice:'3天~5天（进场组装提前）',endDate:''});
		nodeInfo.push({sn:'31',name:'塔吊拆除',startDate:'',startNotice:'外架拆除完成/屋面设备吊装完成/',period:'3',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'32',name:'幕墙施工（分段）',startDate:'',startNotice:'主体分段验收完成后开始',period:'90',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'32_1',name:'幕墙龙骨及保温施工（分段）',startDate:'',startNotice:'主体分段验收完成后开始',period:'45',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'32_2',name:'幕墙外立面面层施工（分段）',startDate:'',startNotice:'幕墙龙骨及保温验收完成后开始',period:'45',periodNotice:'',endDate:''});		
		nodeInfo.push({sn:'33',name:'内装湿作业（分段）',startDate:'',startNotice:'单项样板验收完成（首段开始）/分段主体验收完成（分段开始）/安装穿线完成/机电安装主管线安装及打压完成（管井、电井除外，先湿作业后安装）',period:CreateSelect(20,40),periodNotice:'',endDate:''});
		nodeInfo.push({sn:'33_1',name:'内装墙面面层施工（分段）',startDate:'',startNotice:'内装湿作业结束（分段）',period:CreateSelect(30,60),periodNotice:'',endDate:''});
		nodeInfo.push({sn:'33_2',name:'内装吊顶龙骨及面层施工（分段）',startDate:'',startNotice:'安装管线施工完成',period:CreateSelect(30,60),periodNotice:'',endDate:''});
		nodeInfo.push({sn:'34',name:'安装管道系统打压',startDate:'',startNotice:'分段、分层、分区、分系统完成后',period:'',periodNotice:'与内装饰湿作业同步结束',endDate:''});
		nodeInfo.push({sn:'35',name:'门窗框及门窗安装',startDate:'',startNotice:'分段内、外湿作业门窗洞口收口前（塞口）',period:'',periodNotice:'10天~15天跟随楼层湿作业进度穿插',endDate:''});
		nodeInfo.push({sn:'35_1',name:'机电安装外墙金属门窗、幕墙龙骨防雷接地',startDate:'',startNotice:'与外墙施工同步',period:'',periodNotice:'与外墙施工同步',endDate:''});
		nodeInfo.push({sn:'35_2',name:'分段管井、电井、机房砌体、墙地土建装饰施工，移交安装',startDate:'',startNotice:'相应基础、主体分段验收后/该部位综合样板验收后开始',period:'',periodNotice:'15天~30天（砌体、抹灰、刮白、地面结束后，按计划分段完成移交安装）',endDate:''});
		nodeInfo.push({sn:'35_3',name:'分段机电安装电气穿线',startDate:'',startNotice:'内装湿作业结束后',period:'',periodNotice:'门窗安装结束前',endDate:''});
		nodeInfo.push({sn:'35_4',name:'分段门窗扇安装',startDate:'',startNotice:'最后一道装饰面层施工前',period:'',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'35_5',name:'分段管井、电井、机房门安装移交',startDate:'',startNotice:'分段管井、电井、机房主管线、设备安装结束，电缆安装及穿线前移交',period:'',periodNotice:'分段随层进行门安装封闭',endDate:''});
		nodeInfo.push({sn:'35_6',name:'分段精装施工作业面移交',startDate:'',startNotice:'分段湿作业完成后移交',period:'',periodNotice:'',endDate:''});		
		nodeInfo.push({sn:'35_7',name:'分段管井、电井、机房安装',startDate:'',startNotice:'分段管井、电井、机房土建移交后开始/成品保护到位后开始',period:CreateSelect(7,15),periodNotice:'',endDate:''});		
		nodeInfo.push({sn:'35_8',name:'地下主机房、高低压配电室施工和移交',startDate:'',startNotice:'相应基础分段验收后/砌体、抹灰、腻子、防水、地面样板结束后开始机房土建施工',period:'',periodNotice:'30天，机房、高低压配电室砌体、抹灰、涂料腻子、地面垫层结束后移交安装',endDate:''});	
		nodeInfo.push({sn:'35_9',name:'机电安装工程移交精装',startDate:'',startNotice:'装饰基层龙骨施工前，安装打压完成，线、缆敷设完成后移交',period:'',periodNotice:'',endDate:''});	
		nodeInfo.push({sn:'35_10',name:'机房内设备、高低压配电设备进场及安装',startDate:'',startNotice:'机房及高低压配电室移交/设备按时排产交货',period:'',periodNotice:'90天（主工作量）',endDate:''});
		nodeInfo.push({sn:'36',name:'电梯机房施工和移交',startDate:'',startNotice:'顶段主体验收后/砌体、抹灰、腻子、防水、地面样板结束后开始机房土建施工',period:'',periodNotice:'10天机房砌体、抹灰、腻子、防水、地面垫层结束后移交电梯及设备安装',endDate:''});
		nodeInfo.push({sn:'36_1',name:'电梯机房机电安装配合（电气安装、临电接入）',startDate:'',startNotice:'与土建配合',period:'',periodNotice:'与土建配合',endDate:''});	
		nodeInfo.push({sn:'36_2',name:'电梯井道验收和移交',startDate:'',startNotice:'井道（坑）拆模后（砌体墙体完成后）逐段开始（利用支模平台和架体逐段清理验收），门洞防护随结构安全防护同步，完成安全门、挡水台，成品电梯门安装后施工四周临时封堵',period:'',periodNotice:'电梯进场后移交',endDate:''});		
		nodeInfo.push({sn:'37',name:'电梯设备进场及安装',startDate:'',startNotice:'电梯机房、井道移交/湿作业完成前一个月/塔吊拆除前/施工电梯拆除前一个月',period:'',periodNotice:'20天（主工作量）',endDate:''});	
		nodeInfo.push({sn:'37_1',name:'屋面外露设备安装作业面移交',startDate:'',startNotice:'屋面防水及保护层结束',period:'',periodNotice:'',endDate:''});	
		nodeInfo.push({sn:'37_2',name:'屋面外露机电设备进场安装',startDate:'',startNotice:'屋面作业面移交/设备按时排产交货',period:'',periodNotice:'15天~30天（主工作量），与土建做好穿插移交',endDate:''});			
		nodeInfo.push({sn:'38',name:'施工电梯拆除',startDate:'',startNotice:'屋面主工程量结束/主要湿作业结束/门窗运输结束/室内安装主管线、主设备完成/正式电梯投入使用后',period:'',periodNotice:'5天最迟竣工日前三个月完成',endDate:''});	
		nodeInfo.push({sn:'38_1',name:'拉链收口',startDate:'',startNotice:'施工电梯、塔吊拆除后（拆除前提前做好分段封闭安排穿插施工）',period:CreateSelect(15,30),periodNotice:'',endDate:''});		
		nodeInfo.push({sn:'38_2',name:'窗扇封闭',startDate:'',startNotice:'分段架体拆除后/分段外装结束/分段室内湿作业完成/分段精装开始前',period:'',periodNotice:'10天（分段），按计划分层分段封闭，跟随移交作业面',endDate:''});		
		nodeInfo.push({sn:'38_3',name:'门扇安装',startDate:'',startNotice:'分段交房面层或精装完成后/收尾管理第二阶段销项完成',period:'',periodNotice:'10天（分段），按计划',endDate:''});		
		nodeInfo.push({sn:'38_4',name:'末端设备安装（灯具、开关、插座、配电箱柜、洁具、喷淋头、风口、智能化等）',startDate:'',startNotice:'分段交房面层或精装完成后/门安装完毕',period:'',periodNotice:'门窗安装完成后90天',endDate:''});
		nodeInfo.push({sn:'39',name:'正式电梯（消防梯）使用',startDate:'',startNotice:'安装调试、检测验收程序完成/维保措施到位并验收',period:'',periodNotice:'',endDate:''});			
		nodeInfo.push({sn:'40',name:'室外工程',startDate:'',startNotice:'室外场地临设分区分段清场完成后开始',period:'90',periodNotice:'',endDate:''});		
		nodeInfo.push({sn:'40_1',name:'室外工程场地移交（分区分段）',startDate:'',startNotice:'外架拆除完成/吊篮拆除完成/工程引出室外结构及管线设施完成/场地临设分区分段清场完成',period:'',periodNotice:'',endDate:''});		
		nodeInfo.push({sn:'40_2',name:'室外机电安装工程',startDate:'',startNotice:'室外场地分区分段移交后开始',period:'90',periodNotice:'',endDate:''});			
		nodeInfo.push({sn:'41',name:'收尾管理',startDate:'',startNotice:'竣工日前3个月开始（三阶段：未完工程销项、关门销项、清理验收移交）',period:'90',periodNotice:'',endDate:''});		
		nodeInfo.push({sn:'41_1',name:'机电安装竣工收尾',startDate:'',startNotice:'竣工节点前三个月前开始（三阶段：未完工程销项、关门收尾、清理移交）',period:'90',periodNotice:'',endDate:''});	
		nodeInfo.push({sn:'41_2',name:'机电单机试运装及系统调试',startDate:'',startNotice:'竣工节点前四个月',period:'120',periodNotice:'',endDate:''});	
		nodeInfo.push({sn:'42',name:'正式水、电、燃气工程',startDate:'',startNotice:'竣工前6个月开始',period:'90',periodNotice:'',endDate:''});	
		nodeInfo.push({sn:'43',name:'景观移交',startDate:'',startNotice:'室外工程、外网完成（分区分段）',period:'',periodNotice:'按计划（分区分段）',endDate:''});
		nodeInfo.push({sn:'44',name:'机电安装系统调试',startDate:'',startNotice:'收尾阶段穿插进行',period:'',periodNotice:'竣工收尾三个月内按既定计划完成',endDate:''});
		nodeInfo.push({sn:'45',name:'竣工验收前土建、机电专项检测、验收',startDate:'',startNotice:'收尾阶段穿插进行',period:'',periodNotice:'竣工收尾三个月内按既定计划完成',endDate:''});
		nodeInfo.push({sn:'45_1',name:'环境检测',startDate:'',startNotice:'门窗安装结束/室内施工收尾结束（室内环境稳定）',period:'3',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'45_2',name:'人防验收',startDate:'',startNotice:'人防工程完工、资料报验完成，竣工前报验',period:'3',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'45_3',name:'节能验收',startDate:'',startNotice:'节能分部工程完工，资料完备',period:'3',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'45_4',name:'电气检测',startDate:'',startNotice:'正式电送电后',period:'20',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'45_5',name:'消防检测验收',startDate:'',startNotice:'电气检测后20天',period:'20',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'45_6',name:'防雷检测',startDate:'',startNotice:'竣工节点前一个月',period:'5',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'45_7',name:'智能化检测',startDate:'',startNotice:'机电系统调试运行后一个月',period:'30',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'45_8',name:'水质卫生检测（给排水、水处理）',startDate:'',startNotice:'竣工节点验收前一个月（需正式水运行）',period:'10',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'45_9',name:'设计工艺检测验收（医疗、体育、艺术等）',startDate:'',startNotice:'所需工程完工/检测条件具备/按计划',period:'',periodNotice:'7天~15天',endDate:''});
		nodeInfo.push({sn:'45_10',name:'规划验收',startDate:'',startNotice:'竣工前业主报请规划部门验收',period:'',periodNotice:'3天',endDate:''});
		nodeInfo.push({sn:'46',name:'竣工资料报监督验收',startDate:'',startNotice:'竣工节点前90天开始资料汇总，前30天报监督部门验收',period:'7',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'46_1',name:'竣工资料报监督验收',startDate:'',startNotice:'竣工节点前90天开始资料汇总，前30天报监督部门验收',period:'7',periodNotice:'',endDate:''});		
		nodeInfo.push({sn:'46_2',name:'档案验收',startDate:'',startNotice:'竣工资料监督验收通过报业主档案移交城建档案验收（具体程序执行地方规定）',period:'7',periodNotice:'',endDate:''});
		nodeInfo.push({sn:'47',name:'竣工验收（五方验收、质量监督）',startDate:'',startNotice:'竣工清理完成/专项检测验收完成/竣工资料报验完成/工程建设、监督手续完成/竣工报监手续完成',period:CreateSelect(10,20),periodNotice:'',endDate:''});	
		nodeInfo.push({sn:'47_1',name:'工程竣工验收前移交',startDate:'',startNotice:'收尾管理第三阶段完成，分区分段移交（交钥匙）业主、使用单位或物业单位',period:'',periodNotice:'',endDate:''});	
		nodeInfo.push({sn:'47_2',name:'工程移交机电安装配合维保交接',startDate:'',startNotice:'分区分段移交（交钥匙）业主、使用单位或物业单位',period:'',periodNotice:'',endDate:''});	
		nodeInfo.push({sn:'48',name:'工程竣工验收后移交',startDate:'',startNotice:'分区分段四方竣工验收完成/竣工验收完成分段移交（交钥匙）业主、使用单位或物业单位',period:'',periodNotice:'',endDate:''});
					
		for(var i =0;i<nodeInfo.length;i++){
			var oneInfo = nodeInfo[i];
			var tr = '';
			tr +="<tr id='" + oneInfo.sn + "'>";
			tr +="<td width='10%'><img class = 'delete' src = 'ico/删除.ico'/></td>" //删除列
			tr +="<td width='10%'>" + oneInfo.sn + "</td>"  //序号列    
			tr +="<td width='35%'>" + oneInfo.name + "</td>" //节点名称
			if(oneInfo.startDate ==''){//计划开始时间
				tr += "<td width='15%'><input type='text' title = '" + oneInfo.startNotice +  "' class='datePicker' readonly='readonly'/></td>"; //日期控件
			}else{
				tr +="<td width='15%'>" + oneInfo.startDate + "</td>" 
			}
			if(oneInfo.period ==""){ //工期
				tr += "<td width='15%'><input class='dayInput' type='text' title='" + oneInfo.periodNotice + " '/></td>";
			}else{
				tr +="<td width='15%'>" + oneInfo.period + "</td>";
			}
			
			tr +="<td width='15%'>" + oneInfo.endDate + "</td>";//计划完成时间
			tr +="</tr>";			
			$("#mainTable").append(tr);
			//使用对象中的值更新tr
			if(scObjsPvsArr.length != 0){
				var scObj = model.SearchObj(scObjsPvsArr,oneInfo.sn);
				if(scObj){
					var startTd = $("#mainTable").find("tr:last").find("td:eq(3)").html();
					if(startTd.indexOf("<input")>=0){//更新开始时间 仅更新输入框中的值 
						$("#mainTable").find("tr:last").find("td:eq(3)").find("input:eq(0)").val(scObj.startDate);
					}
					//更新工期
					var dayTd = $("#mainTable").find("tr:last").find("td:eq(4)").html();					
					if(dayTd.indexOf("<input") >=0){//输入框
						$("#mainTable").find("tr:last").find("td:eq(4)").find("input:eq(0)").val(scObj.period);
					}
					if(dayTd.indexOf("<select") >=0){//下拉列表
						$("#mainTable").find("tr:last").find("td:eq(4)").find("select:eq(0)").find("option[value='" + scObj.period + "']").attr("selected",true);										
					}
					//更新结束时间				
					var endTd = $("#mainTable").find("tr:last").find("td:eq(5)").html();
					if(endTd == ""){
						$("#mainTable").find("tr:last").find("td:eq(5)").text(scObj.endDate);					
					}
					$("#mainTable").find("tr:last").find("td:eq(0)").attr("title",scObj.objId);
					//版本
					$('#version').val(scObj.version);
				}else{//没有对象 隐藏tr 并写入增加列表
					$("#mainTable").find("tr:last").addClass("hiddenTr");
					//增加到‘添加项’列表
					var value = $("#mainTable").find("tr:last").attr("id");
					var title = $("#mainTable").find("tr:last").find("td:eq(1)").html() + " " + $("#mainTable").find("tr:last").find("td:eq(2)").html();
					var option = '<option style="width:200px;" value="' + value + '">' + title + '</option>';
					$('#AddItem').append(option);
				}
			}
		}
		
	}
	//开始时间选择后事件
	$(".datePicker").change(function(){		
		var startDate = this.value;
		var dayTd = $(this).parent().next().html();
		if(dayTd.indexOf("<input") >=0){//输入框
			var day = $(this).parent().next().children(":first").val();
			if(IsNum(day)){
				$(this).parent().next().next().text(addDate(startDate,day-1));
			}
		}else if(dayTd.indexOf("<select") >=0)//选择列表
		{
			var day = $(this).parent().next().find('option:selected').text();
			var selectedDate = $(this).val();
			$(this).parent().next().next().text(addDate(selectedDate,day-1));
		}
		else{//固定值
			var day = dayTd;	
			$(this).parent().next().next().text(addDate(startDate,day-1));
		}
	});
	//工期输入框改变事件
	$('.dayInput').change(function(){
		var day = $(this).val();
		if(IsNum(day)){
			var startDate = $(this).parent().prev().html();
			if(startDate.indexOf("<input") >=0){//输入框
				startDate = $(this).parent().prev().children(":first").val();
				if(IsDate(startDate)){
					$(this).parent().next().text(addDate(startDate,day-1));
				}			
			}else{//固定值
				$(this).parent().next().text(addDate(startDate,day-1));
			}	
		}	
	});
	//工期列表选项改变
	$('.daySelect').change(function(){
		var day = $(this).find('option:selected').text();
		var startDate = startDate = $(this).parent().prev().children(":first").val();
		if(IsDate(startDate)){
			$(this).parent().next().text(addDate(startDate,day-1));
		}			
		//alert(day);
	});
	//表中日期空间载入
	$('.datePicker').datepicker({	
		inline: true,
		changeMonth: true,
		changeYear:true}); 
	//载入删除事件
	$('.delete').click(function(){
		var vault = shellFrame.ShellUI.Vault;
	    //添加class 隐藏tr
	    $(this).parent().parent().addClass("hiddenTr");
		//删除对象
		var objId = $(this).parent().attr("title");		
		model.DeleteObj(vault,"ObjScheduleControl",objId);
		//删除td上的对象id
		$(this).parent().removeAttr('title');
		//增加‘添加项’列表
		var value =  $(this).parent().parent().attr("id");
		var title = $(this).parent().next().text() + " " + $(this).parent().next().next().text();
		var snClick =  $(this).parent().next().text();
		var option = '<option style="width:200px;" value="' + value + '">' + title + '</option>';
		$('#AddItem').append(option);
		//增加子节点到‘添加项’列表
		var table = document.getElementById("mainTable");		
		for(var i =1;i<= table.rows.length-1;i++){
			var tr = table.rows[i];
			var sn = tr.cells[1].innerHTML;
			if(sn.indexOf(snClick + "_") != -1){ //发现有子节点
				if(tr.getAttribute("class") == 'hiddenTr') continue;
				value = tr.id;
				title = tr.cells[1].innerHTML + " " + tr.cells[2].innerHTML;
				option = '<option style="width:200px;" value="' + value + '">' + title + '</option>';
				$('#AddItem').append(option);
				tr.setAttribute('class','hiddenTr');
				//删除对象
				objId = tr.cells[0].getAttribute('title');
				model.DeleteObj(vault,"ObjScheduleControl",objId);
				//删除td上的对象id
				tr.cells[0].removeAttribute("title");			
			}
		}		
	});	
};

//日期计算
function addDate(date,days){	
	var d=new Date(date); 
	d.setDate(d.getDate()+days);
	var year = d.getFullYear();	 		
	var m=d.getMonth()+1; 
	var mStr = m.toString();
	if(mStr.length == 1){
		mStr="0" + mStr
	} 
	var day  =d.getDate();
	var dayStr = day.toString();
	if(dayStr.length ==1){
		dayStr = "0" + dayStr
	}
	
	return year+'-'+ mStr +'-'+dayStr;
} 

//是否是整数
function IsNum(day){
	var re = /^[0-9]*[1-9][0-9]*$/ ; 
	if(re.test(day)){
		return true;
	} else{
		return false;
	}
}

//是否为日期
function IsDate(date){
	var re=/^(\d{4})-(\d{2})-(\d{2})$/;
	if(re.test(date)){
		return true;
	}else{
		return false;
	}
}

//创建日期对象
function NewDate(dateStr){
	var myDate=new Date()
	var dateArr = dateStr.split('-');
	myDate.setFullYear(dateArr[0],dateArr[1]-1,dateArr[2])
	return myDate;
}

//改变日期字符串格式 如2016/11/3 改为2016-11-3
function ChangeFormat(str){
	//return str.replace(/\//g,"-");
	if(str == "") return "";
	var arr = str.split('/');
	var year = arr[0];;
	var month = arr[1];
	if(month.length == 1){
		month = "0" + month;
	}
	var day = arr[2];
	if(day.length == 1){
		day = "0" + day;
	}
	return year + "-" + month + "-" + day;
}

//创建下拉框
function CreateSelect(start,end){
	var select = "<select class='daySelect'>";
	for(var i =start;i<=end;i++){
		select +="<option value='" + i + "'>" + i + "</option>";
	}
	select += "</select>";
	return select;
}

//序号格式标准化
function StandFormat(str){
	var arr = str.split("_");
	if(arr.length == 1){
		if(arr[0].length ==1){
			return "0" + arr[0] + "_00";
		}else{
			return arr[0] + "_00";
		}
	}
	else{
		if(arr[0].length ==1){
			arr[0] = "0" + arr[0];
		}
		if(arr[1].length ==1){
			arr[1] = "0" + arr[1];
		}
		return arr[0] + "_" + arr[1];
	}
}

