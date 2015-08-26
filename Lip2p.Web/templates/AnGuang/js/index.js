require("bootstrap-webpack");
require("../css/head.css");
require("../css/index.css");
require("../css/invest-list.css");
require("../css/footer.css");

var $ = require("jquery");

function iconHover(id, style, styleHover) {
    $(id).hover(function () {
        $(this).removeClass(style).addClass(styleHover);
    }, function () {
        $(this).removeClass(styleHover).addClass(style);
    })
}
$(function(){
    // floating icon hover
    iconHover("#floating-qq", "qq-icon", "qqHover-icon");
    iconHover("#floating-cal", "cal-icon", "calHover-icon");
    iconHover("#floating-top", "top-icon", "topHover-icon");

    //返回顶部浮窗隐藏与出现
    $(window).scroll(function () {
        var scrollTop = $(window).scrollTop();
        console.log(scrollTop);
        if (scrollTop >= 130) {
            $("#floating-top-wrap").show();
        } else {
            $("#floating-top-wrap").hide();
        }
    });
});