import "bootstrap-webpack";
import $ from "jquery";
import "../less/bootstrap-datetimepicker.css";
import "../js/bootstrap-datetimepicker.js";
import "../js/bootstrap-datetimepicker.zh-CN.js";

import React from "react";

export default class Picker extends React.Component {
	constructor(props) {
		super(props);
		this.state = {options: []};
	}
	render() {
		var _this = this;
		return (
		<div className="controls">
		    <select name="tradeType" id="typeSel" onChange={(ev) => _this.props.onTypeChange(ev.target.value) }>
		    	{this.state.options.map(op => <option key={op.value} value={op.value}>{op.key}</option>)}
		    </select>
		    <div className="dateSel pull-right" ref="datepickers">
		        <input className="form_date" type="text" ref="startTimePicker" onChange={(ev) => _this.props.onStartTimeChange(ev.target.value) }/>
		        到
		        <input className="form_date" type="text" ref="endTimePicker" onChange={(ev) => _this.props.onEndTimeChange(ev.target.value)} />
		        <a href="#">搜 索</a>
		    </div>
		    <div style={{clear: "both"}}></div>
		</div>);
	}
	componentDidMount() {
		$.ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: this.props.url,
			data: JSON.stringify({ enumFullName: this.props.enumFullName}),
			success: function(data) {
				this.setState({options: JSON.parse(data.d)});
			}.bind(this),
			error: function(xhr, status, err) {
				console.error(this.props.url, status, err.toString());
			}.bind(this)
		});
		//日期设置
		var $datepickers = $(this.refs.datepickers.getDOMNode()).find(".form_date").datetimepicker({
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
		$datepickers.eq(0).on("changeDate", (ev) => this.refs.startTimePicker.props.onChange(ev));
		$datepickers.eq(1).on("changeDate", (ev) => this.refs.endTimePicker.props.onChange(ev));
	}
};

