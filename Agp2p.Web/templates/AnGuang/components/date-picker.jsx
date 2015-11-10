import React from "react";

import "bootstrap-datetime-picker/css/bootstrap-datetimepicker.css"
import "bootstrap-datetime-picker/js/bootstrap-datetimepicker.js"
import "bootstrap-datetime-picker/js/locales/bootstrap-datetimepicker.zh-CN.js"


class DatePicker extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	componentDidMount() {
		//日期设置
		var $datepickers = $(this.refs.picker).datetimepicker({
			language: 'zh-CN',
			format: 'yyyy-mm-dd',
			weekStart: 1,
			todayBtn: true,
			todayHighlight: 1,
			startView: 2,
			forceParse: 0,
			showMeridian: 1,
			autoclose: 1,
			minView: 2,
			bootcssVer: 3
		}).on("changeDate", this.props.onBlur);
	}
	render() {
		return <input type="text" ref="picker" {...this.props} />;
	}
}
export default DatePicker;