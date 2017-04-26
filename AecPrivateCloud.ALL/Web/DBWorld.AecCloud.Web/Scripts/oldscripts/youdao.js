
$(function () {
    //点击效果（包括顶部导航、右侧导航）

    var num = [1, 2, 3, 4, 5, 6, 7, 8, 9];
    // num.length = topNum;
    //alert($("#top2").height());
    //返回顶部
    $(".toTop").click(function () {
        $("html,body").animate({ scrollTop: "0" }, 1000);
        //$(".rightNav .top1").click();
    });
    //每一版的高度
    //$(".doc").css("paddingTop", $("#top2").height() + 50);
    $(".doc").css("height", $(window).height() - 190);
    ////进入页面是默认点击#top1
    //$(".rightNav .top1").click();

    //alert($("#top2").height());
    var top2 = $("#top2").offset().top;
    if (top2 <= $(window).scrollTop()) {
        $("#content .doc:first").css("paddingTop", "190px");
        $.each(num, function (i, j) {
            $(".top" + j + "").click(function () {
                $("#content .doc:first").css("paddingTop", "190px");
                $("html,body").animate({ scrollTop: $("#top" + j + "").offset().top }, 1000);
            });
        });
    } else if (top2 > $(window).scrollTop()) {
        $("#content .doc:first").css("paddingTop", "0");
        $.each(num, function (i, j) {
            $(".top" + j + "").click(function () {
                $("#content .doc:first").css("paddingTop", "0");
                $("html,body").animate({ scrollTop: $("#top" + j + "").offset().top }, 1000);
                // $(".top" + j + "").siblings().removeClass("anchor").end().addClass("anchor");
            });
        });
    }

    $(window).scroll(function () {
        if (top2 <= $(window).scrollTop()) {
            $("#content .doc:first").css("paddingTop", "190px");
            $("#top2").addClass("posFix").css({ left: ($(window).width() - $("#top2").width()) / 2 });
        } else if (top2 > $(window).scrollTop()) {
            $("#content .doc:first").css("paddingTop", "0");
            $("#top2").removeClass("posFix");
        }
    })




    //$(document).on('mousewheel', function (event, delta, deltaX, deltaY) {
    //    var topNum = $("[id^='top']").length;//获取当前页面拥有的锚点个数
    //    var scrollTop = $(window).scrollTop();
    //    var height = $(window).height();
    //    for (var i = 1; i <= topNum; i++) {
    //        if ($("#top" + i + "").offset().top <= scrollTop) {
    //            $(".rightNav .top" + i + "").siblings().removeClass("anchor").end().addClass("anchor");
    //            var topH = $("#top" + i + "").offset().top;
    //        }
    //    }
    //    //var hhh = 0;
    //    //delta为1向上滚动  delta为-1向下滚动
    //    console.info("delta=" + delta + "   deltaX=" + deltaX + "   deltaY=" + deltaY);
    //    if (delta == "1") {
    //        //向上滚
    //        console.info($(window).scrollTop());
    //        $("html,body").stop().animate({ scrollTop: topH - height }, 1000, function () {
    //            for (var i = 1; i <= topNum; i++) {
    //                if ($("#top" + i + "").offset().top <= scrollTop) {
    //                    $(".rightNav .top" + i + "").prev().addClass("anchor").siblings().removeClass("anchor");
    //                }
    //            }
    //            //var $index = $(".rightNav div.anchor").index();
    //            //var $index2 = $index + 1;
    //            //$("#top" + $index2 + "").height();
    //        });

    //    } else if (delta == "-1") {
    //        //向下滚;
    //        $("html,body").stop().animate({ scrollTop: topH + height }, 1000, function () {
    //            for (var i = 1; i <= topNum; i++) {
    //                if ($("#top" + i + "").offset().top <= scrollTop) {
    //                    $(".rightNav .top" + i + "").next().addClass("anchor").siblings().removeClass("anchor");
    //                }
    //            }
    //            //var $index = $(".rightNav div.anchor").index();
    //            //var $index2 = $index + 1;
    //            //$("#top" + $index2 + "").height()
    //            //alert($(".rightNav div").hasClass("anchor").index(this));
    //        });
    //    }
    //});
});




$(window).resize(function () {
    //每一版的高度
    $(".doc").css("height", $(window).height());
});


//鼠标滚动效果、滚动条效果
//$(window).scroll(function () {
//    var topNum = $("[id^='top']").length;//获取当前页面拥有的锚点个数
//    var scrollTop = $(window).scrollTop();
//    for (var i = 1; i <= topNum; i++) {
//        if ($("#top" + i + "").offset().top <= scrollTop) {
//            $(".rightNav .top" + i + "").siblings().removeClass("anchor").end().addClass("anchor");
//        }
//    }
//    //$.each(num, function (i, j) {
//    //    if ($("#top" + j + "").offset().top <= scrollTop) {
//    //        $(".rightNav .top" + j + "").siblings().removeClass("anchor").end().addClass("anchor");
//    //    }
//    //});
//});


