import React from "react";
import { classMapping } from "../js/bank-list.jsx";
import keys from "lodash/object/keys";
import { fetchWalletAndUserInfo } from "../actions/usercenter.js";
import alert from "../components/tips_alert.js";
import "../less/recharge.less";

class RechargePage extends React.Component {
    constructor(props) {
        super(props);
        this.state = { selectedBankId: "NOCARD", chargingAmount: "" };
    }
    componentDidMount () {
        $("#waitforPaymentDialog").on("hide.bs.modal", ev => {
            this.props.dispatch(fetchWalletAndUserInfo());
        });
        if (this.props.groupName == "VIP会员") {
            alert("您暂时无法充值：安广融合平台正在内部试运行中，具体全面开放请留意官网公告。");
        }
    }
    doCharge(ev) {
        //TODO 停止充值临时逻辑
        //alert("您暂时无法充值：安广融合平台正在切换第三方资金托管，具体全面开放请留意官网公告。");
        //ev.preventDefault();
        //return;
        if (this.props.groupName == "VIP会员") {
            alert("您暂时无法充值：安广融合平台正在内部试运行中，具体全面开放请留意官网公告。");
            ev.preventDefault();
            return;
        }
        if (this.state.selectedBankId == "") {
            alert("请先选择银行卡");
            ev.preventDefault();
            return;
        }
        var amount = parseFloat(this.state.chargingAmount || "0");
        if (isNaN(amount) || amount <= 0) {
            alert("请输入正确的金额");
            ev.preventDefault();
            return;
        }
        if ( amount < 100){
            alert("充值金额最低为100元");
            ev.preventDefault();
            return;
        }

        $.ajax({
            type: "post",
            url: "/tools/submit_ajax.ashx?action=recharge",
            dataType: "json",
            data: {
                rechargeSum: this.state.chargingAmount,
                bankCode: this.state.selectedBankId,
                quickPayment: this.state.selectedBankId == "NOCARD"
            },
            success: function(data){
                if(data.status == "0"){
                    alert(data.msg)
                }else{
                    //$("#waitforPaymentDialog").modal('show');
                    //跳转到托管充值地址
                    location.href = data.url;
                }
            },
            error: function(xhr, status, err){
                alert("操作超时，请重试。");
            }
        });
    }
    render() { //我要充值 内容
        let quickPayment = this.state.selectedBankId == "NOCARD";
        return (
			<div>
				<div className="bank-chose-th">
    <span><a href="javascript:" onClick={ev => this.setState({selectedBankId: "NOCARD"})}
    className={quickPayment?"active":""}>快捷支付</a></span>
        <span><a href="javascript:" onClick={ev => this.setState({selectedBankId: ""})}
        className={quickPayment?"":"active"}>网银支付</a></span>
</div>
        {quickPayment ? null :
        <ul className="list-unstyled list-inline bank-select">
            {keys(classMapping).map(k => {
                return (
                    <li id={classMapping[k]} key={classMapping[k]} onClick={ev => this.setState({selectedBankId: classMapping[k]})}>
    {this.state.selectedBankId == classMapping[k]
        ? <img src={TEMPLATE_PATH + "/imgs/usercenter/recharge-icons/selected2.png"} />
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
                    <a onClick={ev => this.doCharge(ev)}>确认充值</a></div>
                    <div className="warm-tips"><span>温馨提示</span></div>
                    <div className="rechargeTips">
                        <p>1. 为了保障账户及资金安全，请在充值前在“个人中心”完成安全认证以及提现密码设置。</p>
                        <p>2. 充值过程中请不要关闭浏览器，请您耐心等待；充值成功后金额将及时汇入您的账户中。</p>
                        <p>3. 请注意您的银行卡充值限制，以免造成不便；每日的充值限额依据各银行限额为准。</p>
                        <p>4. 本平台禁止洗钱、信用卡套现、虚假交易等行为，一经发现并确认，将终止该账户的使用。</p>
                        <p>5. 如果充值金额没有及时到账，请您拨打客服电话400-8878-200，或联系在线客服确认。</p>
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
	return {idleMoney: state.walletInfo.idleMoney, groupName: state.userInfo.groupName };
	}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(RechargePage);
