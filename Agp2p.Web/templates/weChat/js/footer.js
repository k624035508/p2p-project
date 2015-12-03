import "../less/footer.less";

export default function() {
    var key = typeof (forceFooterKey) === "undefined" ? window.location.href : forceFooterKey;
    if (key.indexOf("user") >= 0) {
        $("#span_usercenter").attr("class", "active center-block");
        $("#a_usercenter").attr("class", "active");
    } else if (key.indexOf("project") >= 0) {
        $("#span_invest").attr("class", "active center-block");
        $("#a_invest").attr("class", "active");
    } else if (key.indexOf("setting") >= 0) {
        $("#span_setting").attr("class", "active center-block");
        $("#a_setting").attr("class", "active");
    } else {
        $("#span_index").attr("class", "active center-block");
        $("#a_index").attr("class", "active");
    }
}