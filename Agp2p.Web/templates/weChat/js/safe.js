import "bootstrap-webpack";
import "../less/safe-center.css";
import "../less/footer.less";
import "../less/footer.less";

/*rem的相对单位定义*/
$("html").css("font-size", $(window).width() * 0.9 / 20);

/*切换验证码*/
function ToggleCode(obj, codeurl) {
    $(obj).attr("src", codeurl + "?time=" + Math.random());
    return false;
}
function verify_email(btn, email) {
    email = $.trim(email);
    if (email === "") {
        alert("请先填写邮箱");
        return false;
    }
    if (email === originalEmail) {
        alert("请填写另外一个邮箱");
        return false;
    }
    var disabledBtn = $(btn);
    var originalText = disabledBtn.text();
    disabledBtn.text("正在发送…").attr('disabled', 'disabled').attr('style', 'background-color: gray');

    $.getJSON('/tools/email_verify.ashx?action=sendVerifyEmailViaCode&email=' + email, function (data) {
        disabledBtn.removeAttr('disabled').removeAttr('style').text(originalText);
        alert(data.msg);
    }).fail(function (jqXHR) {
        disabledBtn.removeAttr('disabled').removeAttr('style').text(originalText);
        alert(jqXHR.responseJSON.msg);
    });
    return false;
}
function bindEmail(code) {
    $.ajax({
        dataType: "json",
        url: '/tools/email_verify.ashx?action=verifyEmail&code=' + code,
        success: function (data) {
            alert(data.msg);
            location.reload(); // 验证手机成功，刷新页面
        },
        error: function (jqXHR) {
            alert(jqXHR.responseJSON.msg);
        }
    });
}
function send_mobile_verify_code(btn, mobile, picCode) {
    mobile = $.trim(mobile);
    if (picCode === "") {
        alert("请先填写图形验证码");
        return false;
    }
    if (mobile === "") {
        alert("请先填写手机号码");
        return false;
    }
    if (mobile === originalMobile) {
        alert("请填写另外一个手机号码");
        return false;
    }
    var disabledBtn = $(btn);
    var originalText = disabledBtn.text();
    disabledBtn.text("正在发送…").attr('disabled', 'disabled');

    $.getJSON('/tools/mobile_verify.ashx?act=sendCodeForBindMobile&mobile=' + mobile + '&picCode=' + picCode, function (data) {
        disabledBtn.removeAttr('disabled').text(originalText);
        alert(data.msg);
    }).fail(function (jqXHR) {
        disabledBtn.removeAttr('disabled').text(originalText);
        alert(jqXHR.responseJSON.msg);
    });
    return false;
}
function set_mobile(btn, verifyCode) {
    verifyCode = $.trim(verifyCode);
    if (verifyCode === "") {
        alert("请先填写验证码");
        return false;
    }
    var disabledBtn = $(btn);
    var originalText = disabledBtn.text();
    disabledBtn.text("正在发送…").attr('disabled', 'disabled').attr('style', 'background-color: gray');

    $.getJSON('/tools/mobile_verify.ashx?act=verifyForBindMobile&verifyCode=' + verifyCode, function (data) {
        disabledBtn.removeAttr('disabled').removeAttr('style').text(originalText);
        alert(data.msg);
        location.reload(); // 验证手机成功，刷新页面
    }).fail(function (jqXHR) {
        disabledBtn.removeAttr('disabled').removeAttr('style').text(originalText);
        alert(jqXHR.responseJSON.msg);
    });
    return false;
}
function checkIdentifyAndSubmit(trueName, idCard) {
    var name = $.trim(trueName);
    var idCardNumber = $.trim(idCard);

    if (name === "") {
        alert("请填写姓名");
        return false;
    } else if (idCardNumber === "") {
        alert("请填写身份证号码");
        return false;
    }

    if (confirm("身份资料填写后则不能再修改，是否确认？")) {
        $.post('/tools/submit_ajax.ashx?action=bind_idcard', {
            idCardNumber: idCardNumber,
            trueName: name
        }, function (data) {
            alert(data.msg);
            if (data.status === 1)
                location.reload();
        }, "json").fail(function (jqXHR) {
            alert(jqXHR.responseText);
        });
    }
    return false;
}
function modifyPassword(btn, oldPwd, newPwd, newPwd2) {
    var oldPwdVal = $.trim(oldPwd);
    var newpwdVal = $.trim(newPwd);
    var newpwdrpVal = $.trim(newPwd2);
    if (oldPwdVal === "" || newpwdVal === "" || newpwdrpVal === "") {
        alert("请先填写好表单");
        return;
    }
    if (newpwdVal !== newpwdrpVal) {
        alert("两次输入的新密码不一样");
        return;
    }
    var disabledBtn = $(btn);
    var originalText = disabledBtn.text();
    disabledBtn.text("正在提交…").attr('disabled', 'disabled').attr('style', 'background-color: gray');
    $.post('/tools/submit_ajax.ashx?action=user_password_edit', { txtOldPassword: oldPwdVal, txtPassword: newpwdVal }, function (data) {
        alert(data.msg);
        disabledBtn.removeAttr('disabled').removeAttr('style').text(originalText);
        location.reload();
    }, "json");
}
function setTradePwd(btn, newTradePwd, newTradePwd2) {
    var tradepwdVal = $.trim(newTradePwd);
    var tradepwdrpVal = $.trim(newTradePwd2);

    if (tradepwdVal !== tradepwdrpVal) {
        alert("两次输入的交易密码不同");
        return;
    }

    var disabledBtn = $(btn);
    var originalText = disabledBtn.text();
    disabledBtn.text("正在发送…").attr('disabled', 'disabled').attr('style', 'background-color: gray');
    $.post('/tools/trade_pwd.ashx', { action: "modify", originalTransactPassword: "", newTransactPassword: tradepwdVal }, function (data) {
        disabledBtn.removeAttr('disabled').removeAttr('style').text(originalText);
        alert(data.msg);
        location.reload(); // 设置交易密码成功，刷新页面
    }, "json").fail(function (jqXHR) {
        disabledBtn.removeAttr('disabled').removeAttr('style').text(originalText);
        alert(jqXHR.responseJSON.msg);
    });
}
function modTradePwd(btn, oldTradePwd, newTradePwd, newTradePwd2) {
    var oldtpwdVal = $.trim(oldTradePwd);
    var newtpwdVal = $.trim(newTradePwd);
    var newtpwdrpVal = $.trim(newTradePwd2);

    if (newtpwdVal !== newtpwdrpVal) {
        alert("两次输入的交易密码不同");
        return;
    }

    var disabledBtn = $(btn);
    var originalText = disabledBtn.text();
    disabledBtn.text("正在发送…").attr('disabled', 'disabled').attr('style', 'background-color: gray');
    $.post('/tools/trade_pwd.ashx', { action: "modify", originalTransactPassword: oldtpwdVal, newTransactPassword: newtpwdVal }, function (data) {
        alert(data.msg);
        disabledBtn.removeAttr('disabled').removeAttr('style').text(originalText);
        location.reload(); // 修改交易密码成功，刷新页面
    }, "json").fail(function (jqXHR) {
        disabledBtn.removeAttr('disabled').removeAttr('style').text(originalText);
        alert(jqXHR.responseJSON.msg);
    });
}
function forgotTradePassword() {
    if (confirm("是否确认重置交易密码？")) {
        $.post('/tools/trade_pwd.ashx', { action: "reset" }, function (data) {
            alert(data.msg);
        }, "json").fail(function (jqXHR) {
            alert(jqXHR.responseJSON.msg);
        });
    }
}
$(function(){
    $("a.toggle-btn").click(function(){
        var current = $(this).siblings(".fold");
        current.toggle();
        $("div.fold").filter(function (index, elem) {
            return current[0] !== elem;
        }).hide();
    });
});