import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/loan.less";
import "../less/footerSmall.less";

import header from "./header.js";

window['$'] = $;

$(function () {
    header.setHeaderHighlight(5);

    //data-toggle 初始化
    $('[data-toggle="popover"]').popover();

    var { step,userId,userName,loanerId,pendingProjectId,quotaUse } = $("#main").data();

    var $step1 = $("ul.application-nav li.step1");
    var $step2 = $("ul.application-nav li.step2");
    var $step3 = $("ul.application-nav li.step3");
    var $forms = $(".form-wrapper form");
    var $login = $(".form-wrapper form.login-form");
    var $personalInfo = $(".form-wrapper form.personal-info-form");
    var $loanDetail = $(".form-wrapper form.loan-detail-form");

    $forms.hide();
    if (step == 1) {
        $login.show();
    } else if(step == 2) {
        var { loanerName,loanerMobile } = $("#loaner").data();
        $("#name").val(loanerName);
        $("#phone").val(loanerMobile);

        $personalInfo.show();
    } else if(step == 3) {
        $loanDetail.show();
    } 

    $step1.click(function(){
        $forms.hide();
        $login.show();
    });

    $step2.click(function(){
        $forms.hide();
        $personalInfo.show();
    });

    $step3.click(function(){
        $forms.hide();
        $loanDetail.show();
    });

    //登陆
    $("#loginBtn").click(function(){
        $.ajax({
            type: "post",
            url: "/tools/submit_ajax.ashx?action=user_login",
            dataType: "json",
            data: {
                txtUserName: $("#user-name").val(),
                txtPassword: $("#user-pwd").val(),
                chkRemember: true
            },
            success: function(data){
                if(data.status == 1){
                    location.reload();
                } else {
                    alert(data.msg);
                }
            },
            error: function(data){
                alert("操作超时，请重试");
            }
        });
        /*if (document.addEventListener) { //  >=ie9
            // 记住帐号
            if ($("input[type=checkbox]").is(":checked")) {
                localStorage.setItem("webLogin_UserName", $("#account").val());
            } else {
                localStorage.removeItem("webLogin_UserName");
            }
        }*/
    });
});