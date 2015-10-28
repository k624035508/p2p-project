import React from "react";
import $ from "jquery";
import CityPicker from "../components/city-picker.jsx"
import bank from "../js/bank-list.jsx"
import { appendBankCard, modifyBankCard } from "../actions/bankcard.js"

class CardEditor extends React.Component {
	constructor(props) {
		super(props);
		this.state = this.genStateByValue(props.defaultValue);
	}
    componentWillReceiveProps(nextProps) {
        this.setState(this.genStateByValue(nextProps.value));
    }
    componentDidMount() {
    	if (!this.props.realName) {
    		alert("你未进行实名认证，请先到个人中心进行认证");
    		window.location.hash = "#/safe";
    	}
	}
	genStateByValue(val) {
		var state = {
			bank: "",
			selectedLocation: [],
			openingBank: "",
			cardNumber: "",
			cardNumber2: ""
		};
		if (val) {
			state.bank = val.bankName;
			state.cardNumber = val.cardNumber;
			state.openingBank = val.openingBank;
			state.selectedLocation = val.bankLocation.split(";");
		}
		return state;
	}
	doSaveCard() {
		if (!this.state.bank) {
			alert("请先选择银行");
			return;
		}
		if (this.state.selectedLocation.length < 2) {
			alert("请先选择开户行所在地");
			return;
		}
		if (!this.state.openingBank) {
			alert("请先填写开户行");
			return;
		}
		if (!this.state.cardNumber) {
			alert("请先输入卡号");
			return;
		}
		if (!this.props.value) {
			if (this.state.cardNumber2 != this.state.cardNumber) {
				alert("两次输入的卡号不一致");
				return;
			}
			var promise = this.props.dispatch(appendBankCard(this.state.cardNumber, this.state.bank, this.state.selectedLocation, this.state.openingBank));
			promise.done(this.props.onOperationSuccess);
		} else {
			var promise = this.props.dispatch(modifyBankCard(
				this.props.value.cardId, this.state.bank, this.state.selectedLocation, this.state.openingBank, this.state.cardNumber));
			promise.done(this.props.onOperationSuccess);
		}
	}
	render() {
		return (
			<div className={this.props.rootClass}>
				<ul className="list-unstyled">
					<li><span>开户名：</span><span>{this.props.realName || "（实名认证后的姓名）"}</span></li>
					<li><span>选择银行：</span><select className="bankSelect" value={this.state.bank} onChange={ev => this.setState({bank: ev.target.value})}>
						<option value="">请选择银行</option>
						{bank.bankList.map(b => <option value={b} key={b}>{b}</option>)}
						</select></li>
					<li><span>开户行所在地：</span>
						<CityPicker value={this.state.selectedLocation} onLocationChanged={(...args) => this.setState({selectedLocation: [...args]})} />
					</li>
					<li><span>开户行名称：</span><input type="text" value={this.state.openingBank} onChange={ev => this.setState({openingBank: ev.target.value})} /></li>
					<li><span>银行卡号：</span><input type="text" value={this.state.cardNumber} onChange={ev => this.setState({cardNumber: ev.target.value})} /></li>
					{this.props.value ? null :
					<li><span>确认卡号：</span><input type="text" value={this.state.cardNumber2} onChange={ev => this.setState({cardNumber2: ev.target.value})} /></li>}
				</ul>
				<button type="button" onClick={this.doSaveCard.bind(this)}>提 交</button>
			</div>
		);
	}
}

function mapStateToProps(state) {
	return {
		realName: state.userInfo.realName,
	};
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(CardEditor);