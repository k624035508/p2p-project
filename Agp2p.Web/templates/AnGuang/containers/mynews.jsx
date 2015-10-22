import React from "react"
import DropdownPicker from "../components/dropdown-picker.jsx"

export default class MyNews extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render() {
        return(
            <div className="news-wrap">
                <div className="controls controls-custom">
                    <DropdownPicker
                        onTypeChange={newType => this.setState({type: newType}) }
                        enumFullName="Agp2p.Common.Agp2pEnums+TransactionDetailsDropDownListEnum" />
                    <div className="controls-list pull-right">
                        <span>标为已读</span>
                        <span>全部标为已读</span>
                        <span>删除</span>
                    </div>
                </div>
                <div className="news-cell">
                    <div className="news-th">
                        <span className="checkbox"><input type="checkbox"/></span>
                        <span className="state">状态</span>
                        <span className="th">标题</span>
                        <span className="content">内容</span>
                        <span className="time">时间</span>
                        <span className="detail">详细</span>
                    </div>
                    <div className="news-body">
                        <div className="overview">{ /*消息打开状态时background：#f7f7f7*/}
                            <span className="checkbox"><input type="checkbox"/></span>
                            <span className="state read-icon"></span>
                            <span className="th">注册成功</span>
                            <span className="content">亲爱的会员XXX：您好！感谢您注册安广</span>
                            <span className="time">2015/10/22 14:25</span>
                            <span className="detail close-icon"></span>
                        </div>
                        <div className="news-detail hidden">
                            <p className="appellation">亲爱的会员XXX：</p>
                            <p className="txt">您好！感谢您注册安广融合账号！</p>
                            <p className="txt">您在安广融合可以将自己的闲散资金出借给有需要的人以此获得资金回报。</p>
                            <p className="sender">安广融合团队</p>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}