/*
配置专业
*/
var CC = CC || {};
(function (u, undefined) {
    var setprofessional = {
        /*判断当前用户是否具有权限查看*/
        isPermissions: function (element,isOk) {
            if (isOk) {
                document.getElementById(element).style.display = "";
            } else { 
                document.getElementById(element).style.display = "none";
            }
        },
        /*删除提示*/
        deleteDomTip: function (shellFrame) {
            var msg = "确定删除选中内容？";
            var clickBtn = shellFrame.ShellUI.ShowMessage({
                caption: "删除提示",
                message: msg,
                icon: "warning",
                button1_title: "确定",
                button2_title: "取消",
            });
            if (clickBtn == 1) return true;
            return false;
        }
    };
    u.setprofessional = setprofessional;
})(CC);