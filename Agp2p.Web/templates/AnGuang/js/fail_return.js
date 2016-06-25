import "bootstrap-webpack!./bootstrap.config.js";

import "../less/head.less";
import "../less/fail_return.less";
import "../less/footerSmall.less";

import React from "react";
import ReactDom from "react-dom";
import header from "./header.js";


class ReturnFailCondition extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        return (
            <div >
                <p>
                    <img src="templates/AnGuang/imgs/usercenter/008.png" />
                    很抱歉, {
                        this.props.returnId == "default" ? "操作" : this.props.returnId == "investedFail" ? "投资" : this.props.returnId == "rechargeFail" ? "充值" : this.props.returnId == "withdrawFail" ? "提现申请" : this.props.returnId == "repayFail" ? "还款" :
                            this.props.returnId == "tranClaimFail" ? "债权转让认购" : this.props.returnId == "autoTenderFail" ? "自动投标续约" : this.props.returnId == "autoTenderCancelFail" ? "自动投标解约" :
                                this.props.returnId == "autoAccountFail" ? "个人自动账户开通" : this.props.returnId == "autoAccountCancelFail" ? "个人自动账户取消" : "操作"
                    }失败！
                </p>
                <div className="contentbg">
                </div>
                {this.props.returnId == "rechargeFail" ? <a href={linkrecharge} className="returnBtn">重试</a> :
                    this.props.returnId == "withdrawFail" ? <a href={linkwithdraw} className="returnBtn">重试</a> :
                    this.props.returnId == "repayFail" ? <a href={linkmyloan} className="returnBtn">重试</a> :
                    this.props.returnId == "investedFail" ||  this.props.returnId == "default" ? <a href={linkaccount} className="returnBtn">我的账户</a> :
                            <a href={linkrecord} className="returnBtn">重试</a>
                }
                <a href="/" className="closeBtn">首页</a>
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
    var titlevalue = (returnId == "investedFail" ? "投资" : returnId == "rechargeFail" ? "充值" : returnId == "withdrawFail" ? "提现" : returnId == "repayFail" ? "还款" :
       returnId == "tranClaimFail" ? "债权转让认购" : returnId == "autoTenderFail" ? "自动投标续约" : returnId == "autoTenderCancelFail" ? "自动投标解约" :
           returnId == "autoAccountFail" ? "自动还款开通" : returnId == "autoAccountCancelFail" ? "自动还款取消" : "操作") + "失败";
    //$("title").text(titlevalue);  IE8不适用
    $(document).attr("title",titlevalue);  //IE8修改title值
    ReactDom.render(<ReturnFailCondition returnId={returnId} />, document.getElementById("returnFail"));
});

