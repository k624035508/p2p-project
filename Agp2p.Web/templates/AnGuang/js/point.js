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
                if (data.status == -1) {
                    alert(data.msg);
                } 
                else if (data.status == 0 || data.status == 5) {
                    $("#signConfirm").modal();                    
                    $(".signTable>div:lt(4)").addClass("jinbi").removeClass("nojinbi");
                    $(".signTable").eq(4).find("div").hide().parent().find("p").hide().parent().addClass("signBaoxiang");
                    $(".signDay:lt(5)").css("color", "#37aaf0");
                    $(".signTable .signRight:lt(5)").css("display", "inline-block");
                    $(".signTable .signRight").eq(4).css("marginTop", "84px");
                    $(".content-body .points").text(data.point);
                }
                else {
                    $("#signConfirm").modal();                    
                    $(".signTable>div:lt("+data.status+")").addClass("jinbi").removeClass("nojinbi");
                    $(".signDay:lt("+data.status+")").css("color", "#37aaf0");
                    $(".signTable .signRight:lt("+data.status+")").css("display", "inline-block");
                    $(".content-body .points").text(data.point);
                    $("#signPoint").text("已签到");
                }
            },
            error:function(xhr, status, error) {
                alert("操作超时，请重试");
            }
        });         
    });
});