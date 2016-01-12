import "bootstrap-webpack!./bootstrap.config.js";

import "../less/head.less";
import "../less/footerSmall.less";
import "../less/projects.less";

import header from "./header.js"
window['$'] = $;

$(function(){
    header.setHeaderHighlight(1);

    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();
});