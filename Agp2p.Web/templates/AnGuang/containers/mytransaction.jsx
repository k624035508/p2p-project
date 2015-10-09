import React from "react";

import Picker from "../components/dropdown-date-picker.jsx"
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
	        	<Picker enumFullName="Agp2p.Common.Agp2pEnums+TransactionDetailsDropDownListEnum"
	        			onTypeChange={newType => _this.setState({type: newType}) }
	        			onStartTimeChange={newStartTime => _this.setState({startTime: newStartTime})}
	        			onEndTimeChange={newEndTime => _this.setState({endTime: newEndTime})} />
	        	<TransactionTable
		        	url={USER_CENTER_ASPX_PATH + "/AjaxQueryTransactionHistory"}
		        	type={this.state.type}
		        	pageIndex={this.state.pageIndex}
		        	startTime={this.state.startTime}
		        	endTime={this.state.endTime} />
			</div>);
	}
}
