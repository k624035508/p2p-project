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
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryTransactionHistory", pageSize = 10;
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
    onDetailRowClick(ev) { //交易明细 详情符号翻转
        let $tr = $(ev.target.parentElement);
        $tr.next("tr").toggle();
        $tr.find(".glyphicon-triangle-bottom").toggleClass("glyphicon-triangle-top");
    }
    render() {
        return (
            <div className="tb-wrap">
                <table className="table trade-tb">
                    <thead>
                    <tr>
                        <th>交易类型</th>
                        <th>收入(元)</th>
                        <th>支出(元)</th>
                        <th>余额(元)</th>
                        <th>时间</th>
                        <th>操作</th>
                    </tr>
                    </thead>
                    <tbody>
                    { this.state.data.map(tr => [
                        <tr className="detailRow" onClick={this.onDetailRowClick} key={"a" + tr.id}>
                            <td>{tr.type}</td>
                            <td>{tr.income}</td>
                            <td>{tr.outcome}</td>
                            <td>{tr.idleMoney}</td>
                            <td>{tr.createTime}</td>
                            <td>详情 <span className="glyphicon glyphicon-triangle-bottom" data-toggle="glyphicon-triangle-top"></span></td>
                        </tr>,
                        <tr className="detailMark" key={"b" + tr.id}><td colSpan="6">备注：{tr.remark}</td></tr>]
                    )}
                    </tbody>
                </table>
            </div>
        );
    }
};
