<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="company_edit.aspx.cs" Inherits="Lip2p.Web.admin.loaner.company_edit" ValidateRequest="false" %>
<%@ Import namespace="Lip2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>编辑企业信息</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
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
  <a href="company_list.aspx" class="back"><i></i><span>返回列表页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <a href="company_list.aspx"><span>企业信息列表</span></a>
  <i class="arrow"></i>
  <span>编辑企业信息</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">编辑企业信息</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
  <dl>
    <dt>已关联借款人</dt>
    <dd><asp:TextBox ID="txtLoaners" runat="server" CssClass="input normal" datatype="/^\S+$/"/><span class="Validform_checktip">* 请输入人员账号/姓名，用英文逗号隔开</span></dd>
  </dl>
    <dl>
        <dt>公司名称</dt>
        <dd><asp:TextBox ID="txtCompanyName" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">* </span></dd>
    </dl>
    <dl>
        <dt>成立时间</dt>
        <dd>
            <div class="input-date">
                <asp:TextBox ID="txtSetupTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                    datatype="/^^\d{4}\-\d{1,2}\-\d{1,2}$/" errormsg="请选择正确的日期" sucmsg=" " />
                <i></i>
            </div>
        </dd>
    </dl>
    <dl>
        <dt>注册资本</dt>
        <dd><asp:TextBox ID="txtRegisteredCapital" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">* </span></dd>
    </dl>
    <dl>
        <dt>经营范围</dt>
        <dd><asp:TextBox ID="txtBusinessScope" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">* </span></dd>
    </dl>
    <dl>
        <dt>经营状态</dt>
        <dd><asp:TextBox ID="txtBusinessStatus" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">* </span></dd>
    </dl>
    <dl>
        <dt>涉讼情况</dt>
        <dd><asp:TextBox ID="txtBusinessLawsuit" runat="server" CssClass="input normal" datatype="/^.*$/"></asp:TextBox></dd>
    </dl>
    <dl>
        <dt>年收入</dt>
        <dd><asp:TextBox ID="txtIncomeYearly" runat="server" CssClass="input normal" datatype="/^.*$/"></asp:TextBox></dd>
    </dl>
    <dl>
        <dt>净资产</dt>
        <dd><asp:TextBox ID="txtNetAssets" runat="server" CssClass="input normal" datatype="/^.*$/"></asp:TextBox></dd>
    </dl>
    <dl>
        <dt>备注</dt>
        <dd><asp:TextBox ID="txtCompanyRemark" runat="server" CssClass="input normal" datatype="/^.*$/"></asp:TextBox></dd>
    </dl>
    <dl>
    <dt>企业照片</dt>
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
