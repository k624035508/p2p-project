<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="loan_detail.aspx.cs" Inherits="Agp2p.Web.admin.project.loan_detail" %>

<%@ Import Namespace="Agp2p.Common" %>
<%@ Import Namespace="Agp2p.Linq2SQL" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>借款明细</title>
    <script type="text/javascript" src="../../scripts/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="../../scripts/datepicker/WdatePicker.js"></script>
    <script type="text/javascript" src="../../scripts/jquery/Validform_v5.3.2_min.js"></script>
    <script type="text/javascript" src="../../scripts/lhgdialog/lhgdialog.js?skin=idialog"></script>
    <script type="text/javascript" src="../../scripts/swfupload/swfupload.handlers.js"></script>
    <script type="text/javascript" src="../js/layout.js"></script>
    <link href="../skin/default/style.css" rel="stylesheet" type="text/css" />
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
            <a href="javascript:history.back(-1);" class="back"><i></i><span>返回列表页</span></a> <a href="../center.aspx" class="home"><i></i><span>首页</span></a>
            <i class="arrow"></i><span>借款明细</span>
        </div>
        <div class="line10">
        </div>
        <!--/导航栏-->
        <!--内容-->
        <div class="content-tab-wrap">
            <div id="floatHead" class="content-tab">
                <div class="content-tab-ul-wrap">
                    <ul>
                        <% %>
                        <li><a href="javascript:;" onclick="tabs(this);" class="selected">借款详细</a></li>
                        <li><a href="javascript:;" onclick="tabs(this);" >债权详细</a></li>
                    </ul>
                </div>
            </div>
        </div>
        <div class="tab-content">
            <dl>
                <dt>基本信息</dt>
                <dd>
                    <table border="0" cellspacing="0" cellpadding="0" class="border-table" width="98%">
                        <tr>
                            <th width="20%">借款类别
                            </th>
                            <td>
                                <div class="position">
                                    <span id="spa_category" runat="server"></span>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <th>借款主体
                            </th>
                            <td>
                                <span id="spa_type" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>借款标题
                            </th>
                            <td>
                                <span id="spa_title" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>借款编号
                            </th>
                            <td>
                                <span id="spa_no" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>借款合同编号
                            </th>
                            <td>
                                <span id="spa_contact_no" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>借款金额(元)
                            </th>
                            <td>
                                <span id="spa_amount" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>年化利率(%)
                            </th>
                            <td>
                                <span id="spa_profit_rate" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>还款期限
                            </th>
                            <td>
                                <span id="spa_repayment" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>还款方式
                            </th>
                            <td>
                                <span id="spa_repayment_type" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>平台服务费率(%)
                            </th>
                            <td>
                                <span id="spa_loan_fee" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>风险保证金费率(%)
                            </th>
                            <td>
                                <span id="spa_bond_fee" runat="server"></span>
                            </td>
                        </tr>
                        <% if (ProjectStatus > (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationSuccess)
                            { %>
                        <tr>
                            <th>借款标识
                            </th>
                            <td>
                                <span id="spa_tag" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>募集期限
                            </th>
                            <td>
                                <span id="spa_financing_day" runat="server"></span>天
                            </td>
                        </tr>
                        <% } %>
                        <tr>
                            <th>申请时间
                            </th>
                            <td>
                                <span id="spa_add_time" runat="server"></span>
                            </td>
                        </tr>
                        <% if (ProjectStatus > (int)Agp2pEnums.ProjectStatusEnum.FinancingApplicationSuccess)
                            { %>
                        <tr>
                            <th>发布时间
                            </th>
                            <td>
                                <span id="spa_publish_time" runat="server"></span>
                            </td>
                        </tr>
                        <% }
                            if (ProjectStatus > (int)Agp2pEnums.ProjectStatusEnum.FinancingSuccess)
                            { %>
                        <tr>
                            <th>放款时间
                            </th>
                            <td>
                                <span id="spa_make_loan_time" runat="server"></span>
                            </td>
                        </tr>
                        <% } %>
                    </table>
                </dd>
            </dl>

            <dl>
                <dt>借款人信息</dt>
                <dd>
                    <table border="0" cellspacing="0" cellpadding="0" class="border-table" width="98%">
                        <tr>
                            <th width="20%">姓名
                            </th>
                            <td>
                                <div class="position">
                                    <span id="sp_loaner_name" runat="server"></span>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <th>性别
                            </th>
                            <td>
                                <span id="sp_loaner_gender" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>职业
                            </th>
                            <td>
                                <span id="sp_loaner_job" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>工作所在地
                            </th>
                            <td>
                                <span id="sp_loaner_working_at" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>手机号码
                            </th>
                            <td>
                                <span id="sp_loaner_tel" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>身份证号码
                            </th>
                            <td>
                                <span id="sp_loaner_id_card_number" runat="server"></span>
                            </td>
                        </tr>
                    </table>
                </dd>
            </dl>
            <% if (LoanType == (int)Agp2pEnums.LoanTypeEnum.Company)
                { %>
            <dl>
                <dt>企业信息</dt>
                <dd>
                    <table border="0" cellspacing="0" cellpadding="0" class="border-table" width="98%">
                        <tr>
                            <th width="20%">企业名称
                            </th>
                            <td>
                                <div class="position">
                                    <span id="sp_company_name" runat="server"></span>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <th>成立时间
                            </th>
                            <td>
                                <span id="sp_company_setup_time" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>注册资本
                            </th>
                            <td>
                                <span id="sp_company_registered_capital" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>经营范围
                            </th>
                            <td>
                                <span id="sp_company_business_scope" runat="server"></span>
                            </td>
                        </tr>
                        <tr>
                            <th>经营状况
                            </th>
                            <td>
                                <span id="sp_company_business_status" runat="server"></span>
                            </td>
                        </tr>
                    </table>
                </dd>
            </dl>
            <% } %>
            <dl>
                <dt>抵押物信息</dt>
                <dd>
                    <asp:Repeater ID="rptList" runat="server">
                        <HeaderTemplate>
                            <table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
                                <tr>
                                    <th width="10%">选择</th>
                                    <th align="left">名称</th>
                                    <th align="left">类型</th>
                                    <th align="left">估值</th>
                                    <th align="left">状态</th>
                                </tr>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td align="center">
                                    <asp:CheckBox ID="chkId" CssClass="checkall" runat="server" Style="vertical-align: middle;" Checked='<%# Eval("check")%>' Enabled="False" />
                                    <asp:HiddenField ID="hidId" Value='<%#Eval("id")%>' runat="server" />
                                </td>
                                <td><%# Eval("name")%></td>
                                <td><%# Eval("typeName")%></td>
                                <td><%#string.Format("{0:c}", Eval("valuation"))%></td>
                                <td title="<%#Loan.QueryUsingProject(((Agp2p.BLL.loan.MortgageItem) Container.DataItem).id)%>"><%# Utils.GetAgp2pEnumDes((Agp2pEnums.MortgageStatusEnum)Convert.ToByte(Eval("status")))%></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            <%#rptList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"6\">暂无记录</td></tr>" : ""%>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </dd>
            </dl>
            <!-- 风控信息 -->
            <dl>
                <dt>风控信息</dt>
                <dd>
                    <table border="0" cellspacing="0" cellpadding="0" class="border-table" width="98%">
                        <% if (LoanType == (int)Agp2pEnums.LoanTypeEnum.Creditor)
                            { %>
                        <tr>
                            <th width="20%">债权人
                            </th>
                            <td>
                                <div class="position">
                                    <span id="spa_creditor" runat="server"></span>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <th width="20%">债权人描述
                            </th>
                            <td>
                                <div class="position">
                                    <span id="spa_creditorContent" runat="server"></span>
                                </div>
                            </td>
                        </tr>
                        <% } %>
                        <tr>
                            <th width="20%">借款描述
                            </th>
                            <td>
                                <div class="position">
                                    <span id="spa_loanerContent" runat="server"></span>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <th width="20%">借款用途
                            </th>
                            <td>
                                <div class="position">
                                    <span id="spa_loanUse" runat="server"></span>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <th width="20%">还款来源
                            </th>
                            <td>
                                <div class="position">
                                    <span id="spa_repaymentSource" runat="server"></span>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <th width="20%">风控描述
                            </th>
                            <td>
                                <div class="position">
                                    <span id="spa_txtRiskContent" runat="server"></span>
                                </div>
                            </td>
                        </tr>
                    </table>
                </dd>
            </dl>
            <dl>
                <dt>借款合同</dt>
                <dd>
                    <div class="upload-box upload-album">
                    </div>
                    <div class="photo-list">
                        <ul>
                            <asp:Repeater ID="rptLoanAgreement" runat="server">
                                <ItemTemplate>
                                    <li>
                                        <input type="hidden" name="hid_photo_name" value="<%# Eval("id") %>|<%#Eval("original_path")%>|<%#Eval("thumb_path")%>" />
                                        <input type="hidden" name="hid_photo_remark" value="<%#Eval("remark")%>" />
                                        <div class="img-box" onclick="getBigPic(this);">
                                            <img src="<%#Eval("thumb_path")%>" bigsrc="<%#Eval("original_path")%>" />
                                            <span class="remark"><i>
                                                <%#Eval("remark").ToString() == "" ? "暂无描述..." : Eval("remark").ToString()%></i></span>
                                        </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </div>
                </dd>
                <input type="hidden" name="hid_photo_name" value="splitter" />
                <input type="hidden" name="hid_photo_remark" value="splitter" />
            </dl>
            <dl>
                <dt>抵押合同</dt>
                <dd>
                    <div class="upload-box upload-album">
                    </div>
                    <div class="photo-list">
                        <ul>
                            <asp:Repeater ID="rptMortgageContracts" runat="server">
                                <ItemTemplate>
                                    <li>
                                        <input type="hidden" name="hid_photo_name" value="<%# Eval("id") %>|<%#Eval("original_path")%>|<%#Eval("thumb_path")%>" />
                                        <input type="hidden" name="hid_photo_remark" value="<%#Eval("remark")%>" />
                                        <div class="img-box" onclick="getBigPic(this);">
                                            <img src="<%#Eval("thumb_path")%>" bigsrc="<%#Eval("original_path")%>" />
                                            <span class="remark"><i>
                                                <%#Eval("remark").ToString() == "" ? "暂无描述..." : Eval("remark").ToString()%></i></span>
                                        </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </div>
                </dd>
                <input type="hidden" name="hid_photo_name" value="splitter" />
                <input type="hidden" name="hid_photo_remark" value="splitter" />
            </dl>
            <dl>
                <dt>他项证</dt>
                <dd>
                    <div class="upload-box upload-album">
                    </div>
                    <div class="photo-list">
                        <ul>
                            <asp:Repeater ID="rptLienCertificates" runat="server">
                                <ItemTemplate>
                                    <li>
                                        <input type="hidden" name="hid_photo_name" value="<%# Eval("id") %>|<%#Eval("original_path")%>|<%#Eval("thumb_path")%>" />
                                        <input type="hidden" name="hid_photo_remark" value="<%#Eval("remark")%>" />
                                        <div class="img-box" onclick="getBigPic(this);">
                                            <img src="<%#Eval("thumb_path")%>" bigsrc="<%#Eval("original_path")%>" />
                                            <span class="remark"><i>
                                                <%#Eval("remark").ToString() == "" ? "暂无描述..." : Eval("remark").ToString()%></i></span>
                                        </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </div>
                </dd>
                <input type="hidden" name="hid_photo_name" value="splitter" />
                <input type="hidden" name="hid_photo_remark" value="splitter" />
            </dl>
            <div runat="server" id="div_fabiao" visible="False">
                <dl>
                    <dt>借款标识</dt>
                    <dd>
                        <div class="rule-multi-radio" style="display: inline-block; float: left; margin-right: 10px;">
                            <asp:RadioButtonList ID="rblTag" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                            </asp:RadioButtonList>
                        </div>
                    </dd>
                </dl>
                <dl>
                    <dt>发布时间</dt>
                    <dd>
                        <div class="input-date">
                            <asp:TextBox ID="txtPublishTime" runat="server" CssClass="input date" onfocus="WdatePicker({dateFmt:'yyyy-MM-dd HH:mm:ss'})"
                                datatype="/^\s*$|^\d{4}\-\d{1,2}\-\d{1,2}\s{1}(\d{1,2}:){2}\d{1,2}$/" errormsg="请选择正确的日期"
                                sucmsg=" " />
                            <i></i>
                        </div>
                        <span class="Validform_checktip">不选择默认当前发布时间</span>
                    </dd>
                </dl>
                <dl>
                    <dt>募集期限</dt>
                    <dd>
                        <asp:TextBox ID="txt_financing_day" runat="server" CssClass="input small" datatype="n"
                            sucmsg=" "></asp:TextBox>
                        天
                    <asp:Label ID="financing_day_rate_tip" runat="server" CssClass="Validform_checktip" />
                    </dd>
                </dl>
            </div>
            <div runat="server" id="div_financing_add_day" visible="False">
                <dl visible="False">
                    <dt>募集顺延</dt>
                    <dd>
                        <asp:TextBox ID="txt_financing_add_day" runat="server" CssClass="input small" datatype="n"
                            sucmsg=" "></asp:TextBox>
                        天
                    <asp:Label ID="Label1" runat="server" CssClass="Validform_checktip" />
                    </dd>
                </dl>
            </div>
        </div>
        
        <div class="tab-content" style="display: none">
            <asp:Repeater ID="rptClaimList" runat="server">
            <HeaderTemplate>
                <table width="100%" border="0" cellspacing="0" cellpadding="0" class="ltable">
                    <tr>
                        <th width="6%">序号</th>
                        <th align="left" width="8%">所属用户</th>
                        <th align="left" width="6%">本金</th>
                        <th align="left" width="4%">状态</th>
                        <th align="left" width="12%">债权编号</th>
                        <th align="left" width="10%">创建时间</th>
                        <th align="left" width="10%">状态更新时间</th>
                        <th align="left" width="10%">关联活期项目</th>
                        <th align="left" width="6%">父债权</th>
                        <th align="left" width="10%">操作</th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td align="center"><%# Eval("id") %></td>
                    <td><%# GetFriendlyUserName(((li_claims) Container.DataItem).dt_users) %></td>
                    <td><%# ((li_claims) Container.DataItem).principal.ToString("c") %></td>             
                    <td><%# Utils.GetAgp2pEnumDes((Agp2pEnums.ClaimStatusEnum)((li_claims)Container.DataItem).status) %></td>
                    <td><%#Eval("number")%></td>
                    <td><%#Eval("createTime")%></td>
                    <td><%#Eval("statusUpdateTime")%></td>
                    <td><%# ((li_claims) Container.DataItem).profitingProjectId == ((li_claims) Container.DataItem).projectId ? "" : ((li_claims) Container.DataItem).li_projects1.title %></td>
                    <td><%# Eval("parentClaimId")%></td>
                    <td>
                        <asp:Button runat="server"
                            ID="btnBecomeTransferable"
                            Text="转为可转让债权"
                            UseSubmitBehavior="False"
                            CommandArgument='<%# Eval("id") %>'
                            OnClientClick="return ExeNoCheckPostBack(this.name, '确认将此债权转为可转让债权？');"
                            OnClick="btnBecomeTransferable_OnClick"
                            Visible="<%# ((li_claims) Container.DataItem).profitingProjectId != ((li_claims) Container.DataItem).projectId && ((li_claims) Container.DataItem).status == (int) Agp2pEnums.ClaimStatusEnum.Nontransferable %>" />
                    </td>            
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                <%#rptClaimList.Items.Count == 0 ? "<tr><td align=\"center\" colspan=\"10\">暂无记录</td></tr>" : ""%>
                </table>
            </FooterTemplate>
        </asp:Repeater>
        </div>
        <!--/内容-->

        <!--工具栏-->
        <div class="page-footer">
            <div class="btn-list">
                <asp:LinkButton runat="server"></asp:LinkButton>
                <asp:Button ID="btnAudit" runat="server" Text="通过" CssClass="btn" OnClientClick="return ExeNoCheckPostBack('btnAudit','是否确定审核通过？');" OnClick="btnAudit_OnClick" Visible="False" />
                <asp:Button ID="btnNotAudit" runat="server" Text="不通过" CssClass="btn" OnClientClick="return ExeNoCheckPostBack('btnNotAudit','是否确定审核不通过？');" OnClick="btnNotAudit_OnClick" Visible="False" />
                <asp:Button ID="btnApply" runat="server" Text="发布" CssClass="btn" OnClick="btnApply_OnClick" Visible="False" />
                <asp:Button ID="btnApplyOnTime" runat="server" Text="定时发布" CssClass="btn" OnClick="btnApplyOnTime_OnClick" Visible="False" />
                <asp:Button ID="btnDrop" runat="server" Text="撤销" OnClientClick="return ExeNoCheckPostBack('btnDrop','是否确定撤销该借款？');" CssClass="btn" OnClick="btnDrop_OnClick" Visible="False" />
                <asp:Button ID="btnMakeLoan" runat="server" Text="放款" CssClass="btn" OnClientClick="return ExeNoCheckPostBack('btnMakeLoan','是否确定放款给借款人？');" OnClick="btnMakeLoan_OnClick" Visible="False" />
                <asp:Button ID="btnFail" runat="server" Text="流标" CssClass="btn" OnClientClick="return ExeNoCheckPostBack('btnFail','是否确定流标操作，确定后会把所有资金退回给投资者！');" OnClick="btnFail_OnClick" Visible="False" />
                <asp:Button ID="btnActivate" runat="server" Text="激活" CssClass="btn" OnClientClick="return ExeNoCheckPostBack('btnActivate','是否确定重新激活该项目？');" OnClick="btnActivate_OnClick" Visible="False" />
                <asp:Button ID="btnCut" runat="server" Text="截标" CssClass="btn" OnClientClick="return ExeNoCheckPostBack('btnCut','是否确定对改项目进行截标？');" OnClick="btnCut_OnClick" Visible="False" />
                <input name="btnReturn" type="button" value="返回上一页" class="btn yellow"
                    onclick="location.href = 'javascript:history.back(-1);'" />
                <asp:Button runat="server" CssClass="btn" ID="btnExport" OnClick="btnExport_OnClick" Text="导出 Excel" />
            </div>
            <div class="clear">
            </div>
        </div>
        <!--/工具栏-->
    </form>
</body>
</html>
