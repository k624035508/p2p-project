import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/loan.less";
import "../less/footerSmall.less";

import header from "./header.js";

import React from "react";
import ReactDom from "react-dom";
import DropdownPicker from "../components/dropdown-picker.jsx";

window['$'] = $;

$(function () {
    header.setHeaderHighlight(5);

    //data-toggle 初始化
    $('[data-toggle="popover"]').popover();

    var { step,userId,userName,loanerId,pendingProjectId,quotaUse } = $("#main").data();

    var $step1 = $("ul.application-ul li.step1");
    var $step2 = $("ul.application-ul li.step2");
    var $step3 = $("ul.application-ul li.step3");
    var $forms = $(".form-wrapper > div");
    var $login = $(".form-wrapper div.login-form-wrap");
    var $personalInfo = $(".form-wrapper div.personal-info-form-wrap");
    var $loanDetail = $(".form-wrapper div.loan-detail-form-wrap");
    var ddlSelectIndex = 0;

    $forms.hide();
    if (step == "1") {
        //显示登录步骤
        $login.show();
    } else if(step.toString().indexOf("2")!=-1) {
        //显示申请成为借款人步骤
        var { loanerName,loanerMobile,loanerAge,loanerEducationalBackground,loanerIncome
            ,loanerJob,loanerMaritalStatus,loanerNativePlace,loanerWorkingAt,loanerWorkingCompany } = $("#loaner").data();
        $("#name").val(loanerName);
        $("#phone").val(loanerMobile);
        $("#birthplace").val(loanerNativePlace);
        $("#job").val(loanerJob);
        $("#work-place").val(loanerWorkingAt);
        $("#employer").val(loanerWorkingCompany);
        $("#education").val(loanerEducationalBackground);
        $("#marital-status").val(loanerMaritalStatus);
        $("#income").val(loanerIncome);
        $('div.personal-info-form-wrap div.status').hide();


        if(step != "2"){
            $(".form-wrapper form.personal-info-form input").attr("readonly","readonly");
            $("#marital-status").attr("disabled","disabled");
            $('#loanerApplyBtn').hide();

            if(step == "21"){
                //显示借款人审核中步骤
                $('#loanerApplyChecking').show();
            }else if(step == "22"){
                //显示借款人审核失败，重新提交步骤
                $('#loanerApplyFailed').show();
            }else if(step == "23"){
                //显示禁止再申请借款人步骤
                $('#loanerApplyForbid').show();
            }
        }                
        $step2.css("background","url('/templates/AnGuang/imgs/loan/personal-info.png') no-repeat");
        $personalInfo.show();
    } else if(step == "5") {
        //显示借款完成步骤

        
    } else {
        //显示申请借款步骤
        $step3.css("background","url('/templates/AnGuang/imgs/loan/loan.png') no-repeat");
        $loanDetail.show();
        var { projectCategoryId,projectAmount,projectLoanUsage,projectSourceOfRepayment,projectLoanerContent } = $("#project").data();


        if(step == "3") {            
            $("#loan-amount").val(quotaUse);
            $('div.loan-detail-form-wrap div.status').hide();

        } else if(step == "4") {
            //显示借款审核中步骤
            $loanDetail.show();
            $('#loanApplyChecking').show();

            $(".form-wrapper form.loan-detail-form textarea").attr("readonly","readonly");
            $("#loan-amount").attr("readonly","readonly");            
            $("#ddl_project_type").attr("disabled","disabled");
            
            $("#loan-amount").val(projectAmount);
            $("#ddl_project_type").val(projectCategoryId);
            $("#loan-description").val(projectLoanerContent);
            $("#loan-usage").val(projectLoanUsage);
            $("#repayment-source").val(projectSourceOfRepayment);

            $("#loanApplyBtn").hide();
        } 
    }

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
            error: function(xhr, status, err){
                alert("操作超时，请重试。");
            }
        });        
    });

    //提交借款人申请
    $("#loanerApplyBtn").click(function(){
        $.ajax({
            type: "post",
            url: "/aspx/main/loan.aspx/ApplyLoaner",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({
                userId:userId,
                age:0,
                native_place:$("#birthplace").val(),
                job:$("#job").val(),
                working_at:$("#work-place").val(),
                working_company:$("#employer").val(),
                educational_background:$("#education").val(),
                marital_status:$("#marital-status").val(),
                income:$("#income").val(),
            }),
            success: function(data){
                location.reload();
            },
            error: function(xhr, status, err){                
                alert(xhr.responseJSON.Message);
            }
        });          
    });

    //提交借款申请
    $("#loanApplyBtn").click(function(){
        $.ajax({
            type: "post",
            url: "/aspx/main/loan.aspx/ApplyLoan",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({
                loaner_id:loanerId,
                user_name:userName,
                loaner_content:$("#loan-description").val(),
                loan_usage:$("#loan-usage").val(),
                source_of_repayment:$("#repayment-source").val(),
                category_id:$("#ddl_project_type").val(),
                amount:$("#loan-amount").val()
            }),
            success: function(data){
                location.reload();
            },
            error: function(xhr, status, err){                
                alert(xhr.responseJSON.Message);
            }
        });          
    });
    

});