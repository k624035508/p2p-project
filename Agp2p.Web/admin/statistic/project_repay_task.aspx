<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="project_repay_task.aspx.cs" Inherits="Agp2p.Web.admin.statistic.project_repay_task" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>线上投资项目明细表</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
<style>
td.money { text-align: right; }
td.center { text-align: center; }
</style>
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>线上投资项目明细表</span>
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
                <!--填写时间，加载列表-->
                <div style="display: inline-block;" class="rl">按</div>
                <div class="menu-list" style="display: inline-block; float: left; margin-right: 8px; margin-left: 8px;">
                    <div class="rule-single-select">
                        <asp:DropDownList ID="ddlOrderBy" AutoPostBack="True" runat="server" OnSelectedIndexChanged="ddlOrderBy_SelectedIndexChanged">
                            <asp:ListItem Text="满标时间" Value="invest_complete_time" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="还款时间" Value="should_repay_time"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div style="display: inline-block;" class="rl">查询：</div>
                <div style="display: inline-block;float: left;">
                    <asp:TextBox ID="txtYear" runat="server" CssClass="keyword" datatype="/^\d{4}$/" AutoPostBack="True" OnTextChanged="txtYear_OnTextChanged" Width="4em" />
                </div>
                <div style="display: inline-block;" class="rl">年</div>
                <div style="display: inline-block;float: left;">
                    <asp:TextBox ID="txtMonth" runat="server" CssClass="keyword" datatype="/^\d{1,2}$/" AutoPostBack="True" OnTextChanged="txtMonth_OnTextChanged" Width="2em" />
                </div>
                <div style="display: inline-block;" class="rl">月</div>
                <!--银行交易状态选择，选择后刷新列表-->
                <div style="display: inline-block; padding-left: 2em;" class="rl">状态：</div>
                <div class="rule-multi-radio" style="display: inline-block;float: left; margin-right:10px;">
                    <asp:RadioButtonList ID="rblRepaymentStatus" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="rblRepaymentStatus_OnSelectedIndexChanged">
                        <asp:ListItem Value="0" Selected="True">不限</asp:ListItem>
                        <asp:ListItem Value="1">待还款</asp:ListItem>
                        <asp:ListItem Value="2">已还款</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
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
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th width="5%">序号</th>
    <th align="left">项目名称</th>
    <th align="right">融资金额</th>
    <th align="center">年利率</th>
    <th align="left">期限</th>

    <th align="left">满标日</th>
    <th align="left">到期日</th>
    <th align="left">应付本息时间</th>
    <th align="left">状态</th>
    <th align="right">应付本金</th>

    <th align="right">应付利息</th>
    <th align="right">本息合计</th>
    <th align="center">对应借款合同号</th>
    <th align="left">债权人</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td class="center"><%# Eval("Project.Index")%></td>
    <td><%# Eval("Project.Name")%></td>
    <td class="money"><%# Eval("Project.FinancingAmount") != null ? Convert.ToDecimal(Eval("Project.FinancingAmount")).ToString("c") : "" %></td>
    <td class="center"><%# Eval("Project.ProfitRateYear")%></td>
    <td><%# Eval("Project.Term")%></td>
    <td><%# Eval("Project.InvestCompleteTime")%></td>
    <td><%# Eval("Project.RepayCompleteTime")%></td>

    <td><%# Eval("RepayTime")%></td>
    <td><%# Eval("Status")%></td>
    <td class="money"><%# Convert.ToDecimal(Eval("RepayPrincipal")).ToString("c")%></td>
    <td class="money"><%# Convert.ToDecimal(Eval("RepayInterest")).ToString("c")%></td>
    <td class="money"><%# Convert.ToDecimal(Eval("RepayTotal")).ToString("c")%></td>

    <td class="center"><%# Eval("Project.LoanContractNumber")%></td>
    <td><%# Eval("Project.Creditor")%></td>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"14\">暂无记录</td></tr>" : ""%>
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
