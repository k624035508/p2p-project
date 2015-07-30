<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="bank_transaction_withdrawing_list.aspx.cs" Inherits="Lip2p.Web.admin.audit.bank_transaction_withdrawing_list" %>
<%@ Import namespace="Lip2p.Common" %>
<%@ Import Namespace="Lip2p.BLL" %>
<%@ Import Namespace="Lip2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>提现审批</title>
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
  <span>提现审批</span>
</div>
<!--/导航栏-->

<!--工具栏-->
<div class="toolbar-wrap">
  <div id="floatHead" class="toolbar">
    <div class="l-list">
      <ul class="icon-list">
        <li><a class="all" href="javascript:;" onclick="checkAll(this);"><i></i><span>全选</span></a></li>
        <li><asp:LinkButton ID="btnConfirm" runat="server" CssClass="save" OnClientClick="return ExePostBack('btnConfirm', '确认全部审批通过？');" onclick="btnConfirmCancel_Click"><i></i><span>审批通过</span></asp:LinkButton></li>
        <li><asp:LinkButton ID="btnCancel" runat="server" CssClass="save" OnClientClick="return ExePostBack('btnCancel', '确认全部审批不通过？');" onclick="btnConfirmCancel_Click"><i></i><span>审批不通过</span></asp:LinkButton></li>
        <li><asp:LinkButton ID="btnExportExcel" runat="server" CssClass="quotes" onclick="btnExportExcel_Click"><i></i><span>导出Excel</span></asp:LinkButton></li>
      </ul>
    </div>
    <!--银行交易状态选择，选择后刷新列表-->
    <div class="r-list">
      <div style="display: inline-block;" class="rl">提现状态：</div>
      <div class="rule-multi-radio" style="display: inline-block; margin-right:10px; float:left;">
        <asp:RadioButtonList ID="rblBankTransactionStatus" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="rblBankTransactionStatus_OnSelectedIndexChanged">
        <asp:ListItem Value="0">不限</asp:ListItem>
        <asp:ListItem Value="1" Selected="True">待处理</asp:ListItem>
        <asp:ListItem Value="2">已确认</asp:ListItem>
        <asp:ListItem Value="3">已取消</asp:ListItem>
        </asp:RadioButtonList>
      </div>
        <div style="display: inline-block;" class="rl">更新时间：</div>
        <div class="input-date" style="display: inline-block; float: left;">
            <asp:TextBox ID="txtStartTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                sucmsg=" " Style="font-size: 15px" />
            <i>日期</i>
        </div>
        <span class="rl">到</span>
        <div class="input-date" style="display: inline-block; float: left;">
            <asp:TextBox ID="txtEndTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                sucmsg=" " Style="font-size: 15px" />
            <i>日期</i>
        </div>
        <div class="menu-list" style="display: inline-block; float: left; margin-right: 8px; margin-left: 8px;">
            <div class="rule-single-select">
                <asp:DropDownList ID="ddlUserGroud" AutoPostBack="True" runat="server" OnSelectedIndexChanged="ddlUserGroud_SelectedIndexChanged">
                </asp:DropDownList>
            </div>
        </div>
      <div style="display: inline-block;">
          <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword"  onkeydown="return Enter(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox>
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
    <th width="3%">选择</th>
    <th align="left" width="3%">序号</th>
    <th align="left" width="6%">申请人</th>
    <th align="left" width="6%">提现金额</th>
    <th align="left" width="6%">实付金额</th>
    <th align="left" width="6%">申请时间</th>
    <th align="left" width="6%">付款时间</th>
    <th align="left" width="8%">操作后余额</th>    
    <th align="left" width="7%">银行名称</th>
    <th align="left" width="6%">银行开户名</th>
    <th align="left" width="14%">银行开户行</th>
    <th align="left" width="10%">银行帐号</th>
    <th align="left" width="4%">提现状态</th>
    <th align="left" width="8%">手续费</th>
    <%--<th align="left" width="10%">流水号</th>--%>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td align="center">
      <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" style="vertical-align:middle;" Enabled='<%# Convert.ToByte(Eval("status")) == (byte) Lip2pEnums.BankTransactionStatusEnum.Acting %>' />
      <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" />
    </td>
    <td><%# Container.ItemIndex + pageSize * (page - 1) + 1 %></td>
    <td><a href="../users/user_edit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&id=<%#Eval("li_bank_accounts.dt_users.id")%>"><%#Eval("li_bank_accounts.dt_users.real_name")%></a></td>
    <td><%# Convert.ToDecimal(Eval("value")).ToString("c") %></td>
    <td><%# (Convert.ToDecimal(Eval("value")) - GetHandlingFee((li_bank_transactions)Container.DataItem)).ToString("c")%></td>
    <td><%# string.Format("{0:yy/MM/dd HH:mm}", Eval("create_time"))%></td>
    <td><%# string.Format("{0:yy/MM/dd HH:mm}", Eval("transact_time"))%></td>
    <td><%# QueryIdleMoney(((li_bank_transactions)Container.DataItem)) %></td>
    <td><%# Eval("li_bank_accounts.bank")%></td>
    <td><%# Eval("li_bank_accounts.dt_users.real_name")%></td>
    <td title="<%# Eval("li_bank_accounts.location") == null ? "无地区信息" : Eval("li_bank_accounts.location").ToString()%>"><%# Eval("li_bank_accounts.opening_bank")%></td>
    <td><%# Eval("li_bank_accounts.account")%></td>
    <td><%# Utils.GetLip2pEnumDes((Lip2pEnums.BankTransactionStatusEnum) Convert.ToByte(Eval("status")))%></td>
    <td><%# Utils.GetLip2pEnumDes((Lip2pEnums.BankTransactionHandlingFeeTypeEnum) Convert.ToByte(Eval("handling_fee_type")))%>（<%# GetHandlingFee((li_bank_transactions) Container.DataItem).ToString("c")%>元）</td>
    <%--<td><%# Eval("no_order")%></td>--%>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count >0 ? "<tr><td colspan=\"2\"></td><td style=\"color: red;\">合计</td><td style=\"color: red;\">"+Convert.ToDecimal(value).ToString("c")+"</td><td style=\"color: red;\">"+Convert.ToDecimal(value1).ToString("c")+"</td><td colspan=\"9\">&nbsp;</td></tr>" : ""%>
  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"14\">暂无记录</td></tr>" : ""%>
</table>
</FooterTemplate>
</asp:Repeater>
<!--/列表-->

<!--内容底部-->
<div class="line20"></div>
<div class="pagelist">
  <div class="l-btns">
    <span>显示示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox><span>条/页</span>
  </div>
  <div id="PageContent" runat="server" class="default"></div>
</div>
<!--/内容底部-->
</form>
</body>
</html>
