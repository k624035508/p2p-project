import "../less/footer.less";

export default function() {
    var key = typeof (forceFooterKey) === "undefined" ? window.location.href : forceFooterKey;
    if (key.indexOf("user") >= 0) {
        $("#span_usercenter").attr("class", "footer-span-active icon-mine center-block");
        $("#a_usercenter").attr("class", "txt-active");
    } else if (key.indexOf("project") >= 0) {
        $("#span_invest").attr("class", "footer-span-active icon-invest center-block");
        $("#a_invest").attr("class", "txt-active");
    } else if (key.indexOf("setting") >= 0) {
        $("#span_setting").attr("class", "footer-span-active icon-settings center-block");
        $("#a_setting").attr("class", "txt-active");
    } else {
        $("#span_index").attr("class", "footer-span-active icon-index center-block");
        $("#a_index").attr("class", "txt-active");
    }
}