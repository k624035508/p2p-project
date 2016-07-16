import "bootstrap-webpack";
import "../less/common.less";
import "../less/register_tdlte.less";
import footerInit from "./footer.js";

window['$'] = window['jQuery'] = $;

/*rem的相对单位定义*/
var viewportWidth = $(window).width();
var fontSizeUnit = viewportWidth * 0.9 / 20;
$("html").css("font-size", fontSizeUnit);

/*切换验证码*/
window.ToggleCode = function(obj, codeurl) {
    $(obj).attr("src", codeurl + "?time=" + Math.random());
    return false;
}

function getUrlParameter(sParam) {
    var sPageURL = window.location.search.substring(1);
    var sURLVariables = sPageURL.split('&');
    for (var i = 0; i < sURLVariables.length; i++) {
        var sParameterName = sURLVariables[i].split('=');
        if (sParameterName[0] == sParam) {
            return sParameterName[1];
        }
    }
    return null;
}
//=====================初始化代码======================
$(function () {
    var { step,result } = $("#step").data();
    if (step == "2") {
        //实名验证
        $(".register-step").eq(1).show().siblings().hide();
    } else {       
        $(".register-step").eq(0).show().siblings().hide();
        $(".liuliang-content").hide();
    }    

    $(".liuliang-rules") .click(function() {
            $(".liuliang-content").toggle();
        });

    $(".cancel-btn").click(function() {
        $("#liuliangConfirm").hide();
    })

    //同意条款
    var chkAgree = $("#agreement");
    var btnSendCode = $("#get-auth-code-btn");
    var btnSubmit = $("#register-btn");
    var btnSubmitName = $("#register-identity-btn");

    //发送短信
    btnSendCode.click(function () {
        //检查是否输入手机号码
        var inputContent = $.trim($("#account").val());
        var picCode = $.trim($("input#pic-code").val());
        if (inputContent === "") {
            $.dialog.alert("对不起，请先输入手机号码");
            return false;
        } else if (picCode === "") {
            $.dialog.alert("对不起，请先输入图形验证码");
            return false;
        }

        //判断输入内容是否为11位数字
        var regex = /^\d{11}$/;
        if (!regex.test(inputContent)) {
            $.dialog.alert("请输入正确的手机号码！");
            return false;
        }

        //设置按钮状态        
        btnSendCode.prop("disabled", true);
        $.ajax({
            type: "POST",
            url: "/tools/submit_ajax.ashx?action=user_register_smscode",
            dataType: "json",
            data: {
                mobile: inputContent,
                txtPicCode: picCode
            },
            timeout: 20000,
            success: function (data, textStatus) {
                if (data.status === 1) {
                    $.dialog.tips(data.msg, 2, "32X32/succ.png", function () { });

                    var i = 120;

                    btnSendCode.html(i.toString() + "秒后可重发");
                    btnSendCode.css("font-size", "1.2em");

                    var timer = setInterval(function() {
                        i--;
                        if (i === 0) {
                            btnSendCode.attr("disabled", null);
                            btnSendCode.html("获取验证码");
                            btnSendCode.css("font-size", "1.4em");

                            clearInterval(timer);
                        } else {
                            btnSendCode.html(i.toString() + "秒后可重发");
                        }
                    }, 1000);
                } else {
                    btnSendCode.prop("disabled", false);
                    $.dialog.alert(data.msg);                    
                }
            },
            error: function (xmlHttpRequest, textStatus, errorThrown) {
                btnSendCode.prop("disabled", false);
                $.dialog.alert("状态：" + textStatus + "；出错提示：" + errorThrown);
            }
        });
    });

    //表单提交前
    function showRequest(formData, jqForm, options) {
        btnSubmit.val("正在提交...");
        btnSubmit.prop("disabled", true);
        chkAgree.prop("disabled", true);
    }
    //注册表单提交后
    function showResponse(data, textStatus) {
        if (data.status === 1) { //成功
            //location.href = data.url;
            
            $.dialog.alert(data.msg, function() {
                //注册成功后进入实名认证
                // location.href = "/api/payment/sumapay/index.aspx?api=101";
                location.href="register_tdlte.html?action=2";
            });
        } else { //失败
            $.dialog.alert(data.msg);
            btnSubmit.val("再次提交");
            btnSubmit.prop("disabled", false);
            chkAgree.prop("disabled", false);
        }
    }

    //实名认证表单提交后
    function showResponseName(data, textStatus) {
        if (data.status === 1) { //成功
            //location.href = data.url;
            $.dialog.alert(data.msg, function() {
                //注册成功后进入开户
                location.href = "/api/payment/sumapay/index.aspx?api=101";
            });
        } else { //失败
            $.dialog.alert(data.msg);
            btnSubmitName.val("再次提交");
            btnSubmitName.prop("disabled", false);
        }
    }

    //表单提交出错
    function showError(xmlHttpRequest, textStatus, errorThrown) {
        $.dialog.alert("状态：" + textStatus + "；出错提示：" + errorThrown);
        btnSubmit.val("再次提交");
        btnSubmit.prop("disabled", false);
        chkAgree.prop("disabled", false);
    }
    
    function showErrorName(xmlHttpRequest, textStatus, errorThrown) {
        $.dialog.alert("状态：" + textStatus + "；出错提示：" + errorThrown);
        btnSubmitName.val("再次提交");
        btnSubmitName.prop("disabled", false);
    }

    //初始化验证表单
    $("#regform").Validform({
        btnSubmit: "#register-btn",
        tipSweep: true,
        tiptype: 3,
        callback: function (form) {
            //AJAX提交表单
            $(form).ajaxSubmit({
                beforeSubmit: showRequest,
                success: showResponse,
                error: showError,
                url: "/tools/submit_ajax.ashx?action=user_register",
                type: "post",
                dataType: "json",
                timeout: 60000
            });
            return false;
        }
    });

    $("#realNameForm").Validform({
        btnSubmit: "#register-identity-btn",
        tipSweep: true,
        tiptype:3,
        callback:function(form) {
            $(form).ajaxSubmit({
                success: showResponseName,
                error: showErrorName,
                url: "/tools/submit_ajax.ashx?action=bind_idcard",
                type: "post",
                dataType: "json",
                timeout: 60000
            });
            return false;
        }
    });
});