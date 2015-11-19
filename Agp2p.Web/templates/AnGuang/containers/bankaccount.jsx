import React from "react";
import { classMapping } from "../js/bank-list.jsx"
import CityPicker from "../components/city-picker.jsx"
import CardEditor from "../components/card-editor.jsx"
import { fetchBankCards, deleteBankCard } from "../actions/bankcard.js"

import "../less/bankaccount.less"

class BankAccount extends React.Component {
    constructor(props) {
        super(props);
        this.state = {selectedCardIndex: -1};
    }
    componentDidMount() {
		if (this.props.cards.length == 0) {
			this.props.dispatch(fetchBankCards());
		}
    }
    deleteCard(card) {
    	var {cardId, bankName, last4Char} = card;
    	if (confirm(`是否删除尾号为 ${last4Char} 的 ${bankName} 银行卡？`)) {
    		this.props.dispatch(deleteBankCard(cardId));
    	}
    }
    onCardClicked(index) {
    	this.setState({selectedCardIndex: this.state.selectedCardIndex == index ? -1 : index});
    }
    render() {
        return (
            <div className="bank-account-wrap">
                <div className="cards-list-th"><span>银行卡列表</span></div>
                <div className="cards-list-wrap">
                <ul className="list-unstyled list-inline">
                {this.props.cards.length != 0 ? null : <div>暂无银行卡</div>}
                {this.props.cards.map((c, index) => 
                	<li className={"card " + classMapping[c.bankName]} key={c.cardId}
                		onClick={ev => this.onCardClicked(index)}
                		style={index == this.state.selectedCardIndex ? {"backgroundColor": "#f2f2f2"} : null} >
	                	<i className="glyphicon glyphicon-minus-sign pull-right" onClick={ev => this.deleteCard(c)}></i>
	                	<p className="bank-name">{c.bankName}</p>
	                	<p className="card-num">尾号 {c.last4Char} 银行卡</p>
                	</li>
            	)}
                </ul>
                </div>
                <div className="add-cards-th"><span>{this.state.selectedCardIndex == -1 ? "新增银行卡" : "修改银行卡"}</span></div>
                <CardEditor rootClass="add-cards-wrap"
                    onOperationSuccess={() => this.setState({selectedCardIndex: -1})}
                    value={this.state.selectedCardIndex == -1 ? null : this.props.cards[this.state.selectedCardIndex]}/>
            </div>
        );
    }
}

function mapStateToProps(state) {
    return {
        cards: state.bankCards,
    };
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(BankAccount);