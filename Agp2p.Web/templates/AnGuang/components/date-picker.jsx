import React from "react";
import $ from "jquery";

import "bootstrap-datetime-picker/css/bootstrap-datetimepicker.css"
import "bootstrap-datetime-picker/js/bootstrap-datetimepicker.js"
import "bootstrap-datetime-picker/js/locales/bootstrap-datetimepicker.zh-CN.js"


export default class DatePicker extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	componentDidMount() {
		//日期设置
		var $datepickers = $(this.refs.datepickers).find(".form_date").datetimepicker({
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
		$datepickers.eq(0).on("changeDate", this.refs.startTimePicker.onBlur);
		$datepickers.eq(1).on("changeDate", this.refs.endTimePicker.onBlur);
	}
	render() {
		return (
			<div className="dateSel pull-right" ref="datepickers">
		        <input className="form_date" type="text" ref="startTimePicker" onBlur={(ev) => this.props.onStartTimeChange(ev.target.value) }/>
		        到
		        <input className="form_date" type="text" ref="endTimePicker" onBlur={(ev) => this.props.onEndTimeChange(ev.target.value)} />
		        <a href="javascript:;">搜 索</a>
		    </div>
		);
	}
}