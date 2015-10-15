import React from "react";
import $ from "jquery";
import CityPicker from "../components/city-picker.jsx"

export default class WithdrawPage extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			cards: [],
			selectedCardIndex: -1,
			realityWithdraw: 0,
			moneyReceivingDay: new Date(new Date().getTime() + 1000*60*60*24*2).toJSON().slice(0,10),
			idleMoney: 0
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
	displayHandlingFee(ev) {
		var toWithdraw = parseFloat(ev.target.value);
        $.ajax({
            type: "get",
            url: "/tools/calc_stand_guard_fee.ashx?withdraw_value=" + toWithdraw,
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
	fetchUserInfo() {
		let url = USER_CENTER_ASPX_PATH + "/AjaxQueryUserInfo"
		$.ajax({
            type: "get",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: "",
            success: function(result) {
                let data = JSON.parse(result.d);
                this.setState(data);
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
	}
	getBankDomClassByName(bankName) {
		return {
			"中国银行" : "zhonghang",
			"中国工商银行" : "gonghang",
			"中国建设银行" : "jianhang",
			"中国农业银行" : "nonghang",
			"招商银行" : "zhaohang",
			"中国邮政储蓄银行" : "youzheng",
			"中国光大银行" : "guangda",
			"中信银行" : "zhongxin",
			"浦发银行" : "pufa",
			"中国民生银行" : "minsheng",
			"广发银行" : "guangfa",
			"兴业银行" : "xingye",
			"平安银行" : "pingan",
			"交通银行" : "jiaohang",
			"华夏银行" : "huaxia",
		}[bankName];
	}
	doWithdraw(ev) {
	}
	render() {
		return (
			<div>
				<div className="withdraw">
				    <div className="bank-select-withdraw"><span><i>*</i>选择银行卡：</span><div>
				        <ul className="list-unstyled list-inline ul-withdraw">
				        {this.state.cards.map((c, index) => 
				            <li className={"card " + this.getBankDomClassByName(c.bankName)} key={index}
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
						{/*添加银行卡弹窗*/}
						<div className="modal fade" id="addCards" tabindex="-1" role="dialog" aria-labelledby="addCardsLabel">
							<div className="modal-dialog" role="document">
								<div className="modal-content">
									<div className="modal-header">
										<button type="button" className="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
										<h4 className="modal-title" id="addCardsLabel">新增银行卡</h4>
									</div>
									<div className="modal-body">
										<ul className="list-unstyled">
											<li><span>开户名：</span><span>*嘉敏</span></li>
											<li><span>选择银行：</span><select id="bankSelect">
												<option value="">请选择银行</option>
												<option value="">中国银行</option>
												<option value="">中国工商银行</option>
												<option value="">中国建设银行</option>
												<option value="">中国农业银行</option>
												<option value="">招商银行</option>
												<option value="">中国邮政储蓄银行</option>
												<option value="">中国光大银行</option>
												<option value="">中国民生银行</option>
												<option value="">中信银行</option>
												<option value="">广发银行</option>
												<option value="">浦发银行</option>
												<option value="">兴业银行</option>
												<option value="">平安银行</option>
												<option value="">交通银行</option>
												<option value="">华夏银行</option>
												</select></li>
											<li><span>开户行所在地：</span>
													<CityPicker   />
												</li>
											<li><span>开户行名称：</span><input type="text"/></li>
											<li><span>银行卡号：</span><input type="text"/></li>
											<li><span>确认卡号：</span><input type="text"/></li>
										</ul>
										<button type="button">提 交</button>
									</div>
								</div>
							</div>
						</div>
				    </div></div>
				    <div className="balance-withdraw"><span>可用余额：</span>￥{this.state.idleMoney}</div>
				    <div className="amount-withdraw"><span><i>*</i>提现金额：</span>
				    	<input type="text" onBlur={this.displayHandlingFee.bind(this)}/><span>实际到账：{this.state.realityWithdraw} 元</span></div>
				    <div className="recorded-date"><span>预计到账日期：</span>{this.state.moneyReceivingDay} （1-2个工作日内到账，双休日和法定节假日除外）</div>
				    <div className="psw-withdraw"><span><i>*</i>交易密码：</span><input type="password"/></div>
				    <div className="withdrawBtn"><a href="javascript:;" onClick={this.doWithdraw}>确认提交</a></div>
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
		this.fetchUserInfo();
		this.fetchCards();
    }
}