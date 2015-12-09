import "bootstrap-webpack";
import "../less/mylottery.css";
import "../less/footer.less";
import footerInit from "./footer.js";

/*rem的相对单位定义*/
var viewportWidth = $(window).width();
var fontSizeUnit = viewportWidth / 20;
$("html").css("font-size", fontSizeUnit);

$(function () {
    footerInit();

    var navBtns = $("div.nav-bar a");
    //$("#receiving").addClass("nav-active nav-border-active");
    navBtns.click(function () {
        navBtns.removeClass("nav-active nav-border-active");
        $(this).addClass("nav-active nav-border-active");
    });

    $($('.type')[lotteryStatus-1]).addClass('nav-active nav-border-active');
});