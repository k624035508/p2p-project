import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/lottery.less";
import "../less/footerSmall.less";

window['jQuery'] = $;
window['$'] = $;
import header from "./header.js";

$(function() {
    header.setHeaderHighlight(3);
});
