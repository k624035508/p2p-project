import React from "react";
import $ from "jquery";
import CityPicker from "../components/city-picker.jsx"
import bank from "../js/bank-list.jsx"
import { fetchWalletAndUserInfo } from "../actions/usercenter.js"

class AppendingCardDialog extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			bank: "",
			selectedLocation: [],
			openingBank: "",
			cardNumber: "",
			cardNumber2: ""
		};
	}
	doAppendCard() {
		if (this.state.cardNumber2 == "") {
			alert("两次输入的卡号不一致");
			return;
		}
		let url = USER_CENTER_ASPX_PATH + "/AjaxAppendCard";
        $.ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({cardNumber: this.state.cardNumber, bankName: this.state.bank,
            	bankLocation: this.state.selectedLocation.join(";"), openingBank: this.state.openingBank}),
            success: function (data) {
                alert(data.d);
                this.props.onAppendSuccess();
            }.bind(this),
            error: function (xhr, status, err) {
            	alert(xhr.responseJSON);
                console.error(url, status, err.toString());
            }.bind(this)
        });
        $(this.refs.dialog).modal("hide");
	}
	render() {
		return (
			<div className="modal fade" id="addCards" tabIndex="-1" role="dialog" aria-labelledby="addCardsLabel" ref="dialog">
				<div className="modal-dialog" role="document">
					<div className="modal-content">
						<div className="modal-header">
							<button type="button" className="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
							<h4 className="modal-title" id="addCardsLabel">新增银行卡</h4>
						</div>
						<div className="modal-body">
							<ul className="list-unstyled">
								<li><span>开户名：</span><span>（实名认证后的姓名）</span></li>
								<li><span>选择银行：</span><select className="bankSelect" onChange={ev => this.setState({bank: ev.target.value})}>
									<option value="">请选择银行</option>
									{bank.bankList.map(b => <option value={b} key={b}>{b}</option>)}
									</select></li>
								<li><span>开户行所在地：</span>
										<CityPicker onLocationChanged={(...args) => this.setState({selectedLocation: [...args]})} />
									</li>
								<li><span>开户行名称：</span><input type="text" onBlur={ev => this.setState({openingBank: ev.target.value})} /></li>
								<li><span>银行卡号：</span><input type="text" onBlur={ev => this.setState({cardNumber: ev.target.value})} /></li>
								<li><span>确认卡号：</span><input type="text" onBlur={ev => {
									if (ev.target.value != this.state.cardNumber) {
										alert("两次输入的卡号不一致");
										ev.target.value = "";
									}
									this.setState({cardNumber2: ev.target.value});
								}} /></li>
							</ul>
							<button type="button" onClick={this.doAppendCard.bind(this)}>提 交</button>
						</div>
					</div>
				</div>
			</div>
		);
	}
}

class WithdrawPage extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			cards: [],
			selectedCardIndex: -1,
			toWithdraw: 0,
			realityWithdraw: 0,
			moneyReceivingDay: new Date(new Date().getTime() + 1000*60*60*24*2).toJSON().slice(0,10),
			transactPassword: ""
		};
	}
	fetchCards() {
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryBankAccount"
		$.ajax({
            type: "get",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: "",
            success: function(result) {
                let data = JSON.parse(result.d);
                this.setState({cards: data});
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
	}
	onWithdrawAmountSetted(ev) {
		var toWithdraw = parseFloat(ev.target.value) || 0;
		this.setState({toWithdraw: toWithdraw});

		var url = "/tools/calc_stand_guard_fee.ashx?withdraw_value=" + toWithdraw;
        $.ajax({
            type: "get",
            url: url,
            dataType: "json",
            timeout: 10000,
            success: function (data) {
                var fee = parseFloat(data.handlingFee);
                this.setState({realityWithdraw: toWithdraw - fee});
                console.log(data.msg);
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
	}
	doWithdraw(ev) {
		if (this.state.selectedCardIndex == -1) {
			alert("请先选择银行卡");
			return;
		}
		if (this.state.toWithdraw <= 0) {
			alert("请填写正确的提现金额");
			return;
		}
		$.post("/tools/submit_ajax.ashx?action=withdraw", {
			cardId: this.state.cards[this.state.selectedCardIndex].accountId,
			howmany: this.state.toWithdraw,
			transactPassword: this.state.transactPassword
		}, function(data) {
			alert(data.msg);
			if (data.status == 1) {
				location.reload();
			}
		}, "json").fail(function() {
			alert("提交失败，请重试");
		});
	}
	componentDidMount() {
		this.fetchCards();
		var promise = this.props.dispatch(fetchWalletAndUserInfo());
		promise.done(data => {
			if (!this.props.hasTransactPassword) {
				alert("你未设置交易密码，请先到个人中心设置");
				window.location.hash = "#/safe";
			}
		})
    }
	render() {
		return (
			<div>
				<div className="withdraw">
				    <div className="bank-select-withdraw"><span><i>*</i>选择银行卡：</span><div>
				        <ul className="list-unstyled list-inline ul-withdraw">
				        {this.state.cards.map((c, index) => 
				            <li className={"card " + bank.classMapping[c.bankName]} key={index}
					            onClick={ev => this.setState({selectedCardIndex: index})}>
				                <p className="bank-name">{c.bankName}</p>
				                <p className="card-num">尾号 {c.last4Char} 储蓄卡</p>
				                {this.state.selectedCardIndex == index
				                	? <img src={TEMPLATE_PATH + "/imgs/usercenter/withdraw-icons/selected.png"} />
				                	: null
				                }
				            </li>
			        	)}
				            <li className="add-card" key="append-card" data-toggle="modal" data-target="#addCards">添加银行卡</li>
				        </ul>
						<AppendingCardDialog onAppendSuccess={this.fetchCards} />
				    </div></div>
				    <div className="balance-withdraw"><span>可用余额：</span>￥{this.props.idleMoney}</div>
				    <div className="amount-withdraw"><span><i>*</i>提现金额：</span>
				    	<input type="text" onBlur={this.onWithdrawAmountSetted.bind(this)}/><span>实际到账：{this.state.realityWithdraw} 元</span></div>
				    <div className="recorded-date"><span>预计到账日期：</span>{this.state.moneyReceivingDay} （1-2个工作日内到账，双休日和法定节假日除外）</div>
				    <div className="psw-withdraw"><span><i>*</i>交易密码：</span><input type="password"
				    	onBlur={ev => this.setState({transactPassword: ev.target.value})} disabled={!this.props.hasTransactPassword}
				    	placeholder={this.props.hasTransactPassword ? null : "（请先设置交易密码）"} /></div>
				    <div className="withdrawBtn"><a href="javascript:;" onClick={this.doWithdraw.bind(this)}>确认提交</a></div>
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
}

function mapStateToProps(state) {
	return {
		idleMoney: state.walletInfo.idleMoney,
		hasTransactPassword: state.userInfo.hasTransactPassword
	};
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(WithdrawPage);