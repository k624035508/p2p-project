import React from "react";
import CityPicker from "../components/city-picker.jsx"
import DatePicker from "../components/date-picker.jsx"

export default class SafeCenter extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	render() {
		return (
			<div className="personal-info-content">
				<div className="personal-info">
					<div className="personal-info-th">
						<span>个人信息</span>
						<a href="#" className="pull-right">取消修改</a>
					</div>
					<div className="personal-info-list">
						<ul className="list-unstyled list-inline">
							<li><span>用户名：</span><input className="input-box" type="text"/></li>
							<li><span>昵称：</span><input className="input-box" type="text"/></li>
							<li><span>姓名：</span><input className="input-box" type="text"/></li>
							<li><span>性别：</span><form>
								<input type="radio" name="sex" value="male" /> 男
								<input type="radio" name="sex" value="female" /> 女
							</form></li>
							<li><span>邮箱地址：</span><input className="input-box" type="text"/></li>
							<li><span>出生日期：</span><DatePicker className="input-box" onTimeChange={time => null} /></li>
							<li><span>QQ号码：</span><input className="input-box" type="text"/></li>
							<li><span>所在城市：</span><CityPicker onLocationChanged={(...args) => this.setState({selectedLocation: [...args]})} /></li>
						</ul>
					</div>
					<div className="btn-wrap"><a href="#">提 交</a></div>
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
								<div className="setting-wrap" id="email-setting">
									<div className="cancel">
										<span className="th-setting">绑定邮箱</span>
										<span className="glyphicon glyphicon-remove pull-right cancel-btn"></span>
									</div>
									<div className="settings">
										<div className="form-group">
											<label for="email">您的邮箱：</label>
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
								<div className="setting-wrap" id="phone-setting">
									<div className="cancel">
										<span className="th-setting">修改手机号</span>
										<span className="glyphicon glyphicon-remove pull-right cancel-btn"></span>
									</div>
									<div className="settings">
										<div className="oldPhone">
											<span>原手机号码：</span>
											<span className="phoneNum">13590609455</span>
										</div>
										<div className="form-group">
											<label for="phone">新手机号码：</label>
											<input type="text" id="phone" />
										</div>
										<div className="code-group img-code-wrap">
											<label for="img-code">图形验证码：</label>
											<input type="text" id="img-code" />
											<span>图形验证码</span>
											<a href="#" className="img-change">换一张</a>
										</div>
										<div className="code-group">
											<label for="sms-code">短信验证码：</label>
											<input type="text" id="sms-code" />
											<a href="#" className="sms-code-btn">获取验证码</a>
										</div>
										<div className="btn-wrap"><a href="#">提 交</a></div>
									</div>
								</div>
							</li>
							<li>
								<div className="list-cell">
									<span className="name"></span>
									<span className="list-th">实名认证</span>
									<span className="list-tips">保障账户资金安全，请使用本人身份证，提现时银行卡开户名与姓名一致。</span>
									<span className="pull-right"><a href="#">立即认证</a></span>
								</div>
								<div className="setting-wrap" id="email-setting">
									<div className="cancel">
										<span className="th-setting">实名认证</span>
										<span className="tips">为确保您的战虎安全，每个身份证号只能绑定一个安广融合账号</span>
										<span className="glyphicon glyphicon-remove pull-right cancel-btn"></span>
									</div>
									<div className="settings">
										<div className="form-group">
											<label for="email">真实姓名：</label>
											<input type="text" id="email" />
										</div>
										<div className="form-group">
											<label for="personalID">身份证号：</label>
											<input type="text" id="personalID" />
										</div>
										<div className="btn-wrap"><a href="#">提 交</a></div>
									</div>
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