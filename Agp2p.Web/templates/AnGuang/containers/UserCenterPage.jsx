import React from "react";
import { Link } from 'react-router'

/**
 * Number.prototype.format(n, x)
 * 
 * @param integer n: length of decimal
 * @param integer x: length of sections
 */
Number.prototype.format = function(n, x) {
    var re = '\\d(?=(\\d{' + (x || 3) + '})+' + (n > 0 ? '\\.' : '$') + ')';
    return this.toFixed(Math.max(0, ~~n)).replace(new RegExp(re, 'g'), '$&,');
};

String.prototype.toNum = function () {
	return parseFloat(this.replace(/[^0-9\.]+/g,""));
}

export default class UserCenterPage extends React.Component {
	constructor(props) {
		super(props);
		this.state = {totalMoney: 0, userName: "", prevLoginTime: "", idleMoney: 0, lockedMoney: 0};
	}
	componentDidMount() {
		var $mountNode = $("#app");
		var nodeData = $mountNode.data();
		nodeData.totalMoney = nodeData.totalMoney.toNum();
		nodeData.idleMoney = nodeData.idleMoney.toNum();
		nodeData.lockedMoney = nodeData.lockedMoney.toNum();
		this.setState(nodeData);

		// 得到焦点自动刷新余额
		var _this = this, prevFetchTime = 0;
		window.onfocus = function () { 
			var fetchAt = new Date().getTime();
			if (30000 < fetchAt - prevFetchTime) {
				prevFetchTime = fetchAt;
				_this.fetchUserInfo();
			}
		};
	}
	fetchUserInfo() {
		let url = USER_CENTER_ASPX_PATH + "/AjaxQueryUserInfo"
		$.ajax({
            type: "get",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: "",
            success: function(result) {
                let data = JSON.parse(result.d);
                this.setState(data);
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
	}
	componentDidUpdate() {
		$(".inner-ul li.nav-active").removeClass("nav-active");
		$(".inner-ul li:has(> a.active-link)").addClass("nav-active");
	}
	render() {
		return (
			<div className="content-wrap usercenter">
			    <div className="nav-left">
			        <div className="total-account">
			            <p className="account-h">账户总额</p>
			            <p className="account-num">{this.state.totalMoney < 1e6 ? this.state.totalMoney.format(2) : Math.floor(this.state.totalMoney).format() }<span>&nbsp;元</span></p>
			        </div>
			        <ul className="list-unstyled outside-ul">
			            <li><a href="#" className="account-link">账户总览</a></li>
			            <li><a className="funds">资金管理</a>
			                <ul className="list-unstyled inner-ul">
			                    <li><Link to="/mytrade" activeClassName="active-link">交易明细</Link></li>
			                    <li><Link to="/recharge" activeClassName="active-link">我要充值</Link></li>
			                    <li><Link to="/withdraw" activeClassName="active-link">我要提现</Link></li>
			                </ul>
			            </li>
			            <li><a className="investing">投资管理</a>
			                <ul className="list-unstyled inner-ul">
			                    <li><a href="#">我的投资</a></li>
			                    <li><Link to="/invest-record" activeClassName="active-link">投资记录</Link></li>
			                    <li><Link to="/myrepayments" activeClassName="active-link">回款计划</Link></li>
			                </ul>
			            </li>
			            <li><a className="account">账户管理</a>
			                <ul className="list-unstyled inner-ul">
			                    <li><a href="#">我的信息</a></li>
			                    <li><a href="#">银行账户</a></li>
			                    <li><a href="#">推荐奖励</a></li>
			                    <li><a href="#">我的奖券</a></li>
			                </ul>
			            </li>
			            <li><a className="news">消息管理</a>
			                <ul className="list-unstyled inner-ul">
			                    <li><a href="#">我的消息</a></li>
			                    <li><a href="#">通知设置</a></li>
			                </ul>
			            </li>
			        </ul>
			    </div>
			    <div className="content-right pull-right">
			        <div className="overview-head">
			            <div className="head-left">
			                <p className="username">您好！ {this.state.userName}</p>
			                <p className="save-level">安全级别 <span className="level-icon low"></span></p>
			                <p className="login-time">上次登录时间：{this.state.prevLoginTime}</p>
			            </div>
			            <div className="head-center">
			                <p className="balance">账户余额：<span>{this.state.idleMoney.format(2)}</span></p>
			                <p className="balance-frozen">冻结金额： <span>{this.state.lockedMoney.format(2)}</span></p>
			            </div>
			            <div className="head-right pull-right">
			            	<Link to="/recharge" className="recharge">充 值</Link>
			            	<Link to="/withdraw" className="withdraw">提 现</Link>
			            </div>
			        </div>

			        <div className="content-body">
			        {this.props.children}
			        </div>
			    </div>
			</div>
		);
	}
}