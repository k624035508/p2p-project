<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="bank_transaction_list_account.aspx.cs" Inherits="Agp2p.Web.admin.transact.bank_transaction_list_account" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.BLL" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>银行账户的交易列表</title>
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
  <a href="bank_users_charge_withdraw.aspx"><span>用户列表</span></a>
  <i class="arrow"></i>
  <a href="bank_account_list.aspx?user_id=<%=GetUserId() %>"><span><%= GetUserName() %> 银行账户</span></a>
  <i class="arrow"></i>
  <span><%= GetBankName() %> 的交易列表</span>
</div>
<!--/导航栏-->

<!--工具栏-->
<div class="toolbar-wrap">
  <div id="floatHead" class="toolbar">
    <div class="l-list">
      <ul class="icon-list">
        <li><a class="add" href="bank_transaction_withdraw.aspx?account_id=<%=account_id %>"><i></i><span>提现</span></a></li>
      </ul>
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
    <th align="left" width="10%">交易金额</th>
    <th align="left" width="10%">交易类型</th>
    <th align="left" width="10%">交易状态</th>
    <th align="left" width="10%">手续费</th>
    <th align="left" width="10%">交易时间</th>
    <th align="left" width="10%">创建时间</th>
    <th align="left" width="10%">操作</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td style="padding-left: 1em;"><%# Container.ItemIndex + pageSize * (page - 1) + 1%></td>
    <td><%# Convert.ToDecimal(Eval("value")).ToString("c") %></td>
    <td><%# Utils.GetAgp2pEnumDes((Agp2pEnums.BankTransactionTypeEnum) Convert.ToByte(Eval("type")))%></td>
    <td><%# Utils.GetAgp2pEnumDes((Agp2pEnums.BankTransactionStatusEnum) Convert.ToByte(Eval("status")))%></td>
    <td><%# Utils.GetAgp2pEnumDes((Agp2pEnums.BankTransactionHandlingFeeTypeEnum)Convert.ToByte(Eval("handling_fee_type")))%>（<%# GetHandlingFee((li_bank_transactions) Container.DataItem).ToString("c")%> 元）</td>
    <td><%# Eval("transact_time")%></td>
    <td><%# Eval("create_time")%></td>
    <td><asp:Button ID="btnConfirm" runat="server" Text="确认" UseSubmitBehavior="False" CommandArgument='<%# Eval("id") %>' OnClientClick="return ExeNoCheckPostBack(this.name, '确认银行交易？');"
        OnClick="btnConfirm_OnClick" Visible='<%# Convert.ToByte(Eval("status")) == (byte) Agp2pEnums.BankTransactionStatusEnum.Acting%>'/>

        <asp:Button ID="btnCancel" runat="server" Text="取消" UseSubmitBehavior="False" CommandArgument='<%# Eval("id") %>' OnClientClick="return ExeNoCheckPostBack(this.name, '取消银行交易？');"
        OnClick="btnCancel_OnClick" Visible='<%# Convert.ToByte(Eval("status")) == (byte) Agp2pEnums.BankTransactionStatusEnum.Acting%>'/></td>
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
