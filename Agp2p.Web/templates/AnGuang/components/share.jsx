import "../less/share.less";
import qr from "qr-image";
import React from "react";

export default class SharingButtons extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }
    genQrCodeBase64(str) {
        let res = qr.imageSync(str, {size: 3, margin: 0});
        return res.toString('base64');
    }
    render() {
        return (
            <div className="shareTo">
                <span>{this.props.preText}</span>
                <a className="weChat" href="javascript:">
                    <div className="qr-wrapper">
                        <img src={`data:image/png;base64,${this.genQrCodeBase64(this.props.sharingUrl)}`} />
                        <p>微信扫一扫：分享</p>
                    </div>
                </a>
                <a className="sinaWeibo"
                   target="_blank"
                   href={`http://service.weibo.com/share/share.php?url=${this.props.sharingUrl}&title=${this.props.encodedTitle + this.props.encodedDescription}&pic=${this.props.locationOrigin + this.props.picUrl}`}/>
                <a className="qq"
                   target="_blank"
                   href={`http://connect.qq.com/widget/shareqq/index.html?url=${this.props.sharingUrl}&title=${this.props.encodedTitle}&source=${this.props.locationOrigin}&desc=${this.props.encodedDescription}`}/>
                <a className="qqZone"
                   target="_blank"
                   href={`http://sns.qzone.qq.com/cgi-bin/qzshare/cgi_qzshare_onekey?url=${this.props.sharingUrl}&title=${this.props.encodedTitle}&desc=${this.props.encodedDescription}&summary=${this.props.encodedDescription}&site=${this.props.locationOrigin + this.props.picUrl}`}/>
                <a className="tencentWeibo"
                   target="_blank"
                   href={`http://share.v.t.qq.com/index.php?c=share&a=index&title=${this.props.encodedTitle + this.props.encodedDescription}&url=${this.props.sharingUrl}&pic=${this.props.locationOrigin + this.props.picUrl}`} />
            </div>
        );
    }
}

