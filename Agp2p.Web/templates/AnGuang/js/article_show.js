import "bootstrap-webpack";
import "../less/head.less";
import "../less/article_show.less";
import "../less/footerSmall.less";

import header from "./header.js"

$(function(){
    header.setHeaderHighlight(4);
});