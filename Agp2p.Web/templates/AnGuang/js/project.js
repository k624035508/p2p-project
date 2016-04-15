import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/project.less";
import "../less/footerSmall.less";

import header from "./header.js";
import alert from "../components/tips_alert.js";
import confirm from "../components/tips_confirm.js";

window['$'] = $;

let initRightSideNav = () => {
    let $navContainer = $("#sidemenu > ul");
    $("div.project-content-left > div").each((index, dom) => {
        let name = $(dom).find("span.title-mark").text();
        let $createdDom = $(`<li><a href='#${dom.id}'><span></span>${name}</a></li>`);
        $navContainer.append($createdDom);
    });
    $("body").scrollspy({target: "#sidemenu"});
};

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

    header.setHeaderHighlight(1);

   

    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();

    var $displayField = $("#investAmount");
    if ($displayField.length != 0) {
        var $investAmountInput = $("#investAmount-input");
        $investAmountInput.blur(function () {
            $displayField.text($(this).val() + " 元");
        });

        // 打开投资对话框
        var $investBtn = $("button.investing-btn");
        var projectId = $investBtn.data()["projectId"];
        var buyClaimId = $investBtn.data()["buyClaimId"];
        var projectSum = $investBtn.data()["projectSum"];
        var projectDescription = $investBtn.data()["projectDescription"];
        var huoqi  = $investBtn.data()["projectHuoqi"];
        $investBtn.click(function () {
            var investBtnData = $investBtn.data();
            var hasIdentification = investBtnData["hasIdentification"] == "True";
            /*var hasPayPassword = investBtnData["hasPayPassword"] == "True";
            if (!hasPayPassword) {
                confirm("您需要先设置交易密码，是否现在转到‘安全中心’？", () => {
                    var link = $("#link-recharge").attr("href").replace("#/recharge", "#/safe");
                    location.href = link;
                });
                return;
            }*/
            if (!hasIdentification) {
                confirm("您需要先进行身份认证，是否现在转到‘安全中心’？", () => {
                    var link = $("#link-recharge").attr("href").replace("#/recharge", "#/safe");
                    location.href = link;
                });
                return;
            }
            var investAmount = parseFloat($investAmountInput.val());

            if (investAmount < 100) {
                alert("对不起，最少100元起投！");
                return;
            }
            if (investAmount != ~~investAmount) {
                alert("对不起，请输入整数金额！");
                return;
            }
            if (parseFloat(investBtnData["idleMoney"]) < investAmount) {
                alert("余额不足，请先充值！");
                return;
            }

            if ($investAmountInput[0].disabled && investAmount == 100) {
                // TODO 新手体验标的临时逻辑
                $investAmountInput.blur(); // 刷新 dialog 里面的投资金额
                $("span.profit").text("10 元");
            } else {
                // 计算预期收益
                var profit = parseFloat(investBtnData["profitRate"]) * investAmount;
                $("span.profit").text(profit.toFixed(2) + " 元");
            }

            // 设置投资协议的链接
            $("#show-invest-contract").attr("href",
                `/tools/submit_ajax.ashx?action=generate_user_invest_contract&projectId=${projectId}&investAmount=${investAmount}`);

            // 显示对话框
            $("#investConfirm").modal();
        });

        // 进行投资操作
        $("button.confirm-btn").click(function () {
        	/*var transactPassword = $("div.pswInput input[type=password]").val();
        	if (transactPassword == "") {
        		alert("请先填写支付密码");
        		return;
        	}*/
        	if (!$("div.agreement input[type=checkbox]")[0].checked) {
        		alert("请先同意投资协议");
        		return;
        	}
            var investAmount = parseFloat($investAmountInput.val());
        	$.ajax({
        		type: "post",
        		dataType: "json",
        		url: "/tools/submit_ajax.ashx?action=invest_project",
        		data: {investingAmount: investAmount, projectId, buyClaimId, projectSum: projectSum, projectDescription: projectDescription,huoqi},
        		timeout: 10000,
        		success: function(result) {
        		    if (result.status == 0) {
		                alert(result.msg);
		            } else {
        		        location.href = result.url;
		            }
        		}.bind(this),
        		error: function(xhr, status, err) {
        			alert("操作失败，请重试");
        			console.error(url, status, err.toString());
        		}.bind(this)
        	});
        });
    };

    // 相关资料图片点击放大
    var $thumbnail = $(".photo-cell .thumbnail-custom img");
    var $prev = $("#photoModal .modal-body .control-prev");
    var $next = $("#photoModal .modal-body .control-next");
    var $modalTitle = $("#photoModal .modal-header h4");

    var clickedPicIndex = -1;
    var photoTitle = "";
    $thumbnail.click(function(){
        var currentSrc = $(this).data("originSrc");
        photoTitle = $(this).data("title");
        $("#modalPic").attr("src", currentSrc);
        $modalTitle.html(photoTitle);

        clickedPicIndex = $.inArray(this, $thumbnail);
    });

    $prev.click(function(){
        if(clickedPicIndex <= 0){
            alert("已经是第一张！");
        }else {
            clickedPicIndex = clickedPicIndex - 1;
            var prevSrc = $thumbnail.eq(clickedPicIndex).data("originSrc");
            photoTitle = $thumbnail.eq(clickedPicIndex).data("title");

            $("#modalPic").attr("src", prevSrc);
            $modalTitle.html(photoTitle);
        }
    });

    $next.click(function(){
        if(clickedPicIndex >= $thumbnail.length-1){
            alert("已经是最后一张！");
        }else {
            clickedPicIndex = clickedPicIndex + 1;
            var nextSrc = $thumbnail.eq(clickedPicIndex).data("originSrc");
            photoTitle = $thumbnail.eq(clickedPicIndex).data("title");

            $("#modalPic").attr("src", nextSrc);
            $modalTitle.html(photoTitle);
        }
    });

    initRightSideNav();
});