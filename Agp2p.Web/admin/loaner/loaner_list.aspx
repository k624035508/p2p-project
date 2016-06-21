<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="loaner_list.aspx.cs" Inherits="Agp2p.Web.admin.loaner.loaner_list" %>

<%@ Import Namespace="Agp2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>借贷人列表</title>
    <script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
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
            <span>借贷人列表</span>
        </div>
        <!--/导航栏-->

        <!--工具栏-->
        <div class="toolbar-wrap">
            <div id="floatHead" class="toolbar">
                <div class="l-list">
                    <ul class="icon-list">
                        <li><a class="add" href="loaner_edit.aspx?action=<%=DTEnums.ActionEnum.Add %>"><i></i><span>新增</span></a></li>
                        <li><a class="all" href="javascript:;" onclick="checkAll(this);"><i></i><span>全选</span></a></li>
                        <li>
                            <asp:LinkButton ID="btnDelete" runat="server" CssClass="del" OnClientClick="return ExePostBack('btnDelete');" OnClick="btnDelete_Click"><i></i><span>删除</span></asp:LinkButton></li>
                    </ul>
                </div>
                <div class="r-list">
                    <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" OnTextChanged="txtPageNum_TextChanged" AutoPostBack="True" />
                    <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
                </div>
            </div>
        </div>
        <!--/工具栏-->

        <!--列表-->
        <asp:Repeater ID="rptList" runat="server">
            <HeaderTemplate>
                <table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
                    <tr>
                        <th width="5%">选择</th>
                        <th align="left" width="10%">姓名</th>
                        <th align="left" width="8%">状态</th>
                        <th align="left" width="5%">性别</th>
                        <th align="left" width="10%">电话</th>
                        <th align="left" width="5%">年龄</th>
                        <th align="left" width="5%">户籍</th>
                        <th align="left" width="5%">学历</th>
                        <th align="left" width="5%">婚姻状况</th>
                        <th align="left" width="10%">身份证号码</th>
                        <th align="left" width="10%">收入</th>
                        <th align="left" width="5%">操作</th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td align="center">
                        <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" Style="vertical-align: middle;" />
                        <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" />
                    </td>
                    <td><a href="loaner_edit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&id=<%#Eval("id")%>"><%# Eval("dt_users.real_name") %></a></td>
                    <td><%# Utils.GetAgp2pEnumDes((Agp2pEnums.LoanerStatusEnum)Convert.ToInt16(Eval("status")))%></td>
                    <td><%# Eval("dt_users.sex")%></td>
                    <td><%# string.IsNullOrWhiteSpace((string) Eval("dt_users.mobile")) ? "" : string.Format("{0:### #### ####}", long.Parse(Eval("dt_users.mobile").ToString()))%></td>
                    <td><%# Eval("age")%></td>
                    <td><%# Eval("dt_users.area")%></td>
                    <td><%# Eval("educational_background")%></td>
                    <td><%# Utils.GetAgp2pEnumDes((Agp2pEnums.MaritalStatusEnum) Convert.ToByte(Eval("marital_status")))%></td>
                    <td><%# Eval("dt_users.id_card_number")%></td>
                    <td><%# Eval("income")%></td>
                    <td><a href="mortgage_list.aspx?loaner_id=<%#Eval("id")%>">管理标的物（<%# Eval("li_mortgages.Count")%>）</a>
                        <a href="loaner_edit.aspx?action=<%#DTEnums.ActionEnum.Audit%>&id=<%#Eval("id")%>">审核</a>
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
                <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" OnTextChanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox><span>条/页</span>
            </div>
            <div id="PageContent" runat="server" class="default"></div>
        </div>
        <!--/内容底部-->
    </form>
    <script>
        $(function () {
            $('a[href=""]').hide();
        });
    </script>
</body>
</html>
