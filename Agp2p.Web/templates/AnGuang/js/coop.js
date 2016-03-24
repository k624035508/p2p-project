import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/coop.less";
import "../less/footerSmall.less";
import alert from "../components/tips_alert.js";

import header from "./header.js";

window['$'] = $;

$(function () {
    header.setHeaderHighlight(5);

    //data-toggle 初始化
    $('[data-toggle="popover"]').popover();
    $("a.rongzi").click(function(){
        $(this).addClass("blue-bg").siblings().removeClass("blue-bg");
        $(".cooperation2").slideDown();
        $(".cooperation").slideUp();
    });
    $("a.teamwork").click(function(){
        $(this).addClass("blue-bg").siblings().removeClass("blue-bg");
        $(".cooperation").slideDown();
        $(".cooperation2").slideUp();
    }).trigger("click");
});