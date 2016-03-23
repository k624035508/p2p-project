import React from 'react';
import ReactDom from 'react-dom';
import DropdownPicker from "./dropdown-picker.jsx";
import Pagination from "./pagination.jsx";
import findIndex from "lodash/array/findIndex";
import all from "lodash/collection/all";
import "../../templates/AnGuang/less/mynews.less";
import "bootstrap-webpack!./bootstrap.config.js";

class ManagerMessage extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			type: 0,
			pageIndex: 0,
			pageCount: 0,
			readingMsgIndex: -1,
			msgs: []
		};
	}
	componentDidMount() {
		this.fetchMessages(this.state.type, this.state.pageIndex);
		window.refreshDlg = () => this.fetchMessages(this.state.type, this.state.pageIndex);
	}
	fetchMessages(type, pageIndex) {
		this.setState({type, pageIndex});
		let url = "/admin/index.aspx/AjaxQueryManagerMessages", pageSize = 5;
		$.ajax({
			type: "get",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: {type, pageIndex, pageSize, idOnly: false},
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
		let url = "/admin/index.aspx/AjaxSetManagerMessagesRead";
		$.ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: JSON.stringify({msgIds: msgIds.join(";")}),
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
		let url = "/admin/index.aspx/AjaxDeleteManagerMessages";
		$.ajax({
			type: "post",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: JSON.stringify({msgIds: msgIds.join(";")}),
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
	deleteAllMessages() {
		let url = "/admin/index.aspx/AjaxQueryManagerMessages";
		$.ajax({
			type: "get",
			dataType: "json",
			contentType: "application/json",
			url: url,
			data: {type: this.state.type, pageIndex: 0, pageSize: 9999, idOnly: true},
			success: function(result) {
				let {totalCount, msgs} = JSON.parse(result.d);
				this.deleteMessages(msgs.map(m => m.id));
			}.bind(this),
			error: function(xhr, status, err) {
				console.error(url, status, err.toString());
			}.bind(this)
		});
	}
	render() {
		return (
			<div className="news-wrap">
                <div className="controls controls-custom">
                    <div className="controls-list pull-right">
                        <span onClick={ev => this.setAllCheckedAlreadyRead()}>标为已读</span>
                        <span onClick={ev => {
                        	this.setAllMsgChecked(true);
                        	this.setAllCheckedAlreadyRead();
                        }}>全部标为已读</span>
                        <span onClick={ev => this.deleteSelectedMessages()}>删除</span>
                        <span onClick={ev => this.deleteAllMessages()}>全部删除</span>
                    </div>
                    <DropdownPicker
                        onTypeChange={newType => this.fetchMessages(newType, this.state.pageIndex) }
                        enumFullName="Agp2p.Common.Agp2pEnums+ManagerMessageSourceEnum" />
                </div>
				<div className="news-list">
					<div className="news-tb">
						<div className="news-th">
                        <span className="checkbox"><input type="checkbox"
														  checked={this.state.msgs.length == 0 ? false : all(this.state.msgs, m => m.checked)}
														  onChange={ev => this.setAllMsgChecked(ev.target.checked)}/></span>
							<span className="state">状态</span>
							<span className="th">标题</span>
							<span className="content">内容</span>
							<span className="time">时间</span>
							<span className="detail">详细</span>
						</div>
                        {this.state.msgs.length != 0 ? null : <div style={{textAlign: 'center', padding: '1em'}}>暂无消息</div>}
						{this.state.msgs.map((m, index) =>
							<div className="news-body" key={m.id}>
								<div className="overview"
									style={index == this.state.readingMsgIndex ? {backgroundColor: "#f7f7f7"} : null}
									onClick={ev => this.toggleReading(index)}>
		                            <span className="checkbox">
		                            	<input type="checkbox" checked={this.state.msgs[index].checked}
															  onChange={ev => this.toggleMsgChecked(index)}/>
								    </span>
									<span className={`state ${m.isRead ? "read-icon" : "unread-icon"}`}></span>
									<span className="th">{m.title}</span>
									<span className="content">{m.body.length < 20 ? m.body: m.body.substr(0, 20) + "..."}</span>
									<span className="time">{m.creationTime}</span>
									<span className="detail close-icon"></span>
								</div>
								{this.state.readingMsgIndex != index ? null :
								<div className="news-detail">
									<p className="txt">{m.body}</p>
								</div>}
							</div>)}
					</div>
				</div>
                <Pagination pageIndex={this.state.pageIndex} pageCount={this.state.pageCount}
                    onPageSelected={pageIndex => {
                    	if (this.state.pageIndex == pageIndex) return;
                    	this.fetchMessages(this.state.type, pageIndex);
                    	this.setState({readingMsgIndex: -1})
                    }}/>
            </div>
		);
	}
}

$(() => {
	ReactDom.render(<ManagerMessage />, $(".message-list-wrapper")[0]);
})
