import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/point_detail.less";
import "../less/footerSmall.less";
import header from "./header.js";
window['jQuery'] = $;
window['$'] = $;

import React from "react";
import ReactDom from "react-dom";
import {classMappingXiaomi} from "../js/pointGoods-list.jsx";
import keys from "lodash/object/keys";
import alert from "../components/tips_alert.js";

class XiaomiType extends React.Component {
    constructor(props) {
        super(props);
        this.state = { selectTypeId: "", buyCount: 1, stock: 0, title: "" };
    }
    componentDidMount() {
        var {stock, title} = $("#xiaomiType").data();
        this.setState({ stock: stock, title: title });
    }
    reduceBuy() {
        this.setState({ buyCount: this.state.buyCount - 1 });
        if (this.state.buyCount <= 1) {
            this.setState({ buyCount: 1 });
        }
    }
    plusBuy() {
        this.setState({ buyCount: this.state.buyCount + 1 });
        if (this.state.buyCount >= this.state.stock) {
            this.setState({ buyCount: this.state.stock });
        }
    }
    render() {
        let isOne = this.state.buyCount <= 1, overStock = this.state.buyCount >= this.state.stock;
        return (
            <div>
                <p className="goods-counts"><span>兑换数量：</span><i className={isOne ? "disabled" : ""}  onClick={ev => this.reduceBuy() }>-</i>
                    <input name="count" type="text"  value={this.state.buyCount} />
                    <i className={overStock ? "disabled" : ""}  onClick= { ev => this.plusBuy() }>+</i><span>库存{this.state.stock}件</span>
                </p>
                <div>
                    {this.state.title == "小米迷你充电宝" ? <span className="goods-wenzi">属性：</span> : null}
                    {this.state.title == "小米迷你充电宝" ?
                        <ul className="list-unstyled list-inline type-select">
                            {keys(classMappingXiaomi).map(k => {
                                return (
                                    <li id={classMappingXiaomi[k]} key={classMappingXiaomi[k]} onClick={ev => this.setState({ selectTypeId: classMappingXiaomi[k] }) }>
                                        { this.state.selectTypeId == classMappingXiaomi[k]
                                            ? <img src={TEMPLATE_PATH + "/imgs/point/selected.png"} />
                                            : null}
                                    </li>);
                            }) }
                        </ul> : null }
                    <input className="hiddenCount" type="hidden" name="type" value={this.state.selectTypeId} />
                </div>
            </div>
        )
    }
}

$(function () {
    header.setHeaderHighlight(3);
    ReactDom.render(<XiaomiType />, document.getElementById("xiaomiType"));
    $(".duihuanLogin").click(function () {
        alert("请先登录", () => {
            location.href = "login.html";
        });
    });
});