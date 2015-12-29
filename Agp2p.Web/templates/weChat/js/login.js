import "bootstrap-webpack";
import "../less/common.less";
import "../less/login.less";

/*rem的相对单位定义*/
$("html").css("font-size", $(window).width() * 0.9 / 20);

//初始化验证表单
$(function () {
    //提交表单
    var btnSubmit = $("#btnSubmit");
    btnSubmit.bind("click", function() {
        if ($("#txtUserName").val() == "" || $("#txtPassword").val() == "") {
            $("#msgtips").show();
            $("#msgtips dd").text("请填写用户名和登录密码！");
            return false;
        }
        $.ajax({
            type: "POST",
            url: "/tools/submit_ajax.ashx?action=user_login",
            dataType: "json",
            data: {
                "txtUserName": $("#txtUserName").val(),
                "txtPassword": $("#txtPassword").val(),
                "code": $("#code").val(),
                "chkRemember": $("input[type=checkbox]").is(":checked")
            },
            timeout: 20000,
            beforeSend: function(XMLHttpRequest) {
                btnSubmit.attr("disabled", true);
                $("#msgtips").show();
                $("#msgtips dd").text("正在登录，请稍候...");
            },
            success: function(data, textStatus) {
                if (data.status == 1) {
                    if (typeof(data.url) == "undefined") {
                        location.href = $("#loginform").attr("data-turl");
                        //location.href = '/'
                    } else {
                        location.href = data.url;
                    }
                } else {
                    btnSubmit.attr("disabled", false);
                    $("#msgtips dd").text(data.msg);
                }
            },
            error: function(XMLHttpRequest, textStatus, errorThrown) {
                $("#msgtips dd").text("状态：" + textStatus + "；出错提示：" + errorThrown);
                btnSubmit.attr("disabled", false);
            }
        });
        return false;
    });
});