import React from "react";
import echarts from 'echarts/src/echarts';
import 'echarts/src/chart/pie';
import isEqual from "lodash/lang/isEqual"

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
			{value: Math.max(0.001, investingMoney), name: `红包金额：${investingMoney} 元`},
			{value: Math.max(0.001, profitingMoney), name: `待收益：${profitingMoney} 元 `},
			{value: Math.max(0.001, lotteriesValue), name: `待收本金：${lotteriesValue} 元 `}
		]
	}
	]
});


class MyAccount extends React.Component {
	constructor(props) {
		super(props);
		this.state = {option: null};
	}
	componentDidMount() {
		this.renderChart(genOption(this.props))
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
		let myChart = echarts.init(document.getElementById("data-pie"));
		myChart.setOption(option);
	}
	render() {
		var totalMoneySplitted = this.props.totalMoney.toString().split(".");
		return(
			<div className="overview-wrap">
				<div id="data-pie"></div>
				<div className="total-amount">
					<p>{totalMoneySplitted[0]}{"." + (totalMoneySplitted[1] || "00") + "元"}</p>
					<p>总资产</p>
				</div>
				<div className="recommend-project"><span>推荐项目</span></div>
				<div className="invest-cell invest-cell-custom">
					<div className="invest-title-wrap">
						<span className="invest-style-tab">房贷宝</span>
						<span className="invest-title"><a href="invest_detail.html">石化供应链系列-XXX公司应收</a></span>
						<span className="invest-list-icon jian-icon"></span>
						<span className="invest-list-icon yue-icon"></span>
						<span className="invest-list-icon xin-icon"></span>
					</div>
					<div className="invest-content">
						<div className="apr">
							<div className="red25px margin-bottom10px">13.00<span className="red15px">%</span></div>
							<div className="grey13px">年化利率</div>
						</div>
						<div className="deadline">
							<div className="grey25px margin-bottom10px">1<span className="grey15px">个月</span></div>
							<div className="grey13px">期限</div>
						</div>
						<div className="sum">
							<div className="grey25px margin-bottom10px">456<span className="grey15px">万</span></div>
							<div className="grey13px">借款金额</div>
						</div>
						<div className="repayment">
							<div className="progress progress-custom">
								<div className="progress-bar progress-bar-info" role="progressbar" aria-valuenow="20" aria-valuemin="0" aria-valuemax="100" style={{width: "20%"}}>
									<span className="sr-only">20% Complete</span>
								</div>
							</div>
							<div className="grey13px margin-bottom10px">可投金额 : <span className="dark-grey13px">2,300,500.00元</span></div>
							<div className="grey13px margin-bottom10px hidden">投资人数 : <span className="dark-grey13px">25人</span></div>   {/*满标人数显示*/}
							<div className="grey13px">到期还本付息</div>
						</div>
						<div className="invest-btn">
							<button type="button" className="invest-now-btn">立即投资</button>
							<button type="button" className="invest-full-btn hidden">满标</button>   {/*满标按钮显示*/}
						</div>
					</div>
				</div>

			</div>
		);
	}
}

function mapStateToProps(state) {
	var walletInfo = state.walletInfo;
	return {
		totalMoney: walletInfo.idleMoney + walletInfo.lockedMoney + walletInfo.investingMoney + walletInfo.profitingMoney + walletInfo.lotteriesValue,
		...walletInfo
	};
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(MyAccount);