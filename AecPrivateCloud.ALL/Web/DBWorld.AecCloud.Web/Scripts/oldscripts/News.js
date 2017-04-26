var pageIndex = 1;
var pageSize = 10;
var count;

$(function () {
    LoadNewsTitles();

    $("#newsListContent").on("click", "ul li a[id^=newId_]", function () {
        var newsId = $(this).attr("id");
        newsId = newsId.substr(6);
        location.href = "/NewsAnnounce/NewsDetails?newsId=" + newsId + "&count=" + count;
    });
})

//加载新闻标题
function LoadNewsTitles() {
    $.post("/NewsAnnounce/Titles", { pageIndex: pageIndex, pageSize: pageSize, type: "news" }, function (data) {
        var res = strToJson(data);
        count = res.count;
        var tempStr = "";
        $.each(res.titles, function (i, item) {
            tempStr += "<li><a href='javascript:void(0);' title='" + item.title + "' id='newId_" + item.Id + "'>" + item.title + "</a><time class='time'>" + item.Date + "</time></li>";
        });
        $("#newsListContent ul").html(tempStr);
        if (pageIndex == 1) {
            Paging(".paging", "LoadNewsTitles", count, pageSize, pageIndex);
        }
    });
}