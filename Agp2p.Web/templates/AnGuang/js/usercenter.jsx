import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/usercenter.less";
import "../less/invest-cell.less";
import "../less/footerSmall.less";

import React from "react"
import ReactDom from "react-dom"
import { Router, Route } from 'react-router'
import thunkMiddleware from 'redux-thunk';
import { createStore, applyMiddleware } from 'redux';
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
import InvitationPage from "../containers/invitation.jsx"
import MyNewsPage from "../containers/mynews.jsx"
import SettingsPage from "../containers/settings.jsx"
import MyLotteryPage from "../containers/mylottery.jsx"
import MyInvestPage from "../containers/myinvest.jsx"

import header from "./header.js"
window['$'] = $;


$(function(){
	//弹出窗popover初始化
	$('[data-toggle="popover"]').popover();

	const createStoreWithMiddleware = applyMiddleware(thunkMiddleware)(createStore);
	const store = createStoreWithMiddleware(userCenter);
	
	//点击导航加载相应内容
	ReactDom.render((
		<Provider store={store}>
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
						<Route path="/invitation" component={InvitationPage}/>
						<Route path="/mynews" component={MyNewsPage}/>
						<Route path="/settings" component={SettingsPage}/>
						<Route path="/mylottery" component={MyLotteryPage}/>
						<Route path="/myinvest" component={MyInvestPage}/>
					</Route>
					<Route path="/safe" component={SafeCenterPage}/>
				</Route>
			</Router>
		</Provider>
	), document.getElementById("app"));
	
	header.setHeaderHighlight(2);
});