<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="loaner_edit.aspx.cs" Inherits="Agp2p.Web.admin.loaner.loaner_edit" ValidateRequest="false" %>
<%@ Import namespace="Agp2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>查看借贷人</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.js"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.queue.js"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.handlers.js"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<style>
    #mortgageList {
        width: 100%;
        height: 60vh;
    }
    .readonly { background-color: lightgray;}
</style>
<script type="text/javascript">
    $(function () {
        //初始化表单验证
        $("#form1").initValidform();
        $(".upload-album").each(function () {
            $(this).InitSWFUpload({ btntext: "批量上传", btnwidth: 66, single: false, water: true, thumbnail: true, filesize: "<%=siteConfig.imgsize %>", sendurl: "../../tools/upload_ajax.ashx", flashurl: "../../scripts/swfupload/swfupload.swf", filetypes: "*.jpg;*.jpeg;*.png;*.gif;" });
        });
        $("input[disabled]").addClass("readonly");
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
  <a href="loaner_list.aspx"><span>借贷人列表</span></a>
  <i class="arrow"></i>
  <span>查看借贷人</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">查看借贷人</a></li>
        <li><a href="javascript:;" onclick="tabs(this);">查看抵押物</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
  <dl>
    <dt>姓名</dt>
      <dd><asp:TextBox ID="txtName" runat="server" CssClass="input normal" Enabled="False"/><span class="Validform_checktip">用户姓名或账号</span></dd>
  </dl>
  <dl>
    <dt>性别</dt>
    <dd>
      <div class="rule-multi-radio">
        <asp:RadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" Enabled="False">
        <asp:ListItem Value="保密" Selected="True">保密</asp:ListItem>
        <asp:ListItem Value="男">男</asp:ListItem>
        <asp:ListItem Value="女">女</asp:ListItem>
        </asp:RadioButtonList>
      </div>
    </dd>
  </dl>
  <dl>
    <dt>电话</dt>
    <dd><asp:TextBox ID="txtTel" runat="server" CssClass="input normal" Enabled="False"></asp:TextBox><span class="Validform_checktip"> 要修改灰色字段请在会员管理页面修改</span></dd>
  </dl>
  <dl>
    <dt>身份证号码</dt>
      <dd><asp:TextBox ID="txtIdCardNumber" runat="server" CssClass="input normal" Enabled="False"/></dd>
  </dl>
  <dl>
    <dt>电子邮件</dt>
      <dd><asp:TextBox ID="txtEmail" runat="server" CssClass="input normal" Enabled="False"/></dd>
  </dl>
  <dl>
    <dt>年龄</dt>
      <dd><asp:TextBox ID="txtAge" runat="server" CssClass="input normal" datatype="/^\d+$/" /><span class="Validform_checktip">* </span></dd>
  </dl>
  <dl>
    <dt>籍贯</dt>
      <dd><asp:TextBox ID="txtCencus" runat="server" CssClass="input normal" datatype="/^\S+$/"/><span class="Validform_checktip">* </span></dd>
  </dl>
  <dl>
    <dt>工作</dt>
      <dd><asp:TextBox ID="txtJob" runat="server" CssClass="input normal" datatype="/^.+$/"/><span class="Validform_checktip">* </span></dd>
  </dl>
  <dl>
    <dt>工作地点</dt>
      <dd><asp:TextBox ID="txtWorkingAt" runat="server" CssClass="input normal" datatype="/^.+$/"/><span class="Validform_checktip">* </span></dd>
  </dl>
  <dl>
    <dt>工作单位</dt>
      <dd><asp:TextBox ID="txtWorkingUnit" runat="server" CssClass="input normal" datatype="/^.+$/"/><span class="Validform_checktip">* </span></dd>
  </dl>
  <dl>
    <dt>学历</dt>
      <dd><asp:TextBox ID="txtEducationalBackground" runat="server" CssClass="input normal" datatype="/^\S+$/"/><span class="Validform_checktip">* </span></dd>
  </dl>
  <dl>
    <dt>婚姻状态</dt>
    <dd>
      <div class="rule-multi-radio">
        <asp:RadioButtonList ID="rblMaritalStatus" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
        <asp:ListItem Value="1" Selected="True">未婚</asp:ListItem>
        <asp:ListItem Value="2">已婚</asp:ListItem>
        <asp:ListItem Value="3">离婚</asp:ListItem>
        <asp:ListItem Value="4">丧偶</asp:ListItem>
        </asp:RadioButtonList>
      </div>
    </dd>
  </dl>
  <dl>
    <dt>收入</dt>
      <dd><asp:TextBox ID="txtIncome" runat="server" CssClass="input normal" datatype="/^.+$/"/><span class="Validform_checktip">* </span></dd>
  </dl>
  <dl>
    <dt>身份证照片</dt>
    <dd>
      <div class="upload-box upload-album"></div>
      <div class="photo-list">
        <ul>
          <asp:Repeater ID="rptIdCardPics" runat="server">
            <ItemTemplate>
            <li>
              <input type="hidden" name="hid_photo_name" value="<%# Eval("id") %>|<%#Eval("original_path")%>|<%#Eval("thumb_path")%>" />
              <input type="hidden" name="hid_photo_remark" value="<%#Eval("remark")%>" />
              <div class="img-box" onclick="getBigPic(this);">
                <img src="<%#Eval("thumb_path")%>" bigsrc="<%#Eval("original_path")%>" />
                <span class="remark"><i><%#Eval("remark").ToString() == "" ? "暂无描述..." : Eval("remark").ToString()%></i></span>
              </div>
              <a href="javascript:;" onclick="setRemark(this);">描述</a>
              <a href="javascript:;" onclick="delImg(this);">删除</a>
            </li>
            </ItemTemplate>
          </asp:Repeater>
        </ul>
      </div>
    </dd>
  </dl>
</div>
<div class="tab-content" style="display: none;">
    <iframe id="mortgageList" src="mortgage_list.aspx?loaner_id=<%=id %>&readonly=true"></iframe>
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
