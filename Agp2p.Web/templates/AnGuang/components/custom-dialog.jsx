import React from "react";

export default class CustomDialog extends React.Component {
	constructor(props) {
		super(props);
		this.state = {  };
	}
	alignToCenter() {
		var offsetHeight = ($(window).height() - $(".modal-content", this.refs.customDialog).height()) / 2;
		$(".modal-dialog", this.refs.customDialog).css("margin-top", offsetHeight + "px");
	}
	show() {
		$(this.refs.customDialog).modal();
		this.alignToCenter();
	}
	hide() {
		$(this.refs.customDialog).modal('hide');
	}
	render() {
		return (<div className="modal fade custom-dlg" id="customDialog" ref="customDialog" data-backdrop="static" tabIndex="-1"
					 role="dialog" aria-labelledby="customDialogLabel">
                    <div className="modal-dialog" role="document">
                        <div className="modal-content">
                            <div className="modal-header">
                                <button type="button" className="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                                <h4 className="modal-title" id="customDialogLabel">{this.props.title}</h4>
                            </div>
                            <div className="modal-body">{this.props.children}</div>
                            <div className="modal-footer">
                                <button type="button" className="cancel-btn" data-dismiss="modal">取 消</button>
                                <button type="button" className="confirm-btn" data-dismiss="modal" onClick={ev => this.props.onSubmit()}>确 定</button>
                            </div>
                        </div>
                    </div>
                </div>);
	}
}