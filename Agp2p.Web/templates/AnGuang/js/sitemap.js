import "bootstrap-webpack!./bootstrap.config.js"
import "../less/head.less"
import "../less/sitemap.less"
import "../less/footerSmall.less"
import alert from "../components/tips_alert.js";

window.alert = alert;

$(function(){
    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();
});
