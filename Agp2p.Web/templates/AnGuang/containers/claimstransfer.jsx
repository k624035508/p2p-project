import React from "react";
import { ajax } from "jquery";
import Pagination from "../components/pagination.jsx";
import "../less/claimstransfer.less";

class ClaimsTransfer extends React.Component{
    constructor(props){
        super(props);
        this.state = {};
    }

    render(){
        return(
            <div className="claimsTransferPage">
                <div className="top-wrapper">
                    <div className="topic-icon"><span>安融活期</span></div>
                    <div className="profit-container">
                        <p className="expected-profit-title">今日预期收益<span className="pull-right current-protocol">安融活期协议</span></p>
                        <p className="expected-profit">0.00 <span className="profit-unit">元</span></p>
                    </div>
                    <div></div>
                </div>
                <div className="bottom-wrapper">

                </div>
            </div>
        );
    }
}
export default ClaimsTransfer;