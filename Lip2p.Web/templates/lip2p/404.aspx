<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="404.aspx.cs" Inherits="Lip2p.Web.templates.lip2p._404" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8">
    <title>404Error</title>
    <style>
        *{
            font-family:"Microsoft YaHei",微软雅黑;
            margin: 0;
            padding: 0;
        }

        html,body {
            min-width: 1024px;
            height: auto;
        }

        .outer-wrapper {
            position: relative;
            margin-left: auto;
            margin-right: auto;
            background: url("/templates/lip2p/images/error.png") no-repeat;
            background-position: 50% 0;
            height: 1080px;
        }

        div.content-box {
            position: relative;
            width: 53%;
            left: 47%;
            top: 435px;
        }

        div.content-box > div {
            display: inline-block;
            vertical-align: middle;
        }

        div.btns span a { padding-left: 5px;}

        div.btns span a:link,
        div.btns span a:hover {
            color: #000;
            text-decoration: none;
        }

        img.scancode {
            max-width: 100%;
            height: auto;
        }

        div.btns {
            margin-top: 18px;
        }

        div.btns span {
            margin-right: 20px;
            font-size: 1.125em;
        }

        div.tips {
            margin-left: 10px;
        }

        @media screen and (max-device-width: 1660px) {
            div.content-box{
                left: 45%;
            }
        }
    </style>
</head>
<body>
<div class="outer-wrapper">
    <div class="content-box">
        <div>
            <img class="scancode" src="/templates/lip2p/images/2dcode.png"/>
        </div>
        <div class="tips">
            <div style="font-size: 1.25em">该页面无法正常显示，请浏览其他页面：</div>
            <div class="btns">
                <span><img src="/templates/lip2p/images/back.png"/><a href="javascript:history.back(-1);">返回</a></span>
                <span><img src="/templates/lip2p/images/home.png"/><a href="http://www.lip2p.com/" style="color:#000">网站首页</a></span>
                <span><img src="/templates/lip2p/images/invest.png"><a href="http://www.lip2p.com/invest.html" style="color:#000">去投资</a></span>
            </div>
        </div>
    </div>
</div>
</body>
</html>
