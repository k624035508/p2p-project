window.alert = msg => {
    $("#tipsAlert .modal-body").text(msg);
    $("#tipsAlert").modal();
}
