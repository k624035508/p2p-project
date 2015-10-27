import React from "react";
import echarts from 'echarts/src/echarts';
import 'echarts/src/chart/pie';

let option = {
	tooltip : {
		//关闭提示框
		show: false,
		trigger: 'item',
		formatter: "{a} <br/>{b} : {c} ({d}%)"
	},
	legend: {
		//关闭左侧图例说明
		show: false,
		orient : 'vertical',
		x : 'left',
		data:['直达','营销广告','搜索引擎','邮件营销','联盟广告','视频广告','百度','谷歌','必应','其他']
	},
	toolbox: {
		//关闭顶部工具栏
		show : false,
		feature : {
			mark : {show: true},
			dataView : {show: true, readOnly: false},
			magicType : {
				show: true,
				type: ['pie', 'funnel']
			},
			restore : {show: true},
			saveAsImage : {show: true}
		}
	},
	calculable : false,
	series : [
		{
			name:'访问来源',
			type:'pie',
			selectedMode: 'single',
			radius : [0, 70],

			// for funnel
			x: '20%',
			width: '40%',
			funnelAlign: 'right',
			max: 1548,

			itemStyle : {
				normal : {
					label : {
						position : 'inner'
					},
					labelLine : {
						show : false
					}
				}
			},
			//内环数据
			data:[]
		},
		{
			name:'访问来源',
			type:'pie',
			radius : [112, 125],

			// for funnel
			x: '60%',
			width: '30%',
			funnelAlign: 'left',
			max: 1048,

			data:[
				{value:500, name:'可用余额：500元'},
				{value:300, name:'冻结金额：300元'},
				{value:100, name:'红包金额：100元'},
				{value:100, name:'待收益：100元'},
				{value:100, name:'待收本金：100元'}
			]
		}
	]
};


export default class MyAccount extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	componentDidMount() {
		let mychart = echarts.init(document.getElementById("data-pie"));
		mychart.setOption(option);
	}
	render() {
		return(
			<div className="overview-wrap">
				<div id="data-pie"></div>
				<div className="total-amount">
					<p><span>0</span>.00元</p>
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