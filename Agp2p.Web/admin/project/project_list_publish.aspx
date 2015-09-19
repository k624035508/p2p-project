<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="project_list_publish.aspx.cs" Inherits="Lip2p.Web.admin.project.project_list_publish" %>
<%@ Import namespace="Lip2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>项目立项</title>
<script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="../../scripts/jquery/jquery.lazyload.min.js"></script>
<script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="../js/layout.js"></script>
<link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
<link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    $(function () {
        imgLayout();
        $(window).resize(function () {
            imgLayout();
        });
        //图片延迟加载
        $(".pic img").lazyload({ load: AutoResizeImage, effect: "fadeIn" });
        //点击图片链接
        $(".pic img").click(function () {
            //$.dialog({ lock: true, title: "查看大图", content: "<img src=\"" + $(this).attr("src") + "\" />", padding: 0 });
            var linkUrl = $(this).parent().parent().find(".foot a").attr("href");
            if (linkUrl != "") {
                location.href = linkUrl; //跳转到修改页面
            }
        });

        // alert($("input[name='rptList1$ctl01$hidStatus']").val());

        $(".statustime").each(function () {
            var sid = $(this).val();
            var dtime = $(this).attr("publishtime");
            var id = $(this).attr("pid");
            if (sid == "55") {
                if (dtime != null) {
                    var endtime = new Date(Date.parse(dtime.replace(/-/g, "/"))).getTime();
                    timer(endtime, id);
                }
            }
        });
    });
    //排列图文列表
    function imgLayout() {
        var imgWidth = $(".imglist").width();
        var lineCount = Math.floor(imgWidth / 222);
        var lineNum = imgWidth % 222 / (lineCount - 1);
        $(".imglist ul").width(imgWidth + Math.ceil(lineNum));
        $(".imglist ul li").css("margin-right", parseFloat(lineNum));
    }
    //等比例缩放图片大小
    function AutoResizeImage(e, s) {
        var img = new Image();
        img.src = $(this).attr("src")
        var w = img.width;
        var h = img.height;
        var wRatio = w / h;
        if ((220 / wRatio) >= 165) {
            $(this).width(220); $(this).height(220 / wRatio);
        } else {
            $(this).width(165 * wRatio); $(this).height(165);
        }
    }

    function timer(endtime, itemid) {
        window.setInterval(function () {
            var nowtime = new Date();
            var totalSeconds = parseInt((endtime - nowtime.getTime()) / 1000);
            var intDiff = parseInt(totalSeconds)+2; //倒计时总秒数量
            var hour = "00",
                minute = 0,
                second = 0; //时间默认值
            if (intDiff > 0) {
                hour = Math.floor(intDiff % 86400 / 3600);
                minute = Math.floor(intDiff % 3600 / 60);
                second = Math.floor(intDiff % 60);
                $("#clock" + itemid + "").html(hour + ":" + minute + ":" + second + "");
                intDiff--;
            } else {
                clearInterval(timer);
                location.reload();
            }
        }, 1000);
    }

</script>

</head>

<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="javascript:history.back(-1);" class="back"><i></i><span>返回上一页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>项目立项</span>
</div>
<!--/导航栏-->

<!--工具栏-->
<div class="toolbar-wrap">
  <div id="floatHead" class="toolbar">
    <div class="l-list">
      <ul class="icon-list">
        <li><asp:LinkButton ID="btnSave" runat="server" CssClass="save" onclick="btnSave_Click"><i></i><span>保存</span></asp:LinkButton></li>            
      </ul>
    </div>
    <div class="r-list">
      <div class="menu-list rl" style="display:inline-block;">
        <div class="rule-single-select">
          <asp:DropDownList ID="ddlCategoryId" runat="server" AutoPostBack="True" onselectedindexchanged="ddlCategoryId_SelectedIndexChanged"></asp:DropDownList>
        </div>
        <div class="rule-single-select">
          <asp:DropDownList ID="ddlProperty" runat="server" AutoPostBack="True" onselectedindexchanged="ddlProperty_SelectedIndexChanged">
            <asp:ListItem Value="" Selected="True">所有状态</asp:ListItem>
            <asp:ListItem Value="30">签约</asp:ListItem>
            <asp:ListItem Value="40">立标</asp:ListItem>
            <asp:ListItem Value="50">复核</asp:ListItem>
            <asp:ListItem Value="60">发标</asp:ListItem>
            <%--<asp:ListItem Value="70">截标</asp:ListItem>--%>
            <asp:ListItem Value="80">满标</asp:ListItem>
            <asp:ListItem Value="90">完成</asp:ListItem>
          </asp:DropDownList>
        </div>
      </div>
      <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True" />
      <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" onclick="btnSearch_Click">查询</asp:LinkButton>
      <asp:LinkButton ID="lbtnViewImg" runat="server" CssClass="img-view" onclick="lbtnViewImg_Click" ToolTip="图像列表视图" />
      <asp:LinkButton ID="lbtnViewTxt" runat="server" CssClass="txt-view" onclick="lbtnViewTxt_Click" ToolTip="文字列表视图" />
    </div>
  </div>
</div>
<!--/工具栏-->

<!--文字列表-->
<asp:Repeater ID="rptList1" runat="server" onitemcommand="rptList_ItemCommand">
<HeaderTemplate>
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th width="6%">选择</th>
    <th align="left">标题</th>
    <th align="center" width="8%">所属类别</th>
    <th align="center" width="110">项目状态</th>
    <th align="center" width="150">项目金额</th>
    <th align="center" width="110">还款期限</th>
    <th align="center" width="110">还款方式</th>
    <th align="center" width="10%">立项时间</th>
    <th align="center" width="10%">发标时间</th>
    <th align="center"  width="8%">倒计时</th> 
    <th align="center" width="8%">项目标识</th> 
    <th align="center" width="65">排序</th>    
    <th width="10%">操作</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td align="center">
        <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" style="vertical-align:middle;" />
        <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" />
        <input id="HidStatus" type="hidden" value='<%#Eval("status")%>' publishtime='<%#Eval("publish_time")%>' pid='<%#Eval("id")%>' class="statustime"/>
    </td>
    <td><a href="project_edit_publish.aspx?action=<%#DTEnums.ActionEnum.Edit %>&channel_id=<%#this.channel_id %>&id=<%#Eval("id")%>"><%#Eval("title")%></a></td>
    <td align="center"><%#new Lip2p.BLL.article_category().GetTitle(Convert.ToInt32(Eval("category_id")))%></td>
    <td align="center"><%#Utils.GetLip2pEnumDes((Lip2p.Common.Lip2pEnums.ProjectStatusEnum)Utils.StrToInt(Eval("status").ToString(),0))%></td>
    <td align="center"><%#string.Format("{0:c}", Eval("financing_amount"))%></td>
    <td align="center"><%#Eval("repayment_term_span_count")%> <%#Utils.GetLip2pEnumDes((Lip2p.Common.Lip2pEnums.ProjectRepaymentTermSpanEnum)Utils.StrToInt(Eval("repayment_term_span").ToString(),0))%></td>
    <td align="center"><%#Utils.GetLip2pEnumDes((Lip2p.Common.Lip2pEnums.ProjectRepaymentTypeEnum)Utils.StrToInt(Eval("repayment_type").ToString(), 0))%></td>
    <td align="center"><%#string.Format("{0:g}",Eval("add_time"))%></td>
    <td align="center"><%#string.Format("{0:g}",Eval("publish_time"))%></td>
    <td align="center"><div id="clock<%#Eval("id")%>" style="height: 20px; line-height: 20px; font-size: 14px; overflow: hidden; text-align: center;"></div></td>
    <td align="center">
      <div class="btn-tools">
        <asp:LinkButton ID="lbtnIsOrdered" CommandName="IsOrdered" runat="server" CssClass='<%# Convert.ToInt32(Eval("tag")) == 1 ? "top selected" : "top"%>' ToolTip='<%# Convert.ToInt32(Eval("tag")) == 1 ? "取消约标" : "设置约标"%>' />                
        <asp:LinkButton ID="lbtnIsHot" CommandName="IsHot" runat="server" CssClass='<%# Convert.ToInt32(Eval("tag")) == 2 ? "hot selected" : "hot"%>' ToolTip='<%# Convert.ToInt32(Eval("tag")) == 2 ? "取消火爆" : "设置火爆"%>' />        
        <asp:LinkButton ID="lbtnIsTrial" CommandName="IsTrial" runat="server" CssClass='<%# Convert.ToInt32(Eval("tag")) == (int) Lip2pEnums.ProjectTagEnum.Trial ? "red selected" : "red"%>'
            ToolTip='<%# Convert.ToInt32(Eval("tag")) == (int) Lip2pEnums.ProjectTagEnum.Trial ? "取消设置为新手标" : "设置为新手标"%>' />
        <asp:LinkButton ID="lbtnIsDailyProject" CommandName="IsDailyProject" runat="server" CssClass='<%# Convert.ToInt32(Eval("tag")) == (int) Lip2pEnums.ProjectTagEnum.DailyProject ? "red selected" : "red"%>'
            ToolTip='<%# Convert.ToInt32(Eval("tag")) == (int) Lip2pEnums.ProjectTagEnum.DailyProject ? "取消设置为天标" : "设置为天标"%>' />
      </div>
    </td>
    <td align="center"><asp:TextBox ID="txtSortId" runat="server" Text='<%#Eval("sort_id")%>' CssClass="sort" onkeydown="return checkNumber(event);" /></td>
    <td align="center">
    <%#Utils.StrToInt(Eval("status").ToString(), 0) != (int)Lip2p.Common.Lip2pEnums.ProjectStatusEnum.QianYue ?
        "<a href=\"project_edit_publish.aspx?action=" + DTEnums.ActionEnum.Edit + "&channel_id=" + this.channel_id + "&id=" + Eval("id") + "\">修改</a>" :
        "<a href=\"project_edit_publish.aspx?action="+ DTEnums.ActionEnum.Copy + "&channel_id=" + this.channel_id + "&id=" + Eval("id") + "\">拆标</a>"%>
        <%if (ChkAdminLevelReturn("project_" + page_name, DTEnums.ActionEnum.Replace.ToString()))
          {%>
        <a href="project_edit_reset.aspx?action=<%#DTEnums.ActionEnum.Edit%>&channel_id=<%#this.channel_id%> &id=<%#Eval("id")%>">调整</a>
        <%} %>
    </td>
  </tr>
</ItemTemplate>
<FooterTemplate>
<%#rptList1.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"10\">暂无记录</td></tr>" : ""%>
</table>
</FooterTemplate>
</asp:Repeater>
<!--/文字列表-->

<!--图片列表-->
<asp:Repeater ID="rptList2" runat="server">
<HeaderTemplate>
<div class="imglist">
  <ul>
</HeaderTemplate>
<ItemTemplate>
    <li>
      <div class="details<%#Eval("img_url") != null && Eval("img_url").ToString() != "" ? "" : " nopic"%>">
        <div class="check">
            <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" />
            <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" />
        </div>
        <%#Eval("img_url") != null && Eval("img_url").ToString() != "" ? "<div class=\"pic\"><img src=\"../skin/default/loadimg.gif\" data-original=\"" + Eval("img_url") + "\" /></div><i class=\"absbg\"></i>" : ""%>
        <h1><span><a href="project_edit_publish.aspx?action=<%#DTEnums.ActionEnum.Edit %>&channel_id=<%#this.channel_id %>&id=<%#Eval("id")%>"><%#Eval("title")%></a></span></h1>
        <div class="remark"></div>
        <div class="foot">
          <p class="time"><%#string.Format("{0:yyyy-MM-dd HH:mm:ss}", Eval("add_time"))%></p>
          <a href="project_edit_publish.aspx?action=<%#DTEnums.ActionEnum.Edit %>&channel_id=<%#this.channel_id %>&id=<%#Eval("id")%>" title="编辑" class="edit">编辑</a>
        </div>
      </div>
    </li>
</ItemTemplate>
<FooterTemplate>
    <%#rptList2.Items.Count == 0 ? "<div align=\"center\" style=\"font-size:12px;line-height:30px;color:#666;\">暂无记录</div>" : ""%>
  </ul>
</div>
</FooterTemplate>
</asp:Repeater>
<!--/图片列表-->

<!--内容底部-->
<div class="line20"></div>
<div class="pagelist">
  <div class="l-btns">
    <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox><span>条/页</span>
  </div>
  <div id="PageContent" runat="server" class="default"></div>
</div>
<!--/内容底部-->
</form>
</body>
</html>
