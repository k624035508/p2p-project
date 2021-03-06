﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChoiceUser.aspx.cs" Inherits="Agp2p.Web.admin.users.ChoiceUser" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.BLL" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>批量添加用户</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
    <script>
        
        $(function () {
            $(".all").children("span").text("取消");
            $(".checkall input:enabled").prop("checked", true);
            $("#hidcheck").val("0");
        });

        //全选取消按钮函数
        function checkAllu(chkobj) {
            if ($(chkobj).text() == "全选") {
                $(chkobj).children("span").text("取消");
                $(".checkall input:enabled").prop("checked", true);
                $("#hidcheck").val("0");
            } else {
                $(chkobj).children("span").text("全选");
                $(".checkall input:enabled").prop("checked", false);
                $("#hidcheck").val("1");
            }
        }
    </script>
</head>
<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>批量选择<%=GetGroupName() %>组内发送成员</span>
</div>
<!--/导航栏-->

<!--工具栏-->
    <div class="toolbar-wrap">
        <div id="floatHead" class="toolbar">
            <div class="l-list">
                 <ul class="icon-list">
                    <li><a class="all" href="javascript:;" onclick="checkAllu(this);"><i></i><span>全选</span></a></li>
                    <li>
                        <asp:LinkButton ID="btnDoSwitch" runat="server" CssClass="save" OnClientClick="return ExePostBack('btnDoSwitch', '确认所选发送成员？');"
                            OnClick="btnDoSwitch_Click"><i></i><span>批量选择</span></asp:LinkButton></li>
                </ul>
            </div>
            <div class="r-list">
                 <div style="display: inline-block;">
                    <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" ></asp:TextBox>
                    <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
     <asp:HiddenField ID="hidcheck"  runat="server" />
     <asp:HiddenField ID="hidtitle"  runat="server" />
     <asp:HiddenField ID="hidgroupNames"  runat="server" />
     <asp:HiddenField ID="hidmobiles"  runat="server" />
<!--/工具栏-->

<!--列表-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th width="3%">选择</th>
    <th align="left" width="3%">序号</th>
    <th align="left" width="5%">用户名</th>
    <th align="left" width="5%">当前组别</th>
    <th align="left" width="5%">注册时间</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td align="center">
      <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" style="vertical-align:middle;"/>
      <asp:HiddenField ID="hidId" Value='<%#Eval("mobile")%>' runat="server" />
    </td>
    <td><%# Container.ItemIndex + pageSize * (page - 1) + 1 %></td>
    <td><a href="user_edit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&id=<%#Eval("id")%>"><%#Eval("real_name") != null && Eval("real_name") != "" ? Eval("user_name") + "(" + Eval("real_name") + ")" : Eval("user_name")%></a></td>
    <td><%# Eval("dt_user_groups.title")%></td>
    <td><%# Eval("reg_time")%></td>
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

