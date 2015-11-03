import "es5-shim/es5-shim"
import "es5-shim/es5-sham"

import "bootstrap-webpack";
import "../less/head.less";
import "../less/index.less";
import "../less/invest-cell.less";
import "../less/footer.less";

import $ from "jquery"
import header from "./header.js"

$(function () {
    // 返回顶部浮窗隐藏与出现
    $(window).scroll(function () {
        var scrollTop = $(window).scrollTop();
        if (scrollTop >= 250) {
            $("#floating-top-wrap").show();
        } else {
            $("#floating-top-wrap").hide();
        }
    });

    header.setHeaderHighlight(0);
});