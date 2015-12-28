import React from "react"
import ReactDom from "react-dom"
import groupBy from "lodash/collection/groupBy"
import sortBy from "lodash/collection/sortBy"
import lmap from "lodash/collection/map"
import range from "lodash/utility/range"
import assign from "lodash/object/assign"
import indexOf from "lodash/array/indexOf"
import last from "lodash/array/last"

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

class ProjectCostingPredictTable extends React.Component {
	constructor(props) {
        super(props);
        this.state = {projectPublishCostingPredict: [], repeatDay: 30};
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
    genEditableTd(objRef, propertyName, childrenProjector = x => x) {
    	return objRef.editing == propertyName ? <td><input
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
    	 /></td> : <td
    	 	onClick={ev => {
	    		objRef.editing = propertyName;
	    		this.forceUpdate();
	    	}}>{ childrenProjector(objRef[propertyName]) }</td>
    }
    getPrepayAmount(p) {
        return p.financingAmount * p.prepayRatePercent / 100;
    }
    getCostingOfPredict(p) {
    	return p.financingAmount * p.prepayRatePercent / 100 * p.profitRateYearlyPercent / 100 / 360 * p.termLength;
    }
    getDelayCostingPredict(p) {
    	return p.financingAmount * p.profitRateYearlyPercent / 100 / 360 * p.repayDelayDays;
    }
    getHandlingFee(p) {
        return this.getPrepayAmount(p) * p.handlingFeePercent / 100;
    }
    render() {
    	let group = groupBy(this.state.projectPublishCostingPredict, p => p.date),
    		sortedGroup = sortBy(group, (g, key) => key);
    	let sumOfFinancingAmount = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + parseFloat(predict.financingAmount), 0),
    		sumOfCosting = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + this.getCostingOfPredict(predict), 0),
    		sumOfDelayCosting = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + this.getDelayCostingPredict(predict), 0),
            sumOfPrepayAmount = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + this.getPrepayAmount(predict), 0),
            sumOfHandlingFee = this.state.projectPublishCostingPredict.reduce((sum, predict) => sum + this.getHandlingFee(predict), 0);
        return (
            <table width="100%" border="0" cellSpacing="0" cellPadding="0" className="ltable">
            	<thead>
			    <tr>
				    <th width="10%">发标日期</th>
				    <th width="8%">项目金额</th>
				    <th width="8%">垫付率（%）</th>
                    <th width="8%">垫付资金</th>
				    <th width="8%">年化利率（%）</th>
				    <th width="8%">期限（天）</th>
				    <th width="8%">资金成本</th>
				    <th width="8%">错配期（天）</th>
				    <th width="8%">错配期成本</th>
                    <th width="8%">手续费率（%）</th>
                    <th width="8%">结算成本</th>
                    <th width="8%">总成本</th>
			    </tr>
			    </thead>
			    <tbody>
			  	{lmap(sortedGroup, (predicts, trIndex) => {
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
                                    <a href="javascript:" onClick={deleteCurrentPredict} style={{marginLeft: '40px'}}>删除</a></div>)}
			  				{this.genEditableTd(p, "prepayRatePercent")}
                            <td>{this.getPrepayAmount(p).format()}</td>
			  				{this.genEditableTd(p, "profitRateYearlyPercent")}
			  				{this.genEditableTd(p, "termLength")}
			  				<td>{this.getCostingOfPredict(p).format()}</td>
			  				{this.genEditableTd(p, "repayDelayDays")}
			  				<td>{this.getDelayCostingPredict(p).format()}</td>
                            {this.genEditableTd(p, "handlingFeePercent")}
                            <td>{this.getHandlingFee(p).format()}</td>
                            <td>{(this.getCostingOfPredict(p) + this.getDelayCostingPredict(p) + this.getHandlingFee(p)).format()}</td>
			  			</tr>;
			  		})
			  	})}
			  	<tr className="sum">
			  		<td>{sortedGroup.length + " 天"}</td>
			  		<td>{sumOfFinancingAmount.format()}</td>
                    <td></td>
			  		<td>{sumOfPrepayAmount.format()}</td>
                    <td colSpan="2"></td>
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

$(() => {
	ReactDom.render(<ProjectCostingPredictTable />, $("#mounting-point")[0]);
});