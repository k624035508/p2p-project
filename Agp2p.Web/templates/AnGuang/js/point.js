import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/point.less";
import "../less/footer.less";

import header from "./header.js";
window['$'] = $;

import alert from "../components/tips_alert.js";

$(function() {
    header.setHeaderHighlight(3);   
    $('[data-toggle="popover"]').popover();

    $("#signPoint").click(function(){
        $.ajax({
            type: "post",
            dataType:"JSON",
            url:"/tools/submit_ajax.ashx?action=point_qiandao",
            success: function (data) {
                if (data.status == 0) {
                    alert(data.msg);
                } 
                else {
                    $("#signConfirm").modal();                    
                    $(".signTable>div:lt("+data.status+")").addClass("jinbi").removeClass("nojinbi");
                    $(".signDay:lt("+data.status+")").css("color", "#37aaf0");
                    $(".signTable .signRight:lt("+data.status+")").css("display", "inline-block");
                }
            },
            error:function(xhr, status, error) {
                alert("操作超时，请重试");
            }
        });         
    });
});