import React from "react";
import $ from "jquery";

export default class HorizontalPicker extends React.Component {
    constructor(props) {
        super(props);
        this.state = {options: [], type: -1};
    }
    componentDidMount() {
        let url = USER_CENTER_ASPX_PATH + "/AjaxQueryEnumInfo";
        $.ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({enumFullName: this.props.enumFullName}),
            success: function (data) {
                var options = JSON.parse(data.d);
                this.setState({options: options, type: options[0].value});
            }.bind(this),
            error: function (xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
    }
    changeType(type) {
        this.setState({type: type});
        this.props.onTypeChange(type);
    }
    render() {
        return (
            <ul className="list-unstyled list-inline" id="typePicker" >
                {this.state.options.map(op => {
                    return <li key={op.value} className={op.value == this.state.type ? "active" : ""}
                               onClick={() => this.changeType(op.value)}>{op.key}</li>
                })}
            </ul>
        );
    }
}