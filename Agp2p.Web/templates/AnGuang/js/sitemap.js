import "bootstrap-webpack!./bootstrap.config.js"
import "../less/head.less"
import "../less/sitemap.less"
import "../less/footerSmall.less"

$(function(){
    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();
});
