import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/point.less";
import "../less/footer.less";

import header from "./header.js";
window['$'] = $;

import alert from "../components/tips_alert.js";

$(function() {
    header.setHeaderHighlight(3);   
    $("#signPoint").click(function(){
        $.ajax({
            type: "post",
            dataType:"JSON",
            url:"/tools/submit_ajax.ashx?action=point_qiandao",
            success: function (data) {
                alert(data.msg);                 
            },
            error:function(xhr, status, error) {
                alert("操作超时，请重试");
            }
        });
    });
});