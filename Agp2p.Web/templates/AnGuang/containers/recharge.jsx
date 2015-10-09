import React from "react";
import $ from "jquery";

export default class RechargePage extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	render() { //我要充值 内容
		return (
			<div ref="root">
				<div className="bank-chose-th"><span>选择银行</span></div>
				<ul className="list-unstyled list-inline bank-select">
				    <li id="zhonghang"></li>
				    <li id="gonghang"></li>
				    <li id="jianhang"></li>
				    <li id="nonghang"></li>
				    <li id="zhaohang"></li>
				    <li id="youzheng"></li>
				    <li id="guangda"></li>
				    <li id="zhongxin"></li>
				    <li id="pufa"></li>
				    <li id="minsheng"></li>
				    <li id="guangfa"></li>
				    <li id="xingye"></li>
				    <li id="pingan"></li>
				    <li id="jiaohang"></li>
				    <li id="huaxia"></li>
				</ul>
				<div className="balance-recharge"><span>账户余额：</span>￥0.00</div>
				<div className="amount-recharge">
				    <span><i>*</i>充值金额：</span>
				    <input type="text"/>
				</div>
				<div className="rechargeBtn"><a href="#">确认充值</a></div>
				<div className="bank-chose-tips"><span>温馨提示</span></div>
				<div className="rechargeTips">
				    <p>1. 为保障账户及资金安全，请在充值前完成安全认证以及提现密码设置。</p>
				    <p>2. 本平台禁止洗钱、信用卡套现、虚假交易等行为，一经发现并确认，将终止该账户的使用。</p>
				    <p>3. 如果充值金额没有及时到账，请拨打客服电话：400-8989-089。</p>
				</div>
			</div>
		);
	}
	componentDidMount() {
		//充值选择银行
		var $bank = $(this.refs.root.getDOMNode()).find(".bank-select li");
		
		$bank.click(function(){
			$bank.find("img").remove();
			var img = document.createElement("img");
			img.src = TEMPLATE_PATH + "/imgs/usercenter/recharge-icons/selected.png";
			this.appendChild(img);
		});
	}
}