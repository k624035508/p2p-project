import "bootstrap-webpack";
import "../less/head.less";
import "../less/usercenter.less";
import "../less/footerSmall.less";

import React from "react"
import Router from 'react-router'; // or var Router = ReactRouter; in browsers

var DefaultRoute = Router.DefaultRoute;
var Link = Router.Link;
var Route = Router.Route;
var RouteHandler = Router.RouteHandler;

import UserCenterPage from "../containers/UserCenterPage.jsx"
import MyTransaction from "../containers/mytransaction.jsx"
import RechargePage from "../containers/recharge.jsx"
import WithdrawPage from "../containers/withdraw.jsx"

$(function(){
    //点击导航加载相应内容
    var $mountNode = $("#app");

    var routes = (
        <Route path="/" handler={UserCenterPage}>
            <Route path="mytrade" handler={MyTransaction} />
            <Route path="recharge" handler={RechargePage} />
            <Route path="withdraw" handler={WithdrawPage} />
        </Route>
    );

    Router.run(routes, function (Handler) {
        React.render(<Handler/>, $mountNode[0]);
    });
});