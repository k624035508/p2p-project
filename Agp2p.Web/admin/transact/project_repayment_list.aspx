<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="project_repayment_list.aspx.cs" Inherits="Agp2p.Web.admin.transact.project_repayment_list" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>还款情况列表</title>
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
  <a href="project_list.aspx"><span>项目列表</span></a>
  <i class="arrow"></i>
  <span>还款情况列表</span>
</div>
<!--/导航栏-->

<!--列表-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th align="left" width="10%" style="padding-left: 1em;">期数</th>
    <th align="left" width="10%">还款类型</th>
    <th align="left" width="10%">状态</th>
    <th align="left" width="10%">利息分发总额</th>
    <th align="left" width="10%">本金返还总额</th>
    <th align="left" width="10%">应还款时间</th>
    <th align="left" width="10%">实际还款时间</th>
    <th align="left" width="10%">操作</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td style="padding-left: 1em;"><%# Eval("term")%></td>
    <td><%# GetRepaymentTaskType(Container.DataItem as li_repayment_tasks) %></td>
    <td><%# Utils.GetAgp2pEnumDes((Agp2pEnums.RepaymentStatusEnum) Convert.ToByte(Eval("status")))%></td>
    <td><%# Convert.ToDecimal(Eval("repay_interest")).ToString("c")%></td>
    <td><%# Convert.ToDecimal(Eval("repay_principal")).ToString("c")%></td>
    <td><%# Eval("should_repay_time")%></td>
    <td><%# Eval("repay_at")%></td>
    <td><asp:Button ID="btnRepayNow" runat="server" Text="马上还款" UseSubmitBehavior="False" CommandArgument='<%# Eval("id") %>' OnClientClick="return ExeNoCheckPostBack(this.name, '确认进行还款？');"
        OnClick="btnRepayNow_OnClick" Visible='<%# (Convert.ToByte(Eval("status")) == (byte) Agp2pEnums.RepaymentStatusEnum.Unpaid || Convert.ToByte(Eval("status")) == (byte) Agp2pEnums.RepaymentStatusEnum.OverTime)%>'/></td>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"8\">暂无记录</td></tr>" : ""%>
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
