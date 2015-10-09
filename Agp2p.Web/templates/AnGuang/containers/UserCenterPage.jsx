import React from "react";
import { Link } from 'react-router'

export default class UserCenterPage extends React.Component {
	constructor(props) {
		super(props);
		this.state = {totalMoney: "", userName: "", prevLoginTime: "", idleMoney: "", lockedMoney: ""};
	}
	componentDidMount() {
		var $mountNode = $("#app");
		this.setState($mountNode.data());
	}
	render() {
		return (
			<div className="content-wrap usercenter">
			    <div className="nav-left">
			        <div className="total-account">
			            <p className="account-h">账户总额</p>
			            <p className="account-num">{this.state.totalMoney}<span>&nbsp;元</span></p>
			        </div>
			        <ul className="list-unstyled outside-ul">
			            <li><a href="#" className="account-link">账户总览</a></li>
			            <li><a className="funds">资金管理</a>
			                <ul className="list-unstyled inner-ul">
			                    <li><Link to="/mytrade" activeClassName="nav-active">交易明细</Link></li>
			                    <li><Link to="/recharge" activeClassName="nav-active">我要充值</Link></li>
			                    <li><Link to="/withdraw" activeClassName="nav-active">我要提现</Link></li>
			                </ul>
			            </li>
			            <li><a className="investing">投资管理</a>
			                <ul className="list-unstyled inner-ul">
			                    <li><a href="#">我的投资</a></li>
			                    <li><Link to="/invest-record" activeClassName="nav-active">投资记录</Link></li>
			                    <li><a href="#">回款计划</a></li>
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
			                <p className="balance">账户余额：<span>{this.state.idleMoney}</span></p>
			                <p className="balance-frozen">冻结金额： <span>{this.state.lockedMoney}</span></p>
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