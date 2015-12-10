import "bootstrap-webpack";
import "../less/recharge.less";
import "../less/footer.less";

window['$'] = window['jQuery'] = $;

/*rem的相对单位定义*/
$("html").css("font-size", $(window).width() / 20);


$(() => {
    let $chargeLink = $("#charge-link");
    $("#amount").change(ev => {
        $chargeLink.attr("href", "/api/payment/ecpss/index.aspx?bankcode=NOCARD&amount=" + ev.target.value);
    }).val("");
    $chargeLink.click(ev => {
        var amount = parseFloat($("#amount").val() || "0");
        if (amount <= 0) {
            alert("请输入正确的金额");
            ev.preventDefault();
            return;
        }
    });
});
