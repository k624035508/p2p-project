<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="group_list.aspx.cs" Inherits="Agp2p.Web.admin.users.group_list" %>

<%@ Import Namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>会员组列表</title>
    <script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
    <script type="text/javascript" src="../js/layout.js"></script>
    <script type="text/javascript" src="../../scripts/tablesorter/jquery.tablesorter.min.js"></script>
    <link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
    <link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
    <link href="../../scripts/tablesorter/table-sorter-style.css" rel="stylesheet" type="text/css" />

    <style type="text/css">
        tr.sum td { color: red; }

        td.align-left { text-align: left !important; }
        td.align-center { text-align: center !important; }
        table.ltable td, table.ltable th { text-align: right; }
    </style>
    <script type="text/javascript">
        $(function () {
            $.tablesorter.addParser({
                id: 'rmb',
                is: function () { return false; },
                format: function (s) { return s.replace(/[¥,]/g, ''); },
                type: 'numeric'
            });
            $("table#group-list").tablesorter({
                headers: {
                    4: { sorter: 'rmb' },
                    5: { sorter: 'rmb' },
                    6: { sorter: 'rmb' },
                    7: { sorter: 'rmb' },
                    8: { sorter: 'rmb' },
                    9: { sorter: 'rmb' },
                    10: { sorter: 'rmb' },
                    11: { sorter: 'rmb' },
                    12: { sorter: 'rmb' },
                    13: { sorter: 'rmb' },
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
        <a href="../center.aspx" class="home"><i></i><span>首页</span></a> <i class="arrow">
        </i><span>会员组别</span>
    </div>
    <!--/导航栏-->
    <!--工具栏-->
    <div class="toolbar-wrap">
        <div id="floatHead" class="toolbar">
            <div class="l-list">
                <ul class="icon-list">
                    <li><a class="add" href="group_edit.aspx?action=<%=DTEnums.ActionEnum.Add %>"><i></i><span>新增</span></a></li>
                    <li><a class="all" href="javascript:;" onclick="checkAll(this);"><i></i><span>全选</span></a></li>
                    <li><asp:LinkButton ID="btnDelete" runat="server" CssClass="del" OnClientClick="return ExePostBack('btnDelete','本操作会删除本导航及下属子导航，是否继续？');"
                            OnClick="btnDelete_Click"><i></i><span>删除</span></asp:LinkButton></li>
                </ul>
            </div>
            <div class="r-list">
                <div class="menu-list" style="display: inline-block; float: left; margin-right: 8px; margin-left: 8px;">
                    <div class="rule-single-select">
                        <asp:DropDownList ID="ddlDepartments" AutoPostBack="True" runat="server" OnSelectedIndexChanged="ddlDepartments_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>
                    <div class="rule-single-select" style="display: none;">
                        <asp:DropDownList ID="ddlSubordinates" AutoPostBack="True" runat="server" OnSelectedIndexChanged="ddlSubordinates_SelectedIndexChanged"></asp:DropDownList>
                    </div>
                </div>
                <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" ontextchanged="txtKeywords_TextChanged" AutoPostBack="True"  />
                <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
            </div>
        </div>
    </div>
    <!--/工具栏-->
    <!--列表-->
    <asp:Repeater ID="rptList" runat="server">
        <HeaderTemplate>
            <table id="group-list" width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
                <thead>
                <tr>
                    <th style="text-align: center" width="4%">选择</th>
                    <th style="text-align: center" width="6%">组别名称</th>
                    <th style="text-align: left" width="8%">关联会员</th>
                    <th width="5%">组人数</th>
                    <th width="7%">组员可用余额</th>

                    <th width="7%">组员冻结金额</th>
                    <th width="7%">组员在投金额</th>
                    <th width="8%">组员已还本金</th>
                    <th width="8%">组员累计投资</th>
                    <th width="7%">组员待收益</th>

                    <th width="7%">组员已收益</th>
                    <th width="7%">组员累计收益</th>
                    <th width="8%">组员累计充值</th>
                    <th width="7%">组员累计提现</th>
                    <th style="text-align: center" width="7%">操作</th>
                </tr>
                </thead>
        </HeaderTemplate>
        <ItemTemplate>
            <tr <%# string.Equals(Eval("GroupName"), "总计") ? "class='sum'" : "" %>>
                <td class="align-center"><asp:CheckBox ID="chkId" CssClass="checkall" runat="server" /><asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" /></td>
                <td class="align-center">
                    <%# string.Equals("总计", Eval("GroupName")) ? "总计" : "<a href='group_edit.aspx?action=Edit&id=" + Eval("id") + "'>" + Eval("GroupName") + "</a>" %><%# Eval("DepartmentName")==null?"":"("+Eval("DepartmentName")+")" %>
                </td>
                <td class="align-left"><%# Eval("Servers") %></td>
                <td><%# Eval("UserCount") %></td>
                <td><%# Convert.ToDecimal(Eval("IdleMoney")).ToString("c") %></td>

                <td><%# Convert.ToDecimal(Eval("LockedMoney")).ToString("c") %></td>
                <td><%# Convert.ToDecimal(Eval("InvestingMoney")).ToString("c") %></td>
                <td><%# Convert.ToDecimal(Eval("UnpaidPrincipal")).ToString("c") %></td>
                <td><%# Convert.ToDecimal(Eval("TotalInvestment")).ToString("c") %></td>
                <td><%# Convert.ToDecimal(Eval("ProfitingMoney")).ToString("c") %></td>

                <td><%# Convert.ToDecimal(Eval("PaidMoney")).ToString("c") %></td>
                <td><%# Convert.ToDecimal(Eval("TotalProfit")).ToString("c") %></td>
                <td><%# Convert.ToDecimal(Eval("TotalCharge")).ToString("c") %></td>
                <td><%# Convert.ToDecimal(Eval("TotalWithdraw")).ToString("c") %></td>
                <td class="align-center">
                    <a href="batch_switch_group.aspx?act=include&group_id=<%#Eval("id")%>&type=group" <%#Convert.ToInt32(Eval("is_default")) == 1 ? "style='display: none'" : ""%>>添加</a>
                    <a href="batch_switch_group.aspx?act=exclude&group_id=<%#Eval("id")%>&type=group" <%#Convert.ToInt32(Eval("is_default")) == 1 ? "style='display: none'" : ""%>>移除</a>
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            <%#rptList.Items.Count == 0 ? "<tr><td class='align-center' colspan=\"16\">暂无记录</td></tr>" : ""%>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    <!--/列表-->
    </form>
</body>
</html>
