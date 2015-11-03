import "es5-shim/es5-shim"
import "es5-shim/es5-sham"

import "bootstrap-webpack";
import $ from "jquery";

import "../less/head.less";
import "../less/footerSmall.less";
import "../less/invest_list.less";
import "../less/invest-cell.less";

import { setHeaderHighlight } from "./header.js"

$(function(){
    setHeaderHighlight(1);
});