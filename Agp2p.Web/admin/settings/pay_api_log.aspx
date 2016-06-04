<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="pay_api_log.aspx.cs" Inherits="Agp2p.Web.admin.repayment.pay_api_log" %>
<%@ Import Namespace="Agp2p.Common" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>资金日志管理</title>
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
            <span>资金日志</span>
        </div>
        <!--/导航栏-->
        <!--工具栏-->
        <div class="toolbar-wrap">
            <div id="floatHead" class="toolbar">
                <div class="l-list">
                <ul class="icon-list">
                    <li>
                        <asp:LinkButton ID="btnDelete" runat="server" CssClass="del" OnClientClick="return ExeNoCheckPostBack('btnDelete','删除15天前的资金托管日志，你确定吗?');"
                            OnClick="btnDelete_Click"><i></i><span>删除日志</span></asp:LinkButton>
                    </li>                    
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
                        <th width="4%">序号</th>
                        <th align="left" width="15%">请求编号</th>
                        <th align="left" width="8%">请求类型</th>
                        <th align="left" width="6%">请求状态</th>
                        <th align="left" width="10%">关联用户</th>
                        <th align="left" width="8%">关联项目</th>
                        <th align="left" width="8%">请求时间</th>
                        <th align="left" width="8%">响应时间</th>
                        <th align="left" width="6%">响应结果</th>
                        <th align="center" width="10%">响应备注</th>
                        <th width="5%">操作</th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td align="center"><%#Eval("RequestLog.Index") %></td>
                    <td align="left"><%#Eval("RequestLog.RequestId") %></td>
                    <td align="left"><%#Eval("RequestLog.Type")%></td>
                    <td align="left"><%#Eval("RequestLog.Status") %></td>
                    <td align="left"><%#Eval("RequestLog.UserName") %></td>
                    <td align="left"><%#Eval("RequestLog.ProjectTitle") %></td>
                    <td align="left"><%#Eval("RequestLog.RequestTime") %></td>
                    <td align="left"><%#Eval("ResponseTime") %></td>
                    <td align="left"><%#Eval("ResponseResult") %></td>
                    <td align="left"><%#Eval("ResponseRemark") %></td>
                    <td align="center">
                        <asp:LinkButton runat="server" ID="excBtn" OnClick="excBtn_OnClick" OnClientClick="return confirm('确定执行平台操作吗?');" CommandArgument='<%#Eval("RequestLog.RequestId")%>' Visible='<%#Eval("RequestLog.Status") != null && !Eval("RequestLog.Status").ToString().Equals("已完成")%>'>执行</asp:LinkButton>
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
