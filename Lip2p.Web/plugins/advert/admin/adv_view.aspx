<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="adv_view.aspx.cs" Inherits="Lip2p.Web.Plugin.Advert.admin.adv_view" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>调用广告</title>
<link type="text/css" rel="stylesheet" href="../../../<%=siteConfig.webmanagepath %>/skin/default/style.css" />
<script type="text/javascript" src="../../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="{config.webpath}scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript">
    $(function () {
        $("#btnCopy").bind("click", function () {
            window.clipboardData.setData("Text", $("#txtCopyUrl").val());
            $.dialog.alert("已将代码复制至剪切板，请将其贴粘到指定位置即可。");
        });
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
  <span>调用广告</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<!--内容-->
<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">调用广告</a></li>
      </ul>
    </div>
  </div>
</div>

<div class="tab-content">
  <dl>
    <dt>广告名称</dt>
    <dd><%=model.title %></dd>
  </dl>
  <dl>
    <dt>广告类型</dt>
    <dd><%= GetTypeName(model.type) %></dd>
  </dl>
  <dl>
    <dt>备注说明</dt>
    <dd><%=model.remark%></dd>
  </dl>
  <dl>
    <dt>显示数量</dt>
    <dd><%=model.view_num%></dd>
  </dl>
  <dl>
    <dt>显示大小</dt>
    <dd><%=model.view_width%>×<%=model.view_height%>px</dd>
  </dl>
  <dl>
    <dt>链接目标</dt>
    <dd><%=model.target%></dd>
  </dl>
  <dl>
    <dt>复制代码</dt>
    <dd>
      <textarea id="txtCopyUrl" class="input" style="vertical-align:middle;"><script type="text/javascript" src="<%=siteConfig.webpath%>plugins/advert/advert_js.ashx?id=<%=model.id%>"></script></textarea>
      &nbsp;<input id="btnCopy" type="button" value="复制代码" class="btn" style="vertical-align:middle;" />
    </dd>
  </dl>
  <dl>
    <dt>调用说明</dt>
    <dd>
      <div style="color:#060;">
            1、暂停、过期的广告不会在网站上显示；<br />
            2、请确保该插件目录下的player.swf（FLV插放器）、focus.swf（幻灯片）的存在，否则无法显示广告；<br />
            3、除广告类型为幻灯片、视频、代码外，如该广告位下存在多条广告时，均以&ltul&gt;&ltli&gt;...&lt/li&gt;&lt/ul&gt;包括进行输出；<br />
            4、广告以JS形式输出，可使用CSS进行控制其样式，前提是您熟悉HTML、DIV、CSS的知识；<br />
            5、了解上述，请将复制下列的代码粘贴于网站模板所对应的广告位中。
      </div>
    </dd>
  </dl>
</div>
<!--/内容-->
</form>
</body>
</html>
