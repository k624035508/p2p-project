import "bootstrap-webpack!./bootstrap.config.js";

import "../less/head.less";
import "../less/footerSmall.less";
import "../less/projects.less";
import "../less/invest-cell.less";

import header from "./header.js"

$(function(){
    header.setHeaderHighlight(1);

    // 返回顶部浮窗隐藏与出现
    $(window).scroll(function () {
        var scrollTop = $(window).scrollTop();
        if (scrollTop >= 250) {
            $("#floating-top-wrap").show();
        } else {
            $("#floating-top-wrap").hide();
        }
    });

    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();
});