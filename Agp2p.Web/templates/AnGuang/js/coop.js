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

    $("a.rongzi").hover(function(){
        $("a.teamwork").css("background","#1478d2");
    },function(){
        $("a.teamwork").css("background","#1e94ff");
    });
});