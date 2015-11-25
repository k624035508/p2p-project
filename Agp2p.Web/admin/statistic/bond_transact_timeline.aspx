<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="bond_transact_timeline.aspx.cs" Inherits="Agp2p.Web.admin.statistic.bond_transact_timeline" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>
<%@ Import Namespace="Agp2p.Web.admin.statistic" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>风险保证金明细</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/tablesorter/jquery.tablesorter.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link href="../../css/pagination.css" rel="stylesheet" type="text/css" />
<link href="../../scripts/tablesorter/table-sorter-style.css" rel="stylesheet" type="text/css" />
<style>
tr.sum td { color: red; }
thead * {
  -webkit-box-sizing: border-box;
     -moz-box-sizing: border-box;
          box-sizing: border-box;
}
</style>
<script>
    function smartFloatTableHeader() {
        $('thead').smartFloat(function(elem) {
            $(elem).css('top', '52px');
        });

        var firstRowTds = $('tbody tr:first td');
        $('thead th').each(function(index, elem) {

            //keep th width
            $(elem).css('width', firstRowTds[index].offsetWidth + 'px');

            // keep td width
            firstRowTds[index].setAttribute('width', elem.getAttribute('width'));
        });
    }

    $(function () {
        $.tablesorter.addParser({
            id: 'rmb',
            is: function () { return false; },
            format: function (s) { return s.replace(/[¥,]/g, ''); },
            type: 'numeric'
        });
        $.tablesorter.addParser({
            id: 'short-date',
            is: function () { return false; },
            format: function (s) { return s; },
            type: 'text'
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
  <span>风险保证金明细</span>
</div>
<!--/导航栏-->

<!--工具栏-->
    <div class="toolbar-wrap">
        <div id="floatHead" class="toolbar">
            <div class="l-list">
                <ul class="icon-list">
                    <li><asp:LinkButton ID="btnExportExcel" runat="server" CssClass="quotes" OnClick="btnExportExcel_Click"><i></i><span>导出 Excel</span></asp:LinkButton></li>
                </ul>
            </div>
            <div class="r-list">
                <div style="display: inline-block;" class="rl">时间段：</div>
                <div class="input-date" style="display: inline-block; float:left;">
                    <asp:TextBox ID="txtStartTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                        datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                        sucmsg=" " Style="font-size: 15px" />
                    <i></i>
                </div>
                <span  class="rl">到</span>
                <div class="input-date" style="display: inline-block; float:left; margin-right:10px;">
                    <asp:TextBox ID="txtEndTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                        datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                        sucmsg=" " Style="font-size: 15px" />
                    <i></i>
                </div>
                <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True" />
                <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
            </div>
        </div>
    </div>
<!--/工具栏-->

<!--列表-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table id="wallet" width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
   <thead>
      <tr>
        <th align="center" width="5%" style="padding-left: 1em;">序号</th>
        <th align="center" width="12%">时间</th>
        <th align="right" width="8%">收入</th>
        <th align="right" width="8%">支出</th>
        <th align="center" width="12%">关联项目</th>
        <th align="center" width="8%">关联人员</th>
        <th align="center" width="15%">备注</th>
     </tr>
   </thead> 
</HeaderTemplate>
<ItemTemplate>
  <tr <%# ((BondTransaction)Container.DataItem).index == null ? "class='sum'" : ""%>>
    <td style="text-align:center"><%# Eval("index") %></td>
    <td style="text-align:center"><%# Eval("occurTime")%></td>    
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("income")).ToString("c")%></td>
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("outcome")).ToString("c")%></td>
    <td style="text-align: left;padding-left:50px"><%# Eval("project")%></td>
    <td style="text-align:center"><%# Eval("user")%></td>
    <td style="text-align:left"><%# Eval("remark")%></td>
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
    <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);"
        ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox><span>条/页</span>
  </div>
  <div id="PageContent" runat="server" class="default"></div>
</div>
<!--/内容底部-->
</form>
</body>
</html>
