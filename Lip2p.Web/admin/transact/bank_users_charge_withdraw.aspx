<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="bank_users_charge_withdraw.aspx.cs" Inherits="Lip2p.Web.admin.transact.bank_users_charge_withdraw" %>
<%@ Import namespace="Lip2p.Common" %>
<%@ Import Namespace="Lip2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>用户列表</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>用户列表</span>
</div>
<!--/导航栏-->

<!--工具栏-->
<div class="toolbar-wrap">
  <div id="floatHead" class="toolbar">
    <div class="l-list">
    </div>
    <div class="r-list">
          <div class="menu-list" style="display: inline-block; float: left; margin-right: 8px; margin-left: 8px;">
              <div class="rule-single-select">
                  <asp:DropDownList ID="ddlUserGroud" AutoPostBack="True" runat="server" OnSelectedIndexChanged="ddlUserGroud_SelectedIndexChanged">
                  </asp:DropDownList>
              </div>
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
    <th align="left" width="3%" style="padding-left: 1em;">序号</th>
    <th align="left" width="10%">用户名</th>
    <th align="left" width="10%">充值中金额</th>
    <th align="left" width="10%">提现中金额</th>
    <th align="left" width="10%">充值</th>
    <th align="left" width="10%">提现</th>
    <th align="left" width="10%">操作历史</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td style="padding-left: 1em;"><%# Container.ItemIndex + pageSize * (page - 1) + 1%></td>
    <td><%# Eval("user_name")%></td>
    <td><%# QueryChargingMoney((dt_users) Container.DataItem).ToString("c") %></td>
    <td><%# QueryWithdrawingMoney((dt_users) Container.DataItem).ToString("c") %></td>
    <td><a href="bank_transaction_charge.aspx?user_id=<%#Eval("id")%>">手工充值</a></td>
    <td><a href="bank_account_list.aspx?user_id=<%#Eval("id")%>">选择银行卡（<%# Eval("li_bank_accounts.Count")%>）</a></td>
    <td><a href="bank_transaction_list_all.aspx?user_id=<%#Eval("id")%>">查看（<%# GetBankTransactionCount((dt_users) Container.DataItem) %>）</a></td>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"7\">暂无记录</td></tr>" : ""%>
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
