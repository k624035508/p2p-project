import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/questionnaire.less";
import "../less/footerSmall.less";
import alert from "../components/tips_alert.js";

import header from "./header.js";
window['$'] = $;

$(function() {
    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();

    //加载投资者类型
    var step = $("#step").data("step");      
        if (step >= 31) {
            $(".typeMiddle").html("积极型");
            $(".typeP1").html("书山路，勤为径，");
            $(".typeP2").html("循序渐进齐头进。");
            $(window).scrollTop(600);
            $(".investAlert").css("z-index", "10");
        }
        if (step >= 21 && step <= 30) {
            $(".typeMiddle").html("稳健型");
            $(".typeP1").html("黄叶落，白云扫，");
            $(".typeP2").html("稳健安全才最好。");
            $(window).scrollTop(600);
            $(".investAlert").css("z-index", "10");
        }
        if (step <= 20 && step > 0) {
            $(".typeMiddle").html("保守型");
            $(".typeP1").html("雷不动，打不动，");
            $(".typeP2").html("别人咋劝都没用。");
            $(window).scrollTop(600);
            $(".investAlert").css("z-index", "10");
        }
        
    //ie8兼容单选框样式
    $("label").click(function() {
        $(this).addClass("radioChecked");
        $(this).parent().siblings().find("label").removeClass("radioChecked");
    });

    //提交表格
        $("#questionnaireBtn").click(function() {
                var resualt = false;
                var radios = $(".question-list").length;
            for (var i = 0; i < radios; i++) {
                if ($(".question-list").eq(i).find("label").hasClass("radioChecked")) {
                    resualt = true;
                }
                else {
                    var indexq = i + 1;
                    alert("问题" + indexq + "还没选择");
                    return resualt;
                }
            }


            var url = USER_CENTER_ASPX_PATH + "/AjaxSaveQuestionnaireResult";        
        var radioLen = $(".radioChecked").prev().length;
        var answers = new Array(radioLen);
        for (var i = 0; i < radioLen; i++) {
            answers[i] = "\"" + $(".radioChecked").prev().eq(i).val() + "\"";
        }                  
        answers = "\[" + answers.join() + "\]";
             $.ajax({
                 url: url,
                 type: "post",
                 dataType: "json",
                 contentType: "application/json",
                 data: JSON.stringify({
                      questionnaireId : 1,
                      result : answers
                 }),
                 success:function(data) {
                     let score  = JSON.parse(data.d);
                     if (score >= 31) {
                         $(".typeMiddle").html("积极型");
                         $(".typeP1").html("书山路，勤为径，");
                         $(".typeP2").html("循序渐进齐头进。");
                     }
                     if (score >= 21 && score <= 30) {
                         $(".typeMiddle").html("稳健型");
                         $(".typeP1").html("黄叶落，白云扫，");
                         $(".typeP2").html("稳健安全才最好。");
                     }
                     if ( score <= 20) {
                         $(".typeMiddle").html("保守型");
                         $(".typeP1").html("雷不动，打不动，");
                         $(".typeP2").html("别人咋劝都没用。");
                     }
                     $(window).scrollTop(600);
                     $(".investAlert").css("z-index", "10");
                     
                 },
                 error: function (data) {
                     alert("请登录");
                 }
             }); 
        });

   
});
