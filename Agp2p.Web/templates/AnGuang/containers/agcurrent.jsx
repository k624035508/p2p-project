import React from "react";
import { ajax } from "jquery";
import Pagination from "../components/pagination.jsx";
import "../less/agcurrent.less";

class AgCurrent extends React.Component{
    constructor(props){
        super(props);
        this.state = {pageIndex: 0, pageCount: 3, tableType:0};
    }

    render(){
        return(
            <div className="agCurrentPage">
                <div className="top-wrapper">
                    <div className="topic-icon"><span>安融活期</span></div>
                    <div className="profit-container">
                        <p className="expected-profit-title">今日预期收益<span className="pull-right current-protocol">安融活期协议</span></p>
                        <p className="expected-profit">0.00 <span className="profit-unit">元</span></p>
                        <ul className="profit-db list-unstyled list-inline">
                            <li>
                                <p>本金总额</p>
                                <p>0.00 元</p>
                            </li>
                            <li>
                                <p>累计收益</p>
                                <p>0.00 元</p>
                            </li>
                            <li>
                                <p>当前年化收益率</p>
                                <p>4.2%</p>
                            </li>
                        </ul>
                    </div>
                    <div className="btn-container">
                        <button type="button" className="btn-in">转 入</button>
                        <button type="button" className="btn-out">转 出</button>
                    </div>
                </div>
                <div className="bottom-wrapper">
                    <div className="select-bar">
                        <select name="currentType" id="typeSel">
                            <option value="全部">全 部</option>
                            <option value="收益">收 益</option>
                            <option value="转入">转 入</option>
                            <option value="转出">转 出</option>
                        </select>
                        <a href="javascript:" className={`tradeRecordBtn ${this.state.tableType == 0 ? 'active' : ''}`}
                           onClick={()=> this.setState({tableType: 0})}>交易记录</a>
                        <a href="javascript:" className={`assetsDetailBtn ${this.state.tableType == 1 ? 'active' : ''}`}
                           onClick={() => this.setState({tableType: 1})}>资产明细</a>
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
                                {/*每页显示7行数据*/}
                                <tr>
                                    <td>2016/3/8 17:28</td>
                                    <td>转出</td>
                                    <td>2200.00</td>
                                </tr>
                            </tbody>
                        </table>}
                        {this.state.tableType != 1 ? null :
                        <table className="table assetsDetail-tb">
                            <thead>
                                <tr>
                                    <th>债权编号</th>
                                    <th>买入本金（元）</th>
                                    <th>剩余本金（元）</th>
                                    <th>下一个还款日</th>
                                    <th>到期日</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>001</td>
                                    <td>3000.00</td>
                                    <td>1200.00</td>
                                    <td>2016/3/9</td>
                                    <td>2016/3/29</td>
                                </tr>
                            </tbody>
                        </table>}
                    </div>
                    <Pagination pageIndex={this.state.pageIndex} pageCount={this.state.pageCount}
                                    onPageSelected={pageIndex => this.setState({pageIndex: pageIndex})}/>
                </div>
            </div>
        );
    }
}
export default AgCurrent;