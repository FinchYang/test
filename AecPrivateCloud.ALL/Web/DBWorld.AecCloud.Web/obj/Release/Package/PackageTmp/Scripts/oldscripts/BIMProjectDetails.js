/****
    main: webClient项目概况js
****/

var tempMembersDate;//用于存储项目成员信息的临时字符串，便于后边显示成员信息使用
var partyId;//用于存储邀请成员时候的 参与方ID
var projectId;
var projObj;
var accountList;
var sels = new Array("工长", "安全员", "安全总监", "材料员", "材料主管", "财务会计", "财务经理", "测量师", "测量员", "行管主任", "劳务员", "商务经理", "施工员","试验员",
    "项目部", "项目副经理", "项目经理", "项目经理（安装）", "项目书记", "项目总工程师", "造价员", "质量总监", "总承包项目经理", "门卫", "预算员", "见习生", "合约经理"
    , "成本经理", "造价师", "质检员", "资料员", "外协员", "栋号长", "保管员", "安全主管", "安装施工", "一般成员");
var useridForRoleSelect;

$(function () {
    projectId = getUrlParam("projectId");
    $("#imghead").attr("src", "/BIM/GetImage?projectId=" + projectId);
    LoadDetail(projectId);

    LoadMembers(projectId);
    //预览图片
    $("#previewDiv").on("click", "#imghead", function () {
        $("#imgSrc").val("");
        $("#imgUp").click();
    });

    //项目概况点击
    $("#projectDetail").click(function () {
        return false;
    });
    //三张默认图片点击事件
    $(".projectCoverChoose").on("click", "td img:not([id='imghead'])", function () {
        //先清空 file 的值
        var file = $("#imgUp");
        file.after(file.clone().val(""));
        file.remove();
        $("#imgSrc").val($(this).attr("src"));
        $("#preview img").attr("src", $(this).attr("src"));
    });


    //发送邀请
    $(".InvitateBtn").on("click", "#inviteMember", function () {
        
        var inviteeArr = [];
        var accIs = $('#userAccounts').multipleSelect('getSelects');
        if (!accIs.length) {
            alert("请选择待邀请成员账户！");
            return;
        }
        if (!accountList) return;
        $.each(accIs, function(i, item) {
            inviteeArr.push(accountList[item]);
        });
        //alert(JSON.stringify(inviteeArr));       

        if (projObj && !projObj.HasParty) {
            partyId = 0;
        }
        var projectName = $(".currentProject").val();
        var projectId = $("input[id^=project]").attr("id");
        var partyName = $("#chooseParty").val();
        projectId = projectId.substring(8);
        try {
            $(".btn-wrap input").attr('disabled', 'disabled');

            $.ajax({
                type: "POST",
                url: "/BIM/InviteMembers2Project",
                data: {
                    ProjectId: projectId,
                    ProjectName: projectName,
                    PartyId: partyId,
                    PartyName: partyName,
                    inviteeAccounts: inviteeArr
                },
                async: false,
                success: function (data) {
                    if (data === "success") {
                        alert("邀请成功！");
                        $(".popupInvitateTitle p").html("邀请已成功！");
                        $(".popupInvitate ").hide();
                        $(".popupWrapBg").hide();
                        $(".popupInvitateTitle p").html("");
                    } else {
                        alert(data);
                        $(".popupInvitateTitle p").html(data);
                    }
                }
            });
        } catch (e) {
            alert(e.message);
        } finally {
            $(".btn-wrap input").removeAttr('disabled');
            //刷新页面
            location.reload(false);

            //location.replace("/BIM/ProjectDetail?projectId=" + projectId);
        }      
    });

    //更改当前项目详情
    $(".changeChoose img").click(function () {
        $(".changeProjectList").toggle();
    });
    //替换当前项目名称
    $(".changeProjectList").on("click", "ul li", function () {
        var currentText = $(this).text();
        var changeInput = $(".changeInput").val();
        if (currentText != changeInput) {
            $(".affirmProject").show();
            $(".changeProjectList").hide();
            $(".affirmProjectYes").click(function() {
                $(".affirmProject").hide();
                $(".changeInput").val(currentText);
            });
        } else {
            $(".changeProjectList").hide();
        };
    });
    $(".affirmProjectNo").click(function () {
        $(".affirmProject").hide();
    });


    //添加经理弹窗
    $(".moreManager").click(function () {
        $(".popuoManager").css({ "left": $(this).offset().left, "top": $(this).offset().top + 45 }).show();
    });
    //选择经理
    $(".popuoManager").on("click", "li", function () {
        if ($(this).find("div").length == "0") {
            $(this).append('<div class="chooseManager"><img src="/Content/Images/newProjectClose.png" width="12" height="12" /></div>');
        } else {
            $(this).find("div").remove();
        }
    });


    //添加成员弹窗
    $(".moreMember").click(function () {
        //$('#popupInvitate').show();
        $(window).scrollTop("0");
        //projObj.HasParty = false;
        if (projObj.HasParty) {
            $("#partiesDiv").show();
            $.post("/BIM/GetAllParties", function(data) {
                if (data != "fail") {
                    var res = strToJson(data);
                    var tempStr = "";
                    $.each(res, function(i, item) {
                        tempStr += "<li id='party_" + item.Id + "'>" + item.Name + "</li>";
                    });
                    $("#partiesDiv ul").html(tempStr);
                    $("#partiesDiv #chooseParty").val($("#partiesDiv ul li:first").html());
                    partyId = $("#partiesDiv ul li:first").attr("id").substring(6);
                }
            });
        } else {
            $("#partiesDiv").hide();
            partyId = 0;
        }        
        $('#userAccounts').multipleSelect({
            filter: true,
            selectAll: false
        });
        $.post("/BIM/GetMfAccountList", { projectId: projectId }, function (data) {
            if (data.length) {
                data = data.sort(function (a, b) {
                    return a.FullName.localeCompare(b.FullName);
                });
                accountList = data;
                $('#userAccounts').empty();
                for (var i = 0; i < data.length; i++) {
                    var fullName = data[i].UserName;
                    if (data[i].FullName) {
                        fullName = data[i].FullName;
                    }
                    fullName += "(" + data[i].AccountName + ")";
                    var op = "<option value='" + i.toString() + "'>"
                        + fullName + "</option>";
                    $('#userAccounts').append(op);
                }
            }
            $('#userAccounts').multipleSelect("refresh");
        });       

        $(".popupInvitate").css({ "top": ($(window).height() - $(".popupInvitate").height()) / 2, "left": ($(window).width() - 350) / 2 }).show();
        $(".popupWrapBg").css("height", $(document).height()).show();
    });

    //添加成员时选择参与方
    $("#partiesDiv02").on("click", " ul li", function () {
        $("#partiesDiv #chooseParty").val($(this).html());
        partyId = $(this).attr("id").substring(6);
        $("#partiesDiv02 ul").close();
    });

    //添加成员弹窗里面的成员列表
    var width = $(".InvitateMemberList").width();
    $(".prevInvitate").click(function () {
        $(this).parent().prev().find("ul").css({ marginLeft: 0 }).find("li:last").prependTo($(this).parent().prev().find("ul"));
    });
    $(".nextInvitate").click(function () {
        $(this).parent().prev().find("ul").find("li:first").appendTo($(this).parent().prev().find("ul"));
    });

    //小组成员弹窗
    $(".groupContent").on("click", ".groupListMember", function () {
        $(".groupMemberWrap").css({ "left": $(this).offset().left, "top": $(this).offset().top + 25 }).show();
    });

    //查看小组成员信息--并设置项目岗位信息！
    $("#membersList2").on("click", "a[id^=userId2_]", function () {
      //  console.log("111");
        var userId2 = $(this).attr("id").substring(8);
        useridForRoleSelect = userId2;
        $(".userData").css({
            left: ($(window).width() - $(".userData").width()) / 2 ,
            top: $(".userData").height()/2
        }).show();
        $(".popupWrapBg").css("height", $(document).height()).show();
        var res = strToJson(tempMembersDate);
        var tempStr = "";
       
        $.each(res, function(i, item) {
            if (item.Id == userId2) {
                tempStr += "<div class='userDataHead'><div class='userDataImg'><img src='/BIM/GetUserImage?userId=" + userId2 + "' width='184' height='149' /></div></div>"
                    + "<div class='columnWrap clearfix'><div class='columnLeft left'>工号</div><div class='columnRight columnEmail left' id='UserNameForRoleSelect'>" + (item.UserName == null ? "空" : item.UserName) + "</div></div>"
                      + "<div class='columnWrap clearfix'><div class='columnLeft left'>姓名</div><div class='columnRight columnEmail left' >" + (item.FullName == null ? "空" : item.FullName) + "</div></div>"
                        + "<div class='columnWrap clearfix'><div class='columnLeft left'>电话</div><div class='columnRight columnEmail left'>" + (item.Phone == null ? "空" : item.Phone) + "</div></div>"
                    + "<div class='columnWrap clearfix'><div class='columnLeft left'>邮箱</div><div class='columnRight columnEmail left'>" + (item.Email == null ? "空" : item.Email) + "</div></div>"
                  
                    + "<div class='columnWrap clearfix'><div class='columnLeft left'>QQ</div><div class='columnRight left'>" + (item.QQ == null ? "空" : item.QQ) + "</div></div>"
                    + "<div class='columnWrap clearfix'><div class='columnLeft left'>单位</div><div class='columnRight left'>" + (item.Company == null ? "空" : item.Company) + "</div></div>"
                    + "<div class='columnWrap clearfix'><div class='columnLeft left'>部门</div><div class='columnRight left'>" + (item.Department == null ? "空" : item.Department) + "</div></div>"
                    + "<div class='columnWrap clearfix'><div class='columnLeft left'>职位</div><div class='columnRight left'>" + (item.Post == null ? "空" : item.Post) + "</div></div>"
                    + "<div class='columnWrap clearfix'><div class='columnLeft left'>备注</div><div class='columnRight left'>" + (item.Description == null ? "空" : item.Description) + "</div></div>";
                //+ " <div class='roleselect'><span>请选择：<br>项目岗位：</span><select id='roles' name='ss'class='roleselect' multiple='multiple'>" +
                //" <option value='1'>工长</option>" +
                // " <option value='2'>安全员</option>" +
                //  " <option value='3'>安全总监</option>" +
                //   " <option value='4'>材料员</option>" +
                //    " <option value='5'>材料员</option>" +
                //"</select>" +
                //"<input type='button' name='Submit'class='roleselected' value='提交' />"+
                //"</div>";
            }
        });
        //var selstr = " <span>请选择：<br>项目岗位：</span><select id='roles' name='ss'class='roleselect' multiple='multiple'>" +
        //    " <option value='1'>工长</option>" +
        //    " <option value='2'>安全员</option>" +
        //    " <option value='3'>安全总监</option>" +
        //    " <option value='4'>材料员</option>" +
        //    " <option value='5'>材料员</option>" +
        //    "</select>" +
        //    "<input type='button' name='Submit'id='roleselected' value='提交' />";
        //$("#selectrolesdiv").html(selstr);
        $('#roles').empty();
        //var op = "<option value='" + 0 + "'>"
        //         + "请选择" + "</option>";
        //$('#roles').append(op);
        for (var i = 0; i < sels.length; i++) {
           var  op = "<option value='" + i.toString() + "'>"
                + sels[i] + "</option>";
            $('#roles').append(op);
        }
        $('#roles').multipleSelect("refresh");
       
        $(".userDataContent").html("");
        $(".userDataContent").html(tempStr);
      //  $('.ms-parent').style.width = '180px';
    });
    $("#selectrolesdiv").on("click", "#roleselected", function () {
        
        var accIs = $('#roles').multipleSelect('getSelects');
        if (!accIs.length) {
            alert("请选择项目岗位！");
            return;
        }
        var selrole = [];
        $.each(accIs, function (i, item) {
            selrole.push(sels[item]);
            console.log("roleselected："+item);
        });
        console.log("before ajax：" );
        try {
            
            //var uname = $("#UserNameForRoleSelect").innerHTML;
            //console.log(uname);
            //console.log($("#UserNameForRoleSelect").gette);
            $.ajax({
                type: "POST",
                url: "/BIM/ConfirmProjectRoles",
                data: {
                    ProjectId: projectId,
                    userid: useridForRoleSelect,
                    Roles: selrole
                },
                async: false,
                success: function (data) {
                    if (data === "success") {
                        alert("设置成功！");
                       
                        $(".popupWrap").hide();
                    } else {
                        alert(data);
                    }
                }
            });
        } catch (e) {
            alert(e.message);
        } finally {
            location.reload(false);
        }
        console.log("after ajax：");
    });

    $(".showProject").on("click", ".editSave", function () {
        if ($(this).val() == "编辑") {
            $(".showProject").hide();
            $(".editProject").show();
        } else {
            $("#projectEditForm").submit(function () {
                $.ajax({
                    url: "BIM/UpdateProject",
                    type: "Post",
                    data: $("#projectEditForm").serialize(),
                    success: function (data) {
                        alert(data);
                    }
                });
            });
            $("#projectEditForm").submit();
            $(".showProject").show();
            $(".editProject").hide();
        }
    });

   

    //放弃编辑
    $(".editProject").on("click", ".editCancel", function () {
        $(".showProject").show();
        $(".editProject").hide();
    });
    //高级设置
    $(".seniorShow").click(function () {
        $(".seniorEdit").css({ left: ($(window).width() - 375) / 2 }).show();
        $(".popupWrapBg").show();
    });
    //自定义封面点击事件
    $("#customImg").click(function () {
        $("#imgUp").click();
    });

    $("#imgSubmit").click(function () {
        $("#projectEditForm").submit(function() {
            $.ajax({
                url: "BIM/UpdateProject",
                type: "Post",
                data: $("#projectEditForm").serialize(),
                success: function(data) {
                    alert(data);
                }
            });
        });
        $("#projectEditForm").submit();
    });

    ////删除某个组
    //$(".groupContent").on("mouseover mouseleave", ".groupListMember", function (event) {
    //    if (event.type == "mouseover") {
    //        var currentDiv = $(this);
    //        //鼠标悬浮
    //        var closeUser = $('<span><img src="/Content/Images/uesrCloseImg.gif" /></span>');
    //        if ($(this).find("span").length == "0") {
    //            $(this).append(closeUser);
    //            $(".groupListMember span").click(function () {
    //                if (confirm("确认要删除该小组吗?")) {
    //                    var groupId = currentDiv.attr("id").substring(6);
    //                    $.post("/BIM/RemoveUserGroup", { projectId: projectId, groupId: groupId }, function (data) {
    //                        if (data == "success") {
    //                            GetMemberGroups(projectId);
    //                        } else {
    //                            alert("删除失败！");
    //                        }
    //                    });
    //                }
    //            });
    //        };
    //    } else if (event.type == "mouseleave") {
    //        //鼠标离开
    //        $(this).find("span").remove();
    //    }
    //});

    ////添加小组
    //$(".groupMore").click(function () {
    //    var groupListDiv = $('<div class="groupList left"><input value="" type="text" /><a href="javascript:void(0);" title="确定">确定</a></div>');
    //    $(this).prev(".groupContent").append(groupListDiv);
    //    $(".groupList").on("click", "a", function () {
    //        if ($(this).prev().val().trim() != "") {
    //            if (confirm("确认添加该小组吗?")) {
    //                var groupName = $(this).prev().val();
    //                $.post("/BIM/AddUserGroup", { projectId: projectId, groupName: groupName }, function (data) {
    //                    if (data == "success") {
    //                        GetMemberGroups(projectId);
    //                    } else {
    //                        alert("删除失败！");
    //                    }
    //                });
    //            }
    //        }
    //    });
    //});

    var currentGroupId;
    var currentGroupDiv;

    //显示小组成员
    $(".groupContent").on("click", ".groupListMember", function () {
        var groupId = $(this).attr("id").substring(6);
        currentGroupDiv = $(this);
        //先获取小组中所有成员，转成数组，然后显示项目中所有成员，和数组对比，如果有，checked变为选择，即代表小组中有该成员
        var array = new Array();
        $.ajax({
            url: "/BIM/GetUsernamesInGroup",
            type: "Post",
            async: false,
            data: { projectId: projectId, groupId: groupId },
            success: function (data) {
                var res = strToJson(data);
                $.each(res, function(i, item) {
                    array.push(item);
                });
            }
        });

        currentGroupId = groupId;
        $.post("/BIM/GetProjectMembers", { projectId: projectId }, function (data) {
            var res = strToJson(data);
            var tempStr = "";
            $.each(res, function (i, item) {
                if (array.indexOf(item.UserName) == -1) {
                    tempStr += "<li><input type='checkbox' class='left' /><a href='javascript:void(0)' title='" + item.UserName + "'>" + reduceStr(item.UserName) + "</a></li>";
                } else {
                    tempStr += "<li><input type='checkbox' class='left' checked='checked' /><a href='javascript:void(0)' title='" + item.UserName + "'>" + reduceStr(item.UserName) + "</a></li>";
                }
            });
            $(".groupMemberWrap ul").html(tempStr);
        });

        $(".groupMemberWrap").css({ "left": $(this).offset().left, "top": $(this).offset().top + 25 }).show();
    });

    ////添加或删除某个成员从某个组里
    //$(".groupMemberWrap").on("click", "input[type=checkbox]", function () {
    //    var userName = $(this).next().html();
    //    if (!$(this).prop("checked")) {
    //        if (confirm("确认要删除该成员吗?")) {
    //            $.post("/BIM/RemoveUsersFromGroup", { projectId: projectId, groupId: currentGroupId, userName: userName }, function (data) {
    //                if (data == "success") {
    //                    alert("删除成功！")
    //                } else {
    //                    alert("删除失败！")
    //                }
    //                currentGroupDiv.trigger("click");
    //            });

    //        }
    //    } else {
    //        if (confirm("确认要添加该成员吗?")) {
    //            $.post("/BIM/AddUserToGroup", { projectId: projectId, groupId: currentGroupId, userName: userName }, function (data) {
    //                if (data == "success") {
    //                    alert("添加成功！")
    //                } else {
    //                    alert("添加失败！")
    //                }
    //                currentGroupDiv.trigger("click");
    //            });

    //        }
    //    }
    //})

    //GetMemberGroups(projectId);
});

////获取项目小组集合
//function GetMemberGroups(projectId) {
//    $.post("/BIM/GetMemberGroups", { projectId: projectId }, function (data) {
//        var res = strToJson(data);
//        var tempStr = "";
//        $.each(res, function (i, item) {
//            tempStr += "<div class='groupList groupListMember left' id='group_" + item.Id + "'>" + item.Name + "</div>";
//        })
//        $("#groupDiv span").html(tempStr);
//    });
//}

////获取某个成员从某个小组
//function RemoveUsersFromGroup(projectId) {
//    $.post("/BIM/RemoveUsersFromGroup", { projectId: projectId }, function (data) {
//        var res = strToJson(data);
//        var tempStr = "";
//        var tempStr2 = "";
//        $.each(res, function (i, item) {
//            tempStr = "<li><a href='javascript:void(0);' title='" + item.UserName + "' id='userId_" + item.Id + "'><img src='/Content/Images/userImg.gif' width='45' height='45' alt='用户头像' />"
//                  + "<span>" + item.UserName + "</span></a></li>";
//            tempStr2 += "<a href='#' title='" + item.UserName + "' id='userId2_" + item.Id + "'><img src='/Content/Images/userImg.gif' width='45' height='45' alt='用户头像' /><p>" + item.UserName + "</p></a>"
//        })
//        $("#membersList").append(tempStr);
//        $("#membersList2 span").append(tempStr2);
//    });
//}

////添加成员到某个小组
//function AddUserToGroup(projectId) {
//    $.post("/BIM/AddUserToGroup", { projectId: projectId }, function (data) {
//        var res = strToJson(data);
//        var tempStr = "";
//        var tempStr2 = "";
//        $.each(res, function (i, item) {
//            tempStr = "<li><a href='javascript:void(0);' title='" + item.UserName + "' id='userId_" + item.Id + "'><img src='/Content/Images/userImg.gif' width='45' height='45' alt='用户头像' />"
//                  + "<span>" + item.UserName + "</span></a></li>";
//            tempStr2 += "<a href='#' title='" + item.UserName + "' id='userId2_" + item.Id + "'><img src='/Content/Images/userImg.gif' width='45' height='45' alt='用户头像' /><p>" + item.UserName + "</p></a>"
//        })
//        $("#membersList").append(tempStr);
//        $("#membersList2 span").append(tempStr2);
//    });
//}
function reduceStr(str) {
    var index = str.indexOf('-');
    if (index !== -1) {
        var str1 = str.substr(index + 1);
        var index2 = str1.indexOf('-');
        if (index2 !== -1) {
            str = str1;
        }
    }
    var len = 4;
    if (str.length <= len) {
        return str;
    }
    return str.substr(0, len) + "..";
}

//加载项目成员列表
function LoadMembers(projectId) {
    $.post("/BIM/GetProjectMembers", { projectId: projectId }, function (data) {
        tempMembersDate = data;
        var res = strToJson(data);
        var tempStr = "";
        var tempStr2 = "";
        $.each(res, function (i, item) {
            // var fullName = item.UserName;
            var fullName = item.FullName;
            console.log("username=" + item.UserName +  ",item.FullName=" + item.FullName);
            //if (item.Profile && item.Profile.FullName) {
            //    fullName = item.Profile.FullName;
            //}
            tempStr = "<li><a href='javascript:void(0);' title='" + fullName + "' id='userId_" + item.Id + "'><img src='/BIM/GetUserImage?userId=" + item.Id + "' width='81' height='81' alt='用户头像' />"
                + "<div class='name-display'>" + fullName + "</div></a></li>";
            tempStr2 += "<a href='#' title='" + fullName + "' id='userId2_" + item.Id + "'><img src='/BIM/GetUserImage?userId=" + item.Id + "' width='81' height='81' alt='用户头像' /><div class='name-display'>" + reduceStr(fullName) + "</div></a>";
        });
        $("#membersList").append(tempStr);
        $("#membersList2 span").append(tempStr2);
    });
}

//加载项目详情
function LoadDetail(projectId) {
    $.post("/BIM/GetProjectDetail", { id: projectId }, function (data) {
        var res = strToJson(data);
        projObj = res;
        $(".currentProject").val(res.Name);
        if (res.HasParty) {
            $('#partiesDiv').show();
        } else {
            $('#partiesDiv').hide();
            partyId = 0;
        }
        var tempStr = "<input type='hidden' value='" + res.Id + "' id='project_" + res.Id + "'>"
            + "<div class='columnWrap clearfix'><div class='create-proj-label'>项目名称</div><div class='columnRight left proj-detail-info'>" + res.Name + "</div></div>"
             + "<div class='columnWrap clearfix'><div class='create-proj-label'>项目编号</div><div class='columnRight left proj-detail-info'>" + res.Number + "</div></div>"
              + "<div class='columnWrap clearfix'><div class='create-proj-label'>项目类别</div><div class='columnRight left proj-detail-info'>" + res.ProjectClass + "</div></div>"
               + "<div class='columnWrap clearfix'><div class='create-proj-label'>项目等级</div><div class='columnRight left proj-detail-info'>" + res.Level + "</div></div>"
             + "<div class='columnWrap clearfix'><div class='create-proj-label'>所在地区</div><div class='columnRight left proj-detail-info'>" + res.Area + "</div></div>"
              + "<div class='columnWrap clearfix'><div class='create-proj-label'>建设规模</div><div class='columnRight left proj-detail-info'>" + res.ConstructionScale + "</div></div>"
               + "<div class='columnWrap clearfix'><div class='create-proj-label'>合同金额</div><div class='columnRight left proj-detail-info'>" + res.ContractAmount + "</div></div>"
              + "<div class='columnWrap clearfix'><div class='create-proj-label'>所属公司</div><div class='columnRight left proj-detail-info'>" + res.Company + "</div></div>"
             + "<div class='columnWrap clearfix'><div class='create-proj-label'>项目描述</div><textarea class='columnRight describeHeight left textarea-info' readonly='readonly'>" + (res.Description == null ? "" : res.Description) + "</textarea></div>"
             + "<div class='columnWrap clearfix'><div class='create-proj-label'>起止日期</div><div class='columnRight left proj-detail-info-date'>" + DateOper(res.StartDateUtc) + "~" + DateOper(res.EndDateUtc) + "</div></div>"
             + "<div class='columnWrap clearfix'><div class='create-proj-label'>项目状态</div><div class='columnRight left proj-detail-info-status'>" + res.Status.Name + "</div></div>"
             + "<div class='columnWrap clearfix'><div class='create-proj-label'>建设单位</div><div class='columnRight left proj-detail-info'>" + (res.OwnerUnit == null ? "" : res.OwnerUnit) + "</div></div>"
              + "<div class='columnWrap clearfix'><div class='create-proj-label'>项管单位</div><div class='columnRight left proj-detail-info'>" + (res.PmUnit == null ? "" : res.PmUnit) + "</div></div>"
               + "<div class='columnWrap clearfix'><div class='create-proj-label'>勘察单位</div><div class='columnRight left proj-detail-info'>" + (res.InvestigateUnit == null ? "" : res.InvestigateUnit) + "</div></div>"
             + "<div class='columnWrap clearfix'><div class='create-proj-label'>设计单位</div><div class='columnRight left proj-detail-info'>" + (res.DesignUnit == null ? "" : res.DesignUnit) + "</div></div><div class='columnWrap clearfix'>"
               //     + "<div class='columnLeft left'>施工单位</div><div class='columnRight left'>" + (res.ConstructionUnit == null ? "" : res.ConstructionUnit) + "</div></div><div class='columnWrap clearfix'>"
             + "<div class='create-proj-label'>监理单位</div><div class='columnRight left proj-detail-info'>" + (res.SupervisionUnit == null ? "" : res.SupervisionUnit) + "</div></div>";
        $(".showProject").html(tempStr);
    });
}
//<div class='popupBtn'><input type='submit' class='editSave' value='编辑' /></div>
//去掉日期中的时间
function DateOper(dateStr) {
    var s = new Date(dateStr);
    var str = s.getFullYear() + "-" + (s.getMonth() + 1) + "-" + s.getDate();
    return str;
}

