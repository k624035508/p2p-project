<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="offline_transact_timeline.aspx.cs" Inherits="Agp2p.Web.admin.statistic.offline_transact_timeline" %>

<%@ Import Namespace="Agp2p.Web.admin.statistic" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>平台收支明细</title>
    <script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="../../scripts/tablesorter/jquery.tablesorter.min.js"></script>
    <script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
    <script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
    <script type="text/javascript" src="../js/layout.js"></script>
    <link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
    <link href="../../css/pagination.css" rel="stylesheet" type="text/css" />
    <link href="../../scripts/tablesorter/table-sorter-style.css" rel="stylesheet" type="text/css" />
    <style>
        tr.sum td {
            color: red;
        }

        thead * {
            -webkit-box-sizing: border-box;
            -moz-box-sizing: border-box;
            box-sizing: border-box;
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
            <span>平台收支表</span>
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
                    <div style="display: inline-block;" class="rl">时间段：</div>
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
                    <span class="rl">
                        <asp:CheckBox runat="server" ID="cb_today" Checked="False" Text="当天" AutoPostBack="True" OnCheckedChanged="cb_today_OnCheckedChanged" /></span>

                    <div class="menu-list" style="display: inline-block; float: left; margin-right: 8px; margin-left: 8px;">
                        <div class="rule-single-select">
                            <asp:DropDownList ID="ddlRecordType" AutoPostBack="True" runat="server" OnSelectedIndexChanged="ddlRecordType_OnSelectedIndexChanged" />
                        </div>
                    </div>
                    <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" OnTextChanged="txtPageNum_TextChanged" AutoPostBack="True" />
                    <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
                </div>
            </div>
        </div>
        <!--/工具栏-->

        <!--明细列表-->
        <asp:Repeater ID="rptList" runat="server">
            <HeaderTemplate>
                <table id="wallet" width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
                    <thead>
                        <tr>
                            <th align="center" width="5%" style="padding-left: 1em;">序号</th>
                            <th align="center" width="12%">时间</th>
                            <th align="right" width="8%">收入</th>
                            <th align="right" width="8%">支出</th>
                            <th align="center" width="10%">操作类型</th>
                            <th align="center" width="14%">关联项目</th>
                            <th align="center" width="10%">关联人员</th>
                            <th align="center" width="16%">备注</th>
                        </tr>
                    </thead>
            </HeaderTemplate>
            <ItemTemplate>
                <tr <%# ((OfflineTransaction)Container.DataItem).index == null ? "class='sum'" : ""%>>
                    <td style="text-align: center;"><%# Eval("index") %></td>
                    <td style="text-align: center"><%# Eval("occurTime")%></td>
                    <td style="text-align: right"><%# Convert.ToDecimal(Eval("income")).ToString("c")%></td>
                    <td style="text-align: right"><%# Convert.ToDecimal(Eval("outcome")).ToString("c")%></td>
                    <td style="text-align: center"><%# Eval("type")%></td>
                    <td style="text-align: left"><%# Eval("project")%></td>
                    <td style="text-align: center"><%# Eval("user")%></td>
                    <td style="text-align: left"><%# Eval("remark")%></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"7\">暂无记录</td></tr>" : ""%>
</table>
            </FooterTemplate>
        </asp:Repeater>
        <!--/明细列表-->

        <!--汇总列表-->
        <asp:Repeater ID="rptList_summary" runat="server" Visible="False">
            <HeaderTemplate>
                <table id="wallet" width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
                    <thead>
                        <tr>
                            <th align="center" width="5%" style="padding-left: 1em;">序号</th>
                            <th align="center">操作类型</th>
                            <th align="center">收入</th>
                            <th align="center">支出</th>
                        </tr>
                    </thead>
            </HeaderTemplate>
            <ItemTemplate>
                <tr <%# ((OfflineTransaction)Container.DataItem).index == null ? "class='sum'" : ""%>>
                    <td style="text-align: center;"><%# Eval("index") %></td>
                    <td style="text-align: center"><%# Eval("type")%></td>
                    <td style="text-align: center"><%# Convert.ToDecimal(Eval("income")).ToString("c")%></td>
                    <td style="text-align: center"><%# Convert.ToDecimal(Eval("outcome")).ToString("c")%></td>
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
                <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);"
                    OnTextChanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox><span>条/页</span>
            </div>
            <div id="PageContent" runat="server" class="default"></div>
        </div>
        <!--/内容底部-->
    </form>
</body>
</html>
