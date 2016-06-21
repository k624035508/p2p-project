import "bootstrap-webpack";
import "../less/safe_defence.less";
import "../less/footer.less";

import footerInit from "./footer.js";
import "./radialIndicator.min.js";
import template from "lodash/string/template"

/*rem的相对单位定义*/
var viewportWidth = $(window).width();
var fontSizeUnit = viewportWidth / 20;
$("html").css("font-size", fontSizeUnit);

var templateSettings = {
    evaluate: /@\{([\s\S]+?)\}@/g, // 求值 @{ ... }@
    interpolate: /@\{=([\s\S]+?)\}@/g // 两边带空格，直接输出 @{= ...}@
};
var render = template($("#invest-item").html(), templateSettings);


$(function (){
    footerInit();
    //安全措施
    var index = 0;
    $(".defence-link").eq(0).show().siblings().hide();
    $(".defence-nav a").click(function(){
        index = $(".defence-nav a").index(this);
        $(this).addClass("nav-active nav-border-active").siblings().removeClass("nav-active nav-border-active");
        $(".defence-link").eq(index).show().siblings().hide();       
    });
});