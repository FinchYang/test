/****
    main: webClient首页JS
****/

$(function () {
    //获取项目详情
    //GetProjects();

    //获取项目类别
    GetCategories();

    //绑定项目封面的点击事件到 file的点击事件
    $("#previewDiv").on("click", "#imghead", function () {
        $("#imgUp").click();
    });


    //默认图片
    $("#preview img").attr("src", $("#imgSrc").attr("value"));//imgSrc
    //三张默认图片点击事件
    $(".projectCoverChoose").on("click", "td img:not([id='imghead'])", function () {
        //先清空 file 的值
        var file = $("#imgUp");
        file.after(file.clone().val(""));
        file.remove();
        $("#preview img").attr("src", $(this).attr("src"));
        $("#imgSrc").val($(this).attr("src"));
    });

    //其他项目隐藏显示
    $(".hideShow").click(function () {
        if ($(this).text() == "隐藏>>") {
            $(".otherProject").hide();
            $(this).text("显示>>").attr("title", "显示>>");
        } else {
            $(".otherProject").show();
            $(this).text("隐藏>>").attr("title", "隐藏>>");
        }
    });


    //项目列表li的鼠标移入移出事件
    $(".projectContent ul li").hover(function () {
        //$(this).children(".hoverShow").show();
        $(this).children(".projectListWrap").css("borderColor", "#0f95f4");
        //$(this).children(".projectName a").css("color", "#0f95f4")
        $(this).children(".projectName").find("a").css("color", "#0f95f4");

    }, function () {
        $(".projectListWrap").css("borderColor", "#e0e0e0");
        $(this).children(".projectName").find("a").css("color", "#7a7a7a");
        //$(".hoverShow").hide();
        $(".projectListBtnConfirm").hide();

    });


    var chooseProjectStatu;
    var chooseProjectId;
    //项目状态操作

    $(".projectListBtnConfirm a").click(function () {
        $(".projectListBtnConfirm").fadeOut("fast");
        if ($(this).attr("name") == "sure") {
            var state = "";
            switch (chooseProjectStatu.trim()) {
                case "启动":
                    state = "start";
                    break;
                case "暂停":
                    state = "pause";
                    break;
                case "结束":
                    state = "end";
                    break;
                case "立项":
                    state = "proposal";
                    break;
                default:
                    break;
            }
            $.post("/BIM/ChangeProjectStatu", { projectId: chooseProjectId, oprType: state }, function (data) {
                if (data == "success") {
                    location.reload();
                } else {
                    var res = strToJson(data);
                    alert(res.Message);
                }
            });
        }
    });

    //归档、立项等小按钮的点击后记录下点击的项目id和操作的动作是什么
    $(".projectListBtn dt").click(function () {
        //$(".hoverShowConfirm").css("top", $(this).offset().top - $(this).parent(".hoverListBtn").offset().top).show();
        chooseProjectStatu = $(this).html();
        chooseProjectId = $(this).closest("li[id^=project_]").attr("id").substring(8);

    });

    //选择参与方
    $("#partiesDiv").on("click", " ul li", function () {
        $("#partiesDiv #chooseParty").val($(this).html());
        $("#partyId").val($(this).attr("id").substring(6));
        $("#partiesDiv ul").close();
    });

    $(".newProjectClose").click(function () {
        $(".newProject").hide();
        $(".popupWrapBg").hide();
    });

    var templatePath = '/Content/Images/projects/projectTemplete.gif';

    $("#templatesTable").on("click", ".projectTempleteContent", function () {
        var projectTempleteImg = $('<div class="projectTempleteImg"><img src="'+templatePath+'" /></div>');
        $(this).parents(".projectTemplete").find(".projectTempleteImg").remove();
        $(this).append(projectTempleteImg);
        var strs = $(this).attr("id").split('_');
        var tempId = strs[1];
        $("#templateId").val(tempId);
        //alert($("#templateId").val());
        if (strs.length > 2 && parseInt(strs[2]) === 0) {
            $('#projectparty').hide();
        } else {
            $('#projectparty').show();
        }
    });



    //更改头像
    $(".userHeadFile").click(function () {
        $("#userHeadFile").click();
    });
    //个人信息面板
    $(".userInformation").click(function () {
        $(".userData").css("left", ($(window).width() - 375) / 2).show();
    });
    $(".groupWrap").on("click", ".addGroup", function () {
        $(".groupMoreWrap").css({ "left": $(this).offset().left + 28, "top": $(this).offset().top - 35 }).show();
    });

});

function GetProjects() {
    $.post("/BIM/GetProjects", function (data) {
        alert(data);
    });
}

//function GetProjectMembers() {
//    $.post("/BIM/GetProjectMembers", { projectId: 1 }, function (data) {
//        alert(data);
//    });
//}

function GetCategories() {
    $.post("/BIM/GetTemplates", function (data) {
        var res = strToJson(data);
        var tempStr = "";
        var tempI = 0;
        $.each(res, function (i, item) {
            if (i % 3 == 0) {
                tempStr += "<tr>";
            }
            var partyCode = 0;
            if (item.HasParty) {
                partyCode = 1;
            }
            var name = item.Name ? item.Name : "";
            var desc = item.Description ? item.Description : "";
            tempStr += "<td><div class='projectTempleteContent' id='template_" + item.Id + "_" + partyCode + "'><div class='projectTempleteTitle'>" + name + "</div><div class='projectTempleteIntroduce'>" + desc + "</div></div></td>";
            if (i % 3 == 2) {
                tempStr += "</tr>";
            }
            tempI++;
        });
        var templatePath = '/Content/Images/projects/projectTemplete.gif';
        $("#templatesTable").html(tempStr);
        //默认选中第一个模板
        var dd = $('<div class="projectTempleteImg"><img src="' + templatePath + '" /></div>');
        $("#templatesTable tr td:first div").append(dd);
        //var divId = $("#templatesTable tr td:first div").attr("id");
        var divId = $("#templatesTable .projectTempleteContent:first").attr("id");
        if (!divId) divId = "";
        var strs = divId.split('_');
        var tempId = strs[1];
        $("#templateId").val(tempId);

        if (strs.length > 2 && parseInt(strs[2]) === 0) {
            $('#projectparty').hide();
        } else {
            $('#projectparty').show();
        }
    });
}

