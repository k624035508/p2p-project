<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="mortgage_list.aspx.cs" Inherits="Lip2p.Web.admin.loaner.mortgage_list" %>
<%@ Import namespace="Lip2p.Common" %>
<%@ Import Namespace="Lip2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>标的物列表</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
<% if (IsReadonly()) { %>
<style>
    .toolbar { padding-top: 0; }
    .toolbar-wrap { padding-top: 0; }
</style>
<% } %>
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location" <%=IsReadonly() ? "style='display:none'" : "" %> >
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <a href="loaner_list.aspx"><span>借贷人列表</span></a>
  <i class="arrow"></i>
  <span>标的物列表</span>
</div>
<!--/导航栏-->

<!--工具栏-->
<div class="toolbar-wrap">
  <div id="floatHead" class="toolbar">
    <div class="l-list" style="width: 80%">
      <ul class="icon-list" <%=IsReadonly() ? "style='display:none'" : "" %>>
        <li><a class="add" href="mortgage_edit.aspx?action=<%=DTEnums.ActionEnum.Add %>&owner_id=<%=loaner_id %>"><i></i><span>新增</span></a></li>
        <li><a class="all" href="javascript:;" onclick="checkAll(this);"><i></i><span>全选</span></a></li>
        <li><asp:LinkButton ID="btnDelete" runat="server" CssClass="del" OnClientClick="return ExePostBack('btnDelete');" onclick="btnDelete_Click"><i></i><span>删除</span></asp:LinkButton></li>
      </ul>
      <div class="rule-multi-radio" style="margin-left: 1em">
          <asp:RadioButtonList ID="rblMortgageType" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="rblMortgageType_OnSelectedIndexChanged"/>
      </div>
    </div>
    <div class="r-list" <%=IsReadonly() ? "style='display:none'" : "" %>>
      <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return checkNumber(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"/>
      <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" onclick="btnSearch_Click">查询</asp:LinkButton>
    </div>
  </div>
</div>
<!--/工具栏-->

<!--列表-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th width="5%">选择</th>
    <th align="left">名称</th>
    <th align="left" width="10%">类型</th>
    <th align="left" width="10%">所有者</th>
    <%= GenerateDynamicTableHead() %>
    <th align="left" width="10%">估值</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td align="center">
      <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" style="vertical-align:middle;" />
      <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" />
    </td>
      <% if (IsReadonly()) { %>
    <td><%# Eval("name") %></td>
      <% } else { %>
    <td><a href="mortgage_edit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&id=<%#Eval("id")%>"><%# Eval("name") %></a></td>
      <% } %>
    <td><%# Eval("li_mortgage_types.name") %></td>
    <td><%# QueryOwnerNameById(Eval("owner"))%></td>
    <%# GenerateDynamicTableData((li_mortgages) Container.DataItem) %>
    <td><%# Eval("valuation")%></td>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"9\">暂无记录</td></tr>" : ""%>
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
<script>
    $(function () {
        $('a[href=""]').hide();
    });
</script>
</body>
</html>
