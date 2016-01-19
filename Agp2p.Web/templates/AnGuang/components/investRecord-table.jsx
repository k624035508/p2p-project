import React from "react";
import { ajax } from "jquery";
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
    componentDidMount() {
        this.fetch(this.props.type, this.props.pageIndex);
    }
    fetch(type, pageIndex, startTime = "", endTime = "") {
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryInvestment", pageSize = 10;
        ajax({
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
                        <th>投资时间</th>
                        <th>投标协议</th>
                    </tr>
                    </thead>
                    <tbody>
                    {this.state.data.length != 0 ? null : <tr><td colSpan="8">暂无数据</td></tr>}
                    { this.state.data.map(tr =>
                        <tr className="detailRow" key={tr.ptrId}>
                            <td><a href={tr.projectUrl} target="_blank" title={tr.projectName}>{tr.projectName}</a></td>
                            <td>{tr.projectProfitRateYearly}</td>
                            <td>{tr.term}</td>
                            <td>{tr.investValue}</td>
                            <td>{tr.profit}</td>
                            <td>{tr.status}</td>
                            <td>{tr.investTime}</td>
                            <td>{tr.isNewbieProject ? null : <a href={`/tools/submit_ajax.ashx?action=generate_user_invest_contract&id=${tr.ptrId}`} target="_blank">查看</a>}</td>
                        </tr>
                    )}
                    </tbody>
                </table>
            </div>
        );
    }
};
export default InvestRecordTable;