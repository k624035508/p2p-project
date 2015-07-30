<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="user_list.aspx.cs" Inherits="Lip2p.Web.admin.users.user_list" %>
<%@ Import Namespace="Lip2p.Common" %>
<%@ Import Namespace="Lip2p.Linq2SQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>用户管理</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/tablesorter/jquery.tablesorter.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link href="../../scripts/tablesorter/table-sorter-style.css" rel="stylesheet" type="text/css" />

<script type="text/javascript">
    $(function () {
        $.tablesorter.addParser({
            id: 'rmb',
            is: function () { return false; },
            format: function (s) { return s.replace(/[¥,]/g, ''); },
            type: 'numeric'
        });          
        $("table#user").tablesorter({
            headers: {                 
                6: { sorter: 'rmb' },
                7: { sorter: 'rmb' },
                8: { sorter: 'rmb' },
            }
        });
    });

    //发送短信
    function PostSMS(mobile) {
        var mobiles = "";
        if (arguments.length == 1) { //如果有传入值
            mobiles = mobile;
        } else {
            lenNum = $(".checkall input:checked").length;
            $(".checkall input:checked").each(function (i) {
                if ($(this).parent().siblings('input[name="hidMobile"]').val() != "") {
                    mobiles += $(this).parent().siblings('input[name="hidMobile"]').val();
                    if (i < lenNum - 1) {
                        mobiles += ',';
                    }
                }
            });
        }
        if (mobiles == "") {
            $.dialog.alert('对不起，手机号码不能为空！');
            return false;
        }
        var dialog = $.dialog({
            title: '发送手机短信',
            content: '<textarea id="txtSmsContent" name="txtSmsContent" rows="2" cols="20" class="input"></textarea>',
            min: false,
            max: false,
            lock: true,
            ok: function () {
                var remark = $("#txtSmsContent", parent.document).val();
                if (remark == "") {
                    $.dialog.alert('对不起，请输入手机短信内容！', function () { }, dialog);
                    return false;
                }
                var postData = { "mobiles": mobiles, "content": remark };
                //发送AJAX请求
                $.ajax({
                    type: "post",
                    url: "../../tools/admin_ajax.ashx?action=sms_message_post",
                    data: postData,
                    dataType: "json",
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        $.dialog.alert('尝试发送失败，错误信息：' + errorThrown, function () { }, dialog);
                    },
                    success: function (data, textStatus) {
                        if (data.status == 1) {
                            dialog.close();
                            $.dialog.tips(data.msg, 2, '32X32/succ.png', function () { location.reload(); }); //刷新页面
                        } else {
                            $.dialog.alert('错误提示：' + data.msg, function () { }, dialog);
                        }
                    }
                });
                return false;
            },
            cancel: true
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
  <span>会员管理</span>
  <i class="arrow"></i>
  <span>用户列表</span>
</div>
<!--/导航栏-->

<!--工具栏-->
<div class="toolbar-wrap">
  <div id="floatHead" class="toolbar">
    <div class="l-list">
      <ul class="icon-list">
        <li><a class="add" href="user_edit.aspx?action=<%=DTEnums.ActionEnum.Add %>"><i></i><span>新增</span></a></li>
        <li><a class="list" href="javascript:;" onclick="PostSMS();"><i></i><span>短信</span></a></li>
        <li><a class="all" href="javascript:;" onclick="checkAll(this);"><i></i><span>全选</span></a></li>
        <li><asp:LinkButton ID="btnDelete" runat="server" CssClass="del" OnClientClick="return ExePostBack('btnDelete');" onclick="btnDelete_Click"><i></i><span>删除</span></asp:LinkButton></li>
        <li><asp:LinkButton ID="btnExportExcel" runat="server" CssClass="quotes" OnClick="btnExportExcel_Click"><i></i><span>导出 Excel</span></asp:LinkButton></li>
      </ul>
    </div>
    <div class="r-list">
        <div>
            <div style="display: inline-block;">注册时间：</div>
            <div class="input-date" style="display: inline-block">
                <asp:TextBox ID="txtStartTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                    datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                    sucmsg=" " Style="font-size: 15px" />
                <i>日期</i>
            </div>
            <span>到</span>
            <div class="input-date" style="display: inline-block">
                <asp:TextBox ID="txtEndTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                    datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                    sucmsg=" " Style="font-size: 15px" />
                <i>日期</i>
            </div>
            <div class="menu-list" style="display:inline-block;">
                <div class="rule-single-select">
                    <asp:DropDownList ID="ddlGroupId" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlGroupId_SelectedIndexChanged"></asp:DropDownList>
                </div>
            </div>
            <div style="display: inline-block; min-height:30px; vertical-align:middle;">
                <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True" />
                <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" OnClick="btnSearch_Click">查询</asp:LinkButton>
            </div>
        </div>
    </div>
  </div>
</div>
<!--/工具栏-->

<!--列表-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table id="user" width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
 <thead>
  <tr>
    <th width="5%">选择</th>
    <th align="left">用户名</th>
    <th align="left" width="8%">会员组</th>
    <th width="8%">手机</th>
    <th width="12%">身份证号</th>
    <th align="right" width="8%">邮箱</th>
    <th align="right" width="10%">当天投资金额</th>    
    <th align="right" width="10%">在投金额</th>
    <th align="right" width="10%">余额</th>
    <th width="8%">状态</th>
  </tr>
</thead>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td align="center">
      <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" style="vertical-align:middle;" />
      <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" />
      <input name="hidMobile" type="hidden" value="<%#Eval("mobile")%>" />
    </td>
<%--<td width="64">
      <a href="user_edit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&id=<%#Eval("id")%>">
        <%# !string.IsNullOrWhiteSpace((string) Eval("avatar")) ? "<img width=\"64\" height=\"64\" src=\"" + Eval("avatar") + "\" />" : "<b class=\"user-avatar\"></b>"%>
      </a>
    </td>--%>
    <td>
      <div class="user-box">
        <h4><b><a href="user_edit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&id=<%#Eval("id")%>"><%#Eval("user_name")%></a></b> (姓名：<%#Eval("real_name")%>)</h4>
        <i>注册时间：<%#string.Format("{0:yy/MM/dd HH:mm}",Eval("reg_time"))%></i>
        <span>
      <%--<a class="amount" href="amount_log.aspx?keywords=<%#Eval("user_name")%>" title="消费记录">余额</a>--%>
          <a class="point" href="point_log.aspx?keywords=<%#Eval("user_name")%>" title="积分记录">积分</a>
          <a class="msg" href="message_list.aspx?keywords=<%#Eval("user_name")%>" title="消息记录">短消息</a>
          <%# !string.IsNullOrWhiteSpace((string) Eval("mobile")) ? "<a class=\"sms\" href=\"javascript:;\" onclick=\"PostSMS('" + Eval("mobile") + "');\" title=\"发送手机短信通知\">短信通知</a>" : ""%>
        </span>
      </div>
    </td>
    <td align="left"><%#new Lip2p.BLL.user_groups().GetTitle(Convert.ToInt32(Eval("group_id")))%></td>
    <td align="center"><%#Eval("mobile")%></td>
    <td align="center"><%#Eval("id_card_number")%></td>
    <td align="right"><%#Eval("email")%></td>
    <td align="right"><%# QueryInvestmentToday((dt_users)Container.DataItem).ToString("c") %></td>
    <td align="right"><%# Convert.ToDecimal(Eval("li_wallets.investing_money")).ToString("c") %></td>
    <td align="right"><%# Convert.ToDecimal(Eval("li_wallets.idle_money")).ToString("c")%></td>
<%--    <td align="center"><%#Eval("point")%></td>--%>
    <td align="center"><%#GetUserStatus(Convert.ToInt32(Eval("status")))%></td>
<%--    <td align="center"><a href="user_edit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&id=<%#Eval("id")%>">修改</a></td>--%>
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
    <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox><span>条/页</span>
  </div>
  <div id="PageContent" runat="server" class="default"></div>
</div>
<!--/内容底部-->
</form>
</body>
</html>
