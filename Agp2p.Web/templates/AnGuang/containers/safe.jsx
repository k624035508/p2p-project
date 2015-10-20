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
			<div className="personal-info-content pull-right">
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
								<span className="mail"></span>
								<span className="list-th">邮箱认证</span>
								<span className="list-tips">绑定邮箱，获取更多理财信息。</span>
								<span className="pull-right"><a href="#">立即认证</a></span>
							</li>
							<li>
								<span className="phone"></span>
								<span className="list-th">手机认证</span>
								<span className="list-tips">绑定手机，账户资金变动实时通知。</span>
								<span className="pull-right"><a href="#">修改</a></span>
							</li>
							<li>
								<span className="name"></span>
								<span className="list-th">实名认证</span>
								<span className="list-tips">保障账户资金安全，请使用本人身份证，提现时银行卡开户名与姓名一致。</span>
								<span className="pull-right"><a href="#">立即认证</a></span>
							</li>
							<li>
								<span className="psw-login"></span>
								<span className="list-th">登录密码</span>
								<span className="list-tips">定期更换密码让您的账户更安全。</span>
								<span className="pull-right"><a href="#">修改</a></span>
							</li>
							<li>
								<span className="psw-trade"></span>
								<span className="list-th">交易密码</span>
								<span className="list-tips">从平台账户提现时需要输入的密码。</span>
								<span className="pull-right"><a href="#">设置</a></span>
							</li>
						</ul>
					</div>
				</div>
			</div>
		);
	}
}