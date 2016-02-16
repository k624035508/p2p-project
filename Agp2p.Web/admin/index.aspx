<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="Agp2p.Web.admin.index" %>

<!DOCTYPE html>
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<meta name="viewport" content="width=device-width,minimum-scale=1.0,maximum-scale=1.0,initial-scale=1.0,user-scalable=no" />
<meta name="apple-mobile-web-app-capable" content="yes" />
<title>后台管理中心</title>
<link href="../scripts/artdialog/ui-dialog.css" rel="stylesheet" type="text/css" />
<link href="skin/default/style.css" rel="stylesheet" type="text/css" />
<script type="text/javascript" charset="utf-8" src="../scripts/jquery/jquery-1.11.2.min.js"></script>
<script type="text/javascript" charset="utf-8" src="../scripts/jquery/jquery.nicescroll.js"></script>
<script type="text/javascript" charset="utf-8" src="../scripts/artdialog/dialog-plus-min.js"></script>
<script type="text/javascript" charset="utf-8" src="js/layindex.js"></script>
<script type="text/javascript" charset="utf-8" src="js/common.js"></script>
<script src="../scripts/jquery.signalR-2.2.0.min.js"></script>
<script src="../signalr/hubs"></script>
<script type="text/javascript">

var showMyMessages = function() {
    var dialogMountPoint = $(".manager-msg")[0];
    dialog({
        width: '680px',
        height: '650px',
        content: '<iframe width="100%" height="100%" src="/admin/manager-messages.html"></iframe>',
        quickClose: true
    }).show(dialogMountPoint);
}

var checkMyMessage = function () {
    var url = '<%=Request.FilePath%>' + "/AjaxQueryUnreadMessagesCount";
    $.ajax({
        type: "get",
        dataType: "json",
        contentType: "application/json",
        url: url,
        success: function(result) {
            var originalHint = $(".manager-msg")[0].innerText;
            var newHint = "未读消息：" + result.d;
            if (parseInt(originalHint.match(/\d+/)[0]) < parseInt(result.d) && localStorage.getItem("newMsgAutoPopup") !== "false") {
                showMyMessages();
            }
            $(".manager-msg")[0].innerText = newHint;
        }.bind(this),
        error: function(xhr, status, err) {
            console.error(url, status, err.toString());
        }.bind(this)
    });
}

//页面加载完成时
$(function () {
    //检测IE
    if ('undefined' == typeof(document.body.style.maxHeight)){
        window.location.href = 'ie6update.html';
    }

    $(".manager-msg").click(showMyMessages);


    var hub = $.connection.managerMessageHub;

    hub.client.onNewMsg = function() {
        console.log("收到新消息");
        checkMyMessage();
    };

    hub.client.onMsgRead = function() {
        checkMyMessage();
        console.log("有消息设置为已读了");
    };

    $.connection.hub.start().done(function () {
        console.log("服务器已连接……");
        checkMyMessage();
    });

    document.getElementById("cbxAutoPopup").checked = localStorage.getItem("newMsgAutoPopup") !== "false";
});
</script>
<style>
.manager-msg, .info:hover > .manager-msg-auto-pop {
    display: inline-block;
    float: left;
    margin: 0 15px;
    cursor: pointer;
}
.manager-msg-auto-pop {
    display: none;
}
</style>
</head>

<body class="indexbody">
<form id="form1" runat="server">
  <!--全局菜单-->
  <a class="btn-paograms" onclick="togglePopMenu();"></a>
  <div id="pop-menu" class="pop-menu">
    <div class="pop-box">
      <h1 class="title"><i></i>导航菜单</h1>
      <i class="close" onclick="togglePopMenu();">关闭</i>
      <div class="list-box"></div>
    </div>
    <i class="arrow">箭头</i>
  </div>
  <!--/全局菜单-->

  <div class="main-top">
    <a class="icon-menu"></a>
    <div id="main-nav" class="main-nav"></div>
    <div class="nav-right">
      <div class="info">
        <div class="manager-msg-auto-pop">自动弹出：<input id="cbxAutoPopup" type="checkbox" onclick='localStorage.setItem("newMsgAutoPopup", this.checked)' /> </div>
        <div class="manager-msg">未读消息：0</div>
        <i></i>
        <span>
          您好，<%=admin_info.user_name %><br>
          <%=new Agp2p.BLL.manager_role().GetTitle(admin_info.role_id) %>
        </span>
      </div>
      <div class="option">
        <i></i>
        <div class="drop-wrap">
          <div class="arrow"></div>
          <ul class="item">
            <li>
              <a href="../" target="_blank">预览网站</a>
            </li>
            <li>
              <a href="center.aspx" target="mainframe">管理中心</a>
            </li>
            <li>
              <a href="manager/manager_pwd.aspx" onclick="linkMenuTree(false, '');" target="mainframe">修改密码</a>
            </li>
            <li>
              <asp:LinkButton ID="lbtnExit" runat="server" onclick="lbtnExit_Click">注销登录</asp:LinkButton>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </div>
  
  <div class="main-left">
    <h1 class="logo"></h1>
    <div id="sidebar-nav" class="sidebar-nav"></div>
  </div>
  
  <div class="main-container">
    <iframe id="mainframe" name="mainframe" frameborder="0" src="center.aspx"></iframe>
  </div>

</form>
</body>
</html>
