import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/aboutus.less";
import "../less/footerSmall.less";

import header from "./header.js";
import alert from "../components/tips_alert.js";
window['$'] = $;

$(function(){
    header.setHeaderHighlight(4);

    //.about-nav-bar position fixed
    $(window).scroll(function () {
        var scrollTop = $(window).scrollTop();
        if (scrollTop >= 130) {
            $(".about-nav-bar").addClass("scroll-style");
        } else {
            $(".about-nav-bar").removeClass("scroll-style");
        }
    });

    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();

    //公司资质营业执照点击放大
    var $smallPic = $("#qualification .business-license");
    var $bigPic = $("#picModal .modal-body img");
    $smallPic.click(function(){
        var picSrc = $(this).data("pic");
        $bigPic.attr("src", picSrc);
    });

    //员工风采、办公环境照片墙点击放大浏览
    var $nailOffice = $("#office ul.picWall li a");
    var $nailStaff =  $("#staff ul.picWall li a");
    var $picBox = $("#picWall .modal-body img");

    var $currentPics = null;
    var currentPicIndex = -1;

    var enlarge = function($pics) {
        return function  () {
            $currentPics = $pics;
            currentPicIndex = $.inArray(this, $pics);

            var picSrc = $(this).data("src");
            var $picBox = $("#picWall .modal-body img");
            $("#picWall").modal();

            $picBox.attr("src", picSrc);

            $picBox.one("load", () => {
                var offsetHeight = ($(window).height() - $("#picWall .modal-content").height()) / 2;
                $("#picWall .modal-dialog").css("margin-top", offsetHeight + "px");
            }).each(function() {
                if(this.complete) $(this).load();
            });
        };
    }

    $nailOffice.click(enlarge($nailOffice));
    $nailStaff.click(enlarge($nailStaff));

    var $prev = $("#picWall .modal-body .prev");
    var $next = $("#picWall .modal-body .next");
    $prev.click(function(){
        if(currentPicIndex == 0){
            alert("已经是第一张");
        } else {
            currentPicIndex = currentPicIndex - 1;
            var prevPic = $currentPics.eq(currentPicIndex).data("src");
            $picBox.attr("src", prevPic);
        }
    });

    $next.click(function(){
        if(currentPicIndex == $currentPics.length - 1){
            alert("已经是最后一张");
        } else {
            currentPicIndex = currentPicIndex + 1;
            var nextPic = $currentPics.eq(currentPicIndex).data("src");
            $picBox.attr("src", nextPic);
        }
    });
});