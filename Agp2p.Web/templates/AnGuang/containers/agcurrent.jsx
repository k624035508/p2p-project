import React from "react";
import { ajax } from "jquery";
import Pagination from "../components/pagination.jsx";
import DropdownPicker from "../components/dropdown-picker.jsx";
import "../less/agcurrent.less";
import alert from "../components/tips_alert.js";
import { fetchWalletAndUserInfo } from "../actions/usercenter.js"

class HuoqiFacade extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            TodayProfitPredict: '0.00', TotalHuoqiClaimPrincipal: '0.00',
            TotalHuoqiProfit:'0.00', CurrentHuoqiProjectProfitRateYearly: '3.3%',
            CurrentHuoqiProjectId: null,
            withdrawAmount: '', transactPassword: ''
        };
    }
    componentDidMount() {
        this.fetchHuoqiSummary();
    }
    fetchHuoqiSummary() {
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryHuoqiSummary";
        ajax({
            type: "GET",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: '',
            success: result => {
                let summary = JSON.parse(result.d);
                this.setState(summary);
            },
            error: (xhr, status, err) => {
                console.error(url, status, err.toString());
            }
        });
    }
    gotoHuoqiProject() {
        if (0 < this.state.CurrentHuoqiProjectId) {
            location.href = `/project/${this.state.CurrentHuoqiProjectId}.html`;
        } else {
            alert("目前没有活期项目可投");
        }
    }
    doHuoqiProjectWithdraw() {
        let url = USER_CENTER_ASPX_PATH + "/AjaxWithdrawHuoqiProject";
        ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({amount: this.state.withdrawAmount, transactPassword: this.state.transactPassword}),
            success: result => {
                if (result.d === "ok") {
                    $('#huoqiWithdraw').modal('hide');
                    alert('活期项目转出成功', () => this.fetchHuoqiSummary());
                }
            },
            error: (xhr, status, err) => {
                console.error(url, status, err.toString());
                alert('活期项目转出失败：' + xhr.responseJSON.d);
            }
        });
    }
    render () {
        return (
            <div className="top-wrapper">
                <div className="topic-icon"><span>安融活期</span></div>
                <div className="profit-container">
                    <p className="expected-profit-title">今日预期收益<a className="pull-right current-protocol" href="/api/payment/sumapay/index.aspx?api=6">安融活期协议</a></p>
                    <p className="expected-profit">{this.state.TodayProfitPredict} <span className="profit-unit">元</span></p>
                    <ul className="profit-db list-unstyled list-inline">
                        <li>
                            <p>本金总额</p>
                            <p>{this.state.TotalHuoqiClaimPrincipal + " 元"}</p>
                        </li>
                        <li>
                            <p>累计收益</p>
                            <p>{this.state.TotalHuoqiProfit + " 元"}</p>
                        </li>
                        <li>
                            <p>当前年化收益率</p>
                            <p>{this.state.CurrentHuoqiProjectProfitRateYearly}</p>
                        </li>
                    </ul>
                </div>
                <div className="btn-container">
                    <button type="button" className="btn-in" onClick={ev => this.gotoHuoqiProject()}>转 入</button>
                    <button type="button" className="btn-out" data-toggle="modal" data-target="#huoqiWithdraw">转 出</button>
                </div>

                <div className="modal fade" id="huoqiWithdraw" tabIndex="-1" role="dialog" aria-labelledby="huoqiWithdrawLabel">
                  <div className="modal-dialog" role="document">
                    <div className="modal-content">
                      <div className="modal-header">
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <h4 className="modal-title" id="huoqiWithdrawLabel">活期项目转出</h4>
                      </div>
                      <div className="modal-body">
                        <form className="form-horizontal">
                          {/* hack auto complete */}
                          <input type="text" name="fakeName" className="hidden" />
                          <input type="password" name="fakePwd" className="hidden" />

                          <div className="form-group">
                            <label htmlFor="inputEmail3" className="col-sm-2 control-label">转出金额</label>
                            <div className="col-sm-10">
                              <input type="text" className="form-control" value={this.state.withdrawAmount}
                                onChange={ev => this.setState({withdrawAmount: ev.target.value})} />
                            </div>
                          </div>
                          <div className="form-group">
                            <label htmlFor="inputPassword3" className="col-sm-2 control-label">交易密码</label>
                            <div className="col-sm-10">
                              <input type="password" className="form-control" value={this.state.transactPassword}
                                onChange={ev => this.setState({transactPassword: ev.target.value})} />
                            </div>
                          </div>
                        </form>
                      </div>
                      <div className="modal-footer">
                        <button type="button" className="btn btn-default" data-dismiss="modal">取消</button>
                        <button type="button" className="btn btn-primary" onClick={ev => this.doHuoqiProjectWithdraw()}>确认</button>
                      </div>
                    </div>
                  </div>
                </div>
            </div>
        );
    }
}

export default class AgCurrent extends React.Component {
    constructor(props) {
        super(props);
        this.state = {pageIndex: 0, pageCount: 0, tableType:0, data: [], queryType: 0 };
    }
    componentDidMount() {
        this.fetchHuoqiTransaction();
    }
    fetchHuoqiTransaction(huoqiQueryType = 0, pageIndex = 0) {
        this.setState({tableType: 0, queryType:huoqiQueryType});
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryHuoqiTransactions", pageSize = 7;
        ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({pageSize, huoqiQueryType, pageIndex}),
            success: result => {
                let {totalCount, data} = JSON.parse(result.d);
                this.setState({data: data, pageCount: Math.ceil(totalCount / pageSize)});
            },
            error: (xhr, status, err) => {
                console.error(url, status, err.toString());
            }
        });
    }
    fetchHuoqiClaims(claimQueryType = 0, pageIndex = 0) {
        this.setState({tableType: 1, queryType:claimQueryType});
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryHuoqiClaims", pageSize = 7;
        ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({pageSize, claimQueryType, pageIndex}),
            success: result => {
                let {totalCount, data} = JSON.parse(result.d);
                this.setState({data: data, pageCount: Math.ceil(totalCount / pageSize)});
            },
            error: (xhr, status, err) => {
                console.error(url, status, err.toString());
            }
        });
    }
    render() {
        return(
            <div className="agCurrentPage">
                <HuoqiFacade />
                <div  className={!this.props.isLoaner ? "bottom-wrapper" : "bottom-wrapper-loaner"}>
                    <div className="select-bar">
                        {this.state.tableType == 0
                            ? <DropdownPicker enumFullName="Agp2p.Common.Agp2pEnums+HuoqiTransactionQueryEnum"
                                    onTypeChange={newType => this.fetchHuoqiTransaction(newType, this.state.pageIndex)}
                                    value={this.state.queryType} />
                            : <DropdownPicker enumFullName="Agp2p.Common.Agp2pEnums+HuoqiClaimQueryEnum"
                                    onTypeChange={newType => this.fetchHuoqiClaims(newType, this.state.pageIndex)}
                                    value={this.state.queryType} />}
                        <a href="javascript:" className={`tradeRecordBtn ${this.state.tableType == 0 ? 'active' : ''}`}
                           onClick={() => { if (this.state.tableType != 0) this.fetchHuoqiTransaction(); } }>交易记录</a>
                        <a href="javascript:" className={`assetsDetailBtn ${this.state.tableType == 1 ? 'active' : ''}`}
                           onClick={() => { if (this.state.tableType != 1) this.fetchHuoqiClaims(); } }>资产明细</a>
                    </div>
                    <div className="tb-container">
                        {this.state.tableType != 0 ? null :
                        <table className="table tradeRecord-tb">
                            <thead>
                                <tr>
                                    <th>时间</th>
                                    <th>明细</th>
                                    <th>金额（元）</th>
                                </tr>
                            </thead>
                            <tbody>
                                {this.state.data.length == 0 ? <tr><td colSpan="3">暂无内容</td></tr> :
                                    this.state.data.map(his => 
                                        <tr key={his.id}>
                                            <td>{his.createTime}</td>
                                            <td>{his.type}</td>
                                            <td>{his.outcome ? his.outcome : his.income}</td>
                                        </tr>)
                                }
                            </tbody>
                        </table>}
                        {this.state.tableType != 1 ? null :
                        <table className="table assetsDetail-tb">
                            <thead>
                                <tr>
                                    <th>债权编号</th>
                                    <th>原债权项目</th>
                                    <th>买入本金（元）</th>
                                    <th>状态</th>
                                    <th>创建时间</th>
                                    <th>到期日</th>
                                </tr>
                            </thead>
                            <tbody>
                            {this.state.data.length == 0 ? <tr><td colSpan="6">暂无内容</td></tr> : this.state.data.map(c => 
                                <tr key={c.id}>
                                    <td>{c.number}</td>
                                    <td>{c.project}</td>
                                    <td>{c.principal}</td>
                                    <td>{c.queryType}</td>
                                    <td>{c.createTime}</td>
                                    <td>{c.completeDay}</td>
                                </tr>)
                            }
                            </tbody>
                        </table>}
                    </div>
                    <Pagination pageIndex={this.state.pageIndex} pageCount={this.state.pageCount}
                                onPageSelected={pageIndex => {
                                    if (this.state.tableType == 0)
                                        this.fetchHuoqiTransaction(this.state.queryType, pageIndex);
                                    else
                                        this.fetchHuoqiClaims(this.state.queryType, pageIndex);
                                }}/>
                </div>
            </div>
        );
    }
}
function mapStateToProps(state) {         
            return {              
isLoaner: state.userInfo.isLoaner
};
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(AgCurrent);
