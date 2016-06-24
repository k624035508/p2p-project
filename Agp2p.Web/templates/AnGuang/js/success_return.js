import "bootstrap-webpack!./bootstrap.config.js";

import "../less/head.less";
import "../less/success_return.less";
import "../less/footerSmall.less";

import React from "react";
import ReactDom from "react-dom";
import header from "./header.js";
import { ajax } from "jquery";
import alert from "../components/tips_alert.js";


class ReturnCondition extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        return (
            <div >
                <div className="confirmtran">
                    <span>{
                        this.props.returnId == "default" ? "操作" : this.props.returnId == "invested" ? "投资" : this.props.returnId == "recharge" ? "充值" : this.props.returnId == "withdraw" ? "提现" : this.props.returnId == "repay" ? "还款" :
                            this.props.returnId == "tranClaim" ? "债权转让认购" : this.props.returnId == "autoTender" ? "自动投标续约" : this.props.returnId == "autoTenderCancel" ? "自动投标解约" :
                                this.props.returnId == "autoAccount" ? "自动还款开通" : this.props.returnId == "autoAccountCancel" ? "自动还款取消" : "操作"
                    }</span>确认
                </div>
                <p>
                    <img src="templates/AnGuang/imgs/usercenter/005.png" />
                    恭喜您, {
                        this.props.returnId == "default" ? "操作" : this.props.returnId == "invested" ? "投资" : this.props.returnId == "recharge" ? "充值" : this.props.returnId == "withdraw" ? "提现申请" : this.props.returnId == "repay" ? "还款" :
                            this.props.returnId == "tranClaim" ? "债权转让认购" : this.props.returnId == "autoTender" ? "自动投标续约" : this.props.returnId == "autoTenderCancel" ? "自动投标解约" :
                                this.props.returnId == "autoAccount" ? "自动还款开通" : this.props.returnId == "autoAccountCancel" ? "自动还款取消" : "操作"
                    }成功！
                </p>
                <div className={this.props.returnId == "recharge" ? "rechargebg" : this.props.returnId == "withdraw" ? "withdrawbg" : this.props.returnId == "repay" ? "repaybg" : this.props.returnId == "invested" ? "investedbg" : "contentbg"}>
                </div>
                {/*this.props.returnId == "invested" ?
                    <div className="investedtip">
                        <span>转让成功后，安广融合将以短信通知您。</span>
                        <span>如果在2016-04-18 10: 26: 00前没有人购买，转让项目将自动取消</span>
                    </div>
                    : ""*/}
                {this.props.returnId == "default" || this.props.returnId == "recharge" || this.props.returnId == "withdraw" ? <a href={linkAccount} className="returnBtn">我的账户</a> :
                    this.props.returnId == "repay" || this.props.returnId == "autoAccount" || this.props.returnId == "autoAccountCancel" ? <a href={linkMyloan} className="returnBtn">我的借款</a> :
                        this.props.returnId == "tranClaim" ? <a href={linkClaimRecord} className="returnBtn">转让记录</a> : 
                            this.props.returnId == "invested" || this.props.returnId == "autoTender" || this.props.returnId == "autoTenderCancel" ? <a href={linkRecord} className="returnBtn">投资记录</a> : <a href={linkAccount} className="returnBtn">我的账户</a>
                }
                {this.props.returnId == "invested" ? <a href={linkProject} className="closeBtn">我要理财</a> : this.props.returnId == "recharge" ? <a href={linkRecharge} className="closeBtn">继续充值</a> :
                    this.props.returnId == "withdraw" ? <a href={linkWithdraw} className="closeBtn">继续提现</a> : this.props.returnId == "repay" ? <a href={linkMyloan3} className="closeBtn">继续还款</a> :
                        this.props.returnId == "tranClaim" ? <a href={linkClaimAgain} className="closeBtn">继续转让</a> : this.props.returnId == "default" || this.props.returnId == "autoTender" ? <a href="/" className="closeBtn">首页</a> :
                            this.props.returnId == "autoTenderCancel" ? <a href="/" className="closeBtn">重新开通</a> : this.props.returnId == "autoAccount" ? <a href={linkMyloan3} className="closeBtn">继续开通</a> :
                               this.props.returnId == "autoAccountCancel" ? <a href={linkMyloan3} className="closeBtn">重新开通</a> : <a href="/" className="closeBtn">首页</a>
                }
            </div>
        )
    }
}

$(function () {
    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();
        var url = window.location.href;
    var index = url.indexOf("#");
    var returnId = url.substring(index + 1);
    var titlevalue = (returnId == "invested" ? "投资" : returnId == "recharge" ? "充值" : returnId == "withdraw" ? "提现" : returnId == "repay" ? "还款" :
        returnId == "tranClaim" ? "债权转让认购" : returnId == "autoTender" ? "自动投标续约" : returnId == "autoTenderCancel" ? "自动投标解约" :
            returnId == "autoAccount" ? "自动还款开通" : returnId == "autoAccountCancel" ? "自动还款取消" : "操作") + "确认";
        //$("title").text(titlevalue);  IE8不适用
        $(document).attr("title",titlevalue);  //IE8修改title值
    ReactDom.render(<ReturnCondition returnId={returnId} />, document.getElementById("returnSuccess"));
    });

