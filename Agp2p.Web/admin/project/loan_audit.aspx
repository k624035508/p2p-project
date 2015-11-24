<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="loan_audit.aspx.cs" Inherits="Agp2p.Web.admin.project.loan_audit" %>

<%@ Import Namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>借款审核</title>
    <script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="../../scripts/jquery/jquery.lazyload.min.js"></script>
    <script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
    <script type="text/javascript" src="../js/layout.js"></script>
    <link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
    <link href="../../css/pagination.css" rel="stylesheet" type="text/css" />
</head>
<script type="text/javascript">
    function auditTip(id, status, msg) {
        parent.dialog({
            title: '提示',
            content: msg,
            okValue: '确定',
            ok: function () {
                location.href = "project/loan_audit.aspx?channel_id=<%=this.ChannelId %>&action=" + status + "&id=" + id;
            },
            cancelValue: '取消',
            cancel: function () { }
        }).showModal();

        return false;
    }
</script>
<body class="mainbody">
    <form id="form1" runat="server">
        <!--导航栏-->
        <div class="location">
            <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
            <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
            <i class="arrow"></i>
            <span>借款审核</span>
        </div>
        <!--/导航栏-->
        <!--工具栏-->
        <div class="toolbar-wrap">
            <div id="floatHead" class="toolbar">
                <div class="l-list">
                    <ul class="icon-list">
                        <li><a class="all" href="javascript:;" onclick="checkAll(this);"><i></i><span>全选</span></a></li>
                        <li>
                            <asp:LinkButton ID="btnAudit" runat="server" CssClass="folder" OnClientClick="return ExePostBack('btnAudit','所选项目将通过审批，确定继续吗？');"
                                OnClick="btnAudit_OnClick"><i></i><span>审批通过</span></asp:LinkButton></li>
                    </ul>
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
                        <th width="3%">选择</th>
                        <th width="4%">序号</th>
                        <th align="left" width="15%">标题</th>
                        <th align="left" width="13%">借款人</th>
                        <th align="left" width="8%">产品</th>
                        <th align="left" width="10%">借款金额(元)</th>
                        <th align="left" width="8%">借款期限</th>
                        <th align="left" width="8%">年化利率</th>
                        <th align="left" width="8%">还款方式</th>
                        <th align="left" width="10%">申请时间</th>
                        <th width="6%">操作</th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td align="center">
                        <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" Style="vertical-align: middle;" />
                        <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" />
                    </td>
                    <td align="center"><%# Container.ItemIndex + PageSize * (PageIndex - 1) + 1 %></td>
                    <td><a href="loan_detail.aspx?channel_id=<%=this.ChannelId %>&id=<%#Eval("id")%>&status=<%#Eval("status")%>"><%#Eval("title")%></a></td>
                    <td><%#QueryLoaner(((li_projects) Container.DataItem).id)%></td>
                    <td><%#CategoryIdTitleMap[Convert.ToInt32(Eval("category_id"))]%></td>
                    <td><%#string.Format("{0:c}", Eval("financing_amount"))%></td>
                    <td><%#Eval("repayment_term_span_count")%> <%#Utils.GetAgp2pEnumDes((Agp2p.Common.Agp2pEnums.ProjectRepaymentTermSpanEnum)Utils.StrToInt(Eval("repayment_term_span").ToString(), 0))%></td>
                    <td><%#string.Format("{0:0.0}", Eval("profit_rate_year"))%></td>
                    <td><%#Utils.GetAgp2pEnumDes((Agp2p.Common.Agp2pEnums.ProjectRepaymentTypeEnum)Utils.StrToInt(Eval("repayment_type").ToString(), 0))%></td>
                    <td><%#string.Format("{0:g}",Eval("add_time"))%></td>
                    <td align="center"><a onclick="return auditTip('<%#Eval("id")%>','audit_success','所选借款将通过审批，确定继续吗？');" href="javascript:;">通过</a>
                        <a onclick="return auditTip('<%#Eval("id")%>','audit_fail','所选借款将审批不通过，确定继续吗？');" href="javascript:;">不通过</a></td>
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
