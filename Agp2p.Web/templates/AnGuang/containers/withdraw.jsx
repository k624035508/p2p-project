import React from "react";
import { classMapping } from "../js/bank-list.jsx";
import CardEditor from "../components/card-editor.jsx";
import { fetchBankCards } from "../actions/bankcard.js";
import { fetchWalletAndUserInfo } from "../actions/usercenter.js";
import { ajax } from "jquery";
import every from 'lodash/collection/every';

import "../less/withdraw.less";
import alert from "../components/tips_alert.js";
window['$'] = $;


class AppendingCardDialog extends React.Component {
	constructor(props) {
		super(props);
		this.state = { };
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
						<CardEditor rootClass="modal-body" onOperationSuccess={() => $(this.refs.dialog).modal("hide")} />
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
			selectedCardIndex: -1,
			toWithdraw: "",
			realityWithdraw: 0,
			moneyReceivingDay: new Date(new Date().getTime() + 1000*60*60*24*2).toJSON().slice(0,10),
			transactPassword: "",
		};
	}
	onWithdrawAmountSetted(ev) {
		var toWithdraw = parseFloat(this.state.toWithdraw) || 0;

		if (toWithdraw == 0) {
			this.setState({realityWithdraw: 0});
			return;
		}

		var url = "/tools/calc_stand_guard_fee.ashx?withdraw_value=" + toWithdraw;
        ajax({
            type: "GET",
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
			alert("请先选择银行卡.");
			return;
		}
		if ((parseFloat(this.state.toWithdraw) || 0) <= 0) {
			alert("请填写正确的提现金额！");
			return;
		}
		if(parseFloat(this.state.toWithdraw) > this.props.idleMoney){
		    alert("提现金额超过当前余额！");
		    return;
		}

		ajax({
			type: "POST",
			url: "/tools/submit_ajax.ashx?action=withdraw",
			data: {
			    cardId: this.props.withdrawableCards[this.state.selectedCardIndex].cardId,
			    bankName:this.props.withdrawableCards[this.state.selectedCardIndex].bankName,
			    bankAccount:this.props.withdrawableCards[this.state.selectedCardIndex].cardNumber,
				howmany: this.state.toWithdraw,
				transactPassword: this.state.transactPassword
			},
			dataType: "json",
			success: data => {				
				if (data.status == 1) {
				    //this.setState({toWithdraw: "", transactPassword: ""})
				    //this.props.dispatch(fetchWalletAndUserInfo());
				    location.href = data.url;
				} else{
				    alert(data.msg);
				}

			},
			error: jqXHR => {
				alert("提交失败，请重试");
			}
		});
	}
	componentDidMount() {
		if (this.props.cards.length == 0) {
			this.props.dispatch(fetchBankCards());
		}
		$(".fees-img2").hover(function(){
		    $(".fees-tip").css("zIndex","10");
		},function(){
		    $(".fees-tip").css("zIndex","-10");
		});
    }
	render() {
		var {withdrawableCards, cards, dispatch, realName, idleMoney, extraHint} = this.props;
		return (
			<div>
				{/* hack auto-complete */}
				<input type="text" name="foo" className="hidden"/>
				<input type="password" name="bar" className="hidden"/>
				<div className="withdraw">
				    <div className="bank-select-withdraw"><span><i>*</i>选择银行卡：</span>
					    <div>
					        <ul className="list-unstyled list-inline ul-withdraw">
					        {withdrawableCards.map((c, index) =>
					            <li className={"card " + classMapping[c.bankName]} key={c.cardId}
						            onClick={ev => this.setState({selectedCardIndex: index})}>
					                <p className="bank-name">{c.bankName}</p>
					                <p className="card-num">{"尾号 " + c.last4Char + " 储蓄卡"}</p>
					                {this.state.selectedCardIndex == index
					                	? <img src={TEMPLATE_PATH + "/imgs/usercenter/withdraw-icons/selected2.png"} />
					                	: null
					                }
					            </li>
				        	)}
				        	{1 <= cards.length || withdrawableCards.length != cards.length ? null :
					            <li className="add-card" key="append-card" data-toggle="modal" data-target="#addCards">添加银行卡</li>
				        	}
				        	{extraHint ? <li style={{width: '300px'}}>{extraHint}</li> : null}
					        </ul>
							<AppendingCardDialog dispatch={dispatch} realName={realName}
								onAppendSuccess={() => this.props.dispatch(fetchBankCards())} />
					    </div>
				    </div>
				    <div className="balance-withdraw"><span>可用余额：</span>{"￥" + idleMoney.toString()}</div>
				    <div className="amount-withdraw"><span><i>*</i>提现金额：</span>
				    	<input type="text" onChange={ev => this.setState({toWithdraw: ev.target.value})} value={this.state.toWithdraw} placeholder="最低提现100元"
				    		onBlur={ev => this.onWithdrawAmountSetted(ev)}/><span className="hidden">{"实际到账：" + this.state.realityWithdraw + " 元"}</span>
				    <span className="withComp">预计到账日期：</span>{this.state.moneyReceivingDay + " （1-2个工作日内到账，双休日和法定节假日除外）"}</div>
				        {/* <div className="psw-withdraw"><span><i>*</i>交易密码：</span>
					    <input type="password"
					    	onFocus={ev => this.setState({passwordReadonly: false})}
					    	value={this.state.transactPassword}
					    	onChange={ev => this.setState({transactPassword: ev.target.value})}
					    	disabled={!this.props.hasTransactPassword}
					    	placeholder={this.props.hasTransactPassword ? "" : "（请先设置交易密码）"} />
			    	</div>*/}
                    <div className="fees fees-img"><span className="fees-title">扣除资金托管费：</span><span className="fees-num">0</span>元
                        <span className="fees-img2" ></span>
                    </div>
                    <div className="fees fees-img"><span className="fees-title">扣除提现手续费：</span><span className="fees-num">0</span>元
                        <span className="fees-img2"></span>
                    </div>
                    <div className="fees"><span className="fees-title">银行卡实际到账金额：</span><span  className="fees-num">{this.state.realityWithdraw}</span>元</div> 
				    <div className="withdrawBtn"><a href="javascript:;" onClick={this.doWithdraw.bind(this)}>确认提交</a></div>
                    <div className="fees-tip"><em><i></i></em>现平台暂时不收取管理费、提现手续费、充值手续费，如有资费变动将另行通知。</div>
                   
				</div>
				<div className="bank-chose-tips"><span>温馨提示</span></div>
				<div className="rechargeTips">
				    <p>1、提现时务必使用与您的身份证信息一致的银行卡，且确保填写的银行卡姓名及手机号码与平台预留信息一</p>
                    <p className="recTipsOther">致，否则导致提现失败。</p>
				    <p>2、推广期间暂不收取充值、提现、管理费用，具体收费时间以平台公示为准。</p>
				    <p>3、同一注册帐号提现次数一天不超3次，每次提现额度最高50万元。</p>
				    <p>4、为了保障您的账户及资金安全，本平台不会以任何方式索取您的账户密码，请妥善保管您的账户密码信息。</p>
                    <p>5、用户提交提现申请，资金会在1-2个工作日到账，具体时间以银行到账时间为准，提现仅限银行借记卡，不</p>
                    <p className="recTipsOther">支持存折、信用卡及其他卡种。</p>
                    <p>6、提现过程如有疑问，请在工作日08：30-18：00联系平台客服。</p>
				</div>
			</div>
		);
	}
}

const BankAccountType = {
    Unknown: 1, // 未知
    QuickPay: 2, // 快捷支付
    WebBank: 3, // 网银支付
}

function mapStateToProps(state) {
	var quickPayCards = state.bankCards.filter(c => c.type == BankAccountType.QuickPay)

	var withdrawableCards = null, extraHint = null;

	if (quickPayCards.length === 1) {
		withdrawableCards = quickPayCards
	} else if (every(state.bankCards, c => c.type == BankAccountType.Unknown)) {
		withdrawableCards = state.bankCards;
		extraHint = '尊敬的会员，因您未在丰付平台中绑定银行卡，所以在丰付平台提现时需要手动输入银行卡号。'
	} else if (every(state.bankCards, c => c.type == BankAccountType.WebBank)) {
		withdrawableCards = [];
		extraHint = '尊敬的会员，因您在安广融合平台绑定的银行卡与丰付平台绑定的银行卡不一致，请重新绑定。'
	} else {
		withdrawableCards = [];
		extraHint = '查询银行卡出错，请联系客服'
	}

	return {
		realName: state.userInfo.realName,
		idleMoney: state.walletInfo.idleMoney,
		hasTransactPassword: state.userInfo.hasTransactPassword,
		cards: state.bankCards,
		withdrawableCards,
		extraHint
	};
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(WithdrawPage);


