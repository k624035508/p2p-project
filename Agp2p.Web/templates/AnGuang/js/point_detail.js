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
                    <i className={overStock ? "disabled" : ""}  onClick= { ev => this.plusBuy() }>+</i>
                    
                </p>
                
            </div>
        )
    }
}

$(function () {
        header.setHeaderHighlight(3);
        ReactDom.render(<XiaomiType />, document.getElementById("xiaomiType"));
        var index = $("ul.type-jiaxijuan li").index($(".selectJiaxi"));
        $(".jiaxipoint").eq(index).show().siblings().hide();  
        $(".jiaxiquantity").eq(index).show().siblings().hide();
        $("ul.type-jiaxijuan li").click(function() {
        $(this).addClass("selectJiaxi").siblings().removeClass("selectJiaxi");
        index = $("ul.type-jiaxijuan li").index($(this));
        $(".jiaxipoint").eq(index).show().siblings().hide();
        $(".jiaxiquantity").eq(index).show().siblings().hide();
        $("#jiaxiquanId").val($(this).attr("id"));
    });
});