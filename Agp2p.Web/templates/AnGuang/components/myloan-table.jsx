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

const MyLoanQueryTypeEnum = {
    Applying : 1, // 申请中
    Loaning : 2, // 借款中
    Repaying : 3, // 还款中
    Repaid : 4, // 已还款
}

function getUrlParam(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
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
        this.fetch(
           getUrlParam("loanstatus") == 3 ? 3: getUrlParam("loanstatus") == 4 ? 4: 2,           
            this.props.pageIndex);
    }
    fetch(type, pageIndex, startTime = "", endTime = "") {
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryLoan", pageSize = 12;
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
    applyForAutoRepay(projectId, repayLimit) {
        confirm("是否确定开通自动还款", () => {
            location.href = `/api/payment/sumapay/index.aspx?api=${SumapayApiEnum.AcReO}&projectCode=${projectId}&repayLimit=${repayLimit}`;
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
                            <th>操作</th>
                        </tr>
                    </thead>
                    <tbody>
                        {this.state.data.length != 0 ? null : <tr><td colSpan="6">暂无数据</td></tr>}
                        { this.state.data.map(pr =>
                            <tr className="detailRow" key={pr.id}>
                                <td><a href={pr.url} target="_blank" title={pr.name}>{pr.name}</a></td>
                                <td>{pr.profitRateYearly}</td>
                                <td>{pr.nextRepayTime}</td>
                                <td>{pr.financingAmount}</td>
                                <td>{pr.totalProfit}</td>
                                {this.props.type != MyLoanQueryTypeEnum.Repaying ? <td /> :
                                <td>
                                    {pr.isAutoRepay
                                        ? <a href="javascript:" onClick={ev => this.applyForCanelAutoRepay(pr.id) }>取消自动还款</a>
                                        : <a href="javascript:" onClick={ev => this.applyForAutoRepay(pr.id, pr.repayLimit) }>开通自动还款</a> }
                                    <a href="javascript:" onClick={ev => this.ManualRepay(pr.id) }>手动还款</a>
                                </td>}
                            </tr>
                        ) }
                    </tbody>
                </table>
            </div>
        );
    }
};

export default MyloanTable;