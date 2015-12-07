import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/aboutus.less";
import "../less/footerSmall.less";

import header from "./header.js";

$(function(){
    header.setHeaderHighlight(4);

    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();

    //公司简介图片放大浏览
    var $smallPic = $(".company-wrap .content-body .pic-wrap div");
    var $bigPic = $("#picModal .modal-dialog .modal-content .modal-body img");
    $smallPic.click(function(){
        var picSrc = $(this).data("pic");
        $bigPic.attr("src", picSrc);
    });
});