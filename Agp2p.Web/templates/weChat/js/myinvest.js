import "bootstrap-webpack";
import "../less/myinvest.css";
import "../less/footer.less";
import template from "lodash/string/template";
import footerInit from "./footer.js";

/*rem的相对单位定义*/
var viewportWidth = $(window).width();
var fontSizeUnit = viewportWidth / 20;
$("html").css("font-size", fontSizeUnit);

var templateSettings = {
    evaluate: /@\{([\s\S]+?)\}@/g, // 求值 @{ ... }@
    interpolate: /@\{=([\s\S]+?)\}@/g // 直接输出 @{= ...}@
};

var render = template($("#investment-record").html(), templateSettings);
function loadData(pageIndex, callback) {
    var pageSize = 15;
    var status = Number($("a.nav-active").attr("data-project-status"));
    var loadingHint = $("#loading-hint");
    var emptyBox = $("div.loading-hint");
    loadingHint.text("加载中...");
    loadingHint.show();
    emptyBox.hide();
    $.ajax({
        type: "POST",
        url: reqFilePath + "/AjaxQueryInvestment",
        data: JSON.stringify({ type: status, pageIndex, pageSize, startTime: "", endTime: "" }),
        contentType: "application/json",
        dataType: "json",
        success: function(msg) {
            var ls = JSON.parse(msg.d).data;
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
    }).fail(function() {
        loadingHint.text("加载失败");
        callback(false);
    });
}
$(function () {
    footerInit();

    // init
    var processing = false;
    var loadedPage = 0;
    loadData(loadedPage, function(succ) {
        if (succ) loadedPage += 1;
    });

    // auto load
    $(window).scroll(function () {
        if (processing) return;
        if ($(".nav-active").attr("data-loadCompleted") === "true") return; // load complete

        if (($(document).height() - $(window).height()) * 0.7 <= $(window).scrollTop()) {
            processing = true; //sets a processing AJAX request flag

            loadData(loadedPage, function (succ) {
                if (succ) loadedPage += 1;
                processing = false;
            });
        }
    });

    // changing tab
    $("div.nav-bar > a").click(function(){
        var clicked = $(this);
        if (clicked.hasClass("nav-active")) return;
        clicked.siblings().removeClass("nav-active nav-border-active");
        clicked.addClass("nav-active nav-border-active");
        $("a[data-loadCompleted]").removeAttr("data-loadCompleted");

        $("#pending").html("");
        loadedPage = 0;
        loadData(loadedPage, function(succ) {
            if (succ) loadedPage += 1;
        });
    });
});
