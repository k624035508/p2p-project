import React from "react";
import { ajax } from "jquery";
import isEqual from "lodash/lang/isEqual"
import alert from "../components/tips_alert.js";
import CustomDlg from "../components/custom-dialog.jsx"
import confirm from "../components/tips_confirm.js";

const SumapayApiEnum = {
    AcReO: 7,
    ClRep: 9
}

class MyloanTable extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: [], };
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
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryLoan", pageSize = 15;
        ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({ type: type, pageIndex: pageIndex, pageSize: pageSize, startTime: startTime, endTime: endTime }),
            success: function (result) {
                let {totalCount, data} = JSON.parse(result.d);
                this.setState({ data: data });
                this.props.onPageLoaded(Math.ceil(totalCount / pageSize));
            }.bind(this),
            error: function (xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
    }
    applyForAutoRepay(projectId, financingAmount) {
        confirm("是否确定开通自动还款", () => {
            location.href = `/api/payment/sumapay/index.aspx?api=${SumapayApiEnum.AcReO}&projectCode=${projectId}&repayLimit=${financingAmount}`;
        });
    }
    applyForCanelAutoRepay(projectId) {
        confirm("是否确定取消自动还款", () => {
            location.href = `/api/payment/sumapay/index.aspx?api=${SumapayApiEnum.ClRep}&projectCode=${projectId}`;
        });
    }
    ManualRepay(projectId) {
        let url = USER_CENTER_ASPX_PATH + "/ManualRepay";
        ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({ projectId }),
            success: function (result) {
                let {status, msg, url} = JSON.parse(result.d);
                if (status == 0) {
                    alert(msg);
                } else {
                    location.href = url;
                }
            }.bind(this),
            error: function (xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
    }
    render() {
        return (
            <div className="tb-wrap">
                <table className="table loan-tb" >
                    <thead>
                        <tr>
                            <th>项目名称</th>
                            <th>年利率</th>
                            <th>还款日期</th>
                            <th>借款金额（元）</th>
                            <th>利息（元）</th>
                            <th>期数</th>
                            <th>操作</th>
                        </tr>
                    </thead>
                    <tbody>
                        {this.state.data.length != 0 ? null : <tr><td colSpan="7">暂无数据</td></tr>}
                        { this.state.data.map(tr =>
                            <tr className="detailRow" key={tr.ptrId}>
                                <td><a href={tr.projectUrl} target="_blank"
                                    title={tr.projectName}>{tr.projectName}</a></td>
                                <td>{tr.projectProfitRateYearly}</td>
                                <td>{tr.investTime == "01/01/01" ? "" : tr.investTime}</td>
                                <td>{tr.investValue}</td>
                                <td>{tr.profit}</td>
                                <td>1/1</td>
                                {this.props.type == 2 ?
                                    <td>
                                        {tr.autoRepay == true ? <a href="javascript:" onClick={ev => this.applyForCanelAutoRepay(tr.ptrId) }>取消自动还款</a>
                                            : <a href="javascript:" onClick={ev => this.applyForAutoRepay(tr.ptrId, tr.repayLimit) }>开通自动还款</a> }
                                        <a href="javascript:" onClick={ev => this.ManualRepay(tr.ptrId) }>手动还款</a>
                                    </td>
                                    : <td></td>}
                            </tr>
                        ) }
                    </tbody>
                </table>
            </div>
        );
    }
};

export default MyloanTable;