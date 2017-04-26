/*****************************
*协同云项目报表模块元数据
******************************/

var md = md || {};
(function (u, undefined) {
    var jobLog = {//工作日志
        typeAlias: "ObjJobLog",
        classAlias: "ClassJobLog",
        ownerAlias: null,
        propDefs: {
            LogTitle: "PropLogTitle", //日志标题
            JobTask: "PropJobTask", //工作任务
            JobContent: "PropJobContent", //工作内容
            JobDate: "PropJobDate", //工作日期
            JobTime: "PropJobHours", //工作时间(h)
            JobFruit: "PropJobFruit", //工作成果
            JobQuestion: "PropJobQuestion" //主要问题
        }
    };

    var taskStatusReport = {//任务状态统计
        typeAlias: "ObjProjReport",
        classAlias: "ClassTaskStatusReport",
        ownerAlias: null,
        propDefs: {
            TitleOrName: "0", //名称或标题
            StartDate: "PropReportStartDate", //开始日期
            Deadline: "PropReportDeadline", //截止日期
            Task: "PropReportTask", //项目任务
            ReportInfo: "PropReportInfo" //报表信息
        }
    };
    var taskTimeReport = {//任务工时统计
        typeAlias: "ObjProjReport",
        classAlias: "ClassTaskTimeReport",
        ownerAlias: null,
        propDefs: {
            TitleOrName: "0", //名称或标题
            StartDate: "PropReportStartDate", //开始日期
            Deadline: "PropReportDeadline", //截止日期
            Task: "PropReportTask", //项目任务
            ReportInfo: "PropReportInfo" //报表信息
        }
    };
    var hoursReport = {//成员工时统计
        typeAlias: "ObjProjReport",
        classAlias: "ClassHoursReport",
        ownerAlias: null,
        propDefs: {
            TitleOrName: "0", //名称或标题
            StartDate: "PropReportStartDate", //开始日期
            Deadline: "PropReportDeadline", //截止日期
            ReportUser: "PropReportUser", //成员名称
            ReportInfo: "PropReportInfo" //报表信息
        }
    };
    var projCostReport = {//项目成本控制
        typeAlias: "ObjProjReport",
        classAlias: "ClassProjCostReport",
        ownerAlias: null,
        propDefs: {
            TitleOrName: "0", //名称或标题
            StartDate: "PropReportStartDate", //开始日期
            Deadline: "PropReportDeadline", //截止日期
            ReportUser: "PropReportUser", //成员名称
            ReportInfo: "PropReportInfo" //报表信息
        }
    };


    u.jobLog = jobLog; //工作日志

    u.taskStatusReport = taskStatusReport;//任务状态统计
    u.taskTimeReport = taskTimeReport;//任务工时统计
    u.hoursReport = hoursReport;//成员工时统计
    u.projCostReport = projCostReport;//项目成本控制
})(md);