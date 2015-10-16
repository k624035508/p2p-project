import "bootstrap-webpack";
import $ from "jquery";

import "../less/head.less";
import "../less/footerSmall.less";
import "../less/invest.less";
import "../less/invest-list.less";

import { setHeaderHighlight } from "./header.js"

$(function(){
    setHeaderHighlight(1);
});