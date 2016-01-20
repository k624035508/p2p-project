import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/index.less";
import "../less/footer.less";

import header from "./header.js"
window['$'] = $;

$(function () {
    header.setHeaderHighlight(0);

    //计算器初始化
    $('[data-toggle="popover"]').popover();
});