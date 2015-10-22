import React from "react"

export default class Recommend extends React.Component {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(){
        return(
            <div className="recommend-wrap">
                <div className="recommend-link">
                    <span>我的邀请链接：</span>
                    <span className="site-link">http://www.agrhp2p.com/</span>
                    <a href="#">复 制</a>
                </div>
                <div className="shareTo">
                    <span>或分享到：</span>
                    <i className="weChat"></i>
                    <i className="sinaWeibo"></i>
                    <i className="qq"></i>
                    <i className="qqZone"></i>
                    <i className="tencentWeibo"></i>
                    <i className="more"></i>
                </div>
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
                            <tr>
                                <td>1</td>
                                <td>135****7850</td>
                                <td>10000.00</td>
                                <td>15.00</td>
                            </tr>
                            <tr>
                                <td>2</td>
                                <td>138****7315</td>
                                <td>10000.00</td>
                                <td>15.00</td>
                            </tr>
                            <tr>
                                <td>3</td>
                                <td>138****7315</td>
                                <td>10000.00</td>
                                <td>15.00</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div className="warm-tips-th"><span>温馨提示</span></div>
                <div className="warmTips">
                    <p>1. 安广融合将采用大数据技术杜绝违规操作及作弊行为(包括但不限于：恶意注册虚假账号、恶意刷奖等)，若判<span style={{display:"inline-block", width:"15px"}}></span>定为违规操作及作弊行为，安广融合有权取消其获奖资格。</p>
                    <p>2. 好友需通过您的专属链接注册才能建立推荐关系；奖励针对活动期间的推荐，累计奖励无上限。</p>
                    <p>3. 活动解释权归安广融合所有。</p>
                </div>
            </div>
        );
    }
}