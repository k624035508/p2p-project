<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="wallet_history_list.aspx.cs" Inherits="Agp2p.Web.admin.statistic.wallet_history_list" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>
<%@ Import Namespace="Agp2p.BLL" %>
<%@ Import Namespace="Agp2p.Core" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>资金明细表</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
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
  <% if (!string.IsNullOrWhiteSpace(user_id))
     { %>
  <a href="wallet_list.aspx"><i></i><span>用户钱包列表</span></a>
  <i class="arrow"></i>
  <span><%= getUserName() %> 的资金日志列表</span>
  <% } else { %>
  <span>平台流水</span>
  <% } %>
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
                <div class="input-date" style="display: inline-block; float:left;">
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
                <div class="menu-list" style="display: inline-block; float: left; margin-right:8px; margin-left:8px;">
                    <div class="rule-single-select">
                        <asp:DropDownList ID="ddlWalletHistoryTypeId" AutoPostBack="True" runat="server" OnSelectedIndexChanged="ddlWalletHistoryTypeId_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>
                </div>
                <div style="display: inline-block;">
                    <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True" />
                    <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
<!--/工具栏-->

<!--列表-->
<asp:Repeater ID="rptList" runat="server" OnItemDataBound="rptList_ItemDataBound">
<HeaderTemplate>
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th align="left" width="3%" style="padding-left: 1em;">序号</th>
      <% if (string.IsNullOrWhiteSpace(user_id)) { %>
    <th align="left" width="5%">用户</th>
      <% } %>
    <th align="center" width="5%">操作类型</th>    
    <th align="right" width="6%">收入金额</th>    
    <th align="right" width="6%">支出金额</th>    
    <th align="right" width="6%">可用余额</th>
    <th align="right" width="6%">在投金额</th>    
    <th align="right" width="6%">累计投资</th>
    <th align="right" width="5%">待收利润</th>
    <th align="right" width="5%">已收利润</th>
    <th align="center" width="6%">交易时间</th>
    <th align="center" width="7%">备注</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td style="padding-left: 1em;"><%# Container.ItemIndex + pageSize * (page - 1) + 1 %></td>
      <% if (string.IsNullOrWhiteSpace(user_id)) { %>
      <td>
          <a href="wallet_history_list.aspx?user_id=<%#Eval("user_id")%>">
              <%#Eval("dt_users.real_name") != null && Eval("dt_users.real_name") != "" ? Eval("dt_users.real_name"): Eval("dt_users.user_name")%>
          </a>
      </td>
      <% } %>
    <td style="text-align:center"><%# Utils.GetAgp2pEnumDes((Agp2pEnums.WalletHistoryTypeEnum) Convert.ToByte(Eval("action_type")))%></td>    
    <td style="text-align:right"><%# TransactionFacade.QueryTransactionIncome<string>((li_wallet_histories)Container.DataItem)%></td>   
    <td style="text-align:right"><%# MoneyToString(TransactionFacade.QueryTransactionOutcome((li_wallet_histories)Container.DataItem)) %></td>     
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("idle_money")).ToString("c")%></td>
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("investing_money")).ToString("c")%></td>
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("total_investment")).ToString("c")%></td>
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("profiting_money")).ToString("c")%></td>    
    <td style="text-align:right"><%# Convert.ToDecimal(Eval("total_profit")).ToString("c")%></td>
    <td style="text-align:center"><%# string.Format("{0:yy-MM-dd HH:mm}", Eval("create_time"))%></td>
    <td style="text-align:left">
        <%# QueryTransactionRemark((li_wallet_histories) Container.DataItem, GetHrefByProjectStatus)%>
    </td>
  </tr>
</ItemTemplate>
<FooterTemplate>
<% if (string.IsNullOrWhiteSpace(user_id)) { %>
  <%# 0 < rptList.Items.Count ? "<tr><td></td><td style=\"color: red;\">合计</td><td>&nbsp;</td><td style=\"color: red;text-align:right;\">"
    +TransactionIncome.ToString("c")+"</td><td style=\"color: red;text-align:right;\">"
    +TransactionOutcome.ToString("c")+"</td><td style=\"color: red;text-align:right;\">"
    +"<td colspan=\"7\">&nbsp;</td></tr>" : ""%>
<% } %>

  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"12\">暂无记录</td></tr>" : ""%>
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
