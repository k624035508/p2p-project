import "bootstrap-webpack";

import "../less/head.less";
import "../less/repassword.less";
import "../less/footerSmall.less";

import React from "react"
import ReactDom from "react-dom"
import header from "./header.js"
import ImageVerifyCode from "../components/img-verify-code.jsx"
import { ajax } from "jquery"

class ResetPasswordPage extends React.Component {
	constructor(props) {
		super(props);
		this.state = {step: 1, tel: "", picCode: "", smsCode: "", newPwd: "", repeatPwd: ""};
	}
	getSMSVerifyCode() {
		ajax({
			url: "/tools/mobile_verify.ashx?act=sendCodeForResetPwd&mobile=" + this.state.tel + "&picCode=" + this.state.picCode,
			type: "get",
            dataType: "json",
            contentType: "application/json",
            data: "",
            success: function(result) {
                alert(result.msg);
            }.bind(this),
            error: function(xhr, status, err) {
            	alert(xhr.responseJSON.msg);
                console.error(url, status, err.toString());
            }.bind(this)
		});
	}
	verifySMSVerifyCode() {
		if (!this.state.newPwd) {
			alert("请先输入密码");
			return;
		}
		if (this.state.newPwd != this.state.repeatPwd) {
			alert("两次输入的密码不一致");
			return;
		}
		ajax({
			url: "/tools/mobile_verify.ashx?act=verifyForResetPwd&verifyCode=" + this.state.smsCode,
			type: "post",
            dataType: "json",
            data: {newPwd: this.state.newPwd},
            success: function(result) {
                alert(result.msg);
            	this.setState({step: 3});
            }.bind(this),
            error: function(xhr, status, err) {
            	alert(xhr.responseJSON.msg);
            	this.setState({step: 1, picCode: "", smsCode: "", newPwd:"", repeatPwd:""});
                console.error(url, status, err.toString());
            }.bind(this)
		});
	}
	render() {
		return (
			<div className={`content-wrap reset-pwd ${this.state.step < 2 ? "" : "step2-done"} ${this.state.step < 3 ? "" : "step3-done"}`}>
			    <div className="reset-pwd-th">重置密码</div>
			    <div className="ul-wrap">
			        <ul className="list-unstyled list-inline steps-list">
			            <li className="hr step1-hr"></li>
			            <li className="step1">1</li>
			            <li className="hr step2-hr"></li>
			            <li className="step2">2</li>
			            <li className="hr step3-hr"></li>
			            <li className="step3">3</li>
			            <li className="hr step4-hr"></li>
			        </ul>
			    </div>
			    <div className="steps-tips">
			        <span className="tips1">验证手机</span>
			        <span className="tips2">重置密码</span>
			        <span className="tips3">完成</span>
			    </div>
			    <div className="step-content">
			    	{this.state.step != 1 ? null :
			        <ul className="list-unstyled reset-step1">
			            <li><span>手机号</span><input type="text" className="phone-input" value={this.state.tel}
			            	onChange={ev => this.setState({tel: ev.target.value})}/></li>
			            <li><span>图形验证码</span><input type="text" className="pic-code-input" value={this.state.picCode}
			            	onChange={ev => this.setState({picCode: ev.target.value})}/><a href="javascript:"><ImageVerifyCode /></a></li>
			            <li><span>短信验证码</span><input type="text" className="sms-code-input" value={this.state.smsCode}
			            	onChange={ev => this.setState({smsCode: ev.target.value})}/><a href="javascript:"
			            		onClick={ev => this.getSMSVerifyCode()}>获取验证码</a></li>
			            <li><span></span><button type="button" onClick={ev => {
			            	if (this.state.picCode || this.state.smsCode) {
			            		this.setState({step: 2});
			            	} else {
			            		alert("请先填写验证码");
			            	}
			        	}}>下一步</button></li>
			        </ul>}
			    	{this.state.step != 2 ? null :
			        <ul className="list-unstyled reset-step2">
			            <li><span>新密码</span><input type="password" className="new-pwd" value={this.state.newPwd}
			            	onChange={ev => this.setState({newPwd: ev.target.value})}/></li>
			            <li><span>确认密码</span><input type="password" className="new-pwd2" value={this.state.repeatPwd}
			            	onChange={ev => this.setState({repeatPwd: ev.target.value})}/></li>
			            <li><span></span><button type="button" onClick={ev => this.verifySMSVerifyCode()}>确 认</button></li>
			        </ul>}
			    	{this.state.step != 3 ? null :
			        <div className="reset-step3">
			            <p><span className="succeed-icon"></span><span className="succeed-tips">重置密码成功！</span></p>
			            <p className="txt-style marginTop"><span>您的账号：{this.state.tel}</span></p>
			            <p className="txt-style"><span>请牢记您的新密码。</span></p>
			            <a href="/login.html" className="btn">返回登录</a>
			        </div>}
			    </div>
			    <div className="help-info">
			        验证过程如有其他问题，请致电客服：400-8989-089 <span className="service-time">服务时间：工作日 8:30—18:00</span>
			    </div>
			</div>
		);
	}
}

$(function () {
	header.setHeaderHighlight(2);
	ReactDom.render(<ResetPasswordPage />, document.getElementById("main"));
});