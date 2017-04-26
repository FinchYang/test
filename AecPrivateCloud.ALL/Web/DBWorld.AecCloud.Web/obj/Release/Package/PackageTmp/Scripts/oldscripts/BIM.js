/****
    main: webClient中公用样式的js摆放
****/

$(function () {
    //日期控件
    //$(".form_datetime").datetimepicker({
    //    format: 'yyyy-mm-dd',
    //    autoclose: true,
    //    minView: 2
    //});
    //弹窗关闭
    $(".popupClose").click(function () {
        $(this).parents(".popupWrap").hide();
        $(".popupWrapBg").hide();
    });

    //下拉列表
    $('[name="nice-select"]').click(function (e) {
        $('[name="nice-select"]').find('ul').hide();
        $(this).find('ul').show();
        e.stopPropagation();
    });
    $('[name="nice-select"] li').hover(function (e) {
        $(this).toggleClass('on');
        e.stopPropagation();
    });
    $('[name="nice-select"] li').click(function (e) {
        var val = $(this).text();
        var dataVal = $(this).attr("data-value");
        $(this).parents('[name="nice-select"]').find('input').val(val);
        $('[name="nice-select"] ul').hide();
        e.stopPropagation();
        //alert($(this).parents('[name="nice-select"]').find('input').val());
    });
    $(document).click(function () {
        $('[name="nice-select"] ul').hide();
    });


    //左侧导航列表收缩效果
    $(".leftNav span").click(function () {
        if ($(this).text() == "-") {
            $(".leftNav li").not(".leftNavTitle").hide();
            $(".leftNavTitle span").text("-").attr("class", "asidedown");
            $(this).parent().siblings().show();
            $(this).text("+").attr("class", "asideup");
        } else {
            $(this).parent().siblings().hide();
            $(this).text("-").attr("class", "asidedown");
        }
    });
    //默认第一个展开
    $("aside .leftNav:first span").click();


    //提示信息总弹窗
    $(".promptWrapClick").click(function () {
        $(".popupWrapBg").css("height", $(document).height()).show();
        $(".promptWrap").css({ "left": ($(window).width() - $(".promptWrap").width()) / 2, "top": ($(window).height() - $(".promptWrap").height()) / 2 }).show();
    });


    //Tab 公用tab标签样式
    var $li = $(".tab_ul li");
    $li.click(function () {
        //当前li增加样式li_on同时去除与当前li同一层的li中的li_on样式
        $(this).addClass("li_on").siblings().removeClass("li_on");
        //获得当前li的索引值，以备让div使用。
        var $index = $li.index(this);
        $(".tab_content > div").eq($index).show().siblings().hide();
    }).hover(function () {
        $(this).addClass("hover");
    }, function () {
        $(this).removeClass("hover");
    });

    //拖动事件
    $(".dragDiv").mousedown(function (e) {
        $(this).css("cursor", "move");
        var dragDiv = $(".dragDiv").parent(".popupWrap");
        var offset = dragDiv.offset();
        var x = e.pageX - offset.left;
        var y = e.pageY - offset.top;
        $(window).bind("mousemove", function (ev) {
            dragDiv.stop();
            var _x = ev.pageX - x;
            var _y = ev.pageY - y;
            dragDiv.animate({ left: _x + "px", top: _y + "px" }, 10);
        });
    });
    $(window).mouseup(function () {
        $(".dragDiv").css("cursor", "default");
        $(this).unbind("mousemove");
    });

});

