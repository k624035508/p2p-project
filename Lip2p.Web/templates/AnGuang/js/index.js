require("bootstrap-webpack");
require("../less/head.less");
require("../less/index.less");
require("../less/invest-list.less");
require("../less/footer.less");

var $ = require("jquery");

var header = require("./header.js");

$(function(){
    //返回顶部浮窗隐藏与出现
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