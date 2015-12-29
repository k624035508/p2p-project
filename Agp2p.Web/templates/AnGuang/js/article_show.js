import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/article_show.less";
import "../less/footerSmall.less";
import SharingButtons from "../components/share.jsx";
import header from "./header.js"
import ReactDom from "react-dom";
import React from "react";

function getLocationOrigin() {
    if (!window.location.origin) {
        return window.location.protocol + "//"
            + window.location.hostname
            + (window.location.port ? ':' + window.location.port: '');
    } else {
        return location.origin;
    }
}

$(function(){
    header.setHeaderHighlight(4);

    //弹出窗popover初始化
    $('[data-toggle="popover"]').popover();

    ReactDom.render(<SharingButtons
        preText="分享到："
        sharingUrl={location.href}
        encodedTitle={encodeURI(document.title)}
        encodedDescription=""
        locationOrigin={getLocationOrigin()}
        picUrl="/templates/AnGuang/imgs/index/logo.png"/>, document.getElementById("shareBtn"));
});