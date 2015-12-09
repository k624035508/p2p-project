import "../less/tips-alert.less";

export default (msg, callback = null) => {
    $("#tipsConfirm .modal-body").text(msg);
    $("#tipsConfirm").modal();
    if (callback) {
        $("#tipsConfirm button.confirm-btn").off().one('click', callback);
    }

    var offsetHeight = ($(window).height() - $("#tipsConfirm .modal-content").height()) / 2;
	$("#tipsConfirm .modal-dialog").css("margin-top", offsetHeight + "px");
}
