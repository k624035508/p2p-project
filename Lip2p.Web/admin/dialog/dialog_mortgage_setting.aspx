<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="dialog_mortgage_setting.aspx.cs" Inherits="Lip2p.Web.admin.dialog.dialog_mortgage_setting" %>
<%@ Import Namespace="System.ComponentModel" %>
<%@ Import Namespace="Lip2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>设置标的物</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    //窗口API
    /*var api = frameElement.api, W = api.opener;
    api.button({
        name: '确定',
        focus: true,
        callback: function () {
            document.getElementById('#form1').submit();
            return false;
        }
    }, {
        name: '取消'
    });*/
</script>
</head>

<body>
<form id="form1" runat="server">
<div class="div-content">
  <dl>
    <dt>选择借贷人</dt>
    <dd>
      <div class="rule-single-select">
        <asp:DropDownList id="ddlLoaner" runat="server" onSelectedIndexChanged="ddlLoaner_SelectedIndexChanged" AutoPostBack="True"></asp:DropDownList>
      </div>
    </dd>
  </dl>
  <!--列表-->
<asp:Repeater ID="rptList" runat="server">
<HeaderTemplate>
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th width="10%">选择</th>
    <th align="left">名称</th>
    <th align="left" width="10%">类型</th>
    <th align="left" width="20%">估值</th>
    <th align="left" width="10%">状态</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td align="center">
      <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" style="vertical-align:middle;" Checked='<%# Eval("check")%>' Enabled='<%# Eval("enable") %>' />
      <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" />
    </td>
    <td><%# Eval("name")%></td>
    <td><%# Utils.GetLip2pEnumDes((Lip2pEnums.MortgageTypeEnum) Convert.ToByte(Eval("type")))%></td>
    <td><%# Eval("valuation")%></td>
    <td title="<%# QueryUsingProject(((MortgageItem) Container.DataItem).id)%>"><%# Utils.GetLip2pEnumDes((Lip2pEnums.MortgageStatusEnum)Convert.ToByte(Eval("status")))%></td>
  </tr>
</ItemTemplate>
<FooterTemplate>
  <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"6\">暂无记录</td></tr>" : ""%>
</table>
</FooterTemplate>
</asp:Repeater>
<!--/列表-->
</div>
    <asp:Button ID="btnSave" runat="server" Text="提交保存" CssClass="btn" OnClick="btnSave_Click" style="float: right;" />
</form>
</body>
</html>