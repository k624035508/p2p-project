import React from "react"
import { ajax } from "jquery"
import DropdownPicker from "../components/dropdown-picker.jsx"
import Pagination from "../components/pagination.jsx"
import findIndex from "lodash/array/findIndex"
import all from "lodash/collection/all"
import "../less/mynews.less"

class MyNews extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			type: 1,
			pageIndex: 0,
			pageCount: 0,
			readingMsgIndex: -1,
			onPageLoaded: pageCount => this.setState({pageCount: pageCount}),
			msgs: []
		};
	}
	componentDidMount() {
		this.fetchMessages(this.state.type, this.state.pageIndex);
	}
	fetchMessages(type, pageIndex) {
		this.setState({type, pageIndex});
		let url = USER_CENTER_ASPX_PATH + "/AjaxQueryUserMessages", pageSize = 5;
		ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: JSON.stringify({type, pageIndex, pageSize}),
			success: function(result) {
				let {totalCount, msgs} = JSON.parse(result.d);
				if (msgs.length == 0 && 0 < pageIndex) { // deleted all messages in final page
					this.fetchMessages(type, pageIndex - 1);
				} else {
					this.setState({pageCount: Math.ceil(totalCount / pageSize), msgs});
				}
			}.bind(this),
			error: function(xhr, status, err) {
				console.error(url, status, err.toString());
			}.bind(this)
		});
	}
	setMsgsAlreadyRead(msgIds) {
		let url = USER_CENTER_ASPX_PATH + "/AjaxSetMessagesRead";
		ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: JSON.stringify({messageIds: msgIds.join(";")}),
			success: function(result) {
				var msgs = this.state.msgs;
				msgIds.map(id => findIndex(msgs, m => m.id == id))
				.forEach(index => msgs[index].isRead = true);
				this.setState({msgs});
			}.bind(this),
			error: function(xhr, status, err) {
				console.error(url, status, err.toString());
			}.bind(this)
		});
	}
	deleteMessages(msgIds) {
		let url = USER_CENTER_ASPX_PATH + "/AjaxDeleteMessages";
		ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: JSON.stringify({messageIds: msgIds.join(";")}),
			success: function(result) {
				this.fetchMessages(this.state.type, this.state.pageIndex);
			}.bind(this),
			error: function(xhr, status, err) {
				console.error(url, status, err.toString());
			}.bind(this)
		});
	}
	toggleReading(index) {
		this.setState({readingMsgIndex: this.state.readingMsgIndex == index ? -1 : index});
		if (index != -1 && !this.state.msgs[index].isRead) {
			this.setMsgsAlreadyRead([ this.state.msgs[index].id ]);
		}
	}
	toggleMsgChecked(index) {
		var msgs = this.state.msgs;
		msgs[index].checked = !msgs[index].checked;
		this.setState({msgs});
	}
	setAllMsgChecked(checked) {
		var msgs = this.state.msgs;
		msgs.forEach(m => m.checked = checked);
		this.setState({msgs});
	}
	setAllCheckedAlreadyRead() {
		var msgs = this.state.msgs;
		var willReadMsgs = msgs.filter(m => !m.isRead && m.checked);
		this.setAllMsgChecked(false);
		if (willReadMsgs.length == 0) {
			alert("请先选择未读消息");
			return;
		}
		willReadMsgs.forEach(m => m.isRead = true);
		this.setMsgsAlreadyRead(willReadMsgs.map(m => m.id));
	}
	deleteSelectedMessages() {
		var predelMsgIds = this.state.msgs.filter(m => m.checked).map(m => m.id);
		if (predelMsgIds.length == 0) {
			alert("请先选择消息");
			return;
		}
		if (confirm("确认删除已选中的消息？")) {
			this.deleteMessages(predelMsgIds);
		}
	}
    render() {
        return(
            <div className="news-wrap">
                <div className="controls controls-custom">
                    <DropdownPicker
                        onTypeChange={newType => this.fetchMessages(newType, this.state.pageIndex) }
                        enumFullName="Agp2p.Common.Agp2pEnums+UserMessageTypeEnum" />
                    <div className="controls-list pull-right">
                        <span onClick={ev => this.setAllCheckedAlreadyRead()}>标为已读</span>
                        <span onClick={ev => {
                        	this.setAllMsgChecked(true);
                        	this.setAllCheckedAlreadyRead();
                        }}>全部标为已读</span>
                        <span onClick={ev => this.deleteSelectedMessages()}>删除</span>
                    </div>
                </div>
				<div className="news-list">
					<div className="news-tb">
						<div className="news-th">
                        <span className="checkbox"><input type="checkbox"
														  checked={this.state.msgs.length == 0 ? false : all(this.state.msgs, m => m.checked)}
														  onChange={ev => this.setAllMsgChecked(ev.target.value)}/></span>
							<span className="state">状态</span>
							<span className="th">标题</span>
							<span className="content">内容</span>
							<span className="time">时间</span>
							<span className="detail">详细</span>
						</div>
						{this.state.msgs.map((m, index) =>
							<div className="news-body" key={m.id}>
								<div className="overview" style={index == this.state.readingMsgIndex ? {backgroundColor: "#f7f7f7"} : null}>
                            <span className="checkbox"><input type="checkbox" checked={this.state.msgs[index].checked}
															  onChange={ev => this.toggleMsgChecked(index)}/></span>
									<span className={`state ${m.isRead ? "read-icon" : "unread-icon"}`}></span>
									<span className="th">{m.title}</span>
									<span className="content">{m.content.length < 20 ? m.content: m.content.substr(0, 20) + "..."}</span>
									<span className="time">{m.receiveTime}</span>
									<span className="detail close-icon" onClick={ev => this.toggleReading(index)}></span>
								</div>
								<div className={`news-detail ${this.state.readingMsgIndex == index ? "" : "hidden"}`}>
									<p className="appellation">亲爱的会员 {this.props.userName}：</p>
									<p className="txt">您好！</p>
									<p className="txt">{m.content}</p>
									<p className="sender">安广融合团队</p>
								</div>
							</div>)}
					</div>
				</div>
                <Pagination pageIndex={this.state.pageIndex} pageCount={this.state.pageCount}
                    onPageSelected={pageIndex => this.fetchMessages(this.state.type, pageIndex)}/>
            </div>
        );
    }
}


function mapStateToProps(state) {
    return {
    	userName: state.userInfo.userName,
    };
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(MyNews);

