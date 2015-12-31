import React from "react"
import ReactDom from "react-dom"
import groupBy from "lodash/collection/groupBy"
import sortBy from "lodash/collection/sortBy"
import range from "lodash/utility/range"
import assign from "lodash/object/assign"
import indexOf from "lodash/array/indexOf"
import last from "lodash/array/last"

let _projectPublishCostingPredict = [];

/**
 * Number.prototype.format(n, x)
 * 
 * @param integer n: length of decimal
 * @param integer x: length of sections
 */
Number.prototype.format = function(n = 2, x = 3) {
    var re = '\\d(?=(\\d{' + (x || 3) + '})+' + (n > 0 ? '\\.' : '$') + ')';
    return "¥" + this.toFixed(Math.max(0, ~~n)).replace(new RegExp(re, 'g'), '$&,');
};

String.prototype.format = function() {
    return parseFloat(this).format();
}

let getPrepayAmount = p => {
    return p.financingAmount * p.prepayRatePercent / 100;
}
let getCostingOfPredict = p => {
    return p.financingAmount * p.prepayRatePercent / 100 * p.profitRateYearlyPercent / 100 / 360 * p.termLength;
}
let getDelayCostingPredict = p => {
    return p.financingAmount * p.profitRateYearlyPercent / 100 / 360 * p.repayDelayDays;
}
let getHandlingFee = p => {
    return p.financingAmount * p.handlingFeePercent / 100;
}

class ProjectCostingPredictTable extends React.Component {
	constructor(props) {
        super(props);
        this.state = {projectPublishCostingPredict: _projectPublishCostingPredict, repeatDay: 30};
        this.defaultValue = {
            financingAmount: 100000,
            prepayRatePercent: 30,
            profitRateYearlyPercent: 6,
            termLength: 7,
            repayDelayDays: 2,
            handlingFeePercent: 0.25
        };
    }
    componentDidMount() {
    	window.appendPredict = () => {
    		let today = new Date().toJSON().slice(0,10);
    		this.state.projectPublishCostingPredict.push(assign({date: today}, this.defaultValue));
    		this.forceUpdate();
    	};
    	window.appendNextDayPredict = () => {
            let group = groupBy(this.state.projectPublishCostingPredict, p => p.date),
                sortedGroup = sortBy(group, (g, key) => key),
                lastDay = sortedGroup.length == 0 ? new Date().toJSON().slice(0,10) : last(sortedGroup)[0].date;

    		let tomorrow = new Date(new Date(lastDay).getTime() + 24 * 60 * 60 * 1000).toJSON().slice(0,10);
            this.state.projectPublishCostingPredict.push(assign({date: tomorrow}, this.defaultValue));
            this.forceUpdate();
    	};
    	window.repeatPredict = () => {
    		let addDays = parseInt(prompt('请输入需要重复的次数：') || "0");
    		if (!isNaN(addDays) && 0 < addDays) {
    			let group = groupBy(this.state.projectPublishCostingPredict, p => p.date),
    			sortedGroup = sortBy(group, (g, key) => key),
    			preRepeats = sortedGroup[0];

    			let todayTime = new Date().getTime();
    			let delta = this.state.projectPublishCostingPredict;
    			range(1, addDays + 1).forEach(i => {
    				let thatDay = {date: new Date(todayTime + i * 24 * 60 * 60 * 1000).toJSON().slice(0,10)};
    				preRepeats.forEach(preClone => {
    					let repeated = assign({}, preClone, thatDay);
    					delta.push(repeated);
    				})
    			})
    			this.forceUpdate();
    		}
    	};
        window.openEditingTable = () => {
            $("#title").html("业务预测—资金成本");
            $("#editingTable-mountingPoint").show();
            $("#groupByTermLengthTable-mountingPoint,#groupByPrepayRateTable-mountingPoint").hide();
            this.forceUpdate();
        };
        var originalExport = window.exportExcel;
        window.exportExcel = (...args) => {
            this.forceUpdate();
            originalExport(...args);
        };
        $(".defaultValueSetter").each((index, el) => {
            el.value = this.defaultValue[el.id];
        });
        $(".defaultValueSetter").blur(ev => {
            this.defaultValue[ev.target.id] = ev.target.value;
        });
    }
    clonePredict(p) {
        this.state.projectPublishCostingPredict.push(assign({}, p, this.defaultValue));
        this.forceUpdate();
    }
    genEditableTd(objRef, propertyName, childrenProjector = x => x, extraProps = {}) {
    	return objRef.editing == propertyName ? <td {...extraProps}><input
    		style={{width: '100%'}}
    		autoFocus={true}
    		value={objRef[propertyName]}
    		onChange={ev => {
	    		objRef[propertyName] = ev.target.value;
    			this.forceUpdate();
    		}}
    		onBlur={ev => {
	    		delete objRef.editing;
    			this.forceUpdate();
    		}}
    	 /></td> : <td {...extraProps}
    	 	onClick={ev => {
	    		objRef.editing = propertyName;
	    		this.forceUpdate();
	    	}}>{ childrenProjector(objRef[propertyName]) }</td>
    }
    render() {
    	let group = groupBy(this.state.projectPublishCostingPredict, p => p.date), // {7: [], ...}
    		sortedGroup = sortBy(group, (g, key) => key); // [ [], [], ...]
    	let sumOfFinancingAmount = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + parseFloat(predict.financingAmount), 0),
    		sumOfCosting = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + getCostingOfPredict(predict), 0),
    		sumOfDelayCosting = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + getDelayCostingPredict(predict), 0),
            sumOfPrepayAmount = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + getPrepayAmount(predict), 0),
            sumOfHandlingFee = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + getHandlingFee(predict), 0);
        return (
            <table width="100%" border="0" cellSpacing="0" cellPadding="0" className="ltable" id="editingTable">
            	<thead>
			    <tr>
				    <th width="14%">发标日期</th>
				    <th width="14%">项目金额</th>
				    <th width="4%">垫付率（%）</th>
                    <th width="8%">垫付资金</th>
				    <th width="4%">资金年化利率（%）</th>
				    <th width="4%">期限（天）</th>
				    <th width="8%">资金成本</th>
				    <th width="4%">错配期（天）</th>
				    <th width="8%">错配期成本</th>
                    <th width="4%">结算手续费率（%）</th>
                    <th width="8%">结算成本</th>
                    <th width="8%">总成本</th>
			    </tr>
			    </thead>
			    <tbody>
			  	{sortedGroup.map((predicts, trIndex) => {
			  		return predicts.map((p, index) => {
                        let deleteCurrentPredict = ev => {
                            let pos = indexOf(this.state.projectPublishCostingPredict, p);
                            this.state.projectPublishCostingPredict.splice(pos, 1);
                            this.forceUpdate();
                        };
                        return <tr>
                            {index == 0 ? <td>{p.date}</td> : <td></td>}
			  				{this.genEditableTd(p, "financingAmount",
                                children => <div>{children.format()}
                                    <a href="javascript:" onClick={ev => {ev.stopPropagation(); this.clonePredict(p);}} style={{marginLeft: '10px'}}>添加</a>
                                    <a href="javascript:" onClick={deleteCurrentPredict} style={{marginLeft: '40px'}}>删除</a></div>,
                                    {'data-value': p.financingAmount.format()})}
			  				{this.genEditableTd(p, "prepayRatePercent")}
                            <td>{getPrepayAmount(p).format()}</td>
			  				{this.genEditableTd(p, "profitRateYearlyPercent")}
			  				{this.genEditableTd(p, "termLength")}
			  				<td>{getCostingOfPredict(p).format()}</td>
			  				{this.genEditableTd(p, "repayDelayDays")}
			  				<td>{getDelayCostingPredict(p).format()}</td>
                            {this.genEditableTd(p, "handlingFeePercent")}
                            <td>{getHandlingFee(p).format()}</td>
                            <td>{(getCostingOfPredict(p) + getDelayCostingPredict(p) + getHandlingFee(p)).format()}</td>
			  			</tr>;
			  		})
			  	})}
			  	<tr className="sum">
			  		<td>{sortedGroup.length + " 天"}</td>
			  		<td>{sumOfFinancingAmount.format()}</td>
                    <td></td>
			  		<td>{sumOfPrepayAmount.format()}</td>
                    <td></td>
                    <td></td>
			  		<td>{sumOfCosting.format()}</td>
			  		<td></td>
			  		<td>{sumOfDelayCosting.format()}</td>
                    <td></td>
                    <td>{sumOfHandlingFee.format()}</td>
                    <td>{(sumOfCosting + sumOfDelayCosting + sumOfHandlingFee).format()}</td>
			  	</tr>
			  	</tbody>
			</table>
        );
    }
}




class GroupByTermLengthTable extends React.Component {
    constructor(props) {
        super(props);
        this.state = {projectPublishCostingPredict: _projectPublishCostingPredict};
    } 
    componentDidMount() {
        window.openGroupByTermLengthTable = () => {
            $("#title").html("汇总表（按品种）");
            $("#editingTable-mountingPoint").hide();
            $("#groupByTermLengthTable-mountingPoint").show();
            $("#groupByPrepayRateTable-mountingPoint").hide();
            this.forceUpdate();
        };
        var originalExport = window.exportExcel;
        window.exportExcel = (...args) => {
            this.forceUpdate();
            originalExport(...args);
        };
    }
    render() {
        let group = groupBy(this.state.projectPublishCostingPredict, p => p.termLength), // {7: [], ...}
            sortedGroup = sortBy(group, (gs, key) => parseFloat(key)); // [ [], [], ...]
        return (
            <table width="100%" border="0" cellSpacing="0" cellPadding="0" className="ltable" id="groupByTermLengthTable">
                <thead>
                <tr>
                    <th width="10%">品种</th>
                    <th width="10%">项目金额</th>
                    <th width="10%">垫付金额</th>
                    <th width="10%">垫付资金成本</th>
                    <th width="10%">错配期资金成本</th>
                    <th width="10%">结算成本</th>
                    <th width="10%">成本合计</th>
                </tr>
                </thead>
                <tbody>
                {sortedGroup.map((ps, index) => {
                    return <tr key={index}>
                        <td>{ps[0].termLength + " 天"}</td>
                        <td>{ps.reduce((sum, p) => sum + parseFloat(p.financingAmount), 0).format()}</td>
                        <td>{ps.reduce((sum, p) => sum + getPrepayAmount(p), 0).format()}</td>
                        <td>{ps.reduce((sum, p) => sum + getCostingOfPredict(p), 0).format()}</td>
                        <td>{ps.reduce((sum, p) => sum + getDelayCostingPredict(p), 0).format()}</td>
                        <td>{ps.reduce((sum, p) => sum + getHandlingFee(p), 0).format()}</td>
                        <td>{ps.reduce((sum, p) => sum + getHandlingFee(p) + getDelayCostingPredict(p) + getCostingOfPredict(p), 0).format()}</td>
                    </tr>
                })}
                <tr key="sum" className="sum">
                    <td>合计</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + parseFloat(p.financingAmount), 0).format()}</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + getPrepayAmount(p), 0).format()}</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + getCostingOfPredict(p), 0).format()}</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + getDelayCostingPredict(p), 0).format()}</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + getHandlingFee(p), 0).format()}</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + getHandlingFee(p) + getDelayCostingPredict(p) + getCostingOfPredict(p), 0).format()}</td>
                </tr>
                </tbody>
            </table>
        );
    }
}




class GroupByPrepayRateTable extends React.Component {
    constructor(props) {
        super(props);
        this.state = {projectPublishCostingPredict: _projectPublishCostingPredict};
    } 
    componentDidMount() {
        window.openGroupByPrepayRateTable = () => {
            $("#title").html("汇总表（按垫付率）");
            $("#editingTable-mountingPoint").hide();
            $("#groupByTermLengthTable-mountingPoint").hide();
            $("#groupByPrepayRateTable-mountingPoint").show();
            this.forceUpdate();
        };
        var originalExport = window.exportExcel;
        window.exportExcel = (...args) => {
            this.forceUpdate();
            originalExport(...args);
        };
    }
    render() {
        let group = groupBy(this.state.projectPublishCostingPredict, p => p.prepayRatePercent), // {7: [], ...}
            sortedGroup = sortBy(group, (gs, key) => parseFloat(key)); // [ [], [], ...]
        return (
            <table width="100%" border="0" cellSpacing="0" cellPadding="0" className="ltable" id="groupByPrepayRateTable">
                <thead>
                <tr>
                    <th width="10%">垫付率</th>
                    <th width="10%">项目金额</th>
                    <th width="10%">垫付金额</th>
                    <th width="10%">垫付资金成本</th>
                    <th width="10%">错配期资金成本</th>
                    <th width="10%">结算成本</th>
                    <th width="10%">成本合计</th>
                </tr>
                </thead>
                <tbody>
                {sortedGroup.map((ps, index) => {
                    return <tr key={index}>
                        <td>{ps[0].prepayRatePercent + "%"}</td>
                        <td>{ps.reduce((sum, p) => sum + parseFloat(p.financingAmount), 0).format()}</td>
                        <td>{ps.reduce((sum, p) => sum + getPrepayAmount(p), 0).format()}</td>
                        <td>{ps.reduce((sum, p) => sum + getCostingOfPredict(p), 0).format()}</td>
                        <td>{ps.reduce((sum, p) => sum + getDelayCostingPredict(p), 0).format()}</td>
                        <td>{ps.reduce((sum, p) => sum + getHandlingFee(p), 0).format()}</td>
                        <td>{ps.reduce((sum, p) => sum + getHandlingFee(p) + getDelayCostingPredict(p) + getCostingOfPredict(p), 0).format()}</td>
                    </tr>
                })}
                <tr key="sum" className="sum">
                    <td>合计</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + parseFloat(p.financingAmount), 0).format()}</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + getPrepayAmount(p), 0).format()}</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + getCostingOfPredict(p), 0).format()}</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + getDelayCostingPredict(p), 0).format()}</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + getHandlingFee(p), 0).format()}</td>
                    <td>{this.state.projectPublishCostingPredict.reduce((sum, p) => sum + getHandlingFee(p) + getDelayCostingPredict(p) + getCostingOfPredict(p), 0).format()}</td>
                </tr>
                </tbody>
            </table>
        );
    }
}

$(() => {
    ReactDom.render(<ProjectCostingPredictTable />, $("#editingTable-mountingPoint")[0]);
    ReactDom.render(<GroupByTermLengthTable />, $("#groupByTermLengthTable-mountingPoint")[0]);
    ReactDom.render(<GroupByPrepayRateTable />, $("#groupByPrepayRateTable-mountingPoint")[0]);
});

// http://stackoverflow.com/questions/29698796/how-to-convert-html-table-to-excel-with-multiple-sheet
window.exportExcel = (function() {
    var uri = 'data:application/vnd.ms-excel;base64,'
    , tmplWorkbookXML = '<?xml version="1.0"?><?mso-application progid="Excel.Sheet"?><Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">'
      + '<DocumentProperties xmlns="urn:schemas-microsoft-com:office:office"><Author>Axel Richter</Author><Created>{created}</Created></DocumentProperties>'
      + '<Styles>'
      + '<Style ss:ID="Currency"><NumberFormat ss:Format="Currency"></NumberFormat></Style>'
      + '<Style ss:ID="Date"><NumberFormat ss:Format="Medium Date"></NumberFormat></Style>'
      + '</Styles>' 
      + '{worksheets}</Workbook>'
    , tmplWorksheetXML = '<Worksheet ss:Name="{nameWS}"><Table>{rows}</Table></Worksheet>'
    , tmplCellXML = '<Cell{attributeStyleID}{attributeFormula}><Data ss:Type="{nameType}">{data}</Data></Cell>'
    , base64 = function(s) { return window.btoa(unescape(encodeURIComponent(s))) }
    , format = function(s, c) { return s.replace(/{(\w+)}/g, function(m, p) { return c[p]; }) }
    return function(tables, wsnames, wbname, appname) {
      var ctx = "";
      var workbookXML = "";
      var worksheetsXML = "";
      var rowsXML = "";

      for (var i = 0; i < tables.length; i++) {
        if (!tables[i].nodeType) tables[i] = document.getElementById(tables[i]);
        for (var j = 0; j < tables[i].rows.length; j++) {
          rowsXML += '<Row>'
          for (var k = 0; k < tables[i].rows[j].cells.length; k++) {
            var dataType = tables[i].rows[j].cells[k].getAttribute("data-type");
            var dataStyle = tables[i].rows[j].cells[k].getAttribute("data-style");
            var dataValue = tables[i].rows[j].cells[k].getAttribute("data-value");
            dataValue = (dataValue)?dataValue:tables[i].rows[j].cells[k].innerHTML;
            var dataFormula = tables[i].rows[j].cells[k].getAttribute("data-formula");
            dataFormula = (dataFormula)?dataFormula:(appname=='Calc' && dataType=='DateTime')?dataValue:null;
            ctx = {  attributeStyleID: (dataStyle=='Currency' || dataStyle=='Date')?' ss:StyleID="'+dataStyle+'"':''
                   , nameType: (dataType=='Number' || dataType=='DateTime' || dataType=='Boolean' || dataType=='Error')?dataType:'String'
                   , data: (dataFormula)?'':dataValue
                   , attributeFormula: (dataFormula)?' ss:Formula="'+dataFormula+'"':''
                  };
            rowsXML += format(tmplCellXML, ctx);
          }
          rowsXML += '</Row>'
        }
        ctx = {rows: rowsXML, nameWS: wsnames[i] || 'Sheet' + i};
        worksheetsXML += format(tmplWorksheetXML, ctx);
        rowsXML = "";
      }

      ctx = {created: (new Date()).getTime(), worksheets: worksheetsXML};
      workbookXML = format(tmplWorkbookXML, ctx);


      var link = document.createElement("A");
      link.href = uri + base64(workbookXML);
      link.download = wbname || 'Workbook.xls';
      link.target = '_blank';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  })();