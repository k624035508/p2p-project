import "bootstrap-webpack";
import "../less/recharge.less";
import "../less/footer.less";

window['$'] = window['jQuery'] = $;

/*rem的相对单位定义*/
$("html").css("font-size", $(window).width() / 20);


$(() => {
    $("#charge-link").click(ev => {
        var amount = parseFloat($("#amount").val() || "0");
        if (amount <= 0) {
            alert("请输入正确的金额");
            return;
        }
        location.href = "/api/payment/ecpss/index.aspx?bankcode=NOCARD&amount=" + $("#amount").val();
    });
});
