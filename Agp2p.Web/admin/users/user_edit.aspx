<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="user_edit.aspx.cs" Inherits="Agp2p.Web.admin.users.user_edit"
    ValidateRequest="false" %>

<%@ Import Namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>编辑用户</title>
    <script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="../../scripts/jquery/Validform_v5.3.2_min.js"></script>
    <script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
    <script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
    <script type="text/javascript" src="../../scripts/swfupload/swfupload.js"></script>
    <script type="text/javascript" src="../../scripts/swfupload/swfupload.queue.js"></script>
    <script type="text/javascript" src="../../scripts/swfupload/swfupload.handlers.js"></script>
    <script type="text/javascript" src="../js/layout.js"></script>
    <link  href="../../css/pagination.css" rel="stylesheet" type="text/css" />
    <link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
        var action = "<%=pagein%>";
       
        $(function () {
            $(".tab-content ltable").css("width","92%");
            if (action == "Pageinviter") {
                //alert(page);
                $(".content-tab-ul-wrap a").removeClass("selected");
                $(".content-tab-ul-wrap li").eq(2).find("a").addClass("selected");
                $(".tab-content").css("display","none");
                $(".tab-content").eq(2).css("display","block");
            }
            //初始化表单验证
            $("#form1").initValidform();
            //初始化上传控件
            $(".upload-img").each(function () {
                $(this).InitSWFUpload({ filesize: "<%=siteConfig.imgsize %>", sendurl: "../../tools/upload_ajax.ashx", flashurl: "../../scripts/swfupload/swfupload.swf" });
            });

            $(".upload-album").each(function () {
                $(this).InitSWFUpload({ btntext: "批量上传", btnwidth: 66, single: false, water: true, thumbnail: true, filesize: "<%=siteConfig.imgsize %>", sendurl: "../../tools/upload_ajax.ashx", flashurl: "../../scripts/swfupload/swfupload.swf", filetypes: "*.jpg;*.jpeg;*.png;*.gif;" });
            });
        });
    </script>
</head>
<body class="mainbody">
    <form id="form1" runat="server" AUTOCOMPLETE="off">
    <!--导航栏-->
    <div class="location">
        <a href="user_list.aspx" class="back"><i></i><span>返回列表页</span></a> <a href="../center.aspx"
            class="home"><i></i><span>首页</span></a> <i class="arrow"></i><span>会员管理</span>
        <i class="arrow"></i><span>编辑用户</span>
    </div>
    <div class="line10">
    </div>
    <!--/导航栏-->
    <!--内容-->
    <div class="content-tab-wrap">
        <div id="floatHead" class="content-tab">
            <div class="content-tab-ul-wrap">
                <ul>
                    <li><a href="javascript:;" onclick="tabs(this);" class="selected">基本资料</a></li>
                    <li><a href="javascript:;" onclick="tabs(this);">账户信息</a></li>
                    <li><a href="javascript:;" onclick="tabs(this);">会员部功能</a></li>
                    <li><a href="javascript:;" onclick="tabs(this);">借款人信息</a></li>
                </ul>
            </div>
        </div>
    </div>
    <div class="tab-content">
        <dl>
            <dt>所属组别</dt>
            <dd>
                <div class="rule-single-select">
                    <asp:DropDownList ID="ddlGroupId" runat="server" datatype="*" errormsg="请选择组别" sucmsg=" "></asp:DropDownList>
                </div>
            </dd>
        </dl>
        <dl>
            <dt>用户状态</dt>
            <dd>
                <div class="rule-multi-radio">
                    <asp:RadioButtonList ID="rblStatus" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                        <asp:ListItem Value="0" Selected="True">正常</asp:ListItem>
                        <asp:ListItem Value="1">待验证</asp:ListItem>
                        <asp:ListItem Value="2">待审核</asp:ListItem>
                        <asp:ListItem Value="3">禁用</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
                <span class="Validform_checktip">*禁用账户无法登录</span>
            </dd>
        </dl>
        <dl>
            <dt>用户名</dt>
            <dd>
                <asp:TextBox ID="txtUserName" runat="server" CssClass="input normal" datatype="*2-100" AUTOCOMPLETE="off"
                    sucmsg=" " ajaxurl="../../tools/admin_ajax.ashx?action=validate_username"></asp:TextBox>
                <span class="Validform_checktip">*登录的用户名</span></dd>
        </dl>
        <dl>
            <dt>登录密码</dt>
            <dd>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="input normal" TextMode="Password" AUTOCOMPLETE="off"
                    datatype="*6-20" nullmsg="请设置密码" errormsg="密码范围在6-20位之间" sucmsg=" "></asp:TextBox>
                <span class="Validform_checktip">*登录的密码，至少6位</span></dd>
        </dl>
        <dl>
            <dt>确认密码</dt>
            <dd>
                <asp:TextBox ID="txtPassword1" runat="server" CssClass="input normal" TextMode="Password" AUTOCOMPLETE="off"
                    datatype="*" recheck="txtPassword" nullmsg="请再输入一次密码" errormsg="两次输入的密码不一致" sucmsg=" "></asp:TextBox>
                <span class="Validform_checktip">*再次输入密码</span></dd>
        </dl>
        <dl>
            <dt>邮箱账号</dt>
            <dd>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="input normal" datatype="*0-50"
                    sucmsg=" "></asp:TextBox>
                <span class="Validform_checktip">*取回密码时用到</span></dd>
        </dl>
        <dl>
            <dt>用户昵称</dt>
            <dd>
                <asp:TextBox ID="txtNickName" runat="server" CssClass="input normal"></asp:TextBox></dd>
        </dl>
        <dl>
            <dt>真实姓名</dt>
            <dd>
                <asp:TextBox ID="txtRealName" runat="server" CssClass="input normal"></asp:TextBox></dd>
        </dl>
        <dl>
            <dt>上传头像</dt>
            <dd>
                <asp:TextBox ID="txtAvatar" runat="server" CssClass="input normal upload-path"></asp:TextBox>
                <div class="upload-box upload-img">
                </div>
            </dd>
        </dl>
        <dl>
            <dt>用户性别</dt>
            <dd>
                <div class="rule-multi-radio">
                    <asp:RadioButtonList ID="rblSex" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                        <asp:ListItem Value="保密" Selected="True">保密</asp:ListItem>
                        <asp:ListItem Value="男">男</asp:ListItem>
                        <asp:ListItem Value="女">女</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
            </dd>
        </dl>
        <dl>
            <dt>生日日期</dt>
            <dd>
                <div class="input-date">
                    <asp:TextBox ID="txtBirthday" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                        datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}$/" errormsg="请选择正确的日期" sucmsg=" " />
                    <i></i>
                </div>
            </dd>
        </dl>
        <dl>
            <dt>手机号码</dt>
            <dd>
                <asp:TextBox ID="txtMobile" runat="server" CssClass="input normal"></asp:TextBox></dd>
        </dl>
        <dl>
            <dt>电话号码</dt>
            <dd>
                <asp:TextBox ID="txtTelphone" runat="server" CssClass="input normal"></asp:TextBox></dd>
        </dl>
        <dl>
            <dt>QQ号码</dt>
            <dd>
                <asp:TextBox ID="txtQQ" runat="server" CssClass="input normal"></asp:TextBox></dd>
        </dl>
        <dl>
            <dt>通讯地址</dt>
            <dd>
                <asp:TextBox ID="txtAddress" runat="server" CssClass="input normal"></asp:TextBox></dd>
        </dl>
        <dl>
            <dt>身份证号码</dt>
            <dd>
                <asp:TextBox ID="txtIdCard" runat="server" CssClass="input normal"></asp:TextBox></dd>
        </dl>
        <dl>
            <dt>身份证照片</dt>
            <dd>
                <div class="upload-box upload-album">
                </div>
                <div class="photo-list">
                    <ul>
                        <asp:Repeater ID="rptIdCardPic" runat="server">
                            <ItemTemplate>
                                <li>
                                    <input type="hidden" name="hid_photo_name" value="<%# Eval("id") %>|<%#Eval("original_path")%>|<%#Eval("thumb_path")%>" />
                                    <input type="hidden" name="hid_photo_remark" value="<%#Eval("remark")%>" />
                                    <div class="img-box" onclick="getBigPic(this);">
                                        <img src="<%#Eval("thumb_path")%>" bigsrc="<%#Eval("original_path")%>" />
                                        <span class="remark"><i><%#Eval("remark").ToString() == "" ? "暂无描述..." : Eval("remark").ToString()%></i></span>
                                    </div>
                                    <a href="javascript:;" onclick="setRemark(this);">描述</a> <a href="javascript:;" onclick="delImg(this);">
                                        删除</a> </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>
            </dd>
        </dl>
    </div>
    <div class="tab-content" style="display: none;">
        <dl>
            <dt>账户余额</dt>
            <dd>
                <asp:Literal ID="lblIdleMoney" runat="server" /></dd>
        </dl>
        <dl>
            <dt>待收本金</dt>
            <dd>
                <asp:Literal ID="lblInvestingMoney" runat="server" /></dd>
        </dl>
        <dl>
            <dt>冻结资金</dt>
            <dd>
                <asp:Literal ID="lblLockedMoney" runat="server" /></dd>
        </dl>
        <dl>
            <dt>待收利润</dt>
            <dd>
                <asp:Literal ID="lblProfitingMoney" runat="server" /></dd>
        </dl>
        <dl>
            <dt>累计利润</dt>
            <dd>
                <asp:Literal ID="lblTotalProfit" runat="server" /></dd>
        </dl>
        <dl>
            <dt>累计投资</dt>
            <dd>
                <asp:Literal ID="lblTotalInvestment" runat="server" /></dd>
        </dl>
<%--        <dl style="display: none;">
            <dt>账户金额</dt>
            <dd>
                <asp:TextBox ID="txtAmount" runat="server" CssClass="input small" datatype="/^(([1-9]{1}\d*)|([0]{1}))(\.(\d){1,2})?$/"
                    sucmsg=" ">0</asp:TextBox>
                元 <span class="Validform_checktip">*账户上的余额</span>
            </dd>
        </dl>--%>
        <dl>
            <dt>账户积分</dt>
            <dd>
                <asp:TextBox ID="txtPoint" runat="server" CssClass="input small" datatype="n" sucmsg=" ">0</asp:TextBox>
                分 <span class="Validform_checktip">*积分也可做为交易</span>
            </dd>
        </dl>
 <%--       <dl>
            <dt>升级经验值</dt>
            <dd>
                <asp:TextBox ID="txtExp" runat="server" CssClass="input small" datatype="n" sucmsg=" ">0</asp:TextBox>
                <span class="Validform_checktip">*根据积分计算得来，与积分不同的是只增不减</span>
            </dd>
        </dl>--%>
        <dl>
            <dt>注册时间</dt>
            <dd>
                <asp:Label ID="lblRegTime" Text="-" runat="server"></asp:Label></dd>
        </dl>
        <dl>
            <dt>注册IP</dt>
            <dd>
                <asp:Label ID="lblRegIP" Text="-" runat="server"></asp:Label></dd>
        </dl>
        <dl>
            <dt>最近登录时间</dt>
            <dd>
                <asp:Label ID="lblLastTime" Text="-" runat="server"></asp:Label></dd>
        </dl>
        <dl>
            <dt>最近登录IP</dt>
            <dd>
                <asp:Label ID="lblLastIP" Text="-" runat="server"></asp:Label></dd>
        </dl>
    </div>
    <div class="tab-content" style="display: none;">
            <dl>
                <dt>推荐人自动归组</dt>
                <dd>
                    <div class="rule-single-select">
                        <asp:DropDownList ID="ddlServingGroup" runat="server"></asp:DropDownList>
                    </div>
                </dd>
            </dl>
<%--        <dl>
                <dt>推荐人数</dt>
                <dd>
                    <asp:Literal ID="lblTotalInvitee" runat="server">0</asp:Literal>
                </dd>
            </dl>--%>
            <!--列表-->
            <P style="width: 92%;margin:5px auto;color: #333;font-size: 14px; border-bottom: 2px solid #808080;border-bottom: 1px solid #808080;padding-bottom: 5px;">推荐成员列表</P>
            <table border="0" cellspacing="0" cellpadding="0" class="ltable" style="width: 92%; margin: 10px auto;">
                <asp:Repeater ID="rptList" runat="server">
                    <HeaderTemplate>
                        <tr>
                            <th align="left">用户名</th>
                            <th width="8%">手机</th>
                            <th width="12%">身份证号</th>
                            <th align="right" width="8%">邮箱</th>
                            <th align="right" width="10%">当天投资金额</th>
                            <th align="right" width="10%">在投金额</th>
                            <th width="10%">余额</th>
                       <%--     <th width="8%">状态</th>--%>
                        </tr>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td>
                                <div class="user-box">
                                    <h4><b><a href="user_edit.aspx?action=<%#DTEnums.ActionEnum.Edit %>&id=<%#Eval("id")%>"><%#Eval("user_name")%></a></b> (姓名：<%#Eval("real_name")%>)</h4>
                                    <i>注册时间：<%#string.Format("{0:yy/MM/dd HH:mm}",Eval("reg_time"))%></i>
                                </div>
                            </td>
                            <td align="center"><%#Eval("mobile")%></td>
                            <td align="center"><%#Eval("id_card_number")%></td>
                            <td align="right"><%#Eval("email")%></td>
                            <td align="right"><%# QueryInvestmentToday((dt_users)Container.DataItem) %></td>
                            <td align="right"><%# Convert.ToDecimal(Eval("li_wallets.investing_money")).ToString("c") %></td>
                            <td align="center"><%# Convert.ToDecimal(Eval("li_wallets.idle_money")).ToString("c")%></td>
                           <%-- <td align="center"><%#GetUserStatus(Convert.ToInt32(Eval("status")))%></td>--%>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"8\">暂无记录</td></tr>" : ""%>
                    </FooterTemplate>
                </asp:Repeater>
            </table>
            <!--/列表-->
            <!--内容底部-->
            <div class="line20"></div>
            <div class="pagelist" style="width: 92%; margin: 10px auto;">
                <div class="l-btns">
                    <span>显示</span><asp:TextBox ID="txtPageNum" runat="server" CssClass="pagenum" onkeydown="return checkNumber(event);" OnTextChanged="txtPageNum_TextChanged" AutoPostBack="True"></asp:TextBox><span>条/页</span>
                </div>
                <div id="PageContent" runat="server" class="default"></div>
            </div>
            <!--/内容底部-->
        </div>
    <div class="tab-content" style="display: none;">
        <dl>
            <dt>是否借款人</dt>
            <dd>
                <div class="rule-single-checkbox">
                    <asp:CheckBox ID="chkIsLoaner" runat="server" AutoPostBack="True" OnCheckedChanged="chkIsLoaner_OnCheckedChanged"/>
                </div>
                <span class="Validform_checktip">*是否添加借款人信息</span>
            </dd>
        </dl>
        <% if (chkIsLoaner.Checked) { %>
        <dl>
            <dt>年龄</dt>
            <dd><asp:TextBox ID="txtAge" runat="server" CssClass="input normal" datatype="/^\d+$/" /><span class="Validform_checktip">* </span></dd>
        </dl>
        <dl>
            <dt>工作</dt>
            <dd><asp:TextBox ID="txtJob" runat="server" CssClass="input normal" datatype="/^.*$/"></asp:TextBox></dd>
        </dl>
        <dl>
            <dt>工作地点</dt>
            <dd><asp:TextBox ID="txtWorkingAt" runat="server" CssClass="input normal" datatype="/^.*$/"></asp:TextBox></dd>
        </dl>
        <dl>
            <dt>学历</dt>
            <dd><asp:TextBox ID="txtEducationalBackground" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">* </span></dd>
        </dl>
        <dl>
            <dt>婚姻状态</dt>
            <dd>
                <div class="rule-multi-radio">
                    <asp:RadioButtonList ID="rblMaritalStatus" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                        <asp:ListItem Value="1" Selected="True">未婚</asp:ListItem>
                        <asp:ListItem Value="2">已婚</asp:ListItem>
                        <asp:ListItem Value="3">离婚</asp:ListItem>
                        <asp:ListItem Value="4">丧偶</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
            </dd>
        </dl>
        <dl>
            <dt>收入</dt>
            <dd><asp:TextBox ID="txtIncome" runat="server" CssClass="input normal" datatype="/^.+$/"></asp:TextBox><span class="Validform_checktip">* </span></dd>
        </dl>
        <dl>
            <dt>是否添加公司信息</dt>
            <dd>
                <div class="rule-single-checkbox">
                    <asp:CheckBox ID="chkBindCompany" runat="server" AutoPostBack="True" OnCheckedChanged="chkBindCompany_OnCheckedChanged"/>
                </div>
                <span class="Validform_checktip">*是否添加公司信息</span>
            </dd>
        </dl>
        <% if (chkBindCompany.Checked) { %>
            <dl>
                <dt>公司名称</dt>
                <dd><asp:TextBox ID="txtCompanyName" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">* </span></dd>
            </dl>
            <dl>
                <dt>成立时间</dt>
                <dd>
                    <div class="input-date">
                        <asp:TextBox ID="txtSetupTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd'})"
                            datatype="/^^\d{4}\-\d{1,2}\-\d{1,2}$/" errormsg="请选择正确的日期" sucmsg=" " />
                        <i></i>
                    </div>
                </dd>
            </dl>
            <dl>
                <dt>注册资本</dt>
                <dd><asp:TextBox ID="txtRegisteredCapital" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">* </span></dd>
            </dl>
            <dl>
                <dt>经营范围</dt>
                <dd><asp:TextBox ID="txtBusinessScope" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">* </span></dd>
            </dl>
            <dl>
                <dt>经营状态</dt>
                <dd><asp:TextBox ID="txtBusinessStatus" runat="server" CssClass="input normal" datatype="/^\S+$/"></asp:TextBox><span class="Validform_checktip">* </span></dd>
            </dl>
            <dl>
                <dt>涉讼情况</dt>
                <dd><asp:TextBox ID="txtBusinessLawsuit" runat="server" CssClass="input normal" datatype="/^.*$/"></asp:TextBox></dd>
            </dl>
            <dl>
                <dt>年收入</dt>
                <dd><asp:TextBox ID="txtIncomeYearly" runat="server" CssClass="input normal" datatype="/^.*$/"></asp:TextBox></dd>
            </dl>
            <dl>
                <dt>净资产</dt>
                <dd><asp:TextBox ID="txtNetAssets" runat="server" CssClass="input normal" datatype="/^.*$/"></asp:TextBox></dd>
            </dl>
            <dl>
                <dt>备注</dt>
                <dd><asp:TextBox ID="txtCompanyRemark" runat="server" CssClass="input normal" datatype="/^.*$/"></asp:TextBox></dd>
            </dl>
        <% } %>
        <% } %>
    </div>
    <!--/内容-->
    <!--工具栏-->
    <div class="page-footer">
        <div class="btn-list">
            <asp:Button ID="btnSubmit" runat="server" Text="提交保存" CssClass="btn" OnClick="btnSubmit_Click" />
            <input name="btnReturn" type="button" value="返回上一页" class="btn yellow" onclick="javascript:history.back(-1);" />
        </div>
        <div class="clear">
        </div>
    </div>
    <!--/工具栏-->
    </form>
</body>
</html>
