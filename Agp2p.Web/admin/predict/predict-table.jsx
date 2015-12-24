import React from "react"
import ReactDom from "react-dom"
import groupBy from "lodash/collection/groupBy"
import sortBy from "lodash/collection/sortBy"
import lmap from "lodash/collection/map"

class ProjectCostingPredictTable extends React.Component {
	constructor(props) {
        super(props);
        this.state = {projectPublishCostingPredict: []};
    }
    appendPredict() {
    	let today = new Date().toJSON().slice(0,10);
    	let delta = this.state.projectPublishCostingPredict;
    	delta.push({date: today, financingAmount: 100000, prepayRate: 0.30, profitRateYearly: 0.06, termLength: 7, repayDelayDays: 2})
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
	    		objRef.editing = null;
    			this.forceUpdate();
    		}}
    	 /></td> : <td {...extraProps}
    	 	onClick={ev => {
	    		objRef.editing = propertyName;
	    		this.forceUpdate();
	    	}}>{objRef[propertyName]}</td>
    }
    render() {
    	let group = groupBy(this.state.projectPublishCostingPredict, p => p.date),
    		sortedGroup = sortBy(group, (g, key) => key);
        return (
            <table width="100%" border="0" cellSpacing="0" cellPadding="0" className="ltable">
            	<thead>
			    <tr>
				    <th width="15%">发标日期</th>
				    <th width="15%">项目金额</th>
				    <th width="10%">垫付率</th>
				    <th width="10%">年化利率</th>
				    <th width="10%">期限（天）</th>
				    <th width="13%">成本</th>
				    <th width="13%">错配期（天）</th>
				    <th width="13%">错配成本</th>
			    </tr>
			    </thead>
			    <tbody>
			  	{lmap(sortedGroup, predicts => {
			  		return predicts.map((p, index) => {
			  			return <tr>
			  				{this.genEditableTd(p, "date", index == 0 ? {} : {style: {fontSize: '0px'}})}
			  				{this.genEditableTd(p, "financingAmount")}
			  				{this.genEditableTd(p, "prepayRate")}
			  				{this.genEditableTd(p, "profitRateYearly")}
			  				{this.genEditableTd(p, "termLength")}
			  				<td>{(p.financingAmount * p.prepayRate * p.profitRateYearly / 360 * p.termLength).toFixed(2)}</td>
			  				{this.genEditableTd(p, "repayDelayDays")}
			  				<td>{(p.financingAmount * p.profitRateYearly / 360 * p.repayDelayDays).toFixed(2)}</td>
			  			</tr>;
			  		})
			  	})}
			  	<tr className="noPrint"><td colSpan="8" onClick={ev => this.appendPredict()}>添加估算</td></tr>
			  	</tbody>
			</table>
        );
    }
}

$(() => {
	ReactDom.render(<ProjectCostingPredictTable />, $("#mounting-point")[0]);
});