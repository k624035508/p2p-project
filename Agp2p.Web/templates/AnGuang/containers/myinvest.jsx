import "../less/myinvest.less"

import React from "react"
import { ajax } from "jquery"
import keys from "lodash/object/keys"
import values from "lodash/object/values"
import echarts from 'echarts'
import 'echarts/chart/bar'

const Tab0 = ["累计投资", "在投本金", "已收本金"];
const Tab1 = ["累计收益", "待收益", "已收益"];

let genOption = (chartName, barData) => ({
    color: ["#37aaf0"],
    tooltip : {
        trigger: 'axis'
    },
    grid: {
        y: 10,
        y2: 30,
        borderWidth: 0
    },
    xAxis : [
        {
            type : 'category',
            data : barData.map(b => keys(b)[0]),
            splitLine: { show: false },
            axisLabel: { interval: 0 }
        }
    ],
    yAxis : [
        {
            type : 'value',
        }
    ],
    series : [
        {
            name: chartName,
            type:'bar',
            barWidth: 30,
            data: barData.map(b => values(b)[0])
        }
    ]
});

class MyInvestPage extends React.Component {
    constructor(props) {
        super(props);
        this.state = {tabIndex0: -1, tabIndex1: -1 };
    }
    componentDidMount() {
    	this.loadChart0(0);
    	this.loadChart1(0);
    }
    loadChart0(type) {
    	if (this.state.tabIndex0 == type) return;
    	this.fetchChartData(type).done(result => {
        	let chartData = JSON.parse(result.d), chart = echarts.init(this.refs.chartBox);
            chart.setOption(genOption(Tab0[type], chartData));
            this.setState({tabIndex0: type});
        });
    }
    loadChart1(type) {
    	if (this.state.tabIndex1 == type) return;
    	this.fetchChartData(3 + type).done(result => {
        	let chartData = JSON.parse(result.d), chart = echarts.init(this.refs.chartBox2);
            chart.setOption(genOption(Tab1[type], chartData));
            this.setState({tabIndex1: type});
        });
    }
    fetchChartData(type) {
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryMouthlyHistory";
        return ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({type: type}),
            success: function(result) {
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
    }
    render(){
        return(
            <div className="myinvest-wrap">
                <div className="total-invest">
                    <ul className="list-unstyled list-inline">
                        <li className={"myinvest-tab " + (this.state.tabIndex0 == 0 ? "active" : "")}
	                        onClick={ev => this.loadChart0(0)}>
                            <p>累计投资</p>
                            <p>￥{this.props.totalInvestment}</p>
                        </li>
                        <li className="operator">=</li>
                        <li className={"myinvest-tab " + (this.state.tabIndex0 == 1 ? "active" : "")}
                        	onClick={ev => this.loadChart0(1)}>
                            <p>在投本金</p>
                            <p>￥{this.props.investingMoney}</p>
                        </li>
                        <li className="operator">+</li>
                        <li className={"myinvest-tab " + (this.state.tabIndex0 == 2 ? "active" : "")}
                        	onClick={ev => this.loadChart0(2)}>
                            <p>已收本金</p>
                            <p>￥{this.props.totalInvestment - this.props.investingMoney}</p>
                        </li>
                    </ul>
                    <div id="invest-bar" ref="chartBox"></div>
                </div>
                <div className="divider"></div>
                <div className="total-profit">
                    <ul className="list-unstyled list-inline">
                        <li className={"myinvest-tab " + (this.state.tabIndex1 == 0 ? "active" : "")}
                        	onClick={ev => this.loadChart1(0)}>
                            <p>累计收益</p>
                            <p>￥{this.props.totalProfit + this.props.profitingMoney}</p>
                        </li>
                        <li className="operator">=</li>
                        <li className={"myinvest-tab " + (this.state.tabIndex1 == 1 ? "active" : "")}
                        	onClick={ev => this.loadChart1(1)}>
                            <p>待收益</p>
                            <p>￥{this.props.profitingMoney}</p>
                        </li>
                        <li className="operator">+</li>
                        <li className={"myinvest-tab " + (this.state.tabIndex1 == 2 ? "active" : "")}
                        	onClick={ev => this.loadChart1(2)}>
                            <p>已收益</p>
                            <p>￥{this.props.totalProfit}</p>
                        </li>
                    </ul>
                    <div id="profit-bar" ref="chartBox2"></div>
                </div>
            </div>
        );
    }
}

function mapStateToProps(state) {
    return state.walletInfo;
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(MyInvestPage);