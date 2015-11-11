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