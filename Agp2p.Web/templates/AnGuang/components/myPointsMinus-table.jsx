import React from "react";
import { ajax } from "jquery";
import isEqual from "lodash/lang/isEqual";

class PointsMinusTable extends React.Component {
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
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryMinusPoints", pageSize = 4;
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
                            <th>兑换商品</th>
                            <th>消费积分</th>
                            <th>兑换时间</th>
                            <th>处理状态</th>
                        </tr>
                        </thead>
                        <tbody>
                        {this.state.data.length != 0 ? null : <tr><td colSpan="4">暂无数据</td></tr>}
        { this.state.data.map(tr =>
            <tr className="detailRow" key={tr.id}>
                <td>{tr.title}</td>
                <td className="sumPoint">{tr.point}</td>
                <td>{tr.add_time}</td>
                <td>{tr.status}</td>
                    </tr>
            )}
                    </tbody>
                </table>
            </div>
        );
                    }
}

export default PointsMinusTable;