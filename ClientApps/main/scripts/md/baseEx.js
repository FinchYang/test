/******************************
* 协同云基础的元数据配置:
*部门小组，角色权限分配
*******************************/
var md = md || {};
(function(u, undefined) {
    var branch = {//部门或小组
        typeAlias: 'ObjBranch',
        classAlias: 'ClassBranch',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            BranchMember: "PropBranchMember"//部门或小组成员
        }
    };
    var rolePermission = {//角色权限分配
        typeAlias: 'ObjRolePermission',
        classAlias: 'ClassRolePermission',
        ownerAlias: null,
        propDefs: {
            NameOrTitle: "0",//名称或标题
            EditProject: "PropEditProject",//编辑项目概况
            InviteMember: "PropInviteMember",//邀请项目成员
            DeleteMember: "PropDeleteMember",//删除项目成员
            ModifyMemberRole: "PropModifyMemberRole",//修改成员角色
            CreateBranch: "PropCreateBranch",//创建部门/小组
            DeleteBranch: "PropDeleteBranch",//删除部门/小组
            AddRole: "PropAddRole",//添加自定义角色
            DeleteRole: "PropDeleteRole",//删除自定义角色
            ReadAllTasks: "PropReadAllTasks",//查看全部任务
            ReadProjReport: "PropReadProjReport",//查看项目报表
            ReadAllWorklog: "PropReadAllWorklog"//查看全部工作日志
        }
    };
    var namedAcl = {//命名的控制列表
        Task: "NaclTask",//任务权限
        JobLog: "NaclJobLog",//工作日志权限
        Branch: "NaclBranch",//部门或小组权限
        ProjectReport: "NaclProjectReport",//项目报表权限
        ProjectMemeber: "NaclProjectMemeber"//项目成员权限
    };

    u.branch = branch;
    u.rolePermission = rolePermission;
    u.namedAcl = namedAcl;
})(md);