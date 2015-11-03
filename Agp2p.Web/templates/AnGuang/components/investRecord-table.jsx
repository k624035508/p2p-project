import React from "react";
import $ from "jquery";
import isEqual from "lodash/lang/isEqual"

class InvestRecordTable extends React.Component {
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
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryInvestment", pageSize = 10;
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
                <table className="table investRecord-tb" ref="table">
                    <thead>
                    <tr>
                        <th>项目名称</th>
                        <th>年利率</th>
                        <th>期限</th>
                        <th>投资金额（元）</th>
                        <th>利息（元）</th>
                        <th>状态</th>
                        <th>投资日期</th>
                        <th>投标协议</th>
                    </tr>
                    </thead>
                    <tbody>
                    { this.state.data.map(tr =>
                        <tr className="detailRow" key={tr.ptrId}>
                            <td>{tr.projectName}</td>
                            <td>{tr.projectProfitRateYearly}%</td>
                            <td>{tr.term}</td>
                            <td>{tr.investValue}</td>
                            <td>{tr.profit}</td>
                            <td>{tr.status}</td>
                            <td>{tr.investTime}</td>
                            <td><a href="javascript:;">查看</a></td>
                        </tr>
                    )}
                    </tbody>
                </table>
            </div>
        );
    }
};
export default InvestRecordTable;