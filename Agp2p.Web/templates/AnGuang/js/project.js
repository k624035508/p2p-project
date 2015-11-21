import "bootstrap-webpack";
import "../less/head.less";
import "../less/project.less";
import "../less/footerSmall.less";
import "visualnav";

import header from "./header.js"
import "./tips_alert.js"

let initRightSideNav = () => {
    let $navContainer = $("#sidemenu > ul");
    $("div.project-content-left > div").each((index, dom) => {
        let name = $(dom).find("span.title-mark").text();
        let $createdDom = $(`<li><a href='#${dom.id}'><span></span>${name}</a></li>`);
        $navContainer.append($createdDom);
    });
    $("#sidemenu").visualNav({
        selectedClass : "active",
        selectedAppliedTo : 'a',
        animationTime     : 600,
    });
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

    var $displayField = $("#investAmount");
    if ($displayField.length != 0) {
        var $investAmountInput = $("#investAmount-input");
        $investAmountInput.blur(function () {
            $displayField.text($(this).val() + " 元");
        });

        // 打开投资对话框
        var $investBtn = $("button.investing-btn");
        var projectId = $investBtn.data()["projectId"];
        $investBtn.click(function () {
            var hasPayPassword = $(this).data()["hasPayPassword"] == "True";
            if (!hasPayPassword) {
                if (confirm("您需要先设置交易密码，是否现在转到‘安全中心’？")) {
                    var link = $("#link-recharge").attr("href").replace("#/recharge", "#/safe");
                    location.href = link;
                }
                return;
            }
            var investAmount = parseFloat($investAmountInput.val());

            if (investAmount < 100) {
                window.alert("对不起，最少100元起投！");
                return;
            }
            if (investAmount != ~~investAmount) {
                window.alert("对不起，请输入整数金额！");
                return;
            }
            if (parseFloat($(this).data()["idleMoney"]) < investAmount) {
                window.alert("余额不足，请先充值！");
                return;
            }

            // 计算预期收益
            var profit = parseFloat($(this).data()["profitRate"]) * investAmount;
            $("span.profit").text(profit.toFixed(2) + " 元");

            // 插入输入交易密码 input (避免自动完成)
            if ($("div.pswInput input").length == 0) {
                $("div.pswInput").append($('<input type="password" />'));
            }

            // 设置投资协议的链接
            $("#show-invest-contract").attr("href",
                `/tools/submit_ajax.ashx?action=generate_user_invest_contract&projectId=${projectId}&investAmount=${investAmount}`);

            // 显示对话框
            $("#investConfirm").modal();
        });

        // 进行投资操作
        $("button.confirm-btn").click(function () {
        	var transactPassword = $("div.pswInput input[type=password]").val();
        	if (transactPassword == "") {
        		window.alert("请先填写支付密码");
        		return;
        	}
        	if (!$("div.agreement input[type=checkbox]")[0].checked) {
        		window.alert("请先同意投资协议");
        		return;
        	}
            var investAmount = parseFloat($investAmountInput.val());
        	$.ajax({
        		type: "post",
        		dataType: "json",
        		url: "/tools/submit_ajax.ashx?action=invest_project",
        		data: {investingAmount: investAmount, projectId, transactPassword: transactPassword},
        		timeout: 10000,
        		success: function(result) {
        			window.alert(result.msg, () => {
                        if (result.status == 1) {
                            location.reload();
                        }
                    });
        		}.bind(this),
        		error: function(xhr, status, err) {
        			window.alert("操作失败，请重试");
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