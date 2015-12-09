import "../less/tips-alert.less"

export default (msg, callback = null) => {
	$("#tipsAlert .modal-body").text(msg);
	$("#tipsAlert").modal();
	if (callback) {
		$("#tipsAlert button").off().one('click', callback);
	}

	var offsetHeight = ($(window).height() - $("#tipsAlert .modal-content").height()) / 2;
	$("#tipsAlert .modal-dialog").css("margin-top", offsetHeight + "px");
}
