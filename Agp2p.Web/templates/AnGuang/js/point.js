import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/point.less";
import "../less/footer.less";

import header from "./header.js";
window['jQuery'] = $;
window['$'] = $;

$(function() {
    header.setHeaderHighlight(3);   
})