<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="project_repay_detail.aspx.cs" Inherits="Agp2p.Web.admin.statistic.project_repay_detail" %>

<%@ Import Namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>应兑付明细表</title>
    <script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
    <script type="text/javascript" src="../js/layout.js"></script>
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

        th.padding-right, td.padding-right {
            padding-right: 0.5em;
        }
    </style>
    <script type="text/javascript">
        function ShowAgreeContract(id, projectId) {
            $.ajax({
                type: "post",
                url: "/tools/submit_ajax.ashx?action=user_agree_contract_show",
                data: {
                    "id": id, "projectId": projectId
                },
                dataType: "json",
                beforeSend: function (XMLHttpRequest) {
                    //发送前动作
                },
                success: function (data, textStatus) {
                    if (data.status == 1) {
                        $.dialog({
                            title: "投资协议",
                            content: data.body,
                            padding: 20,
                            width: 800,
                            height: 600,
                        });
                    } else {
                        alert(data.msg);
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    alert("状态：" + textStatus + "；出错提示：" + errorThrown);
                },
                timeout: 20000
            });
        }
    </script>
</head>

<body class="mainbody">
    <form id="form1" runat="server">
        <!--导航栏-->
        <div class="location">
            <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
            <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
            <i class="arrow"></i>
            <span>应兑付明细表</span>
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
                    <div style="display: inline-block;" class="rl">按应付时间查询：</div>
                    <div style="display: inline-block; float: left;">
                        <asp:TextBox ID="txtYear" runat="server" CssClass="keyword" datatype="/^\d{4}$/" AutoPostBack="True" OnTextChanged="txtYear_OnTextChanged" Width="4em" />
                    </div>
                    <div style="display: inline-block;" class="rl">年</div>
                    <div style="display: inline-block; float: left;">
                        <asp:TextBox ID="txtMonth" runat="server" CssClass="keyword" datatype="/^\d{1,2}$/" AutoPostBack="True" OnTextChanged="txtMonth_OnTextChanged" Width="2em" />
                    </div>
                    <div style="display: inline-block;" class="rl">月</div>
                    <!--还款时间选择，选择后刷新列表-->
                    <div style="display: inline-block; padding-left: 2em;" class="rl">还款状态：</div>
                    <div class="rule-multi-radio" style="display: inline-block; float: left; margin-right: 10px;">
                        <asp:RadioButtonList ID="rblRepaymentTaskStatus" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="rblRepaymentTaskStatus_OnSelectedIndexChanged">
                            <asp:ListItem Value="0">不限</asp:ListItem>
                            <asp:ListItem Value="1" Selected="True">未付款</asp:ListItem>
                            <asp:ListItem Value="2">已付款</asp:ListItem>
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

        <!--列表-->
        <asp:Repeater ID="rptList" runat="server">
            <HeaderTemplate>
                <table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
                    <tr>
                        <th width="3%">序号</th>
                        <th align="left">标题</th>
                        <th align="left">债权/借款人</th>
                        <th align="center">产品</th>
                        <th align="right">借款金额</th>
                        <th align="center">年利率</th>
                        <th align="left">满标时间</th>
                        <th align="left">到期日</th>
                        <th align="left">期数</th>
                        <th align="left">应付日期</th>
                        <th align="left">实付日期</th>
                        <th align="left">收款人</th>
                        <th align="left">投资协议</th>
                        <%--<th align="right">投资金额</th>
    <th align="center">投资时间</th>--%>
                        <th align="right">兑付本金</th>
                        <th align="right">兑付利息</th>
                        <th align="right" class="padding-right">本息合计</th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr <%# (Eval("RepaymentTask.ProjectName") + "").EndsWith("合计") ? "class='sum'" : ""%>>
                    <td style="text-align: center;"><%# Eval("RepaymentTask.Index")%></td>
                    <td><%# Eval("RepaymentTask.ProjectName")%></td>
                    <td><%# Eval("RepaymentTask.CreditorName")%></td>
                    <td align="center"><%#Eval("RepaymentTask.Category")%></td>
                    <td class="money"><%# Eval("RepaymentTask.FinancingAmount") == null ? "" : Convert.ToDecimal(Eval("RepaymentTask.FinancingAmount")).ToString("c")%></td>
                    <td class="center"><%# Eval("RepaymentTask.ProfitRateYear")%></td>
                    <td><%# Eval("RepaymentTask.InvestCompleteTime")%></td>
                    <td><%# Eval("RepaymentTask.RepayCompleteTime")%></td>
                    <td><%# Eval("RepaymentTask.Term")%></td>
                    <td><%# Eval("RepaymentTask.ShouldRepayAt")%></td>
                    <td><%# Eval("RepaymentTask.RepayAt")%></td>
                    <td><%# Eval("InvestorRealName") != null && Eval("InvestorRealName") != "" ? Eval("InvestorRealName") : Eval("InvestorUserName")%></td>
                    <td><%# Eval("AgreeNo")%></td>
                    <%--<td class="money"><%# Eval("InvestValue") == null ? "" : Convert.ToDecimal(Eval("InvestValue")).ToString("c")%></td>
    <td class="center"><%# Eval("InvestTime")%></td>--%>
                    <td class="money"><%# Convert.ToDecimal(Eval("RepayPrincipal")).ToString("c")%></td>
                    <td class="money"><%# Convert.ToDecimal(Eval("RepayInterest")).ToString("c")%></td>
                    <td class="money padding-right"><%# Convert.ToDecimal(Eval("RepayTotal")).ToString("c") %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"16\">暂无记录</td></tr>" : ""%>
</table>
            </FooterTemplate>
        </asp:Repeater>
        <!--/列表-->

        <!--内容底部-->
        <div class="line20"></div>
        <div class="pagelist">
            <div class="l-btns">
                <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" OnTextChanged="txtPageNum_TextChanged" AutoPostBack="True" /><span>条/页</span>
            </div>
            <div id="PageContent" runat="server" class="default"></div>
        </div>
        <!--/内容底部-->
    </form>
</body>
</html>
