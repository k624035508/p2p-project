import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/add_order.less";
import "../less/footerSmall.less";
import header from "./header.js";
window['jQuery'] = $;
window['$'] = $;

import React from "react";
import ReactDom from "react-dom";

class Orders extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        return (
            <div className="order-wrap">
                <div className="goods-address">
                    <p>选择收货地址</p>
                    <ul className="list-unstyled list-inline">
                    <li className="addName">
                    </li>
                    </ul>
                </div>
                <div className="order-content">
                    <p className="bill">商品清单</p>
                    <table className="table order-tb">
                    <thead>
                        <tr>
                            <th></th>
                            <th>商品名称</th>
                            <th>价格</th>
                            <th>属性</th>
                            <th>数量</th>
                            <th>小计</th>
                        </tr>
                    </thead>
                    </table>
                    <p className="quantity">商品数量：1件</p>
                    <p className="points">应付积分：<i>1000积分</i></p>
                    <a>提交订单</a>
                    <p className="premise">* 须完成投资的用户才能兑换</p>
                </div>
            </div>
            )
    }
}

$(function () {
    header.setHeaderHighlight(3);
    ReactDom.render(<Orders />,document.getElementById("orderConfirm"));
});