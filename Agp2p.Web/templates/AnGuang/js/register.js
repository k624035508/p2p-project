﻿import "es5-shim/es5-shim"
import "es5-shim/es5-sham"

import "bootstrap-webpack";
import "../less/head.less";
import "../less/register.less";
import "../less/footerSmall.less";

$(function(){
    // 检测邀请码
    var matchInvitationUrl = location.search.match(/inviteCode=([^&]+)/);
    if (matchInvitationUrl) {
        $("#recommended").val(matchInvitationUrl[1]).attr("disabled", "disabled");
    }
    // 手机号码格式判断
    $("#account").blur(function(){
        var regex = /\d{11}/;
        var phone = $("#account").val();
        var $status = $("#account").next();
        if(regex.test(phone)){
            $status.removeClass("error-tips");
            $status.addClass("right-tips");
            $status.text("");
        } else{
            $status.removeClass("right-tips");
            $status.addClass("error-tips");
            $status.text("请输入正确的手机号码");
        }
    }).focus(); // 自动聚焦第一个 input

    //登录密码格式判断
    $("#psw").blur(function(){
        var num = $("#psw").val().length;
        var $status = $("#psw").next();
        $status.removeClass("psw-tips");
        if(5 < num && num < 17){
            $status.removeClass("error-tips");
            $status.addClass("right-tips");
            $status.text("");
        } else {
            $status.removeClass("right-tips");
            $status.addClass("error-tips");
            $status.text("请输入6~16位密码");
        }
    });

    //确认密码格式判断
    $("#psw2").blur(function(){
        var psw = $("#psw").val();
        var psw2 = $("#psw2").val();
        var $status = $("#psw2").next();
        if (psw2 == ""){
            $status.removeClass("right-tips");
            $status.addClass("error-tips");
            $status.text("请输入6~16位密码");
        } else if(psw2 == psw){
            $status.removeClass("error-tips");
            $status.addClass("right-tips");
            $status.text("");
        }else {
            $status.removeClass("right-tips");
            $status.addClass("error-tips");
            $status.text("两次输入的密码不一致");
        }
    });

    // 刷新图文验证码
    var $picCode = $(".pic-code-wrap img");
    var originSrc = $picCode[0].getAttribute("src");
    $picCode.click(function(){
        this.setAttribute("src", originSrc + "?r=" + Math.random());
    });

    //图文验证码格式判断
    $("#pic-code").blur(function(){
        var picCode = $("#pic-code").val().length;
        var $status = $("#pic-code").siblings(".status");
        if(picCode == 0) {
            $status.addClass("error-tips");
            $status.text("请输入图片上的验证码");
        } else {
            $status.removeClass("error-tips");
            $status.text("");
        }
    });

    //短信验证码格式判断
    $("#sms-code").blur(function(){
        var smsCode = $("#sms-code").val().length;
        var $status = $("#sms-code").siblings(".status");
        if(smsCode == 0){
            $status.addClass("error-tips");
            $status.text("请输入短信验证码");
        }else {
            $status.removeClass("error-tips");
            $status.text("");
        }
    });

    // 获取短信验证码
    $("#get-sms-btn").click(function(){
        //判断图文验证码是否为空
        var picCode = $("#pic-code").val().length;
        if (picCode == 0) {
            alert("请输入验证码！");
        }else {
            var phone = $("#account").val();
            var picCode = $("#pic-code").val();
            $.ajax({
                url: "/tools/submit_ajax.ashx?action=user_register_smscode",
                type: "post",
                dataType: "json",
                data: {
                    mobile: phone,
                    txtPicCode: picCode
                },
                success: function (data) {
                    alert(data.msg);
                },
                error: function (data) {
                    alert("操作超时，请重试");
                }
            });
        }
    });

    // 注册
    $("#registerBtn").click(function(){
        if (!$("input[type=checkbox]")[0].checked) {
            alert("请先同意注册协议");
            return;
        }
        var txtPw1 = $("#psw").val(), txtPw2 = $("#psw2").val();
        if (txtPw1 != txtPw2) {
            alert("两次输入的密码不一致");
            return;
        }
        $.ajax({
            url: "/tools/submit_ajax.ashx?action=user_register",
            type: "post",
            dataType: "json",
            data: {
                txtSMSCode: $("#sms-code").val(),
                txtMobile: $("#account").val(),
                txtPassword: txtPw1,
                txtInviteCode: $("#recommended").val()
            },
            success: function(data){
                alert(data.msg);
                location.href="/";
            },
            error: function(data){
                alert("操作超时，请重试");
            }
        });
    });
});
