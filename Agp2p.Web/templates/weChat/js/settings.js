import "bootstrap-webpack";
import "../less/setting.less";
import "../less/footer.less";

import footerInit from "./footer.js";

/*rem的相对单位定义*/
var viewportWidth = $(window).width();
var fontSizeUnit = viewportWidth / 20;
$("html").css("font-size", fontSizeUnit);

$(function(){
    footerInit();
});
