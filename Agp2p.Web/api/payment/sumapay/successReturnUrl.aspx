<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="successReturnUrl.aspx.cs" Inherits="Agp2p.Web.api.payment.sumapay.successReturnUrl" %>

<%@ Import Namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>
<%@ Import Namespace="Agp2p.Core" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
    <head>
        <title>激活</title>
        <style type="text/css">
            .contenttips{
                min-width:1100px; height:660px;
                margin:30px auto;
                  p{
                    height:45px; margin-left:341px; padding-left:72px;margin-top:130px;
                    background:url("../imgs/usercenter/005.png") no-repeat;
                    font-size:40px; color:#37aaf0; font-weight:bold;
                  }
                 button{
                     display:inline-block;  width:115px;height:35px;border-radius:3px; margin-bottom:95px;
                     color:#fff;background:red;font-size:16px;
                 }
                 .returnBtn{ margin-left:341px;}
                 .closeBtn{margin-left:183px;}
            }
            .chongzhi{ background:url("../imgs/usercenter/003.png") no-repeat center;}
            .tixian {
                background: url("../imgs/usercenter/002.png") no-repeat center;
            }
        </style>
    </head>
    <body>
 <%if (RequestLog.api == (int)Agp2pEnums.SumapayApiEnum.URegi || RequestLog.api == (int)Agp2pEnums.SumapayApiEnum.Activ) { %>          
      
<% }%>

      <% if(RequestLog.api == (int)Agp2pEnums.SumapayApiEnum.AtBid) {%>
                         

      <% } %>
      
        <% if(RequestLog.api ==(int)Agp2pEnums.SumapayApiEnum.ClBid) {%>
        <%} %>

        <% if(RequestLog.api == (int)Agp2pEnums.SumapayApiEnum.AcReO || RequestLog.api == (int)Agp2pEnums.SumapayApiEnum.AbReO) { %>
        <%} %>

        <% if(RequestLog.api ==(int)Agp2pEnums.SumapayApiEnum.ClRep) {%>
        <%} %>

<%if (RequestLog.api == (int)Agp2pEnums.SumapayApiEnum.WeRec || RequestLog.api == (int)Agp2pEnums.SumapayApiEnum.WhRec) { %>
   <div class="contenttips chongzhi">
   <p>恭喜您，充值成功</p>          
            <button type="button">返回</button>
            <button type="button">关闭</button>
    </div>
<%} %>

        <% if(RequestLog.api ==(int)Agp2pEnums.SumapayApiEnum.Wdraw) {%>
        <div class="contenttips tixian">
            <p>恭喜您，提现成功</p>
            <button class="returnBtn" type="button">返回</button>
            <button class="closeBtn" type="button">关闭</button>
        </div>
        <%} %>

    </body>
</html>
