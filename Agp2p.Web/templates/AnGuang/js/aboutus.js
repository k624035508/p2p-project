import "bootstrap-webpack"
import "../less/head.less"
import "../less/aboutus.less"
import "../less/footerSmall.less"

import header from "./header.js"

$(function(){
    header.setHeaderHighlight(4);

    var $nav = $(".aboutPage .left-nav .nav-list li a");
    var $parentLi = $(".aboutPage .left-nav .nav-list li");
    $nav.click(function(){
        $parentLi.removeClass("clicked");
        $(this).parent().addClass("clicked");
    });
});