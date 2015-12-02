import "bootstrap-webpack!./bootstrap.config.js"
import "../less/head.less"
import "../less/aboutus.less"
import "../less/footerSmall.less"

import header from "./header.js"

$(function(){
    header.setHeaderHighlight(4);

    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();
});