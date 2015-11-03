import React from "react"
import { ajax } from "jquery"
import Pagination from "../components/pagination.jsx"
import "../less/mylottery.less"

const LotteryStatusEnum = {
	Acting: 1,
	Confirm: 2,
	Cancel: 3,
};

export default class MyLottery extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			data: [],
			pageIndex: 0,
			pageCount: 0,
			selectedTabIndex: 0,
		};
	}
	componentDidMount() {
		this.fetchLotteries(this.state.pageIndex);
	}
	fetchLotteries(pageIndex) {
		this.setState({pageIndex});
		let url = USER_CENTER_ASPX_PATH + "/AjaxQueryLotteries", pageSize = 5;
		ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: JSON.stringify({pageIndex, pageSize}),
			success: function(result) {
				let {totalCount, data} = JSON.parse(result.d);
				this.setState({pageCount: Math.ceil(totalCount / pageSize), data});
			}.bind(this),
			error: function(xhr, status, err) {
				console.error(url, status, err.toString());
			}.bind(this)
		});
	}
	render() {
		return(
            <div className="lotteries-wrap">
                <div className="lottery-th">
                    {["未使用","已失效", "历史记录"].map((s, index) =>
                        <span key={index}><a href="javascript:" className={this.state.selectedTabIndex == index ? "active" : null}
                            onClick={ ev => this.setState({ selectedTabIndex: index }) }>{s}</a></span>)}
                </div>
                <div className="lottery-content">
                	{this.state.selectedTabIndex < 2 &&
                    <div className="lottery-list">
                    	{this.state.data.map(l => 
                        <div className={l.status == LotteryStatusEnum.Acting ? "lottery-usable" : "lottery-fail"}>
                            <div className="lottery-face">
                                <p className="lottery-value">￥{l.value}</p>
                                <p className="use-condition">使用条件</p>
                                <p className="use-date">使用期限</p>
                            </div>
                            <div className="lottery-state">{l.status == LotteryStatusEnum.Acting ? "待使用" : "已失效"}</div>
                        </div>)}
                    </div>}
                    {this.state.selectedTabIndex == 2 &&
                    <div className="lottery-history">
                        <table className="table">
                            <thead>
                                <tr>
                                    <th>红包金额</th>
                                    <th>红包来源</th>
                                    <th>使用规则</th>
                                    <th>红包状态</th>
                                    <th>有效期限</th>
                                </tr>
                            </thead>
                            <tbody>
                            	{this.state.data.map(l => 
                                <tr>
                                    <td>{"￥" + l.value}</td>
                                    <td>红包来源</td>
                                    <td>使用规则</td>
                                    <td>{l.status == LotteryStatusEnum.Acting ? "待使用" : "已失效"}</td>
                                    <td>有效期限</td>
                                </tr>)}
                            </tbody>
                        </table>
                    </div>}
	                <Pagination pageIndex={this.state.pageIndex} pageCount={this.state.pageCount}
	                    onPageSelected={pageIndex => this.fetchLotteries(pageIndex)}/>
                </div>
                <div className="use-rules-th"><span>使用规则</span></div>
                <div className="use-rules">
                    <p>1.投资额需要达到一定数额才可使用红包，具体数值见红包说明文字。</p>
                    <p>2.单笔投资可同时使用多张红包，使用时所需投资额为各个红包要求的起投金额相累加。</p>
                    <p>3.安广融合有权根据运营情况调整红包使用规则，规则最终解释权归安广融合所有。</p>
                </div>
            </div>
        );
    }
}