<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="daily_project_ticket_usages.aspx.cs" Inherits="Lip2p.Web.admin.statistic.daily_project_ticket_usages" %>
<%@ Import namespace="Lip2p.Common" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>天标券使用情况统计</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/tablesorter/jquery.tablesorter.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link href="../../css/pagination.css" rel="stylesheet" type="text/css" />
<link href="../../scripts/tablesorter/table-sorter-style.css" rel="stylesheet" type="text/css" />
<style>
td.center {
    text-align: center;
}
td.align-right {
    text-align: right;
    padding-right: 1em;
}
th.align-right {
    text-align: right;
}
.has-details:hover {
    background-color: #b5d7ef;
    background-color: rgba(0, 122, 204, 0.25);
}
</style>
<script>
$(function() {
    $("td.has-details").click(function () {
        var parentTr = $(this).parent("tr");
        var tdIndex = parentTr.find("td").index(this);
        var th = $("thead th:eq(" + tdIndex + ")");

        var href = parentTr.find("a").attr("href");
        var id = Number(href.match(/id=(\d+)/)[1]);
        $.ajax({
            type: "POST",
            url: "daily_project_ticket_usages.aspx/QueryDetails",
            data: JSON.stringify({userId: id, status: Number(th.attr("data-status")), paid: th.attr("data-paid") === "true"}),
            contentType: "application/json",
            dataType: "json",
            success: function (msg) {
                var statusMap = { 1: "未使用", 2: "已使用", 3: "已过期" };
                var tickets = JSON.parse(msg.d);
                if (tickets.length == 0) {
                    $.dialog.tips("无内容");
                    return;
                }
                var tiStr = $.map(tickets, function(it) {
                    return [
                        it.TicketValue + " 元　天标券：" + statusMap[it.Status] + "　" + (it.Paid ? "已放款" : "未放款"),
                        "　　发放日期：" + it.CreateTime,
                        "　　利息：" + it.Profit + " 元",
                        "　　收益日期：" + it.RepayTime,
                        "　　过期时间：" + it.Deadline
                    ].join("<br>");
                });
                $.dialog({
                    content: tiStr.join("<br><br>"),
                    title: "详细信息",
                    max: false,
                    min: false
                });
            }
        });
    });
});
</script>
</head>
<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>天标券使用情况统计</span>
</div>
<!--/导航栏-->

<!--工具栏-->
<div class="toolbar-wrap">
    <div id="floatHead" class="toolbar">
        <div class="l-list">
        </div>
        <div class="r-list">
            <div style="display: inline-block;">
                <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True" ></asp:TextBox>
                <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
            </div>
        </div>
    </div>
</div>
<!--/工具栏-->

<!--列表-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table id="main" width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <thead>
  <tr>
    <th align="left" width="3%" style="padding-left: 1em;">序号</th>
    <th align="left" width="7%">用户</th>
    <th class="center" width="6%" data-status="1" data-paid="false">未使用的天标券</th>    
    <th class="center" width="6%" data-status="3" data-paid="false">已过期的天标券</th>    
    <th class="center" width="6%" data-status="2" data-paid="false">待放款的天标券</th>
    <th class="center" width="6%" data-status="2" data-paid="true">已放款的天标券</th>    
  </tr>
  </thead>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td style="padding-left: 1em;"><%# Container.ItemIndex + pageSize * (page - 1) + 1 %></td>
      <td>
          <a href="../users/user_edit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&id=<%#Eval("User.id")%>">
              <%#Eval("User.real_name") != null && Eval("User.real_name") != "" ? Eval("User.user_name") + "(" + Eval("User.real_name") + ")" : Eval("User.user_name")%>
          </a>
      </td>
    <td class="has-details center"><%# Eval("UnusedTicketCount")%></td>
    <td class="has-details center"><%# Eval("ExpiredTicketCount")%></td>
    <td class="has-details center"><%# Eval("UsedTicketCount")%></td>
    <td class="has-details center"><%# Eval("PaidTicketCount")%></td>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"6\">暂无记录</td></tr>" : ""%>
</table>
</FooterTemplate>
</asp:Repeater>
<!--/列表-->

<!--内容底部-->
<div class="line20"></div>
<div class="pagelist">
  <div class="l-btns">
    <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox><span>条/页</span>
  </div>
  <div id="PageContent" runat="server" class="default"></div>
</div>
<!--/内容底部-->
</form>
</body>
</html>
