import React from "react";
import { Link } from 'react-router'
import $ from "jquery";
import assign from "lodash/Object/assign"

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


export default class UserStatus extends React.Component {
	constructor(props) {
		super(props);
		this.state = {totalMoney: 0, userName: "", prevLoginTime: "", idleMoney: 0, lockedMoney: 0};
	}
	componentDidMount() {
		var $mountNode = $("#app");
		var nodeData = assign({}, $mountNode.data()); // 避免修改原 data
		nodeData.totalMoney = nodeData.totalMoney.toNum();
		nodeData.idleMoney = nodeData.idleMoney.toNum();
		nodeData.lockedMoney = nodeData.lockedMoney.toNum();
		this.setState(nodeData);
		this.directUpdateDom(nodeData);

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
	directUpdateDom(data) {
        // totalMoney 暂时需要手动通过 jquery 修改
        var totalMoneyStr = data.totalMoney < 1e6 ? data.totalMoney.format(2) : Math.floor(data.totalMoney).format();
        $("#totalMoney").text(totalMoneyStr);
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
                this.directUpdateDom(data);
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
		);
	}
}