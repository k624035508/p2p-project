import React from "react";
import DatePicker from "./date-picker.jsx"

class DateSpanPicker extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	render() {
		return (
			<div className="dateSel pull-right">
				<DatePicker onBlur={ev => this.props.onStartTimeChange(ev.target.value)} />
		        到
				<DatePicker onBlur={ev => this.props.onEndTimeChange(ev.target.value)} />
		    </div>
		);
	}
}
export default DateSpanPicker;