<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="view.aspx.cs" Inherits="Lip2p.Web.Plugin.LineService.admin.view" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>调用客服代码</title>
<link type="text/css" rel="stylesheet" href="../../../<%=siteConfig.webmanagepath %>/skin/default/style.css" />
<script type="text/javascript" src="../../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../../scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="../../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../../../<%=siteConfig.webmanagepath %>/js/layout.js"></script>
<script type="text/javascript">
    $(function () {
        //初始化表单验证
        $("#form1").initValidform(); ;
    });
    function CopyCode() {
        window.clipboardData.setData("Text", $("#txtCopyUrl").val());
        alert("已将代码复制至剪切板，请将其贴粘到对应的模板页即可。");
    };
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
  <span>客服管理</span>
  <i class="arrow"></i>
  <span>调用说明</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">调用客服代码</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
  <dl>
    <dt>客服电话</dt>
    <dd><asp:TextBox ID="txtPhone" runat="server" CssClass="input normal"></asp:TextBox></dd>
  </dl>
  <dl>
    <dt>初始状态</dt>
    <dd>
      <div class="rule-multi-radio">
        <asp:RadioButtonList ID="rblStatus" runat="server"  RepeatDirection="Horizontal" RepeatLayout="Flow">
            <asp:ListItem Selected="True" Value="0">最小化</asp:ListItem>
            <asp:ListItem Value="1">最大化</asp:ListItem>
        </asp:RadioButtonList>
      </div>
    </dd>
  </dl>
  <dl>
    <dt>选择皮肤</dt>
    <dd>
      <div class="rule-multi-radio">
        <asp:RadioButtonList ID="rblSkin" runat="server"  RepeatDirection="Horizontal" RepeatLayout="Flow"></asp:RadioButtonList>
      </div>
    </dd>
  </dl>
  <dl>
    <dt>复制代码</dt>
    <dd>
      <textarea id="txtCopyUrl" class="input" style="vertical-align:middle;"><script type="text/javascript" src="<%=siteConfig.webpath%>plugins/lineservice/online_js.ashx"></script></textarea>
      <input id="btnCopy" type="button" value="复制代码" class="btn" onclick="CopyCode();" />
    </dd>
  </dl>
  <dl>
    <dt>注意事项</dt>
    <dd>
      1、请确认页面头部已经引用了jquery类库；<br />
      2、请复制下列代码，放到你想显示的模板页的&lt;body&gt;与&lt;/body&gt;之间的任意位置；
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
