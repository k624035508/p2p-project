import "bootstrap-webpack";
import "../less/head.less";
import "../less/usercenter.less";
import "../less/footerSmall.less";

import React from "react"
import MyTransaction from "../containers/mytransaction.jsx"
import RechargePage from "../containers/recharge.jsx"
import WithdrawPage from "../containers/withdraw.jsx"

$(function(){
    //点击导航加载相应内容
    var $mainContent = $("div.content-body");
    var basePath = $mainContent.data("templateskin");
    var aspxPath = $mainContent.data("aspx-path");
    var $nav = $(".outside-ul li");

    $("#tradeDetails").click(function(){
        $nav.removeClass("nav-active");
        $(this).parent().addClass("nav-active");

        React.render(<MyTransaction aspxPath={aspxPath} />, $mainContent[0]);
    });

    //加载我要充值内容
    $("#recharge").click(function(){
        $nav.removeClass("nav-active");
        $(this).parent().addClass("nav-active");

        React.render(<RechargePage templateBasePath={basePath} />, $mainContent[0]);
    });

    //加载我要提现内容
    $("#withdraw").click(function(){
        $nav.removeClass("nav-active");
        $(this).parent().addClass("nav-active");

        React.render(<WithdrawPage templateBasePath={basePath} />, $mainContent[0]);
    });
});