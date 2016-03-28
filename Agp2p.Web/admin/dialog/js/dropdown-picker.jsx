import React from "react";

class DropdownPicker extends React.Component {
    constructor(props) {
        super(props);
        this.state = {options: []};
    }
    componentDidMount() {
        let url = "/aspx/main/usercenter.aspx/AjaxQueryEnumInfo";
        $.ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({enumFullName: this.props.enumFullName}),
            success: function (data) {
                this.setState({options: JSON.parse(data.d)});
            }.bind(this),
            error: function (xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
    }
    render() {
        return (
            <select className="form-control" onChange={(ev) => this.props.onTypeChange(ev.target.value) }>
                {this.state.options.map(op => {
                    return <option key={op.value} value={op.value}>{op.key}</option>
                })}
            </select>
        );
    }
}
export default DropdownPicker;