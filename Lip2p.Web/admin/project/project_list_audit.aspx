<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="project_list_audit.aspx.cs" Inherits="Lip2p.Web.admin.project.project_list_audit" %>
<%@ Import namespace="Lip2p.Common" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>项目审批</title>
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
        img.src = $(this).attr("src");
        var w = img.width;
        var h = img.height;
        var wRatio = w / h;
        if ((220 / wRatio) >= 165) {
            $(this).width(220); $(this).height(220 / wRatio);
        } else {
            $(this).width(165 * wRatio); $(this).height(165);
        }
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
        <li><a class="all" href="javascript:;" onclick="checkAll(this);"><i></i><span>全选</span></a></li>   
        <li><asp:LinkButton ID="btnAudit" runat="server" CssClass="folder" OnClientClick="return ExePostBack('btnAudit','所选项目将通过审批，确定继续吗？');" onclick="btnAudit_Click"><i></i><span>审批</span></asp:LinkButton></li>     
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
            <asp:ListItem Value="10">立项</asp:ListItem>
            <asp:ListItem Value="40">立标</asp:ListItem>
          </asp:DropDownList>
        </div>
      </div>
      <asp:TextBox ID="txtKeywords" runat="server" CssClass="keyword" onkeydown="return Enter(event);" ontextchanged="txtPageNum_TextChanged" AutoPostBack="True"/>
      <asp:LinkButton ID="lbtnSearch" runat="server" CssClass="btn-search" onclick="btnSearch_Click">查询</asp:LinkButton>
      <asp:LinkButton ID="lbtnViewImg" runat="server" CssClass="img-view" onclick="lbtnViewImg_Click" ToolTip="图像列表视图" />
      <asp:LinkButton ID="lbtnViewTxt" runat="server" CssClass="txt-view" onclick="lbtnViewTxt_Click" ToolTip="文字列表视图" />
    </div>
  </div>
</div>
<!--/工具栏-->

<!--文字列表-->
<asp:Repeater ID="rptList1" runat="server" >
<HeaderTemplate>
<table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
  <tr>
    <th width="6%">选择</th>
    <th align="left">标题</th>
    <th align="left" width="12%">所属类别</th>
    <th align="left" width="110">项目状态</th>
    <th align="left" width="150">项目金额</th>
    <th align="left" width="110">还款期限</th>
    <th align="left" width="110">还款方式</th>
    <th align="left" width="16%">立项时间</th>
    <th align="left" width="65">排序</th>    
    <th width="8%">操作</th>
  </tr>
</HeaderTemplate>
<ItemTemplate>
  <tr>
    <td align="center"><asp:CheckBox ID="chkId" CssClass="checkall" runat="server" style="vertical-align:middle;" />
    <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" /></td>
    <td><a href="project_edit_audit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&channel_id=<%#this.channel_id %>&id=<%#Eval("id")%>"><%#Eval("title")%></a></td>
    <td><%#new Lip2p.BLL.article_category().GetTitle(Convert.ToInt32(Eval("category_id")))%></td>
    <td><%#Utils.GetLip2pEnumDes((Lip2p.Common.Lip2pEnums.ProjectStatusEnum)Utils.StrToInt(Eval("status").ToString(),0))%></td>
    <td><%#string.Format("{0:c}", Eval("financing_amount"))%></td>
    <td><%#Eval("repayment_term_span_count")%> <%#Utils.GetLip2pEnumDes((Lip2p.Common.Lip2pEnums.ProjectRepaymentTermSpanEnum)Utils.StrToInt(Eval("repayment_term_span").ToString(), 0))%></td>
    <td><%#Utils.GetLip2pEnumDes((Lip2p.Common.Lip2pEnums.ProjectRepaymentTypeEnum)Utils.StrToInt(Eval("repayment_type").ToString(), 0))%></td>
    <td><%#string.Format("{0:g}",Eval("add_time"))%></td>
    <td><asp:TextBox ID="txtSortId" runat="server" Text='<%#Eval("sort_id")%>' CssClass="sort" onkeydown="return checkNumber(event);" /></td>
    <td align="center"><a href="project_edit_audit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&channel_id=<%#this.channel_id %>&id=<%#Eval("id")%>">审批</a>
    </td></td>
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
        <div class="check"><asp:CheckBox ID="chkId" CssClass="checkall" runat="server" /><asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" /></div>
        <%#Eval("img_url") != null && Eval("img_url").ToString() != "" ? "<div class=\"pic\"><img src=\"../skin/default/loadimg.gif\" data-original=\"" + Eval("img_url") + "\" /></div><i class=\"absbg\"></i>" : ""%>
        <h1><span><a href="project_edit_audit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&channel_id=<%#this.channel_id %>&id=<%#Eval("id")%>"><%#Eval("title")%></a></span></h1>
        <div class="remark">          
        </div>
        <div class="foot">
          <p class="time"><%#string.Format("{0:yyyy-MM-dd HH:mm:ss}", Eval("add_time"))%></p>
          <a href="project_edit_audit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&channel_id=<%#this.channel_id %>&id=<%#Eval("id")%>" title="审批" class="edit">审批</a>
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
