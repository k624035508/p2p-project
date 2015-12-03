import React from "react";
import { classMapping } from "../js/bank-list.jsx";
import keys from "lodash/object/keys";
import { fetchWalletAndUserInfo } from "../actions/usercenter.js";
import alert from "../components/tips_alert.js";
import "../less/recharge.less";

class RechargePage extends React.Component {
	constructor(props) {
		super(props);
		this.state = { selectedBankId: "", chargingAmount: "" };
	}
	componentDidMount () {
		$("#waitforPaymentDialog").on("hide.bs.modal", ev => {
			this.props.dispatch(fetchWalletAndUserInfo());
		});
	}
	doCharge(ev) {
		if (this.state.selectedBankId == "") {
			alert("请先选择银行卡");
			ev.preventDefault();
			return;
		}
		var amount = parseFloat(this.state.chargingAmount || "0");
		if (amount <= 0) {
			alert("请输入正确的金额");
			ev.preventDefault();
			return;
		}
		$("#waitforPaymentDialog").modal('show');
	}
	render() { //我要充值 内容
		let quickPayment = this.state.selectedBankId == "NOCARD";
		return (
			<div>
				<div className="bank-chose-th">
					<span><a href="javascript:" onClick={ev => this.setState({selectedBankId: ""})}
						className={quickPayment?"":"active"}>网银支付</a></span>
					<span><a href="javascript:" onClick={ev => this.setState({selectedBankId: "NOCARD"})}
						className={quickPayment?"active":""}>快捷支付</a></span>
				</div>
				{quickPayment ? null :
				<ul className="list-unstyled list-inline bank-select">
					{keys(classMapping).map(k => {
						return (
							<li id={classMapping[k]} key={classMapping[k]} onClick={ev => this.setState({selectedBankId: classMapping[k]})}>
							{this.state.selectedBankId == classMapping[k]
								? <img src={TEMPLATE_PATH + "/imgs/usercenter/recharge-icons/selected.png"} />
								: null}
							</li>);
					})}
				</ul>}
				<div className="balance-recharge"><span>账户余额：</span>{"￥" + this.props.idleMoney}</div>
				<div className="amount-recharge">
				    <span><i>*</i>充值金额：</span>
				    <input type="text" value={this.state.chargingAmount} onChange={ev => this.setState({chargingAmount: ev.target.value})}/>
				</div>
				<div className="rechargeBtn">
                <a target="_blank"
                	href={`/api/payment/ecpss/index.aspx?bankcode=${this.state.selectedBankId}&amount=${this.state.chargingAmount}`}
                	onClick={ev => this.doCharge(ev)}>确认充值</a></div>
				<div className="warm-tips"><span>温馨提示</span></div>
				<div className="rechargeTips">
				    <p>1. 为保障账户及资金安全，请在充值前完成安全认证以及提现密码设置。</p>
				    <p>2. 本平台禁止洗钱、信用卡套现、虚假交易等行为，一经发现并确认，将终止该账户的使用。</p>
				    <p>3. 如果充值金额没有及时到账，请拨打客服电话：400-8878-200。</p>
				</div>
				<div className="modal fade" id="waitforPaymentDialog" tabIndex="-1" role="dialog" aria-labelledby="waitforPaymentDialogLabel"
					data-backdrop="static" data-keyboard="false">
				  <div className="modal-dialog" role="document">
				    <div className="modal-content">
				      <div className="modal-header">
				        <button type="button" className="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
				        <h4 className="modal-title" id="waitforPaymentDialogLabel">请您在新打开的网上银行页面上完成付款</h4>
				      </div>
				      <div className="modal-body">
                            <p>付款完成前请不要关闭此窗口</p>
                            <p>完成付款后请根据您的情况点击下面的按钮</p>
				      </div>
				      <div className="modal-footer">
				        <button type="button" className="btn btn-default" data-dismiss="modal">付款遇到问题</button>
				        <button type="button" className="btn btn-primary" data-dismiss="modal">已完成付款</button>
				      </div>
				    </div>
				  </div>
				</div>
			</div>
		);
	}
}

function mapStateToProps(state) {
	return {idleMoney: state.walletInfo.idleMoney };
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(RechargePage);
