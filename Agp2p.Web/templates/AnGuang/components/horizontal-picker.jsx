import React from "react";

class HorizontalPicker extends React.Component {
    constructor(props) {
        super(props);
        this.state = {options: [] };
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
                this.setState({options: options});
            }.bind(this),
            error: function (xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
    }
    changeType(type) {
        if (type == this.props.value) return;
        this.props.onTypeChange(type);
    }
    render() {
        return (
            <ul className="list-unstyled list-inline" id="typePicker" >
                {this.state.options.map(op => {
                    return <li key={op.value} className={op.value == this.props.value ? "active" : ""}
                               onClick={() => this.changeType(op.value)}>{op.key}</li>
                })}
            </ul>
        );
    }
}
export default HorizontalPicker;