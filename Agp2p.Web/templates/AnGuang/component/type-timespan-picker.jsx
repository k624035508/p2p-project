import "bootstrap-webpack";
import $ from "jquery";
import "../less/bootstrap-datetimepicker.css";
import "../js/bootstrap-datetimepicker.js";
import "../js/bootstrap-datetimepicker.zh-CN.js";

import React from "react";

export default React.createClass({
	render: function () {
		return (
		<div className="controls">
		    <select name="tradeType" id="typeSel">
		        <option value="">全部类型</option>
		        <option value="">充值</option>
		        <option value="">提现</option>
		        <option value="">投资</option>
		        <option value="">回款</option>
		        <option value="">其他</option>
		    </select>
		    <div className="dateSel pull-right">
		        <input className="form_date" type="text" /> 到
		        <input className="form_date" type="text" />
		        <a href="#">搜 索</a>
		    </div>
		    <div style={{clear: "both"}}></div>
		</div>);
	},
	componentDidMount: function () {
		//日期设置
        $(this.getDOMNode()).find(".form_date").datetimepicker({
            language: 'zh-CN',
            format: 'yyyy-mm-dd',
            weekStart: 1,
            todayBtn: true,
            todayHighlight: 1,
            startView: 2,
            forceParse: 0,
            showMeridian: 1,
            autoclose: 1,
            minView: 2
        });
    }
});

