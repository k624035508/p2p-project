import "bootstrap-webpack";
import "../less/projects.less";
import "../less/invest-list.less";
import "../less/footer.less";

import footerInit from "./footer.js";
import _ from "underscore";
import "./radialIndicator.min.js";

/*rem的相对单位定义*/
var viewportWidth = $(window).width();
var fontSizeUnit = viewportWidth / 20;
$("html").css("font-size", fontSizeUnit);

_.templateSettings = {
    evaluate: /@\{([\s\S]+?)\}@/g, // 求值 @{ ... }@
    interpolate: /@\{=([\s\S]+?)\}@/g // 两边带空格，直接输出 @{= ...}@
};
var render = _.template($("#invest-item").html());
function loadData(pageIndex, callback) {
    var pageSize = 10;
    var loadingHint = $("#loading-hint");
    var emptyBox = $("div.loading-hint");
    loadingHint.text("加载中...");
    loadingHint.show();
    emptyBox.hide();
    $.ajax({
        type: "POST",
        url: aspxPath + "/AjaxQueryProjectList",
        data: JSON.stringify({ category_id: categoryId, pageIndex: pageIndex, pageSize: pageSize }),
        contentType: "application/json",
        dataType: "json",
        success: function (msg) {
            var ls = JSON.parse(msg.d);
            $("#pending").append(render({ items: ls }));
            if (ls.length < pageSize) {
                $(window).unbind('scroll');
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
    $("#projects").addClass("nav-active nav-border-active");
    navBtns.click(function(){
        navBtns.removeClass("nav-active nav-border-active");
        $(this).addClass("nav-active nav-border-active");
    });
    //首次进入页面加载数据
    var processing = false;
    var loadedPage = 1;
    loadData(loadedPage, function () {
        loadedPage += 1;
        //百分比样式
        $('.indicatorContainer').each(function (index, obj) {
            $(obj).radialIndicator({
                radius: 28,
                barColor: '#fd6500',
                barBgColor: '#d9d9d9',
                barWidth: 2,
                initValue: $(obj).attr('data-progress'),
                roundCorner: true,
                percentage: true
            });
        });
        //分类样式选择
        $(".invest-nav a").removeClass("nav-active nav-border-active");
        if(categoryId==33) $("#golden-house").attr("class", "nav-active nav-border-active");
        else if(categoryId==34) $("#cars-invest").attr("class", "nav-active nav-border-active");
        else $("#projects").attr("class", "nav-active nav-border-active");
    });
    //拖动滚动条加载数据
    $(window).scroll(function() {
        if (processing) return;

        if (($(document).height() - $(window).height())*0.7 <= $(window).scrollTop()) {
            processing = true; //sets a processing AJAX request flag

            loadData(loadedPage, function() {
                loadedPage += 1;
                processing = false;
            });
        }
    });
});