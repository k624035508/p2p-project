import React from "react";
import ReactDom from "react-dom";

import "bootstrap-webpack!./bootstrap.config.js";
import "../less/head.less";
import "../less/loan.less";
import "../less/footerSmall.less";
import alert from "../components/tips_alert.js";

import header from "./header.js";

window['$'] = $;

const LoanApplyStep = {
    Unlogin : 0, // 未登录
    UnApplyAsLoaner : 1, // 未申请借款人
    ApplyAsLoanerAuditting : 2, // 申请借款人审核中
    UnApplyProject : 3, // 未申请借款项目
    ProjectAuditting : 4, // 申请借款项目审核中
    ProjectApplyCompleted : 5, // 完成借款申请
};

const LoanerStatusEnum = {
    IsNotALoaner : -1, // 不是借款人
    Normal : 1, // 正常
    Pending : 10, // 待审核
    PendingFail : 11, // 审核不通过
    Disable : 20 // 禁用
};

// 这个枚举跟服务器上面的不同
const ProjectStatusEnum = {
    FinancingApplicationNotExist : -1, // 未申请
    FinancingApplicationUncommitted : 1, // 提交了申请
    FinancingApplicationChecking : 2, // 审核中
    FinancingApplicationCancel : 0, // 审核失败
};

class ApplyingStatusPanel extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }
    render () {
        var step1HighLightStyle = this.props.step >= 1
            ? {background: 'url("/templates/AnGuang/imgs/loan/personal-info.png") no-repeat'} : null;
        var step2HighLightStyle = this.props.step >= 2
            ? {background: 'url("/templates/AnGuang/imgs/loan/loan.png") no-repeat'} : null;
        var step3HighLightStyle = this.props.step >= 5
            ? {background: 'url("/templates/AnGuang/imgs/loan/loan-finish.png") no-repeat'} : null;

        return (
            <div className="nav-wrap">
                <p className="title">借款流程</p>
                <ul className="list-unstyled list-inline application-ul">
                    <li className="step1">登录</li>
                    <li className="next-icon"></li>
                    <li className="step2" style={step1HighLightStyle} >申请成为借款人</li>
                    <li className="next-icon"></li>
                    <li className="step3" style={step2HighLightStyle} >申请借款</li>
                    <li className="next-icon"></li>
                    <li className="step4" style={step3HighLightStyle} >完成借款</li>
                </ul>
            </div>
        );
    }
}

class LoginPanel extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }
    doLogin() {
        $.ajax({
            type: "post",
            url: "/tools/submit_ajax.ashx?action=user_login",
            dataType: "json",
            data: {
                txtUserName: this.refs.userName.value,
                txtPassword: this.refs.passwd.value,
                chkRemember: true
            },
            success: data => {
                if(data.status == 1){
                    location.reload();
                } else {
                    alert(data.msg);
                }
            },
            error: function(xhr, status, err){
                alert("操作超时，请重试。");
            }
        });
    }
    render () {
        return (
            <div className="login-form-wrap">
                <div className="form-title">
                    <span className="title-style">登录</span>
                    <span className="pull-right register-btn">如无账号 <a href='/register.html'>快速注册</a></span>
                </div>
                <form className="form-horizontal login-form">
                    <div className="form-group">
                        <label htmlFor="user-name" className="control-label">用户名：</label>
                        <input type="text" className="form-control" ref="userName" id="user-name" />
                    </div>
                    <div className="form-group">
                        <label htmlFor="user-pwd" className="control-label">密码：</label>
                        <input type="text" style={{display: 'none'}} />
                        <input type="password" className="form-control" ref="passwd" id="user-pwd" autoComplete="off" />
                    </div>
                    <div className="form-group">
                        <button type="button" className="btn btn-default" id="loginBtn" onClick={ev => this.doLogin()}>登 录</button>
                    </div>
                </form>
            </div>
        );
    }
}

class LoanerApplyingPanel extends React.Component {
    constructor(props) {
        super(props);
        this.state = props.value;
    }
    componentDidMount() {
        if(!this.state.loanerName){
            alert("请先前往会员中心进行实名认证", function(){
                location.href = '/user/center/index.html#/safe';
            });
        }
    }
    
    doLoanerApplySubmit() {
        $.ajax({
            type: "post",
            url: "/aspx/main/loan.aspx/ApplyLoaner",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({
                age:0,
                nativePlace: this.state.loanerNativePlace,
                job: this.state.loanerJob,
                workingCompany: this.state.loanerWorkingCompany,
                educationalBackground: this.state.loanerEducationalBackground,
                maritalStatus: this.state.loanerMaritalStatus,
                income: this.state.loanerIncome
            }),
            success: data => {
                if (data.d === "ok") location.reload();
            },
            error: function(xhr, status, err){                
                alert(xhr.responseJSON.Message);
            }
        });
    }
    render () {
        var { loanerName,loanerMobile,loanerAge,loanerEducationalBackground,loanerIncome, loanerStatus,
            loanerJob,loanerMaritalStatus,loanerNativePlace,loanerWorkingAt,loanerWorkingCompany } = this.state;
        var isEditable = loanerStatus == LoanerStatusEnum.IsNotALoaner || loanerStatus == LoanerStatusEnum.PendingFail;
        return (
            <div className="personal-info-form-wrap">
                <div className="form-title">
                    <span className="title-style">借款人资料</span>
                </div>
                {loanerStatus == LoanerStatusEnum.Pending ?
                <div id="loanerApplyChecking" className="status">
                    <span className="status-icon checking"></span>
                    <span className="tips-title">申请借款人审核中</span>
                    <span className="tips">您提交的申请正在审核中，请耐心等待！</span>
                </div> : null }
                {loanerStatus == LoanerStatusEnum.PendingFail ?
                <div id="loanerApplyFailed" className="status">
                    <span className="status-icon failed"></span>
                    <span className="tips-title">申请借款人失败</span>
                    <span className="tips">您提交的申请资料有误，请重新核对资料后再提交！</span>
                </div>:null}
                {loanerStatus == LoanerStatusEnum.Disable ?
                <div id="loanerApplyForbid" className="status">
                    <span className="status-icon forbid"></span>
                    <span className="tips-title">禁止申请借款人</span>
                    <span className="tips">抱歉！您已被禁止申请成为借款人。</span>
                </div>:null}
                <form className="form-horizontal personal-info-form">
                    <div className="form-group">
                        <label htmlFor="name" className="control-label">姓名：</label>
                        <input type="text" id="name" value={loanerName} className="form-control" readOnly />
                    </div>
                    <div className="form-group">
                        <label htmlFor="phone" className="control-label">手机号码：</label>
                        <input type="text" id="phone" value={loanerMobile} className="form-control" readOnly />
                    </div>
                    <div className="form-group">
                        <label htmlFor="birthplace" className="control-label">籍贯：</label>
                        <input type="text" id="birthplace" className="form-control" readOnly={!isEditable}
                            value={loanerNativePlace} onChange={ev => this.setState({loanerNativePlace: ev.target.value})} />
                    </div>
                    <div className="form-group">
                        <label htmlFor="job" className="control-label">职业：</label>
                        <input type="text" id="job" className="form-control" value={loanerJob} readOnly={!isEditable}
                            onChange={ev => this.setState({loanerJob: ev.target.value})} />
                    </div>
                    <div className="form-group">
                        <label htmlFor="employer" className="control-label">工作单位：</label>
                        <input type="text" id="employer" className="form-control" value={loanerWorkingCompany} readOnly={!isEditable}
                            onChange={ev => this.setState({loanerWorkingCompany: ev.target.value})} />
                    </div>
                    <div className="form-group">
                        <label htmlFor="education" className="control-label">学历：</label>
                        <select id="education" className="form-control" value={loanerEducationalBackground} disabled={!isEditable}
                            onChange={ev => this.setState({loanerEducationalBackground: ev.target.value})} >
                            <option value="" style={{display: 'none'}}></option>
                            <option value="小学及以下">小学及以下</option>
                            <option value="初中">初中</option>
                            <option value="高中">高中</option>
                            <option value="中专">中专</option>
                            <option value="大专">大专</option>
                            <option value="本科">本科</option>
                            <option value="研究生">研究生</option>
                            <option value="博士及以上">博士及以上</option>
                        </select>
                    </div>
                    <div className="form-group">
                        <label htmlFor="marital-status" className="control-label">婚姻状况：</label>
                        <select id="marital-status" className="form-control" value={loanerMaritalStatus} disabled={!isEditable}
                            onChange={ev => this.setState({loanerMaritalStatus: ev.target.value})} >
                            <option style={{display: 'none'}}></option>
                            <option value="1">未婚</option>
                            <option value="2">已婚</option>
                            <option value="3">离异</option>
                            <option value="4">丧偶</option>
                        </select>
                    </div>
                    <div className="form-group">
                        <label htmlFor="income" className="control-label">收入：</label>
                        <input type="text" id="income" className="form-control" value={loanerIncome} readOnly={!isEditable}
                            onChange={ev => this.setState({loanerIncome: ev.target.value})} />
                    </div>
                    <div className="form-group">
                        <button type="button" className="btn btn-default" id="loanerApplyBtn" style={isEditable?null:{display:'none'}} onClick={ev => this.doLoanerApplySubmit()}>提 交</button>
                    </div>
                </form>
            </div>
        );
    }
}

class ProjectApplyingPanel extends React.Component {
    constructor(props) {
        super(props);
        this.state = props.value;
        this.state.quotaUse = parseFloat(props.quotaUse);
    }
    doProjectApply() {
        $.ajax({
            type: "post",
            url: "/aspx/main/loan.aspx/ApplyLoan",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({
                loanerContent: this.state.projectLoanerContent,
                loanUsage: this.state.projectLoanUsage,
                sourceOfRepayment: this.state.projectSourceOfRepayment,
                amount: this.state.projectAmount,
            }),
            success: function(data){
                if (data.d === "ok") location.reload();
            },
            error: function(xhr, status, err){                
                alert(xhr.responseJSON.Message);
            }
        });
    }
    doQuotaCheck() {
        if (parseFloat(this.state.projectAmount) > this.state.quotaUse) {
            alert("借款额度不能大于可用额度！");
        }
    }
    render() {
        var { projectCategoryId,projectAmount,projectLoanUsage,projectSourceOfRepayment,projectLoanerContent,quotaUse, projectStatus } = this.state;
        var isEditable = projectStatus == ProjectStatusEnum.FinancingApplicationNotExist || projectStatus == ProjectStatusEnum.FinancingApplicationCancel;
        return (
            <div className="loan-detail-form-wrap">
                <div className="form-title">
                    <span className="title-style">申请借款</span>
                </div>
                {projectStatus != ProjectStatusEnum.FinancingApplicationChecking && projectStatus != ProjectStatusEnum.FinancingApplicationUncommitted
                ? null
                : <div id="loanApplyChecking" className="status">
                    <span className="status-icon checking"></span>
                    <span className="tips-title">申请借款审核中</span>
                    <span className="tips">您提交的申请正在审核中，请耐心等待！</span>
                </div>}
                {projectStatus != ProjectStatusEnum.FinancingApplicationCancel
                ? null
                : <div id="loanApplyFailed" className="status">
                    <span className="status-icon failed"></span>
                    <span className="tips-title">申请借款失败</span>
                    <span className="tips">您提交的申请资料有误，请重新核对资料后再提交！</span>
                </div>}
                <form className="form-horizontal loan-detail-form">
                    <div className="form-group">
                        <label htmlFor="loan-amount" className="control-label">借款金额：</label>
                        <input type="text" id="loan-amount" className="form-control" value={projectAmount}
                            onChange={ev => this.setState({projectAmount: ev.target.value})}
                            readOnly={!isEditable}
                            onBlur={ev => this.doQuotaCheck()}/>
                        <span id="largest-amount">{`（可用额度 ${quotaUse}）`}</span>
                    </div>
                    <div className="form-group">
                        <label htmlFor="loan-description" className="control-label">借款描述：</label>
                        <textarea id="loan-description" className="form-control" value={projectLoanerContent}
                            readOnly={!isEditable}
                            onChange={ev => this.setState({projectLoanerContent: ev.target.value})}></textarea>
                    </div>
                    <div className="form-group">
                        <label htmlFor="loan-usage" className="control-label">借款用途：</label>
                        <textarea id="loan-usage" className="form-control" value={projectLoanUsage}
                            readOnly={!isEditable}
                            onChange={ev => this.setState({projectLoanUsage: ev.target.value})}></textarea>
                    </div>
                    <div className="form-group">
                        <label htmlFor="repayment-source" className="control-label">还款来源：</label>
                        <textarea id="repayment-source" className="form-control" value={projectSourceOfRepayment}
                            readOnly={!isEditable}
                            onChange={ev => this.setState({projectSourceOfRepayment: ev.target.value})}></textarea>
                    </div>
                    <div className="form-group">
                        <button type="button" className="btn btn-default" id="loanApplyBtn"
                            style={isEditable?null:{display: 'none'}}
                            onClick={ev => this.doProjectApply()}>提 交</button>
                    </div>
                </form>
            </div>
        );
    }
}

class CompletePanel extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }
    render () {
        return (
            <div className="Completion-loan-form-wrap">
                <div className="form-title"><span className="title-style">完成借款</span></div>
                <form className="form-horizontal Completion-loan-form">
                    <div className="form-group">
                        <label className="control-label"><img src={require('../imgs/loan/001.png')} /></label><br /><br />
                        <label className="control-label">恭喜您！您申请的借款已审核通过！</label><br /><br /><br />
                        <label className="control-label"><input id="loanApplyAgainBtn" type="button" value="申请借款" onClick={ev => this.props.onApplyLoanClick()} /></label>
                    </div>
                </form>
            </div>
        );
    }
}

class LoanApplying extends React.Component {
    constructor(props) {
        super(props);
        var { step,userId,userName,loanerId,pendingProjectId,quotaUse,status } = $("#main").data();
        this.state = {step: parseInt(step)};
    }
    render () {
        var step = this.state.step;
        return (
            <div className="content-wrap application-form">
                <ApplyingStatusPanel step={this.state.step} />
                <div className="form-wrapper">
                {step == 0 ? <LoginPanel /> : null}
                {step == 1 || step == 2 ? <LoanerApplyingPanel value={$("#loaner").data()} /> : null}
                {step == 3 || step == 4 ? <ProjectApplyingPanel value={$("#project").data()} quotaUse={$("#main").data().quotaUse} /> : null}
                {step == 5 ? <CompletePanel onApplyLoanClick={() => this.setState({step: 3})} /> : null}
                </div>
            </div>
        );
    }
}

$(function () {
    header.setHeaderHighlight(2);

    //data-toggle 初始化
    $('[data-toggle="popover"]').popover();

    ReactDom.render(<LoanApplying />, document.querySelector('.react-root'));
});

