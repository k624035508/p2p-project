<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="agent_buying_claims.aspx.cs" Inherits="Agp2p.Web.admin.claims.agent_buying_claims" EnableEventValidation="false" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>
<%@ Import Namespace="Agp2p.Web.admin.claims" %>

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
  <span>中间人可投债权列表</span>
</div>
<!--/导航栏-->

<!--工具栏-->
<div class="toolbar-wrap">
  <div id="floatHead" class="toolbar">
    <div class="l-list">
      <div class="menu-list" style="display: inline-block; float: left; margin-right: 8px; margin-left: 8px;">
          <div class="rule-single-select">
              <asp:DropDownList ID="ddlAgent" AutoPostBack="True" runat="server" OnSelectedIndexChanged="ddlAgent_OnSelectedIndexChanged">
              </asp:DropDownList>
          </div>
      </div>
    </div>
    <div class="r-list">
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
    <th align="left" width="10%">债权所有者</th>
    <th align="left" width="10%">本金</th>
    <th align="left" width="10%">剩余可投</th>
    <th align="left" width="10%">项目</th>
    <th align="left" width="10%">申请转让时间</th>
    <th align="left" width="10%">操作</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td style="padding-left: 1em;"><%# Container.ItemIndex + pageSize * (page - 1) + 1%></td>
    <td><%# Eval("Owner") %></td>
    <td><%# Eval("Principal") %></td>
    <td><%# Eval("BuyableAmount") %></td>
    <td><%# Eval("ProjectName") %></td>
    <td><%# Eval("WithdrawTime") %></td>
      <td><asp:LinkButton ID="btnBuy" runat="server" OnClick="btnBuy_OnClick"
              CommandArgument="<%# ((BuyableClaim) Container.DataItem).ClaimId %>"
              principal="<%# ((BuyableClaim) Container.DataItem).Principal%>"
              OnClientClick="return PromptPostBack(this.id, '请输入买入金额：', this.getAttribute('principal'));"
              Text="买入" /></td>
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
