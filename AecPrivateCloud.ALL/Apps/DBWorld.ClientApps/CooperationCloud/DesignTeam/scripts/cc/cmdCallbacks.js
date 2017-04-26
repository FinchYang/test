/***************
*向MFiles添加命令
***************/

var CC = CC || {};
(function(u, undefined) {
    u.cmds = {};

    u.sheetFlow = function(shellFrame) {
        ///<summary>出图校审</summary>
        var vault = shellFrame.ShellUI.Vault;
        var groupName = "图纸校审";
        var groupId = shellFrame.TaskPane.CreateGroup(groupName, -6);

        var cmdPass = new MF.Command("提交/通过", "icons/check.ico");
        cmdPass.create(shellFrame);
        cmdPass.registerEvent(shellFrame, function (sf) {
            //sf.CurrentPath = "项目文档\\设计文档\\校审区";
            CC.reviews.sheetPass(sf);
        });
        cmdPass.add2TaskGroup(shellFrame, groupId, 1);

        var cmdReject = new MF.Command("退回", "icons/reject.ico");
        cmdReject.create(shellFrame);
        cmdReject.registerEvent(shellFrame, function (sf) {
            //sf.CurrentPath = "项目文档\\设计文档\\校审区";
            CC.reviews.sheetReject(sf);
        });
        cmdReject.add2TaskGroup(shellFrame, groupId, 2);
        /*
        var cmdComment = new MF.Command("校审意见", "icons/reject.ico");
        cmdComment.create(shellFrame);
        cmdComment.registerEvent(shellFrame, function (sf) {
            CC.reviews.newReviewComment(sf);
        });
        cmdComment.add2TaskGroup(shellFrame, groupId, 3);
        */
        //设置校审流程状态
        var path = shellFrame.CurrentPath;
        var flag = false;
        var arr = path.split("\\");
        if (arr.length >= 3 && arr[0] + "\\" + arr[1] + "\\" + arr[2] == "项目文档\\设计文档\\校审区") {
            flag = true;
        }
        if (cmdReject.command && cmdPass.command && flag) {
            shellFrame.Commands.SetCommandState(cmdPass.command, CommandLocation_All, CommandState_Active);
            shellFrame.Commands.SetCommandState(cmdReject.command, CommandLocation_All, CommandState_Active);
        } else {
            shellFrame.Commands.SetCommandState(cmdPass.command, CommandLocation_All, CommandState_Hidden);
            shellFrame.Commands.SetCommandState(cmdReject.command, CommandLocation_All, CommandState_Hidden);
        }
    };

    u.earnedValueManageCmds = function (shellFrame){
        ///<summary>赢得值管理相关命令</summary>
        var vault = shellFrame.ShellUI.Vault;
        var groupName = "赢得值管理";
        var groupId = shellFrame.TaskPane.CreateGroup(groupName, -5);

        if (CC.userRole.isPM(vault)) {//项目经理组可见
            var cmdProjReport = new MF.Command("项目报表", 'icons/项目报表.ico');
            cmdProjReport.create(shellFrame);
            cmdProjReport.registerEvent(shellFrame, function (sf) {
                sf.CurrentPath = "项目报表";
            });
            cmdProjReport.add2TaskGroup(shellFrame, groupId, 0);

            //var cmdDutyCycle = new MF.Command("项目成员忙闲度", 'icons/项目成员忙闲度.ico');
            //cmdDutyCycle.create(shellFrame);
            //cmdDutyCycle.registerEvent(shellFrame, function (sf) {
            //    sf.ShellUI.ShowMessage("未实现！");
            //});
            //cmdDutyCycle.add2TaskGroup(shellFrame, groupId, 1);
        }

        var cmdDesignSchedule = new MF.Command("设计进度", "icons/设计进度.ico");
        cmdDesignSchedule.create(shellFrame);
        cmdDesignSchedule.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = "策划管理\\设计进度";
        });
        cmdDesignSchedule.add2TaskGroup(shellFrame, groupId, 1);

        var cmdJobRecordView = new MF.Command("查看工作日志", 'icons/查看工作日志.ico');
        cmdJobRecordView.create(shellFrame);
        cmdJobRecordView.registerEvent(shellFrame, function (sf) {
            var path = "工作日志";
            var tempPath = path;
            if (CC.userRole.isMember(vault)) {
                try {
                    var uId = vault.SessionInfo.UserID;
                    var uName = "";
                    var uLst = vault.UserOperations.GetUserList();
                    for (var i = 1; i <= uLst.Count; i++) {
                        if (uLst.Item(i).Key === uId) {
                            uName = uLst.Item(i).Name;
                            break;
                        }
                    }
                    if (uName) {
                        tempPath += "\\" + uName;
                    }
                    sf.CurrentPath = tempPath;
                } catch (e) {
                    //sf.CurrentPath = path;
                }
            }
            sf.CurrentPath = path;
        });
        cmdJobRecordView.add2TaskGroup(shellFrame, groupId, 2);

        var cmdJobLogNew = new MF.Command("新建工作日志", 'icons/新建工作日志.ico');
        cmdJobLogNew.create(shellFrame);
        cmdJobLogNew.registerEvent(shellFrame, function (sf) {
            CC.viewlog.CreateWorkLog(sf.ShellUI.Vault);
        });
        cmdJobLogNew.add2TaskGroup(shellFrame, groupId, 3);
    };

    u.createCrowdsourcingCmds = function(shellFrame) {
        ///<summary>众包管理相关命令</summary>
        var vault = shellFrame.ShellUI.Vault;
        var groupName = "众包管理";
        var groupId = shellFrame.TaskPane.CreateGroup(groupName, -5);

        var cmdName = "我的众包任务";
        if (CC.userRole.isOutsourceMember(vault)) {
            cmdName = "我领取的任务";
        }
        var cmdCrowdsrcTask = new MF.Command(cmdName, 'icons/我的众包任务.ico');
        cmdCrowdsrcTask.create(shellFrame);
        cmdCrowdsrcTask.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = groupName + "\\众包任务";
        });
        cmdCrowdsrcTask.add2TaskGroup(shellFrame, groupId, 0);

        var cmdCrowdsrcDoc = new MF.Command("众包文档管理", 'icons/众包文档管理.ico');
        cmdCrowdsrcDoc.create(shellFrame);
        cmdCrowdsrcDoc.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = groupName + "\\" + "众包文档";
        });
        cmdCrowdsrcDoc.add2TaskGroup(shellFrame, groupId, 1);

        if (CC.userRole.isOutsourceMember(vault) === false) {
            var cmdNewCrowdsrcTask = new MF.Command("新建众包任务", 'icons/新建众包任务.ico');
            cmdNewCrowdsrcTask.create(shellFrame);
            cmdNewCrowdsrcTask.registerEvent(shellFrame, function(sf) {
                //sf.ShellUI.ShowMessage("未实现！");
                CC.crowdsourcing.createTask(sf);
            });
            cmdNewCrowdsrcTask.add2TaskGroup(shellFrame, groupId, 2);

            var inviteCSMember = new MF.Command('邀请众包成员', 'icons/邀请成员.ico');
            inviteCSMember.create(shellFrame);
            inviteCSMember.registerEvent(shellFrame, function (sf) {
                //sf.ShellUI.ShowMessage("未实现！");
                CC.member.inviteMember(sf, 1);
            });
            inviteCSMember.add2TaskGroup(shellFrame, groupId, 3);
        }
    };

    u.createProjCmds = function (shellFrame) {
        ///<summary>项目相关命令</summary>
        var projManagerName = "项目配置";
        var projManagerId = shellFrame.TaskPane.CreateGroup(projManagerName, -5);

        var infoCmdName = "项目概况";
        var infoCmd = new MF.Command(infoCmdName, 'icons/项目概况.ico');
        infoCmd.create(shellFrame);
        infoCmd.registerEvent(shellFrame, function (sf) {
            //sf.CurrentPath = infoCmdName;

            var vault = shellFrame.ShellUI.Vault;
            //var typeId = MF.alias.objectType(vault, md.proj.typeAlias);
            //var objVns = MF.ObjectOps.SearchObjectsByType(vault, typeId);
            //if (objVns.Count > 0) {
            //    var objVer = objVns.Item(1).ObjVer;
            //    vault.ObjectOperations.ShowBasicEditObjectWindow(0, objVer);
            //} else {
            //    shellFrame.ShowMessage("项目异常！");
            //}

            var srcParties = [];
            if (MF.alias.objectType(vault, md.participant.typeAlias) !== -1) {
                srcParties = CC.invite.GetAllParties(vault);
            }
            var gToken = CC.getToken(vault);
            var projId = CC.getProjectId(vault);

            var project;
            try {
                var projectInfo = webapi.getProject(vault, projId, gToken);
                var projObj = eval('(' + projectInfo + ')');
                if (projObj.status !== 200) {
                    shellFrame.ShellUI.ShowMessage(projObj.response.Message);
                    return;
                }
                project = projObj.response;
            } catch (e) {
                shellFrame.ShellUI.ShowMessage(e.message);
                return;
            }
            var cover = project.Cover;
            // Replace the listing with a dashboard.
            var customData = {
                token: gToken,
                projId: projId,
                cover: cover,
                srcParties: srcParties,
                project: project
            };
            
            shellFrame.ShowPopupDashboard("home", true, customData);
        });
        infoCmd.add2TaskGroup(shellFrame, projManagerId, 1);

        var memberCmdName = "成员列表";
        var memberCmd = new MF.Command(memberCmdName, 'icons/成员列表.ico');
        memberCmd.create(shellFrame);
        memberCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = '项目成员';
        });
        memberCmd.add2TaskGroup(shellFrame, projManagerId, 2);

        var disciplineCmdName = "专业配置";
        var disciplineCmd = new MF.Command(disciplineCmdName, 'icons/专业配置.ico');
        disciplineCmd.create(shellFrame);
        disciplineCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = "策划管理" + "\\" + "专业配置";
        });
        disciplineCmd.add2TaskGroup(shellFrame, projManagerId, 3);

        var projPlanCmdName = "项目策划";
        var projPlanCmd = new MF.Command(projPlanCmdName, 'icons/项目策划.ico');
        projPlanCmd.create(shellFrame);
        projPlanCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = "策划管理" + "\\" + "项目策划"; 
        });
        projPlanCmd.add2TaskGroup(shellFrame, projManagerId, 4);

        var designPlanCmdName = "设计策划";
        var designPlanCmd = new MF.Command(designPlanCmdName, 'icons/设计策划.ico');
        designPlanCmd.create(shellFrame);
        designPlanCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = "策划管理" + "\\" + "设计策划";
        });
        designPlanCmd.add2TaskGroup(shellFrame, projManagerId, 5);

        var inviteCmdName = "邀请成员";
        var inviteCmd = new MF.Command(inviteCmdName, 'icons/邀请成员.ico');
        inviteCmd.create(shellFrame);
        inviteCmd.registerEvent(shellFrame, function (sf) {
            CC.member.inviteMember(sf);
        });
        inviteCmd.add2TaskGroup(shellFrame, projManagerId, 6);  
    }

    u.createTaskCmds = function (shellFrame) {
        ///<summary>任务相关命令</summary>

        var taskGroupName = "任务管理";
        var taskGroupId = shellFrame.TaskPane.CreateGroup(taskGroupName, -5);

        var taskCmdNameMy = "指派给我的任务";
        var taskCmdMy = new MF.Command(taskCmdNameMy, 'icons/指派给我的任务.ico');
        taskCmdMy.create(shellFrame);
        taskCmdMy.registerEvent(shellFrame, function (sf) {
            try {
                sf.CurrentPath = "指派给我的任务";
            } catch (e) {
                sf.CurrentPath = "我的任务";
            }
        });
        taskCmdMy.add2TaskGroup(shellFrame, taskGroupId, 1);

        var taskCmdName = "我指派的任务";
        var taskCmd = new MF.Command(taskCmdName, 'icons/我指派的任务.ico');
        taskCmd.create(shellFrame);
        taskCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = taskCmdName;
        });
        taskCmd.add2TaskGroup(shellFrame, taskGroupId, 2);

        var taskCmdNameAll = "全部任务列表";
        var taskCmdAll = new MF.Command(taskCmdNameAll, 'icons/全部任务列表.ico');
        taskCmdAll.create(shellFrame);
        taskCmdAll.registerEvent(shellFrame, function (sf) {
            try {
                sf.CurrentPath = "项目任务";
            } catch (e) {

            }
        });
        taskCmdAll.add2TaskGroup(shellFrame, taskGroupId, 3);

        //var taskExportCmdName = "导出任务列表";
        //var taskCmdExport = new MF.Command(taskExportCmdName, 'icons/全部任务列表.ico');
        //taskCmdExport.create(shellFrame);
        //taskCmdExport.registerEvent(shellFrame, function (sf) {
        //    if (sf.CurrentPath !== "项目任务") {
        //        sf.CurrentPath = "项目任务";
        //    }
        //    var ok = false;
        //    while (!ok) {
        //        try {
        //            sf.Commands.ExecuteCommand(BuiltinCommand_ExportObjects, null);
        //            ok = true;
        //        } catch (e) {

        //        }
        //    }
        //});
        //taskCmdExport.add2TaskGroup(shellFrame, taskGroupId, 4);

        var taskNewCmdName = "新建任务";
        var taskCmdNew = new MF.Command(taskNewCmdName, 'icons/新建任务.ico');
        taskCmdNew.create(shellFrame);
        taskCmdNew.registerEvent(shellFrame, function (sf) {
            //MF.ui.createNewObjectWithWindow(sf, MFBuiltInObjectTypeAssignment);
            //指定一般任务
            var vault = shellFrame.ShellUI.Vault;
            var classId = MF.alias.classType(vault, md.genericTask.classAlias);
            MF.ui.createNewObjectShowWindow(sf, MFBuiltInObjectTypeAssignment,
                { type: MFBuiltInPropertyDefClass, value: classId });
        });
        taskCmdNew.add2TaskGroup(shellFrame, taskGroupId, 4);

    };

    u.createDocumentCmds = function (shellFrame) {
        ///<summary>文档相关命令</summary>
        var docManagerName = "文档管理";
        var docManagerId = shellFrame.TaskPane.CreateGroup(docManagerName, -5);
        var rootName = "项目文档";
        var vault = shellFrame.ShellUI.Vault;

        var hasParty = MF.alias.objectType(vault, md.participant.typeAlias) !== -1; //'ObjParticipant'
        var hasProjPlan = MF.alias.objectType(vault, md.projectPlan.typeAlias) !== -1;
        if (hasProjPlan) {
            var docDesignCmdName = "设计文档";
            var docDesignCmd = new MF.Command(docDesignCmdName, 'icons/设计文档.ico');
            docDesignCmd.create(shellFrame);
            docDesignCmd.registerEvent(shellFrame, function (sf) {
                sf.CurrentPath = rootName + "\\" + docDesignCmdName;
            });
            docDesignCmd.add2TaskGroup(shellFrame, docManagerId, 1);

            var docManageCmdName = "管理文档";
            var docManageCmd = new MF.Command(docManageCmdName, 'icons/管理文档.ico');
            docManageCmd.create(shellFrame);
            docManageCmd.registerEvent(shellFrame, function (sf) {
                sf.CurrentPath = rootName + "\\" + docManageCmdName;
            });
            docManageCmd.add2TaskGroup(shellFrame, docManagerId, 2);

            var docArchiveCmdName = "归档文档";
            var docArchiveCmd = new MF.Command(docArchiveCmdName, 'icons/归档文档.ico');
            docArchiveCmd.create(shellFrame);
            docArchiveCmd.registerEvent(shellFrame, function (sf) {
                sf.CurrentPath = rootName + "\\" + docArchiveCmdName;
            });
            docArchiveCmd.add2TaskGroup(shellFrame, docManagerId, 3);
        } else {
            var docPersonalCmdName = "个人文档";
            var postStr = '';
            if (hasParty) {
                var account = CC.member.getAccountByUserId(vault, vault.SessionInfo.UserID);
                docPersonalCmdName = "工作文档";
                if (account && account.partyName) {
                    postStr = "\\" + account.partyName;
                }
            }
            var docPersonalCmd = new MF.Command(docPersonalCmdName, 'icons/个人文档.ico');
            docPersonalCmd.create(shellFrame);
            docPersonalCmd.registerEvent(shellFrame, function (sf) {
                sf.CurrentPath = rootName + "\\" + docPersonalCmdName + postStr;
            });
            docPersonalCmd.add2TaskGroup(shellFrame, docManagerId, 1);

            var docShareCmdName = "共享文档";
            if (!hasParty) {
                docShareCmdName = "工作空间";
            }
            var docShareCmd = new MF.Command(docShareCmdName, 'icons/共享文档.ico');
            docShareCmd.create(shellFrame);
            docShareCmd.registerEvent(shellFrame, function (sf) {
                sf.CurrentPath = rootName + "\\" + docShareCmdName;
            });
            docShareCmd.add2TaskGroup(shellFrame, docManagerId, 2);
        }

        var templateCmd = new MF.Command("模板库", 'icons/模板库.ico');
        templateCmd.create(shellFrame);
        templateCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = "模板";
        });
        templateCmd.add2TaskGroup(shellFrame, docManagerId, 100);
    };

    u.createBIMCmds = function (shellFrame) {
        ///<summary>BIM相关命令</summary>
        var bimManagerName = "BIM模型管理";
        var bimManagerId = shellFrame.TaskPane.CreateGroup(bimManagerName, -5);
        
        var cmdModelPlan = new MF.Command("模型策划", 'icons/模型策划.ico');
        cmdModelPlan.create(shellFrame);
        cmdModelPlan.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = bimManagerName + "\\模型策划";
        });
        cmdModelPlan.add2TaskGroup(shellFrame, bimManagerId, 0);

        var bimMouldListName = "模型组织";
        var bimMouldListCmd = new MF.Command(bimMouldListName, 'icons/文件浏览.ico');
        bimMouldListCmd.create(shellFrame);
        bimMouldListCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = bimManagerName + "\\" + "BIM模型";
        });
        bimMouldListCmd.add2TaskGroup(shellFrame, bimManagerId, 1);

        var bimMouldBrowseName = "模型浏览";
        var bimMouldBrowseCmd = new MF.Command(bimMouldBrowseName, 'icons/模型浏览.ico');
        bimMouldBrowseCmd.create(shellFrame);
        bimMouldBrowseCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = bimManagerName + "\\" + "模型预览";
        });
        bimMouldBrowseCmd.add2TaskGroup(shellFrame, bimManagerId, 2);
    };

    u.createMailCmds = function (shellFrame) {
        ///<summary>邮件相关命令</summary>
        var emailManagerName = "邮件管理";
        var emailManagerId = shellFrame.TaskPane.CreateGroup(emailManagerName, -5);

        var rootName = "邮件系统";

        var inboxCmdName = "收件箱";
        var inboxCmd = new MF.Command(inboxCmdName, 'icons/收件箱.ico');
        inboxCmd.create(shellFrame);
        inboxCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = rootName + '\\' + inboxCmdName;
        });
        inboxCmd.add2TaskGroup(shellFrame, emailManagerId, 1);

        var sentCmdName = "发件箱";
        var sentCmd = new MF.Command(sentCmdName, 'icons/发件箱.ico');
        sentCmd.create(shellFrame);
        sentCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = rootName + '\\' + sentCmdName;
        });
        sentCmd.add2TaskGroup(shellFrame, emailManagerId, 2);

        var draftCmdName = "草稿箱";
        var draftCmd = new MF.Command(draftCmdName, 'icons/草稿箱.ico');
        draftCmd.create(shellFrame);
        draftCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = rootName + '\\' + draftCmdName;
        });
        draftCmd.add2TaskGroup(shellFrame, emailManagerId, 3);

        var addrBookCmdName = "通讯录";
        var addrBookCmd = new MF.Command(addrBookCmdName, 'icons/通讯录.ico');
        addrBookCmd.create(shellFrame);
        addrBookCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = '邮件通讯录';
        });
        addrBookCmd.add2TaskGroup(shellFrame, emailManagerId, 4);

        var emailNewCmdName = "新建邮件";
        var emailNewCmd = new MF.Command(emailNewCmdName, 'icons/新建邮件.ico');
        emailNewCmd.create(shellFrame);
        emailNewCmd.registerEvent(shellFrame, function (sf) {
            var v = sf.ShellUI.Vault;
            var exePath = CC.getInstallPath(sf.ShellUI.Vault) + "\\" + "DBWorld.MailClient.exe";
            var type = "0";
            var objId = "0";
            var selObjVersAndPros = sf.ActiveListing.CurrentSelection.ObjectVersionsAndProperties;
            if (selObjVersAndPros.count == 1) {
                //判断是否为草稿箱
                if (CC.mail.getSelFolderName(v, selObjVersAndPros) === "草稿箱") {
                    type = "1";
                    objId = selObjVersAndPros.Item(1).ObjVer.ID;
                } else {
                    type = "0";
                    objId = "0";
                }
            }
            if (!wshUtils.fileExists(exePath)) {
                sf.ShellUI.ShowMessage('未能找到文件：' + exePath);
                return;
            }
            wshUtils.runProgramWithUI(exePath, [sf.ShellUI.Vault.Name, type, objId]);
        });
        emailNewCmd.add2TaskGroup(shellFrame, emailManagerId, 5);

        var setMailCmdName = "邮箱设置";
        var setMailCmd = new MF.Command(setMailCmdName, 'icons/邮箱设置.ico');
        setMailCmd.create(shellFrame);
        setMailCmd.registerEvent(shellFrame, function (sf) {
            var exePath = CC.getInstallPath(sf.ShellUI.Vault) + "\\" + "DBWorld.MailConfig.exe";
            var projId = CC.getProjectId(sf.ShellUI.Vault);
            var webUrl = webapi.getWebHost(sf.ShellUI.Vault);
            wshUtils.runProgramWithUI(exePath, [sf.ShellUI.Vault.Name, projId, webUrl]);
        });
        setMailCmd.add2TaskGroup(shellFrame, emailManagerId, 6);
    };

    u.createProgressCmds = function (shellFrame) {
        ///<summary>进度相关命令</summary>
        var progressManagerName = "进度管理";
        var progressManagerId = shellFrame.TaskPane.CreateGroup(progressManagerName, -5);
        var rootName = "设计进度";

        var planCmdName = "方案设计";
        var planCmd = new MF.Command(planCmdName, 'icons/方案设计.ico');
        planCmd.create(shellFrame);
        planCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = rootName + '\\' + planCmdName;
        });
        planCmd.add2TaskGroup(shellFrame, progressManagerId, 1);

        var preliminaryCmdName = "初步设计";
        var preliminaryCmd = new MF.Command(preliminaryCmdName, 'icons/初步设计.ico');
        preliminaryCmd.create(shellFrame);
        preliminaryCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = rootName + '\\' + preliminaryCmdName;
        });
        preliminaryCmd.add2TaskGroup(shellFrame, progressManagerId, 2);

        var buildNewCmdName = "施工图";
        var buildNewCmd = new MF.Command(buildNewCmdName, 'icons/施工图.ico');
        buildNewCmd.create(shellFrame);
        buildNewCmd.registerEvent(shellFrame, function (sf) {
            sf.CurrentPath = rootName + '\\' + '施工图设计';
        });
        buildNewCmd.add2TaskGroup(shellFrame, progressManagerId, 3);
    }
    //添加命令入口
    u.createCmds = function (shellFrame, globalObject) {
        if (!shellFrame.TaskPane.Available) return;
        var vault = shellFrame.ShellUI.Vault;
        if (CC.userRole.isOutsourceMember(vault) === false) {
            //图纸校审
            this.sheetFlow(shellFrame);

            this.createProjCmds(shellFrame);
            this.createTaskCmds(shellFrame);
            this.createDocumentCmds(shellFrame);
            var hasUnit = MF.alias.objectType(vault, md.unit.typeAlias) !== -1; //'ObjModelUnit'
            if (hasUnit) {
                this.createBIMCmds(shellFrame);
            }
            this.createMailCmds(shellFrame);
            //this.createProgressCmds(shellFrame);
            var hasReport = MF.alias.objectType(vault, md.taskStatusReport.typeAlias) !== -1; //'ObjProjReport'
            if (hasReport) {
                this.earnedValueManageCmds(shellFrame);
            }
        }
        
        //var hasCrowSrcTask = MF.alias.classType(vault, md.crowdSrcTask.classAlias) !== -1; //'ClassCrowdSrcTask'
        //if (hasCrowSrcTask) {
        //    this.createCrowdsourcingCmds(shellFrame);
        //}

        this.createContextMenuCmds(shellFrame,globalObject);
    };

    u.createContextMenuCmds = function (shellFrame, globalObject) {
        var vault = shellFrame.ShellUI.Vault;
        var hasParty = MF.alias.objectType(vault, md.participant.typeAlias) !== -1; //'ObjParticipant'
        if (hasParty) {
            //添加“共享”按钮
            var cmdShare = new MF.Command('共享给...', '');
            cmdShare.create(shellFrame);
            cmdShare.registerEvent(shellFrame, cmdCallbackFn.shareDocs(shellFrame));
            cmdShare.add2ContextMenu(shellFrame, MenuLocation_ContextMenu_Top, 0);
            u.cmds.cmdShareDoc = cmdShare.command;
            shellFrame.commands.SetCommandState(u.cmds.cmdShareDoc, CommandLocation_All, CommandState_Hidden);
        }
        //添加“删除属性文件夹”按钮
        var cmdDeleteFolder = new MF.Command('删除文件夹', '');
        cmdDeleteFolder.create(shellFrame);
        cmdDeleteFolder.registerEvent(shellFrame, cmdCallbackFn.deletePropertyFolder(shellFrame));
        cmdDeleteFolder.add2ContextMenu(shellFrame, MenuLocation_ContextMenu_Top, 1);
        u.cmds.cmdDeletePropFolder = cmdDeleteFolder.command;
        shellFrame.Commands.SetCommandState(u.cmds.cmdDeletePropFolder, CommandLocation_All, CommandState_Hidden);

        //设计策划模板
        var hasDesignPlan = MF.alias.objectType(vault, md.drawingPlan.typeAlias) !== -1;
        if (hasDesignPlan) {
            //添加“提资给”按钮
            var cmdSharingTo = new MF.Command('提资给...', '');
            cmdSharingTo.create(shellFrame);
            cmdSharingTo.registerEvent(shellFrame, cmdCallbackFn.sharingTo(shellFrame));
            cmdSharingTo.add2ContextMenu(shellFrame, MenuLocation_ContextMenu_Top, 3);
            u.cmds.cmdSharingTo = cmdSharingTo.command;
            shellFrame.commands.SetCommandState(u.cmds.cmdSharingTo, CommandLocation_All, CommandState_Hidden);
            //添加“上传到校审区”按钮
            var cmdCopy2Review = new MF.Command('上传到校审区', '');
            cmdCopy2Review.create(shellFrame);
            cmdCopy2Review.registerEvent(shellFrame, cmdCallbackFn.copy2Review(shellFrame));
            cmdCopy2Review.add2ContextMenu(shellFrame, MenuLocation_ContextMenu_Top, 4);
            u.cmds.cmdCopy2Review = cmdCopy2Review.command;
            shellFrame.commands.SetCommandState(u.cmds.cmdCopy2Review, CommandLocation_All, CommandState_Hidden);
            //添加"归档"按钮
            var cmdArchiveFiles = new MF.Command('归档...', '');
            cmdArchiveFiles.create(shellFrame);
            cmdArchiveFiles.registerEvent(shellFrame, cmdCallbackFn.archiveFiles(shellFrame));
            cmdArchiveFiles.add2ContextMenu(shellFrame, MenuLocation_ContextMenu_Top, 5);
            u.cmds.cmdArchiveFiles = cmdArchiveFiles.command;
            shellFrame.commands.SetCommandState(u.cmds.cmdArchiveFiles, CommandLocation_All, CommandState_Hidden);
        }

        //添加“从云端打开”按钮
        var cmdOpenAtCloud = new MF.Command('从云端打开', '');
        cmdOpenAtCloud.create(shellFrame);
        cmdOpenAtCloud.registerEvent(shellFrame, cmdCallbackFn.openAtCloud(shellFrame, globalObject));
        cmdOpenAtCloud.add2ContextMenu(shellFrame, MenuLocation_ContextMenu_Top, 10);
        u.cmds.cmdOpenAtCloud = cmdOpenAtCloud.command;
        shellFrame.commands.SetCommandState(u.cmds.cmdOpenAtCloud, CommandLocation_All, CommandState_Hidden);    
    };

    u.setCmdSateAsSelectionChanged = function (shellFrame) {
        if (!shellFrame.ActiveListing || !shellFrame.ActiveListing.CurrentSelection) {
            return;
        }
        try {
            var vault = shellFrame.ShellUI.Vault;
            
            //设置“删除文件夹”按钮显示状态
            var folders = shellFrame.ActiveListing.CurrentSelection.Folders;
            var cmdDeleteId = u.cmds.cmdDeletePropFolder;
            if (cmdDeleteId && folders.Count == 1) {
                if (folders.Item(1).FolderDefType == MFFolderDefTypePropertyFolder) {
                    shellFrame.Commands.SetCommandState(cmdDeleteId, CommandLocation_All, CommandState_Active);
                } else {
                    shellFrame.Commands.SetCommandState(cmdDeleteId, CommandLocation_All, CommandState_Hidden);
                }
            } else {
                shellFrame.Commands.SetCommandState(cmdDeleteId, CommandLocation_All, CommandState_Hidden);
            }

            var hasParty = MF.alias.objectType(vault, md.participant.typeAlias) !== -1; //'ObjParticipant'
            if (hasParty) {
                //设置“共享给”按钮显示状态
                var objVns = shellFrame.ActiveListing.CurrentSelection.ObjectVersions;
                var cmdShareId = u.cmds.cmdShareDoc;
                if (objVns.Count > 0 && folders.Count === 0) {
                    var docClassId = MF.alias.classType(vault, md.currentPartDoc.classAlias);  //"ClassCurrentPartDoc"
                    var iCount = 0;
                    for (var i = 1; i <= objVns.Count; i++) {
                        if (objVns.Item(i).Deleted == false && objVns.Item(i).Class == docClassId) {
                            iCount++;
                        } else { break; }
                    }
                    
                    if (iCount > 0 && objVns.Count == iCount) {
                        shellFrame.Commands.SetCommandState(cmdShareId, CommandLocation_All, CommandState_Active);
                    } else {
                        shellFrame.Commands.SetCommandState(cmdShareId, CommandLocation_All, CommandState_Hidden);
                    }
                }
                //选择"虚拟文件夹"时，设置"共享给"按钮状态，项目文档\工作文档\**方...下显示
               else if (objVns.Count === 0 && folders.Count > 0) {
                   var currentPath = shellFrame.ActiveListing.CurrentPath;
                    var flag = false;
                    if (currentPath) {
                        //shellFrame.ShellUI.ShowMessage(currentPath);
                        var arr = currentPath.split("\\");
                        if (arr.length > 2 && arr[0] == "项目文档" && arr[1] == "工作文档") {
                            flag = true;
                        }
                    }
                    if (folders.Count == 1 && flag && folders.Item(1).FolderDefType == MFFolderDefTypePropertyFolder) {                       
                        shellFrame.Commands.SetCommandState(cmdShareId, CommandLocation_All, CommandState_Active);
                    } else {
                        shellFrame.Commands.SetCommandState(cmdShareId, CommandLocation_All, CommandState_Hidden);
                    }
                } else {
                   shellFrame.Commands.SetCommandState(cmdShareId, CommandLocation_All, CommandState_Hidden);
                }
            }

            //设置"从云端打开"显示状态
            var objFiles = shellFrame.ActiveListing.CurrentSelection.ObjectFiles;
            if (objFiles.Count == 1) {
                shellFrame.Commands.SetCommandState(this.cmds.cmdOpenAtCloud, CommandLocation_All, CommandState_Active);
            } else {
                shellFrame.Commands.SetCommandState(this.cmds.cmdOpenAtCloud, CommandLocation_All, CommandState_Hidden);
            }

            //设置“上传到校审区”,"提资给.."显示状态
            var hasDesignPlan = MF.alias.objectType(vault, md.drawingPlan.typeAlias) !== -1;
            if (hasDesignPlan) {
                var docVns = shellFrame.ActiveListing.CurrentSelection.ObjectVersions;
                var cmdSharingToId = u.cmds.cmdSharingTo;
                var cmdCopy2ReviewId = u.cmds.cmdCopy2Review;
                if (docVns.Count > 0 && folders.Count === 0) {
                    var designDocClassId = MF.alias.classType(vault, md.designDoc.classAlias); 
                    iCount = 0;
                    for (var j = 1; j <= docVns.Count; j++) {
                        if (docVns.Item(j).Deleted == false && docVns.Item(j).Class == designDocClassId) {
                            iCount++;
                        } else {
                            break;
                        }
                    }
                    if (iCount > 0 && docVns.Count == iCount) {
                        shellFrame.Commands.SetCommandState(cmdCopy2ReviewId, CommandLocation_All, CommandState_Active);
                        shellFrame.Commands.SetCommandState(cmdSharingToId, CommandLocation_All, CommandState_Active);
                    } else {
                        shellFrame.Commands.SetCommandState(cmdCopy2ReviewId, CommandLocation_All, CommandState_Hidden);
                        shellFrame.Commands.SetCommandState(cmdSharingToId, CommandLocation_All, CommandState_Hidden);
                    }
                } else {
                    shellFrame.Commands.SetCommandState(cmdCopy2ReviewId, CommandLocation_All, CommandState_Hidden);
                    shellFrame.Commands.SetCommandState(cmdSharingToId, CommandLocation_All, CommandState_Hidden);
                }

                //归档
                var cmdArchiveFiles = u.cmds.cmdArchiveFiles;
                if (docVns.Count > 0 && folders.Count === 0) {
                    iCount = 0;
                    for (j = 1; j <= docVns.Count; j++) {
                        if (docVns.Item(j).Deleted == false && docVns.Item(j).ObjVer.Type === 0) {
                            iCount++;
                        } else {
                            break;
                        }
                    }
                    if (iCount > 0 && docVns.Count == iCount) {
                        shellFrame.Commands.SetCommandState(cmdArchiveFiles, CommandLocation_All, CommandState_Active);
                    } else {
                        shellFrame.Commands.SetCommandState(cmdArchiveFiles, CommandLocation_All, CommandState_Hidden);
                    }
                } else {
                    shellFrame.Commands.SetCommandState(cmdArchiveFiles, CommandLocation_All, CommandState_Hidden);
                }
            }
        } catch (e) {
            return;
        }
    };

    u.addTabAsSelectionChanged = function(shellFrame) {
        if (!shellFrame.ActiveListing || !shellFrame.ActiveListing.CurrentSelection) return;
        try {
            var vault = shellFrame.ShellUI.Vault;
            var reportType = MF.alias.objectType(vault, md.taskStatusReport.typeAlias); //'ObjProjReport'
            if (reportType !== -1) {
                //当选中"项目报表"对象时，添加Tab
                var objVnsPros = shellFrame.ActiveListing.CurrentSelection.ObjectVersionsAndProperties;
                if (objVnsPros && objVnsPros.Count === 1 && objVnsPros.Item(1).ObjVer.Type == reportType) {
                    //隐藏内建的tabs
                    cmdCallbackFn.hideTabAtRightPane(shellFrame, "_details");
                    cmdCallbackFn.hideTabAtRightPane(shellFrame, "_preview");

                    var objVn = objVnsPros.Item(1).VersionData;
                    var objProps = objVnsPros.Item(1).Properties;
                    
                    var taskStatusReportClass = MF.alias.classType(vault, md.taskStatusReport.classAlias);
                    var taskTimeReportClass = MF.alias.classType(vault, md.taskTimeReport.classAlias);              
                    var hoursReportClass = MF.alias.classType(vault, md.hoursReport.classAlias);                  
                    var projCostReportClass = MF.alias.classType(vault, md.projCostReport.classAlias);

                    var tabId;
                    var customData;
                    if (objVn.Class == taskStatusReportClass) {//任务状态统计
                        tabId = "_taskStatusReport";
                        cmdCallbackFn.toBeVisible(shellFrame, tabId);

                        customData = { "objVersion": objVn, "properties": objProps };
                        cmdCallbackFn.addTabAtRightPane(shellFrame, 'tasksstatusreport', customData, tabId, "任务状态统计");
                    } else if (objVn.Class == taskTimeReportClass) {//任务工时统计
                        
                        tabId = "_taskTimeReport";
                        cmdCallbackFn.toBeVisible(shellFrame, tabId);

                        customData = { "objVersion": objVn, "properties": objProps };
                        cmdCallbackFn.addTabAtRightPane(shellFrame, 'tasktimereport', customData, tabId, "任务工时统计");
                    } else if (objVn.Class == hoursReportClass) {//成员工时统计
                        
                        tabId = "_hoursReport";
                        cmdCallbackFn.toBeVisible(shellFrame, tabId);

                        customData = { "objVersion": objVn, "properties": objProps };
                        cmdCallbackFn.addTabAtRightPane(shellFrame, 'memberhoursreport', customData, tabId, "成员工时统计");
                    } else if (objVn.Class == projCostReportClass) {//项目成本控制
                        
                        tabId = "_projCostReport";
                        cmdCallbackFn.toBeVisible(shellFrame, tabId);

                        customData = { "objVersion": objVn, "properties": objProps };
                        cmdCallbackFn.addTabAtRightPane(shellFrame, 'projectcostreport', customData, tabId, "项目成本控制");
                    }
                }
            }
        } catch (e) {
            return;
        }
    };
})(CC);

//<summary>命令的回调函数</summary>
var cmdCallbackFn = cmdCallbackFn || {};

(function (fn) {
    ///<summary>从云端打开</summary>
    fn.openAtCloud = function (sf, globalObject) {
        var that = this;
        return function () {
            var vault = sf.shellUI.Vault;
            that.setGlobalObject(vault, globalObject);

            var accountName = vault.SessionInfo.AccountName;
            //sf.shellUI.ShowMessage(accountName);
            if (sf.ActiveListing.CurrentSelection.ObjectFiles.Count > 0) {
                var selectedItems = sf.ActiveListing.CurrentSelection.ObjectFiles;
                for (var i = 1; i <= selectedItems.Count; i++) {
                    var sItem = selectedItems.Item(i);
                    var ext = sItem.ObjectFile.Extension;
                    var appSofts = softConfigOp.getAppsByExtension(ext);
                    //var mySoftList = globalObject.mySoftList;
                    ////var appSoft = this.getOpenSoftFromBoughts(mySoftList, appSofts);
                    //var appSoft = that.getOpenSoftFromBoughtsByExt(mySoftList, ext);
                    var appSoft = appSofts.length == 0 ? undefined : appSofts[0];
                    if (appSoft) {
                        var path0 = vault.ObjectFileOperations.GetPathInDefaultView(sItem.ObjVer.ObjID, sItem.ObjVer.Version,
                            sItem.ObjectFile.ID, sItem.ObjectFile.Version, MFLatestSpecificBehaviorAutomatic, false);

                        that.openFileFromMfile(sf, appSoft, path0, accountName, sItem.ObjVer.ObjID.ID, globalObject);
                    } else {
                        sf.shellUI.ShowMessage("您未购买相应的软件！");
                    }
                    break;
                };
            }
        }        
    };
    fn.getOpenSoftFromBoughtsByExt = function (boughtSofts, ext) {
        for (var i = 0; i < boughtSofts.length; i++) {
            var extstr = boughtSofts[i].OpenExt;
            if (extstr) {
                var exts = extstr.split(",");
                for (var j = 0; j < exts.length; j++) {
                    if (exts[j] == ext || exts[j] == "." + ext) {
                        return boughtSofts[i];
                    }
                }
            }
        }
        return null;
    };
    fn.getOpenSoftFromBoughts = function (boughtSofts, canOpenSofts) {
        for (var i = 0; i < canOpenSofts.length; i++) {
            var appSoft = canOpenSofts[i];
            for (var j = 0; j < boughtSofts.length; j++) {
                if (boughtSofts[j].ApplicationID == appSoft.appID) {
                    return appSoft;
                }
            }
        }
        return null;
    }
    //从云端打开文件
    fn.openFileFromMfile = function (shellFrame, app, filePath, accountName, objId, globalObject) {
        var domain = "";
        var userName = "";
        var account = accountName.split('\\');
        if (account.length > 1) {
            userName = account[1];
            domain = account[0];
        } else {
            userName = account[0];
        }
        //var pwd = this.getPassword();
        //if (!pwd) pwd = "abcde12345!";
        var appId = app.ApplicationID;
        var appName = app.AppName;
        //shellFrame.shellUI.ShowMessage(appId);

        //文件路径:中间件
        var vault = shellFrame.shellUI.Vault;
        var vaultGuid = vault.SessionInfo.VaultGUID;
        var vaultName = vault.Name;
        filePath = vaultGuid + " " + vaultName + " " + appId + " " + filePath;

        //转码，保证IE设别 中文字符串
        domain = encodeURIComponent(domain);
        userName = encodeURIComponent(userName);
        var pwd = encodeURIComponent(globalObject.password);
        //appId = encodeURIComponent(appId);
        appName = encodeURIComponent(appName);
        filePath = encodeURIComponent(filePath);

        var appIdM = "Citrix.MPS.App.Farm1.AppManagement";
        appIdM = encodeURIComponent(appIdM);
        var appNameM = "AppManagement";
        appNameM = encodeURIComponent(appNameM);

        var cloudUrl = globalObject.cloudUrl;
        //shellFrame.shellUI.ShowMessage(cloudUrl);
        //shellFrame.shellUI.ShowMessage(globalObject.token);

        var url = cloudUrl + "/Common/launch.aspx?Domain=" + domain + "&UserName=" + userName + "&Password=" + pwd
                + "&ApplicationID=" + appIdM + "&AppName=" + appNameM + "&ProjectName=&FilePath=" + filePath;

        var appPath = MFiles.ApplicationPath + "WebConsole.exe";

        var fso = new ActiveXObject('Scripting.FileSystemObject');
        if (!fso.FileExists(appPath)) {
            shellFrame.shellUI.ShowMessage("缺少WebConcole！");
            return;
        }
        var fileName = objId || userName;
        var urlTempFile = fso.GetSpecialFolder(2) + "\\" + appName + "-" + fileName + ".txt";
        this.createFile(urlTempFile, url);
        var tempPath = fso.GetSpecialFolder(2) + "\\" + appName + "-" + fileName + ".ica";
        //shellFrame.shellUI.ShowMessage(tempPath);
        this.runProgram(appPath, [url, tempPath]);
        this.runProgram(tempPath);
        //this.runProgramUrl(url);
    };
    fn.createFile = function (filePath, content) { //for write
        var fso = new ActiveXObject("Scripting.FileSystemObject");
        try {
            //判断文件是否已经存在
            if (fso.FileExists(filePath)) {
                fso.DeleteFile(filePath);
            }
            var fh = fso.CreateTextFile(filePath, 2, false); //8
            fh.WriteLine(content);
            fh.Close();
        } catch (e) {
            throw ('无权限创建文件！');
        } finally {
            fso = null;
        }
    };
    fn.runProgram = function (exeFile, args) {
        /// <summary>Run external program.</summary>
        /// <param name="exeFile" type="String">the program file(*.exe).</param>
        /// <param name="args" type="Array">commandline arguments.</param>
        /// <returns>errorCode:0, success;other, error</returns>
        var shell = new ActiveXObject("WScript.Shell");
        var cmd = '"' + exeFile + '"';
        if (args != undefined) {
            for (var i = 0; i < args.length; i++) {
                cmd = cmd + ' ' + args[i];
            }
        }
        var errorCode = shell.Run(cmd, 0, true);
        //shell = null;
        return errorCode;
    };
    fn.runProgramUrl = function (exeFile, args, wait, windowId) {
        /// <summary>Run external program.</summary>
        /// <param name="exeFile" type="String">the program file(*.exe).</param>
        /// <param name="args" type="Array">commandline arguments. []</param>
        /// <returns>errorCode:0, success;other, error</returns>
        var shell = new ActiveXObject("WScript.Shell");
        var cmd = '"' + exeFile + '"';
        args = args || [];
        for (var i = 0; i < args.length; i++) {
            if (args[i] === '') continue;
            cmd = cmd + ' ' + '"' + args[i] + '"';
        }
        //shellFrame.ShellUI.ShowMessage(cmd);
        if (!windowId) windowId = 0;
        var errorCode;
        if (args.length > 0) {
            errorCode = shell.Run(cmd, windowId, wait);
        } else {
            errorCode = shell.Run(cmd);
        }
        shell = null;
        return errorCode;
    };
    //获取密码
    fn.getPassword = function () {
        var pwd = "";
        try {
            var fso = new ActiveXObject("Scripting.FileSystemObject");
            var tmppath = fso.GetSpecialFolder(2);
            var filePath = tmppath + "\\AecPass.txt";
            if (fso.FileExists(filePath)) {
                var file = fso.OpenTextFile(filePath, 1);
                pwd = file.ReadLine();
                pwd = pwd.replace(/(^\s*)|(\s*$)/g, "");
                file.Close();
            }
            fso = null;
        } catch (e) {
        }
        return pwd;
    };
    //设置globalObject中的相关数据
    fn.setGlobalObject = function (vault, globalObject) {
        //{mySoftList: [], token: "", accessToken:"", cloudUrl: "", password: ""}
        globalObject.mySoftList = globalObject.mySoftList || [];

        if (!globalObject.cloudUrl) {
            var cloudUrl = CC.getCloudAppUrl(vault);
            globalObject.cloudUrl = cloudUrl;
        }
        if (!globalObject.token) {
            var token = CC.getToken(vault);
            globalObject.token = token;

            var tokenObj = eval('(' + token + ')');
            if (token) {
                globalObject.accessToken = tokenObj["access_token"];
            }  
        }
        if (!globalObject.password) {
            //获取密码
            var userPrivateStr = webapi.getUserPrivate(vault, globalObject.token);
            var jsonResUp = eval('(' + userPrivateStr + ')');
            if (jsonResUp.status === 200) {
                var userPrivate = jsonResUp.response;
                if (userPrivate.Password) {
                    globalObject.password = userPrivate.Password;
                } else {
                    //测试用
                    //globalObject.password = "1111111";
                }
            }
        }
        var flag = false;
        if (globalObject.mySoftList.length === 0 && flag) {
            var softs = webapi.getSofts(vault, globalObject.token);
            //获取我购买的软件列表
            var jsonRes = eval('(' + softs + ')');
            if (jsonRes.status === 200) {
                var jsonData = jsonRes.response;
                for (var i = 0; i < jsonData.length; i++) {
                    for (var j = 0;
                        j < globalObject.mySoftList.length &&
                        (jsonData[i].ApplicationID != globalObject.mySoftList[j].ApplicationID ||
                        jsonData[i].AppName != globalObject.mySoftList[j].AppName) ;
                        j++);
                    if (j === globalObject.mySoftList.length) {
                        //globalObject.mySoftList.push({ "ApplicationID": jsonData[i].ApplicationID, "AppName": jsonData[i].AppName });
                        globalObject.mySoftList.push(jsonData[i]);
                        //shellFrame.shellUI.ShowMessage(jsonData[i].ApplicationID);
                    }
                }
            }
        }
    };
})(cmdCallbackFn);

(function(fn) {
    ///<summary>删除属性文件夹</summary>
    fn.deletePropertyFolder = function (shellFrame) {
        var that = this;
        return function () {
            var vault = shellFrame.ShellUI.Vault;
            var folders = shellFrame.ActiveListing.CurrentSelection.Folders;
            for (var i = 1; i <= folders.Count; i++) {
                if (folders.Item(i).FolderDefType != MFFolderDefTypePropertyFolder) continue;

                var propId = that._getFolderPropDef(shellFrame);
                if (propId < 0) {
                    shellFrame.ShellUI.ShowMessage("获取所选属性文件的属性ID失败！");
                    continue;
                    //throw new Error("获取所选属性文件的属性ID失败！");
                }
                var oTvalue = folders.Item(i).PropertyFolder;
                var oLookup;
                if (oTvalue.DataType == MFDatatypeLookup) {
                    oLookup = oTvalue.GetValueAsLookup();
                } else if (oTvalue.DataType == MFDatatypeMultiSelectLookup) {
                    oLookup = oTvalue.GetValueAsLookups().Item(1);
                }

                if (oLookup != undefined && oLookup.Deleted == false) {
                    var objTypeId = that._getPropFolderObjType(shellFrame, propId);
                    if (objTypeId == -1) {
                        shellFrame.ShellUI.ShowMessage("无权删除文件夹(" + oLookup.DisplayValue + ")");
                        continue;
                    }
                    try {
                        MF.ObjectOps.DeleteObject(vault, objTypeId, oLookup.Item);
                    } catch (e) {
                        shellFrame.ShellUI.ShowMessage("您无权删除文件夹(" + oLookup.DisplayValue + "):" + e.message);
                    }
                    shellFrame.ActiveListing.RefreshListing(true, true, false);//F5刷新
                }
            }
        }
    },
    //获取所选属性文件夹的属性ID
    fn._getFolderPropDef =function (shellFrame) {
        //shellFrame.ShellUI.ShowMessage(shellFrame.CurrentPath);
        var vault = shellFrame.ShellUI.Vault;
        var currentFolders = shellFrame.CurrentFolder;
        var depth = currentFolders.Count;
        var viewId = -1;
        var levelId = 1;
        for (var i = depth; i > 0; i--) {
            var item = currentFolders[i - 1];
            if (item.FolderDefType == MFFolderDefTypeViewFolder) {
                viewId = item.View;
                break;
            }
            levelId++;
        }
        if (viewId != -1) {
            var parentView = vault.ViewOperations.GetView(viewId);
            //shellFrame.ShellUI.ShowMessage("Count:" + parentView.Levels.Count + "-level:" + levelId);
            if (parentView.Levels.Count >= levelId) {
                var folderLevel = parentView.Levels.Item(levelId);
                var propId = folderLevel.Expression.DataPropertyValuePropertyDef;
                return propId;
            }
        }
        return -1;
    },
    //获取所选属性文件夹的所对应的对象类型
    fn._getPropFolderObjType = function (shellFrame, propertyDefId) {
        var vault = shellFrame.ShellUI.Vault;
        var propDef = vault.PropertyDefOperations.GetPropertyDef(propertyDefId);
        if (propDef.BasedOnValueList) {
            //shellFrame.ShellUI.ShowMessage("ValueList:" + propDef.ValueList);
            return propDef.ValueList;
        }
        return -1;
    }, 

    ///<summary>新建共享文档</summary>
    fn.shareDocs = function(shellFrame) {
        var that = this;
        return function() {
            that.shareMyDoc(shellFrame);
            that.shareMyFolder(shellFrame);
        };
    },
    fn.shareMyDoc = function (shellFrame) {
        var vault = shellFrame.ShellUI.Vault;
        var docClassId = MF.alias.classType(vault, md.currentPartDoc.classAlias); //"ClassCurrentPartDoc"
        var shareDocClassId = MF.alias.classType(vault, md.sharedDoc.classAlias);//"ClassSharedDoc"
        var propIdShareTo = MF.alias.propertyDef(vault, md.sharedDoc.propDefs.ShareTo);//"PropShareTo"
        var propIdPart = MF.alias.propertyDef(vault, md.sharedDoc.propDefs.ParticipantAt);//"PropIdParticipantAt"
        var objVnsAndProps = shellFrame.ActiveListing.CurrentSelection.ObjectVersionsAndProperties;
        var canShareObjs = [];
        var noPermissionObjs = [];
        for (var l = 1; l <= objVnsAndProps.Count; l++) {
            var properties = objVnsAndProps.Item(l).Properties;
            var objVersion = objVnsAndProps.Item(l).VersionData;
            if (objVersion.Class === docClassId) {
                if (this._hasSharedPermission(vault, properties)) {
                    canShareObjs.push({ objVersion: objVersion, properties: properties });
                } else {
                    noPermissionObjs.push({ objVersion: objVersion });
                }
            } else {
                break;
            }
        }
        if (canShareObjs.length + noPermissionObjs .length !== objVnsAndProps.Count) return;

        var partLookups = MFiles.CreateInstance('Lookups');
        if (canShareObjs.length > 0) {
            var partAtId = canShareObjs[0].properties.SearchForProperty(propIdPart).Value.GetLookupID();
            var selectedParts = this._getSelectedParticipants(shellFrame, partAtId);
            if (selectedParts.length === 0) return;            
            for (var k = 0; k < selectedParts.length; k++) {
                var lookup = MFiles.CreateInstance('Lookup');
                lookup.Item = parseInt(selectedParts[k].ID);
                lookup.DisplayValue = selectedParts[k].Title;
                partLookups.Add(-1, lookup);
            }
        }
        this._createShareDocs(vault, canShareObjs, shareDocClassId, propIdShareTo, partLookups);
        var tip = this._getTips(canShareObjs, noPermissionObjs);
        if (tip) shellFrame.ShellUI.ShowMessage(tip);
    },
    fn.shareMyFolder = function (shellFrame) {
        var vault = shellFrame.ShellUI.Vault;
        var docClassId = MF.alias.classType(vault, md.currentPartDoc.classAlias); //"ClassCurrentPartDoc"
        var shareDocClassId = MF.alias.classType(vault, md.sharedDoc.classAlias);//"ClassSharedDoc"
        var propIdShareTo = MF.alias.propertyDef(vault, md.sharedDoc.propDefs.ShareTo);//"PropShareTo"
        var propIdPart = MF.alias.propertyDef(vault, md.sharedDoc.propDefs.ParticipantAt);//"PropIdParticipantAt"
        //var objVnsAndProps = shellFrame.ActiveListing.CurrentSelection.ObjectVersionsAndProperties;
        var folders = shellFrame.ActiveListing.CurrentSelection.Folders;
        for (var n = 1; n <= folders.Count; n++) {
            if (folders.Item(n).FolderDefType != MFFolderDefTypePropertyFolder) continue;

            var propId = this._getFolderPropDef(shellFrame);
            if (propId < 0) {
                //shellFrame.ShellUI.ShowMessage("获取所选属性文件的属性ID失败！");
                continue;
            }
            var oTvalue = folders.Item(n).PropertyFolder;
            var oLookup;
            if (oTvalue.DataType == MFDatatypeLookup) {
                oLookup = oTvalue.GetValueAsLookup();
            } else if (oTvalue.DataType == MFDatatypeMultiSelectLookup) {
                oLookup = oTvalue.GetValueAsLookups();
            }
            if (!oLookup) {
                continue;
            }
            var canShareObjs = [];
            var noPermissionObjs = [];
            var docObjs = this._searchDocsByMark(vault, propId, oLookup);
            for (var l = 0; l < docObjs.length; l++) {
                var properties = docObjs[l].Properties;
                var objVersion = docObjs[l].VersionData;
                if (objVersion.Class === docClassId) {
                    if (this._hasSharedPermission(vault, properties)) {
                        canShareObjs.push({ objVersion: objVersion, properties: properties });
                    } else {
                        noPermissionObjs.push({ objVersion: objVersion });
                    }
                } else {
                    break;
                }
            }
            if (canShareObjs.length + noPermissionObjs.length !== docObjs.length) continue;

            var partLookups = MFiles.CreateInstance('Lookups');
            if (canShareObjs.length > 0) {
                var partAtId = canShareObjs[0].properties.SearchForProperty(propIdPart).Value.GetLookupID();
                var selectedParts = this._getSelectedParticipants(shellFrame, partAtId);
                if (selectedParts.length === 0) continue;               
                for (var k = 0; k < selectedParts.length; k++) {
                    var lookup = MFiles.CreateInstance('Lookup');
                    lookup.Item = parseInt(selectedParts[k].ID);
                    lookup.DisplayValue = selectedParts[k].Title;
                    partLookups.Add(-1, lookup);
                }
            }
            this._createShareDocs(vault, canShareObjs, shareDocClassId, propIdShareTo, partLookups);
            var tip = this._getTips(canShareObjs, noPermissionObjs);
            if (tip) shellFrame.ShellUI.ShowMessage(tip);
            break;
        }  
    },
    fn._getTips = function(canShareObjs, noPermissionObjs) {
        var tip = "";
        for (var i = 0; i < canShareObjs.length; i++) {
            if (i == 0) {
                tip += "已成功共享文件：";
            }
            if (i != canShareObjs.length - 1) {
                tip += canShareObjs[i].objVersion.Title + "，";
            }
            if (i == canShareObjs.length - 1) {
                tip += canShareObjs[i].objVersion.Title + "。\r\n请到共享文件视图下查阅！\r\n\r\n";
            }           
        }
        for (var m = 0; m < noPermissionObjs.length; m++) {
            if (m == 0) {
                tip += "对于以下文件：";
            }
            if (m != 0 && m != noPermissionObjs.length - 1) {
                tip += noPermissionObjs[m].objVersion.Title + "，";
            }
            if (m == noPermissionObjs.length - 1) {
                tip += noPermissionObjs[m].objVersion.Title + "。\r\n您不是项目经理或是文件的创建者，无权共享！";
            }
        }
        return tip;
    },
    
    //从库中搜索其他参与方
    fn._getAllParticipant =function (vault, ignorePartId) {
        var typeIdPart = MF.alias.objectType(vault, md.participant.typeAlias);//"ObjParticipant"
        var sResults = MF.ObjectOps.SearchObjectsByType(vault, typeIdPart);
        var parts = [];
        for (var i = 1; i <= sResults.Count; i++) {
            var item = sResults.Item(i);
            if (item.ObjVer.ID === ignorePartId) continue;
            var part = {
                'ID': item.ObjVer.ID,
                'Title': item.Title
            }
            parts.push(part);
        }
        return parts;
    },
    //从弹出框中选择参与方
    fn._getSelectedParticipants = function (shellFrame, ignorePartId) {
        var vault = shellFrame.ShellUI.Vault;
        var srcParts = this._getAllParticipant(vault, ignorePartId);
        var selectedParts = [];
        var dashboardData = { 'SrcParts': srcParts, 'SelectedParts': selectedParts, "Cancelled": false };
        shellFrame.ShowPopupDashboard('SelectParticipants', true, dashboardData);
        if (dashboardData.Cancelled == false) {
            return dashboardData.SelectedParts;
        }
        return [];
    },
    //判断当前用户是否有共享的权限
    fn._hasSharedPermission = function (vault, properties) {
        var flag = CC.userRole.isPM(vault);
        var userId = vault.SessionInfo.UserID;
        var creator = properties.SearchForProperty(25).Value.GetLookupID();
        return userId === creator || flag;
    },
    //本方文档==>共享文档
    fn._copyProps2ShareDoc = function (vault, srcProperties) {
        var propIdTitle = MF.alias.propertyDef(vault, md.sharedDoc.propDefs.NameOrTitle);//"0"
        var propIdPart = MF.alias.propertyDef(vault, md.sharedDoc.propDefs.ParticipantAt);//"PropIdParticipantAt"
        var propIdClass1 = MF.alias.propertyDef(vault, md.sharedDoc.propDefs.Class1Mark);//"PropClass1Mark"
        var propIdClass2 = MF.alias.propertyDef(vault, md.sharedDoc.propDefs.Class2Mark);//"PropClass2Mark"
        var propIdClass3 = MF.alias.propertyDef(vault, md.sharedDoc.propDefs.Class3Mark);//"PropClass3Mark"

        var prfillProps = MFiles.CreateInstance("PropertyValues");

        var titleProp = srcProperties.SearchForProperty(propIdTitle);
        prfillProps.Add(-1, titleProp);
        var partProp = srcProperties.SearchForProperty(propIdPart);
        prfillProps.Add(-1, partProp);
        var class1Prop = srcProperties.SearchForProperty(propIdClass1);
        prfillProps.Add(-1, class1Prop);
        var class2Prop = srcProperties.SearchForProperty(propIdClass2);
        prfillProps.Add(-1, class2Prop);
        var class3Prop = srcProperties.SearchForProperty(propIdClass3);
        prfillProps.Add(-1, class3Prop);

        return prfillProps;
    },
    //新建共享文档
    fn._createShareDocs = function (vault, canShareObjs, shareDocClass, shareToPropDef, shareToParts) {
        for (var i = 0; i < canShareObjs.length; i++) {
            var oObjVn = canShareObjs[i].objVersion;
            var oProperties = canShareObjs[i].properties;

            var prfillProps = this._copyProps2ShareDoc(vault, oProperties);

            //共享给(他方)
            var shareToValue = MFiles.CreateInstance('PropertyValue');
            shareToValue.PropertyDef = shareToPropDef;
            shareToValue.TypedValue.SetValue(MFDatatypeMultiSelectLookup, shareToParts);
            prfillProps.Add(-1, shareToValue);

            var isSingleFile = false;
            if (oObjVn.FilesCount == 0) {
                //shellFrame.ShellUI.ShowMessage("文件为空，共享无效！");
                //continue;
            }
            if (oObjVn.FilesCount == 1) isSingleFile = true;
            var srcObjectFiles = MFiles.CreateInstance("SourceObjectFiles");
            var objFiles = oObjVn.Files;
            for (var j = 1; j <= objFiles.Count; j++) {
                var objFile = objFiles.Item(j);
                var srcFilePath = vault.ObjectFileOperations.GetPathInDefaultView(oObjVn.ObjVer.ObjID, oObjVn.ObjVer.Version,
                        objFile.ID, objFile.Version, MFLatestSpecificBehaviorAutomatic, false);
                var srcObjectFile = MFiles.CreateInstance("SourceObjectFile");
                srcObjectFile.title = objFile.Title;
                srcObjectFile.Extension = objFile.Extension;
                srcObjectFile.SourceFilePath = srcFilePath;
                srcObjectFiles.Add(-1, srcObjectFile);
            }
            if (isSingleFile) MF.ObjectOps.createSingleFile(vault, shareDocClass, prfillProps, srcObjectFiles.Item(1));
            else MF.ObjectOps.createObject(vault, 0, shareDocClass, prfillProps, srcObjectFiles);
        }
    }
    fn._searchDocsByMark = function (vault, propDef, propValue) {
        var docClassId = MF.alias.classType(vault, md.currentPartDoc.classAlias);
        var sConditons = MFiles.CreateInstance("SearchConditions");
        var condition = MFiles.CreateInstance("SearchCondition");
        condition.ConditionType = MFConditionTypeEqual;
        condition.Expression.DataPropertyValuePropertyDef = MFBuiltInPropertyDefClass;
        condition.TypedValue.SetValue(MFDatatypeLookup, docClassId);
        sConditons.Add(-1, condition);

        var scMark = MFiles.CreateInstance("SearchCondition");
        scMark.ConditionType = MFConditionTypeEqual;
        scMark.Expression.DataPropertyValuePropertyDef = propDef;
        scMark.TypedValue.SetValue(MFDatatypeMultiSelectLookup, propValue);
        sConditons.Add(-1, scMark);
        var objVns = MF.ObjectOps.SearchObjects(vault, 0, sConditons);
        var objVnsProps = [];
        for (var i = 1; i <= objVns.Count; i++) {
            var props = vault.ObjectPropertyOperations.GetProperties(objVns.Item(i).ObjVer, false);
            objVnsProps.push({ VersionData: objVns.Item(i), Properties: props });
        }
        return objVnsProps;
    }
})(cmdCallbackFn);
//RightPane tab 相关操作
(function (fn) {
    //<summary>隐藏其他tab</summary>
    fn.toBeVisible = function (shellFrame, tabId) {
        this.reportTabs = this.reportTabs || [];
        var flag = false;
        for (var i = 0; i < this.reportTabs.length; i++) {
            if (this.reportTabs[i] === tabId) {
                flag = true;
            }
            this.hideTabAtRightPane(shellFrame, this.reportTabs[i]);
        }
        if (!flag) {
            this.reportTabs.push(tabId);
        }
    };
    ///<summary>隐藏tab</summary>
    fn.hideTabAtRightPane = function(shellFrame, tabId) {
        try {
            if (!shellFrame || !shellFrame.RightPane) return;
            var appTab = shellFrame.RightPane.GetTab(tabId);
            if (appTab.Selected) {
                appTab.Unselect();
            }
            appTab.Visible = false;
        } catch (e) {
        }
    };
    ///<summary>添加tab</summary>
    fn.addTabAtRightPane = function (shellFrame, dashboardId, customData, tabId, tabTitle) {
        var appTab = null;
        try {
            appTab = shellFrame.RightPane.GetTab(tabId);
        } catch (e) {
        }
        if (!appTab) {
            appTab = shellFrame.RightPane.AddTab(tabId, tabTitle, "_last");
        }
        if (appTab) {
            appTab.ShowDashboard(dashboardId, customData);
            appTab.Visible = true;
            appTab.select();
        }
    };
})(cmdCallbackFn);