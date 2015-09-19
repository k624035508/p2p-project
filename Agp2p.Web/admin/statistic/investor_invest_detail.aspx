<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="investor_invest_detail.aspx.cs" Inherits="Agp2p.Web.admin.statistic.investor_invest_detail" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>投资人投资明细汇总表</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/tablesorter/jquery.tablesorter.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
<link href="../../scripts/tablesorter/table-sorter-style.css" rel="stylesheet" type="text/css" />
<style>
tr.sum td { color: red; }
td { text-align: center; }
</style>
    <script>
        $(function () {
            $.tablesorter.addParser({
                id: 'rmb',
                is: function () { return false; },
                format: function (s) { return s.replace(/[¥,]/g, ''); },
                type: 'numeric'
            });
            $("table#invest").tablesorter({
                headers: {
                    2: { sorter: 'rmb' },
                    3: { sorter: 'rmb' },
                    4: { sorter: 'rmb' },
                    5: { sorter: 'rmb' },
                    6: { sorter: 'rmb' },
                    7: { sorter: 'rmb' },
                    8: { sorter: 'rmb' },
                    9: { sorter: 'rmb' }
                }
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
  <span>投资人投资明细汇总表</span>
</div>
<!--/导航栏-->

    <!--工具栏-->
    <div class="toolbar-wrap">
        <div id="floatHead" class="toolbar">
            <div class="l-list">
                <ul class="icon-list">
                    <li><asp:LinkButton ID="btnExportExcel" runat="server" CssClass="quotes" OnClick="btnExportExcel_Click"><i></i><span>导出 Excel</span></asp:LinkButton></li>
                </ul>
            </div>
            <div class="r-list">
                <div style="display: inline-block;" class="rl">按投资时间查询：</div>
                <div style="display: inline-block; float:left;">
                    <asp:TextBox ID="txtYear" runat="server" CssClass="keyword" datatype="/^\d{4}$/" AutoPostBack="True" OnTextChanged="txtYear_OnTextChanged" Width="4em" />
                </div>
                <div style="display: inline-block;" class="rl">年</div>
                <div style="display: inline-block; float:left;">
                    <asp:TextBox ID="txtMonth" runat="server" CssClass="keyword" datatype="/^\d{1,2}$/" AutoPostBack="True" OnTextChanged="txtMonth_OnTextChanged" Width="2em" />
                </div>
                <div style="display: inline-block;" class="rl">月</div>
                <div style="display: inline-block;">
                    <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True" />
                    <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <!--/工具栏-->

<!--列表-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table id="invest" width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
    <thead>
        <tr>
            <th width="4%">序号</th>
            <th>用户名</th>
            <th align="left">项目名称</th>
            <th>投资时间</th>
            <th>到期时间</th>
            <th>期限</th>
            <th>年利率</th>
            <th>投资本金</th>
            <th>利息</th>
            <th>返还本息合计</th>
        </tr>
    </thead>
</HeaderTemplate>
<ItemTemplate>
  <tr <%# string.Equals(Eval("InvestorUserName"), "小计") || string.Equals(Eval("InvestorUserName"), "合计") ? "class='sum'" : ""%>>
    <td style="text-align: center;"><%# Eval("Index")%></td>
    <td><%# string.IsNullOrWhiteSpace((string) Eval("InvestorRealName")) ? Eval("InvestorUserName") : Eval("InvestorRealName") %></td>
    <td style="text-align: left; padding-left:5px;"><%# Eval("ProjectName")%></td>
    <td><%# Eval("InvestTime")%></td>
    <td><%# Eval("ProjectCompleteTime")%></td>
    <td><%# Eval("Term")%></td>
    <td><%# Eval("ProfitRateYear")%></td>
    <td class="money"><%# Eval("InvestValue") == null ? "" : Convert.ToDecimal(Eval("InvestValue")).ToString("c")%></td>
    <td class="money"><%# Eval("RepayTotal") == null ? "" : Convert.ToDecimal(Eval("RepayTotal")).ToString("c")%></td>
    <td class="money"><%# Convert.ToDecimal(Eval("Total")).ToString("c") %></td>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"10\">暂无记录</td></tr>" : ""%>
</table>
</FooterTemplate>
</asp:Repeater>
<!--/列表-->

<!--内容底部-->
<div class="line20"></div>
<div class="pagelist">
  <div class="l-btns">
      <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"/><span>条/页</span>
  </div>
  <div id="PageContent" runat="server" class="default"></div>
</div>
<!--/内容底部-->
</form>
</body>
</html>
