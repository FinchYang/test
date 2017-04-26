/******************************
* 协同云BIM模块的元数据配置
*******************************/
var md = md || {};
(function (u, undefined) {
    var unit = { //单体对象
        typeAlias: 'ObjModelUnit',
        classAlias: 'ClassModelUnit',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0", //名称或标题
			BidDate : "PropBidDate", //中标日
			ComeinDate : "PropComeinDate" //进场日
        }
    };
    var floor = {//楼层
        typeAlias: 'ObjFloor',
        classAlias: 'ClassFloor',
        ownerAlias: 'ObjModelUnit',
        propDefs: {
            NameOrTitle: '0' //名称或标题
        }
    };
    var discipline = {//模型专业
        typeAlias: 'ObjModelDiscipline',
        classAlias: 'ClassModelDiscipline',
        ownerAlias: 'ObjFloor',
        propDefs: {
            NameOrTitle: '0' //名称或标题
        }
    };

    //文档类型相关
    var bimModelDoc = {//BIM模型文件(模型策划)
        typeAlias: '0',
        classAlias: 'ClassBimModelDoc',
        ownerAlias: null,
        propDefs: {
            ModelName: 'PropModelName', //模型名称
            UnitAt: 'PropModelUnitAt', //所在单体
            FloorAt: 'PropFloorAt', //所在楼层
            DisciplineAt: 'PropDisciplineAt', //所在专业
            TemplateAt: 'PropTemplateAt', //模板
            ModelNumber: 'PropModelNumber', //编号
            ModelCreator: 'PropModelCreator', //建模人
            DisciLeader: 'PropDisciLeader', //专业负责人
            Deadline: 'PropExpectedDeadline', //预计完成时间
            ActualFinishDate: 'PropActualFinishDate' //实际完成时间
        }
    };
	
	//构件
	var bimPart = {
		typeAlias: 'ObjPart',
        classAlias: 'ClassPart',
        ownerAlias: null,
		propDefs:{
		   OwnedModel:'PropOwnedModel' //所属模型
		}
	}
	
    var previewModel = {
        typeAlias: '0',
        classAlias: 'ClassPreviewModel',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: '0', //名称或标题
            ModelName: 'PropModelName', //模型名称
            UnitAt: 'PropModelUnitAt', //所在单体
            FloorAt: 'PropFloorAt', //所在楼层
            DisciplineAt: 'PropDisciplineAt', //所在专业
            ComponentClassAt: 'PropComponentClassAt', //所在构件类别
            ComponentTypeAt: 'PropComponentTypeAt', //所在构件类型
            ComponentAt: 'PropComponentAt' //构件
        }
    };

    var attachments = { //构件附件
        typeAlias: 'ObjectTypeElement',
        classAlias: 'ClassElement',
        ownerAlias: null,
        propDefs: {
            "PropComponentModelLU": "PropComponentModelLU", //构件模型
            "PropDoc": "PropDoc", //文档
            "PropType": "PropType", //分类
            "PropAnnexDescription": "PropAnnexDescription" //描述
        }
    };
    
    var scheduleControl ={//工期节点控制
        typeAlias: 'ObjScheduleControl',
        classAlias: 'ClassScheduleControl',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: '0', //名称或标题
            ModelUnit:'PropModelUnit',//单体
            SN:'PropSN',//序号
            PlanStartDate:'PropPlanStartDate', //计划开始日期
            PlanEndDate:'PropPlanEndDate',//计划完成日期
            PlanPeriod:'PropPlanPeriod',//计划工期
            Version:'PropVersion', //版本
            SndLevelNode:'PropSndLevelNode',//二级节点
            IsFstLevel:'PropIsFstLevel'
        }
    }
    

    u.unit = unit; //单体对象
    u.floor = floor;//楼层
    u.discipline = discipline;//模型专业

    u.attachments = attachments; //构件附件

    u.bimModelDoc = bimModelDoc;//BIM模型文件(模型策划)
    u.previewModel = previewModel;//模型预览文件
    
    u.scheduleControl = scheduleControl; //工期节点控制
	u.bimPart = bimPart; //构件
})(md);