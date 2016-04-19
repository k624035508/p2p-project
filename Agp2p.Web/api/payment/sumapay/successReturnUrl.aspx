<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="successReturnUrl.aspx.cs" Inherits="Agp2p.Web.api.payment.sumapay.successReturnUrl"  ResponseEncoding="utf-8"%>

<%@ Import Namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>
<%@ Import Namespace="Agp2p.Core" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
    <head>
        <title>激活</title>
        <style type="text/css">
            body{
                background:#f2f2f2;
                margin:0;padding:0;
            }
            .contenttips {
                background:#fff;
                min-width: 1100px;
                height: 660px; width:1100px;
                margin: 30px auto;
            }
                  p{
                    height:45px; margin-left:341px; padding-left:72px;margin-top:130px;
                    background:url("../../../../templates/AnGuang/imgs/usercenter/005.png") no-repeat;
                    font-size:40px; color:#37aaf0; font-weight:900;
                  }
                 a{
                     display:inline-block;  width:115px;height:35px;border-radius:3px; margin-bottom:95px;
                     color:#fff;background:red;font-size:16px; text-align:center; 
                 }
                 .returnBtn{ margin-left:341px;}
                 .closeBtn{margin-left:183px;}
            
            .chongzhi{  height:229px;background:url("../../../../templates/AnGuang/imgs/usercenter/003.png") no-repeat center;}
            .tixian { background: url("../../../../templates/AnGuang/imgs/usercenter/002.png") no-repeat center;}
            .repay{ background: url("../../../../templates/AnGuang/imgs/usercenter/004.png") no-repeat center;}
        </style>
    </head>
<body>
    <div class="contenttips">
        <p>恭喜您，充值成功</p>
        <div class="chongzhi"></div>
        <a class="returnBtn">返回</a>
        <a class="closeBtn">关闭</a>
    </div>
</body>
</html>
