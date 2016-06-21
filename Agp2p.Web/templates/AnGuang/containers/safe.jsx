import React from "react";
import CityPicker from "../components/city-picker.jsx";
import DatePicker from "../components/date-picker.jsx";
import ImageVerifyCode from "../components/img-verify-code.jsx";
import { updateWalletInfo, updateUserInfo, updateUserInfoByName, fetchWalletAndUserInfo } from "../actions/usercenter.js";
import { ajax } from "jquery";

import "../less/safe.less";
import alert from "../components/tips_alert.js";
import confirm from "../components/tips_confirm.js";
import mask from "../js/mask.js";

let emptyIfNull = str => str == null ? "" : str;

class UserInfoEditor extends React.Component {
	constructor(props) {
		super(props);
		this.state = { editing: false, modified: false, saving: false };
	}
	onUserInfoModify(fieldName, value) {
		if (this.props[fieldName] == value) return;
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
		ajax({
			type: "POST",
			url: '/tools/submit_ajax.ashx?action=user_edit',
			data: {
				nickName: this.props.nickName,
				sex: this.props.sex,
				birthday: this.props.birthday,
				area: this.props.area,
				qq: this.props.qq,
				address: this.props.address
			},
			dataType: "json",
			success: data => {
				alert(data.msg);
				this.setState({saving: false, modified: false, editing: false});
			},
			error: () => {

			}
		});
	}
	render() {
		let area = emptyIfNull(this.props.area);
		return (
			<div className="personal-info">
				<div className="personal-info-th">
					<span>个人信息</span>
					<a href="javascript:;" onClick={ev => this.setState({editing: !this.state.editing })}
						className="pull-right">{this.state.editing ? "取消修改" : "修改信息"}</a>
				</div>
				<div className="personal-info-list">
					<ul className="list-unstyled list-inline">
						<li><span>用 户 名：</span>{ mask(this.props.userName) }</li>
						<li><span>昵　　称：</span>{ this.genInputBox("nickName") }</li>
						<li><span>姓　　名：</span>{ this.props.realName }</li>
						<li><span>性　　别：</span>{ this.state.editing
							? <form>{ ["保密", "男", "女"].map(v =>
								[<input type="radio" name="sex" value={v} key={v} checked={this.props.sex == v}
									onChange={ev => this.onUserInfoModify("sex", v)} />, v])}</form>
							: this.props.sex }</li>
						<li><span>邮箱地址：</span>{ mask(this.props.email) }</li>
						<li><span>出生日期：</span>{ this.state.editing
							? <DatePicker className="input-box" onBlur={ev => this.onUserInfoModify("birthday", ev.target.value)}
								defaultValue={this.props.birthday} />
							: this.props.birthday }</li>
						<li><span>QQ 号码：</span>{ this.genInputBox("qq") }</li>
						<li><span>所在城市：</span>{ this.state.editing
							? <CityPicker defaultValue={area.split(",")}
								onLocationChanged={(...args) => this.onUserInfoModify("area", [...args].join(","))} />
							: area.replace(/,/g, "")}</li>
					</ul>
				</div>
				{ this.state.modified ? <div className="btn-wrap"><a href="javascript:;" onClick={ev => this.saveUserInfo()}
					disabled={this.state.saving}>提 交</a></div> : null }
			</div>
		);
	}
}

class EmailBinding extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			bindingEmail: false, newEmail: "", bindingEmailPending: false,
		};
	}
	componentDidMount() {
		if (window.location.hash.indexOf("code=") != -1) {
			var cutIndex = window.location.hash.indexOf("?") + 1;
			this.bindEmail(window.location.hash.substr(cutIndex));
		}
	}
	bindingEmail() {
		this.setState({bindingEmailPending: true});
		ajax({
			dataType: "json",
			url: '/tools/email_verify.ashx?action=sendVerifyEmail&email=' + this.state.newEmail,
			success: data => {
				alert(data.msg);
				this.setState({bindingEmailPending: false});
			},
			error: jqXHR => {
				alert(jqXHR.responseJSON.msg);
				this.setState({bindingEmailPending: false});
			}
		});
	}
	bindEmail(hash) {
		ajax({
			dataType: "json",
			url: '/tools/email_verify.ashx?' + hash,
			success: data => {
				alert(data.msg);
				this.props.dispatch(fetchWalletAndUserInfo());
			},
			error: jqXHR => {
				alert(jqXHR.responseJSON.msg);
			}
		});
	}
	render() {
		return (
			<li>
				<div className="list-cell">
					<span className="mail"></span>
					<span className="list-th">邮箱认证</span>
					<span className="list-tips">绑定邮箱，获取更多理财信息。</span>
					<span className="pull-right"><a href="javascript:;" 
						onClick={ev => this.setState({bindingEmail: !this.state.bindingEmail})}>{this.props.email ? "重新绑定" : "立即认证"}</a></span>
				</div>
				{!this.state.bindingEmail ? null :
				<div className="setting-wrap" id="email-setting">
					<div className="cancel">
						<span className="th-setting">绑定邮箱</span>
						<span className="glyphicon glyphicon-remove pull-right cancel-btn"
							onClick={ev => this.setState({bindingEmail: false})}></span>
					</div>
					<div className="settings">
						<div className="form-group"><label htmlFor="email">原邮箱：</label>{ mask(this.props.email) || "（未绑定）" }</div>
						<div className="form-group">
							<label htmlFor="email">新邮箱：</label>
							<input type="text" id="email" onBlur={ev => this.setState({newEmail: ev.target.value})}
								disabled={this.state.bindingEmailPending} />
						</div>
						<div className="btn-wrap"><a href="javascript:;" onClick={ev => this.bindingEmail()}
							disabled={this.state.bindingEmailPending} >提 交</a></div>
					</div>
				</div>}
			</li>
		);
	}
}

class MobileBinding extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			bindingMobile: false, newMobile: "", bindingMobilePending: false,
			imgCode: "", smsCode: "", imgVerifyCodeSeed: 0
		};
	}
	getSMSVerifyCode() {
		ajax({
			dataType: "json",
			url: `/tools/mobile_verify.ashx?act=sendCodeForBindMobile&mobile=${this.state.newMobile}&picCode=${this.state.imgCode}`,
			success: data => {
				alert(data.msg);
			},
			error: jqXHR => {
				alert(jqXHR.responseJSON.msg);
			}
		});
	}
	bindMobile() {
		ajax({
			dataType: "json",
			url: '/tools/mobile_verify.ashx?act=verifyForBindMobile&verifyCode=' + this.state.smsCode,
			success: data => {
	            alert(data.msg);
				this.props.dispatch(fetchWalletAndUserInfo());
				this.setState({newMobile: "", imgCode: "", smsCode: "", bindingMobile: false});
				this.setState({bindingMobile: true}); // 清空输入框
			},
			error: jqXHR => {
				alert(jqXHR.responseJSON.msg);
			}
		});
	}
	render() {
		return (
			<li>
				<div className="list-cell">
					<span className="phone"></span>
					<span className="list-th">手机认证</span>
					<span className="list-tips">绑定手机，账户资金变动实时通知。</span>
					<span className="pull-right"><a href="javascript:;"
						onClick={ev => this.setState({bindingMobile: !this.state.bindingMobile})}>{this.props.mobile ? "修改" : "立即认证"}</a></span>
				</div>
				{!this.state.bindingMobile ? null :
				<div className="setting-wrap" id="phone-setting">
					<div className="cancel">
						<span className="th-setting">修改手机号</span>
						<span className="glyphicon glyphicon-remove pull-right cancel-btn"
							onClick={ev => this.setState({bindingMobile: false})}></span>
					</div>
					<div className="settings">
						<div className="oldPhone">
							<span>原手机号码：</span>
							<span className="phoneNum">{mask(this.props.mobile) || "（未绑定）"}</span>
						</div>
						<div className="form-group">
							<label htmlFor="phone">新手机号码：</label>
							<input type="text" id="phone" disabled={this.state.bindingMobilePending}
								onBlur={ev => this.setState({newMobile: ev.target.value})} />
						</div>
						<div className="code-group img-code-wrap">
							<label htmlFor="img-code">图形验证码：</label>
							<input type="text" id="img-code" disabled={this.state.bindingMobilePending}
								onBlur={ev => this.setState({imgCode: ev.target.value})} />
							<span><ImageVerifyCode seed={this.state.imgVerifyCodeSeed} /></span>
							<a href="javascript:" className="img-change-btn"
								onClick={ev => this.setState({imgVerifyCodeSeed: Math.random()})}>换一张</a>
						</div>
						<div className="code-group">
							<label htmlFor="sms-code">短信验证码：</label>
							<input type="text" id="sms-code" disabled={this.state.bindingMobilePending}
								onBlur={ev => this.setState({smsCode: ev.target.value})} />
							<a href="javascript:" className="sms-code-btn" onClick={ev => this.getSMSVerifyCode()}
								disabled={!this.state.imgCode}>获取验证码</a>
						</div>
						<div className="btn-wrap"><a href="javascript:" disabled={this.state.bindingMobilePending}
							onClick={ev => this.bindMobile()}>提 交</a></div>
					</div>
				</div>}
			</li>
		);
	}
}

class IdentityBinding extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			bindingIdCard: false, trueName: "", idCardNumber: ""
		};
	}
	componentWillReceiveProps(nextProps) {
		this.setState({trueName: nextProps.realName, idCardNumber: nextProps.idCardNumber});
	}
	bindIdentity() {
		confirm("身份资料填写后则不能再修改，是否确认？", () => {
			ajax({
				type: "POST",
				url: '/tools/submit_ajax.ashx?action=bind_idcard',
				data: {
					trueName: this.state.trueName,
					idCardNumber: this.state.idCardNumber,
				},
				dataType: "json",
				    beforeSend:function(XMLHttpRequest){ 
				        //在后台返回success之前显示loading图标
                        $(".btn-wrap a").css("background","#f2f2f2");
				    }, 
				success: data => {
					alert(data.msg);
                    $(".btn-wrap a").css("background","#ff414b");
					this.props.dispatch(fetchWalletAndUserInfo());
				},
				error: jqXHR => {
                    $(".btn-wrap a").css("background","#ff414b");
					alert(jqXHR.responseJSON.msg);
				}
			});
		});
	}
	render() {
		return (
			<li>    
				<div className="list-cell">
					<span className="name"></span>
					<span className="list-th">实名认证</span>
					<span className="list-tips">保障账户资金安全，请使用本人身份证，提现时银行卡开户名与姓名一致。</span>
					<span className="pull-right"><a href="javascript:" 
						onClick={ev => this.setState({bindingIdCard: !this.state.bindingIdCard})}>{this.props.realName ? "查看" : "立即认证"}</a></span>
				</div>
				{!this.state.bindingIdCard ? null :
				<div className="setting-wrap" id="name-setting">
					<div className="cancel">
						<span className="th-setting">实名认证</span>
						<span className="tips">为确保您的账户安全，每个身份证号只能绑定一个安广融合账号</span>
						<span className="glyphicon glyphicon-remove pull-right cancel-btn"
							onClick={ev => this.setState({bindingIdCard: false})}></span>
					</div>
					<div className="settings">
						<div className="form-group">
							<label htmlFor="email">真实姓名：</label>
							<input type="text" id="email" onBlur={ev => this.setState({trueName: ev.target.value})}
								defaultValue={this.props.realName} disabled={this.props.realName}/>
						</div>
						<div className="form-group">
							<label htmlFor="personalID">身份证号：</label>
							<input type="text" id="personalID" onBlur={ev => this.setState({idCardNumber: ev.target.value})}
								defaultValue={mask(this.props.idCardNumber)} disabled={this.props.idCardNumber} />
						</div>
						<div className="btn-wrap" style={this.props.realName ? {display: "none"} : null}><a href="javascript:"
							onClick={ev => this.bindIdentity()}>提 交</a></div>                        
					</div>
                    <div className="real-name-th"><span>温馨提示</span></div>
                    <p className="warmTip-realName">为了保障您的信息安全，请务必认真填写个人信息，若信息填写错误达3次以上，您的账户将被系统自动锁定，请及时与客服联系，谢谢。</p>
				</div>}
			</li>
		);
	}
}

class CustodyAccount extends React.Component {
	constructor(props) {
		super(props);	
            }
	render() {
		return (
			<li>
				<div className="list-cell">
					<span className="custody"></span>
					<span className="list-th">资金存管</span>
					<span className="list-tips">用户拥有独立的专用账户，交易资金第三方存管、监管</span>
                    <span className="pull-right">
                    {this.props.identityId? 
                    <a href={"/api/payment/sumapay/index.aspx?api=4"} target="_blank">查看</a> 
                    :
                    <a href={"/api/payment/sumapay/index.aspx?api=1"} target="_blank">设置</a>
	                    }
                    </span>
				</div>
			</li>
		);
			}
				}

class QuestionnaireResult extends React.Component {
    constructor(props) {
        super(props);
        }
        render() {
            return(
            <li>
                <div className="list-cell">
                    <span className="questionnaire-result"></span>
                    <span className="list-th">风险测评</span>
                    <span className="list-tips">测一测您是哪种类型的投资人</span>
                    <span className="pull-right">
                        {this.props.isQuestionnaire? 
                        <a href={"/questionnaire.html?action="+this.props.questionnaireScore} target="_blank">查看</a> 
                        : 
                        <a href={"/questionnaire.html"} target="_blank">测试</a>
                        }
                    </span>
                </div>
            </li>
                );
        }
}

class ResetLoginPassword extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			resetingLoginPassword: false,
			originalPassword: "",
			newPassword: "", newPassword2: ""
		};
	}
	resetLoginPassword() {
		if (this.state.newPassword != this.state.newPassword2) {
			alert("两次输入的密码不一致");
			return;
		}
		if (this.state.newPassword.length < 6) {
			alert("登录密码太短，请设置至少 6 位密码");
			return;
		}
		ajax({
			type: "POST",
			url: '/tools/submit_ajax.ashx?action=user_password_edit',
			data: {
				txtOldPassword: this.state.originalPassword,
				txtPassword: this.state.newPassword
			},
			dataType: "json",
			success: data => {
				alert(data.msg);
				if (data.status == 1) {
					this.setState({resetingLoginPassword: false});
					this.setState({resetingLoginPassword: true}); // 清空输入框
				}
			},
			error: jqXHR => {
				alert("操作失败，请重试");
			}
		});
	}
	render() {
		return (
			<li>
				<div className="list-cell">
					<span className="psw-login"></span>
					<span className="list-th">登录密码</span>
					<span className="list-tips">定期更换密码让您的账户更安全。</span>
					<span className="pull-right"><a href="javascript:"
						onClick={ev => this.setState({resetingLoginPassword: !this.state.resetingLoginPassword})}>修改</a></span>
				</div>
				{!this.state.resetingLoginPassword ? null :
				<div className="setting-wrap" id="pswLogin-setting">
					<div className="cancel">
						<span className="th-setting">修改登录密码</span>
						<span className="glyphicon glyphicon-remove pull-right cancel-btn"
							onClick={ev => this.setState({resetingLoginPassword: false})}></span>
					</div>
					<div className="settings">
						<div className="form-group">
							<label htmlFor="pswLogin">原密码：</label>
							<input type="password" id="pswLogin" onBlur={ev => this.setState({originalPassword: ev.target.value})} />
						</div>
						<div className="form-group">
							<label htmlFor="pswLogin-new">新密码：</label>
							<input type="password" id="pswLogin-new" onBlur={ev => this.setState({newPassword: ev.target.value})} />
						</div>
						<div className="form-group pswLogin-new-confirm">
							<label htmlFor="pswLogin-new2">确认新密码：</label>
							<input type="password" id="pswLogin-new2" onBlur={ev => this.setState({newPassword2: ev.target.value})} />
						</div>
						<div className="btn-wrap"><a href="javascript:" onClick={ev => this.resetLoginPassword()}>提 交</a></div>
					</div>
				</div>}
			</li>
		);
	}
}

class ResetTransactPassword extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			settingTransactionPassword: false,
			originalTransactPassword: "",
			newTransactPassword: "",
			newTransactPassword2: "",
		};
	}
	resetTransactPassword() {
		if (this.state.newTransactPassword != this.state.newTransactPassword2) {
			alert("两次输入的交易密码不同");
			return;
		}
		if (this.state.newTransactPassword.length < 6) {
			alert("交易密码太短，请设置至少 6 位密码");
			return;
		}
		ajax({
			type: "POST",
			url: '/tools/trade_pwd.ashx',
			data: {
				action: "modify",
				originalTransactPassword: this.state.originalTransactPassword,
				newTransactPassword: this.state.newTransactPassword
			},
			dataType: "json",
			success: data => {
				alert(data.msg);
				this.props.dispatch(fetchWalletAndUserInfo());
				this.setState({settingTransactionPassword: false});
				this.setState({settingTransactionPassword: true}); // 清空输入框
			},
			error: jqXHR => {
				alert(jqXHR.responseJSON.msg);
			}
		});
	}
	render() {
		return (
			<li>
				<div className="list-cell">
					<span className="psw-trade"></span>
					<span className="list-th">交易密码</span>
					<span className="list-tips">从平台账户提现时需要输入的密码。</span>
					<span className="pull-right">
						<a href="javascript:" style={this.props.hasTransactPassword ? null : {color: "red"}}
							onClick={ev => this.setState({settingTransactionPassword: !this.state.settingTransactionPassword})}>
							{this.props.hasTransactPassword ? "重新设置" : "设置"}</a>
					</span>
				</div>
				{!this.state.settingTransactionPassword ? null :
				<div className="setting-wrap" id="pswTrade-setting">
					<div className="cancel">
						<span className="th-setting">修改交易密码</span>
						<span className="tips">为确保您的账户安全，请设置交易密码与登录密码不同</span>
						<span className="glyphicon glyphicon-remove pull-right cancel-btn"
							onClick={ev => this.setState({settingTransactionPassword: false})}></span>
					</div>
					<div className="settings">
						{!this.props.hasTransactPassword ? null :
						<div className="form-group old-psw-trade">
							<label htmlFor="pswTrade">原交易密码：</label>
							<input type="password" id="pswTrade" onBlur={ev => this.setState({originalTransactPassword: ev.target.value})}
								disabled={!this.props.hasTransactPassword} />
							<a href="/forgot_password.html?forTransactPassword" >忘记密码？</a>
						</div>}
						<div className="form-group">
							<label htmlFor="pswTrade-new">新交易密码：</label>
							<input type="password" id="pswTrade-new" onBlur={ev => this.setState({newTransactPassword: ev.target.value})} />
						</div>
						<div className="form-group pswTrade-new-confirm">
							<label htmlFor="pswTrade-new2">确认新交易密码：</label>
							<input type="password" id="pswTrade-new2" onBlur={ev => this.setState({newTransactPassword2: ev.target.value})} />
						</div>
						<div className="btn-wrap"><a href="javascript:" onClick={ev => this.resetTransactPassword()} >提 交</a></div>
					</div>
				</div>}
			</li>
		);
	}
}

class SafeCenter extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	componentDidMount() {
		this.props.dispatch(fetchWalletAndUserInfo());
	}
	render() {
		return (
			<div className="personal-info-content"  >
                
				<UserInfoEditor {...this.props} />
	{!this.props.isLoaner ?
       <div className="safe-center"> 
                <div className="safe-center-th"><span>安全中心</span></div>
					<div className="setting-list">
						<ul className="list-unstyled">							
							<MobileBinding {...this.props} />
							<IdentityBinding {...this.props} />
                            <CustodyAccount {...this.props} />
                            <EmailBinding {...this.props} />
							<ResetLoginPassword {...this.props} />
                            <QuestionnaireResult {...this.props} />
                    {/*<ResetTransactPassword {...this.props} />*/}
                </ul>
					</div>
				   </div>
     : <div className="safe-center-loaner" > 
                <div className="safe-center-th"><span>安全中心</span></div>
					<div className="setting-list">
						<ul className="list-unstyled">							
							<MobileBinding {...this.props} />
							<IdentityBinding {...this.props} />
                            <CustodyAccount {...this.props} />
                            <EmailBinding {...this.props} />
							<ResetLoginPassword {...this.props} />
                            <QuestionnaireResult {...this.props} />
                {/*<ResetTransactPassword {...this.props} />*/}
    </ul>
					</div>
				   </div>  }
			</div>
		);
					}
                }

function mapStateToProps(state) {
	return state.userInfo;
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(SafeCenter);
