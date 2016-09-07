import React from "react";
import { ajax } from "jquery";
import Pagination from "../components/pagination.jsx";

import "../less/mylottery.less";

const LotteryStatusEnum = {
	Acting: 1,
	Confirm: 2,
	Cancel: 3,
};

const LotteryTypeEnum = {
    InterestRateTicket: 5,
    HongBao: 6,
};

class MyLottery extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
		    data: [],
		    dataHaveUsed: [],
            dataGuoqi: [],
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
		let url = USER_CENTER_ASPX_PATH + "/AjaxQueryLotteries", pageSize = 6;
		ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: JSON.stringify({pageIndex, pageSize}),
			success: function(result) {
				let {totalCount, data, dataHaveUsed, dataGuoqi} = JSON.parse(result.d);
				this.setState({pageCount: Math.ceil(totalCount / pageSize), data, dataHaveUsed, dataGuoqi});
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
                    {["未使用", "已使用", "已过期"].map((s, index) =>
                        <span key={index}><a href="javascript:" className={this.state.selectedTabIndex == index ? "active" : null}
                            onClick={ ev => this.setState({ selectedTabIndex: index }) }>{s}</a></span>)}
                </div>
                <div className="lottery-content">
                {this.state.selectedTabIndex == 0 &&
                    <div className="lottery-list">
                {this.state.data.length != 0 ? null : <div>暂无可用奖券</div>}
                {this.state.data.map(l =>                
                        (<div className={l.activity_type == LotteryTypeEnum.InterestRateTicket ? "interest-rate-ticket" : "hongbao"} key={l.id}>
                            <div className="lottery-title">
                {l.activity_type == LotteryTypeEnum.InterestRateTicket ? "加息券" : "红包"}
                            </div>
                            <div className="lottery-face">
                                <p className="lottery-value">{l.activity_type == LotteryTypeEnum.HongBao ? <span>{l.value}元</span> : <span>{l.details.InterestRateBonus}%</span>}</p>
                                <p className="use-condition">投资{l.activity_type == LotteryTypeEnum.InterestRateTicket ? l.details.minInvestValue : l.details.InvestUntil}元以上可用</p>
                                <p className="use-date">有效期至{l.details.Deadline}</p>
                            </div>
                            <div className="lottery-state">待使用</div> 
                        </div>)
                        )}
                    </div>
                    }
                {this.state.selectedTabIndex == 1 &&
                    <div className="lottery-list">
                {this.state.dataHaveUsed.length != 0 ? null : <div>暂无可用奖券</div>}
                {this.state.dataHaveUsed.map(l =>                
                        (<div className={l.activity_type == LotteryTypeEnum.InterestRateTicket ? "interest-rate-ticket-guoqi" : "hongbao-guoqi"} key={l.id}>
                            <div className="lottery-title">
                {l.activity_type == LotteryTypeEnum.InterestRateTicket ? "加息券" : "红包"}
                            </div>
                            <div className="lottery-face">
                                <p className="lottery-value">{l.activity_type == LotteryTypeEnum.HongBao ? <span>{l.value}元</span> : <span>{l.details.InterestRateBonus}%</span>}</p>
                                <p className="use-condition">投资{l.activity_type == LotteryTypeEnum.InterestRateTicket ? l.details.minInvestValue : l.details.InvestUntil}万元以上可用</p>
                                <p className="use-date">有效期至{l.details.Deadline}</p>
                            </div>
                            <div className="lottery-state">已使用</div>
                        </div>)
                        )}
                    </div>
                    }
                {this.state.selectedTabIndex == 2 &&
                <div className="lottery-list">
                {this.state.dataGuoqi.length != 0 ? null : <div>暂无奖券过期</div>}
                {this.state.dataGuoqi.map(l =>                
                        (<div className={l.activity_type == LotteryTypeEnum.InterestRateTicket ? "interest-rate-ticket-guoqi" : "hongbao-guoqi"} key={l.id}>
                            <div className="lottery-title">
                {l.activity_type == LotteryTypeEnum.InterestRateTicket ? "加息券" : "红包"}
                            </div>
                            <div className="lottery-face">
                                <p className="lottery-value">{l.activity_type == LotteryTypeEnum.HongBao ? <span>{l.value}元</span> : <span>{l.details.InterestRateBonus}%</span>}</p>
                                <p className="use-condition">投资{l.activity_type == LotteryTypeEnum.InterestRateTicket ? l.details.minInvestValue : l.details.InvestUntil}万元以上可用</p>
                                <p className="use-date">有效期至{l.details.Deadline}</p>
                            </div>
                            <div className="lottery-state">已过期</div>
                        </div>)
                        )}
                    </div>}
                        {/*<Pagination pageIndex={this.state.pageIndex} pageCount={this.state.pageCount}
                    onPageSelected={pageIndex => this.fetchLotteries(pageIndex)}/>   */}  
                </div>
                <div className="use-rules-th"><span>使用规则</span></div>
                <div className="use-rules">
                    <p>1.投资额需要达到一定数额才可使用红包，具体数值见红包说明文字。</p>
                    {/*<p>2.单笔投资可同时使用多张红包，使用时所需投资额为各个红包要求的起投金额相累加。</p>*/}
                    <p>2.安广融合有权根据运营情况调整红包使用规则，规则最终解释权归安广融合所有。</p>
                </div>
            </div>
        );
    }
}
export default MyLottery;