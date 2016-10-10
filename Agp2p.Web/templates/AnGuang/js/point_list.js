import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/point_list.less";
import "../less/footerSmall.less";

import header from "./header.js";
window['$'] = $;

$(function(){
    header.setHeaderHighlight(3);
})