var $ = require("jquery");
require("bootstrap-webpack");
require("../css/head.css");
require("../css/invest_detail.less");
require("../css/footer.css");

$(function () {
    $(window).scroll(function () {
        var scrollTop = $(window).scrollTop();
        var rightNav = $(".project-content-right ul");
        if (scrollTop >= 560) {
            rightNav.removeClass("notScroll");
            rightNav.addClass("scrolled");
        } else {
            rightNav.removeClass("scrolled");
            rightNav.addClass("notScroll");
        }
    });
});