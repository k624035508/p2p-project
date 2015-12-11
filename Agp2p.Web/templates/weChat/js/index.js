import "../less/index.less";
import "bootstrap-webpack";
import "../less/invest-list.less";

import footerInit from "./footer.js";
import "./radialIndicator.min.js";

/*rem的相对单位定义*/
$("html").css("font-size", $(window).width() / 20);

$(function () {
    footerInit();
    $('.indicatorContainer').each(function(index, obj) {
        $(obj).radialIndicator({
            radius: 28,
            barColor: '#ff414b',
            barBgColor: '#d9d9d9',
            barWidth: 2,
            initValue: $(obj).attr('data-progress'),
            roundCorner: true,
            percentage: true
        });
    });
});