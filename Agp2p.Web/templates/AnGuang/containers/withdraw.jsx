import React from "react";
import $ from "jquery";

export default class WithdrawPage extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	render() {
		return (
			<div ref="root">
				<div className="withdraw">
				    <div className="bank-select-withdraw"><span><i>*</i>选择银行卡：</span><div>
				        <ul className="list-unstyled list-inline ul-withdraw">
				            <li className="card zhonghang">
				                <p className="bank-name">中国银行</p>
				                <p className="card-num">尾号 3444 储蓄卡</p>
				            </li>
				            <li className="card jianhang">
				                <p className="bank-name">中国建设银行</p>
				                <p className="card-num">尾号 8075 储蓄卡</p>
				            </li>
				            <li className="add-card">添加银行卡</li>
				        </ul>
				    </div></div>
				    <div className="balance-withdraw"><span>可用余额：</span>￥0.00</div>
				    <div className="amount-withdraw"><span><i>*</i>提现金额：</span><input type="text"/><span>实际到账：99.00 元</span></div>
				    <div className="recorded-date"><span>预计到账日期：</span>2015-09-30 （1-2个工作日内到账，双休日和法定节假日除外）</div>
				    <div className="psw-withdraw"><span><i>*</i>交易密码：</span><input type="password"/></div>
				    <div className="withdrawBtn"><a href="#">确认提交</a></div>
				</div>
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
		//提现银行卡选择
        var $card = $(this.refs.root.getDOMNode()).find(".ul-withdraw .card");
		var {templateBasePath} = this.props;
		
        $card.click(function(){
            $card.find("img").remove();
            var img = document.createElement("img");
            img.src = templateBasePath + "/imgs/usercenter/withdraw-icons/selected.png";
            this.appendChild(img);
        });
    }
}