import React from "react";
import bank from "../js/bank-list.jsx"
import CityPicker from "../components/city-picker.jsx"

export default class BankAccount extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render() {
        return (
            <div className="bank-account-wrap">
                <div className="cards-list-th"><span>银行卡列表</span></div>
                <div className="cards-list-wrap">
                    <ul className="list-unstyled list-inline">
                        <li className="zhonghang">
                            <i className="glyphicon glyphicon-minus-sign pull-right"></i>
                            <p className="bank-name">中国银行</p>
                            <p className="card-num"><span>尾号</span> <span>7855</span> <span>储蓄卡</span></p>
                        </li>
                        <li className="jianhang">
                            <i className="glyphicon glyphicon-minus-sign pull-right"></i>
                            <p className="bank-name">建设银行</p>
                            <p className="card-num"><span>尾号</span> <span>3444</span> <span>储蓄卡</span></p>
                        </li>
                        <li className="zhaohang">
                            <i className="glyphicon glyphicon-minus-sign pull-right"></i>
                            <p className="bank-name">招商银行</p>
                            <p className="card-num"><span>尾号</span> <span>2682</span> <span>信用卡</span></p>
                        </li>
                    </ul>
                </div>
                <div className="add-cards-th"><span>新增银行卡</span></div>
                <div className="add-cards-wrap">
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
                    <button type="button">提 交</button>
                </div>
            </div>
        );
    }
}