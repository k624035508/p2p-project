require("bootstrap-webpack");
require("../less/head.less");
require("../less/index.less");
require("../less/invest-list.less");
require("../less/footer.less");

var $ = require("jquery");

$(function(){
    //返回顶部浮窗隐藏与出现
    $(window).scroll(function () {
        var scrollTop = $(window).scrollTop();
        console.log(scrollTop);
        if (scrollTop >= 200) {
            $("#floating-top-wrap").show();
        } else {
            $("#floating-top-wrap").hide();
        }
    });
});