//浏览器改变大小时执行（网页头部梯形中间部分）
$(window).resize(function () {
    $(".Topcontent").width($(".ContentWrap").width() - 70);
    //中间内容部分最小高度
    $("#content").css({ "minHeight": $(window).height() - 430 });
})


/*首页*/
$(function () {
    //中间内容部分最小高度
    $("#content").css({ "minHeight": $(window).height() - 430 });

    //网页头部梯形中间部分
    $(".Topcontent").width($(".ContentWrap").width() - 70);
    //定位锚点，如果有。统一在导航的首页上设定name=“anchor”为锚点
    //location.hash = "anchor";
    $("#DBWORLDTitle").html("DBWorld云平台");
    //左侧导航样式切换
    var currentPage = $("#currentWrap a[name^=current_]").attr("name"); //asideWrap
    if (currentPage != null) {
        var tempStr = currentPage.substring(8, currentPage.length);
        switch (tempStr) {
            case "client":
                $("#DBWORLDTitle").html("DBWorld客户端");
                break;
            case "design":
                $("#DBWORLDTitle").html("DBWorld协同云");
                break;
            case "knowledge":
                $("#DBWORLDTitle").html("DBWorld知识云");
                break;
            case "software":
                $("#DBWORLDTitle").html("DBWorld软件云");
                break;
            case "antiture":
                $("#DBWORLDTitle").html("DBWorld用户");
                break;
            case "announcement":
                $("#DBWORLDTitle").html("DBWorld公告");
                break;
            case "news":
                $("#DBWORLDTitle").html("DBWorld新闻");
                break;
            case "help":
                $("#DBWORLDTitle").html("DBWorld帮助");
                break;
            default:
                break;
        }
        $(".asideWrap>a").each(function (i) {
            if ($(this).attr("name") == tempStr) {
                $(this).next(".ul_list").show().siblings(".ul_list").hide();
                $(this).addClass("on").siblings().removeClass("on");
            }
        });
    }

    //知识库搜索
    var searchTime = null;
    $(".selType").mouseover(function () {
        $(".selTypeList").css({ top: $(".selType").offset().top + 39, left: $(".selType").offset().left }).show();
    }).mouseleave(function () {
        searchTime = setTimeout(function () {
            $(".selTypeList").hide();
        }, 100);
    })
    //鼠标移进列表框时，列表框不消失
    $(".selTypeList").mousemove(function () {
        clearTimeout(searchTime);
    }).mouseleave(function () {
        searchTime = setTimeout(function () {
            $(".selTypeList").hide();
        }, 100);
    });
    //列表的点击事件
    $(".selTypeList ul li").click(function () {
        $(".selType span").text($(this).text());
        $(".selTypeList").hide();
    });


    //导航条
    //主导航li的鼠标移上去事件
    $(".navTable").on("mouseover", "ul li", function () {
        //把当前显示的所有二级导航隐藏起来
        $(this).siblings().find(".secondNav").hide();
        $(this).siblings().removeClass('navOn');
        //当前加上选中状态样式
        $(this).addClass("navOn");
        $(this).find(".secondNav").css({ top: $(this).offset().top + $(".navContent").height() - $(document).scrollTop(), left: $(this).offset().left }).show();
    });
    var hideTimer = null;
    $(".navTable ul li").mouseleave(function () {
        //鼠标移开1秒钟元素消失
        hideTimer = setTimeout("$('.secondNav').hide();$('.navTable ul li').removeClass('navOn')", 100);
    }).mouseover(function () {
        //鼠标进去元素区域，元素不消失
        clearTimeout(hideTimer);//清除计时器
    });

    //在线咨询
    //每次页面加载的时候动态定位qq咨询在网页中的位置
    $("#online_qq_layer").css({ top: (($(window).height() - $("#online_qq_layer").height()) / 3) * 2 })


    //搜索框提示
    var $Searchinput = $(".AESearching .searching_box");
    //默认状态下input框的值
    $Searchinput.focus(function () {
        //获取焦点是，即鼠标移到搜索框时执行提示信息清空
        $Searchinput.attr("value", "");
        $Searchinput.css("color", "#000");
    }).blur(function () {
        //当搜索完成或者用户把鼠标移除搜索框提示信息再次出现（即搜索框失去焦点的时候执行）
        if ($Searchinput.val() == "") {
            $Searchinput.attr("value", "请输入搜索关键字...");
            $Searchinput.css("color", "#999");
        } else {
            var $val = $Searchinput.val();
            $Searchinput.attr("value", $val);
            //return true;
        }
    });

});


//新闻滚动
$(function () {
    var $this = $("#gun");
    var $li = $("#gun ul li");
    var scrollTimer = setInterval(function () {
        scrollNews($this);
    }, 4000);

    $li.hover(function () {
        clearInterval(scrollTimer);
    }, function () {
        scrollTimer = setInterval(function () {
            scrollNews($this);
        }, 4000);
    });
});
function scrollNews(obj) {
    //自定义图片自动轮转函数
    var $self = obj.find("ul:first");
    var lineHeight = $self.find("li:first").height();//获取行高
    $self.animate({ "marginTop": -lineHeight + "px" }, 600, function () {
        $self.css({ marginTop: 0 }).find("li:first").appendTo($self);//appendTo能直接移动元素
    })
};

//分页
function paging(pages, jumpPage, pageDiv, method) {
    $(pageDiv).html("");
    $(pageDiv).append("<a href='#' class='prev_next_border' id='page_first'><<</a><a href='#' class='prev_next_border' id='page_previous'><</a>");
    if (pages > 4) {
        if (pages - jumpPage > 4) {
            $(pageDiv).append("<a href='#' class='prev_next_on'  id='page_" + Number(jumpPage) + "'>" + Number(jumpPage) + "</a><a href='#' class='prev_next_on' id='page_" + (Number(jumpPage) + 1) + "'>" + (Number(jumpPage) + 1) + "</a><a href='#' class='prev_next_on' id='page_" + (Number(jumpPage) + 2) + "'>" + (Number(jumpPage) + 2) + "</a><a href='#' class='prev_next_on' id='page_" + (Number(jumpPage) + 3) + "'>" + (Number(jumpPage) + 3) + "</a><a href='#' class='prev_next_on' id='page_" + (Number(jumpPage) + 4) + "'>" + (Number(jumpPage) + 4) + "</a>");
        } else {
            $(pageDiv).append("<a href='#' class='prev_next_on' id='page_" + (pages - 4) + "'>" + (pages - 4) + "</a><a href='#' class='prev_next_on' id='page_" + (pages - 3) + "'>" + (pages - 3) + "</a><a href='#' class='prev_next_on' id='page_" + (pages - 2) + "'>" + (pages - 2) + "</a><a href='#' class='prev_next_on' id='page_" + (pages - 1) + "'>" + (pages - 1) + "</a><a href='#' class='prev_next_on' id='page_" + pages + "'>" + pages + "</a>");
        }
    } else {
        for (var i = 0; i < pages; i++) {
            $(pageDiv).append("<a href='#' id='page_" + (i + 1) + "'>" + (i + 1) + "</a>");
        }
    }
    $(pageDiv).append("<a href='#' class='prev_next_border' id='page_next'>></a><a href='#' class='prev_next_border' id='page_last'>>></a>");
}
//pageID:跳转页面的页码ID
function gotoPage(pages, pageID, pageDiv, method) {
    var jumpPageIndex = pageID.substr(5); //跳转的页码
    switch (jumpPageIndex) {
        case "first":   //首页
            jumpPage = 1;
            break;
        case "last": //尾页
            jumpPage = pages;
            break;
        case "previous":  //上一页
            if (jumpPage == 1) {
                return false;
            } else {
                jumpPage--;
            }
            break;
        case "next": //下一页
            if (jumpPage == pages) {
                return false;
            } else {
                jumpPage++;
            }
            break;
        default:
            jumpPage = jumpPageIndex;
            break;
    }
    paging(pages, jumpPage, pageDiv, method);
    switch (method) {
        case "loadNews":
            LoadNews(jumpPage);
            break;
        default:
            break;
    }
}

//为跳转的当前页面的页码染色,pagingDiv为分页id或者class名字，jumpPage为跳转页码
function pageColor(pagingDiv, jumpPage) {
    $(pagingDiv + " a").each(function () {
        $(this).css("color", "");
        var tempId = $(this).attr("id").substr(5);
        if (tempId == jumpPage) {
            $(pagingDiv + " #page_" + tempId).css("color", "#e44b07");
        }
    });
}

