import React from "react";
import CityPicker from "../components/city-picker.jsx"
import DatePicker from "../components/date-picker.jsx"
import { updateWalletInfo, updateUserInfo, updateUserInfoByName } from "../actions/usercenter.js"
import { post } from "jquery";

class SafeCenter extends React.Component {
	constructor(props) {
		super(props);
		this.state = { editing: false, modified: false, saving: false };
	}
	onUserInfoModify(fieldName, value) {
		this.props.dispatch(updateUserInfoByName(fieldName, value));
		this.setState({modified: true});
	}
	genInputBox(fieldName) {
		return this.state.editing
			? <input className="input-box" type="text" onBlur={ ev => this.onUserInfoModify(fieldName, ev.target.value) }
				defaultValue={ this.props[fieldName] } />
			: this.props[fieldName];
	}
	saveUserInfo() {
		this.setState({saving: true});
		post('/tools/submit_ajax.ashx?action=user_edit', {
			nickName: this.props.nickName,
			sex: this.props.sex,
			birthday: this.props.birthday,
			area: this.props.area,
			qq: this.props.qq,
			address: this.props.address
		}, function(data) {
			alert(data.msg);
			this.setState({saving: false, modified: false, editing: false});
		}.bind(this), "json");
	}
	componentDidMount() {
		this.fetchUserInfo();
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
                this.props.dispatch(updateWalletInfo(data.walletInfo));
                this.props.dispatch(updateUserInfo(data.userInfo));
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
	}
	render() {
		return (
			<div className="personal-info-content">
				<div className="personal-info">
					<div className="personal-info-th">
						<span>个人信息</span>
						<a href="javascript:;" onClick={ev => this.setState({editing: !this.state.editing })}
							className="pull-right">{this.state.editing ? "取消修改" : "修改"}</a>
					</div>
					<div className="personal-info-list">
						<ul className="list-unstyled list-inline">
							<li><span>用户名：</span>{ this.props.userName }</li>
							<li><span>昵称：</span>{ this.genInputBox("nickName") }</li>
							<li><span>姓名：</span>{ this.props.realName }</li>
							<li><span>性别：</span>{ this.state.editing
								? <form>{ ["保密", "男", "女"].map(v =>
									[<input type="radio" name="sex" value={v} key={v} checked={this.props.sex == v}
										onChange={ev => this.onUserInfoModify("sex", v)} />, v])}</form>
								: this.props.sex }</li>
							<li><span>邮箱地址：</span>{ this.props.email }</li>
							<li><span>出生日期：</span>{ this.state.editing
								? <DatePicker className="input-box" onBlur={ev => this.onUserInfoModify("birthday", ev.target.value)}
									defaultValue={this.props.birthday} />
								: this.props.birthday }</li>
							<li><span>QQ号码：</span>{ this.genInputBox("qq") }</li>
							<li><span>所在城市：</span>{ this.state.editing
								? <CityPicker defaultValue={this.props.area.split(",")}
									onLocationChanged={(...args) => this.onUserInfoModify("area", [...args].join(","))} />
								: this.props.area.replace(/,/g, "")}</li>
						</ul>
					</div>
					{ this.state.modified ? <div className="btn-wrap"><a href="javascript:;" onClick={ev => this.saveUserInfo()}
						disabled={this.state.saving}>提 交</a></div> : null }
				</div>
				<div className="safe-center">
					<div className="safe-center-th"><span>安全中心</span></div>
					<div className="setting-list">
						<ul className="list-unstyled">
							<li>
								<div className="list-cell">
									<span className="mail"></span>
									<span className="list-th">邮箱认证</span>
									<span className="list-tips">绑定邮箱，获取更多理财信息。</span>
									<span className="pull-right"><a href="#">立即认证</a></span>
								</div>
								<div className="mail-setting-wrap">
									<div className="cancel"><span className="th-setting">绑定邮箱</span><span className="glyphicon glyphicon-remove pull-right cancel-btn"></span></div>
									<div className="mail-setting">
										<div className="form-group">
											<label htmlFor="email">您的邮箱：</label>
											<input type="text" id="email" />
										</div>
										<div className="btn-wrap"><a href="#">提 交</a></div>
									</div>
								</div>
							</li>
							<li>
								<div className="list-cell">
									<span className="phone"></span>
									<span className="list-th">手机认证</span>
									<span className="list-tips">绑定手机，账户资金变动实时通知。</span>
									<span className="pull-right"><a href="#">修改</a></span>
								</div>
							</li>
							<li>
								<div className="list-cell">
									<span className="name"></span>
									<span className="list-th">实名认证</span>
									<span className="list-tips">保障账户资金安全，请使用本人身份证，提现时银行卡开户名与姓名一致。</span>
									<span className="pull-right"><a href="#">立即认证</a></span>
								</div>
							</li>
							<li>
								<div className="list-cell">
									<span className="psw-login"></span>
									<span className="list-th">登录密码</span>
									<span className="list-tips">定期更换密码让您的账户更安全。</span>
									<span className="pull-right"><a href="#">修改</a></span>
								</div>
							</li>
							<li>
								<div className="list-cell">
									<span className="psw-trade"></span>
									<span className="list-th">交易密码</span>
									<span className="list-tips">从平台账户提现时需要输入的密码。</span>
									<span className="pull-right"><a href="#">设置</a></span>
								</div>
							</li>
						</ul>
					</div>
				</div>
			</div>
		);
	}
}

function mapStateToProps(state) {
	return {...state.userInfo};
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(SafeCenter);
