import React from "react";

import HorizontalPicker from "../components/horizontal-picker.jsx"
import DatePicker from "../components/date-picker.jsx"
import Table from "../components/investRecord-table.jsx"

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
                    <HorizontalPicker onTypeChange={newType => _this.setState({type: newType}) } enumFullName="Agp2p.Common.Agp2pEnums+MyInvestRadioBtnTypeEnum" />
                    <DatePicker onStartTimeChange={newStartTime => _this.setState({startTime: newStartTime})} onEndTimeChange={newEndTime => _this.setState({endTime: newEndTime})}/>
                    <div style={{clear: "both"}}></div>
                </div>
                <Table
                    url={USER_CENTER_ASPX_PATH + "/AjaxQueryTransactionHistory"}
                    type={this.state.type}
                    pageIndex={this.state.pageIndex}
                    startTime={this.state.startTime}
                    endTime={this.state.endTime} />
            </div>);
    }
}
