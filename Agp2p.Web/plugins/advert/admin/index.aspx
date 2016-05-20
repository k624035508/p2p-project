<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="Agp2p.Web.Plugin.Advert.admin.index" %>
<%@ Import namespace="Agp2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>广告管理</title>
<link type="text/css" rel="stylesheet" href="../../../<%=siteConfig.webmanagepath %>/skin/default/style.css" />
<link type="text/css" rel="stylesheet" href="../../../css/pagination.css" />
<script type="text/javascript" src="../../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../../<%=siteConfig.webmanagepath %>/js/layout.js"></script>
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../../../<%=siteConfig.webmanagepath %>/center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>插件管理</span>
  <i class="arrow"></i>
  <span>广告管理</span>
  <i class="arrow"></i>
  <span>广告位</span>
</div>
<!--/导航栏-->

<!--工具栏-->
<div class="toolbar-wrap">
  <div id="floatHead" class="toolbar">
    <div class="l-list">
      <ul class="icon-list">
        <li><a class="add" href="adv_edit.aspx?action=<%=DTEnums.ActionEnum.Add %>"><i></i><span>新增</span></a></li>
        <li><a class="all" href="javascript:;" onclick="checkAll(this);"><i></i><span>全选</span></a></li>
        <li><asp:LinkButton ID="btnDelete" runat="server" CssClass="del" OnClientClick="return ExePostBack('btnDelete');" onclick="btnDelete_Click"><i></i><span>删除</span></asp:LinkButton></li>
      </ul>
    </div>
    <div class="r-list">
       <div class="menu-list rl" style=" display:inline-block;">
        <div class="rule-single-select">
          <asp:DropDownList ID="ddlProperty" runat="server" CssClass="select2" AutoPostBack="True" onselectedindexchanged="ddlProperty_SelectedIndexChanged">
                <asp:ListItem Value="">所有类别</asp:ListItem>
                <asp:ListItem Value="1">文字</asp:ListItem>
                <asp:ListItem Value="2">图片</asp:ListItem>
                <asp:ListItem Value="3">幻灯片</asp:ListItem>
                <asp:ListItem Value="4">动画</asp:ListItem>
                <asp:ListItem Value="5">视频</asp:ListItem>
                <asp:ListItem Value="6">代码</asp:ListItem>
          </asp:DropDownList>
        </div>
      </div>
      <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"  />
      <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn-search" onclick="btnSearch_Click">查询</asp:LinkButton>
    </div>
  </div>
</div>
<!--/工具栏-->

<!--列表展示.开始-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th width="6%">选择</th>
    <th align="left">广告位名称</th>
    <th width="6%" align="left">类型</th>
    <th width="8%">数量</th>
    <th width="12%" align="left">价格</th>
    <th width="10%" align="left">尺寸</th>
    <th width="10%" align="left">链接目标</th>
    <th width="15%" align="left">添加时间</th>
    <th width="15%">操作</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td align="center">
      <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" />
      <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" />
    </td>
    <td><a href="adv_edit.aspx?action=<%=DTEnums.ActionEnum.Edit %>&id=<%#Eval("id")%>"><%#Eval("title")%></a></td>
    <td><%#GetTypeName(Eval("type").ToString())%></td>
    <td align="center"><%#Eval("view_num")%></td>
    <td><%#Eval("price")%>元/月</td>
    <td><%#Eval("view_width")%>×<%#Eval("view_height")%></td>
    <td><%#Eval("target")%></td>
    <td><%#string.Format("{0:g}",Eval("add_time"))%></td>
    <td align="center">
      <a href="bar_list.aspx?aid=<%#Eval("id")%>">内容</a>&nbsp;
      <a href="adv_view.aspx?id=<%#Eval("id")%>">调用</a>&nbsp;
      <a href="adv_edit.aspx?action=<%=DTEnums.ActionEnum.Edit %>&id=<%#Eval("id")%>">编辑</a>
    </td>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"9\">暂无记录</td></tr>" : ""%>
</table>
</FooterTemplate>
</asp:Repeater>
<!--列表展示.结束-->

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
