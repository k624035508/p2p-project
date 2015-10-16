import React from "react";
import $ from "jquery";
import isEqual from "lodash/lang/isEqual"

export default class TransactionTable extends React.Component {
    constructor(props) {
        super(props);
        this.state = {data: []};
    }
    componentWillReceiveProps(nextProps) {
        if (!isEqual(this.props, nextProps)) {
            this.fetch(nextProps.type, nextProps.pageIndex, nextProps.startTime, nextProps.endTime);
        }
    }
    fetch(type, pageIndex, startTime = "", endTime = "") {
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryRepayments", pageSize = 10;
        $.ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({type: type, pageIndex: pageIndex, pageSize: pageSize, startTime: startTime, endTime: endTime}),
            success: function(result) {
                let {totalCount, data} = JSON.parse(result.d);
                this.setState({data: data});
                this.props.onPageLoaded(Math.ceil(totalCount / pageSize));
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
    }
    componentDidMount() {
        this.fetch(this.props.type, this.props.pageIndex);
    }
    render() {
        return (
            <div className="tb-wrap">
                <table className="table trade-tb">
                    <thead>
                    <tr>
                        <th>项目名称</th>
                        <th>年利率</th>
                        <th>投资金额</th>
                        <th>回款日期</th>
                        <th>支付本金</th>
                        <th>支付利息</th>
                        <th>期数</th>
                    </tr>
                    </thead>
                    <tbody>
                    { this.state.data.map(tr => 
                        <tr className="detailRow" key={tr.RepaymentId}>
                            <td>{tr.Project == null ? "" : tr.Project.Name}</td>
                            <td>{tr.Project == null ? "" : tr.Project.ProfitRateYear}</td>
                            <td>{tr.Project == null ? "" : tr.Project.InvestValue}</td>
                            <td>{tr.ShouldRepayDay}</td>
                            <td>{tr.RepayPrincipal}</td>
                            <td>{tr.RepayInterest}</td>
                            <td>{tr.Term}</td>
                        </tr>
                    )}
                    </tbody>
                </table>
            </div>
        );
    }
};
