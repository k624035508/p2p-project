<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="bank_transaction_charging_list.aspx.cs" Inherits="Agp2p.Web.admin.audit.bank_transaction_charging_list" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.BLL" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>充值审批</title>
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
  <span>充值审批</span>
</div>
<!--/导航栏-->

<!--工具栏-->
    <div class="toolbar-wrap">
        <div id="floatHead" class="toolbar">
            <div class="l-list">
                 <ul class="icon-list">
                    <li><a class="all" href="javascript:;" onclick="checkAll(this);"><i></i><span>全选</span></a></li>
                    <li><asp:LinkButton ID="btnConfirm" runat="server" CssClass="save" OnClientClick="return ExePostBack('btnConfirm', '确认审批 通过 选中的充值申请？');" OnClick="btnConfirmCancel_Click"><i></i><span>审批通过</span></asp:LinkButton></li>
                    <li><asp:LinkButton ID="btnCancel" runat="server" CssClass="save" OnClientClick="return ExePostBack('btnCancel', '确认审批 不通过 选中的充值申请？');" OnClick="btnConfirmCancel_Click"><i></i><span>审批不通过</span></asp:LinkButton></li>
                </ul>
            </div>
            <div class="r-list">
                <!--银行交易状态选择，选择后刷新列表-->
                <div style="display: inline-block;" class="rl">充值状态：</div>
                <div class="rule-multi-radio" style="display: inline-block; margin-right: 10px; float: left;">
                    <asp:RadioButtonList ID="rblBankTransactionStatus" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="rblBankTransactionStatus_OnSelectedIndexChanged">
                        <asp:ListItem Value="0">不限</asp:ListItem>
                        <asp:ListItem Value="1" Selected="True">待处理</asp:ListItem>
                        <asp:ListItem Value="2">已确认</asp:ListItem>
                        <asp:ListItem Value="3">已取消</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
                <div style="display: inline-block;" class="rl">创建时间：</div>
                <div class="input-date" style="display: inline-block; float: left;">
                    <asp:TextBox ID="txtStartTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                        datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                        sucmsg=" " Style="font-size: 15px" />
                    <i></i>
                </div>
                <span class="rl">到</span>
                <div class="input-date" style="display: inline-block; float: left;">
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
                 <div style="display: inline-block;">
                    <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox>
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
    <th align="left" width="5%">用户名</th>
    <th align="left" width="5%">创建时间</th>
    <th align="left" width="5%">支付网关</th>
    <th align="left" width="10%">流水号</th>
    <th align="left" width="5%">充值金额</th>
    <th align="left" width="5%">充值状态</th>
    <th align="left" width="5%">完成时间</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td align="center">
      <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" style="vertical-align:middle;" Enabled='<%# Convert.ToByte(Eval("status")) == (byte) Agp2pEnums.BankTransactionStatusEnum.Acting %>' />
      <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" />
    </td>
    <td><%# Container.ItemIndex + pageSize * (page - 1) + 1 %></td>
    <td><a href="../users/user_edit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&id=<%#Eval("dt_users.id")%>"><%#Eval("dt_users.real_name") != null && Eval("dt_users.real_name") != "" ? Eval("dt_users.real_name") : Eval("dt_users.user_name")%></a></td>
    <td><%# Eval("create_time")%></td>
    <td><asp:Label ID="lb_pay_type" runat="server" Text='<%# Eval("pay_api") == null ? "" : Utils.GetAgp2pEnumDes((Agp2pEnums.PayApiTypeEnum)Convert.ToByte(Eval("pay_api")))%>'></asp:Label></td>
    <td><asp:Label ID="lb_order_no" runat="server" Text='<%# Eval("no_order")%>'></asp:Label></td>
    <td><%# Convert.ToDecimal(Eval("value")).ToString("c") %></td>
    <td><%# Utils.GetAgp2pEnumDes((Agp2pEnums.BankTransactionStatusEnum) Convert.ToByte(Eval("status")))%></td>
    <td><%# Eval("transact_time")%></td>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count >0 ? "<tr><td colspan=\"2\">&nbsp;</td><td style=\"color: red;\">合计</td><td colspan=\"3\"></td><td style=\"color: red;\">"+Convert.ToDecimal(value).ToString("c")+"</td><td colspan=\"2\">&nbsp;</td></tr>" : ""%>
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
