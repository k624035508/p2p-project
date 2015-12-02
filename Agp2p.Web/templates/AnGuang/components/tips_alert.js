import "../less/tips-alert.less"

export default (msg, callback = null) => {
    $("#tipsAlert .modal-body").text(msg);
    $("#tipsAlert").modal();
    if (callback) {
        $("#tipsAlert button").off().on('click', callback);
    }
}
