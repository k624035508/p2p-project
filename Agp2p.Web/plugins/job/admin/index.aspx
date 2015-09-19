 <%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="Lip2p.Web.Plugin.Job.admin.index" %>
 <%@ Import namespace="Lip2p.Common" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>在线招聘管理</title>
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
  <span>招聘插件</span>
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
    <asp:Repeater ID="rptList" runat="server" onitemcommand="rptList_ItemCommand">
    <HeaderTemplate>
   <table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
      <tr>
        <th width="6%">选择</th>
        <th align="left">招聘职位</th>
        <th width="10%" align="center">招聘部门</th>
        <th width="10%" align="center">招聘人数</th>
        <th width="13%" align="center">开始日期</th>
        <th width="13%" align="center">结束如期</th>
        <th width="13%" align="center">排序</th>
        <th width="8%" align="center">状态</th>
        <th width="8%" align="center">操作</th>
      </tr>
    </HeaderTemplate>
    <ItemTemplate>
      <tr>
        <td align="center"><asp:CheckBox ID="chkId" CssClass="checkall" runat="server" /><asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" /></td>
        <td><a href="edit.aspx?action=Edit&id=<%#Eval("id")%>"><%#Eval("title")%></a></td>
        <td align="center"><%# Eval("depart")%></td>
        <td align="center"><%# Eval("number")%></td>
        <td align="center"><%#string.Format("{0:g}",Eval("start_time"))%></td>
        <td align="center"><%#string.Format("{0:g}",Eval("end_time"))%></td>
        <td align="center"><asp:TextBox ID="txtSortId" runat="server" Text='<%#Eval("sort_id")%>' CssClass="sort" onkeypress="return (/[\d]/.test(String.fromCharCode(event.keyCode)));" /></td>
        <td align="center">
          <asp:ImageButton ID="ibtnLock" CommandName="ibtnLock" runat="server" ImageUrl='<%# Convert.ToInt32(Eval("is_lock")) == 0 ? "../../../"+siteConfig.webmanagepath+"/skin/default/au_1.gif" : "../../../"+siteConfig.webmanagepath+"/skin/default/au_0.gif"%>' ToolTip='<%# Convert.ToInt32(Eval("is_lock")) == 0 ? "关闭招聘" : "启用招聘"%>' />
        </td>
        <td align="center"><a href="edit.aspx?action=Edit&id=<%#Eval("id")%>">修改</a></td>
      </tr>
    </ItemTemplate>
    <FooterTemplate>
      <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"9\">暂无记录</td></tr>" : ""%>
      </table>
    </FooterTemplate>
    </asp:Repeater>
    <!--列表展示.结束-->
    <div class="line10"></div>

</form>
</body>
</html>
