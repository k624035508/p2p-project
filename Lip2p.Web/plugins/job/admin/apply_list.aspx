 <%@ Page Language="C#" AutoEventWireup="true" CodeBehind="apply_list.aspx.cs" Inherits="Lip2p.Web.Plugin.Job.admin.apply_list" %>
  <%@ Import namespace="Lip2p.Common" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>应聘管理</title>
<link type="text/css" rel="stylesheet" href="../../../<%=siteConfig.webmanagepath %>/skin/default/style.css" />
<link type="text/css" rel="stylesheet" href="../../../css/pagination.css" />
<script type="text/javascript" src="../../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../../<%=siteConfig.webmanagepath %>/js/layout.js"></script>
</head>
<body class="mainbody">
<form id="form2" runat="server">
 <!--导航栏-->
<div class="location">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../../../<%=siteConfig.webmanagepath %>/center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>插件管理</span>
  <i class="arrow"></i>
  <span>应聘管理</span>
</div>
<!--/导航栏-->
<!--工具栏-->
<div class="toolbar-wrap">
  <div id="floatHead" class="toolbar">
    <div class="l-list">
      <ul class="icon-list">
        <li><a class="add" href="edit.aspx?action=<%=DTEnums.ActionEnum.Add %>"><i></i><span>新增</span></a></li>
        <li><a class="all" href="apply_list.aspx"><i></i><span>应聘列表</span></a></li>
        <li><a class="all" href="javascript:;" onclick="checkAll(this);"><i></i><span>全选</span></a></li>
        <li><asp:LinkButton ID="btnSave" runat="server" CssClass="save" onclick="btnSave_Click"><i></i><span>保存排序</span></asp:LinkButton></li>
        <li><asp:LinkButton ID="btnDelete" runat="server" CssClass="del" OnClientClick="return ExePostBack('btnDelete');" onclick="btnDelete_Click"><i></i><span>删除</span></asp:LinkButton></li>
      </ul>
    </div>
    <div class="r-list">
       <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword"></asp:TextBox>
       <asp:Button ID="btnSearch" runat="server" Text="查询" CssClass="btn-search" onclick="btnSearch_Click" />
     
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
        <th align="left">联系人</th>
        <th width="10%" align="center">性别</th>
        <th width="12%" align="center">联系电话</th>
        <th width="12%" align="center">邮箱</th>
        <th width="12%" align="center">应聘岗位</th>
        <th width="12%" align="center">应聘时间</th>
        <th width="12%" align="center">排序</th>
        <th width="8%" align="center">状态</th>
        <th width="8%" align="center">操作</th>
      </tr>
    </HeaderTemplate>
    <ItemTemplate>
      <tr>
        <td align="center"><asp:CheckBox ID="chkId" CssClass="checkall" runat="server" /><asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" /></td>
        <td><a href="apply_edit.aspx?action=Edit&id=<%#Eval("id")%>"><%# Eval("realname")%></a></td>
        <td align="center"><%# Eval("sex")%></td>
        <td align="center"><%# Eval("tel")%></td>
        <td align="center"><%# Eval("email")%></td>
        <td align="center"><%# GetJobName(int.Parse(Eval("job_id").ToString()))%></td>
        <td align="center"><%# Eval("add_time")%></td>
        <td align="center"><asp:TextBox ID="txtSortId" runat="server" Text='<%#Eval("sort_id")%>' CssClass="sort" onkeypress="return (/[\d]/.test(String.fromCharCode(event.keyCode)));" /></td>
        <td align="center"><%# Eval("is_see").ToString()=="1"?"已查看":"未查看" %></td>
        <td align="center"><a href="apply_edit.aspx?action=Edit&id=<%#Eval("id")%>">查看</a></td>
      </tr>
    </ItemTemplate>
    <FooterTemplate>
      <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"10\">暂无记录</td></tr>" : ""%>
      </table>
    </FooterTemplate>
    </asp:Repeater>
    <!--列表展示.结束-->
    <div class="line10"></div>

</form>
</body>
</html>
