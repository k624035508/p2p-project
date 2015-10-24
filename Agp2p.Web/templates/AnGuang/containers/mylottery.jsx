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
                <div className="lottery-list">
                    <div className="lottery-usable">
                        <div className="lottery-face">
                            <p className="lottery-value">￥30</p>
                            <p className="use-condition">投资满2000元可用</p>
                            <p className="use-date">请于2015-10-30前使用</p>
                        </div>
                        <div className="lottery-state">待使用</div>
                    </div>
                    <div className="lottery-usable">
                        <div className="lottery-face">
                            <p className="lottery-value">￥30</p>
                            <p className="use-condition">投资满2000元可用</p>
                            <p className="use-date">请于2015-10-30前使用</p>
                        </div>
                        <div className="lottery-state">待使用</div>
                    </div>
                    <div className="lottery-usable">
                        <div className="lottery-face">
                            <p className="lottery-value">￥30</p>
                            <p className="use-condition">投资满2000元可用</p>
                            <p className="use-date">请于2015-10-30前使用</p>
                        </div>
                        <div className="lottery-state">待使用</div>
                    </div>
                    <div className="lottery-usable">
                        <div className="lottery-face">
                            <p className="lottery-value">￥30</p>
                            <p className="use-condition">投资满2000元可用</p>
                            <p className="use-date">请于2015-10-30前使用</p>
                        </div>
                        <div className="lottery-state">待使用</div>
                    </div>
                </div>
            </div>
        );
    }
}