<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="guarantor_edit.aspx.cs" Inherits="Agp2p.Web.admin.loaner.guarantor_edit" ValidateRequest="false" %>
<%@ Import namespace="Agp2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>编辑担保机构</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.js"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.queue.js"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.handlers.js"></script>
<script type="text/javascript" charset="utf-8" src="../../editor/kindeditor-min.js"></script>
<script type="text/javascript" charset="utf-8" src="../../editor/lang/zh_CN.js"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    $(function () {
        //初始化表单验证
        $("#form1").initValidform();
        $(".upload-album").each(function () {
            $(this).InitSWFUpload({ btntext: "批量上传", btnwidth: 66, single: false, water: true, thumbnail: true, filesize: "<%=siteConfig.imgsize %>", sendurl: "../../tools/upload_ajax.ashx", flashurl: "../../scripts/swfupload/swfupload.swf", filetypes: "*.jpg;*.jpeg;*.png;*.gif;" });
        });
        //初始化编辑器
        KindEditor.create('.editor', {
            width: '98%',
            height: '400px',
            resizeType: 1,
            uploadJson: '../../tools/upload_ajax.ashx?action=EditorFile&IsWater=1',
            fileManagerJson: '../../tools/upload_ajax.ashx?action=ManagerFile',
            allowFileManager: true
        });
        KindEditor.create('.editor-mini', {
            width: '98%',
            height: '400px',
            resizeType: 1,
            uploadJson: '../../tools/upload_ajax.ashx?action=EditorFile&IsWater=1',
            items: [
				'fontname', 'fontsize', '|', 'forecolor', 'hilitecolor', 'bold', 'italic', 'underline',
				'removeformat', '|', 'justifyleft', 'justifycenter', 'justifyright', 'insertorderedlist',
				'insertunorderedlist', '|', 'emoticons', 'image', 'link']
        });
    });
</script>
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="guarantor_list.aspx" class="back"><i></i><span>返回列表页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <a href="guarantor_list.aspx"><span>担保机构列表</span></a>
  <i class="arrow"></i>
  <span>编辑担保机构</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">编辑担保机构</a></li>
        <li><a href="javascript:;" onclick="tabs(this);">股东信息</a></li>
        <li><a href="javascript:;" onclick="tabs(this);">银行授信额度情况</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
    <dl>
        <dt>担保机构名称</dt>
        <dd><asp:TextBox ID="txtGuarantorName" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">* </span></dd>
    </dl>
    <dl>
        <dt>注册号</dt>
        <dd><asp:TextBox ID="txtRegistNumber" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox></dd>
    </dl>
    <dl>
      <dt>类型</dt>
      <dd>
        <div class="rule-multi-radio">
          <asp:RadioButtonList ID="rblType" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
          <asp:ListItem Value="1" Selected="True">小额贷款公司</asp:ListItem>
          <asp:ListItem Value="2">担保公司</asp:ListItem>
          </asp:RadioButtonList>
        </div>
      </dd>
    </dl>
    <dl>
        <dt>法定代表人</dt>
        <dd><asp:TextBox ID="txtLegalPerson" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox></dd>
    </dl>
    <dl>
        <dt>注册资本</dt>
        <dd><asp:TextBox ID="txtRegisteredCapital" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox></dd>
    </dl>
    <dl>
        <dt>成立日期</dt>
        <dd>
            <div class="input-date">
                <asp:TextBox ID="txtSetupDate" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                    datatype="/^^\d{4}\-\d{1,2}\-\d{1,2}$/" errormsg="请选择正确的日期" sucmsg=" " />
                <i></i>
            </div>
        </dd>
    </dl>
    <dl>
        <dt>公司地址</dt>
        <dd><asp:TextBox ID="txtAddr" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox></dd>
    </dl>
    <dl>
        <dt>经营范围</dt>
        <dd><asp:TextBox ID="txtBusinessScope" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox></dd>
    </dl>
    <dl>
        <dt>描述</dt>
        <dd><asp:TextBox ID="txtDescription" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox></dd>
    </dl>
    <dl>
    <dt>担保机构照片</dt>
    <dd>
      <div class="upload-box upload-album"></div>
      <div class="photo-list">
        <ul>
          <asp:Repeater ID="rptPics" runat="server">
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
    <dl>
      <dt>额度管理-合作业务品种</dt>
      <dd><asp:TextBox ID="txtCMBusinessTypes" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox></dd>
    </dl>
    <dl>
      <dt>合作时间</dt>
      <dd><asp:TextBox ID="txtCMCooperationTime" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox></dd>
    </dl>
    <dl>
      <dt>合作总额度</dt>
      <dd><asp:TextBox ID="txtCMCooperationTotalDegree" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox></dd>
    </dl>
    <dl>
      <dt>已使用额度</dt>
      <dd><asp:TextBox ID="txtCMCooperationUsedDegree" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox></dd>
    </dl>
    <dl>
      <dt>额度余额</dt>
      <dd><asp:TextBox ID="txtCMCooperationRemainDegree" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox></dd>
    </dl>
</div>


<div class="tab-content" style="display: none;">
    <dl>
      <dt>股东信息</dt>
      <dd>
        <textarea id="txtShareholdersInfo" class="editor" style="visibility:hidden;" runat="server"></textarea>
      </dd>
    </dl>
</div>
<div class="tab-content" style="display: none;">
    <dl>
      <dt>银行授信额度情况</dt>
      <dd>
        <textarea id="txtCreditSituationInfo" class="editor" style="visibility:hidden;" runat="server"></textarea>
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
