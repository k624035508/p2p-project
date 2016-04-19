import "bootstrap-webpack";
import "../less/common.less";
import "../less/withdraw.less";
import "../less/card-select.css";
import footerInit from "./footer.js";
import { classMappingPingYing as classMapping } from "../../AnGuang/js/bank-list.jsx";

/*rem的相对单位定义*/
$("html").css("font-size", $(window).width() / 20);

$(function() {
    $("div.select-card-wrap").click(function(){
        $('#bank-select-dialog').modal();
    });

    //提现银行卡信息读取
    var bankSelected = $("#bank-select-dialog .bank-select-body>div");
    if (bankSelected.length === 0) {
        alert("请先到“设置 > 银行卡绑定”添加银行卡");
        location.href = mycardUrl;
    }
    bankSelected.click(function(){
        $("input#select-card").val(this.innerText).attr("data-cardId", $(this).attr("data-cardId"));
    });

    setTimeout(function() {
        $("input#trade-psw").attr("type", "password"); // skip auto complete
    }, 1000);

    //获取提现手续费
    var txtHowmany = $("#withdrawals");
    txtHowmany.blur(function(e) {
        if (txtHowmany.val() === "") return;
        var withdrawVal = Number(txtHowmany.val());
        if (idleMoney < withdrawVal) {
            alert("您提现的金额超出了您的余额，您的余额为：" + idleMoney);
            return;
        }
        $.getJSON("/tools/calc_stand_guard_fee.ashx?withdraw_value=" + withdrawVal).done(function(data) {
            $("#getting").text((withdrawVal - data.handlingFee) + " 元");
            $("#handling-fee").text(data.msg);
        }).fail(function(data) {
            alert(data.responseJSON.msg);
        });
    });
    $("#submit-btn").click(function() {
        var withdrawVal = Number(txtHowmany.val());
        if (idleMoney < withdrawVal) {
            alert("您提现的金额超出了您的余额，您的余额为：" + idleMoney);
            return;
        }
        $.post("/tools/submit_ajax.ashx?action=withdraw", {
            cardId: $("input#select-card").attr("data-cardId"),
            howmany: withdrawVal,
            transactPassword: $("input#trade-psw").val(),
            backUrl: location.href
        }, function(data) {
            if (data.status === 1) {
                location.href = data.url;
            }else{
                alert(data.msg);
            }
        }, "json").fail(function() {
            alert("提交失败，请重试");
        });
    });
    $(".bank-select-body span").each(function(index, item) {
        var span = $(item);
        var keyword = span.parent().attr("data-bankName");
        span.prev().addClass("sprite-" + classMapping[keyword]);
    });
});