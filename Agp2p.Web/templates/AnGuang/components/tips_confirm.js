import React from "react"
import ReactDom from "react-dom"
import "../less/tips-alert.less"

class TipsConfirm extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(){
        return(
            <div className="modal fade" id="tipsConfirm" data-backdrop="static" tabIndex="-1" role="dialog" aria-labelledby="tipsConfirmLabel">
                <div className="modal-dialog" role="document">
                    <div className="modal-content">
                        <div className="modal-header">
                            <button type="button" className="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                            <h4 className="modal-title" id="tipsConfirmLabel">温馨提示</h4>
                        </div>
                        <div className="modal-body"></div>
                        <div className="modal-footer">
                            <button type="button" className="cancel-btn" data-dismiss="modal">取 消</button>
                            <button type="button" data-dismiss="modal">确 定</button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

export default (msg, callback = null) => {
    let mountPoint = document.getElementById("confirmModal");
    if (!mountPoint) {
        $("body").append("<div id='confirmModal'></div>");
        ReactDom.render(<TipsConfirm />, document.getElementById("confirmModal"));
    }
    $("#tipsConfirm .modal-body").text(msg);
    $("#tipsConfirm").modal();
    if (callback) {
        $("#tipsConfirm button").off().on('click', callback);
    }
}
