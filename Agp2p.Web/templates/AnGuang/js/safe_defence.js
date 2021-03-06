import "bootstrap-webpack!./bootstrap.config.js"
import "../less/head.less"
import "../less/safe_defence.less"
import "../less/footerSmall.less"

import header from "./header.js";
window['$'] = $;

let selectedTabByHash = () => {
    var search = location.hash || "#tab0";
    var match = search.match(/#tab(\d+)/);
    var tabIndex = parseInt(match ? match[1] : "0");
    $(".left-nav .nav-list li a").eq(tabIndex).click();
};

$(function(){
    header.setHeaderHighlight(5);

    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();

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

    $(".project").click(function(){
        $nav.removeClass("clicked");
        $(this).parent().addClass("clicked");

        $rightContent.hide();
        $(".project-wrap").show();
    });

    $(".law").click(function(){
        $nav.removeClass("clicked");
        $(this).parent().addClass("clicked");

        $rightContent.hide();
        $(".law-wrap").show();
    });

    selectedTabByHash();

    //url加载
    $(window).load(function() {
        var url = location.href;
        var loca = url.indexOf("#");
        var index = url.substring(loca+1);
        $(".left-nav .nav-list li").eq(index).addClass("clicked").siblings().removeClass("clicked");
        if (index == 1) {          
            $rightContent.hide();
            $(".data-wrap").show();
        } 
        if (index == 2) {        
            $rightContent.hide();
            $(".project-wrap").show();
        }
        
    });
});