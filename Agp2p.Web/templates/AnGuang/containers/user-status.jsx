import React from "react";
import { Link } from 'react-router'
import $ from "jquery";
import assign from "lodash/Object/assign"
import refreshUserInfo from "../actions/usercenter.js"
import { connect } from 'react-redux';

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

function mapStateToProps(state) {
	return {
		userName: state.userInfo.userName,
		prevLoginTime: state.userInfo.prevLoginTime,
		idleMoney: state.userInfo.idleMoney,
		lockedMoney: state.userInfo.lockedMoney,
	};
}

@connect(mapStateToProps)
export default class UserStatus extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	componentDidMount() {
		var $mountNode = $("#app");
		var nodeData = assign({}, $mountNode.data()); // 避免修改原 data
		nodeData.totalMoney = nodeData.totalMoney.toNum();
		nodeData.idleMoney = nodeData.idleMoney.toNum();
		nodeData.lockedMoney = nodeData.lockedMoney.toNum();
		this.props.dispatch(refreshUserInfo(nodeData));

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
                this.props.dispatch(refreshUserInfo(data));
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
	}
	render() {
		return (
			<div className="content-right pull-right">
		        <div className="overview-head">
		            <div className="head-left">
		                <p className="username">您好！ {this.props.userName}</p>
		                <p className="save-level">安全级别 <span className="level-icon low"></span></p>
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
