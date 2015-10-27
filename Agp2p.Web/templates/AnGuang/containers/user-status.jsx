import React from "react";
import { Link } from 'react-router'
import { fetchWalletAndUserInfo } from "../actions/usercenter.js"

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

class UserStatus extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	componentDidMount() {
		if (!this.props.alreadyFetchUserInfo) {
			this.props.dispatch(fetchWalletAndUserInfo());
		}
	}
	render() {
		return (
			<div className="content-right">
		        <div className="overview-head">
		            <div className="head-left">
		                <p className="username">您好！ {this.props.userName}</p>
		                <p className="save-level">安全级别 <Link className={"level-icon " + this.props.safeLevel}
		                	title="到 “个人中心 -> 安全中心” 进行相关设置以提高安全等级" to="/safe"></Link></p>
		                <p className="login-time">上次登录时间：{this.props.prevLoginTime}</p>
		            </div>
		            <div className="head-center">
		                <p className="balance">账户余额：<span>{this.props.idleMoney.format(2)}</span></p>
		                <p className="balance-frozen">冻结金额： <span>{this.props.lockedMoney.format(2)}</span></p>
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
		);
	}
}

function mapStateToProps(state) {
	var walletInfo = state.walletInfo, userInfo = state.userInfo;
	var safeLevelInt = (userInfo.realName ? 1 : 0) + (userInfo.email ? 1 : 0) + (userInfo.hasTransactPassword ? 1 : 0);
	return {
		userName: userInfo.userName,
		prevLoginTime: userInfo.prevLoginTime,
		idleMoney: walletInfo.idleMoney,
		lockedMoney: walletInfo.lockedMoney,
		alreadyFetchUserInfo: !!userInfo.invitationCode,
		safeLevel: safeLevelInt <= 1 ? "low" : (safeLevelInt == 2 ? "middle" : "high")
	};
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(UserStatus);