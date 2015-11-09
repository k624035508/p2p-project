import "bootstrap-webpack";

import "../less/head.less";
import "../less/footerSmall.less";
import "../less/projects.less";
import "../less/invest-cell.less";

import header from "./header.js"

$(function(){
    header.setHeaderHighlight(1);
});