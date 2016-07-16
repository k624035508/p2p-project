import React from "react";
import { ajax } from "jquery";
import isEqual from "lodash/lang/isEqual";

class PointsTable extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: [] };
    }
    componentWillReceiveProps(nextProps) {
        if (!isEqual(this.props, nextProps)) {
            this.fetch(nextProps.pageIndex, nextProps.startTime, nextProps.endTime);
        }
    }
    componentDidMount() {
        this.fetch(this.props.pageIndex);
    }
    fetch(pageIndex, startTime="", endTime="") {
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryPoints", pageSize = 4;
        ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({pageIndex: pageIndex, pageSize: pageSize, startTime: startTime, endTime: endTime}),
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
        return(
            <div className="tb-wrap">
                <table className="table pointsRecord-tb" ref="table">
                    <thead>
                    <tr>
                        <th>积分变更说明</th>
                        <th>类型</th>
                        <th>变更积分</th>
                        <th>时间</th>
                    </tr>
                    </thead>
                    <tbody>
                    {this.state.data.length != 0 ? null : <tr><td colSpan="4">暂无数据</td></tr>}
        { this.state.data.map(tr =>
                <tr className="detailRow" key={tr.id}>
                    <td>{tr.remark}</td>
                    <td>{tr.type}</td>
                    <td>+{tr.value}</td>
                    <td>{tr.add_time}</td>
                        </tr>
            )}
                    </tbody>
                </table>
            </div>
        );
    }
}

export default PointsTable;