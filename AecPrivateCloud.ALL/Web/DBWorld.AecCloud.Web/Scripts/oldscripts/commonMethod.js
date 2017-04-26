/**************
+通用方法集合
**************/

//字符串转JSON
function strToJson(str) {
    //return $.parseJSON(str);
    var json = eval('(' + str + ')');
    return json;
}

//此方法为分页方法
//pagingDiv:分页DIV, method：分页所对应的加载数据的方法, count：总数据量, pageSize：每页大小, pageIndex：当前页码
function Paging(pagingDiv, method, count, pageSize, pageIndex) {
    var pages = Math.ceil(count / pageSize);
    pageChange(pagingDiv, 1, pages);
    pageColor(pagingDiv, "page_" + pageIndex); //首次加载第一页
    $(pagingDiv).on("click", "a", function (e) {
        var $this = $(this);
        var jumpPageIndex = $this.attr("id").substr(5); //跳转的页码
        switch (jumpPageIndex) {
            case "first":   //首页
                pageIndex = 1;
                break;
            case "last": //尾页
                pageIndex = pages;
                break;
            case "up":  //上一页
                if (pageIndex == 1) {
                    return false;
                } else {
                    pageIndex--;
                }
                break;
            case "down": //下一页
                if (pageIndex == pages) {
                    return false;
                } else {
                    pageIndex++;
                }
                break;
            default:
                pageIndex = jumpPageIndex;
                break;
        }
        pageChange(pagingDiv, pageIndex, pages);
        pageColor(pagingDiv, "page_" + pageIndex);
        switch (method) {
            case "LoadNewsTitles":
                LoadNewsTitles(pageIndex);
                break;
            case "loadSearchResult":
                loadSearchResultList(pageIndex);
                break;
            case "LoadAnnounceTitles":
                LoadAnnounceTitles(pageIndex);
                break;
            case "loadComment":
                loadComment(pageIndex);
                break;
            case "getAlbumsList":
                getAlbumsList(pageIndex);
                break;
            case "LoadFileList":
                LoadFileList(pageIndex);
                break;
            default:
                break;
        }
        return false;
    });
}

//页数切换
function pageChange(pagingDiv, jumpPage, pages) {
    if (pages == 0) {
        $(pagingDiv).html("无数据！");
    } else {
        $(pagingDiv).html("");
        //$(pagingDiv).append("<a href='#' id='page_first'>首页</a><a href='#' id='page_up'>上一页</a> ");
        $(pagingDiv).append("<a href='#' title='首页' class='pageA' id='page_first'>首页</a><a href='#' title='上一页' id='page_up' class='pageA'>上一页</a>");
        if (pages > 4) {
            if (pages - jumpPage > 4) {
                $(pagingDiv).append("<a href='#' class='pageA' id='page_" + Number(jumpPage) + "'>" + Number(jumpPage) + "</a><a href='#' class='pageA' id='page_" + (Number(jumpPage) + 1) + "'>" + (Number(jumpPage) + 1) + "</a><a href='#' class='pageA' id='page_" + (Number(jumpPage) + 2) + "'>" + (Number(jumpPage) + 2) + "</a><a href='#' class='pageA' id='page_" + (Number(jumpPage) + 3) + "'>" + (Number(jumpPage) + 3) + "</a><a href='#' class='pageA' id='page_" + (Number(jumpPage) + 4) + "'>" + (Number(jumpPage) + 4) + "</a>");
            } else {
                $(pagingDiv).append("<a href='#' class='pageA'  id='page_" + (pages - 4) + "'>" + (pages - 4) + "</a><a href='#' class='pageA' id='page_" + (pages - 3) + "'>" + (pages - 3) + "</a><a href='#' class='pageA' id='page_" + (pages - 2) + "'>" + (pages - 2) + "</a><a href='#' class='pageA' id='page_" + (pages - 1) + "'>" + (pages - 1) + "</a><a href='#' class='pageA' id='page_" + pages + "'>" + pages + "</a>");
            }
        } else {
            for (var i = 0; i < pages; i++) {
                $(pagingDiv).append("<a href='#' class='pageA'  id='page_" + (i + 1) + "'>" + (i + 1) + "</a>");
            }
        }
        $(pagingDiv).append("<a href='#' title='下一页' class='pageA' id='page_down'>下一页</a><a href='#' title='尾页' class='pageA' id='page_last'>尾页</a>");
    }
}

//点击页码时候切换样式,jumpPageId:要跳转的页数
function pageColor(pagingDiv, jumpPageId) {
    $(pagingDiv + " a").each(function () {
        $(this).removeClass("pageOn");
        var tempId = $(this).attr("id");
        if (tempId == jumpPageId) {
            $("#" + tempId).addClass("pageOn");
        }
    });
}

//获取url参数方法
function getUrlParam(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)"); //构造一个含有目标参数的正则表达式对象
    var r = window.location.search.substr(1).match(reg);  //匹配目标参数
    if (r != null) return unescape(decodeURIComponent(r[2])); return null; //返回参数值
}

//获取上传文件名
function getFileName(o) {
    var pos = o.lastIndexOf("\\");
    return o.substring(pos + 1);
}
//盘点上传文件名是否为图片
function isimg(src) {
    var ext = ['.gif', '.jpg', '.jpeg', '.png'];
    var s = src.toLowerCase();
    var r = false;
    for (var i = 0; i < ext.length; i++) {
        if (s.indexOf(ext[i]) > 0) {
            r = true;
            break;
        }
    }
    return r;
}
//图片预览（并未上传到服务器）
function previewImage(file) {
    var upFile = $("#imgUp").val();
    var fileName = getFileName(upFile);
    if (!isimg(fileName)) {
        if (fileName) {
            alert("上传非图片！请重新选择！");
        } 
        return false;
    }
    var MAXWIDTH = 185;
    var MAXHEIGHT = 150;
    var div = document.getElementById('preview');
    if (file.files && file.files[0]) {
        div.innerHTML = '<img id=imghead>';
        var img = document.getElementById('imghead');
        img.onload = function () {
            var rect = clacImgZoomParam(MAXWIDTH, MAXHEIGHT, img.offsetWidth, img.offsetHeight);
            img.width = rect.width;
            img.height = rect.height;
            //img.style.marginLeft = rect.left + 'px';
            //img.style.marginTop = rect.top + 'px';
        }
        var reader = new FileReader();
        reader.onload = function (evt) { img.src = evt.target.result; }
        reader.readAsDataURL(file.files[0]);
    }
    else {
        var sFilter = 'filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(sizingMethod=scale,src="';
        file.select();
        var src = document.selection.createRange().text;
        div.innerHTML = '<img id=imghead>';
        var img = document.getElementById('imghead');
        img.filters.item('DXImageTransform.Microsoft.AlphaImageLoader').src = src;
        var rect = clacImgZoomParam(MAXWIDTH, MAXHEIGHT, img.offsetWidth, img.offsetHeight);
        status = ('rect:' + rect.top + ',' + rect.left + ',' + rect.width + ',' + rect.height);
        div.innerHTML = "<div id=divhead style='width:" + rect.width + "px;height:" + rect.height + "px;margin-top:" + rect.top + "px;margin-left:" + rect.left + "px;" + sFilter + src + "\"'></div>";
    }
}
function clacImgZoomParam(maxWidth, maxHeight, width, height) {
    var param = { top: 0, left: 0, width: width, height: height };
    if (width > maxWidth || height > maxHeight) {
        rateWidth = width / maxWidth;
        rateHeight = height / maxHeight;
        if (rateWidth > rateHeight) {
            param.width = maxWidth;
            param.height = Math.round(height / rateWidth);
        } else {
            param.width = Math.round(width / rateHeight);
            param.height = maxHeight;
        }
    }
    param.left = Math.round((maxWidth - param.width) / 2);
    param.top = Math.round((maxHeight - param.height) / 2);
    return param;
}

//JS邮箱验证
function isEmail(str) {
    var reg = /^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+((\.[a-zA-Z0-9_-]{2,3}){1,2})$/;
    return reg.test(str);
}