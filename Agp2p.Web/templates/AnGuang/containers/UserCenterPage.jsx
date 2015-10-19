﻿import React from "react";
import { Link } from 'react-router'
import $ from "jquery";
import { connect } from 'react-redux';

import StatusContainer from "../containers/user-status.jsx"
import MyAccountPage from "../containers/myaccount.jsx"

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

class UserCenterPage extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
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
			            <p className="account-num">{this.props.totalMoney < 1e6
			            	? this.props.totalMoney.format(2)
			            	: Math.floor(this.props.totalMoney).format()}<span>&nbsp;元</span></p>
			        </div>
			        <ul className="list-unstyled outside-ul">
			            <li><Link to="/myaccount" className="account-link">账户总览</Link></li>
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
			                    <li><Link to="/safe" activeClassName="active-link">安全中心</Link></li>
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
		        {this.props.children || <StatusContainer><MyAccountPage/></StatusContainer>}
			</div>
		);
	}
}

function mapStateToProps(state) {
	return {
		totalMoney: state.userInfo.totalMoney
	};
}

export default connect(mapStateToProps)(UserCenterPage);