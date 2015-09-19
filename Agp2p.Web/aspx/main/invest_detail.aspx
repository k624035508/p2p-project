<%@ Page Language="C#" AutoEventWireup="true" Inherits="Lip2p.Web.UI.Page.project" ValidateRequest="false" %>
<%@ Import namespace="System.Collections.Generic" %>
<%@ Import namespace="System.Text" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Lip2p.Common" %>
<%@ Import namespace="Newtonsoft.Json.Linq" %>
<%@ Import namespace="Newtonsoft.Json" %>

<script runat="server">
override protected void OnInit(EventArgs e)
{

	/* 
		This page was created by Lip2p Template Engine at 2015/9/16 10:44:26.
		本页面代码由Lip2p模板引擎生成于 2015/9/16 10:44:26. 
	*/

	base.OnInit(e);
	StringBuilder templateBuilder = new StringBuilder(220000);
	templateBuilder.Append("\r\n<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <title>标书</title>\r\n    <!--[if lt IE 9]>\r\n    <script src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/js/ie8-polyfill.js\"></");
	templateBuilder.Append("script>\r\n    <script src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/js/html5shiv.min.js\"></");
	templateBuilder.Append("script>\r\n    <script src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/js/respond.min.js\"></");
	templateBuilder.Append("script>\r\n    <![endif]-->\r\n    <script src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/build/commons.bundle.js\"></");
	templateBuilder.Append("script>\r\n    <script src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/build/invest_detail.bundle.js\"></");
	templateBuilder.Append("script>\r\n</head>\r\n<body>\r\n<!--head-->\r\n");

	templateBuilder.Append("<div class=\"container-fluid top-bar-wrap\">\r\n    <div class=\"top-bar\">\r\n        <span class=\"top-bar-icon head-hot-line-icon\"></span>\r\n        <span class=\"head-hot-line-num\">客服热线：400-8989-089</span>\r\n        <span class=\"top-bar-icon weibo-icon\"></span>\r\n        <span class=\"top-bar-icon wechat-icon\"></span>\r\n\r\n        <div class=\"pull-right\">\r\n            <ul class=\"list-inline\">\r\n                <li><a href=\"#\">登录</a></li>\r\n                <li class=\"hr-line\">|</li>\r\n                <li><a href=\"#\">注册</a></li>\r\n                <li class=\"hr-line\">|</li>\r\n                <li><a href=\"#\">帮助</a></li>\r\n            </ul>\r\n        </div>\r\n    </div>\r\n</div>\r\n<div class=\"container-fluid nav-bar-wrap\">\r\n    <div class=\"nav-bar\">\r\n        <img src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/imgs/index/logo.png\" />\r\n        <nav class=\"pull-right\">\r\n            <ul class=\"list-inline in-header\">\r\n                <li><a href=\"/\">首页</a></li>\r\n                <li><a href=\"");
	templateBuilder.Append(linkurl("invest"));

	templateBuilder.Append("\">我要理财</a></li>\r\n                <li role=\"presentation\" id=\"myAccount\" class=\"dropdown\">\r\n                    <a class=\"dropdown-toggle\" data-toggle=\"dropdown\" href=\"#\" role=\"button\" aria-haspopup=\"true\" aria-expanded=\"false\">\r\n                        我的账户 <span class=\"caret\"></span></a>\r\n                    <ul class=\"dropdown-menu dropdown-menu-custom\">\r\n                        <li><a href=\"#\">账户总览</a></li>\r\n                        <li><a href=\"#\">交易明细</a></li>\r\n                        <li><a href=\"#\">回款计划</a></li>\r\n                        <li><a href=\"#\">安全中心</a></li>\r\n                        <li><a href=\"#\">银行卡管理</a></li>\r\n                    </ul>\r\n                </li>\r\n                <li><a href=\"#\">安全保障</a></li>\r\n                <li><a href=\"#\">关于我们</a></li>\r\n            </ul>\r\n        </nav>\r\n    </div>\r\n</div>");


	templateBuilder.Append("\r\n<!--head end-->\r\n\r\n<!--面包屑导航-->\r\n<div class=\"breadcrumbs\">\r\n    <a href=\"#\">我要理财</a>\r\n    <span> > </span>\r\n    <span>项目详情</span>\r\n</div>\r\n<!--面包屑导航 end-->\r\n\r\n<!--标书样式-->\r\n<div class=\"tender-content\">\r\n    <div class=\"tender-face\">\r\n        <div class=\"invest-title\">\r\n            <span class=\"invest-style-tab\">房贷宝</span>\r\n            <span>");
	templateBuilder.Append(Utils.ObjectToStr(projectModel.title));
	templateBuilder.Append("</span>\r\n            \r\n            ");
	if (projectModel.tag.ToString()=="1")
	{

	templateBuilder.Append("\r\n            <span class=\"invest-list-icon yue-icon\"></span>\r\n            ");
	}
	else if (projectModel.tag.ToString()=="2")
	{

	templateBuilder.Append("\r\n            <span class=\"invest-list-icon jian-icon\"></span>\r\n            ");
	}	//end for if

	templateBuilder.Append("\r\n            <span class=\"invest-list-icon xin-icon\"></span>\r\n        </div>\r\n        <div class=\"invest-rules\">\r\n            <div class=\"sum\">\r\n                <div>借款金额</div>\r\n                <div class=\"rules-style\">");
	templateBuilder.Append(string.Format("{0:N0}", projectModel.financing_amount).ToString());

	templateBuilder.Append("<span>元</span></div>\r\n            </div>\r\n            <div class=\"deadline\">\r\n                <div>期限</div>\r\n                <div class=\"rules-style\">");
	templateBuilder.Append(Utils.ObjectToStr(projectModel.repayment_term_span_count));
	templateBuilder.Append("<span>");
	templateBuilder.Append(Utils.GetLip2pEnumDes((Lip2p.Common.Lip2pEnums.ProjectRepaymentTermSpanEnum)projectModel.repayment_term_span).ToString());

	templateBuilder.Append("</span></div>\r\n            </div>\r\n            <div class=\"apr\">\r\n                <div>年化利率</div>\r\n                <div class=\"rules-style\">");
	templateBuilder.Append(string.Format("{0:0.0}", projectModel.profit_rate_year).ToString());

	templateBuilder.Append("<span>%</span></div>\r\n            </div>\r\n        </div>\r\n        <div class=\"repayment\">\r\n            <div class=\"repayment-way\">\r\n                <table>\r\n                    <tbody>\r\n                        <tr><td>还款方式</td><td colspan=\"2\">");
	templateBuilder.Append(Utils.GetLip2pEnumDes((Lip2p.Common.Lip2pEnums.ProjectRepaymentTypeEnum)projectModel.repayment_type).ToString());

	templateBuilder.Append("</td></tr>\r\n                        <tr><td>投标人数</td><td colspan=\"2\">");
	templateBuilder.Append(Utils.ObjectToStr(invsetorCount));
	templateBuilder.Append("人</td></tr>\r\n                        <tr><td>投标进度</td><td><div class=\"progress progress-custom\">\r\n                            <div class=\"progress-bar progress-bar-info\" role=\"progressbar\" aria-valuenow=\"");
	templateBuilder.Append(Utils.ObjectToStr(investmentProgress));
	templateBuilder.Append("\" aria-valuemin=\"0\" aria-valuemax=\"100\" style=\"width: ");
	templateBuilder.Append(Utils.ObjectToStr(investmentProgress));
	templateBuilder.Append("%\">\r\n                            </div>\r\n                        </div></td><td>");
	templateBuilder.Append(Utils.ObjectToStr(investmentProgress));
	templateBuilder.Append("%</td></tr>\r\n                    </tbody>\r\n                </table>\r\n            </div>\r\n            <div class=\"interest-way\">\r\n                <table>\r\n                    <tbody>\r\n                        <tr><td>计息方式</td><td>满标当天，立即计息</td></tr>\r\n                        <tr><td>发标时间</td><td>");
	templateBuilder.Append(string.Format("{0:yyyy-MM-dd HH:mm}",projectModel.invest_complete_time).ToString());

	templateBuilder.Append("</td></tr>\r\n                    </tbody>\r\n                </table>\r\n            </div>\r\n        </div>\r\n    </div>\r\n    <div class=\"invest-control pull-right\">\r\n        <div><span class=\"title-mark\">可投金额</span></div>\r\n        <div class=\"investable-amount-wrap\">\r\n            <div class=\"investable-amount\">");
	templateBuilder.Append(Utils.ObjectToStr(investmentBalance));
	templateBuilder.Append("</div>\r\n            ");
	if (projectModel.status>(int)Lip2pEnums.ProjectStatusEnum.FinancingTimeout)
	{

	templateBuilder.Append("\r\n            <!--满标-->\r\n            <div class=\"account-balance-empty\">\r\n                <div class=\"tips\">已经有");
	templateBuilder.Append(Utils.ObjectToStr(invsetorCount));
	templateBuilder.Append("名聪明的投资者投资此项目,敬请关注安广融合其他项目！</div>\r\n                <button type=\"button\" class=\"btn-common empty-btn\" disabled>抢 光 了</button>\r\n            </div>\r\n            ");
	}
	else if (!IsUserLogin())
	{

	templateBuilder.Append("\r\n            <!--未满标 未登录-->\r\n            <div class=\"account-balance-unlogin\">\r\n                <div>起投金额100元</div>\r\n                <button type=\"button\" class=\"btn-common register-btn\">30秒快速注册</button>\r\n                <span>已有账号？ <a href='");
	templateBuilder.Append(linkurl("login"));

	templateBuilder.Append("'>立即登录</a></span>\r\n            </div>\r\n            ");
	}
	else
	{

	templateBuilder.Append("\r\n            <!--未满标 登录后-->\r\n            <div class=\"account-balance\">\r\n                <div><span>账户余额 ");
	templateBuilder.Append(Utils.ObjectToStr(idle_money));
	templateBuilder.Append(" 元</span><a href='");
	templateBuilder.Append(linkurl("recharge"));

	templateBuilder.Append("' class=\"pull-right\">[充值]</a></div>\r\n                <input type=\"text\" placeholder=\"请输入投资金额\"/>\r\n                <button type=\"button\" class=\"btn-common investing-btn\">投 资</button>\r\n            </div>\r\n            ");
	}	//end for if

	templateBuilder.Append("\r\n        </div>\r\n    </div>\r\n</div>\r\n<!--标书样式 end-->\r\n\r\n<!--项目内容-->\r\n<div class=\"project-content-wrap clearfix\">\r\n    <div class=\"project-content-left\">\r\n        <div id=\"project-summary\">\r\n            <span class=\"title-mark\">1.项目简介</span>\r\n            <ul class=\"list-unstyled\">\r\n                <li><span>项目描述</span><span class=\"span-sec\">借款企业主营业务为矿产品、煤炭等产品的销售，借款用于采购铝矾土，销售给与其合作关系稳定的下游客户，采购的货物由监管公司进行监管，借款企业分批回款分批出货。该项目的风控要点在于对供应链的控制。</span></li>\r\n                <li><span>借款用途</span><span class=\"span-sec\">补充流动资金。</span></li>\r\n                <li><span>还款来源</span><span class=\"span-sec\">工资收入。</span></li>\r\n                <li><span>借款期限</span><span class=\"span-sec\">借款1个月，期限较短。</span></li>\r\n            </ul>\r\n        </div>\r\n        \r\n        <div id=\"borrower-info\">\r\n            <span class=\"title-mark\">2.借款人信息</span>\r\n            <div class=\"personal-info clearfix\">\r\n                <ul class=\"list-unstyled\">\r\n                    <li><span>姓&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;名</span><span>");
	templateBuilder.Append(loaner.dt_users.real_name.Substring(0, 1)+"**".ToString());

	templateBuilder.Append("</span></li>\r\n                    <li><span>出生年月</span><span>暂无数据</span></li>\r\n                    <li><span>月 收 入</span><span>");
	templateBuilder.Append(Utils.ObjectToStr(loaner.income));
	templateBuilder.Append("</span></li>\r\n                    <li><span>文化程度</span><span>");
	templateBuilder.Append(Utils.ObjectToStr(loaner.educational_background));
	templateBuilder.Append("</span></li>\r\n                </ul>\r\n                <ul class=\"list-unstyled\">\r\n                    <li><span>籍&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;贯</span><span>");
	templateBuilder.Append(Utils.ObjectToStr(loaner.dt_users.area));
	templateBuilder.Append("</span></li>\r\n                    <li><span>婚姻状况</span><span>");
	templateBuilder.Append(Lip2p.Common.Utils.GetLip2pEnumDes((Lip2p.Common.Lip2pEnums.MaritalStatusEnum)loaner.marital_status).ToString());

	templateBuilder.Append("</span></li>\r\n                    <li><span>房产数量</span><span>暂无数据</span></li>\r\n                    <li><span>是否购车</span><span>暂无数据</span></li>\r\n                </ul>\r\n                <ul class=\"list-unstyled\">\r\n                    <li><span>性&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;别</span><span>");
	templateBuilder.Append(loaner.dt_users.sex.ToString());

	templateBuilder.Append("</span></li>\r\n                    <li><span>逾期次数</span><span>暂无数据</span></li>\r\n                    <li><span>借款次数</span><span>暂无数据</span></li>\r\n                </ul>\r\n            </div>\r\n        </div>\r\n        \r\n        <div id=\"corporate-info\">\r\n            <span class=\"title-mark\">3.企业信息</span>\r\n            ");
	if (loaner_company==null)
	{

	templateBuilder.Append("\r\n            <div class=\"content-null\">暂无数据</div>\r\n            ");
	}
	else
	{

	templateBuilder.Append("\r\n            <ul class=\"list-unstyled\">\r\n                <li><span><span class=\"specific-location\">企业名称</span></span><span class=\"span-sec\">");
	templateBuilder.Append(Utils.ObjectToStr(loaner_company.name));
	templateBuilder.Append("</span></li>\r\n                <li><span><span class=\"specific-location\">成立时间</span></span><span class=\"span-sec\">");
	templateBuilder.Append(Utils.ObjectToStr(loaner_company.setup_time));
	templateBuilder.Append("</span></li>\r\n                <li><span><span class=\"specific-location\">注册资本</span></span><span class=\"span-sec\">");
	templateBuilder.Append(Utils.ObjectToStr(loaner_company.registered_capital));
	templateBuilder.Append("</span></li>\r\n                <li><span><span class=\"specific-location\">净 资 产</span></span><span class=\"span-sec\">");
	templateBuilder.Append(Utils.ObjectToStr(loaner_company.net_assets));
	templateBuilder.Append("</span></li>\r\n                <li><span><span class=\"specific-location\">经营范围</span></span><span class=\"span-sec\">");
	templateBuilder.Append(Utils.ObjectToStr(loaner_company.business_scope));
	templateBuilder.Append("</span></li>\r\n                <li><span><span class=\"specific-location\">经营状况</span></span><span class=\"span-sec\">");
	templateBuilder.Append(Utils.ObjectToStr(loaner_company.business_status));
	templateBuilder.Append("</span></li>\r\n                <li><span><span class=\"specific-location\">涉诉情况</span></span><span class=\"span-sec\">");
	templateBuilder.Append(Utils.ObjectToStr(loaner_company.business_lawsuit));
	templateBuilder.Append("</span></li>\r\n                <li><span><span class=\"specific-location\">年收入</span></span><span class=\"span-sec\">");
	templateBuilder.Append(Utils.ObjectToStr(loaner_company.income_yearly));
	templateBuilder.Append("</span></li>\r\n                <li><span><span class=\"specific-location\">备注</span></span><span class=\"span-sec\">");
	templateBuilder.Append(Utils.ObjectToStr(loaner_company.remark));
	templateBuilder.Append("</span></li>\r\n            </ul>\r\n            ");
	}	//end for if

	templateBuilder.Append("\r\n        </div>\r\n        \r\n        <div id=\"risk-control\">\r\n            <span class=\"title-mark\">4.风控措施</span>\r\n            <ul class=\"list-unstyled risk-control-content\">\r\n                <li><span><span class=\"specific-location\">风控描述</span></span><span class=\"span-sec\">");
	templateBuilder.Append(Utils.ObjectToStr(risk.risk_content));
	templateBuilder.Append("</span></li>\r\n                ");
	foreach(var mortgage in mortgages)
	{

	                var schemeObj = (JObject) JsonConvert.DeserializeObject(mortgage.li_mortgage_types.scheme);
	                var kv = (JObject)JsonConvert.DeserializeObject(mortgage.properties);
	
	                var properties = schemeObj.Cast
	                <KeyValuePair<string, JToken>>().Select(p => new Tuple<string,string>(p.Value.ToString(), kv[p.Key].ToString()))
	                .Concat(new [] {new Tuple<string,string>("市场价值",mortgage.valuation.ToString("c")) }).ToList();
	            foreach (var p in properties) {
	            

	templateBuilder.Append("\r\n            <li><span><span class=\"specific-location\">");
	templateBuilder.Append(p.Item1.ToString());

	templateBuilder.Append("</span></span><span class=\"span-sec\">");
	templateBuilder.Append(p.Item2.ToString());

	templateBuilder.Append("</span></li>\r\n            "); } 

	}	//end for if

	templateBuilder.Append("\r\n            </ul>\r\n        </div>\r\n        \r\n        <div id=\"relevant-info\">\r\n            <span class=\"title-mark\">5.相关资料</span>\r\n            <div class=\"row photo-cell\">\r\n                ");
	foreach(var picture in albums_pictures)
	{

	templateBuilder.Append("\r\n                <div class=\"col-md-3 col-xs-3\">\r\n                    <a href=\"javascript:;\" class=\"thumbnail thumbnail-custom\">\r\n                        <img src=\"");
	templateBuilder.Append(Utils.ObjectToStr(picture.thumb_path));
	templateBuilder.Append("\" origin-src=\"");
	templateBuilder.Append(Utils.ObjectToStr(picture.original_path));
	templateBuilder.Append("\" />\r\n                    </a>\r\n                    <div class=\"photo-title\">");
	templateBuilder.Append(Utils.ObjectToStr(picture.remark));
	templateBuilder.Append("</div>\r\n                </div>\r\n                ");
	}	//end for if

	foreach(var picture in albums_credit)
	{

	templateBuilder.Append("\r\n                <div class=\"col-md-3 col-xs-3\">\r\n                    <a href=\"javascript:;\" class=\"thumbnail thumbnail-custom\">\r\n                        <img src=\"");
	templateBuilder.Append(Utils.ObjectToStr(picture.thumb_path));
	templateBuilder.Append("\" origin-src=\"");
	templateBuilder.Append(Utils.ObjectToStr(picture.original_path));
	templateBuilder.Append("\" />\r\n                    </a>\r\n                    <div class=\"photo-title\">");
	templateBuilder.Append(Utils.ObjectToStr(picture.remark));
	templateBuilder.Append("</div>\r\n                </div>\r\n                ");
	}	//end for if

	foreach(var picture in albums_mortgage)
	{

	templateBuilder.Append("\r\n                <div class=\"col-md-3 col-xs-3\">\r\n                    <a href=\"javascript:;\" class=\"thumbnail thumbnail-custom\">\r\n                        <img src=\"");
	templateBuilder.Append(Utils.ObjectToStr(picture.thumb_path));
	templateBuilder.Append("\" origin-src=\"");
	templateBuilder.Append(Utils.ObjectToStr(picture.original_path));
	templateBuilder.Append("\" />\r\n                    </a>\r\n                    <div class=\"photo-title\">");
	templateBuilder.Append(Utils.ObjectToStr(picture.remark));
	templateBuilder.Append("</div>\r\n                </div>\r\n                ");
	}	//end for if

	templateBuilder.Append("\r\n            </div>\r\n        </div>\r\n        \r\n        <div id=\"invest-record\">\r\n            <span class=\"title-mark\">6.投标记录</span>\r\n            <div class=\"invest-record\">\r\n                <table class=\"table table-hover\">\r\n                    <thead>\r\n                        <tr><th>序号</th><th>用户名</th><th>投标金额</th><th>投标时间</th></tr>\r\n                    </thead>\r\n                    <tbody>\r\n                        ");
	int totalCount = 0;

	var investments = query_investment(projectModel,page-1,PageSize,out totalCount);

	for(int i=0;i                        <investments.Count;i++)
	{

	templateBuilder.Append("\r\n                            <tr><td>");
	templateBuilder.Append(Utils.ObjectToStr(i+1));
	templateBuilder.Append("</td><td class=\"account-style\">");
	templateBuilder.Append(investments[i].user_name.ToString());

	templateBuilder.Append("</td><td>");
	templateBuilder.Append(investments[i].value.ToString());

	templateBuilder.Append("</td><td>");
	templateBuilder.Append(investments[i].create_time.ToString());

	templateBuilder.Append("</td></tr>\r\n                            ");
	}	//end for if

	if (totalCount<1)
	{

	templateBuilder.Append("\r\n                                <tr><td align=\"center\" colspan=\"4\">暂无记录</td></tr>\r\n                                ");
	}	//end for if

	templateBuilder.Append("\r\n                    </tbody>\r\n                </table>\r\n                <div class=\"flickr\" style=\"text-align: center;\">");
	templateBuilder.Append(get_page_link(PageSize,page,totalCount,linkurl("invest_detail",project_id,"__id__")).ToString());

	templateBuilder.Append("</div> <!--放置页码列表-->\r\n            </div>\r\n        </div>\r\n    </div>\r\n\r\n    <div id=\"sidemenu\" class=\"project-content-right\">\r\n        <ul class=\"list-unstyled notScroll\">\r\n            <li><a href=\"#project-summary\"><span></span>1.项目简介</a></li>\r\n            <li><a href=\"#borrower-info\"><span></span>2.借款人信息</a></li>\r\n            <li><a href=\"#corporate-info\"><span></span>3.企业信息</a></li>\r\n            <li><a href=\"#risk-control\"><span></span>4.风控措施</a></li>\r\n            <li><a href=\"#relevant-info\"><span></span>5.相关资料</a></li>\r\n            <li><a href=\"#invest-record\"><span></span>6.投资记录</a></li>\r\n        </ul>\r\n    </div>\r\n</div>\r\n<!--项目内容 end-->\r\n\r\n<!--footer-->\r\n");

	templateBuilder.Append("<div class=\"container-fluid footer-wrap\">\r\n        <div class=\"footer-top\">\r\n            <div class=\"content-left\">\r\n                <ul class=\"list-unstyled list-inline\">\r\n                    <li><a href=\"#\">网站首页</a></li>\r\n                    <li><a href=\"#\">关于我们</a></li>\r\n                    <li><a href=\"#\">产品介绍</a></li>\r\n                    <li><a href=\"#\">帮助中心</a></li>\r\n                    <li><a href=\"#\">联系我们</a></li>\r\n                    <li><a href=\"#\">网站地图</a></li>\r\n                </ul>\r\n                <p class=\"contacts\">客户服务热线：400-999-6680 <span>投诉邮箱： agrhp2p@163.com</span></p>\r\n                <p class=\"links\">\r\n                    友情链接：\r\n                    <a href=\"#\">网贷之家</a>\r\n                    <a href=\"#\">网贷天眼</a>\r\n                    <a href=\"#\">融途网</a>\r\n                    <a href=\"#\">网贷中心</a>\r\n                    <a href=\"#\">网贷天下</a>\r\n                    <a href=\"#\">网贷中国</a>\r\n                    <a href=\"#\">网贷110</a>\r\n                    <a href=\"#\">一起网贷</a>\r\n                    <a href=\"#\">互联网金融知识社区</a>\r\n                </p>\r\n            </div>\r\n            <div class=\"qrcode pull-right\"></div>\r\n        </div>\r\n        <div class=\"footer-bottom\">2015 广东安广融合电子商务股份有限公司 粤ICP备14084776号-1</div>\r\n    </div>\r\n    <!--[if lt IE 8]>\r\n\r\n        <script>\r\n        document.body.innerHTML =\r\n    '<div class=\"ltie8\">您目前使用的浏览器版本过低，无法正常使用安广融合官网，我们推荐您使用<a href=\"http://browser.qq.com/\">极速QQ浏览器</a>或<a href=\"https://oss.aliyuncs.com/towerfiles/%E8%B0%B7%E6%AD%8C%E6%B5%8F%E8%A7%88%E5%99%A8%20%E7%A8%B3%E5%AE%9A%E7%89%88_23.0.1271.64.exe\">谷歌浏览器</a>等更棒的浏览器</div>'\r\n        </");
	templateBuilder.Append("script>\r\n\r\n        <style>\r\n            body { background: #f3f5e9; }\r\n        </style>\r\n    <![endif]-->");


	templateBuilder.Append("\r\n<!--footer end-->\r\n</body>\r\n</html>");
	Response.Write(templateBuilder.ToString());
}
</script>
