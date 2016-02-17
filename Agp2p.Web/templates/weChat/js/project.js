import "bootstrap-webpack";
import "../less/project.less";
import "../less/project-detail.less";
import "../less/invest-record.less";
import "../less/claims-photos.less";
import "../less/receive-plan-detail.less";
import "../less/invest-success.less";
import "../less/footer.less";
import "fullpage.js/jquery.fullPage.css"
import "fullpage.js"

import footerInit from "./footer.js";
import "./radialIndicator.min.js";

/*rem的相对单位定义*/
var viewportWidth = $(window).width();
var fontSizeUnit = viewportWidth / 20;
$("html").css("font-size", fontSizeUnit);

window['$'] = window['jQuery'] = $;

function initPage(){
    $('.indicatorContainer').radialIndicator({
        radius: 35,
        barColor: '#b8d6f1',
        barBgColor: '#fff',
        barWidth: 6,
        initValue: investProgress,
        roundCorner : true,
        percentage: true
    });

    $("#btnInvest").click(function() {
        // 初始化投资确认对话框
        let $investingValueInput = $(".invest-action input");
        var investingValue = Number($investingValueInput.val());
        if (0 >= investingValue || investingValue % 1 !== 0) {
            $.dialog.tips("请输入正整数");
            return;
        } else if (idleMoney < investingValue) {
            $.dialog.tips("您的余额不足 " + investingValue + " 元");
            return;
        }
        var profit = finalProfitRate * investingValue;

        var dlg = $("#dlgInvestConfirm");
        dlg.find("#dlgField_InvestAmount").text(investingValue + " 元");

        if ($investingValueInput[0].disabled && investingValue == 100) {
            // TODO 新手体验标的临时逻辑
            dlg.find("#dlgField_ProfitPredict").text("10 元");
        } else {
            dlg.find("#dlgField_ProfitPredict").text(profit.toFixed(2) + " 元");
        }

        dlg.find("#dlgField_IdleMoney").text(idleMoney + " 元");
        dlg.find("#dlgField_ProjectName").text(projectName);
        dlg.modal();
    });

    var investActionUrl = "/tools/submit_ajax.ashx?action=invest_project";
    $("#dlgInvestConfirm .btn-primary").click(function() { // 确认投资
        $.post(investActionUrl, {
            investingAmount: $(".invest-action input").val(),
            projectId: projectId,
            transactPassword: $("#transactPwd").val()
        }, function(data) {
            if (data.status === 0) {
                $.dialog.alert(data.msg);
            } else if (data.status === 3) {
                $.dialog.tips("投资成功！", 2, "32X32/succ.png", function(){
                    location.reload();
                });
                //$.dialog.alert(data.msg, function() {
                //    location.reload();
                //});
            } else {
                $.dialog.tips(data.msg, 2, "32X32/succ.png", function(){
                    location.reload();
                });
            }
        }, "json").fail(function(resp) {
            $.dialog.alert("投资失败，请重试。<br>" + resp.errorMessage);
        });
        return false;
    });
};
function initFullpage() {
    $('#fullpage').fullpage({
        anchors:['project'],
        controlArrows: false,
        verticalCentered: false,
        loopHorizontal: false,
        autoScrolling: false,
        fitToSection: false,
        onSlideLeave: function(){
            $(".section").scrollTop(0);
        }
    });
    $.fn.fullpage.setAllowScrolling(false);
    if (!location.hash) {
        if(history.pushState) {
            history.pushState(null, null, '#project');
        } else {
            location.href = '#project';
        }
    }
}
function initPics() {
    //照片大图弹出
    var zoomingImg = $("#photo-enlarge-dialog img");
    var dlgZooming = $('#photo-enlarge-dialog');
    $("div.photo-cell img").click(function() {
        zoomingImg.attr("src", $(this).attr("data-origin-src"));
        dlgZooming.modal();
    });
}
function fixAndroid2xOverflowCannotScroll($affectedElem) {
    var ua = navigator.userAgent;
    if (ua.indexOf("Android") >= 0) {
        var androidversion = parseFloat(ua.slice(ua.indexOf("Android") + 8));
        if (androidversion < 3) {
            var script = document.createElement('script');
            script.src = "<%templateskin%>/js/touchscroll.js";
            script.onload = function () {
                $affectedElem.each(function(index, elem) {
                    touchScroll(elem);
                });
            };
            document.head.appendChild(script);
        }
    }
}
$(function() {
    footerInit();
    initPage();
    initPics();
    initFullpage();
    fixAndroid2xOverflowCannotScroll($(".inner-scrollable"));
});