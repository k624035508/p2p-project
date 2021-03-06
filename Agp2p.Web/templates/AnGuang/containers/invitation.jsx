import "../less/invitation.less";
import React from "react";
import Pagination from "../components/pagination.jsx";
import SharingButtons from "../components/share.jsx";
import alert from "../components/tips_alert.js";

class InvitationPage extends React.Component {
	constructor(props) {
		super(props);
		this.state = {clipboard: null, data: [], pageIndex: 0, pageCount: 0};
	}
	componentWillUnmount() {
		if (this.state.clipboard) {
			this.state.clipboard.destroy();
		}
	}
	componentDidMount() {
		var fallback = function () {
			$("#btn-copy").click(function () {
				var text = $("span.site-link").text();
				window.prompt("按 Ctrl+C 复制到剪贴板，然后关闭对话框", text);
			});
		};
		if (!document.addEventListener) { // ie8 或以下
			fallback();
		} else {
			var Clipboard = require("clipboard");
			var clipboard = new Clipboard("#btn-copy");
			clipboard.on('error', function(e) {
				fallback();
			}).on('success', function(e) {
				alert("复制成功");
				e.clearSelection();
			});
			this.setState({clipboard: clipboard});
		}
		this.fetchInvitationData();
    }
    getLocationOrigin() {
        if (!window.location.origin) {
            return window.location.protocol + "//"
            + window.location.hostname
            + (window.location.port ? ':' + window.location.port: '');
        } else {
            return location.origin;
        }
    }
    genEncodedTitle() {
        return encodeURI("安广融合注册链接分享：");
    }
    genEncodedDescription() {
        return encodeURI("邀请人：" + this.props.inviter);
    }
    genSharingLink() {
        return `${this.getLocationOrigin()}/register.html?inviteCode=${this.props.invitationCode}`;
    }
    genPicUrl() {
        return "/templates/AnGuang/imgs/index/logo.png";
    }
	fetchInvitationData(pageIndex = 0) {
		this.setState({pageIndex});

		let url = USER_CENTER_ASPX_PATH + "/AjaxQueryInvitationInfo", pageSize = 4;
        $.ajax({
            type: "post",
            dataType: "json",
            contentType: "application/json",
            url: url,
            data: JSON.stringify({ pageIndex, pageSize}),
            success: function(result) {
                let {totalCount, data} = JSON.parse(result.d);
                this.setState({data: data, pageCount: Math.ceil(totalCount / pageSize)});
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(url, status, err.toString());
            }.bind(this)
        });
	}

    render(){
        return(
            <div className="recommend-wrap">
                <div className="recommend-link">
                    <span>我的邀请链接：</span>
                    <span className="site-link">{this.genSharingLink()}</span>
                    <a id="btn-copy" href="javascript:" data-clipboard-target=".site-link">复 制</a>
                </div>
                <SharingButtons
                    preText="或分享到："
                    sharingUrl={this.genSharingLink()}
                    encodedTitle={this.genEncodedTitle()}
                    encodedDescription={this.genEncodedDescription()}
                    locationOrigin={this.getLocationOrigin()}
                    picUrl={this.genPicUrl()}
                />
                <div id="sharejs"></div>
                <div className="invited-th"><span>已邀请的好友</span></div>
                <div className="table-wrap">
                    <table className="table">
                        <thead>
                            <tr>
                                <th>编号</th>
                                <th>用户名</th>
                                <th>首次投标金额(元)</th>
                                <th>收益奖励(元)</th>
                            </tr>
                        </thead>
                        <tbody>
                            {this.state.data.length != 0 ? null : <tr><td colSpan="4">暂无数据</td></tr>}
                        	{this.state.data.map((inv, index) => 
                            <tr key={inv.inviteeId}>
                                <td>{index + 1}</td>
                                <td>{inv.inviteeName}</td>
                                <td>{inv.firstInvestmentAmount}</td>
                                <td>{inv.reward}</td>
                            </tr>)}
                        </tbody>
                    </table>
	              <div className="nav-parent">  <Pagination pageIndex={this.state.pageIndex} pageCount={this.state.pageCount}
	                    onPageSelected={pageIndex => this.fetchInvitationData(pageIndex)}/>  </div>
                </div>
                <div className="warm-tips-th"><span>温馨提示</span></div>
                <div className="warmTips">
                    <p>1. 安广融合将采用大数据技术杜绝违规操作及作弊行为(包括但不限于：恶意注册虚假账号、恶意刷奖等)，若判<span style={{display:"inline-block", width:"17px"}}></span>定为违规操作及作弊行为，安广融合有权取消其获奖资格。</p>
                    <p>2. 好友需通过您的专属链接注册才能建立推荐关系；</p>
                    <p>3. 活动解释权归安广融合所有。</p>
                </div>
            </div>
        );
    }
}

function mapStateToProps(state) {
    return {
        invitationCode: state.userInfo.invitationCode,
        inviter: state.userInfo.userName
    };
}

import { connect } from 'react-redux';
export default connect(mapStateToProps)(InvitationPage);