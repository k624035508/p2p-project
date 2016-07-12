import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/add_order.less";
import "../less/footerSmall.less";
import header from "./header.js";
window['jQuery'] = $;
window['$'] = $;

import React from "react";
import ReactDom from "react-dom";

import CityPicker from "../components/city-picker.jsx";

class Orders extends React.Component {
    constructor(props) {
        super(props);
    }
    componentDidMount() {
        $(".addName").click(function() {
            $("#addressConfirm").modal();
        });
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
                    <div className="modal fade" id="addressConfirm"  role="dialog" aria-labelledby="addressConfirmLabel" data-backdrop="static">
        <div className="modal-dialog addressConfirm-dialog" role="document">
            <div className="modal-content">
                <div className="modal-header">
                    <h4 className="modal-title" id="myModalLabel">新增收货地址</h4>
                    <span className="close" data-dismiss="modal" aria-label="Close">&times;</span>
                </div>
                <div className="modal-body">
                    <ul className="list-unstyled">
                      <li><span><i>*</i>所在地区</span><CityPicker /> </li>
                    <li className="dizhi"><span><i>*</i>详细地址</span><textarea rows="3" placeholder="建议您如实填写详细收货地址，例如街道名称，门牌号码，楼层和房间号码等信息" /></li>
                    <li><span><i>*</i>邮政编码</span><input type="text" /></li>
                    <li><span><i>*</i>收货人姓名</span><input type="text" /></li>
                    <li><span><i>*</i>手机号码</span><input type="text" /></li>
                    </ul>
                       <a>保 存</a> 
                </div>
            </div>
        </div>
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