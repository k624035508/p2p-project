<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="project_recently_repay_list.aspx.cs" Inherits="Agp2p.Web.admin.transact.project_recently_repay_list" %>
<%@ Import namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>最近的还款计划列表</title>
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
  <span>项目列表</span>
</div>
<!--/导航栏-->

<!--工具栏-->
    <div class="toolbar-wrap">
        <div id="floatHead" class="toolbar">
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
                <!--还款时间选择，选择后刷新列表-->
                <div style="display: inline-block;" class="rl">还款日期：</div>
                <div class="rule-multi-radio" style="display: inline-block; margin-right:10px; float:left;">
                    <asp:RadioButtonList ID="rblProjectShowType" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="rblProjectShowType_OnSelectedIndexChanged">
                        <asp:ListItem Value="0">不限</asp:ListItem>
                        <asp:ListItem Value="1" Selected="True">当日</asp:ListItem>
                    </asp:RadioButtonList>
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
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th align="left" width="3%" style="padding-left: 1em">序号</th>
    <th align="left" width="5%">名称</th>
    <th align="left" width="5%">状态</th>
    <th align="left" width="5%">金额</th>
    <th align="left" width="5%">期限</th>
    <th align="left" width="5%">年化收益率</th>
    <th align="left" width="5%">放款类型</th>
    <th align="left" width="5%">融资情况</th>
    <th align="left" width="5%">放款情况</th>
    <th align="left" width="5%">操作</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td style="padding-left: 1em"><%# Container.ItemIndex + pageSize * (page - 1) + 1%></td>
    <td><%# Eval("title") %></td>
    <td><%# Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectStatusEnum)Eval("status"))%></td>
    <td><%# string.Format("{0:0.##}", Eval("financing_amount"))%></td>
    <td><%# Eval("repayment_term_span_count")%>
      <%# Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTermSpanEnum) Eval("repayment_term_span"))%></td>
    <td><%# Eval("profit_rate_year")%></td>
    <td><%# Utils.GetAgp2pEnumDes((Agp2pEnums.ProjectRepaymentTypeEnum)Eval("repayment_type"))%></td>
    <td><a href="project_investment_list.aspx?project_id=<%#Eval("id")%>"><%# calcProjectProgress((li_projects) Container.DataItem) %></a></td>
    <td><a href="project_repayment_list.aspx?project_id=<%#Eval("id")%>"><%# QueryRepaymentProgress((li_projects) Container.DataItem) %></a></td>
      <td>
          <asp:Button ID="btnRepayNow" runat="server" Text="马上还款" UseSubmitBehavior="False"
              Visible="<%# IsProjectCanRepayToday((li_projects)Container.DataItem) %>"
              CommandArgument='<%# Eval("id") %>' OnClick="btnRepayNow_OnClick" OnClientClick="return ExeNoCheckPostBack(this.name, '确认放款？');" />
      </td>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"10\">暂无记录</td></tr>" : ""%>
</table>
</FooterTemplate>
</asp:Repeater>
<!--/列表-->

<!--内容底部-->
<div class="line20"></div>
<div class="pagelist">
  <div class="l-btns">
      <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"/><span>条/页</span>
  </div>
  <div id="PageContent" runat="server" class="default"></div>
</div>
<!--/内容底部-->
</form>
</body>
</html>
