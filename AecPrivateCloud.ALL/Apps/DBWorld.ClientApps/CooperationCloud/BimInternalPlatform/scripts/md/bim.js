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
            NameOrTitle: "0" //名称或标题
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
    var componentClasses = {//构件类别
        typeAlias: 'ObjComponentClasses',
        classAlias: 'ClassComponentClasses',
        ownerAlias: 'ObjModelDiscipline',
        propDefs: {
            NameOrTitle: '0', //名称或标题
            ID: 'PropID'
        }
    };
    var componentType = {//构件类型
        typeAlias: 'ObjComponentType',
        classAlias: 'ClassComponentType',
        ownerAlias: 'ObjComponentClasses',
        propDefs: {
            NameOrTitle: '0', //名称或标题
            ID: 'PropID'
        }
    };
    var componentModel = {//构件模型
        typeAlias: 'ObjComponentModel',
        classAlias: 'ClassComponentModel',
        ownerAlias: 'ObjComponentType',
        propDefs: {
            NameOrTitle: '0', //名称或标题
            ID: 'PropID',
            GUID: 'PropGUID'
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
    }

    u.unit = unit; //单体对象
    u.floor = floor;//楼层
    u.discipline = discipline;//模型专业
    u.componentClasses = componentClasses;//构件类别
    u.componentType = componentType;//构件类型
    u.componentModel = componentModel;//构件模型

    u.attachments = attachments; //构件附件

    u.bimModelDoc = bimModelDoc;//BIM模型文件(模型策划)
    u.previewModel = previewModel;//模型预览文件
})(md);