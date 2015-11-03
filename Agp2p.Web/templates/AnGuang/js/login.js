import "es5-shim/es5-shim"
import "es5-shim/es5-sham"

import "bootstrap-webpack";
import "../less/head.less";
import "../less/login.less";
import "../less/footerSmall.less";

import $ from "jquery";

$(function(){
	$("#account").focus();

	$("#loginBtn").click(function(){
		$.ajax({
			type: "post",
			url: "/tools/submit_ajax.ashx?action=user_login",
			dataType: "json",
			data: {
				txtUserName: $("#account").val(),
				txtPassword: $("#psw").val(),
			},
			success: function(data){
				if(data.status == 1){
					location.href ="user/center/index.html";
				} else {
					alert(data.msg);
				}
			},
			error: function(data){
				alert("操作超时，请重试");
			}
		});
		if (document.addEventListener) { //  >=ie9
	        // 记住帐号
	        if ($("input[type=checkbox]").is(":checked")) {
	        	localStorage.setItem("webLogin_UserName", $("#account").val());
	        } else {
	        	localStorage.removeItem("webLogin_UserName");
	        }
	    }
	});
    $("#psw").keyup(function (e) {
        if (e.keyCode == 13) {
            $("#loginBtn").click();
        }
    });

    var userName = localStorage.getItem("webLogin_UserName");
    if (userName) {
    	$("#account").val(userName);
    }
});