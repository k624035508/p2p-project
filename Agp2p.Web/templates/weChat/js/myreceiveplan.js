import "bootstrap-webpack";
import "../less/receive-plan.less";
import "../less/receive-plan-detail.less";
import "../less/footer.less";
import "fullpage.js/jquery.fullPage.css";
import "fullpage.js";
import template from "lodash/string/template";
import footerInit from "./footer.js";

window['$'] = window['jQuery'] = $;

/*rem的相对单位定义*/
$("html").css("font-size", $(window).width() / 20);

var templateSettings = {
    evaluate: /@\{([\s\S]+?)\}@/g, // 求值 @{ ... }@
    interpolate: /@\{=([\s\S]+?)\}@/g // 直接输出 @{= ...}@
};

var render = template($("#repaying-project-fragment").html(), templateSettings);
function loadData(pageIndex, callback) {
    var pageSize = 15;
    var projectStatus = $("a.nav-active").data("projectStatus");
    var loadingHint = $(".loading-hint");
    var emptyBox = $("div.empty-box");
    loadingHint.text("加载中...");
    loadingHint.show();
    emptyBox.hide();
    $.ajax({
        type: "POST",
        url: reqFilePath + "/AjaxQueryInvestedProject",
        data: JSON.stringify({ projectStatus, pageIndex, pageSize }),
        contentType: "application/json",
        dataType: "json",
        success: function (msg) {
            var ls = JSON.parse(msg.d);
            $("#pending").append(render({ items: ls }));
            if (ls.length < pageSize) {
                $(".nav-active").attr("data-loadCompleted", "true"); // disable auto load for this page
            }
            loadingHint.hide();
            if (pageIndex === 0 && ls.length === 0) {
                emptyBox.show();
            } else {
                emptyBox.hide();
            }
            callback(true);
        }
    }).fail(function () {
        loadingHint.text("加载失败");
        callback(false);
    });
}
var renderDetail = template($("#detail-page").html(), templateSettings);
function loadDetailData(projectId, ticketId) {
    var pageWrapper = $(".receive-plan-detail-page");
    pageWrapper.html("<div class='loading-hint'>加载中...</div>");
    $.ajax({
        type: "POST",
        url: reqFilePath + "/AjaxQueryProjectRepaymentDetail",
        data: JSON.stringify({ projectId: projectId, ticketId: ticketId }),
        contentType: "application/json",
        dataType: "json",
        success: function (msg) {
            var pro = JSON.parse(msg.d);
            pageWrapper.html(renderDetail({ project: pro }));
        }
    }).fail(function () {
        pageWrapper.html("<div class='loading-hint'>加载失败</div>");
    });
}
function initFullpage() {
    $('#fullpage').fullpage({
        anchors: ['projects'],
        controlArrows: false,
        verticalCentered: false,
        loopHorizontal: false,
        autoScrolling: false,
        fitToSection: false
    });
    $.fn.fullpage.setAllowScrolling(false);
    if (location.hash != '#projects') {
        if (history.pushState) {
            history.pushState(null, null, '#projects');
        } else {
            location.href = '#projects';
        }
    }
}
function fixAndroid2xOverflowCannotScroll($affectedElem) {
    var ua = navigator.userAgent;
    if (ua.indexOf("Android") >= 0) {
        var androidversion = parseFloat(ua.slice(ua.indexOf("Android") + 8));
        if (androidversion < 3) {
            var script = document.createElement('script');
            script.src = templateSkin + "/js/touchscroll.js";
            script.onload = function () {
                $affectedElem.each(function (index, elem) {
                    touchScroll(elem);
                });
            };
            document.head.appendChild(script);
        }
    }
}
$(function(){
    initFullpage();
    footerInit();

    // init
    var processing = false;
    var loadedPage = 0;
    loadData(loadedPage, function (succ) {
        if (succ) loadedPage += 1;
    });

    // auto load
    $(".inner-scrollable").scroll(function () {
        if (processing) return;
        if ($(".nav-active").attr("data-loadCompleted") === "true") return; // load complete

        if (($(document).height() - $(this).height()) * 0.7 <= $(this).scrollTop()) {
            processing = true; //sets a processing AJAX request flag

            loadData(loadedPage, function (succ) {
                if (succ) loadedPage += 1;
                processing = false;
            });
        }
    });

    // changing tab
    $("div.nav-bar a").click(function(){
        var clicked = $(this);
        if (clicked.hasClass("nav-active")) return;
        clicked.siblings().removeClass("nav-active nav-border-active");
        clicked.addClass("nav-active nav-border-active");
        $("a[data-loadCompleted]").removeAttr("data-loadCompleted");

        $("#pending").html("");
        loadedPage = 0;
        loadData(loadedPage, function (succ) {
            if (succ) loadedPage += 1;
        });
    });

    $("#pending").on("click", ".project-cell", function () {
        var clicked = $(this);
        loadDetailData(clicked.attr("data-projectId"), clicked.attr("data-ticketId"));
        location.href = "#projects/1";
    });

    fixAndroid2xOverflowCannotScroll($(".inner-scrollable"));
});