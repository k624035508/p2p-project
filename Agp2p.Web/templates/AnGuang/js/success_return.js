import "bootstrap-webpack!./bootstrap.config.js"
import "../less/head.less"
import "../less/success_return.less"
import "../less/footerSmall.less"

$(function(){
    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();
    $(window).load(function(){
        var url = location.href;
        var index = url.indexOf("#");
        var returnid = url.substring(index+1);
        $("."+returnid).show().siblings().hide();
    })
});
