<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="user_group_access_key_settings.aspx.cs" Inherits="Lip2p.Web.admin.users.user_group_access_key_settings" ValidateRequest="false" %>
<%@ Import namespace="Lip2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>用户组访问权限设置</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.js"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.queue.js"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.handlers.js"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    $(function () {
        //初始化表单验证
        $("#form1").initValidform();
        $(".upload-album").each(function () {
            $(this).InitSWFUpload({ btntext: "批量上传", btnwidth: 66, single: false, water: true, thumbnail: true, filesize: "<%=siteConfig.imgsize %>", sendurl: "../../tools/upload_ajax.ashx", flashurl: "../../scripts/swfupload/swfupload.swf", filetypes: "*.jpg;*.jpeg;*.png;*.gif;" });
        });
    });
</script>
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="loaner_list.aspx" class="back"><i></i><span>返回列表页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>用户组访问权限设置</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">用户组访问权限设置</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
    <dl>
        <dt>部门名称：</dt>
        <dd>
            <div class="rule-single-select">
                <asp:DropDownList ID="ddlDepartments" runat="server" OnSelectedIndexChanged="ddlDepartments_OnSelectedIndexChanged" AutoPostBack="True"></asp:DropDownList>
            </div>
        </dd>
    </dl>
  <dl>
      <dt>管理员：</dt>
      <dd>
          <div class="rule-single-select">
              <asp:DropDownList ID="ddlManager" runat="server" OnSelectedIndexChanged="ddlManager_OnSelectedIndexChanged" AutoPostBack="True"  datatype="*" errormsg="请选择管理员" sucmsg=" "></asp:DropDownList>
          </div>
      </dd>
  </dl>
  <dl>
    <dt>可访问会员组：</dt>
    <dd>
      <div class="rule-multi-porp">
          <asp:CheckBoxList ID="cblUserGroup" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow"></asp:CheckBoxList>
      </div>
    </dd>
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
