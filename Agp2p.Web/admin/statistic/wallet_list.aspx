<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="wallet_list.aspx.cs" Inherits="Agp2p.Web.admin.statistic.wallet_list" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>用户钱包列表</title>
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
        $("table#wallet").tablesorter({
            headers: {
                2: { sorter: 'rmb' },
                3: { sorter: 'rmb' },
                4: { sorter: 'rmb' },
                5: { sorter: 'rmb' },
                6: { sorter: 'rmb' },
                7: { sorter: 'rmb' },
                8: { sorter: 'rmb' },
                9: { sorter: 'rmb' },
                10: { sorter: 'rmb' },
                11: { sorter: 'rmb' },
                12: { sorter: 'short-date' }
            }
        });

        smartFloatTableHeader();
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
  <span>用户钱包列表</span>
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
                <div class="menu-list" style="display: inline-block; float: left; margin-right: 8px; margin-left: 8px;">
                    <div class="rule-single-select">
                        <asp:DropDownList ID="ddlUserGroud" AutoPostBack="True" runat="server" OnSelectedIndexChanged="ddlUserGroud_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>
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
        <th align="left" width="4%" style="padding-left: 1em;">序号</th>
        <th align="left" width="6%">用户</th>
        <th align="right" width="8%">可用余额</th>
        <th align="right" width="8%">冻结金额</th>
        <th align="right" width="8%">在投金额</th>

        <th align="right" width="8%">已还本金</th>
        <th align="right" width="9%">累计投资</th>
        <th align="right" width="5%">待收益</th>
        <th align="right" width="5%">已收益</th>    
        <th align="right" width="7%">累计收益</th>

        <th align="right" width="7%">累计充值</th>
        <th align="right" width="7%">累计提现</th>
        <th align="center" width="7%">更新时间</th>
     </tr>
   </thead> 
</HeaderTemplate>
<ItemTemplate>
  <tr <%# ((li_wallets)Container.DataItem).user_id == 0 ? "class='sum'" : ""%>>
    <td style="padding-left: 1em;"><%# ((li_wallets)Container.DataItem).user_id == 0 ? "" : (Container.ItemIndex + pageSize * (page - 1) + 1).ToString() %></td>
    <td><%# ((li_wallets)Container.DataItem).user_id == 0 ? "总计" : ""%>
        <a <%# ((li_wallets)Container.DataItem).user_id == 0 ? "style='display:none'" : ""%> href="wallet_history_list.aspx?user_id=<%#Eval("user_id")%>"><%#Eval("dt_users.real_name") != null && Eval("dt_users.real_name") != "" ? Eval("dt_users.real_name") : Eval("dt_users.user_name")%></a></td>
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("idle_money")).ToString("c")%></td>
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("locked_money")).ToString("c")%></td>
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("investing_money")).ToString("c")%></td>

    <td style="text-align:right"><%# (Convert.ToDecimal(Eval("total_investment")) - Convert.ToDecimal(Eval("investing_money"))).ToString("c")%></td>    
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("total_investment")).ToString("c")%></td>   
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("profiting_money")).ToString("c")%></td> 
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("total_profit")).ToString("c")%></td>
    <td style="text-align:right"><%# (Convert.ToDecimal(Eval("profiting_money")) + Convert.ToDecimal(Eval("total_profit"))).ToString("c")%></td>

    <td style="text-align:right"><%# Convert.ToInt32(Eval("total_charge")).ToString("c")%></td>
    <td style="text-align:right"><%# Convert.ToInt32(Eval("total_withdraw")).ToString("c")%></td>
    <td style="text-align:center"><%# string.Format("{0:yy-MM-dd HH:mm}", Eval("last_update_time"))%></td>    
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
</body>
</html>
