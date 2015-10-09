import React from "react";

import DropdownPicker from "./dropdown-picker.jsx"
import DatePicker from "./date-picker.jsx"

export default class DropdownAndDatePicker extends React.Component {
	constructor(props) {
		super(props);
		this.state = {};
	}
	render() {
		return (
			<div className="controls">
				<DropdownPicker onTypeChange={this.props.onTypeChange} enumFullName={this.props.enumFullName} />
			    <DatePicker onStartTimeChange={this.props.onStartTimeChange} onEndTimeChange={this.props.onEndTimeChange}/>
			    <div style={{clear: "both"}}></div>
			</div>);
	}
};

