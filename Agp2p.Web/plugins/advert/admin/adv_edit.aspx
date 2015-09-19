<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="adv_edit.aspx.cs" Inherits="Agp2p.Web.Plugin.Advert.admin.adv_edit" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>编辑广告位</title>
<link type="text/css" rel="stylesheet" href="../../../<%=siteConfig.webmanagepath %>/skin/default/style.css" />
<script type="text/javascript" src="../../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../../scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="../../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../../<%=siteConfig.webmanagepath %>/js/layout.js"></script>
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
  <a href="index.aspx" class="back"><i></i><span>返回列表页</span></a>
  <a href="../../../<%=siteConfig.webmanagepath %>/center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>插件管理</span>
  <i class="arrow"></i>
  <span>广告管理</span>
  <i class="arrow"></i>
  <span>编辑广告位</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">编辑广告位</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
  <dl>
    <dt>广告位名称</dt>
    <dd><asp:TextBox ID="txtTitle" runat="server" CssClass="input normal" datatype="*2-100" sucmsg=" "></asp:TextBox></dd>
  </dl>
  <dl>
    <dt>广告类型</dt>
    <dd>
      <div class="rule-multi-radio">
          <asp:RadioButtonList ID="rblType" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
            <asp:ListItem Selected="True" Value="1">文字</asp:ListItem>
            <asp:ListItem Value="2">图片</asp:ListItem>
            <asp:ListItem Value="3">幻灯片</asp:ListItem>
            <asp:ListItem Value="4">动画</asp:ListItem>
            <asp:ListItem Value="5">FLV视频</asp:ListItem>
            <asp:ListItem Value="6">代码 </asp:ListItem>
           </asp:RadioButtonList>
       </div>
    </dd>
  </dl>
  <dl>
    <dt>备注说明</dt>
    <dd>
      <asp:TextBox ID="txtRemark" runat="server" maxlength="255" TextMode="MultiLine" CssClass="input"></asp:TextBox>
    </dd>
  </dl>
  <dl>
    <dt>显示数量</dt>
    <dd><asp:TextBox ID="txtViewNum" runat="server" CssClass="input small" datatype="n" sucmsg=" " /></dd>
  </dl>
  <dl>
    <dt>价格</dt>
    <dd>
      <asp:TextBox ID="txtPrice" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,2})?$/" sucmsg=" ">0</asp:TextBox> 元
    </dd>
  </dl>
  <dl>
    <dt>宽度</dt>
    <dd><asp:TextBox ID="txtViewWidth" runat="server" CssClass="input small" datatype="n" sucmsg=" " /> px</dd>
  </dl>
  <dl>
    <dt>高度</dt>
    <dd><asp:TextBox ID="txtViewHeight" runat="server" CssClass="input small" datatype="n" sucmsg=" " /> px</dd>
  </dl>
  <dl>
    <dt>链接目标</dt>
    <dd>
      <div class="rule-multi-radio">
        <asp:RadioButtonList ID="rblTarget" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
            <asp:ListItem Selected="True" Value="_blank">新窗口</asp:ListItem>
            <asp:ListItem Value="_self">原窗口</asp:ListItem>
        </asp:RadioButtonList>
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
