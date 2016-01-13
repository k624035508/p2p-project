<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="repay_manage.aspx.cs" Inherits="Agp2p.Web.admin.repayment.repay_manage" %>

<%@ Import Namespace="Agp2p.Common" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>应还借款</title>
    <script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="../../scripts/jquery/jquery.lazyload.min.js"></script>
    <script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
    <script type="text/javascript" src="../../scripts/swfupload/swfupload.handlers.js"></script>
    <script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
    <script type="text/javascript" src="../../scripts/tablesorter/jquery.tablesorter.min.js"></script>
    <script type="text/javascript" src="../js/layout.js"></script>
    <link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
    <link href="../../css/pagination.css" rel="stylesheet" type="text/css" />
    <link href="../../scripts/tablesorter/table-sorter-style.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript">
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
            $("table#repayTable").tablesorter({
                headers: {
                    3: { sorter: 'rmb' },
                    4: { sorter: 'rmb' },
                    6: { sorter: 'short-date' },
                    7: { sorter: 'short-date' },
                    8: { sorter: 'short-date' }
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
            <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
            <i class="arrow"></i>
            <span>应还借款</span>
        </div>
        <!--/导航栏-->
        <!--工具栏-->
        <div class="toolbar-wrap">
            <div id="floatHead" class="toolbar">
                <div class="l-list">
                    <div class="rule-multi-radio" style="display: inline-block; float: left; margin-right: 10px;">
                        <asp:RadioButtonList ID="rblStatus" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True"
                            OnSelectedIndexChanged="rblStatus_OnSelectedIndexChanged">
                            <asp:ListItem Value="0" Selected="True">待还款</asp:ListItem>
                            <asp:ListItem Value="1">已还款</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                </div>
                <div class="r-list">
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
                    <div class="menu-list rl" style="display: inline-block;">
                        <div class="rule-single-select">
                            <asp:DropDownList ID="ddlCategoryId" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlCategoryId_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                    </div>
                    <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" OnTextChanged="txtPageNum_TextChanged" AutoPostBack="True" />
                    <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
                </div>
            </div>
        </div>
        <!--/工具栏-->

        <asp:Repeater ID="rptList1" runat="server">
            <HeaderTemplate>
                <table id="repayTable" width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
                    <thead>
                        <tr>
                            <th width="4%">序号</th>
                            <th align="left" width="15%">标题</th>
                            <th align="left" width="13%">借款人</th>
                            <th align="left" width="8%">应还本金(元)</th>
                            <th align="left" width="8%">应还利息(元)</th>
                            <th align="left" width="6%">还款期数</th>
                            <th align="left" width="6%">放款日期</th>
                            <th align="left" width="6%">应还日期</th>
                            <th align="left" width="6%">实还日期</th>
                            <th align="left" width="6%">状态</th>
                            <th align="left" width="6%">产品</th>
                            <th align="left" width="6%">年化利率(%)</th>
                            <th align="left" width="6%">还款方式</th>
                            <th width="6%">操作</th>
                        </tr>
                    </thead>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td align="center"><%# Container.ItemIndex + PageSize * (PageIndex - 1) + 1 %></td>
                    <td><a href="../project/loan_detail.aspx?channel_id=<%=this.ChannelId %>&id=<%#Eval("ProjectID")%>&status=<%#Eval("ProjectStatus")%>&repay_status=<%#Eval("RepayStatus")%>"><%#Eval("ProjectTitle")%></a></td>
                    <td><%#Eval("Loaner")%></td>
                    <td><%#Convert.ToDecimal(Eval("Principal")).ToString("C")%></td>
                    <td><%#Convert.ToDecimal(Eval("Interest")).ToString("C")%></td>
                    <td><%#Eval("TimeTerm")%></td>
                    <td><%#Eval("MakeLoanTime")%></td>
                    <td><%#Eval("ShouldRepayTime")%></td>
                    <td><%#Eval("RepayTime")%></td>
                    <td><%#Utils.GetAgp2pEnumDes((Agp2pEnums.RepaymentStatusEnum) Convert.ToInt16(Eval("RepayStatus")))%></td>
                    <td><%#CategoryIdTitleMap[Convert.ToInt32(Eval("Category"))]%></td>
                    <td><%#string.Format("{0:0.0}", Eval("ProfitRate"))%></td>
                    <td><%#Eval("RepaymentType")%></td>
                    <td align="center">
                        <% if (rblStatus.SelectedValue == "0")
                            { %>
                        <asp:LinkButton ID="lbt_repay" runat="server" Text="还款" OnClientClick="return confirm('确定还款吗?');"
                            CommandArgument='<%#Eval("RepayId")%>' OnClick="lbt_repay_OnClick" Visible='<%#Convert.ToInt16(Eval("RepayStatus")) != (int)Agp2pEnums.RepaymentStatusEnum.OverTime%>'></asp:LinkButton>
                        <% } %>
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                <%#rptList1.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"11\">暂无记录</td></tr>" : ""%>
                </table>
            </FooterTemplate>
        </asp:Repeater>

        <!--内容底部-->
        <div class="line20"></div>
        <div class="pagelist">
            <div class="l-btns">
                <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" OnTextChanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox><span>条/页</span>
            </div>
            <div id="PageContent" runat="server" class="default"></div>
        </div>
        <!--/内容底部-->
    </form>
</body>
</html>
