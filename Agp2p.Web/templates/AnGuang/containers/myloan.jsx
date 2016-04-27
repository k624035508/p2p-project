import React from "react";
import "../less/myloan.less";

import HorizontalPicker from "../components/horizontal-picker.jsx";
import DateSpanPicker from "../components/date-span-picker.jsx";
import Table from "../components/myloan-table.jsx";
import Pagination from "../components/pagination.jsx";

const MyLoanQueryTypeEnum = {
    Applying : 1, // 申请中
    Loaning : 2, // 借款中
    Repaying : 3, // 还款中
    Repaid : 4, // 已还款
}

class MyTransaction extends React.Component {
    constructor(props) {
        super(props);
        this.state = { type: MyLoanQueryTypeEnum.Applying, startTime: "", endTime: "", pageIndex: 0,
            onPageLoaded: pageCount => this.setState({ pageCount: pageCount }) };
    }
    render() {
        return (
            <div>
                <div className="controls">
                    <HorizontalPicker onTypeChange={newType => this.setState({ type: newType }) }
                        enumFullName="Agp2p.Common.Agp2pEnums+MyLoanQueryTypeEnum"
                        value={this.state.type} />
                    <DateSpanPicker
                        onStartTimeChange={newStartTime => this.setState({ startTime: newStartTime }) }
                        onEndTimeChange={newEndTime => this.setState({ endTime: newEndTime }) }/>
                    <div style={{ clear: "both" }}></div>
                </div>
                <Table
                    type={this.state.type}
                    pageIndex={this.state.pageIndex}
                    startTime={this.state.startTime}
                    endTime={this.state.endTime}
                    onPageLoaded={this.state.onPageLoaded} />
                <Pagination pageIndex={this.state.pageIndex} pageCount={this.state.pageCount}
                    onPageSelected={pageIndex => this.setState({ pageIndex: pageIndex }) }/>
            </div>
        );
    }
}

export default MyTransaction;
