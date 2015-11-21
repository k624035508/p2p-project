import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/index.less";
import "../less/invest-cell.less";
import "../less/footer.less";

import header from "./header.js"

$(function () {
    header.setHeaderHighlight(0);

    // 返回顶部浮窗隐藏与出现
    $(window).scroll(function () {
        var scrollTop = $(window).scrollTop();
        if (scrollTop >= 250) {
            $("#floating-top-wrap").show();
        } else {
            $("#floating-top-wrap").hide();
        }
    });

    //计算器初始化
    $('[data-toggle="popover"]').popover();

    window.calculate = function () {
        var money = $("#money").val();
        var rate = $("#rate").val();
        var time = $("#time").val();
        var profit = money * rate / 100 / 365 * time;
        var forProfit = profit.toFixed(2);
        $("#profit").html(forProfit);
    }

});