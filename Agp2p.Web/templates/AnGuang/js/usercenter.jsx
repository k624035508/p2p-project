import "bootstrap-webpack";
import "../less/head.less";
import "../less/usercenter.less";
import "../less/footerSmall.less";

import React from "react"
import ReactDOM from "react-dom"
import { Router, Route, Link } from 'react-router'

import UserCenterPage from "../containers/UserCenterPage.jsx"
import MyTransaction from "../containers/mytransaction.jsx"
import RechargePage from "../containers/recharge.jsx"
import WithdrawPage from "../containers/withdraw.jsx"
import InvestRecordPage from "../containers/invest-record.jsx"
import MyRepaymentsPage from "../containers/myrepayments.jsx"

import { setHeaderHighlight } from "./header.js"

$(function(){
    //点击导航加载相应内容
    ReactDOM.render((
    	<Router>
	    	<Route path="/" component={UserCenterPage}>
		    	<Route path="mytrade" component={MyTransaction}/>
		    	<Route path="recharge" component={RechargePage}/>
		    	<Route path="withdraw" component={WithdrawPage}/>
		    	<Route path="invest-record" component={InvestRecordPage}/>
		    	<Route path="myrepayments" component={MyRepaymentsPage}/>
	    	</Route>
    	</Router>
	), document.getElementById("app"));
    
	setHeaderHighlight(2);
});