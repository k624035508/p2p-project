import "bootstrap-webpack"
import "../less/head.less"
import "../less/safe_defence.less"
import "../less/footerSmall.less"

import header from "./header.js"

$(function(){
    header.setHeaderHighlight(3);

    var $nav = $(".left-nav .nav-list li");
    var $rightContent = $(".right-content > div");
    $(".funds").click(function(){
        $nav.removeClass("clicked");
        $(this).parent().addClass("clicked");

        $rightContent.hide();
        $(".funds-wrap").show();
    });

    $(".data").click(function(){
        $nav.removeClass("clicked");
        $(this).parent().addClass("clicked");

        $rightContent.hide();
        $(".data-wrap").show();
    });

    $(".law").click(function(){
        $nav.removeClass("clicked");
        $(this).parent().addClass("clicked");

        $rightContent.hide();
        $(".law-wrap").show();
    });
});