/******************************
* 协同云基础的元数据配置
*******************************/
var md = md || {};
(function (u, undefined) {

    var valueList = {//值列表
        DrawSendPhase: "VLDrawSendPhase",//发图阶段
        Discipline: "VLDiscipline",//专业
        Identity: "VLIdentity",//参与角色
        ProjPhase: "VLProjPhase",//项目状态(阶段)
        DifficultyLevel: "VLDifficultyLevel",//任务难度等级
        MailFolders: "VLMailFolders",//邮件文件夹
        CrowdSrcTaskState: "VLCrowdSrcTaskState",//众包任务状态
        ProjDocFolder: "VLProjDocFolder"//工作文件夹
    };
    var userGroups = {//用户组
         Outsource: "UGroupOutsource",//外包成员
        ZmCost: "UGroupZmCost",//中民成本部
        ZmDesign: "UGroupZmDesign",//中民设计部
        ZmEngineering: "UGroupZmEngineering",//中民工程部
        ProjCreator: "UGroupProjCreator",//项目创建者
        PM: "UGroupPM",//项目经理
        EMD: "UgEngineeringManagementDepartment",
        SG: "UgSecurityGuard",
        SPMD: "UgSafetyProductionManagementDepartment",
        GM: "UgGangmaster",
        CC: "UgConstructionCrew",
        CE: "UgChiefEngineer",
        VicePM: "UGroupVicePM",//项目副理
        Member: "UGroupMember",//一般成员
        HEMD:"UgHeadquartersEngineeringManagementDepartment" //公司总部工程管理部
    };

    var proj = { //项目对象
        typeAlias: 'ObjProject',
        classAlias: 'ClassProject',
        ownerAlias: null,
        propDefs: {
            ProjName: "PropProjName", //项目名称
            ProjNum: "PropProjNum", //项目编号
            ProjPhase: "PropProjPhase", //项目状态
            Description: "PropDescription", //项目描述
            StartDate: "PropStartDate", //开始日期
            Deadline: "42", //截止日期
            ProjProgress: "PropProjProgress", //项目总体进度
            ProprietorUnit: "PropProprietorUnit", //建设单位
            DesignUnit: "PropDesignUnit", //设计单位
            BuilderUnit: "PropBuilderUnit", //施工单位
            SupervisorUnit: "PropSupervisorUnit" //监理单位
        }
    };
    var participant = {
        typeAlias: 'ObjParticipant',
        classAlias: 'ClassParticipant',
        ownerAlias: null,
        propDefs: {
            Name: "PropParticipantName",//参与方名称
            ProjPM: "PropProjPM",//项目经理
            ProjVicePMs: "PropProjVicePMs",//项目副理
            Members: "PropMembers"//一般成员
        }
    };
    var genericTask = {//任务(内建的)
        typeAlias: '10',
        classAlias: '-100',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
             Participant: "PropIdParticipant", //参与方(多参与方版本)
            TaskTitle: "PropTaskTitle", //任务标题
            TaskNumber: "PropTaskNum", //任务编号
            Description: "41", //任务说明
            StartDate: "PropStartDate", //开始日期
            Deadline: "42", //截止日期
            AssignedTo: "44", //分配给
            MonitoredBy: "43", //监控人
            CompletedBy: "45", //被标记完成
            DifficultyLevel: "PropDifficultyLevel", //任务难度等级
            JobTime: "PropJobTime", //预计工时(h)
            ProgressPercentage: "PropProgressPercentage", //完成百分比(%)
            DescriptDoc: "PropTaskDescriptDoc", //任务说明文档
            FruitDoc: "PropTaskFruitDoc", //任务成果文档
            OtherBudget: "PropOtherBudget" //其他预算(￥)
        }
    };
    var noticeTask = {//通知(任务)
        typeAlias: '10',
        classAlias: 'ClassNotification',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            Description: "41", //任务说明
            Deadline: "42", //截止日期
            BidInvitingDeadline: "PropBidInvitingDeadline", //招标截止日期
            AssignedTo: "44", //分配给
            MonitoredBy: "43", //监控人
            CompletedBy: "45", //被标记完成
            AssignToGroup: "PropAssignToGroup" //分配给用户组
        }
    };

    var sharedDoc = {//共享文档
        typeAlias: '0',
        classAlias: 'ClassSharedDoc',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            ShareTo: "PropShareTo",//共享给(他方)，多参与方版本
            ParticipantAt: "PropIdParticipantAt",//所在参与方，多参与方版本
            ProjDocFolder: "PropProjDocFolder",//所在文件夹, 单参与方版本
            Class1Mark: "PropClass1Mark",//1级分类
            Class2Mark: "PropClass2Mark",//2级分类
            Class3Mark: "PropClass3Mark"//3级分类
        }
    };
    var currentPartDoc = {//工作文档（多参与方版本） or 个人文档
        typeAlias: '0',
        classAlias: 'ClassCurrentPartDoc',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            ParticipantAt: "PropIdParticipantAt",//所在参与方，多参与方版本
            ProjDocFolder: "PropProjDocFolder",//所在文件夹, 单参与方版本
            Class1Mark: "PropClass1Mark",//1级分类
            Class2Mark: "PropClass2Mark",//2级分类
            Class3Mark: "PropClass3Mark"//3级分类
        }
    };

    var contacts = {//项目成员(联系人)
        typeAlias: 'ObjContacts',
        classAlias: 'ClassContacts',
        ownerAlias: 'ObjParticipant',
        propDefs: {
            PropLinkmanName: "PropLinkmanName", //姓名
            PropAccount: "PropAccount", //登录账户
            PropProjectRole: "PropProjectRole", //项目角色
            PropTelPhone: "PropTelPhone", //电话
            PropEmail: "PropEmail", //邮箱
            PropQQ: "PropQQ", //QQ
            PropUnit: "PropUnit", //单位
            PropDepartment: "PropDepartment", //部门
            PropPosition: "PropPosition", //职位
            PropUserStatus: "PropUserStatus", //禁用？
            PropIsProjCreator: "PropIsProjCreator", //项目创建者
            PropDailyCost: "PropDailyCost", //费率(￥/天)
            PropRemarks: "PropRemarks" //备注
        }
    };
    var multiChecker = {//多级审核人
        typeAlias: 'ObjMultiChecker',
        classAlias: 'ClassMultiChecker',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            PropCheck1: "PropCheck1",//一级审核
            PropCheck2: "PropCheck2",//二级级审核
            PropCheck3: "PropCheck3",//三级级审核
            PropCheck4: "PropCheck4",//四级审核
            PropCheck5: "PropCheck5",//五级审核
            PropCheck6: "PropCheck6",//六级审核
            PropCheck7: "PropCheck7",//七级审核
            PropCheck8: "PropCheck8",//八级审核
            PropCheck9: "PropCheck9",//九级审核
            PropCheck10: "PropCheck10"//十级审核
        }
    }
    var multiCheckFlowDoc = {
        typeAlias: '0',
        classAlias: null,
        ownerAlias: null,
        propDefs: {
            PropMultiChecker: "PropMultiChecker" //多级审核
        }
    }
    var drawingList = {
        typeAlias: 'ObjDrawingList',
        classAlias: 'ClassDrawingList',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            PropMultiChecker: "PropMultiChecker",//多级审核
            //PropDrawSendPhase: "PropDrawSendPhase"//发图阶段
            PropCcGroup: "PropCcGroup",//抄送单位
            PropReceiveGroup: "PropReceiveGroup",//收件单位
            PropDrawings: "PropDrawings"//图纸
        }
    }

    u.drawingList = drawingList;
    u.multiChecker = multiChecker;
    u.multiCheckFlowDoc = multiCheckFlowDoc;

    u.valueList = valueList;//值列表
    u.userGroups = userGroups;//用户组
    u.proj = proj;//项目对象
    u.participant = participant;//参与方
    u.contacts = contacts;//项目成员(联系人)

    u.genericTask = genericTask;//任务(内建的)
    u.noticeTask = noticeTask;//通知(任务)

    u.sharedDoc = sharedDoc;//共享文档
    u.currentPartDoc = currentPartDoc;//工作文档 or 个人文档
})(md);