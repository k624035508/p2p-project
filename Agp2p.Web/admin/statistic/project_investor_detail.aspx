<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="project_investor_detail.aspx.cs" Inherits="Agp2p.Web.admin.statistic.project_investor_detail" %>

<%@ Import Namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>借款满标明细</title>
    <script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
    <script type="text/javascript" src="../js/layout.js"></script>
    <script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>

    <link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
    <link href="../../css/pagination.css" rel="stylesheet" type="text/css" />
    <style>
        tr.sum td {
            color: red;
        }

        td.money {
            text-align: right;
        }

        td.center {
            text-align: center;
        }
    </style>
</head>

<body class="mainbody">
    <form id="form1" runat="server">
        <!--导航栏-->
        <div class="location">
            <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
            <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
            <i class="arrow"></i>
            <span>借款满标明细</span>
        </div>
        <!--/导航栏-->

        <!--工具栏-->
        <div class="toolbar-wrap">
            <div id="floatHead" class="toolbar">
                <div class="l-list">
                    <ul class="icon-list">
                        <li>
                            <asp:LinkButton ID="btnExportExcel" runat="server" CssClass="quotes" OnClick="btnExportExcel_Click"><i></i><span>导出 Excel</span></asp:LinkButton></li>
                    </ul>
                </div>
                <div class="r-list">
                    <div class="rule-multi-radio" style="display: inline-block; margin-right: 10px; float: left;">
                        <asp:RadioButtonList ID="rblType" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="rblType_OnSelectedIndexChanged">
                            <asp:ListItem Value="0" Selected="True">明细</asp:ListItem>
                            <asp:ListItem Value="1">汇总</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div style="display: inline-block;" class="rl">按满标时间查询：</div>
                    <div class="input-date" style="display: inline-block; float: left;">
                        <asp:TextBox ID="txtStartTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                            datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                            sucmsg=" " Style="font-size: 15px" />
                        <i></i>
                    </div>
                    <span class="rl">到</span>
                    <div class="input-date" style="display: inline-block; float: left; margin-right: 10px;">
                        <asp:TextBox ID="txtEndTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                            datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                            sucmsg=" " Style="font-size: 15px" />
                        <i></i>
                    </div>
                    <div class="menu-list rl" style="display: inline-block; float: left; margin-left: 10px;">
                        <div class="rule-single-select">
                            <asp:DropDownList ID="ddlCategoryId" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlCategoryId_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                    </div>
                    <!--还款时间选择，选择后刷新列表-->
                    <div style="display: inline-block; padding-left: 2em;" class="rl">还款状态：</div>
                    <div class="rule-multi-radio" style="display: inline-block; float: left; margin-right: 10px;">
                        <asp:RadioButtonList ID="rblProjectStatus" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="rblProjectStatus_OnSelectedIndexChanged">
                            <asp:ListItem Value="0">不限</asp:ListItem>
                            <asp:ListItem Value="1">融资中</asp:ListItem>
                            <asp:ListItem Value="2">融资结束</asp:ListItem>
                            <asp:ListItem Value="3" Selected="True">已满标</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div style="display: inline-block;">
                        <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" OnTextChanged="txtPageNum_TextChanged" AutoPostBack="True" />
                        <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
                    </div>
                </div>
            </div>
        </div>
        <!--/工具栏-->

        <!--明细列表-->
        <asp:Repeater ID="rptList" runat="server">
            <HeaderTemplate>
                <table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
                    <tr>
                        <th width="5%">序号</th>
                        <th align="left">标题</th>
                        <th align="center">产品</th>
                        <th align="right">融资金额</th>
                        <th align="center">年利率</th>
                        <th align="left">期限</th>

                        <th align="left">发标时间</th>
                        <th align="left">满标时间</th>
                        <th align="left">到期时间</th>
                        <th align="left">投资者</th>
                        <%--<th align="left">会员号</th>--%>
                        <th align="left">会员组</th>
                        <th align="right">投资金额</th>

                        <th align="center">投资时间</th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr <%# (Eval("Project.Name") + "").EndsWith("计") ? "class='sum'" : ""%>>
                    <td style="text-align: center;"><%# Eval("Project.Index")%></td>
                    <td><%# Eval("Project.Name")%></td>
                    <td align="center"><%#Eval("Project.Category")%></td>
                    <td class="money"><%# Eval("Project.FinancingAmount") == null ? "" : Convert.ToDecimal(Eval("Project.FinancingAmount")).ToString("c") %></td>
                    <td class="center"><%# Eval("Project.ProfitRateYear")%></td>
                    <td><%# Eval("Project.Term")%></td>
                    <td><%# Eval("Project.PublishTime")%></td>
                    <td><%# Eval("Project.InvestCompleteTime")%></td>
                    <td><%# Eval("Project.RepayCompleteTime")%></td>

                    <td><%# Eval("InvestorRealName") != null && Eval("InvestorRealName") != "" ? Eval("InvestorRealName") : Eval("InvestorUserName")%></td>
                    <%--<td><%# Eval("InvestorUserName")%></td>--%>
                    <td><%# Eval("InvestorGroupName")%></td>
                    <td class="money"><%# Convert.ToDecimal(Eval("InvestValue")).ToString("c")%></td>
                    <td class="center"><%# Eval("InvestTime")%></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"11\">暂无记录</td></tr>" : ""%>
</table>
            </FooterTemplate>
        </asp:Repeater>
        <!--/列表-->

        <!--汇总列表-->
        <asp:Repeater ID="rptList_summary" runat="server" Visible="False">
            <HeaderTemplate>
                <table id="wallet" width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
                    <thead>
                        <tr>
                            <th align="center" width="5%" style="padding-left: 1em;">序号</th>
                            <th align="center">产品</th>
                            <th align="center">借款金额</th>
                            <th align="center">已投金额</th>
                            <th align="center">未投金额</th>
                        </tr>
                    </thead>
            </HeaderTemplate>
            <ItemTemplate>
                <tr <%# ((InvestorDetail)Container.DataItem).Project.Index == null ? "class='sum'" : ""%>>
                    <td style="text-align: center;"><%# Eval("Project.Index") %></td>
                    <td style="text-align: center"><%# Eval("Project.Category")%></td>
                    <td style="text-align: center"><%# Convert.ToDecimal(Eval("Project.FinancingAmount")).ToString("c")%></td>
                    <td style="text-align: center"><%# Convert.ToDecimal(Eval("InvestValue")).ToString("c")%></td>
                    <td style="text-align: center"><%# Convert.ToDecimal(Eval("UnInvestValue")).ToString("c")%></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                <%#rptList_summary.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"7\">暂无记录</td></tr>" : ""%>
</table>
            </FooterTemplate>
        </asp:Repeater>
        <!--/汇总列表-->

        <!--内容底部-->
        <div class="line20"></div>
        <div class="pagelist" id="div_page" runat="server">
            <div class="l-btns">
                <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" OnTextChanged="txtPageNum_TextChanged" AutoPostBack="True" /><span>条/页</span>
            </div>
            <div id="PageContent" runat="server" class="default"></div>
        </div>
        <!--/内容底部-->
    </form>
</body>
</html>
