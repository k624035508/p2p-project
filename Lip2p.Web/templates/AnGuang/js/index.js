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
});