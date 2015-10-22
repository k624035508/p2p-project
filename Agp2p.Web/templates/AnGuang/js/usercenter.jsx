import "bootstrap-webpack";
import "../less/head.less";
import "../less/usercenter.less";
import "../less/footerSmall.less";

import React from "react"
import { render } from "react-dom"
import { Router, Route } from 'react-router'
import { createStore } from 'redux';
import { Provider } from 'react-redux';

import userCenter from "../reducers/usercenter.js"

import UserCenterPage from "../containers/UserCenterPage.jsx"
import StatusContainer from "../containers/user-status.jsx"
import MyTransaction from "../containers/mytransaction.jsx"
import RechargePage from "../containers/recharge.jsx"
import WithdrawPage from "../containers/withdraw.jsx"
import InvestRecordPage from "../containers/invest-record.jsx"
import MyRepaymentsPage from "../containers/myrepayments.jsx"
import MyAccountPage from "../containers/myaccount.jsx"
import SafeCenterPage from "../containers/safe.jsx"
import BankAccountPage from "../containers/bankaccount.jsx"
import RecommendPage from "../containers/recommend.jsx"

import { setHeaderHighlight } from "./header.js"


$(function(){
    //点击导航加载相应内容
    render((
    	<Provider store={createStore(userCenter)}>
	    	<Router>
		    	<Route path="/" component={UserCenterPage}>
		    		<Route path="/status" component={StatusContainer}>
				    	<Route path="/mytrade" component={MyTransaction}/>
				    	<Route path="/recharge" component={RechargePage}/>
				    	<Route path="/withdraw" component={WithdrawPage}/>
				    	<Route path="/invest-record" component={InvestRecordPage}/>
				    	<Route path="/myrepayments" component={MyRepaymentsPage}/>
				    	<Route path="/myaccount" component={MyAccountPage}/>
						<Route path="/bankaccount" component={BankAccountPage}/>
						<Route path="/recommend" component={RecommendPage}/>
					</Route>
			    	<Route path="/safe" component={SafeCenterPage}/>
		    	</Route>
	    	</Router>
    	</Provider>
	), document.getElementById("app"));
    
	setHeaderHighlight(2);
});