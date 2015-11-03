import React from "react";
import keys from "lodash/object/keys"
import values from "lodash/object/values"
import "../less/settings.less"

class Settings extends React.Component {
	constructor(props) {
		super(props);
		this.state = { valueTable: {}, enabledNotificationTypes: new Set([]), column: []};
		// valueTable format: {"充值成功" : {"站内消息": 10, "短信": 20}, ...}
	}
	componentDidMount() {
		this.fetchNotificationSettings();
	}
	fetchNotificationSettings() {
		let url = USER_CENTER_ASPX_PATH + "/AjaxQueryNotificationSettings";
		$.ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: "",
			success: function(result) {
				let {valueTable, enabledNotificationTypes} = JSON.parse(result.d);
				if (valueTable) {
					let valuesOfFirstRow = values(valueTable)[0];
					this.setState({ valueTable, enabledNotificationTypes: new Set(enabledNotificationTypes), column: keys(valuesOfFirstRow) });
				}
			}.bind(this),
			error: function(xhr, status, err) {
				console.error(url, status, err.toString());
			}.bind(this)
		});
	}
	saveNotificationSettings() {
		let url = USER_CENTER_ASPX_PATH + "/AjaxSaveNotificationSettings";
		$.ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: JSON.stringify({enabledNotificationTypes: Array.from(this.state.enabledNotificationTypes).join(",")}),
			success: function(result) {
				alert(result.d);
			}.bind(this),
			error: function(xhr, status, err) {
				console.error(url, status, err.toString());
			}.bind(this)
		});
	}
    render() {
    	let state = this.state;
        return(
            <div className="settings-wrap">
                <p>为了您的资金安全，建议您不要取消重要的消息项。如有问题，请联系客服：400-8989-089</p>
                <div className="news-tb">
                    <table className="table table-bordered">
                        <thead>
                            <tr>
                                <th>通知类型</th>
                                {state.column.map(c => <th key={c}>{c}</th>)}
                            </tr>
                        </thead>
                        <tbody>
                            {keys(state.valueTable).map(rowName => 
                            <tr key={rowName}>
                                <td>{rowName}</td>
                                {state.column.map(columnName =>
                                    state.valueTable[rowName][columnName] == undefined
                                        ? <td key={columnName}></td>
                                        : <td key={columnName}><input type="checkbox"
                                        	checked={state.enabledNotificationTypes.has(state.valueTable[rowName][columnName])}
                                        	onChange={ev => {
                                        		if (ev.target.checked) {
                                        			state.enabledNotificationTypes.add(state.valueTable[rowName][columnName]);
                                        		} else {
                                        			state.enabledNotificationTypes.delete(state.valueTable[rowName][columnName]);
                                        		}
                                        		this.setState({enabledNotificationTypes: state.enabledNotificationTypes});
                                        	}}/></td>)}
                            </tr>)}
                        </tbody>
                    </table>
                </div>
                <div className="btn-wrap"><a href="javascript:" onClick={ev => this.saveNotificationSettings()}>保 存</a></div>
            </div>
        );
    }
}
export default Settings;