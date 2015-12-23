import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/help.less";
import "../less/footerSmall.less";

window['$'] = $;

$(function(){
    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();

    var search = location.search || "?tab=0";
    var match = search.match(/\?tab=(\d+)/);
    var tabIndex = parseInt(match ? match[1] : "0");

    if (tabIndex == 2) {
    	$(".left-nav a.charge").closest("li").addClass("clicked");
    	$(".charge-wrap").show();
    } else if (categoryId != 0) {
    } else if (tabIndex == 0) {
    	$(".left-nav a.guide").closest("li").addClass("clicked");
    	$(".guide-wrap").show();
    }
});
