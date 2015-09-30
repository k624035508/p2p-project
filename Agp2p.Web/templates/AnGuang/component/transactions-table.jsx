import React from "react";
import $ from "jquery";

var TransactionTable = React.createClass({
    getInitialState: function() {
        return {data: []};
    },
    componentDidMount: function() {
    	$.ajax({
    		type: "post",
    		dataType: "json",
    		contentType: "application/json",
    		url: this.props.url,
    		data: JSON.stringify(this.props.args),
    		success: function(data) {
    			this.setState({data: JSON.parse(data.d)});
    		}.bind(this),
    		error: function(xhr, status, err) {
    			console.error(this.props.url, status, err.toString());
    		}.bind(this)
    	});
    },
    render: function() {
        return (
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
                        <tr className="detailRow">
                            <td>{tr.type}</td>
                            <td>{tr.income}</td>
                            <td>{tr.outcome}</td>
                            <td>{tr.idleMoney}</td>
                            <td>{tr.createTime}</td>
                            <td>详情 <span className="glyphicon glyphicon-triangle-bottom" data-toggle="glyphicon-triangle-top"></span></td>
                        </tr>,
                        <tr className="detailMark"><td colSpan="6">备注：{tr.remark}</td></tr>]
                    )}
                </tbody>
            </table>);
    },
    componentDidUpdate: function () {
        //交易明细 详情符号翻转
        $(this.getDOMNode()).find(".detailRow").click(function(){
        	$(this).next("tr").toggle();
        	$(this).find(".glyphicon-triangle-bottom").toggleClass("glyphicon-triangle-top");
        });
    }
});

export default TransactionTable;