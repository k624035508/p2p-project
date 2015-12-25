import React from "react"
import ReactDom from "react-dom"
import groupBy from "lodash/collection/groupBy"
import sortBy from "lodash/collection/sortBy"
import lmap from "lodash/collection/map"
import range from "lodash/utility/range"
import assign from "lodash/object/assign"
import indexOf from "lodash/array/indexOf"

class ProjectCostingPredictTable extends React.Component {
	constructor(props) {
        super(props);
        this.state = {projectPublishCostingPredict: [], repeatDay: 30};
    }
    appendPredict() {
    	let today = new Date().toJSON().slice(0,10);
    	let delta = this.state.projectPublishCostingPredict;
    	delta.push({date: today, financingAmount: 100000, prepayRatePercent: 30, profitRateYearlyPercent: 6, termLength: 7, repayDelayDays: 2})
    	this.forceUpdate();
    }
    appendTomorrowPredict() {
    	let tomorrow = new Date(new Date().getTime() + 24 * 60 * 60 * 1000).toJSON().slice(0,10);
    	let delta = this.state.projectPublishCostingPredict;
    	delta.push({date: tomorrow, financingAmount: 100000, prepayRatePercent: 30, profitRateYearlyPercent: 6, termLength: 7, repayDelayDays: 2})
    	this.forceUpdate();
    }
    repeatPredict(addDays) {
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
    	this.setState({projectPublishCostingPredict: delta});
    }
    genEditableTd(objRef, propertyName, extraProps = {}) {
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
	    	}}>{objRef[propertyName]}</td>
    }
    getCostingOfPredict(p) {
    	return p.financingAmount * p.prepayRatePercent / 100 * p.profitRateYearlyPercent / 100 / 360 * p.termLength;
    }
    getDelayCostingPredict(p) {
    	return p.financingAmount * p.profitRateYearlyPercent / 100 / 360 * p.repayDelayDays;
    }
    render() {
    	let group = groupBy(this.state.projectPublishCostingPredict, p => p.date),
    		sortedGroup = sortBy(group, (g, key) => key);
    	let sumOfFinancingAmount = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + parseFloat(predict.financingAmount), 0),
    		sumOfCosting = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + this.getCostingOfPredict(predict), 0),
    		sumOfDelayCosting = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + this.getDelayCostingPredict(predict), 0);
        return (
            <table width="100%" border="0" cellSpacing="0" cellPadding="0" className="ltable">
            	<thead>
			    <tr>
				    <th width="13%">发标日期</th>
				    <th width="13%">项目金额</th>
				    <th width="10%">垫付率（%）</th>
				    <th width="12%">年化利率（%）</th>
				    <th width="10%">期限（天）</th>
				    <th width="10%">成本</th>
				    <th width="10%">错配期（天）</th>
				    <th width="10%">错配成本</th>
				    <th className="noPrint" width="5%">删除</th>
			    </tr>
			    </thead>
			    <tbody>
			  	{lmap(sortedGroup, predicts => {
			  		return predicts.map((p, index) => {
			  			return <tr>
			  				{this.genEditableTd(p, "date", index == 0 ? {} : {style: {fontSize: '0px'}})}
			  				{this.genEditableTd(p, "financingAmount")}
			  				{this.genEditableTd(p, "prepayRatePercent")}
			  				{this.genEditableTd(p, "profitRateYearlyPercent")}
			  				{this.genEditableTd(p, "termLength")}
			  				<td>{this.getCostingOfPredict(p).toFixed(2)}</td>
			  				{this.genEditableTd(p, "repayDelayDays")}
			  				<td>{this.getDelayCostingPredict(p).toFixed(2)}</td>
			  				<td className="noPrint" style={{cursor: 'pointer',color: 'red'}}
			  					onClick={ev => {
			  						let pos = indexOf(this.state.projectPublishCostingPredict, p);
			  						this.state.projectPublishCostingPredict.splice(pos, 1);
			  						this.forceUpdate();
			  					}}>X</td>
			  			</tr>;
			  		})
			  	})}
			  	<tr className="sum">
			  		<td>{sortedGroup.length + " 天"}</td>
			  		<td>{sumOfFinancingAmount.toFixed(2)}</td>
			  		<td colSpan="3"></td>
			  		<td>{sumOfCosting.toFixed(2)}</td>
			  		<td></td>
			  		<td>{sumOfDelayCosting.toFixed(2)}</td>
			  		<td className="noPrint"></td>
			  	</tr>
		  		{this.state.projectPublishCostingPredict.length == 0 ? 
			  	<tr className="noPrint pointer">
			  		<td colSpan="7" onClick={ev => this.appendPredict()}>添加当日估算</td>
			  		<td colSpan="2" onClick={ev => this.appendTomorrowPredict()}>添加明日估算</td>
			  	</tr> :
			  	<tr className="noPrint pointer">
			  		<td colSpan="4" onClick={ev => this.appendPredict()}>添加当日估算</td>
			  		<td colSpan="2" onClick={ev => this.appendTomorrowPredict()}>添加明日估算</td>
			  		<td colSpan="1">重复<input value={this.state.repeatDay} style={{width: '30px'}}
			  			onChange={ev => this.setState({repeatDay: ev.target.value})} />次</td>
			  		<td colSpan="2" onClick={ev => this.repeatPredict(parseInt(this.state.repeatDay || "0"))}>重复首日估算</td>
			  	</tr>}
			  	</tbody>
			</table>
        );
    }
}

$(() => {
	ReactDom.render(<ProjectCostingPredictTable />, $("#mounting-point")[0]);
});