import React from "react";
import $ from "jquery";
import "../less/bootstrap-datetimepicker.css";
import "../js/bootstrap-datetimepicker.js";
import "../js/bootstrap-datetimepicker.zh-CN.js";


export default class DatePicker extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	componentDidMount() {
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
	render() {
		return (
			<div className="dateSel pull-right" ref="datepickers">
		        <input className="form_date" type="text" ref="startTimePicker" onChange={(ev) => this.props.onStartTimeChange(ev.target.value) }/>
		        到
		        <input className="form_date" type="text" ref="endTimePicker" onChange={(ev) => this.props.onEndTimeChange(ev.target.value)} />
		        <a href="#">搜 索</a>
		    </div>
		);
	}
}