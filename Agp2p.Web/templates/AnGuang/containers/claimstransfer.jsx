import React from "react";
import { ajax } from "jquery";
import HorizontalPicker from "../components/horizontal-picker.jsx";
import Pagination from "../components/pagination.jsx";
import "../less/claimstransfer.less";
import confirm from "../components/tips_confirm.js";
import alert from "../components/tips_alert.js";
import CustomDlg from "../components/custom-dialog.jsx"

import "rc-slider/assets/index.css";
import Slider from "rc-slider";

import find from "lodash/collection/find";
import assign from "lodash/object/assign";

const ClaimStatusEnum = {
    Nontransferable : 1,
    Transferable : 2,
    NeedTransfer : 3,
    Completed : 10,
    CompletedUnpaid : 11,
    Transferred : 20,
    TransferredUnpaid : 21,
    Invalid : 30,
};

class KeepInterestPickerDlg extends React.Component {
    constructor(props) {
        super(props);
        this.state = { callback: null, currentVal: 0, isAgreeToAgreement: false};
    }
    alignToCenter() {
        var offsetHeight = ($(window).height() - $(".modal-content", this.refs.customDialog).height()) / 2;
        $(".modal-dialog", this.refs.customDialog).css("margin-top", offsetHeight + "px");
    }
    show(callback) {
        var {principal: claimPrincipal, withdrawClaimFinalInterest: profittedInterest} = this.props.transferingClaim;

        this.setState({callback: () => {
            if (this.state.isAgreeToAgreement) {
                $(this.refs.customDialog).modal('hide');
                callback(this.state.currentVal - claimPrincipal)
            } else {
                alert("请先阅读并同意债权转让协议");
            }
        }, currentVal: claimPrincipal + profittedInterest});

        $(this.refs.customDialog).modal();
        this.alignToCenter();
    }
    hide() {
        $(this.refs.customDialog).modal('hide');
    }
    render() {
        var {principal: claimPrincipal, withdrawClaimFinalInterest: profittedInterest, originalClaimFinalInterest: originalInterest,
            profitingYearly: originalProfitRateYearly, remainDays, staticWithdrawCostPercent} = this.props.transferingClaim;
        var profitingRateAfterTransfer = (originalInterest - (this.state.currentVal - claimPrincipal)) * 360 / claimPrincipal / remainDays;
        return (<div className="modal fade custom-dlg" id="customDialog" ref="customDialog" data-backdrop="static" tabIndex="-1"
                     role="dialog" aria-labelledby="customDialogLabel">
                    <div className="modal-dialog" role="document">
                        <div className="modal-content">
                            <div className="modal-header">
                                <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                                    <span aria-hidden="true">&times;</span></button>
                                <h4 className="modal-title" id="customDialogLabel">申请债权转让确认</h4>
                            </div>
                            <div className="modal-body">
                                <div className="container">
                                    <div className="row">
                                        <div className="col-md-3">转让价格：</div>
                                        <div className="col-md-3">
                                            <Slider min={claimPrincipal} max={claimPrincipal + profittedInterest}
                                                value={this.state.currentVal} step={0.01} onChange={v => this.setState({currentVal: v})} />
                                        </div>
                                        <div className="col-md-6 span-desc">
                                            <span className="glyphicon glyphicon-exclamation-sign warning" aria-hidden="true" />{`只能设置在 ${claimPrincipal} ~ ${claimPrincipal + profittedInterest} 元之间`}</div>
                                    </div>
                                    <div className="row">
                                        <div className="col-md-3">原年利率：</div><div className="col-md-9 pink">{originalProfitRateYearly}</div>
                                    </div>
                                    <div className="row">
                                        <div className="col-md-3">折让后年利率：</div><div className="col-md-9 pink">{(profitingRateAfterTransfer*100).format(1) + '%'}</div>
                                    </div>
                                    <div className="row" style={{paddingBottom: '0'}}>
                                        <div className="col-md-3">转让手续费：</div><div className="col-md-9">{(this.state.currentVal * staticWithdrawCostPercent).format(2) + '元'}</div>
                                    </div>
                                </div>
                            </div>
                            <div className="modal-footer">
                                <div className="agreement-issue">
                                    <input type="checkbox" id="isAgreeToAgreement" checked={this.state.isAgreeToAgreement} onClick={ev => this.setState({isAgreeToAgreement: ev.target.checked})} />
                                    <label htmlFor="isAgreeToAgreement">我已经阅读并同意<a href="javascript:" className="claimTransferAgreement">《债权转让协议》</a></label>
                                </div>
                                <button type="button" className="confirm-btn" onClick={this.state.callback}>确 定</button>
                                <button type="button" className="cancel-btn" data-dismiss="modal">取 消</button>
                            </div>
                        </div>
                    </div>
                </div>);
    }
}

export default class ClaimsTransfer extends React.Component {
    constructor(props) {
        super(props);
        this.state = {claimQueryType:1, pageIndex:0, pageCount:0, claims: [],
            StaticClaimWithdrawAmount: '0.00', StaticClaimWithdrawCount: 0,
            BuyedStaticClaimAmount: '0.00', BuyedStaticClaimCount: 0, applyingTransferClaimId: 0};
    }
    componentDidMount() {
        this.fetchSummery();
        this.fetchClaims(1, 0);
    }
    fetchSummery() {
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryClaimTransferSummery";
        ajax({
            type: "GET",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: '',
            success: function(result) {
                let summery = JSON.parse(result.d);
                this.setState(summery);
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
    }
    fetchClaims(type, pageIndex) {
        this.setState({claimQueryType: type, pageIndex: pageIndex});

        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryStaticClaim", pageSize = 9;
        ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({claimQueryType: type, pageIndex: pageIndex, pageSize: pageSize}),
            success: function(result) {
                let {totalCount, data} = JSON.parse(result.d);
                this.setState({claims: data, pageCount: Math.ceil(totalCount / pageSize)});
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
    }
    doClaimTransfer(claimId, keepInterestPercent) {
        let url = USER_CENTER_ASPX_PATH + "/AjaxApplyForClaimTransfer";
        ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({claimId, keepInterestPercent}),
            success: function(result) {
                if (result.d === "ok") {
                    alert("申请转让成功", () => {
                        this.fetchSummery();
                        this.fetchClaims(2, 0);
                    });
                }
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
                alert('申请债权转让失败：' + xhr.responseJSON.d);
            }.bind(this)
        }); 
    }
    applyForClaimTransfer(claimId) {
        var claim = find(this.state.claims, c => c.id == claimId);
        if (!claim.loadedExtraInfo) {
            let url = USER_CENTER_ASPX_PATH + "/AjaxQueryWithdrawClaimExtraInfo";
            ajax({
                type: "POST",
                dataType: "json",
                contentType: "application/json",
                url: url,
                data: JSON.stringify({claimId}),
                success: function(result) {
                    let extraInfo = JSON.parse(result.d);
                    extraInfo.loadedExtraInfo = true;

                    assign(claim, extraInfo);
                    this.setState({applyingTransferClaimId: claimId, claims: this.state.claims});

                    this.refs.legacyInterestPicker.show(
                        v => this.doClaimTransfer(claimId, extraInfo.withdrawClaimFinalInterest == 0 ? 1 : v.toFixed(2)/extraInfo.withdrawClaimFinalInterest));
                }.bind(this),
                error: function(xhr, status, err) {
                    console.error(url, status, err.toString());
                    alert('查询债权应收利息失败：' + xhr.responseJSON.d);
                }.bind(this)
            });
        } else {
            this.refs.legacyInterestPicker.show(
                v => this.doClaimTransfer(claimId, claim.withdrawClaimFinalInterest == 0 ? 1 : v.toFixed(2)/claim.withdrawClaimFinalInterest));
        }
    }
    applyForCancelClaimTransfer(withdrawClaimId) {
        confirm("是否取消债权转让申请？", () => {
            let url = USER_CENTER_ASPX_PATH + "/AjaxApplyForCancelClaimTransfer";
            ajax({
                type: "POST",
                dataType: "json",
                contentType: "application/json",
                url: url,
                data: JSON.stringify({withdrawClaimId}),
                success: function(result) {
                    if (result.d === "ok") {
                        alert("撤回申请转让成功", () => {
                            this.fetchSummery();
                            this.fetchClaims(1, 0);
                        });
                    }
                }.bind(this),
                error: function(xhr, status, err) {
                    console.error(url, status, err.toString());
                    alert('取消债权转让申请失败：' + xhr.responseJSON.d);
                }.bind(this)
            });
        });
    }
    render() {
        return(
            <div className="claimsTransferPage">
                <div className="top-wrapper">
                    <div className="blue-container">
                        <p className="whatFor"><a href="javascript:">什么是债权转让？</a></p>
                        <ul className="claims-db list-unstyled list-inline ">
                            <li>
                                <p>成功转出金额</p>
                                <p>{this.state.StaticClaimWithdrawAmount + " 元"}</p>
                            </li>
                            <li>
                                <p>已转出债权笔数</p>
                                <p>{this.state.StaticClaimWithdrawCount}</p>
                            </li>
                            <li>
                                <p>成功转入金额</p>
                                <p>{this.state.BuyedStaticClaimAmount + " 元"}</p>
                            </li>
                            <li>
                                <p>已转入债权笔数</p>
                                <p>{this.state.BuyedStaticClaimCount}</p>
                            </li>
                        </ul>
                    </div>
                </div>
                <div className="bottom-wrapper">
                    <div className="warm-tips"><span>债权转让</span></div>
                    <HorizontalPicker onTypeChange={newType => this.fetchClaims(newType, 0) }
                        enumFullName="Agp2p.Common.Agp2pEnums+StaticClaimQueryEnum" value={this.state.claimQueryType} />
                    <div className="tb-container">
                        <table className="table claimsTransfer-tb">
                            <thead>
                                <tr>
                                    <th>债权编号</th>
                                    <th>收益项目</th>
                                    <th>年化利率</th>
                                    <th>本金</th>
                                    <th>状态</th>
                                    <th>创建时间</th>
                                    <th>下个还款日</th>
                                    <th>操作</th>
                                </tr>
                            </thead>
                            <tbody>
                            {this.state.claims.length == 0 ? <tr><td colSpan="8">暂无内容</td></tr> :
                                this.state.claims.map(c => 
                                    <tr key={c.id}>
                                        <td>{c.number}</td>
                                        <td>{c.profitingProject}</td>
                                        <td>{c.profitingYearly}</td>
                                        <td>{c.principal.format(2)}</td>
                                        <td>{c.queryType}</td>
                                        <td>{c.createTime}</td>
                                        <td>{c.nextProfitDay}</td>
                                        <td>{c.status == ClaimStatusEnum.Nontransferable
                                            ? <a href="javascript:" onClick={ev => this.applyForClaimTransfer(c.id)}>申请转让</a>
                                            : (c.status == ClaimStatusEnum.NeedTransfer && c.buyerCount == 0
                                                && new Date(c.createTime).valueOf() + 24*60*60*1000 < new Date().valueOf()
                                                ? <a href="javascript:" onClick={ev => this.applyForCancelClaimTransfer(c.id)}>撤回转让申请</a> : "")}</td>
                                    </tr>)
                            }
                            </tbody>
                        </table>
                    </div>
                    <Pagination pageIndex={this.state.pageIndex} pageCount={this.state.pageCount}
                                onPageSelected={pageIndex => this.fetchClaims(this.state.claimQueryType, pageIndex)}/>
                    <KeepInterestPickerDlg ref="legacyInterestPicker" transferingClaim={find(this.state.claims, c => c.id == this.state.applyingTransferClaimId) || {}} />
                </div>
            </div>
        );
    }
}