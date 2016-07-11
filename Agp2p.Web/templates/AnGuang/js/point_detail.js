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
        this.state = { selectTypeId: "" };
    }
    render() {
        return (
            <div>
            <span className="goods-wenzi">颜色分类：</span>
                <ul className="list-unstyled list-inline type-select">
                    {keys(classMappingXiaomi).map(k => {
                        return (
                            <li id={classMappingXiaomi[k]} key={classMappingXiaomi[k]} onClick={ev => this.setState({ selectTypeId: classMappingXiaomi[k] }) }>
                                { this.state.selectTypeId == classMappingXiaomi[k]
                                    ? <img src={TEMPLATE_PATH + "/imgs/point/selected.png"} />
                                    : null}
                            </li>);
                    }) }
                </ul>
            </div>
        )
    }
}

$(function () {
    header.setHeaderHighlight(3);
    ReactDom.render(<XiaomiType />, document.getElementById("xiaomiType"));
});