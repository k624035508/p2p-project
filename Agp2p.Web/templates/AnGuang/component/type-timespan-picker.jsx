import "bootstrap-webpack";
import $ from "jquery";
import "../less/bootstrap-datetimepicker.css";
import "../js/bootstrap-datetimepicker.js";
import "../js/bootstrap-datetimepicker.zh-CN.js";

import React from "react";

export default React.createClass({
	getInitialState: function() {
		return {options: []};
	},
	render: function () {
		return (
		<div className="controls">
		    <select name="tradeType" id="typeSel">
		    	{this.state.options.map(op => <option key={op.value} value={op.value}>{op.key}</option>)}
		    </select>
		    <div className="dateSel pull-right">
		        <input className="form_date" type="text" /> 到
		        <input className="form_date" type="text" />
		        <a href="#">搜 索</a>
		    </div>
		    <div style={{clear: "both"}}></div>
		</div>);
	},
	componentDidMount: function () {
		$.ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: this.props.url,
			data: JSON.stringify(this.props.args),
			success: function(data) {
				this.setState({options: JSON.parse(data.d)});
			}.bind(this),
			error: function(xhr, status, err) {
				console.error(this.props.url, status, err.toString());
			}.bind(this)
		});
		//日期设置
		$(this.getDOMNode()).find(".form_date").datetimepicker({
			language: 'zh-CN',
			format: 'yyyy-mm-dd',
			weekStart: 1,
			todayBtn: true,
			todayHighlight: 1,
			startView: 2,
			forceParse: 0,
			showMeridian: 1,
			autoclose: 1,
			minView: 2
		});
	}
});

