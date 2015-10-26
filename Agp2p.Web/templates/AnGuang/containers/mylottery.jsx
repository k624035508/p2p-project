import React from "react"

export default class MyLottery extends React.Component {
    constructor(props) {
        super(props);
        this.state = { selectedTabIndex: 0 };
    }

    render() {
        return(
            <div className="lotteries-wrap">
                <div className="lottery-th">
                    {["未使用","已失效", "历史记录"].map((s, index) =>
                        <span key={index}><a href="javascript:" className={this.state.selectedTabIndex == index ? "active" : null}
                            onClick={ ev => this.setState({ selectedTabIndex: index }) }>{s}</a></span>)}
                </div>
                <div className="lottery-content">
                    <div className="lottery-list hidden">
                        <div className="lottery-usable">
                            <div className="lottery-face">
                                <p className="lottery-value">￥30</p>
                                <p className="use-condition">投资满2000元可用</p>
                                <p className="use-date">请于2015-10-30前使用</p>
                            </div>
                            <div className="lottery-state">待使用</div>
                        </div>
                        <div className="lottery-fail">
                            <div className="lottery-face">
                                <p className="lottery-value">￥30</p>
                                <p className="use-condition">投资满2000元可用</p>
                                <p className="use-date">请于2015-10-30前使用</p>
                            </div>
                            <div className="lottery-state">已失效</div>
                        </div>
                    </div>
                    <div className="lottery-history">
                        <table className="table">
                            <thead>
                                <tr>
                                    <th>红包金额</th>
                                    <th>红包来源</th>
                                    <th>使用规则</th>
                                    <th>红包状态</th>
                                    <th>有效期限</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>￥30</td>
                                    <td>新手红包</td>
                                    <td>投资金额满2000使用</td>
                                    <td>待使用</td>
                                    <td>2015-12-25</td>
                                </tr>
                                <tr>
                                    <td>￥20</td>
                                    <td>投资赠送</td>
                                    <td>投资金额满1500使用</td>
                                    <td>待使用</td>
                                    <td>2015-12-30</td>
                                </tr>
                                <tr>
                                    <td>￥30</td>
                                    <td>新手红包</td>
                                    <td>投资金额满2000使用</td>
                                    <td>待使用</td>
                                    <td>2015-12-25</td>
                                </tr>
                                <tr>
                                    <td>￥20</td>
                                    <td>投资赠送</td>
                                    <td>投资金额满1500使用</td>
                                    <td>待使用</td>
                                    <td>2015-12-30</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div className="use-rules-th"><span>使用规则</span></div>
                <div className="use-rules">
                    <p>1.投资额需要达到一定数额才可使用红包，具体数值见红包说明文字。</p>
                    <p>2.单笔投资可同时使用多张红包，使用时所需投资额为各个红包要求的起投金额相累加。</p>
                    <p>3.安广融合有权根据运营情况调整红包使用规则，规则最终解释权归安广融合所有。</p>
                </div>
            </div>
        );
    }
}