<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="mortgage_edit.aspx.cs" Inherits="Lip2p.Web.admin.loaner.mortgage_edit" ValidateRequest="false" %>
<%@ Import namespace="Lip2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>编辑标的物</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.js"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.queue.js"></script>
<script type="text/javascript" src="../../scripts/swfupload/swfupload.handlers.js"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    function initFields(scheme, data) {
        var schemeObj = JSON.parse(scheme || "{}");
        var obj = JSON.parse(data || "{}");

        var firstTime = true;
        for (var property in schemeObj) {
            if (schemeObj.hasOwnProperty(property)) {
                var container = firstTime ? $(".fieldTemplate") : $(".fieldTemplate:last").clone().insertBefore("#txtProperties");
                container.find(".fieldName").text(schemeObj[property]);
                container.find(".fieldValue").val(obj[property]).attr("data-fieldName", property);
                firstTime = false;
            }
        }
    }
    function saveFields() {
        var obj = {};
        var $fields = $(".fieldValue");
        $fields.each(function (index, elem) {
            obj[elem.getAttribute("data-fieldName")] = elem.value;
        });
        $("#txtProperties").val(JSON.stringify(obj));
        return true;
    }
    $(function () {
        //初始化表单验证
        $("#form1").initValidform();

        $(".upload-album").each(function () {
            $(this).InitSWFUpload({ btntext: "批量上传", btnwidth: 66, single: false, water: true,
                thumbnail: true, filesize: "<%=siteConfig.imgsize %>", sendurl: "../../tools/upload_ajax.ashx",
                flashurl: "../../scripts/swfupload/swfupload.swf", filetypes: "*.jpg;*.jpeg;*.png;*.gif;" });
        });
        initFields($("#txtScheme").val(), $("#txtProperties").val());
    });
</script>
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="mortgage_list.aspx?loaner_id=<%=owner_id %>" class="back"><i></i><span>返回列表页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <a href="loaner_list.aspx"><span>借贷人列表</span></a>
  <i class="arrow"></i>
  <a href="mortgage_list.aspx?loaner_id=<%=owner_id %>"><span>标的物列表</span></a>
  <i class="arrow"></i>
  <span>编辑标的物</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">编辑标的物</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
  <dl>
    <dt>会员</dt>
      <dd><asp:Literal ID="lblOwner" runat="server"/></dd>
  </dl>
  <dl>
    <dt>类别</dt>
    <dd>
      <div class="rule-multi-radio">
          <asp:RadioButtonList ID="rblMortgageType" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="rblMortgageType_OnSelectedIndexChanged"/>
      </div>
    </dd>
  </dl>
  <dl>
    <dt>名称</dt>
    <dd><asp:TextBox ID="txtName" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">*</span></dd>
  </dl>
  <asp:HiddenField runat="server" ID="txtScheme"/>
  <dl class="fieldTemplate">
    <dt class="fieldName"></dt>
    <dd><input class="fieldValue input normal" datatype="/^\S+$/"/><span class="Validform_checktip">*</span></dd>
  </dl>
  <asp:HiddenField runat="server" ID="txtProperties"/>
  <dl>
    <dt>抵押物估价</dt>
    <dd><asp:TextBox ID="txtValuation" runat="server" CssClass="input normal" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,2})?$/"></asp:TextBox><span class="Validform_checktip">*</span></dd>
  </dl>
  <dl>
    <dt>备注</dt>
    <dd>
        <asp:TextBox ID="txtRemark" runat="server" CssClass="input" TextMode="MultiLine"
            datatype="*0-255" sucmsg=" "></asp:TextBox>
    </dd>
    </dl>
  <dl>
    <dt>照片</dt>
    <dd>
      <div class="upload-box upload-album"></div>
      <div class="photo-list">
        <ul>
          <asp:Repeater ID="rptPictures" runat="server">
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
              <input type="hidden" name="hid_photo_name" value="splitter"/>
              <input type="hidden" name="hid_photo_remark" value="splitter"/>
  <dl>
    <dt>产权证</dt>
    <dd>
      <div class="upload-box upload-album"></div>
      <div class="photo-list">
        <ul>
          <asp:Repeater ID="rptPropertyCertificates" runat="server">
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
              <input type="hidden" name="hid_photo_name" value="splitter"/>
              <input type="hidden" name="hid_photo_remark" value="splitter"/>
</div>
<!--/内容-->

<!--工具栏-->
<div class="page-footer">
  <div class="btn-list">
    <asp:Button ID="btnSubmit" runat="server" Text="提交保存" CssClass="btn" onclick="btnSubmit_Click" OnClientClick="return saveFields()"/>
    <input name="btnReturn" type="button" value="返回上一页" class="btn yellow" onclick="javascript:history.back(-1);" />
  </div>
  <div class="clear"></div>
</div>
<!--/工具栏-->
</form>
</body>
</html>
