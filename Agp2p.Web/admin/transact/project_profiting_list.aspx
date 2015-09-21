<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="project_profiting_list.aspx.cs" Inherits="Agp2p.Web.admin.transact.project_profiting_list" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>融资情况列表</title>
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
  <a href="project_investment_list.aspx?project_id=<%= project_id %>"><span>融资情况列表</span></a>
  <i class="arrow"></i>
  <span>收益情况列表</span>
</div>
<!--/导航栏-->

<!--列表-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th align="left" style="padding-left: 1em" width="3%">序号</th>
    <th align="left" >投资者</th>
    <th align="left" width="10%">交易类型</th>
    <th align="left" width="10%">本金返还</th>
    <th align="left" width="10%">获利</th>
    <th align="left" width="10%">交易时间</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td style="padding-left: 1em"><%# Container.ItemIndex + pageSize * (page - 1) + 1 %></td>
    <td ><%# Eval("dt_users.user_name")%></td>
    <td><%# Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectTransactionTypeEnum) Convert.ToByte(Eval("type"))) %></td>
    <td><%# Convert.ToDecimal(Eval("principal")).ToString("c") %></td>
    <td><%# Convert.ToDecimal(Eval("interest")).ToString("c")%></td>
    <td><%# Eval("create_time")%></td>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"5\">暂无记录</td></tr>" : ""%>
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
