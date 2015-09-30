import "bootstrap-webpack";
import "../less/head.less";
import "../less/usercenter.less";
import "../less/footerSmall.less";

import $ from "jquery";
import header from "./header.js";
import "../less/bootstrap-datetimepicker.css";
import "./bootstrap-datetimepicker.js";
import  "./bootstrap-datetimepicker.zh-CN.js";
$(function(){
    //加载交易明细内容
    var $mainContent = $("div.content-body");
    var basePath = $mainContent.data("templateskin");
    var $nav = $(".outside-ul li");

    $("#tradeDetails").click(function(){
        $nav.removeClass("nav-active");
        $(this).parent().addClass("nav-active");
        $mainContent.load(basePath + "/_tradeDetails.html", function () {
            //日期设置
            $(".form_date").datetimepicker({
                language: 'zh-CN',
                format: 'yyyy-mm-dd',
                weekStart: 1,
                todayBtn: true,
                todayHighlight: 1,
                startView: 2,
                forceParse: 0,
                showMeridian: 1,
                autoclose: 1,
                minView: 2
            });

            //交易明细 详情符号翻转
            $(".detailRow").click(function(){
                $(this).next("tr").toggle();
                $(this).find(".glyphicon-triangle-bottom").toggleClass("glyphicon-triangle-top");
            });
        });
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