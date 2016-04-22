import "bootstrap-webpack!./bootstrap.config.js";

import "../less/head.less";
import "../less/success_return.less";
import "../less/footerSmall.less";

import React from "react";
import ReactDom from "react-dom";
import header from "./header.js";

import alert from "../components/tips_alert.js";

class ReturnCondition extends React.Component{
    constructor(props) {
        super(props);
    }
    render(){         
        return(
            <div >
            <div className="confirmtran">
                确认<span>{
                    this.props.returnId == "recharge" ? "充值" : (this.props.returnId == "withdraw" ? "提现" : (this.props.returnId == "repay" ? "还款" : "转让"))                   
                    }</span>信息
            </div>
            <p>
            <img src="templates/AnGuang/imgs/usercenter/005.png" />
            恭喜您 , {
            this.props.returnId == "invested" ? "投资" : this.props.returnId == "recharge" ? "充值" : this.props.returnId == "withdraw" ? "提现" : this.props.returnId == "repay" ? "还款" :
                this.props.returnId == "tranClaim" ? "债权转让认购" : this.props.returnId == "autoTender" ? "自动投标续约" : this.props.returnId == "autoTenderCancel" ? "自动投标解约" :
                this.props.returnId == "autoAccount" ? "个人自动账户开通" : "个人自动账户取消"
            }成功！
            </p>
            <div className={this.props.returnId == "recharge" ? "rechargebg" : this.props.returnId == "withdraw" ? "withdrawbg" : this.props.returnId == "repay" ? "repaybg" : this.props.returnId == "invested" ? "investedbg" : "contentbg"}>
            </div>
    {this.props.returnId == "invested" ?
    <div className="investedtip">
          <span>转让成功后，安广融合将以短信通知您。</span>
           <span>如果在2016-04-18 10:26:00前没有人购买，转让项目将自动取消</span>
    </div>
           : ""}
    {this.props.returnId == "recharge" || this.props.returnId =="withdraw" ?   <a href={linkaccount} className="returnBtn">我的账户</a> :
     this.props.returnId == "repay" ? <a href={linkmyloan} className="returnBtn">我的借款</a> :
        <a href={linkrecord} className="returnBtn">投资记录</a>
    }
        <a href="/" className="closeBtn">首页</a>
            </div>
            )
        }
}

    $(function(){
        //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();   
        var url =window.location.href;
        var index = url.indexOf("#");
        var returnId = url.substring(index+1);         
        ReactDom.render(<ReturnCondition returnId={returnId} />, document.getElementById("returnSuccess"));
    });

