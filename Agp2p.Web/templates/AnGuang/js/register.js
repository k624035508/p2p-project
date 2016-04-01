import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/register.less";
import "../less/footerSmall.less";

import alert from "../components/tips_alert.js";

$(function() {
    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();

    //根据步骤显示
    var { step } = $("#step").data();
    if (step == "2") {
        //实名验证
        $(".register-left").eq(0).addClass("hidden");
        $(".register-left").eq(1).removeClass("hidden");
        $(".step2-hr").addClass("step-red");
        $(".step2").addClass("redstep");
        $(".tips2").addClass("tips-red");
    } else if (step == "3"){
        //托管开户
        $(".register-left").eq(0).addClass("hidden");
        $(".register-left").eq(1).addClass("hidden");
        $(".register-left").eq(2).removeClass("hidden");
        $(".step3-hr").addClass("step-red");
        $(".step3").addClass("redstep");
        $(".tips3").addClass("tips-red");
    }

    //邀请码选填 
    $("div.invite2").hide();
    $("div.invited").click(function(){      
        if($("div.invite2").is(":visible")) {
            $("div.invite2").hide();
            $("div.invited span").css("transform","rotate(360deg)");
        }else{            
            $("div.invite2").show();
            $("div.invited span").css("transform","rotate(90deg)");
        }
    });

    // 检测邀请码
    var matchInvitationUrl = location.search.match(/inviteCode=([^&]+)/);
    if (matchInvitationUrl) {
        $("#recommended").val(matchInvitationUrl[1]).attr("disabled", "disabled");
    }

    // 手机号码格式判断
    $("#account").blur(function() {
        var regex = /^\d{11}$/;
        var phone = $("#account").val();
        var $status = $("#account").next();
        if (regex.test(phone)) {
            $status.removeClass("error-tips");
            $status.addClass("right-tips");
            $status.text("");
        } else {
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
        if(5 < num && num < 17) {
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
    $("#psw2").blur(function() {
        var psw = $("#psw").val();
        var psw2 = $("#psw2").val();
        var $status = $("#psw2").next();
        if (psw2 == "") {
            $status.removeClass("right-tips");
            $status.addClass("error-tips");
            $status.text("请输入6~16位密码");
        } else if (psw2 == psw) {
            $status.removeClass("error-tips");
            $status.addClass("right-tips");
            $status.text("");
        } else {
            $status.removeClass("right-tips");
            $status.addClass("error-tips");
            $status.text("两次输入的密码不一致");
        }
    });

    // 刷新图文验证码
    var $picCode = $(".pic-code-wrap img");
    var originSrc = $picCode[0].getAttribute("src");
    let refreshPicVerifyCode = () => {
        $picCode[0].setAttribute("src", originSrc + "?r=" + Math.random());
        $("#pic-code").val("");
        $("#sms-code").val("");
    }
    $picCode.click(refreshPicVerifyCode);

    //图文验证码格式判断
    $("#pic-code").blur(function() {
        var picCode = $("#pic-code").val().length;
        var $status = $("#pic-code").siblings(".status");
        if (picCode == 0) {
            $status.addClass("error-tips");
            $status.text("请输入图片上的验证码");
        } else {
            $status.removeClass("error-tips");
            $status.text("");
        }
    });

    //短信验证码格式判断
    $("#sms-code").blur(function() {
        var smsCode = $("#sms-code").val().length;
        var $status = $("#sms-code").siblings(".status");
        if (smsCode == 0) {
            $status.addClass("error-tips");
            $status.text("请输入短信验证码");
        } else {
            $status.removeClass("error-tips");
            $status.text("");
        }
    });

    // 获取短信验证码
    var timerId = 0, countDown = 0;
    var $sendSmsCodeBtn = $("#get-sms-btn");
    var intervalControlLogic = () => {
        var originalText = $sendSmsCodeBtn.text();
        countDown = 120;
        timerId = setInterval(() => {
            $sendSmsCodeBtn.text(`${countDown--}秒后再试`);
            if (countDown == 0) {
                clearInterval(timerId);
                timerId = 0;
                $sendSmsCodeBtn.text(originalText);
            }
        }, 1000);
    };
    $sendSmsCodeBtn.click(function() {
        //判断图文验证码是否为空
        var picCode = $("#pic-code").val();
        if (picCode.length == 0) {
            alert("请输入验证码！");
        } else if (0 < countDown) {
            alert(`请等待 ${countDown} 秒后再试`);
        } else {
            $.ajax({
                url: "/tools/submit_ajax.ashx?action=user_register_smscode",
                type: "post",
                dataType: "json",
                data: {
                    mobile: $("#account").val(),
                    txtPicCode: picCode
                },
                success: function (data) {
                    alert(data.msg);
                    if (data.status == 1) {
                        intervalControlLogic();
                    } else {
                        refreshPicVerifyCode();
                    }
                },
                error: function (data) {
                    alert("操作超时，请重试");
                }
            });
        }
    });
  
    //图片滚动
    var leng=$(".register-img").length;
    var index=0;
    var adTimer;
    $(".left-icon").click(function(){
        index--;
        if(index<0) {index=leng-1;}
        showImg(index);
    });
    $(".right-icon").click(function(){
        index++;
        if(index==leng) {index=0;}
        showImg(index);
    });
    $('.register-right').hover(function(){
        clearInterval(adTimer);
    },function(){
        adTimer = setInterval(function() {
            showImg(index);
            index++;
            if(index==leng){index=0;}
        } , 5000);
    }).trigger("mouseleave");
    function showImg(index){
        $(".register-img").eq(index).fadeIn(200).siblings().fadeOut(200);
    }

    // 注册
    $("#registerBtn").click(function() {
        if (!$("input[type=checkbox]")[0].checked) {
            alert("请先同意注册协议");
            return;
        }
        var txtPw1 = $("#psw").val(), txtPw2 = $("#psw2").val();
       
        if (txtPw1 != txtPw2) {
            alert("两次输入的密码不一致");
            return;
        } else if ($(".error-tips").length != 0) {
            alert("请先填写好表单");
            return;
        }

        var verifyCode = $(".register-box").data("needSmsVerify") ? $("#sms-code").val() : $("#pic-code").val();
        $.ajax({
            url: "/tools/submit_ajax.ashx?action=user_register",
            type: "post",
            dataType: "json",
            data: {
                txtVerifyCode: verifyCode,
                txtMobile: $("#account").val(),
                txtPassword: txtPw1,
                txtInviteCode: $("#recommended").val()
            },
            success: function(data) {
                alert(data.msg);
                if (data.status == 1) {
                    //注册成功到实名验证步骤
                    location.href="register.html?action=2";
                } else {
                    refreshPicVerifyCode();
                }
            },
            error: function(data) {
                alert("操作超时，请重试");
            }
        });
    });

    //实名认证
    
    $("#realNameAuthBtn").click(function(){  
      /*
        $.ajax({
              url: "/tools/submit_ajax.ashx?action=bind_idcard",
            
            type: "post",
            dataType: "json",
            data: {
                idCardNumber: $("#identify").val(),
                trueName: $("#realname").val()
            },
            beforeSend:function(XMLHttpRequest){ 
                $("#realNameAuthBtn").hide(); //在后台返回success之前显示loading图标
                $(".nameLoading").show().html("<img src='/templates/AnGuang/imgs/register/loading2.gif' />");
            }, 
            success: function(data) {
                $("#realNameAuthBtn").show();
                $(".nameLoading").empty();
                if (data.status == 1) {
                    //实名验证成功，进入开户步骤
                    location.href="register.html?action=3";
                } else {
                    alert(data.msg);
                }
            },
            error: function(data) {
                $("#realNameAuthBtn").show();
                $(".nameLoading").empty();
                alert("操作超时，请重试");
            }
        });
        */


        $.ajax({
            url:"",
            type: "post",
            dataType: "json",
            data: {
                idCardNumber: $("#identify").val(),
                trueName: $("#realname").val()
            },
            beforeSend:function(XMLHttpRequest){ 
                $("#realNameAuthBtn").hide(); //在后台返回success之前显示loading图标
                $(".nameLoading").show().html("<img src='/templates/AnGuang/imgs/register/loading2.gif' />");
            }, 
            success: function(data) {
                $("#realNameAuthBtn").show();
                $(".nameLoading").empty();
                if (data.status == 1) {
                    //实名验证成功，进入开户步骤
                    location.href="register.html?action=3";
                } else {
                    alert(data.msg);
                }
            },
            error: function(data) {
                $("#realNameAuthBtn").show();
                $(".nameLoading").empty();
                alert("操作超时，请重试");
            }
        });
    });   
});
