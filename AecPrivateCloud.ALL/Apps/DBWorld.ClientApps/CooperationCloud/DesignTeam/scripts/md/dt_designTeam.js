/******************************
* 协同云设计团队模块的新增元数据配置
*******************************/
var md = md || {};
(function (u, undefined) {
    var projectPlan = {//项目策划
        typeAlias: 'ObjProjPlan',
        classAlias: 'ClassProjPlan',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            DesignPhase: "PropDesignPhase",//设计阶段
            ProjManager: "PropProjManager",//项目经理
            ChiefDesigner: "PropChiefDesigner",//设总
            Disciplines: "PropPlanDisciDefault"//策划专业组
        }
    };
    var planDiscipline = {//专业组
        typeAlias: 'ObjPlanDiscipline',
        classAlias: 'ClassPlanDiscipline',
        ownerAlias: null,
        propDefs: {
            Title: "PropDisciTitle", //专业名称
            DisciplineLead: "PropDisciplineLead", //专业负责人
            DrawingPerson: "PropDrawingPerson", //绘图人
            Designer: "PropDesigner", //设计人
            Verifier: "PropVerifier", //校对人
            Checker: "PropChecker", //审核人
            Validator: "PropValidator" //审定人
        }
    };
    var drawingPlan = {//设计策划or图纸策划
        typeAlias: 'ObjDrawingPlan',
        classAlias: 'ClassDrawingPlan',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            DesignPhase: "PropDesignPhaseAuto",//阶段
            Discipline: "PropPlanDisciplineAuto",//专业
            DrawingTitle: "PropDrawingTitle",//图名
            DrawingNumber: "PropDrawingNumber",//图号
            DrawingScale: "PropDrawingScale",//比例
            FrameSize: "PropFrameSize",//图幅
            DrawingPerson: "PropDrawingPerson",//绘图人 
            Designer: "PropDesigner",//设计人
            Verifier: "PropVerifier",//校对人
            Checker: "PropChecker",//审核人
            Validator: "PropValidator",//审定人
            DisciplineLead: "PropDisciplineLead",//专业负责人
            ChiefDesigner: "PropChiefDesigner",//设总
            ProjManager: "PropProjManager",//项目经理
            Deadline: "42",//截止日期
            JobTime: "PropJobTime",//预计工时
            PlanProgress: "PropProgressStatus",//进度
            PlanVersion: "PropPlanVersion"//版本
        }
    };
    var sharingLog = {//提资记录
        typeAlias: 'ObjSharingLog',
        classAlias: 'ClassSharingLog',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            DesignPhase: "PropDesignPhase",//设计阶段
            Discipline: "PropPlanDiscipline",//策划专业
            SharedBy: "PropSharedBy",//提资人
            SharingTo: "PropSharingTo",//提资给
            SharedDocs: "PropSharedDocs",//提资文档
            SharingDate: "PropSharingDate"//提资日期
        }
    };

    var reviewLog = {//校审意见
        typeAlias: 'ObjReviews',
        classAlias: 'ClassReviews',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            FruitDrawing: "PropFruitDrawing",//成果图纸
            DrawSheetVersion: "PropDrawSheetVersion",//图纸版本
            NotPass: "PropApproved",//确定修改或异议
            ReviewState: "PropReviewState"//校审流程状态
        }
    };

    var designDoc = {//设计文档
        typeAlias: '0',
        classAlias: 'ClassDesignDoc',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            Discipline: "PropPlanDiscipline",//策划专业
            DocStatus: "PropDocStatus"//文档状态
        }
    };
    var sharingDoc = {//提资文档
        typeAlias: '0',
        classAlias: 'ClassSharingDoc',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            Discipline: "PropPlanDiscipline",//策划专业
            DocStatus: "PropDocStatus",//文档状态
            SharingTo: "PropSharingTo"//提资给
        }
    };
    var manageDoc = {
        typeAlias: '0',
        classAlias: 'ClassManageDoc',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            ManageType: "PropManageDocType",//管理文档类型
            DocStatus: "PropDocStatus"//文档状态
        }
    };
    var drawingSheet = {//成果图纸
        typeAlias: '0',
        classAlias: 'ClassDrawingSheet',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            DrawingPlan: "PropDrawingPlan",//设计策划(单选)
            DesignPhase: "PropDesignPhase",//设计阶段
            Discipline: "PropPlanDiscipline",//策划专业
            DrawingTitle: "PropDrawingTitle",//图名
            DrawingNumber: "PropDrawingNumber",//图号
            DrawingScale: "PropDrawingScale",//比例
            FrameSize: "PropFrameSize",//图幅
            DrawingPerson: "PropDrawingPerson",//绘图人
            Designer: "PropDesigner",//设计人
            Verifier: "PropVerifier",//校对人
            Checker: "PropChecker",//审核人
            Validator: "PropValidator",//审定人
            DisciplineLead: "PropDisciplineLead",//专业负责人
            ChiefDesigner: "PropChiefDesigner",//设总
            ProjManager: "PropProjManager",//项目经理
            DocStatus: "PropDocStatus"//文档状态
        }
    };

    var reviewFlow = {//图纸校审流程
        alias: "WFSigns",
        stateAlias: {
            Designer: "WFSDesigner", //设计人
            Verifier: "WFSVerifier", //校对人
            VerifierJudge: "WFSVerifierJudge", //校对判定
            DisciplineLead: "WFSDisciplineLead", //专业负责人
            DisciLeadJudge: "WFSLeadJudge", //专业负责人判定
            Checker: "WFSChecker", //审核人
            CheckerJudge: "WFSCheckerJudge", //审核判定
            Validator: "WFSValidator", //审定人
            ValidatorJudge: "WFSValidatorJudge", //审定判定
            DesignGeneral: "WFSDesignGeneral", //设总
            DesignGeneralJudge: "WFSDesignGeneralJudge", //设总判定
            Manager: "WFSManager",//项目经理
            ManagerJudge: "WFSManagerJudge",//项目经理判定
            ReviewPass: "WFSReviewPass" //完成校审
        }
    };

    u.projectPlan = projectPlan;//项目策划
    u.planDiscipline = planDiscipline;//专业组
    u.drawingPlan = drawingPlan;//图纸策划
    u.sharingLog = sharingLog;//提资记录
    u.reviewLog = reviewLog;//校审意见

    u.designDoc = designDoc;//设计文档
    u.sharingDoc = sharingDoc;//提资文档
    u.manageDoc = manageDoc;//管理文档
    u.drawingSheet = drawingSheet;//成果图纸

    u.reviewFlow = reviewFlow;//图纸校审流程
})(md);