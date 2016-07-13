import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/add_order.less";
import "../less/footerSmall.less";
import header from "./header.js";
window['jQuery'] = $;
window['$'] = $;

import React from "react";
import ReactDom from "react-dom";
import { Router, Route } from 'react-router'
import thunkMiddleware from 'redux-thunk';
import { createStore, applyMiddleware } from 'redux';
import { Provider } from 'react-redux';

import userCenter from "../reducers/usercenter.js"

import Orders from "../containers/add_order.jsx";

$(function () {
    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();

    const createStoreWithMiddleware = applyMiddleware(thunkMiddleware)(createStore);
    const store = createStoreWithMiddleware(userCenter);

    ReactDom.render((
        <Provider store={store}>
            <Router>
                <Route path="/" component={Orders} />
            </Router>
        </Provider>
        ), document.getElementById("orderConfirm"));
    header.setHeaderHighlight(4);
});