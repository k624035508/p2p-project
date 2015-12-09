import "bootstrap-webpack";
import "../less/aboutus.less";
import "../less/footer.less";
import footerInit from "./footer.js";

/*rem的相对单位定义*/
$("html").css("font-size", $(window).width() / 20);
$(function(){
    footerInit();
});