//====================初始化验证表单====================
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
            url: $("#loginform").attr("data-url"),
            dataType: "json",
            data: {
                "txtUserName": $("#txtUserName").val(),
                "txtPassword": $("#txtPassword").val(),
                "code": $("#code").val(),
                "chkRemember": $("#chkRemember").attr("checked")
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
                        //location.href = 'index.html'
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
        // 记住帐号
        if ($("#chkRemember").is(":checked")) {
            localStorage.setItem("mobileLogin_UserName", $("#txtUserName").val());
        } else {
            localStorage.removeItem("mobileLogin_UserName");
        }
        return false;
    });
    var userName = localStorage.getItem("mobileLogin_UserName");
    if (userName) {
        setTimeout(function() {
            $("#txtUserName").val(userName);
        }, 1000);
    }
});