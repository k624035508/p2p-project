<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="bank_account_edit.aspx.cs" Inherits="Agp2p.Web.admin.transact.bank_account_edit" ValidateRequest="false" %>
<%@ Import namespace="Agp2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>编辑银行账户</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    $(function () {
        //初始化表单验证
        $("#form1").initValidform();
    });
</script>
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="bank_account_list.aspx" class="back"><i></i><span>返回列表页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <a href="bank_users_charge_withdraw.aspx"><span>用户列表</span></a>
  <i class="arrow"></i>
  <a href="bank_account_list.aspx"><span><%= GetUserName() %> 银行账户</span></a>
  <i class="arrow"></i>
  <span>编辑银行账户</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">编辑银行账户</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
  <dl>
    <dt>会员</dt>
    <dd><asp:Literal ID="lblUser" runat="server"></asp:Literal></dd>
  </dl>
  <dl>
    <dt>银行</dt>
      <dd><asp:TextBox ID="txtBank" runat="server" CssClass="input normal" datatype="/^\S{3,20}$/"/><span class="Validform_checktip">*</span></dd>
  </dl>
  <dl>
    <dt>开户行</dt>
      <dd><asp:TextBox ID="txtOpeningBank" runat="server" CssClass="input normal" datatype="/^\S{3,50}$/"/><span class="Validform_checktip">*</span></dd>
  </dl>
  <dl>
    <dt>银行账户</dt>
      <dd><asp:TextBox ID="txtAccount" runat="server" CssClass="input normal" datatype="/^\S{15,25}$/"/><span class="Validform_checktip">*</span></dd>
  </dl>
</div>
<!--/内容-->

<!--工具栏-->
<div class="page-footer">
  <div class="btn-list">
    <asp:Button ID="btnSubmit" runat="server" Text="提交保存" CssClass="btn" onclick="btnSubmit_Click" />
    <input name="btnReturn" type="button" value="返回上一页" class="btn yellow" onclick="javascript:history.back(-1);" />
  </div>
  <div class="clear"></div>
</div>
<!--/工具栏-->
</form>
</body>
</html>
