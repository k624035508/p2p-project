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

class XiaomiType extends React.Component {
    constructor(props) {
        super(props);
        this.state = { selectTypeId: "", buyCount: 1 ,stock: 0, title:""};
    }
    componentDidMount(){
        var {stock, title} = $("#xiaomiType").data();
        this.setState({stock:stock,title:title});   
    }
    render() {
        return (
            <div>
            <p className="goods-counts">兑换数量：<i className="jian" onClick={ev => this.setState({buyCount: this.state.buyCount-1})}>-</i>
                <input name="count" type="text" value={this.state.buyCount} onChange={ev => this.setState({buyCount: ev.target.value})}/>
                <i className="jia" onClick= { ev => this.setState({ buyCount: this.state.buyCount+1 }) } >+</i><span>库存{this.state.stock}件</span>
                </p>
            <div>
                {this.state.title == "小米迷你充电宝" ?  <span className="goods-wenzi">属性：</span> : null}
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
            <input type="hidden" name="type" value={this.state.selectTypeId} />
            </div>
            </div>
        )
    }
}

$(function () {
    header.setHeaderHighlight(3);
    ReactDom.render(<XiaomiType />, document.getElementById("xiaomiType"));
});