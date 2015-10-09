import React from "react";

import DropdownPicker from "../components/dropdown-picker.jsx"
import DatePicker from "../components/date-picker.jsx"
import TransactionTable from "../components/transactions-table.jsx"

export default class MyTransaction extends React.Component {
	constructor(props) {
		super(props);
		this.state = {type: 0, startTime: "", endTime: "", pageIndex: 0};
	}
	render() {
		var _this = this;
		return (
			<div>
				<div className="controls">
					<DropdownPicker onTypeChange={newType => _this.setState({type: newType}) } enumFullName="Agp2p.Common.Agp2pEnums+TransactionDetailsDropDownListEnum" />
					<DatePicker onStartTimeChange={newStartTime => _this.setState({startTime: newStartTime})} onEndTimeChange={newEndTime => _this.setState({endTime: newEndTime})}/>
					<div style={{clear: "both"}}></div>
				</div>
	        	<TransactionTable
		        	url={USER_CENTER_ASPX_PATH + "/AjaxQueryTransactionHistory"}
		        	type={this.state.type}
		        	pageIndex={this.state.pageIndex}
		        	startTime={this.state.startTime}
		        	endTime={this.state.endTime} />
			</div>);
	}
}
