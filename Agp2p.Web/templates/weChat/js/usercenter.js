import "bootstrap-webpack";
import "../less/usercenter.less";
import "../less/footer.less";

import footerInit from "./footer.js";

/*rem的相对单位定义*/
var viewportWidth = $(window).width();
var fontSizeUnit = viewportWidth / 20;
$("html").css("font-size", fontSizeUnit);

$(function() {
    //TODO 停止充值临时逻辑
    //$("#reChargeBtn").click(function() {
    //    alert("您暂时无法充值：安广融合平台正在切换第三方资金托管，具体全面开放请留意官网公告。");
    //});

    var { userIdentity , questionnairescore } = $("#app").data();
    if (userIdentity == "True") {
        if(confirm("安广融合已切换第三方支付平台（丰付），请到支付平台页面激活存管账户。 "))
        {
            location.href="/api/payment/sumapay/index.aspx?api=3";
        }
    }
    if (questionnairescore == "False") {
        $("#questionConfirm").modal();
    }

    footerInit();
});
