import "bootstrap-webpack"
import "../less/head.less"
import "../less/aboutus.less"
import "../less/footerSmall.less"

import header from "./header.js"

function render() {
    var hash = location.hash || "#tab=0";
    var match = hash.match(/#tab=(\d+)/);
    var tabIndex = parseInt(match ? match[1] : "0");

    var $navArray = $(".left-nav .nav-list li");
    $navArray.removeClass("clicked");
    $navArray.eq(tabIndex).addClass("clicked");

    var $contents = $(".right-content > div");
    $contents.hide();

    var contentClass = $navArray.eq(tabIndex).data("binding");
    $("." + contentClass).show();
}

$(function(){
    header.setHeaderHighlight(4);

    render();
    window.onhashchange = function () {
        render();
    };

    //加入我们 招聘列表开关样式
    var $office = $(".join-us-wrap .content-body .office ul li");
    $office.click(function(){
        $(this).find("i").toggleClass("glyphicon-menu-up");
        $(this).children(".office-detail").toggle();
    });

    //公司简介图片放大浏览
    var $smallPic = $(".company-wrap .content-body .pic-wrap div");
    var $bigPic = $("#picModal .modal-dialog .modal-content .modal-body img");
    $smallPic.click(function(){
        var picSrc = $(this).data("pic");
        $bigPic.attr("src", picSrc);
    });
});