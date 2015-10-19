import React from "react";
import DatePicker from "./date-picker.jsx"

export default class DateSpanPicker extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	render() {
		return (
			<div className="dateSel pull-right">
				<DatePicker onTimeChange={this.props.onStartTimeChange} />
		        到
				<DatePicker onTimeChange={this.props.onEndTimeChange} />
		        <a href="javascript:;">搜 索</a>
		    </div>
		);
	}
}