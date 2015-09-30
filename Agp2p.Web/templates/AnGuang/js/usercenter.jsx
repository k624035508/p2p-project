import "bootstrap-webpack";
import "../less/head.less";
import "../less/usercenter.less";
import "../less/footerSmall.less";


import React from "react"
import Picker from "../component/type-timespan-picker.jsx"
import TransactionTable from "../component/transactions-table.jsx"

$(function(){
    //点击导航加载相应内容
    var $mainContent = $("div.content-body");
    var basePath = $mainContent.data("templateskin");
    var aspxPath = $mainContent.data("aspx-path");
    var $nav = $(".outside-ul li");

    $("#tradeDetails").click(function(){
        $nav.removeClass("nav-active");
        $(this).parent().addClass("nav-active");

        React.render(
        	<div>
	        	<Picker url={aspxPath + "/AjaxQueryEnumInfo"} args={{ enumFullName: "Agp2p.Common.Agp2pEnums+TransactionDetailsDropDownListEnum"}} />
	        	<TransactionTable url={aspxPath + "/AjaxQueryTransactionHistory"} args={{ pageIndex: 0, pageSize: 10}} />
        	</div>, $mainContent[0]);
    });

    //加载我要充值内容
    $("#recharge").click(function(){
        $nav.removeClass("nav-active");
        $(this).parent().addClass("nav-active");
        $mainContent.load(basePath + "/_recharge.html",function(){
            //充值选择银行
            var $bank = $(".bank-select li");
            $bank.click(function(){
                $bank.find("img").remove();
                var img = document.createElement("img");
                img.src = basePath + "/imgs/usercenter/bank-icons/selected.png";
                this.appendChild(img);
            });
        });
    });
});