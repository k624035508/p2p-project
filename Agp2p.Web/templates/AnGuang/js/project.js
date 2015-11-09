import "bootstrap-webpack";
import "../less/head.less";
import "../less/project.less";
import "../less/footerSmall.less";
import "visualnav";

import header from "./header.js"

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

    $("#sidemenu").visualNav({
        selectedClass : "active",
        selectedAppliedTo : 'a',
        animationTime     : 600,
    });

    var $displayField = $("#investAmount");
    if ($displayField.length != 0) {
        var $investAmountInput = $("#investAmount-input");
        $investAmountInput.blur(function () {
            $displayField.text($(this).val() + " 元");
        });

        // 打开投资对话框
        var $investBtn = $("button.investing-btn");
        $investBtn.click(function () {
            var hasPayPassword = $(this).data()["hasPayPassword"] == "True";
            if (!hasPayPassword) {
                alert("请到安全中心设置交易密码");
                return;
            }
            var investAmount = parseFloat($investAmountInput.val());
            if (investAmount < 100) {
                alert("对不起，最少100元起投");
                return;
            }
            if (investAmount != ~~investAmount) {
                alert("对不起，请输入整数金额");
                return;
            }
            if (parseFloat($(this).data()["idleMoney"]) < investAmount) {
                alert("余额不足，请先充值");
                return;
            }

            // 计算预期收益
            var profit = parseFloat($(this).data()["profitRate"]) * investAmount;
            $("span.profit").text(profit.toFixed(2) + " 元");
            $("#investConfirm").modal();
        });

        // 进行投资操作
        $("button.confirm-btn").click(function () {
        	var transactPassword = $("div.pswInput input[type=password]").val();
        	if (transactPassword == "") {
        		alert("请先填写支付密码");
        		return;
        	}
        	if (!$("div.agreement input[type=checkbox]")[0].checked) {
        		alert("请先同意投资协议");
        		return;
        	}
            var investAmount = parseFloat($investAmountInput.val());
        	$.ajax({
        		type: "post",
        		dataType: "json",
        		url: "/tools/submit_ajax.ashx?action=invest_project",
        		data: {investingAmount: investAmount, projectId: $investBtn.data()["projectId"], transactPassword: transactPassword},
        		timeout: 10000,
        		success: function(result) {
        			alert(result.msg);
        			if (result.status == 1) {
        				location.reload();
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
});