import "bootstrap-webpack";
import "../less/common.less";
import "../less/forgot_password.less";
import footerInit from "./footer.js";

window['$'] = window["jQuery"] = $;

/*rem的相对单位定义*/
var viewportWidth = $(window).width();
var fontSizeUnit = viewportWidth * 0.9 / 20;
$("html").css("font-size", fontSizeUnit);

/*切换验证码*/
function ToggleCode(obj, codeurl) {
    $(obj).attr("src", codeurl + "?time=" + Math.random());
    return false;
}

var forTransactPassword = 0 <= location.search.indexOf("forTransactPassword");
if (forTransactPassword) {
    document.title = "重置交易密码";
}

$(function () {
    window.alert = $.dialog.alert;

    if (forTransactPassword) {
        $("#newPwd").attr("placeholder", "请输入新交易密码");
        $("#newPwd2").attr("placeholder", "请再次输入新交易密码");
    }

    $("#imgVerifyCode").click(ev => {
        ToggleCode(ev.currentTarget, '/tools/verify_code.ashx');
    });

    $("#btnGetVerifyCode").click(function () {
        var mobile = $("#txtMobile").val();
        var picCode = $("input#picCode").val();
        if ($.trim(mobile) === "") {
            alert("请先输入手机号码");
            return;
        } else if ($.trim(picCode) === "") {
            alert("请先输入图形验证码");
            return;
        }
                
        var sendingBtn = $(this);
        var originalText = sendingBtn.text();
        sendingBtn.text("正在发送…").attr('disabled', 'disabled');
        $.getJSON('/tools/mobile_verify.ashx?act=sendCodeForResetPwd&mobile=' + mobile + '&picCode=' + picCode, function (data) {
            sendingBtn.removeAttr('disabled').text(originalText);
            alert(data.msg);
        }).fail(function (jqXHR) {
            sendingBtn.removeAttr('disabled').text(originalText);
            alert(jqXHR.responseJSON.msg);
        });
    });
    $("#btnSubmit").click(function() {
        var verifyCode = $("#verifyCode").val();
        var newPwd = $("#newPwd").val();
        var newPwd2 = $("#newPwd2").val();
        if (newPwd !== newPwd2) {
            alert("两次输入的密码不一致");
            return;
        }

        var submitBtn = $(this);
        var originalText = submitBtn.text();
        submitBtn.text("正在发送…").attr('disabled', 'disabled').attr('style', 'background-color: gray');

        $.post(`/tools/mobile_verify.ashx?act=verifyForResetPwd&verifyCode=${verifyCode}&forTransactPassword=${forTransactPassword}`, { newPwd }).done(function(data) {
            submitBtn.removeAttr('disabled').removeAttr('style').text(originalText);
            alert(data.msg, function() {
                location.href = '/login.html'; // 重置密码成功，回首页
            });
        }).fail(function(jqXHR) {
            submitBtn.removeAttr('disabled').removeAttr('style').text(originalText);
            alert(jqXHR.responseJSON.msg);
        });
    });
});
