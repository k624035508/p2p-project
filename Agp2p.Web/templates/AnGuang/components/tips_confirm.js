import "../less/tips-alert.less";

export default (msg, callback = null) => {
    $("#tipsConfirm .modal-body").text(msg);
    $("#tipsConfirm").modal();
    if (callback) {
        $("#tipsConfirm button.confirm-btn").off().on('click', callback);
    }
}
