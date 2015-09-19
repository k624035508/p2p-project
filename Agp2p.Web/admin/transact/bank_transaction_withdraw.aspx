<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="bank_transaction_withdraw.aspx.cs" Inherits="Agp2p.Web.admin.transact.bank_transaction_withdraw" ValidateRequest="false" %>
<%@ Import namespace="Agp2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>手工提现</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    function queryStandGuardFee() {
        var withdrawVal = $('#txtValue');
        var feeHint = $('#stand-guard-fee');
        var idleMoney = $('#idle-money');
        if (withdrawVal.val()) {
            if (Number(idleMoney.text()) < Number(withdrawVal.val())) {
                $.dialog.alert("提现金额超出余额");
                withdrawVal.val(idleMoney.text());
            }
            $.getJSON('../../tools/calc_stand_guard_fee.ashx?user_id=' + <%=userId %> + '&withdraw_value=' + withdrawVal.val(), function(data) {
                feeHint.text(data.fee);
                feeHint.show();
            });
        } else {
            feeHint.hide();
        }
    }

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
  <a href="bank_transaction_list.aspx?account_id=<%=accountId %>" class="back"><i></i><span>返回账户列表页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <a href="bank_users_charge_withdraw.aspx"><span>用户列表</span></a>
  <i class="arrow"></i>
  <a href="bank_account_list.aspx?user_id=<%=userId %>"><span><%= GetUserName() %> 银行账户列表</span></a>
  <i class="arrow"></i>
  <span>手工提现</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">手工提现</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
  <dl>
    <dt>会员</dt>
      <dd><asp:Literal ID="lblUser" runat="server"/></dd>
  </dl>
  <dl>
    <dt>可提现金额</dt>
      <dd><span id="idle-money"><asp:Literal ID="lblIdleMoney" runat="server" /></span></dd>
  </dl>
  <dl>
    <dt>开户行</dt>
      <dd><asp:Literal ID="lblBank" runat="server"/></dd>
  </dl>
  <dl>
    <dt>银行账户</dt>
      <dd><asp:Literal ID="lblAccount" runat="server"/></dd>
  </dl>
  <dl>
    <dt>提现金额</dt>
    <dd><asp:TextBox ID="txtValue" runat="server" CssClass="input normal" datatype="/^[0-9\.]+$/" onchange="queryStandGuardFee()"></asp:TextBox><span class="Validform_checktip">*</span>
    <span id="stand-guard-fee" style="display: none;">充值无需手续费</span></dd>
  </dl>
  <dl>
    <dt>备注</dt>
    <dd><asp:TextBox ID="txtRemark" runat="server" CssClass="input normal" datatype="/^.*$/"></asp:TextBox></dd>
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
