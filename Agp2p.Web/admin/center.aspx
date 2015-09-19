<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="center.aspx.cs" Inherits="Agp2p.Web.admin.center" %>

<%@ Import Namespace="Agp2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>管理首页</title>
    <link href="skin/default/style.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="../scripts/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="js/layout.js"></script>
</head>

<body class="mainbody">
    <form id="form1" runat="server">
        <!--导航栏-->
        <div class="location">
            <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
            <a class="home"><i></i><span>首页</span></a>
            <i class="arrow"></i>
            <span>管理中心</span>
        </div>
        <!--/导航栏-->

        <!--内容-->
        <div class="line10"></div>
        <div class="nlist-1">
            <ul>
                <li>本次登录IP：<asp:Literal ID="litIP" runat="server" Text="-" /></li>
                <li>上次登录IP：<asp:Literal ID="litBackIP" runat="server" Text="-" /></li>
                <li>上次登录时间：<asp:Literal ID="litBackTime" runat="server" Text="-" /></li>
            </ul>
        </div>
        <div class="line10"></div>

        <%if (admin_info != null && admin_info.user_name == "admin")
          { %>
        <div class="nlist-2">
            <h3><i></i>站点信息</h3>
            <ul>
                <li>站点名称：<%=siteConfig.webname %></li>
                <li>公司名称：<%=siteConfig.webcompany %></li>
                <li>网站域名：<%=siteConfig.weburl %></li>
                <li>安装目录：<%=siteConfig.webpath %></li>
                <li>网站管理目录：<%=siteConfig.webmanagepath %></li>
                <li>附件上传目录：<%=siteConfig.filepath %></li>
                <li>服务器名称：<%=Server.MachineName%></li>
                <li>服务器IP：<%=Request.ServerVariables["LOCAL_ADDR"] %></li>
                <li>NET框架版本：<%=Environment.Version.ToString()%></li>
                <li>操作系统：<%=Environment.OSVersion.ToString()%></li>
                <li>IIS环境：<%=Request.ServerVariables["SERVER_SOFTWARE"]%></li>
                <li>服务器端口：<%=Request.ServerVariables["SERVER_PORT"]%></li>
                <li>目录物理路径：<%=Request.ServerVariables["APPL_PHYSICAL_PATH"]%></li>
            </ul>
        </div>
<%--        <div class="line20"></div>
        
        <div class="nlist-3">
            <ul>
                <li><a onclick="parent.linkMenuTree(true, 'site_config');" class="icon-setting" href="javascript:;"></a><span>系统设置</span></li>
                <li><a onclick="parent.linkMenuTree(true, 'site_channel_category');" class="icon-channel" href="javascript:;"></a><span>频道分类</span></li>
                <li><a onclick="parent.linkMenuTree(true, 'app_templet_list');" class="icon-templet" href="javascript:;"></a><span>模板管理</span></li>
                <li><a onclick="parent.linkMenuTree(true, 'app_builder_html');" class="icon-mark" href="javascript:;"></a><span>生成静态</span></li>
                <li><a onclick="parent.linkMenuTree(true, 'app_plugin_list');" class="icon-plugin" href="javascript:;"></a><span>插件配置</span></li>
                <li><a onclick="parent.linkMenuTree(true, 'user_list');" class="icon-user" href="javascript:;"></a><span>会员管理</span></li>
                <li><a onclick="parent.linkMenuTree(true, 'manager_list');" class="icon-manaer" href="javascript:;"></a><span>管理员</span></li>
                <li><a onclick="parent.linkMenuTree(true, 'manager_log');" class="icon-log" href="javascript:;"></a><span>系统日志</span></li>
            </ul>
        </div>

        <div class="nlist-4">
            <h3><i class="site"></i>建站三步曲</h3>
            <ul>
                <li>1、进入后台管理中心，点击“系统设置”修改网站配置信息；</li>
                <li>2、点击“频道分类”进行系统频道分类、建立频道、扩展字段等信息；</li>
                <li>3、制作好网站模板，上传到站点templates目录下，点击“模板管理”生成模板；</li>
            </ul>
        </div>--%>
        <%} %>

        <div class="line20"></div>
        <div class="nlist-2">
            <h3><i></i>统计数据</h3>
            <ul>
                <li>累计成交金额：<%=totalInvested %></li>
                <li>累计支付利息：<%=totalProfit%></li>
                <li>累计待收本金：<%=totalInvesting %></li>
                <li>昨日成交金额：<%=tradingVolume%></li>
                <li>站岗资金：<%=totalIdle %></li>
                <li>累计充值：<%=totalRecharge %></li>
                <li>累计提现：<%=totalWithDraw %></li>
                <li>注册人数：<%=userCount%>人</li>
                <li>标的总数：<%=projectCount%>个</li>  
                <li>今日登陆人数：<%=totalLoginCount%>人</li>
                <li>今日注册人数：<%=totalRegisterCount%>个</li>               
            </ul>
        </div>

        <!--/内容-->
    </form>
</body>
</html>
