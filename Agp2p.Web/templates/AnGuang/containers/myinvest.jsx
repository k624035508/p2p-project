import React from "react"
import echarts from 'echarts/src/echarts'
import 'echarts/src/chart/bar'
import "../less/myinvest.less"

let option = {
    color: ["#37aaf0"],
    tooltip : {
        trigger: 'axis'
    },
    calculable : true,
    grid: {
        y: 10,
        y2: 30,
        borderWidth: 0
    },
    xAxis : [
        {
            type : 'category',
            data : ['1月','2月','3月','4月','5月','6月','7月','8月','9月','10月','11月','12月'],
            splitLine: { show: false }
        }
    ],
    yAxis : [
        {
            type : 'value',
            min: 0,
            max: 10000,
        }
    ],
    series : [
        {
            name:'投资',
            type:'bar',
            barWidth: 30,
            data:[500, 1000, 5000, 3000, 8500, 6000, 10000, 5000, 7000, 5500, 4000, 2000]
        }
    ]
};

export default class MyInvestPage extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }

    componentDidMount() {
        this.renderChart();
    }

    renderChart() {
        let myChart = echarts.init(this.refs.chartBox);
        myChart.setOption(option);

        let myChart2 = echarts.init(this.refs.chartBox2);
        myChart2.setOption(option);
    }

    render(){
        return(
            <div className="myinvest-wrap">
                <div className="total-invest">
                    <ul className="list-unstyled list-inline">
                        <li>
                            <p>累计投资</p>
                            <p>￥0.00</p>
                        </li>
                        <li className="operator">=</li>
                        <li>
                            <p>在投本金</p>
                            <p>￥0.00</p>
                        </li>
                        <li className="operator">+</li>
                        <li>
                            <p>已收本金</p>
                            <p>￥0.00</p>
                        </li>
                    </ul>
                    <div id="invest-bar" ref="chartBox"></div>
                </div>
                <div className="divider"></div>
                <div className="total-profit">
                    <ul className="list-unstyled list-inline">
                        <li>
                            <p>累计收益</p>
                            <p>￥0.00</p>
                        </li>
                        <li className="operator">=</li>
                        <li>
                            <p>待收益</p>
                            <p>￥0.00</p>
                        </li>
                        <li className="operator">+</li>
                        <li>
                            <p>已收益</p>
                            <p>￥0.00</p>
                        </li>
                    </ul>
                    <div id="profit-bar" ref="chartBox2"></div>
                </div>
            </div>
        );
    }
}