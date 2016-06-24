<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="all_bank_account_list.aspx.cs" Inherits="Agp2p.Web.admin.transact.all_bank_account_list" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>银行账户列表</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
</head>

<body class="mainbody">
<form id="form2" runat="server">
<!--导航栏-->
<div class="location">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>银行卡列表</span>
</div>
<!--/导航栏-->

<!--工具栏-->
<div class="toolbar-wrap">
  <div id="floatHead" class="toolbar">
    <div class="r-list">
      <asp:TextBox ID="txtKeywords2" runat="server" CssClass="keyword" />
      <asp:LinkButton ID="lbtnSearch2" runat="server" CssClass="btn-search" onclick="btnSearch_Click2">查询</asp:LinkButton>
    </div>
  </div>
</div>
<!--/工具栏-->

<!--列表-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th width="3%" align="left" style="padding-left:.5%;">序号</th>
    <th align="left" width="10%">用户名</th>
    <th align="left" width="10%">银行</th>
    <th align="left" width="20%">开户行</th>
    <th align="left" width="20%">银行账户</th>
    <th align="left" width="10%">交易记录</th>
    <th align="left" width="10%">银行卡状态</th>
    <th align="left" width="10%">操作</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
   <tr>
    <td align="left" style="padding-left:.5%;"><%# Container.ItemIndex + pageSize * (page - 1) + 1%></td> 
    <td><%# Eval("dt_users.real_name") != null && Eval("dt_users.real_name") != "" ? Eval("dt_users.real_name") : Eval("dt_users.user_name")%></td>     
    <td><%# Eval("bank") %></td>
    <td><%# Eval("opening_bank") %></td>
    <td><%# Eval("account") %></td>
    <td><a href="bank_transaction_list_account.aspx?account_id=<%#Eval("id")%>&user_id=<%#Eval("dt_users.id")%>">查看（<%# Eval("li_bank_transactions.Count")%>）</a></td>
    <td>
        <%# GetTypeName(Eval("type").ToString()) %>         
    </td>
    <td>
        <asp:LinkButton  runat="server" ID="lbt_removecard" Text="快捷支付解绑" CommandArgument='<%#Eval("id")%>' OnClientClick="return confirm('确定解绑银行卡吗?');" 
            OnClick="btnRemoveCard_OnClick" Visible='<%#Convert.ToInt16(Eval("type")) == 2  %>'></asp:LinkButton>
        <asp:LinkButton  runat="server" ID="lbt_removecardNormal" Text="普通解绑" CommandArgument='<%#Eval("id")%>' OnClientClick="return confirm('确定解绑银行卡吗?');" 
            OnClick="btnRemoveCardNormal_OnClick" Visible='<%#Convert.ToInt16(Eval("type")) == 1  %>'></asp:LinkButton>
    </td>
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
    <span>显示</span><asp:TextBox ID="txtPageNum2" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox><span>条/页</span>
  </div>
  <div id="PageContent2" runat="server" class="default"></div>
</div>
<!--/内容底部-->
</form>
</body>
</html>
