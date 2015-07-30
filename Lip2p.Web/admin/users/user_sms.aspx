<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="user_sms.aspx.cs" Inherits="Lip2p.Web.admin.users.user_sms" ValidateRequest="false" %>
<%@ Import Namespace="Lip2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>短信通知</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
<script type="text/javascript" src="../js/jquery.skygqOneDblClick.1.1.js"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    $(function () {
        //初始化表单验证
        $("#form1").initValidform();

        var click_event = function (e) {

        };
        var dblclick_event = function (e) {
            var groupName='';
            var liLength = $("#div_group li.selected").length;
            //alert(liLength);
            for (var i = 0; i < liLength; i++) {
                var inputElement = $("#div_group li.selected a")[i];
                if (i == 0) {
                    groupName = inputElement.innerHTML;
                } else {
                    groupName = groupName + ',' + inputElement.innerHTML;
                }
            }
            //alert(groupName);
            var title = $(this).text();
           
            var mobiles = $("#txtMobileNumbers").val();
            if (mobiles == undefined) {
                mobiles = "";
            }

            $("#title").val(title);
            $("#groupNames").val(groupName);
            $("#mobiles").val(mobiles);
            $("#f1").submit();
        }
        $("#div_group").find("ul a").skygqOneDblClick({ oneclick: click_event, dblclick: dblclick_event });

        var action = '<%=Action%>';
        if (action == "cbkuser") {
            $("#div_group li").removeClass("selected");
            var liLength = $("#div_group li").length;
            var groupNames = '<%=groupNames%>';
            var garr = groupNames.split(",");
            for (var j = 0; j < garr.length; j++) {
                for (var i = 0; i < liLength; i++) {
                    if (garr[j] == $("#div_group li a")[i].innerHTML) {
                        $("#div_group li").eq([i]).addClass("selected");
                    }
                }
            } 
        }
    });

</script>
</head>
<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="user_list.aspx" class="back"><i></i><span>返回列表页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>会员管理</span>
  <i class="arrow"></i>
  <span>短信通知</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">批量短信通知</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
  <dl>
    <dt>输入类型</dt>
    <dd>
      <div class="rule-multi-radio">
        <asp:RadioButtonList ID="rblSmsType" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" onselectedindexchanged="rblSmsType_SelectedIndexChanged">
          <asp:ListItem Value="1">手动输入</asp:ListItem>
          <asp:ListItem Value="2">批量通知</asp:ListItem>
        </asp:RadioButtonList>
      </div>
    </dd>
  </dl>
  <dl id="div_group" runat="server" visible="false">
    <dt>会员组别</dt>
    <dd>
      <div style="line-height: 25px;color: red;">注：单击选择默认发送组内所有成员；双击默认发送选中所勾选成员</div>
      <div class="rule-multi-porp">
        <asp:CheckBoxList ID="cblGroupId" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow"></asp:CheckBoxList>
      </div>
    </dd>
  </dl>
  <dl id="div_mobiles" runat="server" visible="false">
    <dt>手机号码</dt>
    <dd>
      <asp:TextBox ID="txtMobileNumbers" runat="server" CssClass="input" style="padding:0;width:98%;height:150px;" datatype="/((^1\d{10})(,1\d{10})*$)+/"  nullmsg="请填写手机号码，多个手机号以“,”号分隔开！" errormsg="手机号必须是以1开头的11位数字，多个手机号以“,”号分隔开！" TextMode="MultiLine"></asp:TextBox>
      <div class="clear"></div>
      <span class="Validform_checktip">*多个手机号码以英文“,”逗号分隔开</span>
    </dd>
  </dl>
  <dl>
    <dt>短信内容</dt>
    <dd>
      <asp:TextBox ID="txtSmsContent" runat="server" CssClass="input" style="padding:0;width:98%;height:150px;" datatype="*" tip="长短信按62个字符每条短信扣取" nullmsg="请填写短信内容！" TextMode="MultiLine" onkeydown="checktxt(this, 'txtTip');" onkeyup="checktxt(this, 'txtTip');"></asp:TextBox>
      <div class="clear"></div>
      <span id="txtTip"></span>
      <span class="Validform_checktip">*长短信按62个字符每条短信扣取</span>
    </dd>
  </dl>
</div>
<!--/内容-->

<!--工具栏-->
<div class="page-footer">
  <div class="btn-list">
    <asp:Button ID="btnSubmit" runat="server" Text="提交发送" CssClass="btn" onclick="btnSubmit_Click" />
    <input name="btnReturn" type="button" value="返回上一页" class="btn yellow" onclick="javascript:history.back(-1);" />
  </div>
  <div class="clear"></div>
</div>
<!--/工具栏-->
</form>
    <form id="f1" name="f1" action="ChoiceUser.aspx" method="post">
        <input type="hidden" id="title" name="title" />
        <input type="hidden" id="groupNames" name="groupNames" />
        <input type="hidden" id="mobiles" name="mobiles" />
    </form>
</body>
</html>
