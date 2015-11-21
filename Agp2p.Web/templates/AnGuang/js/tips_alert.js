import React from "react"
import ReactDom from "react-dom"
import $ from "jquery"
import "bootstrap-webpack";
import "../less/tips-alert.less"

class TipsAlert extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(){
        return(
            <div className="modal fade" id="tipsAlert" tabindex="-1" role="dialog" aria-labelledby="tipsAlertLabel">
                <div className="modal-dialog" role="document">
                    <div className="modal-content">
                        <div className="modal-header">
                            <button type="button" className="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                            <h4 className="modal-title" id="tipsAlertLabel">温馨提示</h4>
                        </div>
                        <div className="modal-body"></div>
                        <div className="modal-footer">
                            <button type="button" data-dismiss="modal">确 定</button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

$(function(){
    $(".footer-wrap").prepend("<div id='tipsModal'></div>");
    ReactDom.render(<TipsAlert />, document.getElementById("tipsModal"));
});

window.alert = (msg, callback = null) => {
    $("#tipsAlert .modal-body").text(msg);
    $("#tipsAlert").modal();
    if (callback) {
        $("#tipsAlert button").off().on('click', callback);
    }
}
