import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/login.less";
import "../less/footerSmall.less";

import alert from "../components/tips_alert.js";
import header from "./header.js";

$(function(){
	$("#account").focus();

	//弹出窗popover初始化
	$('[data-toggle="popover"]').popover();
	header.setHeaderHighlight(4);

	$("#loginBtn").click(function(){
		$.ajax({
			type: "post",
			url: "/tools/submit_ajax.ashx?action=user_login",
			dataType: "json",
			data: {
				txtUserName: $("#account").val(),
				txtPassword: $("#psw").val(),
				chkRemember: $("input[type=checkbox]").is(":checked")
			},
			success: function(data){
				if(data.status == 1){
					if (document.referrer !== "" && document.referrer.indexOf("/project/") != -1) {
						location.href = document.referrer;
					} else {
						location.href = $("div.nav-bar li#myAccount > a").attr("href");
					}
				} else {
					alert(data.msg);
				}
			},
			error: function(data){
				alert("操作超时，请重试");
			}
		});
		/*if (document.addEventListener) { //  >=ie9
	        // 记住帐号
	        if ($("input[type=checkbox]").is(":checked")) {
	        	localStorage.setItem("webLogin_UserName", $("#account").val());
	        } else {
	        	localStorage.removeItem("webLogin_UserName");
	        }
	    }*/
	});
    $("#psw").keyup(function (e) {
        if (e.keyCode == 13) {
            $("#loginBtn").click();
        }
    });

    /*var userName = localStorage.getItem("webLogin_UserName");
    if (userName) {
    	$("#account").val(userName);
    }*/
});