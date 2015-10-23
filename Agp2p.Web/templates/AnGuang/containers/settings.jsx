import React from "react";

export default  class Settings extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render() {
        return(
            <div className="settings-wrap">
                <p>为了您的资金安全，建议您不要取消重要的消息项。如有问题，请联系客服：400-8989-089</p>
                <div className="news-tb">
                    <table className="table table-bordered">
                        <thead>
                            <tr>
                                <th>通知类型</th>
                                <th>电子邮件</th>
                                <th>短信</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>充值成功</td>
                                <td><input type="checkbox"/></td>
                                <td><input type="checkbox"/></td>
                            </tr>
                            <tr>
                                <td>提现成功</td>
                                <td><input type="checkbox"/></td>
                                <td><input type="checkbox"/></td>
                            </tr>
                            <tr>
                                <td>提现失败</td>
                                <td></td>
                                <td><input type="checkbox"/></td>
                            </tr>
                            <tr>
                                <td>借款标回款</td>
                                <td><input type="checkbox"/></td>
                                <td><input type="checkbox"/></td>
                            </tr>
                            <tr>
                                <td>提前回款</td>
                                <td><input type="checkbox"/></td>
                                <td><input type="checkbox"/></td>
                            </tr>
                            <tr>
                                <td>逾期垫付回款</td>
                                <td><input type="checkbox"/></td>
                                <td><input type="checkbox"/></td>
                            </tr>
                            <tr>
                                <td>电子账单</td>
                                <td><input type="checkbox"/></td>
                                <td></td>
                            </tr>
                            <tr>
                                <td>红包过期提醒</td>
                                <td><input type="checkbox"/></td>
                                <td><input type="checkbox"/></td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div className="btn-wrap"><a href="javascript:">保 存</a></div>
            </div>
        );
    }
}