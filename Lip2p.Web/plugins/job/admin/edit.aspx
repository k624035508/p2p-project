<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="edit.aspx.cs" Inherits="Lip2p.Web.Plugin.Job.admin.edit" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>编辑招聘信息</title>
<script type="text/javascript" src="/scripts/jquery/jquery-1.10.2.min.js"></script>
<script type="text/javascript" src="/scripts/jquery/Validform_v5.3.2_min.js"></script>
<script type="text/javascript" src="/scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
<script type="text/javascript" src="/scripts/datepicker/WdatePicker.js"></script>
<script type="text/javascript" src="/scripts/swfupload/swfupload.js"></script>
<script type="text/javascript" src="/scripts/swfupload/swfupload.queue.js"></script>
<script type="text/javascript" src="/scripts/swfupload/swfupload.handlers.js"></script>
<script type="text/javascript" charset="utf-8" src="/editor/kindeditor-min.js"></script>
<script type="text/javascript" charset="utf-8" src="/editor/lang/zh_CN.js"></script>
<script type="text/javascript" src="/admin/js/layout.js"></script>
<link href="/admin/skin/default/style.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    //加载编辑器
    $(function () {
        //初始化编辑器
        var editor = KindEditor.create('.editor', {
            width: '98%',
            height: '350px',
            resizeType: 1,
            uploadJson: '../../tools/upload_ajax.ashx?action=EditorFile&IsWater=1',
            fileManagerJson: '../../tools/upload_ajax.ashx?action=ManagerFile',
            allowFileManager: true
        });

    });
    //表单验证
    $(function () {
        $("#form1").validate({
            errorPlacement: function (lable, element) {
                element.ligerTip({ content: lable.html(), appendIdTo: lable });
            },
            success: function (lable) {
                lable.ligerHideTip();
            }
        });
    });
</script>
</head>
<body class="mainbody">
<form id="form1" runat="server">
<!--导航栏-->
<div class="location">
  <a href="index.aspx" class="back"><i></i><span>返回列表页</span></a>
  <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
  <i class="arrow"></i>
  <span>编辑招聘</span>
</div>
<div class="line10"></div>
<!--/导航栏-->

<div class="content-tab-wrap">
  <div id="floatHead" class="content-tab">
    <div class="content-tab-ul-wrap">
      <ul>
        <li><a href="javascript:;" onclick="tabs(this);" class="selected">基本信息</a></li>

        <li><a href="javascript:;" onclick="tabs(this);">联系信息</a></li>
      
      </ul>
    </div>
  </div>
</div>

<div class="tab-content" style="display:block;">

            <dl>
                <dt>启用状态：</dt>
                <dd>
                 <div class="rule-multi-radio">
                    <asp:RadioButtonList ID="rblIsLock" runat="server"  RepeatDirection="Horizontal" RepeatLayout="Flow">
                        <asp:ListItem Selected="True" Value="0">正常 </asp:ListItem>
                        <asp:ListItem Value="1">暂停 </asp:ListItem>
                    </asp:RadioButtonList>
                    </div>
                </dd>
            </dl>
            <dl>
                <dt>职位名称：</dt>
                <dd><asp:TextBox ID="txtTitle" runat="server" CssClass="txtInput normal required" minlength="2" maxlength="255"></asp:TextBox><label>*</label></dd>
            </dl>
            <dl>
                <dt>招聘部门：</dt>
                <dd><asp:TextBox ID="txtDepart" runat="server" CssClass="txtInput normal required" minlength="2" maxlength="255"></asp:TextBox><label>*</label></dd>
            </dl>
            <dl>
                <dt>招聘人数：</dt>
                <dd><asp:TextBox ID="txtNumber" runat="server" CssClass="txtInput normal small required digits"></asp:TextBox><label>*</label></dd>
            </dl>
            <dl>
                <dt>性别要求：</dt>
                <dd>
                 <div class="rule-single-select">
                    <asp:DropDownList ID="ddlSex" runat="server" CssClass="select2">
                    <asp:ListItem Value="不限" Selected="True">不限</asp:ListItem>
                    <asp:ListItem Value="男">男</asp:ListItem>
                    <asp:ListItem Value="女">女</asp:ListItem>
                    </asp:DropDownList>
                    </div>
                </dd>
            </dl>
            <dl>
                <dt>工作经验：</dt>
                <dd><asp:TextBox ID="txtExperience" runat="server" CssClass="txtInput normal" maxlength="50"></asp:TextBox><label>提示：如填写三年以上工作经验</label></dd>
            </dl>
            <dl>
                <dt>学历要求：</dt>
                <dd><asp:TextBox ID="txtEducation" runat="server" CssClass="txtInput normal" maxlength="50"></asp:TextBox><label>提示：如填写大专/本科</label></dd>
            </dl>
            <dl>
                <dt>年龄要求：</dt>
                <dd><asp:TextBox ID="txtAge" runat="server" CssClass="txtInput normal" maxlength="50"></asp:TextBox><label>提示：如填写20-30岁</label></dd>
            </dl>
            <dl>
                <dt>专业要求：</dt>
                <dd><asp:TextBox ID="txtProfession" runat="server" CssClass="txtInput normal" maxlength="50"></asp:TextBox><label>提示：如填写电子商务专业</label></dd>
            </dl>
            <dl>
                <dt>工作地区：</dt>
                <dd><asp:TextBox ID="txtWork_area" runat="server" CssClass="txtInput normal" maxlength="50"></asp:TextBox><label>提示：如填写上海地区</label></dd>
            </dl>
            <dl>
                <dt>开始日期：</dt>
                <dd><asp:TextBox ID="txtStart_time" runat="server" CssClass="txtInput normal required dateISO" onclick="return Calendar('txtStart_time');"></asp:TextBox><label>*</label></dd>
            </dl>
            <dl>
                <dt>结束日期：</dt>
                <dd><asp:TextBox ID="txtEnd_time" runat="server" CssClass="txtInput normal required dateISO" onclick="return Calendar('txtEnd_time');"></asp:TextBox><label>*</label></dd>
            </dl>
            <dl>
                <dt>详细内容：</dt>
                <dd>
                 <textarea id="txtContent" class="editor" style="visibility:hidden;" runat="server"></textarea>

                </dd>
            </dl>
            <dl>
                <dt>浏览次数：</dt>
                <dd><asp:TextBox ID="txtClick" runat="server" CssClass="txtInput normal small required digits">0</asp:TextBox><label>*</label></dd>
            </dl>
            <dl>
                <dt>排 序：</dt>
                <dd><asp:TextBox ID="txtSortId" runat="server" CssClass="txtInput normal small required digits">1</asp:TextBox><label>*</label></dd>
            </dl>

        </div>
<div class="tab-content" style="display:none">
         
            <dl>
                <dt>联系人姓名：</dt>
                <dd><asp:TextBox ID="txtContactName" runat="server" CssClass="txtInput normal" maxlength="50"></asp:TextBox></dd>
            </dl>
            <dl>
                <dt>联系人电话：</dt>
                <dd><asp:TextBox ID="txtPhone" runat="server" CssClass="txtInput normal" maxlength="50"></asp:TextBox></dd>
            </dl>
            <dl>
                <dt>单位传真：</dt>
                <dd><asp:TextBox ID="txtFax" runat="server" CssClass="txtInput normal" maxlength="50"></asp:TextBox></dd>
            </dl>
            <dl>
                <dt>联系邮箱：</dt>
                <dd><asp:TextBox ID="txtEmail" runat="server" CssClass="txtInput normal" maxlength="50"></asp:TextBox></dd>
            </dl>
            <dl>
                <dt>联系地址：</dt>
                <dd><asp:TextBox ID="txtAddress" runat="server" CssClass="txtInput normal" maxlength="200" Width="300px"></asp:TextBox></dd>
            </dl>
        
        </div>
    
<div class="page-footer">
  <div class="btn-list">
    <asp:Button ID="btnSubmit" runat="server" Text="提交保存" CssClass="btn" onclick="btnSubmit_Click" />
    <input name="btnReturn" type="button" value="返回上一页" class="btn yellow" onclick="javascript:history.back(-1);" />
  </div>
  <div class="clear"></div>
</div>
</form>
</body>
</html>
