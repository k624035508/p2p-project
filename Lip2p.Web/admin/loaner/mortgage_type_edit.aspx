<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="mortgage_type_edit.aspx.cs" Inherits="Lip2p.Web.admin.loaner.mortgage_type_edit" ValidateRequest="false" %>
<%@ Import namespace="Lip2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>抵押物类型编辑</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<style>
input[type=button] {
    margin: 4.5px 0px;
    padding: 1px 6px;
}
.fieldSchemeContainer input.normal {
    margin: 2px 0px;
}
</style>
<script type="text/javascript">
    function saveFields() {
        try {
            var obj = {};
            var $ids = $(".txtFieldId");
            var $names = $(".txtFieldName");
            $ids.each(function (index, elem) {
                if (obj.hasOwnProperty($.trim(elem.value))) {
                    throw "字段标识不能重复";
                }
                obj[$.trim(elem.value)] = $.trim($names[index].value);
            });
            $("#txtScheme").val(JSON.stringify(obj));
        } catch (e) {
            alert(e);
            return false;
        }
        return true;
    }
    function cloneFieldScheme() {
        $(".fieldSchemeContainer:last").clone().insertBefore("#btnAppendField").find("input.normal").val("");

        $(".btnDeleteField").show().eq(0).hide();
    }
    function deleteFieldScheme(btn) {
        $(btn).closest(".fieldSchemeContainer").remove();
    }
    function initFields(data) {
        var obj = JSON.parse(data || "{}");
        var firstTime = true;
        for (var property in obj) {
            if (obj.hasOwnProperty(property)) {
                var container = firstTime ? $(".fieldSchemeContainer") : $(".fieldSchemeContainer:last").clone().insertBefore("#btnAppendField");
                container.find(".txtFieldId").val(property);
                container.find(".txtFieldName").val(obj[property]);
                firstTime = false;
            }
        }
    }

    $(function () {
        //初始化表单验证
        $("#form1").initValidform();

        initFields($("#txtScheme").val());
        $(".btnDeleteField").show().eq(0).hide();
    });
</script>
</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回列表页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <a href="loaner_list.aspx"><span>抵押物类型列表</span></a>
  <i class="arrow"></i>
  <span>抵押物类型编辑</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">编辑抵押物类型</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
  <dl>
    <dt>名称</dt>
    <dd><asp:TextBox ID="txtTypeName" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">*</span></dd>
  </dl>
  <dl>
      <dt>字段提示</dt>
      <dd>
          注：删除字段或更改“字段标识”可能会导致抵押物属性数据丢失，如要修改请咨询开发人员
          <asp:HiddenField id="txtScheme" runat="server"/>
      </dd>
  </dl>
  <dl class="fieldSchemeContainer">
      <dt>字段标识</dt><dd><input class="txtFieldId input normal" datatype="/^\S+$/"/></dd>
      <dt>字段名称</dt><dd><input class="txtFieldName input normal" datatype="/^\S+$/"/></dd>
      <dt>删除字段</dt><dd><input type="button" class="btnDeleteField" value="删除字段" onclick="deleteFieldScheme(this)"/></dd>
  </dl>
  <dl id="btnAppendField">
      <dt>添加字段</dt><dd><input type="button" value="添加字段" onclick="cloneFieldScheme()"/></dd>
      <br/>
      <span class="Validform_checktip"></span>
  </dl>
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
