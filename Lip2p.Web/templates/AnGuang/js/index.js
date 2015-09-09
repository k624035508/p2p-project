require("bootstrap-webpack");
require("../css/head.less");
require("../css/index.less");
require("../css/invest-list.css");
require("../css/footer.css");

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