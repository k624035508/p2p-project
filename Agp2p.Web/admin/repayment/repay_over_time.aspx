<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="repay_over_time.aspx.cs" Inherits="Agp2p.Web.admin.repayment.repay_over_time" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>逾期借款</title>
    <script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="../../scripts/jquery/jquery.lazyload.min.js"></script>
    <script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
    <script type="text/javascript" src="../js/layout.js"></script>
    <link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
    <link href="../../css/pagination.css" rel="stylesheet" type="text/css" />
</head>
<body class="mainbody">
    <form id="form1" runat="server">
        <!--导航栏-->
        <div class="location">
            <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
            <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
            <i class="arrow"></i>
            <span>逾期借款</span>
        </div>
        <!--/导航栏-->
        <!--工具栏-->
        <div class="toolbar-wrap">
            <div id="floatHead" class="toolbar">
                <div class="l-list">
                    <div class="rule-multi-radio" style="display: inline-block; float: left; margin-right: 10px;">
                        <asp:RadioButtonList ID="rblStatus" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True"
                             OnSelectedIndexChanged="rblStatus_OnSelectedIndexChanged">
                            <asp:ListItem Value="0" Selected="True">逾期未还</asp:ListItem>
                            <asp:ListItem Value="1">逾期已还</asp:ListItem>
                            <asp:ListItem Value="2">垫付借款</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                </div>
                <div class="r-list">
                    <div class="menu-list rl" style="display: inline-block;">
                        <div class="rule-single-select">
                            <asp:DropDownList ID="ddlCategoryId" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlCategoryId_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                    </div>
                    <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" OnTextChanged="txtPageNum_TextChanged" AutoPostBack="True" />
                    <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search">查询</asp:LinkButton>
                </div>
            </div>
        </div>
        <!--/工具栏-->

        <asp:Repeater ID="rptList1" runat="server">
            <HeaderTemplate>
                <table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
                    <tr>
                        <th width="4%">序号</th>
                        <th align="left" width="15%">标题</th>
                        <th align="left" width="10%">借款人</th>
                        <th align="left" width="8%">应还金额(元)</th>
                        <th align="left" width="8%">逾期罚金(元)</th>
                        <th align="left" width="6%">逾期期数</th>
                        <th align="left" width="6%">逾期天数</th>
                        <th align="left" width="8%">应还时间</th>
                        <th align="left" width="8%">实还时间</th>
                        <th align="left" width="6%">产品</th>
                        <th align="left" width="6%">年化利率(%)</th>
                        <th align="left" width="6%">还款方式</th>
                        <th width="5%">操作</th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td align="center"><%# Container.ItemIndex + PageSize * (PageIndex - 1) + 1 %></td>
                    <td><a href="../project/loan_detail.aspx?channel_id=<%=this.ChannelId %>&id=<%#Eval("ProjectID")%>&status=<%#Eval("ProjectStatus")%>&repay_status=<%#Eval("RepayStatus")%>"><%#Eval("ProjectTitle")%></a></td>
                    <td><%#Eval("Loaner")%></td>
                    <td><%#Eval("Amount")%></td>
                    <td><%#Eval("Forfeit")%></td>
                    <td><%#Eval("OverTimeTerm")%></td>
                    <td><%#Eval("OverDayCount")%></td>
                    <td><%#Eval("ShouldRepayTime")%></td>
                    <td><%#Eval("RepayTime")%></td>
                    <td><%#new Agp2p.BLL.article_category().GetTitle(Convert.ToInt32(Eval("Category")))%></td>
                    <td><%#Eval("ProfitRate")%></td>
                    <td><%#Eval("RepaymentType")%></td>
                    <td align="center">
                        <% if (rblStatus.SelectedValue == "0")
                           { %>
                        <asp:LinkButton ID="lbt_prepay" runat="server" Text="垫付" OnClientClick="return confirm('确定垫付吗?');" 
                            CommandArgument='<%#Eval("RepayId")%>' OnClick="lbt_prepay_OnClick" ></asp:LinkButton> 
                        &nbsp;
                        <asp:LinkButton ID="lbt_repay" runat="server" Text="还款" OnClientClick="return confirm('确定还款吗?');" 
                            CommandArgument='<%#Eval("RepayId")%>' OnClick="lbt_repay_OnClick"></asp:LinkButton>
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
