import "bootstrap-webpack";
import "../less/mycard.less";
import "../less/cards-list.css";
import "../less/card-select.css";
import "../less/footer.less";
import "fullpage.js/jquery.fullPage.css";
import "fullpage.js";
import { classMappingPingYing as classMapping } from "../../AnGuang/js/bank-list.jsx";
import keys from "lodash/object/keys"
import footerInit from "./footer.js";

/*rem的相对单位定义*/
$("html").css("font-size", $(window).width() / 20);

function loadCardInfo(cardId, callback) {
    $.ajax({
        type: "GET",
        url: reqFilePath + "/AjaxQueryCardInfo?cardId=" + cardId,
        data: "",
        contentType: "application/json",
        dataType: "json",
        success: function (msg) {
            var info = JSON.parse(msg.d);
            callback(info);
        }
    }).fail(function (data) {
        callback(null, data.responseJSON.d);
    });
}
function ajaxInvoke(methodName, params, callback) {
    $.ajax({
        type: "POST",
        url: reqFilePath + "/" + methodName,
        data: JSON.stringify(params),
        contentType: "application/json",
        dataType: "json",
        success: function (msg) {
            callback(true, msg.d);
        }
    }).fail(function (data) {
        callback(false, data.responseJSON.d);
    });
}
function initCardList() {
    $(".bank-select-body div").click(function () { // 在对话框选择银行卡后
        $('#bank-select-dialog').modal("hide");
        $("div.bank-select-wrap input").val(this.innerText);
    });
    var cardNumberInput = $("#cardNumber");
    var bankNameInput = $("#bankName");
    var submitBtn = $("button#submitBtn");
    var modifyBtns = $(".modify-btns-box");
    var cardId = null; // 当前查看的银行卡
    $("div.card-cell").click(function () {
        submitBtn.hide();
        modifyBtns.show();
        $.fn.fullpage.moveSlideRight();

        cardNumberInput[0].readOnly = true;
        cardNumberInput.val("加载中...");

        cardId = Number($(this).attr("data-cardId"));
        loadCardInfo(cardId, function (info, msg) {
            if (info === null) {
                alert(msg);
                $.fn.fullpage.moveSlideLeft();
                return;
            }
            cardNumberInput.val(info.CardNumber);
            bankNameInput.val(info.BankName);
        });
    });
    $(".add-cards").click(function() {
        $.fn.fullpage.moveSlideRight();
        submitBtn.show();
        modifyBtns.hide();

        cardNumberInput[0].readOnly = false;
        cardNumberInput.val("");
        bankNameInput.val("");
    });
    submitBtn.click(function() {
        ajaxInvoke("AjaxAppendCard", {
            cardNumber: cardNumberInput.val(),
            bankName: bankNameInput.val(),
        }, function (succ, result) {
            if (!succ) alert(result);
            else {
                alert(result);
                history.back();
                setTimeout(function() {
                    location.reload();
                }, 500);
            }
        });
    });
    modifyBtns.find("#modify-btn").click(function() {
        ajaxInvoke("AjaxModifyCard", {
            cardId: cardId,
            bankName: bankNameInput.val(),
        }, function (succ, result) {
            if (!succ) alert(result);
            else {
                alert(result);
                history.back();
                setTimeout(function () {
                    location.reload();
                }, 500);
            }
        });
    });
    modifyBtns.find("#delete-btn").click(function () {
        if (!confirm("确定删除银行卡？")) return;
        ajaxInvoke("AjaxDeleteCard", {
            cardId: cardId
        }, function (succ, result) {
            if (!succ) alert(result);
            else {
                alert(result);
                history.back();
                setTimeout(function() {
                    location.reload();
                }, 500);
            }
        });
    });
}
function initFullpage() {
    $('#fullpage').fullpage({
        anchors: ['slide'],
        controlArrows: false,
        verticalCentered: false,
        loopHorizontal: false,
        autoScrolling: true,
        fitToSection: false,
        onSlideLeave: function(){
            $(".scroll").scrollTop(0);
        }
    });
    $.fn.fullpage.setAllowScrolling(false);
    if (history.pushState) {
        history.pushState(null, null, '#slide');
    } else {
        location.href = "#slide";
    }
}
function fixAndroid2xOverflowCannotScroll($affectedElem) {
    var ua = navigator.userAgent;
    if (ua.indexOf("Android") >= 0) {
        var androidversion = parseFloat(ua.slice(ua.indexOf("Android") + 8));
        if (androidversion < 3) {
            var script = document.createElement('script');
            script.src = templateSkin + "/js/touchscroll.js";
            script.onload = function () {
                $affectedElem.each(function (index, elem) {
                    touchScroll(elem);
                });
            };
            document.head.appendChild(script);
        }
    }
}

$(function() {
    footerInit();

    if (!realName) {
        alert("请先进行实名认证");
        location.href = safeUrl;
    }

    var $bankListContainer = $("#bank-select-dialog .bank-select-body");
    var bankNames = keys(classMapping);
    bankNames.forEach(name => {
        var el = $(`<div><i class="sprite-dlg sprite-${classMapping[name]}"></i><span>${name}</span></div>`);
        $bankListContainer.append(el);
    });

    initFullpage();
    initCardList();
    
    fixAndroid2xOverflowCannotScroll($(".inner-scrollable, .bank-select-body"));

    $("div.card-cell p").each(function(index, item) {
        var p = $(item);
        p.parent().prev().addClass(classMapping[p.text()]);
    });
});
