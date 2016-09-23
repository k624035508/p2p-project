import "bootstrap-webpack";
import "../less/projects.less";
import "../less/invest-list.less";
import "../less/footer.less";

import footerInit from "./footer.js";
import "./radialIndicator.min.js";
import template from "lodash/string/template"

/*rem的相对单位定义*/
var viewportWidth = $(window).width();
var fontSizeUnit = viewportWidth / 20;
$("html").css("font-size", fontSizeUnit);

var templateSettings = {
    evaluate: /@\{([\s\S]+?)\}@/g, // 求值 @{ ... }@
    interpolate: /@\{=([\s\S]+?)\}@/g // 两边带空格，直接输出 @{= ...}@
};
var render = template($("#invest-item").html(), templateSettings);
function loadData(pageIndex, callback) {
    var pageSize = 10;
    var loadingHint = $("#loading-hint");
    var emptyBox = $("div.loading-hint");
    var newbieBox = $("div.newbie-box");
    loadingHint.text("加载中...");
    loadingHint.show();
    emptyBox.hide();
    $.ajax({
        type: "POST",
        url: aspxPath + "/AjaxQueryProjectList",
        data: JSON.stringify({ categoryId, pageIndex, pageSize, profitRateIndex, repaymentIndex, statusIndex }),
        contentType: "application/json",
        dataType: "json",
        success: function (msg) {
            var ls = JSON.parse(msg.d);
            $("#pending").append(render({ items: ls }));
            if (ls.length < pageSize) {
                $(".scroll").unbind('scroll');
            }
            loadingHint.hide();
            if (pageIndex === 0 && ls.length === 0) {
                emptyBox.show();

            } else {
                emptyBox.hide();
            }
        }
    }).fail(function() {
        loadingHint.text("加载失败");
    }).always(callback);
}

$(function (){
    footerInit();

    var navBtns = $(".invest-nav a");
    //首次进入页面加载数据
    var processing = false;
    var loadedPage = 0;
    loadData(loadedPage, function () {
        loadedPage += 1;
        //百分比样式
        $('.indicatorContainer').each(function (index, obj) {
            $(obj).radialIndicator({
                radius: 28,
                barColor: '#ff414b',
                barBgColor: '#d9d9d9',
                barWidth: 2,
                initValue: $(obj).attr('data-progress'),
                roundCorner: true,
                percentage: true
            });
        });
    });
    //拖动滚动条加载数据
    var $scrollable = $(".scroll");
    $scrollable.scroll(function() {
        if (processing) return;

        if (($scrollable[0].scrollHeight - $scrollable.height())*0.9 <= $scrollable.scrollTop()) {
            processing = true; //sets a processing AJAX request flag

            loadData(loadedPage, function() {
                loadedPage += 1;
                processing = false;
            });
        }
    });

    //头部按钮
    $(".projects-top .projects-one").click(function(){
        $(this).find(".dropdown-menu").show().parent().siblings().find(".dropdown-menu").hide();
        $(this).find("span").addClass("topping").parent().siblings().find("span").removeClass("topping");
    });
});