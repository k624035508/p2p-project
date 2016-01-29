import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/about_more.less";
import "../less/footerSmall.less";

import header from "./header.js";
window['$'] = $;

$(function(){
    header.setHeaderHighlight(4);

    //加入我们 招聘列表开关样式
    var $office = $(".join-us-wrap .content-body .office ul li");
    $office.click(function(){
        $(this).find("i").toggleClass("glyphicon-menu-up");
        $(this).children(".office-detail").toggle();
    });
    $('[data-toggle="popover"]').popover();
});