<%@ Page Language="C#" AutoEventWireup="true" Inherits="Agp2p.Web.UI.Page.index" ValidateRequest="false" %>
<%@ Import namespace="System.Collections.Generic" %>
<%@ Import namespace="System.Text" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Agp2p.Common" %>

<script runat="server">
override protected void OnInit(EventArgs e)
{

	/* 
		This page was created by Agp2p Template Engine at 2015/9/19 14:36:41.
		本页面代码由Agp2p模板引擎生成于 2015/9/19 14:36:41. 
	*/

	base.OnInit(e);
	StringBuilder templateBuilder = new StringBuilder(220000);
	templateBuilder.Append("<!DOCTYPE html>\r\n<html lang=\"zh-Hans\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <title>首页</title>\r\n    <!-- HTML5 shim and Respond.js for IE8 support of HTML5 elements and media queries -->\r\n    <!--[if lt IE 9]>\r\n      <script src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/js/ie8-polyfill.js\"></");
	templateBuilder.Append("script>\r\n      <script src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/js/html5shiv.min.js\"></");
	templateBuilder.Append("script>\r\n      <script src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/js/respond.min.js\"></");
	templateBuilder.Append("script>\r\n    <![endif]-->\r\n    <script src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/build/commons.bundle.js\"></");
	templateBuilder.Append("script>\r\n    <script src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/build/index.bundle.js\"></");
	templateBuilder.Append("script>\r\n</head>\r\n<body>\r\n<!--head-->\r\n");

	templateBuilder.Append("<div class=\"container-fluid top-bar-wrap\">\r\n    <div class=\"top-bar\">\r\n        <span class=\"top-bar-icon head-hot-line-icon\"></span>\r\n        <span class=\"head-hot-line-num\">客服热线：400-8989-089</span>\r\n        <span class=\"top-bar-icon weibo-icon\"></span>\r\n        <span class=\"top-bar-icon wechat-icon\"></span>\r\n\r\n        <div class=\"pull-right\">\r\n            <ul class=\"list-inline\">\r\n                <li><a href=\"#\">登录</a></li>\r\n                <li class=\"hr-line\">|</li>\r\n                <li><a href=\"#\">注册</a></li>\r\n                <li class=\"hr-line\">|</li>\r\n                <li><a href=\"#\">帮助</a></li>\r\n            </ul>\r\n        </div>\r\n    </div>\r\n</div>\r\n<div class=\"container-fluid nav-bar-wrap\">\r\n    <div class=\"nav-bar\">\r\n        <img src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/imgs/index/logo.png\" />\r\n        <nav class=\"pull-right\">\r\n            <ul class=\"list-inline in-header\">\r\n                <li><a href=\"/\">首页</a></li>\r\n                <li><a href=\"");
	templateBuilder.Append(linkurl("invest"));

	templateBuilder.Append("\">我要理财</a></li>\r\n                <li role=\"presentation\" id=\"myAccount\" class=\"dropdown\">\r\n                    <a class=\"dropdown-toggle\" data-toggle=\"dropdown\" href=\"#\" role=\"button\" aria-haspopup=\"true\" aria-expanded=\"false\">\r\n                        我的账户 <span class=\"caret\"></span></a>\r\n                    <ul class=\"dropdown-menu dropdown-menu-custom\">\r\n                        <li><a href=\"#\">账户总览</a></li>\r\n                        <li><a href=\"#\">交易明细</a></li>\r\n                        <li><a href=\"#\">回款计划</a></li>\r\n                        <li><a href=\"#\">安全中心</a></li>\r\n                        <li><a href=\"#\">银行卡管理</a></li>\r\n                    </ul>\r\n                </li>\r\n                <li><a href=\"#\">安全保障</a></li>\r\n                <li><a href=\"#\">关于我们</a></li>\r\n            </ul>\r\n        </nav>\r\n    </div>\r\n</div>");


	templateBuilder.Append("\r\n<!--head end-->\r\n\r\n<!--banner-->\r\n<div class=\"container-fluid\">\r\n    <div id=\"bannerCarousel\" class=\"carousel slide\" data-ride=\"carousel\">\r\n        <!-- Indicators -->\r\n        <ol class=\"carousel-indicators\">\r\n            <li data-target=\"#bannerCarousel\" data-slide-to=\"0\" class=\"active\"></li>\r\n            <li data-target=\"#bannerCarousel\" data-slide-to=\"1\"></li>\r\n        </ol>\r\n\r\n        <!-- Wrapper for slides -->\r\n        <div class=\"carousel-inner\" role=\"listbox\">\r\n            <div class=\"item active\">\r\n                <img src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/imgs/index/bannerTest.jpg\" alt=\"Chania\">\r\n            </div>\r\n\r\n            <div class=\"item\">\r\n                <img src=\"");
	templateBuilder.Append("/templates/AnGuang");
	templateBuilder.Append("/imgs/index/bannerTest-02.jpg\" alt=\"Chania\">\r\n            </div>\r\n\r\n            <!-- Left and right controls -->\r\n            <a class=\"left carousel-control\" href=\"#bannerCarousel\" role=\"button\" data-slide=\"prev\">\r\n                <span class=\"glyphicon glyphicon-chevron-left\" aria-hidden=\"true\"></span>\r\n                <span class=\"sr-only\">Previous</span>\r\n            </a>\r\n            <a class=\"right carousel-control\" href=\"#bannerCarousel\" role=\"button\" data-slide=\"next\">\r\n                <span class=\"glyphicon glyphicon-chevron-right\" aria-hidden=\"true\"></span>\r\n                <span class=\"sr-only\">Next</span>\r\n            </a>\r\n        </div>\r\n    </div>\r\n</div>\r\n<!--banner end-->\r\n\r\n<!--announcement-->\r\n<div class=\"container-fluid announcement-wrap\">\r\n    <div class=\"announcement\">\r\n        <span class=\"announce-board-icon announce-icon\"></span>\r\n        ");
	var announce_repay = get_article_list("content", 43, 1, "").Rows.Cast<DataRow>().FirstOrDefault();

	templateBuilder.Append("\r\n        <span class=\"return-announce\">还款公告：\r\n            ");
	if (announce_repay!=null)
	{

	templateBuilder.Append("\r\n            <a href='");
	templateBuilder.Append(linkurl("article_content_show",Utils.ObjectToStr(announce_repay["id"]),Utils.ObjectToStr(announce_repay["category_id"])));

	templateBuilder.Append("'>\r\n            ");
	templateBuilder.Append(announce_repay["title"].ToString().Length>25?announce_repay["title"].ToString().Substring(0, 25)+"...":announce_repay["title"].ToString().ToString());

	templateBuilder.Append("\r\n            </a>\r\n            ");
	}
	else
	{

	templateBuilder.Append("\r\n            <span>暂无内容</span>\r\n            ");
	}	//end for if

	templateBuilder.Append("\r\n        </span>\r\n        <span class=\"announce-board-icon news-icon\"></span>\r\n        ");
	var announce54 = get_article_list("content_project", 54, 1, "").Rows.Cast<DataRow>().FirstOrDefault();

	templateBuilder.Append("\r\n        <span class=\"web-announce\">网站公告：\r\n            ");
	if (announce54!=null)
	{

	templateBuilder.Append("\r\n            <a href='");
	templateBuilder.Append(linkurl("article_invest_show",Utils.ObjectToStr(announce54["id"]),Utils.ObjectToStr(announce54["category_id"])));

	templateBuilder.Append("'>\r\n            ");
	templateBuilder.Append(announce54["title"].ToString().Length>30?announce54["title"].ToString().Substring(0, 30)+"...":announce54["title"].ToString().ToString());

	templateBuilder.Append("\r\n            </a>\r\n            ");
	}
	else
	{

	templateBuilder.Append("\r\n            <span>暂无内容</span>\r\n            ");
	}	//end for if

	templateBuilder.Append("\r\n        </span>\r\n        <span class=\"announce-board-icon news-icon\"></span>\r\n        <span class=\"pull-right\"><a href=\"#\">更多></a></span>\r\n    </div>\r\n</div>\r\n<!--announcement end-->\r\n\r\n<!--invest list-->\r\n<div class=\"container-fluid\">\r\n    ");
	foreach(DataRow dr in get_project_list(5,0,0,0,0).Rows)
	{

	templateBuilder.Append("\r\n    <div class=\"invest-cell\">\r\n        <div class=\"invest-title-wrap\">\r\n            <span class=\"invest-style-tab\">房贷宝</span>\r\n            <span class=\"invest-title\"><a href='");
	templateBuilder.Append(linkurl("invest_detail",Utils.ObjectToStr(dr["id"])));

	templateBuilder.Append("'>" + Utils.ObjectToStr(dr["title"]) + "</a></span>\r\n            ");
	if (dr["tag"].ToString()=="1")
	{

	templateBuilder.Append("\r\n            <span class=\"invest-list-icon yue-icon\"></span>\r\n            ");
	}
	else if (dr["tag"].ToString()=="2")
	{

	templateBuilder.Append("\r\n            <span class=\"invest-list-icon jian-icon\"></span>\r\n            ");
	}	//end for if

	templateBuilder.Append("\r\n            <span class=\"invest-list-icon xin-icon\"></span>\r\n        </div>\r\n        <div class=\"invest-content\">\r\n            <div class=\"apr\">\r\n                <div class=\"red25px margin-bottom10px\">" + Utils.ObjectToStr(dr["profit_rate_year"]) + "<span class=\"red15px\">%</span></div>\r\n                <div class=\"grey13px\">年化利率</div>\r\n            </div>\r\n            <div class=\"deadline\">\r\n                <div class=\"grey25px margin-bottom10px\">" + Utils.ObjectToStr(dr["repayment_number"]) + "<span class=\"grey15px\">" + Utils.ObjectToStr(dr["repayment_term"]) + "</span></div>\r\n                <div class=\"grey13px\">期限</div>\r\n            </div>\r\n            <div class=\"sum\">\r\n                <div class=\"grey25px margin-bottom10px\">" + Utils.ObjectToStr(dr["project_amount_str"]) + "<span class=\"grey15px\">万</span></div>\r\n                <div class=\"grey13px\">借款金额</div>\r\n            </div>\r\n            <div class=\"repayment\">\r\n                <div class=\"progress progress-custom\">\r\n                    <div class=\"progress-bar progress-bar-info\" role=\"progressbar\" aria-valuenow=\"" + Utils.ObjectToStr(dr["project_investment_progress"]) + "\"\r\n                         aria-valuemin=\"0\" aria-valuemax=\"100\" style=\"width: " + Utils.ObjectToStr(dr["project_investment_progress"]) + "%\">\r\n                        <span class=\"sr-only\">20% Complete</span>\r\n                    </div>\r\n                </div>\r\n                <div class=\"grey13px margin-bottom10px\">可投金额 : <span class=\"dark-grey13px\">" + Utils.ObjectToStr(dr["project_investment_balance"]) + "</span></div>\r\n                <div class=\"grey13px margin-bottom10px hidden\">投资人数 : <span class=\"dark-grey13px\">" + Utils.ObjectToStr(dr["project_investment_count"]) + "人</span></div>   <!--满标人数显示-->\r\n                <div class=\"grey13px\">到期还本付息</div>\r\n            </div>\r\n            <div class=\"invest-btn pull-right\">\r\n                ");
	if ((int)dr["status"]==(int)Agp2p.Common.Agp2pEnums.ProjectStatusEnum.FinancingAtTime)
	{

	templateBuilder.Append("\r\n                <button type=\"button\" class=\"invest-full-btn\" onclick=\"location.href='");
	templateBuilder.Append(linkurl("invest_detail",Utils.ObjectToStr(dr["id"])));

	templateBuilder.Append("'\">待发标</button>\r\n                ");
	}
	else if ((int)dr["status"]==(int)Agp2p.Common.Agp2pEnums.ProjectStatusEnum.Financing)
	{

	templateBuilder.Append("\r\n                <button type=\"button\" class=\"invest-now-btn\" onclick=\"location.href='");
	templateBuilder.Append(linkurl("invest_detail",Utils.ObjectToStr(dr["id"])));

	templateBuilder.Append("'\">立即投资</button>\r\n                ");
	}
	else if ((int)dr["status"]==(int)Agp2p.Common.Agp2pEnums.ProjectStatusEnum.FinancingTimeout||(int)dr["status"]==(int)Agp2p.Common.Agp2pEnums.ProjectStatusEnum.FinancingSuccess)
	{

	templateBuilder.Append("\r\n                <button type=\"button\" class=\"invest-full-btn\" onclick=\"location.href='");
	templateBuilder.Append(linkurl("invest_detail",Utils.ObjectToStr(dr["id"])));

	templateBuilder.Append("'\">募集结束</button>\r\n                ");
	}
	else if ((int)dr["status"]==(int)Agp2p.Common.Agp2pEnums.ProjectStatusEnum.ProjectRepaying)
	{

	templateBuilder.Append("\r\n                <button type=\"button\" class=\"invest-now-btn\" onclick=\"location.href='");
	templateBuilder.Append(linkurl("invest_detail",Utils.ObjectToStr(dr["id"])));

	templateBuilder.Append("'\">还款中</button>\r\n                ");
	}
	else if ((int)dr["status"]>=(int)Agp2p.Common.Agp2pEnums.ProjectStatusEnum.RepayCompleteIntime)
	{

	templateBuilder.Append("\r\n                <button type=\"button\" class=\"invest-full-btn\" onclick=\"location.href='");
	templateBuilder.Append(linkurl("invest_detail",Utils.ObjectToStr(dr["id"])));

	templateBuilder.Append("'\">已完成</button>\r\n                ");
	}	//end for if

	templateBuilder.Append("\r\n            </div>\r\n        </div>\r\n    </div>\r\n    ");
	}	//end for if

	templateBuilder.Append("\r\n    <div class=\"more-projects\">\r\n        <a href='");
	templateBuilder.Append(linkurl("invest"));

	templateBuilder.Append("'>查看更多投资理财项目</a>\r\n    </div>\r\n</div>\r\n<!--invest list end-->\r\n\r\n<!--dynamic boards-->\r\n<div class=\"container-fluid\">\r\n    <div class=\"dynamic-wrap\">\r\n        <!--公司动态-->\r\n        <div class=\"company-news\">\r\n            <div class=\"dynamic-title-wrap\">\r\n                <a href=\"#\">\r\n                    <span class=\"dynamic-title\">公司动态</span>\r\n                    <span class=\"link-mark pull-right\"></span>\r\n                </a>\r\n            </div>\r\n            <div class=\"news-wrap\">\r\n                ");
	var inc_news_ls = get_article_list("content", 42, 4, "").Rows.Cast<DataRow>();;

	var inc_news_first = inc_news_ls.FirstOrDefault();

	if (inc_news_first!=null)
	{

	templateBuilder.Append("\r\n                <a class=\"img-wrap\" href='");
	templateBuilder.Append(linkurl("article_content_show",Utils.ObjectToStr(inc_news_first["id"]),Utils.ObjectToStr(inc_news_first["category_id"])));

	templateBuilder.Append("'>\r\n                    <img src=\"" + Utils.ObjectToStr(inc_news_first["img_url"]) + "\" alt=\"\"/>\r\n                </a>\r\n                <div class=\"img-title\">" + Utils.ObjectToStr(inc_news_first["title"]) + "</div>\r\n                ");
	}
	else
	{

	templateBuilder.Append("\r\n                <div style=\"text-align: center\">暂无内容</div>\r\n                ");
	}	//end for if

	templateBuilder.Append("\r\n                <div class=\"news-list\">\r\n                    <ul class=\"list-unstyled\">\r\n                        ");
	foreach(DataRow dr in inc_news_ls.Skip(1))
	{

	templateBuilder.Append("\r\n                        <li>\r\n                            <span class=\"list-title-wrap\"><a href='");
	templateBuilder.Append(linkurl("article_content_show",Utils.ObjectToStr(dr["id"]),Utils.ObjectToStr(dr["category_id"])));

	templateBuilder.Append("' class=\"dark-grey14px\"><span class=\"list-mark\"></span>\r\n                                <span class=\"list-title\">");
	templateBuilder.Append(dr["title"].ToString());

	templateBuilder.Append("</span><span class=\"pull-right\">");
	templateBuilder.Append(string.Format("{0:yyyy-MM-dd}", dr["add_time"]).ToString());

	templateBuilder.Append("</span></a></span>\r\n                        </li>\r\n                        ");
	}	//end for if

	templateBuilder.Append("\r\n                    </ul>\r\n                </div>\r\n            </div>\r\n        </div>\r\n\r\n        <!--媒体报道-->\r\n        <div class=\"media-report\">\r\n            <div class=\"dynamic-title-wrap\">\r\n                <a href=\"#\">\r\n                    <span class=\"dynamic-title\">媒体报道</span>\r\n                    <span class=\"link-mark pull-right\"></span>\r\n                </a>\r\n            </div>\r\n            <div class=\"report-wrap\">\r\n                ");
	var media_dt_ls = get_article_list("content", 44, 4, "").Rows.Cast<DataRow>();;

	var media_dt_first = media_dt_ls.FirstOrDefault();

	if (media_dt_first!=null)
	{

	templateBuilder.Append("\r\n                <a class=\"img-wrap\" href='");
	templateBuilder.Append(linkurl("article_content_show",Utils.ObjectToStr(media_dt_first["id"]),Utils.ObjectToStr(media_dt_first["category_id"])));

	templateBuilder.Append("'>\r\n                    <img src=\"" + Utils.ObjectToStr(media_dt_first["img_url"]) + "\" alt=\"\"/>\r\n                </a>\r\n                <div class=\"img-title\">" + Utils.ObjectToStr(media_dt_first["title"]) + "</div>\r\n                ");
	}
	else
	{

	templateBuilder.Append("\r\n                <div style=\"text-align: center\">暂无内容</div>\r\n                ");
	}	//end for if

	templateBuilder.Append("\r\n                <div class=\"report-list\">\r\n                    <ul class=\"list-unstyled\">\r\n                        ");
	foreach(DataRow dr in media_dt_ls.Skip(1))
	{

	templateBuilder.Append("\r\n                        <li><span class=\"list-title-wrap\">\r\n                            <a href='");
	templateBuilder.Append(linkurl("article_content_show",Utils.ObjectToStr(dr["id"]),Utils.ObjectToStr(dr["category_id"])));

	templateBuilder.Append("' class=\"dark-grey14px\"><span class=\"list-mark\"></span>\r\n                                <span class=\"list-title\">");
	templateBuilder.Append(dr["title"].ToString());

	templateBuilder.Append("</span><span class=\"pull-right\">");
	templateBuilder.Append(string.Format("{0:yyyy-MM-dd}", dr["add_time"]).ToString());

	templateBuilder.Append("</span></a>\r\n                            </span>\r\n                        </li>\r\n                        ");
	}	//end for if

	templateBuilder.Append("\r\n                    </ul>\r\n                </div>\r\n            </div>\r\n        </div>\r\n\r\n        <!--网站公告-->\r\n        <div class=\"company-placard\">\r\n            <div class=\"dynamic-title-wrap\">\r\n                <a href=\"#\">\r\n                    <span class=\"dynamic-title\">公司公告</span>\r\n                    <span class=\"link-mark pull-right\"></span>\r\n                </a>\r\n            </div>\r\n            <ul class=\"list-unstyled\">\r\n                ");
	var announces_ls = get_article_list("content", 43, 10, "").Rows.Cast<DataRow>();;

	foreach(DataRow dr in announces_ls)
	{

	templateBuilder.Append("\r\n                <li><span class=\"list-title-wrap\"><a href='");
	templateBuilder.Append(linkurl("article_content_show",Utils.ObjectToStr(dr["id"]),Utils.ObjectToStr(dr["category_id"])));

	templateBuilder.Append("' class=\"dark-grey14px\"><span class=\"list-mark\"></span>\r\n                                                        <span class=\"list-title\">");
	templateBuilder.Append(dr["title"].ToString());

	templateBuilder.Append("</span><span class=\"pull-right\">");
	templateBuilder.Append(string.Format("{0:yyyy-MM-dd}", dr["add_time"]).ToString());

	templateBuilder.Append("</span>\r\n                                                        </a></span>\r\n                </li>\r\n                ");
	}	//end for if

	templateBuilder.Append("\r\n            </ul>\r\n        </div>\r\n    </div>\r\n</div>\r\n<!--dynamic boards end-->\r\n\r\n<!--floating icon-->\r\n<div class=\"floating-wrap\">\r\n    <a href=\"#\"><span id=\"floating-qq\"></span></a>\r\n    <a href=\"#\"><span id=\"floating-cal\" class=\"floating-icon cal-icon\"></span></a>\r\n    <a href=\"#\" id=\"floating-top-wrap\" style=\"display:none\"><span id=\"floating-top\" class=\"floating-icon top-icon\"></span></a>\r\n</div>\r\n<!--floating icon end-->\r\n\r\n<!--footer-->\r\n");

	templateBuilder.Append("<div class=\"container-fluid footer-wrap\">\r\n        <div class=\"footer-top\">\r\n            <div class=\"content-left\">\r\n                <ul class=\"list-unstyled list-inline\">\r\n                    <li><a href=\"#\">网站首页</a></li>\r\n                    <li><a href=\"#\">关于我们</a></li>\r\n                    <li><a href=\"#\">产品介绍</a></li>\r\n                    <li><a href=\"#\">帮助中心</a></li>\r\n                    <li><a href=\"#\">联系我们</a></li>\r\n                    <li><a href=\"#\">网站地图</a></li>\r\n                </ul>\r\n                <p class=\"contacts\">客户服务热线：400-999-6680 <span>投诉邮箱： agrhp2p@163.com</span></p>\r\n                <p class=\"links\">\r\n                    友情链接：\r\n                    <a href=\"#\">网贷之家</a>\r\n                    <a href=\"#\">网贷天眼</a>\r\n                    <a href=\"#\">融途网</a>\r\n                    <a href=\"#\">网贷中心</a>\r\n                    <a href=\"#\">网贷天下</a>\r\n                    <a href=\"#\">网贷中国</a>\r\n                    <a href=\"#\">网贷110</a>\r\n                    <a href=\"#\">一起网贷</a>\r\n                    <a href=\"#\">互联网金融知识社区</a>\r\n                </p>\r\n            </div>\r\n            <div class=\"qrcode pull-right\"></div>\r\n        </div>\r\n        <div class=\"footer-bottom\">2015 广东安广融合电子商务股份有限公司 粤ICP备14084776号-1</div>\r\n    </div>\r\n    <!--[if lt IE 8]>\r\n\r\n        <script>\r\n        document.body.innerHTML =\r\n    '<div class=\"ltie8\">您目前使用的浏览器版本过低，无法正常使用安广融合官网，我们推荐您使用<a href=\"http://browser.qq.com/\">极速QQ浏览器</a>或<a href=\"https://oss.aliyuncs.com/towerfiles/%E8%B0%B7%E6%AD%8C%E6%B5%8F%E8%A7%88%E5%99%A8%20%E7%A8%B3%E5%AE%9A%E7%89%88_23.0.1271.64.exe\">谷歌浏览器</a>等更棒的浏览器</div>'\r\n        </");
	templateBuilder.Append("script>\r\n\r\n        <style>\r\n            body { background: #f3f5e9; }\r\n        </style>\r\n    <![endif]-->");


	templateBuilder.Append("\r\n<!--footer end-->\r\n</body>\r\n</html>");
	Response.Write(templateBuilder.ToString());
}
</script>
