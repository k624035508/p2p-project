<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="investment_rank_list.aspx.cs" Inherits="Lip2p.Web.admin.statistic.investment_rank_list" %>
<%@ Import namespace="Lip2p.Common" %>
<%@ Import Namespace="Lip2p.Linq2SQL" %>
<%@ Import Namespace="Lip2p.BLL" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>投资排行榜</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/tablesorter/jquery.tablesorter.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link href="../../css/pagination.css" rel="stylesheet" type="text/css" />
<link href="../../scripts/tablesorter/table-sorter-style.css" rel="stylesheet" type="text/css" />
<style>
td.align-right {
    text-align: right;
    padding-right: 1em;
}
th.align-right {
    text-align: right;
}
</style>
<script>
    $(function() {
        $.tablesorter.addParser({
            id: 'rmb',
            is: function() { return false; },
            format: function (s) { return s.replace(/[¥,]/g, ''); },
            type: 'numeric'
        });
        $("table#main").tablesorter({
            headers: {
                2: { sorter: 'rmb' },
                4: { sorter: 'rmb' }
            }
        });
    });
</script>
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>投资排行榜</span>
</div>
<!--/导航栏-->

<!--工具栏-->
    <div class="toolbar-wrap">
        <div id="floatHead" class="toolbar">
            <div class="l-list">
                <ul class="icon-list">
                     <li><asp:LinkButton ID="btnExportExcel" runat="server" CssClass="quotes" onclick="btnExportExcel_Click"><i></i><span>导出 Excel</span></asp:LinkButton></li>
                </ul>
            </div>
            <div class="r-list">
                <div style="display: inline-block;" class="rl">时间段：</div>
                <div class="input-date" style="display: inline-block; float:left;">
                    <asp:TextBox ID="txtStartTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd HH:mm:ss'})"
                        datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                        sucmsg=" " Style="font-size: 15px" />
                    <i>日期</i>
                </div>
                <span style=" display:inline-block;" class="rl">到</span>
                <div class="input-date" style="display: inline-block; float: left; margin-right:10px;">
                    <asp:TextBox ID="txtEndTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd HH:mm:ss'})"
                        datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                        sucmsg=" " Style="font-size: 15px" />
                    <i>日期</i>
                </div>
                <div style="display: inline-block;">
                    <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True" ></asp:TextBox>
                    <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
<!--/工具栏-->

<!--列表-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table id="main" width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <thead>
  <tr>
    <th align="left" width="3%" style="padding-left: 1em;">序号</th>
    <th align="left" width="7%">用户</th>
    <th class="align-right" width="6%">投资金额</th>    
    <th class="align-right" width="6%">投资过的邀请人数</th>
    <th class="align-right" width="5%">邀请人首次投资金额</th>    
  </tr>
  </thead>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td style="padding-left: 1em;"><%# Container.ItemIndex + pageSize * (page - 1) + 1 %></td>
      <td>
          <a href="../users/user_edit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&id=<%#Eval("User.id")%>">
              <%#Eval("User.real_name") != null && Eval("User.real_name") != "" ? Eval("User.user_name") + "(" + Eval("User.real_name") + ")" : Eval("User.user_name")%>
          </a>
      </td>
    <td class="align-right"><%# Convert.ToDecimal(Eval("InvestmentSum")).ToString("c")%></td>
    <td class="align-right"><%# Eval("InviteeCount")%></td>
    <td class="align-right"><%# Convert.ToDecimal(Eval("InviteeInvestmentSum")).ToString("c")%></td>
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
