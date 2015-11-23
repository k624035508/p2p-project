import React from "react";
import echarts from 'echarts';
import 'echarts/chart/pie';
import isEqual from "lodash/lang/isEqual";

import "../less/myaccount.less";

const ProjectTagEnum = {
	Ordered : 1,
	Recommend : 2,
	CreditGuarantee : 4,
	Hot : 8,
	Trial : 16,
}

let genOption = ({idleMoney, lockedMoney, investingMoney, profitingMoney, lotteriesValue}) => ({
	color: ["#ff404a", "#f7a543", "#a5d858", "#35aaf1", "#fce435"],
	tooltip : {
		show: true,
		trigger: 'item',
		formatter: "{d}%"
	},
	legend: { show: false, data:[]},
	series : [
	{
		name:'我的账户',
		type:'pie',
		radius : [112, 125],
		minAngle: 5,

		data:[
			{value: Math.max(0.001, idleMoney), name: `可用余额：${idleMoney} 元`},
			{value: Math.max(0.001, lockedMoney), name: `冻结金额：${lockedMoney} 元`},
			{value: Math.max(0.001, lotteriesValue), name: `红包金额：${lotteriesValue} 元`},
			{value: Math.max(0.001, profitingMoney), name: `待收益：${profitingMoney} 元 `},
			{value: Math.max(0.001, investingMoney), name: `待收本金：${investingMoney} 元 `}
		]
	}
	]
});

const ProjectStatusEnum = {
	FinancingAtTime: 10,
	Financing: 11,
	FinancingSuccess: 21,
	ProjectRepaying: 30,
	RepayCompleteIntime: 40
};

const ProjectStatusEnumDesc = {
	10: "待发标",
	11: "立即投资",
	20: "审核中", // 已过期
	21: "审核中", // 满标
	30: "还款中",
	40: "已完成"
};

class MyAccount extends React.Component {
	constructor(props) {
		super(props);
		this.state = {option: null, recommendProjects: []};
	}
	componentDidMount() {
		this.renderChart(genOption(this.props));
		this.fetchRecommendProject();
	}
	componentWillReceiveProps(nextProps) {
        if (!isEqual(this.props, nextProps)) {
        	var newOption = genOption(nextProps);
        	if (!isEqual(this.state.option, newOption)) {
        		this.renderChart(newOption);
        	}
        }
	}
	renderChart(option) {
		this.setState({option: option});
		let myChart = echarts.init(this.refs.chartBox);
		myChart.setOption(option);
	}
	fetchRecommendProject() {
		let url = USER_CENTER_ASPX_PATH + "/AjaxQueryProjectsDetail";
		$.ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: JSON.stringify({ pageIndex: 0, pageSize: 1}),
			success: function(result) {
				let {totalCount, data} = JSON.parse(result.d);
				this.setState({recommendProjects: data || []});
			}.bind(this),
			error: function(xhr, status, err) {
				console.error(url, status, err.toString());
			}.bind(this)
		});
	}
	render() {
		var totalMoneySplitted = this.props.totalMoney.toFixed(2).toString().split(".");
		return(
			<div className="overview-wrap">
				<div id="data-pie" ref="chartBox"></div>
				<div className="total-amount">
					<p>{totalMoneySplitted[0]} {"." + (totalMoneySplitted[1] || "00") + "元"}</p>
					<p>总资产</p>
				</div>
				<div className="recommend-project"><span>推荐项目</span></div>
				{this.state.recommendProjects.length == 0
					? <div style={{textAlign: "center", lineHeight: "60px", color: "#646464", fontSize: "15px"}}>暂无项目</div>
					: this.state.recommendProjects.map(pro => 
				<div className="invest-cell invest-cell-custom" key={pro.id}>
					<div className="invest-title-wrap">
						{pro.categoryCallIndex!="ypb" ? null:
						<span className="pull-right acceptance-bank"><i></i>承兑银行：{pro.conversionBank}</span>}
						<span className="invest-style-tab">{pro.categoryTitle}</span>
						<span className="invest-title"><a href={`/project/${pro.id}.html`} target="_blank">{pro.title}</a></span>
						{(pro.tag & ProjectTagEnum.Recommend) == 0 ? null : <span className="invest-list-icon jian-icon"></span>}
						{(pro.tag & ProjectTagEnum.Ordered) == 0 ? null : <span className="invest-list-icon yue-icon"></span>}
						{(pro.tag & ProjectTagEnum.CreditGuarantee) == 0 ? null : <span className="invest-list-icon xin-icon"></span>}
					</div>
					<div className="invest-content">
						<div className="apr">
							<div className="red25px margin-bottom10px">{pro.profit_rate_year}<span className="red15px">%</span></div>
							<div className="grey13px">年化利率</div>
						</div>
						<div className="deadline">
							<div className="grey25px margin-bottom10px">{pro.repayment_number}<span className="grey15px">{pro.repayment_term}</span></div>
							<div className="grey13px">期限</div>
						</div>
						<div className="sum">
							<div className="grey25px margin-bottom10px">{pro.project_amount_str}<span className="grey15px">元</span></div>
							<div className="grey13px">借款金额</div>
						</div>
						<div className="repayment">
							<div className="progress progress-custom">
								<div className="progress-bar progress-bar-info" role="progressbar" aria-valuenow="20"
									aria-valuemin="0" aria-valuemax="100" style={{width: pro.project_investment_progress + "%"}}>
									<span className="sr-only">{`${pro.project_investment_progress}% Complete`}</span>
								</div>
							</div>
							<div className="grey13px margin-bottom10px">可投金额 : <span className="dark-grey13px">{pro.project_investment_balance}</span></div>
							<div className="grey13px margin-bottom10px hidden">投资人数 : <span className="dark-grey13px">{pro.project_investment_count}人</span></div>
							<div className="grey13px">到期还本付息</div>
						</div>
						<div className="invest-btn">
							<a className={pro.status == ProjectStatusEnum.Financing ? "invest-now-btn" : "invest-full-btn"}
							   href={`/project/${pro.id}.html`} target="_blank" >{ProjectStatusEnumDesc[pro.status]}</a>
						</div>
					</div>
				</div>)}
			</div>
		);
	}
}

import assign from "lodash/object/assign"

function mapStateToProps(state) {
	let walletInfo = state.walletInfo;
	let totalMoney = walletInfo.idleMoney + walletInfo.lockedMoney + walletInfo.investingMoney +
		walletInfo.profitingMoney + walletInfo.lotteriesValue;

	return assign({totalMoney}, walletInfo);
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(MyAccount);