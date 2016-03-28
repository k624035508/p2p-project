import React from "react";

class DropdownPicker extends React.Component {
    constructor(props) {
        super(props);
        this.state = {options: []};
    }
    fetchSelectData(enumFullName) {
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryEnumInfo";
        $.ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({enumFullName}),
            success: function (data) {
                this.setState({options: JSON.parse(data.d)});
            }.bind(this),
            error: function (xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
    }
    componentDidMount() {
        this.fetchSelectData(this.props.enumFullName);
    }
    componentWillReceiveProps(nextProps, nextState) {
        if (this.props.enumFullName !== nextProps.enumFullName) {
            this.fetchSelectData(nextProps.enumFullName);
        }
    }
    render() {
        return (
            <select name="tradeType" id="typeSel"
                onChange={(ev) => this.props.onTypeChange(ev.target.value) }
                value={this.props.value} >
                {this.state.options.map(op => {
                    return <option key={op.value} value={op.value}>{op.key}</option>
                })}
            </select>
        );
    }
}
export default DropdownPicker;