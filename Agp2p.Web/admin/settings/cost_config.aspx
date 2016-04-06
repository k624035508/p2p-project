<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cost_config.aspx.cs" Inherits="Agp2p.Web.admin.settings.cost_config" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>费用设置</title>
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
        <div class="content-tab-wrap">
            <div id="floatHead" class="content-tab">
                <div class="content-tab-ul-wrap">
                    <ul>
                        <li><a href="javascript:;" onclick="tabs(this);" class="selected">平台费用设置</a></li>
                    </ul>
                </div>
            </div>
        </div>
        <div class="tab-content">
            <dl>
                <dt>平台服务费</dt>
                <dd>
                    <asp:TextBox ID="txt_loan_fee_rate" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">% * 借款本金</span></dd>
            </dl>
            <dl>
                <dt>平台服务费(票据)</dt>
                <dd>
                    <asp:TextBox ID="txt_loan_fee_rate_bank" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">% * 借款本金</span></dd>
            </dl>
            <dl>
                <dt>风险保证金</dt>
                <dd>
                    <asp:TextBox ID="txt_bond_fee_rate" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">% * 借款本金</span></dd>
            </dl>
            <dl>
                <dt>风险保证金(票据)</dt>
                <dd>
                    <asp:TextBox ID="txt_bond_fee_rate_bank" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">% * 借款本金</span></dd>
            </dl>
            <dl>
                <dt>提前还款罚息</dt>
                <dd>
                    <asp:TextBox ID="txt_earlier_pay" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">% * 借款本金</span></dd>
            </dl>
            <dl>
                <dt>逾期还款罚息</dt>
                <dd>
                    <asp:TextBox ID="txt_overtime_pay" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">% * 借款利息</span></dd>
            </dl>
            <dl>
                <dt>逾期管理费(30天以下)</dt>
                <dd>
                    <asp:TextBox ID="txt_overtime_cost" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">‰ * 借款本金 * 逾期天数</span></dd>
            </dl>
            <dl>
                <dt>逾期管理费(30天以上)</dt>
                <dd>
                    <asp:TextBox ID="txt_overtime_cost2" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">‰ * 借款本金 * 逾期天数</span></dd>
            </dl>
            <dl>
                <dt>逾期管理费(票据)</dt>
                <dd>
                    <asp:TextBox ID="txt_overtime_cost_bank" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">‰ * 借款本金 * 逾期天数</span></dd>
            </dl>
            <dl>
                <dt>提现费</dt>
                <dd>
                    <asp:TextBox ID="txt_withdraw" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">% * 提现金额</span></dd>
            </dl>
            <dl>
                <dt>定期项目转让管理费</dt>
                <dd>
                    <asp:TextBox ID="txt_static_withdraw" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">% * 转让金额</span></dd>
            </dl>
            <dl>
                <dt>最低充值</dt>
                <dd>
                    <asp:TextBox ID="txt_recharge_lowest" runat="server" CssClass="input small" datatype="n" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">元</span></dd>
            </dl>
            <dl>
                <dt>网银充值费</dt>
                <dd>
                    <asp:TextBox ID="txt_recharge_fee_rate" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">‰</span></dd>
            </dl>
            <dl>
                <dt>快捷充值</dt>
                <dd>
                    <asp:TextBox ID="txt_recharge_fee_rate_quick" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,4})?$/" sucmsg=" "></asp:TextBox>
                    <span class="Validform_checktip">‰</span></dd>
            </dl>
        </div>
        <div class="page-footer">
            <div class="btn-list">
                <asp:Button ID="btnSave" runat="server" Text="提交保存" CssClass="btn" OnClick="btnSave_OnClick" />
                <input name="btnReturn" type="button" value="返回上一页" class="btn yellow" onclick="javascript:history.back(-1);" />
            </div>
            <div class="clear">
            </div>
        </div>
    </form>
</body>
</html>
